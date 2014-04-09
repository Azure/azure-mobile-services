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
    /// Node representing a unary operator.
    /// </summary>
    public sealed class UnaryOperatorNode: QueryNode
    {
        /// <summary>
        /// Gets the operator represented by this node.
        /// </summary>
        public override QueryNodeKind Kind
        {
            get { return QueryNodeKind.UnaryOperator; }
        }

        /// <summary>
        /// The operand of the unary operator.
        /// </summary>
        public QueryNode Operand { get; private set; }

        /// <summary>
        /// The operator represented by this node.
        /// </summary>
        public UnaryOperatorKind OperatorKind { get; private set; }

        internal override void SetChildren(IList<QueryNode> children)
        {
            Debug.Assert(children.Count >= 1);
            this.Operand = children[0];
        }

        /// <summary>
        /// Initializes an instance of <see cref="UnaryOperatorNode"/>
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="operand"></param>
        public UnaryOperatorNode(UnaryOperatorKind kind, QueryNode operand)
        {
            this.OperatorKind = kind;
            this.Operand = operand;
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
    }
}
