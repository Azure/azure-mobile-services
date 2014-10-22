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
    /// Base class for all query nodes.
    /// </summary>
    public abstract class QueryNode
    {
        /// <summary>
        /// Gets the kind of the query node.
        /// </summary>
        public abstract QueryNodeKind Kind { get; }

        internal virtual void SetChildren(IList<QueryNode> children)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Accept a QueryNodeVisitor that
        /// walks a tree of QueryNode
        /// </summary>
        /// <typeparam name="T">Type that the visitor will return after visiting this token.</typeparam>
        /// <param name="visitor">An implementation of the visitor interface.</param>
        /// <returns>An object whose type is determined by the type parameter of the visitor.</returns>
        public virtual T Accept<T>(QueryNodeVisitor<T> visitor)
        {
            throw new NotImplementedException();
        }
    }
}
