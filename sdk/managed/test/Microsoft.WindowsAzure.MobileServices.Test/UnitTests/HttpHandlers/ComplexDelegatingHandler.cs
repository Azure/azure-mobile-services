// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    internal class ComplexDelegatingHandler : DelegatingHandler
    {
        private static List<string> allMessages = new List<string>();

        public string MessageBeforeSend { get; private set; }
        public string MessageAfterSend { get; private set; }

        public ComplexDelegatingHandler(string messageBeforeSend, string messageAfterSend)
        {
            this.MessageBeforeSend = messageBeforeSend;
            this.MessageAfterSend = messageAfterSend;
        }

        public static void ClearStoredMessages()
        {
            allMessages.Clear();
        }

        public static IEnumerable<string> AllMessages
        {
            get { return allMessages; }
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            allMessages.Add(this.MessageBeforeSend);
            var response = await base.SendAsync(request, cancellationToken);
            allMessages.Add(this.MessageAfterSend);
            return response;
        }
    }
}
