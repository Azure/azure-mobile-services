// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Evaluates all of an expression's subtrees that aren't dependent on any
    /// parameters to the expression (things like local variables).  This is
    /// necessary so we can turn the evaluated values into constants that we
    /// can send to the server where the query executes.
    /// </summary>
    /// <remarks>
    /// Note that by evaluating as much of the expression as possible now, any
    /// future re-evaluation of the query (for the purposes of paging in the
    /// MobileServiceCollectionView, etc.) will use constants rather than
    /// the latest values in closed variables, etc.  This is desirable behavior
    /// for data source paging but should be carefully considered for other
    /// scenarios.
    /// -
    /// This code is based on the concepts discussed in
    /// http://blogs.msdn.com/b/mattwar/archive/2007/08/01/linq-building-an-iqueryable-provider-part-iii.aspx
    /// which is pointed to by MSDN as the reference for building LINQ 
    /// providers.
    /// </remarks>
    internal static class PartialEvaluator
    {
        /// <summary>
        /// Evaluate all subtrees of an expression that aren't dependent on
        /// parameters to that expression and replace the subtree with a
        /// constant expression.
        /// </summary>
        /// <param name="expression">
        /// The expression to partially evaluate.
        /// </param>
        /// <returns>
        /// An expression in which every node is either a constant or part of
        /// a subtree that depends on a one of the expression's parameters.
        /// </returns>
        public static Expression PartiallyEvaluate(Expression expression)
        {
            // Find the independent subtrees and then replace them with
            // constant expressions equivalent to the evaluated subtrees.
            List<Expression> independentSubtrees = FindIndependentSubtrees(expression);
            return EvaluateIndependentSubtrees(expression, independentSubtrees);
        }

        /// <summary>
        /// Walk the expression and compute all of the subtrees that are not
        /// dependent on any of the expressions parameters.
        /// </summary>
        /// <param name="expression">The expression to analyze.</param>
        /// <returns>
        /// A set of all the expression subtrees that are independent from the
        /// expression's parameters.  Note that this set will include every
        /// element of an independent subtree, but it's not a problem because
        /// the EvaluateIndependentSubtrees walks top down.
        /// </returns>
        private static List<Expression> FindIndependentSubtrees(Expression expression)
        {
            List<Expression> subtrees = new List<Expression>();

            // The dependent and isMemberInit flags are closed over and used to communicate
            // between different layers of the recursive call
            bool dependent = false;
            bool isMemberInit = false;

            // Walk the tree finding every 
            VisitorHelper.VisitAll(
                expression,
                (expr, recur) =>
                {
                    if (expr != null)
                    {
                        // Cache whether or not our parent is already dependent
                        // or a MemberInitExpression because we're going to reset 
                        // the flags while we evalute our descendents to see if all
                        // of our subtrees are independent.
                        bool parentIsDependent = dependent;
                        bool parentIsMemberInit = isMemberInit;

                        // Set flags to false and visit my entire subtree
                        // to see if anything sets it to true
                        dependent = false;
                        isMemberInit = expr is MemberInitExpression;

                        recur(expr);

                        // If nothing in my subtree is dependent
                        if (!dependent)
                        {
                            // A NewExpression itself will appear to be independent,
                            // but if the parent is a MemberInitExpression, the NewExpression
                            // can't be evaluated by itself. The MemberInitExpression will
                            // determine if the full expression is dependent or not, so 
                            // the NewExpression should simply not be checked for dependency.
                            NewExpression newExpression = expr as NewExpression;
                            if (newExpression != null && parentIsMemberInit)
                            {
                                return expr;
                            }

                            // Then the current node is independent if it's not
                            // related to the parameter or if it's not the
                            // constant query root (to handle degenerate cases 
                            // where we don't actually use any parameters in
                            // the query - like table.skip(2).take(3)).
                            ConstantExpression constant = expr as ConstantExpression;
                            
                            if (expr.NodeType == ExpressionType.Parameter ||
                                (constant != null && constant.Value is IQueryable))
                            {
                                dependent = true;
                            }
                            else
                            {
                                // Then the whole subtree starting at this node
                                // is independent of any expression parameters
                                // and safe for partial evaluation
                                subtrees.Add(expr);
                            }
                        }

                        dependent |= parentIsDependent;
                    }

                    return expr;
                });

            return subtrees;
        }

        /// <summary>
        /// Replace all the independent subtrees in an expression with
        /// constants equal to the expression's evaluated result.
        /// </summary>
        /// <param name="expression">
        /// The expression to partially evaluate.
        /// </param>
        /// <param name="subtrees">
        /// The independent subtrees in the expression.
        /// </param>
        /// <returns>
        /// An expression that depends only on parameter values.
        /// </returns>
        private static Expression EvaluateIndependentSubtrees(Expression expression, List<Expression> subtrees)
        {
            // Walk top down checking each node to see if it's the root of an
            // indedent subtree.  Once we find one, we'll compile it and
            // replace the whole subtree with a constant.
            return VisitorHelper.VisitAll(
                expression,
                (expr, recur) =>
                {
                    if (expr != null &&
                        subtrees.Contains(expr) &&
                        expr.NodeType != ExpressionType.Constant)
                    {
                        // Turn the expression into a parameterless lambda,
                        // evaluate it, and return the response as a constant.
                        Delegate compiled = Expression.Lambda(expr).Compile();
                        object value = compiled.DynamicInvoke();
                        return Expression.Constant(value, expr.Type);
                    }
                    else
                    {
                        return recur(expr);
                    }
                });
        }
    }
}
