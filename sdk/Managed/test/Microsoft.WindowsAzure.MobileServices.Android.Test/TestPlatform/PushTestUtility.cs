// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    class PushTestUtility : IPushTestUtility
    {
        private const string DefaultChannelUri = "17BA0791499DB908433B80F37C5FBC89B870084B";

        public string GetPushHandle()
        {
            return DefaultChannelUri;
        }

        public string GetUpdatedPushHandle()
        {
            return DefaultChannelUri.Replace('A', 'B');
        }

        public JObject GetInstallation(string installationId, bool includeTemplates = false, string defaultChannelUri = null)
        {
            JObject installation = new JObject();
            installation[PushInstallationProperties.PUSHCHANNEL] = defaultChannelUri ?? DefaultChannelUri;
            installation[PushInstallationProperties.PLATFORM] = Platform.Instance.PushUtility.GetPlatform();
            if (includeTemplates)
            {
                JObject msg = new JObject();
                msg["msg"] = "$(message)";
                JObject data = new JObject();
                data["data"] = msg;
                installation[PushInstallationProperties.TEMPLATES] = data;
            }
            return installation;
        }

        public JObject GetTemplates()
        {
            JObject msg = new JObject();
            msg["msg"] = "$(message)";
            JObject data = new JObject();
            data["data"] = msg;
            return data;
        }
    }
}