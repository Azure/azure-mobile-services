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
    /// Enumeration of unary operators.
    /// </summary>
    public enum UnaryOperatorKind
    {
        /// <summary>
        /// The unary - operator.
        /// </summary>
        Negate,
        /// <summary>
        /// The not operator.
        /// </summary>
        Not
    }
}
