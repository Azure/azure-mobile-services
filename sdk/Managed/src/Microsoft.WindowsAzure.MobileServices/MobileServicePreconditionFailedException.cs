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
        public MobileServicePreconditionFailedException(MobileServiceInvalidOperationException innerException, JToken value): base(innerException.Message, innerException.Request, innerException.Response)
        {
            this.Value = value;
        }

        public JToken Value { get; private set; }
    }

    /// <summary>
    /// Provides details of http response with status code of 'Precondition Failed'
    /// </summary>
    public class MobileServicePreconditionFailedException<T> : MobileServicePreconditionFailedException
    {
        public MobileServicePreconditionFailedException(MobileServicePreconditionFailedException innerException, T item): base(innerException, innerException.Value)
        {
            this.Item = item;
        }

        public T Item { get; private set; }
    }
}
