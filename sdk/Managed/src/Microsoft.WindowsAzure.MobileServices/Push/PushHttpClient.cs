//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal sealed class PushHttpClient
    {
        private readonly MobileServiceClient client;

        internal PushHttpClient(MobileServiceClient client)
        {
            this.client = client;
        }

        public Task DeleteInstallationAsync()
        {
            return this.client.MobileAppHttpClient.RequestAsync(HttpMethod.Delete, string.Format("push/installations/{0}", Uri.EscapeUriString(this.client.applicationInstallationId)), this.client.CurrentUser, ensureResponseContent: false);
        }

        public Task CreateOrUpdateInstallationAsync(JObject installation)
        {
            return this.client.MobileAppHttpClient.RequestAsync(HttpMethod.Put, string.Format("push/installations/{0}", Uri.EscapeUriString(this.client.applicationInstallationId)), this.client.CurrentUser, installation.ToString(), ensureResponseContent: false);
        }
    }
}