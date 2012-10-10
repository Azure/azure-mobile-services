// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Compiles a LINQ expression tree into a
    /// MobileServiceTableQueryDescription that can be executed on the server.
    /// </summary>
    /// <remarks>
    /// This code is based on the concepts discussed in
    /// http://blogs.msdn.com/b/mattwar/archive/2008/11/18/linq-links.aspx
    /// which is pointed to by MSDN as the reference for building LINQ 
    /// providers.
    /// </remarks>
    internal class MobileServiceTableQueryTranslator : ExpressionVisitor
    {
        /// <summary>
        /// The compiled query description generated from the expression tree.
        /// </summary>
        private MobileServiceTableQueryDescription query = new MobileServiceTableQueryDescription();

        /// <summary>
        /// Initializes a new instance of the MobileServiceTableQueryTranslator
        /// class.
        /// </summary>
        private MobileServiceTableQueryTranslator()
        {
        }

        /// <summary>
        /// Translate an expression tree into a compiled query description that
        /// can be executed on the server.
        /// </summary>
        /// <param name="expression">The expression tree.</param>
        /// <returns>A compiled query description.</returns>
        public static MobileServiceTableQueryDescription Translate(Expression expression)
        {
            Debug.Assert(expression != null, "expression cannot be null!");

            // Evaluate any independent subexpressions so we end up with a tree
            // full of constants or things that depend directly on our values.
            expression = PartialEvaulator.PartiallyEvalulate(expression);

            // Build a new query from the expression tree
            MobileServiceTableQueryTranslator translator = new MobileServiceTableQueryTranslator();
            translator.Visit(expression);
            return translator.query;
        }

        /// <summary>
        /// Remove the quote from quoted expressions.
        /// </summary>
        /// <param name="expression">The expression to check.</param>
        /// <returns>An unquoted expression.</returns>
        private static Expression StripQuote(Expression expression)
        {
            Debug.Assert(expression != null, "expression cannot be null!");
            return expression.NodeType == ExpressionType.Quote ?
                ((UnaryExpression)expression).Operand :
                expression;
        }

        /// <summary>
        /// Process the core LINQ operators that are supported by Mobile
        /// Services.
        /// </summary>
        /// <param name="expression">The expression to visit.</param>
        /// <returns></returns>
        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            // Recurse down the target of the method call
            if (expression != null && expression.Arguments.Count >= 1)
            {
                Visit(expression.Arguments[0]);
            }

            // Handle the method call itself
            string name = null;
            if (expression != null &&
                expression.Method != null &&
                expression.Method.DeclaringType == typeof(Queryable))
            {
                name = expression.Method.Name;
            }
            switch (name)
            {
                case "Where":
                    this.AddFilter(expression);
                    break;
                case "Select":
                    this.AddProjection(expression);
                    break;
                case "OrderBy":
                case "ThenBy":
                    this.AddOrdering(expression, true);
                    break;
                case "OrderByDescending":
                case "ThenByDescending":
                    this.AddOrdering(expression, false);
                    break;
                case "Skip":
                    this.query.Skip = GetCountArgument(expression);
                    break;
                case "Take":
                    this.query.Top = GetCountArgument(expression);
                    break;
                default:
                    ThrowForUnsupportedException(expression);
                    break;                    
            }

            return expression;
        }

        /// <summary>
        /// Throw a NotSupportedException for an unsupported expression.
        /// </summary>
        /// <param name="expression">The unsupported expression.</param>
        private static void ThrowForUnsupportedException(MethodCallExpression expression)
        {
            // Try and get the body of the lambda for a more descriptive error
            // message (if possible)
            Expression deepest = expression;
            if (expression != null && expression.Arguments.Count >= 2)
            {
                LambdaExpression lambda = StripQuote(expression.Arguments[1]) as LambdaExpression;
                if (lambda != null)
                {
                    deepest = lambda.Body ?? lambda;
                }
            }

            throw new NotSupportedException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.MobileServiceTableQueryTranslator_ThrowForUnsupportedException_Unsupported,
                    expression != null && expression.Method != null ? expression.Method : null,
                    deepest != null ? deepest.ToString() : null));
        }

        /// <summary>
        /// Add a filter expression to the query.
        /// </summary>
        /// <param name="expression">Where method call expression.</param>
        private void AddFilter(MethodCallExpression expression)
        {
            if (expression != null && expression.Arguments.Count >= 2)
            {
                LambdaExpression lambda = StripQuote(expression.Arguments[1]) as LambdaExpression;
                if (lambda != null)
                {
                    string filter = FilterBuildingExpressionVisitor.Create(lambda.Body);
                    if (this.query.Filter != null)
                    {
                        // If there's already a filter value, that means the
                        // query has multiple where clauses which we'll just
                        // join together with "and"s.
                        this.query.Filter += " and " + filter;
                    }
                    else
                    {
                        this.query.Filter = filter;
                    }
                    return;
                }
            }

            ThrowForUnsupportedException(expression);
        }

        /// <summary>
        /// Add a projection to the query.
        /// </summary>
        /// <param name="expression">Select method call expression.</param>
        private void AddProjection(MethodCallExpression expression)
        {
            // We only support Select(x => ...) projections.  Anything else
            // will throw a NotSupportException.
            if (expression != null && expression.Arguments.Count == 2)
            {
                LambdaExpression projection = StripQuote(expression.Arguments[1]) as LambdaExpression;
                if (projection != null && projection.Parameters.Count == 1)
                {
                    // Store the type of the input to the projection as we'll
                    // need that for deserialization of values (since the
                    // projection will change the expected type of the data
                    // source)
                    this.query.ProjectionArgumentType = projection.Parameters[0].Type;

                    // Compile the projection into a function that we can apply
                    // to the deserialized value to transform it accordingly.
                    this.query.Projection = projection.Compile();

                    // Filter the selection down to just the values used by
                    // the projection
                    VisitorHelper.VisitMembers(
                        projection.Body,
                        (expr, recur) =>
                        {
                            // Ensure we only process members of the parameter
                            if (expr != null && expr.Expression.NodeType == ExpressionType.Parameter)
                            {
                                SerializableMember member = SerializableType.GetMember(expr.Member);
                                if (member != null)
                                {
                                    query.Selection.Add(member.Name);
                                }
                            }
                            return recur(expr);
                        });
                    
                    // Make sure we also include all the members that would be
                    // required for deserialization
                    foreach (SerializableMember member in
                        SerializableType.Get(this.query.ProjectionArgumentType)
                        .Members
                        .Select(p => p.Value)
                        .Where(m => m.IsRequired))
                    {
                        if (!this.query.Selection.Contains(member.Name))
                        {
                            this.query.Selection.Add(member.Name);
                        }
                    }

                    return;
                }
            }

            ThrowForUnsupportedException(expression);
        }

        /// <summary>
        /// Add an ordering constraint for an OrderBy/ThenBy call.
        /// </summary>
        /// <param name="expression">The ordering method call.</param>
        /// <param name="ascending">
        /// Whether the order is ascending or descending.
        /// </param>
        private void AddOrdering(MethodCallExpression expression, bool ascending)
        {
            // Keep updating with the deepest nested expression structure we
            // can get to so that we can provide a more detailed error message
            Expression deepest = expression;

            // We only allow OrderBy(x => x.Member) expressions.  Anything else
            // will result in a NotSupportedException.
            if (expression != null && expression.Arguments.Count >= 2)
            {
                LambdaExpression lambda = StripQuote(expression.Arguments[1]) as LambdaExpression;
                if (lambda != null)
                {
                    deepest = lambda.Body ?? lambda;

                    // Find the name of the member being ordered
                    MemberExpression memberAccess = lambda.Body as MemberExpression;
                    if (memberAccess != null) 
                    {
                        if (memberAccess.Expression.NodeType == ExpressionType.Parameter)
                        {
                            SerializableMember member = SerializableType.GetMember(memberAccess.Member);
                            if (member != null && member.Name != null)
                            {
                                // Add the ordering
                                this.query.Ordering.Add(new KeyValuePair<string, bool>(member.Name, ascending));
                                return;
                            }
                        }
                    }
                }
            }

            throw new NotSupportedException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.MobileServiceTableQueryTranslator_GetOrdering_Unsupported,
                    expression != null && expression.Method != null ? expression.Method.Name : null,
                    deepest != null ? deepest.ToString() : null));
        }

        /// <summary>
        /// Get the count argument value for a Skip or Take method call.
        /// </summary>
        /// <param name="expression">The method call expression.</param>
        /// <returns>The count argument.</returns>
        private static int GetCountArgument(MethodCallExpression expression)
        {
            // Keep updating with the deepest nested expression structure we
            // can get to so that we can provide a more detailed error message
            Expression deepest = expression;

            // We only allow Skip(x) expressions.  Anything else will result in
            // a NotSupportedException.
            if (expression != null && expression.Arguments.Count >= 2)
            {
                ConstantExpression constant = expression.Arguments[1] as ConstantExpression;
                if (constant != null)
                {
                    deepest = constant;
                    if (constant.Value is int)
                    {
                        return (int)constant.Value;
                    }
                }
            }

            throw new NotSupportedException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.MobileServiceTableQueryTranslator_GetCountArgument_Unsupported,
                    expression != null  && expression.Method != null ? expression.Method.Name : null,
                    deepest != null ? deepest.ToString() : null));
        }
    }
}
