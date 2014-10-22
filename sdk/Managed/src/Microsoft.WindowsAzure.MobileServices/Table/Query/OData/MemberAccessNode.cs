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
    /// Node representing an access to a member value.
    /// </summary>
    public sealed class MemberAccessNode : QueryNode
    {
        /// <summary>
        /// Gets the kind of the query node.
        /// </summary>
        public override QueryNodeKind Kind
        {
            get { return QueryNodeKind.MemberAccess; }
        }

        /// <summary>
        /// Instance to access member of.
        /// </summary>
        public QueryNode Instance { get; private set; }

        /// <summary>
        /// Gets the member name to access.
        /// </summary>
        public string MemberName { get; private set; }

        /// <summary>
        /// Initializes an instance of <see cref="MemberAccessNode"/>
        /// </summary>
        /// <param name="instance">Instance to access member of.</param>
        /// <param name="memberName">The member name to access.</param>
        public MemberAccessNode(QueryNode instance, string memberName)
        {
            this.Instance = instance;
            this.MemberName = memberName;
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
