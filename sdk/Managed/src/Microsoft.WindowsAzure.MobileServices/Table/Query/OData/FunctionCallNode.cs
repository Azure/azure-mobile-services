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
    /// Node representing a function call.
    /// </summary>
    public sealed class FunctionCallNode : QueryNode
    {
        /// <summary>
        /// Gets the kind of the query node.
        /// </summary>
        public override QueryNodeKind Kind
        {
            get { return QueryNodeKind.FunctionCall; }
        }

        /// <summary>
        /// Gets the name of the function to call.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the list of arguments to this function call.
        /// </summary>
        public IList<QueryNode> Arguments { get; private set; }

        /// <summary>
        /// Initializes an instance of <see cref="FunctionCallNode"/>
        /// </summary>
        /// <param name="name">The name of the function to call.</param>
        /// <param name="arguments">The list of arguments to this function call.</param>
        public FunctionCallNode(string name, IList<QueryNode> arguments)
        {
            this.Name = name;
            this.Arguments = arguments;
        }

        internal override void SetChildren(IList<QueryNode> children)
        {
            this.Arguments = children.ToList();
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
