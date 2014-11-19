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
    /// Enumeration of binary operators.
    /// </summary>
    public enum BinaryOperatorKind
    {
        /// <summary>
        /// The logical or operator.
        /// </summary>
        Or = 0,

        /// <summary>
        /// The logical and operator.
        /// </summary>
        And = 1,

        /// <summary>
        /// The eq operator.
        /// </summary>
        Equal = 2,

        /// <summary>
        /// The ne operator.
        /// </summary>
        NotEqual = 3,

        /// <summary>
        /// The gt operator.
        /// </summary>
        GreaterThan = 4,

        /// <summary>
        /// The ge operator.
        /// </summary>
        GreaterThanOrEqual = 5,

        /// <summary>
        /// The lt operator.
        /// </summary>
        LessThan = 6,

        /// <summary>
        /// The le operator.
        /// </summary>
        LessThanOrEqual = 7,

        /// <summary>
        /// The add operator.
        /// </summary>
        Add = 8,

        /// <summary>
        /// The sub operator.
        /// </summary>
        Subtract = 9,

        /// <summary>
        /// The mul operator.
        /// </summary>
        Multiply = 10,

        /// <summary>
        /// The div operator.
        /// </summary>
        Divide = 11,

        /// <summary>
        /// The mod operator.
        /// </summary>
        Modulo = 12
    }
}
