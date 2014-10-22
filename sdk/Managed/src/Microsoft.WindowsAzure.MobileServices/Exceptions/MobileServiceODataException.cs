// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Exception type representing exceptions in parsing of OData queries.
    /// </summary>
    public class MobileServiceODataException : InvalidOperationException
    {
        /// <summary>
        /// The position in string where error exists
        /// </summary>
        public int ErrorPosition { get; private set; }

        /// <summary>Creates a new instance of the <see cref="T:Microsoft.WindowsAzure.MobileService.MobileServiceODataException" /> class.</summary>
        public MobileServiceODataException() : base()
        {
        }
        /// <summary>Creates a new instance of the <see cref="T:Microsoft.WindowsAzure.MobileService.MobileServiceODataException" /> class with an error message.</summary>
        /// <param name="message">The plain text error message for this exception.</param>
        /// <param name="errorPos">The position in string where error exists.</param>
        public MobileServiceODataException(string message, int errorPos)
            : this(message, errorPos, null)
        {
        }
        
        /// <summary>Creates a new instance of the <see cref="T:Microsoft.WindowsAzure.MobileService.MobileServiceODataException" /> class with an error message and an inner exception.</summary>
        /// <param name="message">The plain text error message for this exception.</param>
        /// <param name="errorPos">The position in string where error exists.</param>
        /// <param name="innerException">The inner exception that is the cause of this exception to be thrown.</param>
        public MobileServiceODataException(string message, int errorPos, Exception innerException)
            : base(message, innerException)
        {
            this.ErrorPosition = errorPos;
        }
    }
}
