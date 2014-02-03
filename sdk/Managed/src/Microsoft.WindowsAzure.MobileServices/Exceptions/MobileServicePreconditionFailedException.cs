// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides details of http response with status code of 'Precondition Failed'
    /// </summary>
    public class MobileServicePreconditionFailedException : MobileServiceInvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="MobileServicePreconditionFailedException"/> class.
        /// </summary>
        /// <param name="innerException">
        /// The inner exception.
        /// </param>
        /// <param name="value">
        /// The current instance from the server that the precondition failed for.
        /// </param>
        public MobileServicePreconditionFailedException(MobileServiceInvalidOperationException innerException, JToken value): base(innerException.Message, innerException.Request, innerException.Response)
        {
            this.Value = value;
        }

        /// <summary>
        /// The current instance from the server that the precondition failed for.
        /// </summary>
        public JToken Value { get; private set; }
    }

    /// <summary>
    /// Provides details of http response with status code of 'Precondition Failed'
    /// </summary>
    public class MobileServicePreconditionFailedException<T> : MobileServicePreconditionFailedException
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="MobileServicePreconditionFailedException"/> class.
        /// </summary>
        /// <param name="innerException">
        /// The inner exception.
        /// </param>
        /// <param name="item">
        /// The current instance from the server that the precondition failed for.
        /// </param>
        public MobileServicePreconditionFailedException(MobileServicePreconditionFailedException innerException, T item): base(innerException, innerException.Value)
        {
            this.Item = item;
        }

        /// <summary>
        /// The current instance from the server that the precondition failed for.
        /// </summary>
        public T Item { get; private set; }
    }
}
