// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
    internal class MobileServiceTableQueryTranslator<T>
    {
        /// <summary>
        /// The compiled query description generated from the expression tree.
        /// </summary>
        private MobileServiceTableQueryDescription queryDescription;

        /// <summary>
        /// The query which is being translated.
        /// </summary>
        private MobileServiceTableQuery<T> query;

        /// <summary>
        /// Initializes a new instance of the MobileServiceTableQueryTranslator
        /// class.
        /// </summary>
        /// <param name="query">
        /// The <see cref="T:MobileServiceTableQuery`1{T}"/> which 
        /// is being translated.
        /// </param>
        internal MobileServiceTableQueryTranslator(MobileServiceTableQuery<T> query)
        {
            Debug.Assert(query != null);

            this.query = query;

            this.queryDescription = new MobileServiceTableQueryDescription(query.Table.TableName, query.RequestTotalCount);
        }

        /// <summary>
        /// The contract resolver to use to determine property 
        /// names from members used within expressions.
        /// </summary>
        private MobileServiceContractResolver ContractResolver
        {
            get
            {
                return this.query.Table.MobileServiceClient.SerializerSettings.ContractResolver;
            }
        }

        /// <summary>
        /// Translate an expression tree into a compiled query description that
        /// can be executed on the server.
        /// </summary>
        /// <returns>
        /// A compiled query description.
        /// </returns>
        public MobileServiceTableQueryDescription Translate()
        {
            // Evaluate any independent subexpressions so we end up with a tree
            // full of constants or things that depend directly on our values.
            IExpressionUtility expressionUtility = Platform.Instance.ExpressionUtility;
            Expression expression = expressionUtility.PartiallyEvaluate(this.query.Query.Expression);

            // Build a new query from the expression tree
            this.Visit(expression);

            // set the projectType if there was no Projection in the query
            if (this.queryDescription.ProjectionArgumentType == null)
            {
                this.queryDescription.ProjectionArgumentType = typeof(T);
            }

            return this.queryDescription;
        }

        /// <summary>
        /// Visits the <paramref name="expression"/>. 
        /// </summary>
        /// <remarks>
        /// The root of the expression must be a MethodCall node or the query root itself.  
        /// This will be the case
        /// because we generated the expression using <see cref="MobileServiceTableQuery{T}"/>
        /// instance.
        /// </remarks>
        /// <param name="expression">
        /// The expression to visit.
        /// </param>
        private void Visit(Expression expression)
        {
            MethodCallExpression methodCallExpression = expression as MethodCallExpression;
            if (methodCallExpression != null)
            {
                VisitMethodCall(methodCallExpression);
            }
        }

        /// <summary>
        /// Process the core LINQ operators that are supported by Mobile
        /// Services.
        /// </summary>
        /// <param name="expression">
        /// The expression to visit.
        /// </param>
        /// <returns>
        /// The visited expression.
        /// </returns>
        private Expression VisitMethodCall(MethodCallExpression expression)
        {
            // Recurse down the target of the method call
            if (expression != null && expression.Arguments.Count >= 1)
            {
                Expression firstArgument = expression.Arguments[0];
                if (firstArgument.NodeType == ExpressionType.Call)
                {
                    VisitMethodCall((MethodCallExpression)firstArgument);
                }
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
                    this.AddOrdering(expression, true, true);
                    break;                    
                case "ThenBy":
                    this.AddOrdering(expression, true);
                    break;
                case "OrderByDescending":
                    this.AddOrdering(expression, false, true);
                    break;                    
                case "ThenByDescending":
                    this.AddOrdering(expression, false);
                    break;
                //multiple skips add up
                case "Skip":
                    int skipCount = GetCountArgument(expression);
                    this.queryDescription.Skip = this.queryDescription.Skip.HasValue ? this.queryDescription.Skip.Value + skipCount : skipCount;
                    break;
                // multiple takes, takes minimum
                case "Take":
                    int takeCount = GetCountArgument(expression);
                    this.queryDescription.Top = this.queryDescription.Top.HasValue ? Math.Min(this.queryDescription.Top.Value, takeCount) : takeCount;
                    break;
                default:
                    ThrowForUnsupportedException(expression);
                    break;                    
            }

            return expression;
        }

        /// <summary>
        /// Add a filter expression to the query.
        /// </summary>
        /// <param name="expression">
        /// Where method call expression.
        /// </param>
        private void AddFilter(MethodCallExpression expression)
        {
            if (expression != null && expression.Arguments.Count >= 2)
            {
                LambdaExpression lambda = StripQuote(expression.Arguments[1]) as LambdaExpression;
                if (lambda != null)
                {
                    string filter = FilterBuildingExpressionVisitor.Create(lambda.Body, this.ContractResolver);
                    if (this.queryDescription.Filter != null)
                    {
                        // If there's already a filter value, that means the
                        // query has multiple where clauses which we'll just
                        // join together with "and"s.
                        this.queryDescription.Filter += " and " + filter;
                    }
                    else
                    {
                        this.queryDescription.Filter = filter;
                    }
                    return;
                }
            }

            ThrowForUnsupportedException(expression);
        }

        /// <summary>
        /// Add a projection to the query.
        /// </summary>
        /// <param name="expression">
        /// Select method call expression.
        /// </param>
        private void AddProjection(MethodCallExpression expression)
        {
            // We only support Select(x => ...) projections.  Anything else
            // will throw a NotSupportException.
            if (expression != null && expression.Arguments.Count == 2)
            {
                LambdaExpression projection = StripQuote(expression.Arguments[1]) as LambdaExpression;
                if (projection != null && projection.Parameters.Count == 1)
                {
                    // Compile the projection into a function that we can apply
                    // to the deserialized value to transform it accordingly.
                    this.queryDescription.Projections.Add(projection.Compile());

                    // We only need to capture the projection argument type and members for the
                    // very first projection.
                    if (this.queryDescription.ProjectionArgumentType == null)
                    {
                        // Store the type of the very first input to the projection as we'll
                        // need that for deserialization of values (since the
                        // projection will change the expected type of the data
                        // source)
                        this.queryDescription.ProjectionArgumentType = projection.Parameters[0].Type;

                        // Filter the selection down to just the values used by
                        // the projection
                        IExpressionUtility expressionUtility = Platform.Instance.ExpressionUtility;
                        foreach (MemberExpression memberExpression in expressionUtility.GetMemberExpressions(projection.Body))
                        {
                            // Ensure we only process members of the parameter
                            string memberName = FilterBuildingExpressionVisitor.GetTableMemberName(memberExpression, this.ContractResolver);
                            if (memberName != null)
                            {
                                queryDescription.Selection.Add(memberName);
                            }
                        }

                        //Make sure we also include all the members that would be
                        //required for deserialization
                        JsonContract contract = this.ContractResolver.ResolveContract(this.queryDescription.ProjectionArgumentType);
                        JsonObjectContract objectContract = contract as JsonObjectContract;
                        if (objectContract != null)
                        {
                            foreach (string propertyName in objectContract.Properties
                                                                          .Where(p => p.Required == Required.Always || 
                                                                                      p.Required == Required.AllowNull)
                                                                          .Select(p => p.PropertyName))
                            {
                                if (!this.queryDescription.Selection.Contains(propertyName))
                                {
                                    this.queryDescription.Selection.Add(propertyName);
                                }
                            }
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
        /// <param name="expression">
        /// The ordering method call.
        /// </param>
        /// <param name="ascending">
        /// Whether the order is ascending or descending.
        /// </param>
        /// <param name="prepend">
        /// Indicates if the expression should be prepended or not.
        /// </param>
        private void AddOrdering(MethodCallExpression expression, bool ascending, bool prepend = false)
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
                        string memberName = FilterBuildingExpressionVisitor.GetTableMemberName(memberAccess, this.ContractResolver);
                        if (memberName != null)
                        {
                            // Add the ordering
                            if(prepend)
                            {
                                this.queryDescription.Ordering.Insert(0, new KeyValuePair<string, bool>(memberName, ascending));
                            } 
                            else 
                            {
                                this.queryDescription.Ordering.Add(new KeyValuePair<string, bool>(memberName, ascending));                            
                            }

                            return;
                        }
                    }
                }
            }

            throw new NotSupportedException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.MobileServiceTableQueryTranslator_MembersOnlyInExpression,
                    expression != null && expression.Method != null ? expression.Method.Name : null,
                    deepest != null ? deepest.ToString() : null));
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
        /// Throw a NotSupportedException for an unsupported expression.
        /// </summary>
        /// <param name="expression">
        /// The unsupported expression.
        /// </param>
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
                    Resources.MobileServiceTableQueryTranslator_UnsupportedExpression,
                    expression != null && expression.Method != null ? expression.Method : null,
                    deepest != null ? deepest.ToString() : null));
        }

        /// <summary>
        /// Get the count argument value for a Skip or Take method call.
        /// </summary>
        /// <param name="expression">
        /// The method call expression.
        /// </param>
        /// <returns>
        /// The count argument.
        /// </returns>
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
                    Resources.MobileServiceTableQueryTranslator_SingleIntegerOnly,
                    expression != null && expression.Method != null ? expression.Method.Name : null,
                    deepest != null ? deepest.ToString() : null));
        }
    }
}
