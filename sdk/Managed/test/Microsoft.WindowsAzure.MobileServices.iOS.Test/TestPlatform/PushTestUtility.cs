// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json.Linq;
using Foundation;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    class PushTestUtility : IPushTestUtility
    {
        private const string DefaultDeviceToken =
            "<f6e7cd2 80fc5b5 d488f8394baf216506bc1bba 864d5b483d>";
        const string BodyTemplate = "{\"aps\": {\"alert\":\"boo!\"}, \"extraprop\":\"($message)\"}";
        const string DefaultToastTemplateName = "templateForToastiOS";
        readonly string[] DefaultTags = { "fooiOS", "bariOs" };

        public string GetPushHandle()
        {
            return DefaultDeviceToken;
        }

        public string GetUpdatedPushHandle()
        {
            return DefaultDeviceToken.Replace('b', 'a').Replace('B', 'a');
        }

        public JObject GetInstallation(string installationId, bool includeTemplates = false, string defaultChannelUri = null)
        {
            JObject installation = new JObject();
            installation[PushInstallationProperties.PUSHCHANNEL] = defaultChannelUri ?? TrimDeviceToken(DefaultDeviceToken);
            installation[PushInstallationProperties.PLATFORM] = Platform.Instance.PushUtility.GetPlatform();
            if (includeTemplates)
            {
                installation[PushInstallationProperties.TEMPLATES] = GetTemplates();
            }
            return installation;
        }

        public JObject GetTemplates()
        {
            JObject alert = new JObject();
            alert["alert"] = "$(message)";
            JObject aps = new JObject();
            aps["aps"] = alert;
            JObject templates = new JObject();
            templates[DefaultToastTemplateName] = aps;
            return templates;
        }
        
        internal static string TrimDeviceToken(string deviceToken)
        {
            if (deviceToken == null)
            {
                throw new ArgumentNullException("deviceToken");
            }

            return deviceToken.Trim('<', '>').Replace(" ", string.Empty).ToUpperInvariant();
        }
    }
}