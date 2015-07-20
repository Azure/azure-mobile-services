// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    class PushTestUtility : IPushTestUtility
    {
        const string BodyTemplate = "<wp:Notification xmlns:wp=\"WPNotification\"><wp:Toast><wp:Text1>$(message)</wp:Text1><wp:Text2>Test message</wp:Text2></wp:Toast></wp:Notification>";
        const string DefaultToastTemplateName = "templateForToast";

        private const string DefaultChannelUri =
            "http://sn1.notify.live.net/throttledthirdparty/01.00/AQG14T6NQCB_QYweWtUweyqjAgAAAAADAQAAAAQUZm52OkJCMjg1QTg1QkZDMkUxREQFBlVTU0MwMQ";
        static readonly string[] DefaultTags = { "foo", "bar" };
        static readonly ReadOnlyDictionary<string, string> DefaultHeaders = new ReadOnlyDictionary<string, string>(new Dictionary<string, string> { { "X-MessageID", "TestMessageID" } });
        static readonly ReadOnlyDictionary<string, string> DetectedHeaders = new ReadOnlyDictionary<string, string>(new Dictionary<string, string> { { "X-WindowsPhone-Target", "toast" }, { "X-NotificationClass", "2" } });

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
                installation[PushInstallationProperties.TEMPLATES] = GetTemplates();
            }
            return installation;
        }

        public JObject GetTemplates()
        {
            JObject templateHeaders = new JObject();
            templateHeaders["X-WindowsPhone-Target"] = "mpns/toast";
            JObject temptlateBody = new JObject();
            temptlateBody["body"] = BodyTemplate;
            temptlateBody["headers"] = templateHeaders;
            JObject templates = new JObject();
            templates[DefaultToastTemplateName] = temptlateBody;
            return templates;
        }
    }
}
