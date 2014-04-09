// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Query
{
    /// <summary>
    /// Query node representing a binary operator.
    /// </summary>
    public sealed class BinaryOperatorNode: QueryNode
    {
        /// <summary>
        /// Gets the kind of the query node.
        /// </summary>
        public override QueryNodeKind Kind
        {
            get { return QueryNodeKind.BinaryOperator; }
        }

        /// <summary>
        /// The operator represented by this node.
        /// </summary>
        public BinaryOperatorKind OperatorKind { get; private set; }

        /// <summary>
        /// Gets the left operand.
        /// </summary>
        public QueryNode LeftOperand { get; internal set; }

        /// <summary>
        /// Gets the right operand.
        /// </summary>
        public QueryNode RightOperand { get; internal set; }

        /// <summary>
        /// Initializes instance of <see cref="BinaryOperatorNode"/>
        /// </summary>
        /// <param name="kind">The kind of this node.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        public BinaryOperatorNode(BinaryOperatorKind kind, QueryNode left, QueryNode right)
        {
            this.OperatorKind = kind;
            this.LeftOperand = left;
            this.RightOperand = right;
        }

        /// <summary>
        /// Accept a <see cref="QueryNodeVisitor{T}" /> to walk a tree of <see cref="QueryNode" />s.
        /// </summary>
        /// <typeparam name="T">Type that the visitor will return after visiting this token.</typeparam>
        /// <param name="visitor">An implementation of the visitor interface.</param>
        /// <returns>An object whose type is determined by the type parameter of the visitor.</returns>
        public override T Accept<T>(QueryNodeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }        

        internal override void SetChildren(IList<QueryNode> children)
        {
            Debug.Assert(children.Count == 2);

            this.LeftOperand = children[0];
            this.RightOperand = children[1];
        }
    }
}
