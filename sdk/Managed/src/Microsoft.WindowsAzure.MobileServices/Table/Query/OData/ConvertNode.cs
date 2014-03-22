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
    /// Node representing a conversion of primitive type to another type.
    /// </summary>
    public sealed class ConvertNode: QueryNode
    {
        /// <summary>
        /// Gets the kind of the query node.
        /// </summary>
        public override QueryNodeKind Kind
        {
            get { return QueryNodeKind.Convert; }
        }

        /// <summary>
        /// Get the source value to convert.
        /// </summary>
        public QueryNode Source { get; private set; }

        /// <summary>
        /// Get the type we're converting to.
        /// </summary>
        public Type  TargetType {get; private set;}

         /// <summary>
        /// Initializes an instance of <see cref="ConvertNode"/>
        /// </summary>
        /// <param name="source">The node to convert.</param>
        /// <param name="targetType">The type to convert the node to</param>
        public ConvertNode(QueryNode source, Type targetType)
        {
            this.Source = source;
            this.TargetType = targetType;
        }

        internal override void SetChildren(IList<QueryNode> children)
        {
            Debug.Assert(children.Count >= 1);
            this.Source = children[0];
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
