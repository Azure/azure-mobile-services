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
    /// Enumeration of kinds of query nodes.
    /// </summary>
    public enum QueryNodeKind
    {
        /// <summary>
        /// A constant value.
        /// </summary>
        Constant = 0,
        /// <summary>
        /// Node used to represent a unary operator.
        /// </summary>
        UnaryOperator = 1,
        /// <summary>
        /// Node used to represent a binary operator.
        /// </summary>
        BinaryOperator = 2,
        /// <summary>
        /// Node the represents a function call.
        /// </summary>
        FunctionCall = 3,
        /// <summary>
        /// Node describing access to a member.
        /// </summary>
        MemberAccess = 4,
        /// <summary>
        /// A node that represents conversion from one type to another.
        /// </summary>
        Convert = 5
    }
}
