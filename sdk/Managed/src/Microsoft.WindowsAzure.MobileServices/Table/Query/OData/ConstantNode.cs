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
    /// Node representing a primitive constant value.
    /// </summary>  
    public sealed class ConstantNode: QueryNode
    {
        /// <summary>
        /// Gets the kind of the query node.
        /// </summary>
        public override QueryNodeKind Kind
        {
            get { return QueryNodeKind.Constant; }
        }

        /// <summary>
        /// Gets the primitive constant value.
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Initializes an instance of <see cref="ConstantNode"/>
        /// </summary>
        /// <param name="value">This node's primitive value.</param>
        public ConstantNode(object value)
        {
            this.Value = value;
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
