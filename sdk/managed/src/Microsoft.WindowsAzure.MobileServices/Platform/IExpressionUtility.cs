// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq.Expressions;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// An interface for platform-specific assemblies to provide utility functions
    /// regarding Linq expressions.
    /// </summary>
    interface IExpressionUtility
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
        /// a subtree that depends on one or more of the expression's parameters.
        /// </returns>
        Expression PartiallyEvaluate(Expression expression);

        /// <summary>
        /// Returns all of the <see cref="MemberExpression"/> instances in the 
        /// expression hierarchy of the <paramref name="expression"/> regardless 
        /// of their depth in the hierarchy.
        /// </summary>
        /// <param name="expression">
        /// The expression to search for descendant <see cref="MemberExpression"/> 
        /// instances.
        /// </param>
        /// <returns>
        /// A collection of descendant <see cref="MemberExpression"/> instances.
        /// </returns>
        IEnumerable<MemberExpression> GetMemberExpressions(Expression expression);
    }
}
