// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq.Expressions;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// An implementation of the <see cref="IExpressionUtility"/> interface
    /// for that uses the ExpressionVisitor
    /// </summary>
    internal class ExpressionUtility : IExpressionUtility
    {
        /// <summary>
        /// A singleton instance of the <see cref="ExpressionUtility"/>.
        /// </summary>
        private static IExpressionUtility instance = new ExpressionUtility();

        /// <summary>
        /// A singleton instance of the <see cref="ExpressionUtility"/>.
        /// </summary>
        public static IExpressionUtility Instance
        {
            get
            {
                return instance;
            }
        }

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
        public Expression PartiallyEvaluate(Expression expression)
        {
            return PartialEvaluator.PartiallyEvaluate(expression);
        }

        /// <summary>
        /// Returns the MemberExpressions in the expression hierarchy of the <paramref name="expression"/>.
        /// </summary>
        /// <param name="expression">The expression to search for children MemberExpressions</param>
        /// <returns>A collection of MemberExpressions.</returns>
        public IEnumerable<MemberExpression> GetMemberExpressions(Expression expression)
        {
            List<MemberExpression> members = new List<MemberExpression>();
            VisitorHelper.VisitMembers(
                        expression,
                        (expr, recur) =>
                        {
                            // Ensure we only process members of the parameter
                            if (expr is MemberExpression)
                            {
                                members.Add((MemberExpression)expr);
                            }

                            return recur(expr);
                        });

            return members;
        }
    }
}
