// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Query
{
    /// <summary>
    /// Visitor interface for walking the QueryNode Tree.
    /// </summary>
    /// <typeparam name="T">Generic type produced by the visitor.</typeparam>
    public abstract class QueryNodeVisitor<T>
    {
        /// <summary>
        /// Visit a BinaryOperatorNode
        /// </summary>
        /// <param name="nodeIn">the node to visit</param>
        /// <returns>Defined by the implementer</returns>
        public virtual T Visit(BinaryOperatorNode nodeIn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visit a ConstantNode
        /// </summary>
        /// <param name="nodeIn">the node to visit</param>
        /// <returns>Defined by the implementer</returns>
        public virtual T Visit(ConstantNode nodeIn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visit a MemberAccessNode
        /// </summary>
        /// <param name="nodeIn">the node to visit</param>
        /// <returns>Defined by the implementer</returns>
        public virtual T Visit(MemberAccessNode nodeIn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visit an ODataMethodCallNode
        /// </summary>
        /// <param name="nodeIn">the node to visit</param>
        /// <returns>Defined by the implementer</returns>
        public virtual T Visit(FunctionCallNode nodeIn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visit a UnaryOperatorNode
        /// </summary>
        /// <param name="nodeIn">the node to visit</param>
        /// <returns>Defined by the implementer</returns>
        public virtual T Visit(UnaryOperatorNode nodeIn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visit a ConvertNode
        /// </summary>
        /// <param name="nodeIn">the node to visit</param>
        /// <returns>Defined by the implementer</returns>
        public virtual T Visit(ConvertNode nodeIn)
        {
            throw new NotImplementedException();
        }
    }
}
