// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------


namespace Microsoft.WindowsAzure.MobileServices
{
    internal class MobileServiceHttpResponse
    {
        public string Content { get; private set; }

        public string Etag { get; private set; }

        public MobileServiceHttpResponse(string content, string etag)
        {
            this.Content = content;
            this.Etag = etag;
        }
    }
}
