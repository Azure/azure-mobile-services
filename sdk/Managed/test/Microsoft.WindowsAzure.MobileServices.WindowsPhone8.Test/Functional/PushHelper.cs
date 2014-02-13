// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.MobileServices.Test.Functional
{
    class PushHelper
    {
        const string BodyTemplate = "<wp:Notification xmlns:wp=\"WPNotification\"><wp:Toast><wp:Text1>$(message)</wp:Text1><wp:Text2>Test message</wp:Text2></wp:Toast></wp:Notification>";
        const string DefaultToastTemplateName = "templateForToast";

        private const string DefaultChannelUri =
            "http://sn1.notify.live.net/throttledthirdparty/01.00/AQG14T6NQCB_QYweWtUweyqjAgAAAAADAQAAAAQUZm52OkJCMjg1QTg1QkZDMkUxREQFBlVTU0MwMQ";
        static readonly string[] DefaultTags = { "foo", "bar" };
        static readonly ReadOnlyDictionary<string, string> DefaultHeaders = new ReadOnlyDictionary<string, string>(new Dictionary<string, string> { { "X-MessageID", "TestMessageID" } });
        static readonly ReadOnlyDictionary<string, string> DetectedHeaders = new ReadOnlyDictionary<string, string>(new Dictionary<string, string> { { "X-WindowsPhone-Target", "toast" }, { "X-NotificationClass", "2" } });

        public static string GetChannel()
        {
            return DefaultChannelUri;
        }

        public static string GetUpdatedChannel()
        {
            return DefaultChannelUri.Replace('A', 'B');
        }

        public static TemplateRegistration GetTemplateRegistrationForToast()
        {
            var channel = GetChannel();
            return new TemplateRegistration(channel, BodyTemplate, DefaultToastTemplateName, DefaultTags, DefaultHeaders);
        }

        static void ValidateTemplateRegistration(TemplateRegistration registration)
        {
            Assert.AreEqual(registration.BodyTemplate, BodyTemplate);
            
            foreach (KeyValuePair<string, string> header in DefaultHeaders)
            {
                Assert.IsTrue(registration.MpnsHeaders.ContainsKey(header.Key));
                Assert.AreEqual(registration.MpnsHeaders[header.Key], header.Value);
            }

            foreach (KeyValuePair<string, string> header in DetectedHeaders)
            {
                Assert.IsTrue(registration.MpnsHeaders.ContainsKey(header.Key));
                Assert.AreEqual(registration.MpnsHeaders[header.Key], header.Value);
            }

            Assert.AreEqual(registration.MpnsHeaders.Count, DefaultHeaders.Count + DetectedHeaders.Count);

            foreach (string tag in DefaultTags)
            {
                Assert.IsTrue(registration.Tags.Contains(tag));
            }

            Assert.AreEqual(registration.Name, DefaultToastTemplateName);
            Assert.AreEqual(registration.TemplateName, DefaultToastTemplateName);
        }

        public static void ValidateTemplateRegistrationBeforeRegister(TemplateRegistration registration)
        {
            ValidateTemplateRegistration(registration);                        
            Assert.AreEqual(registration.Tags.Count, DefaultTags.Length);
            Assert.IsNull(registration.RegistrationId);
        }

        public static void ValidateTemplateRegistrationAfterRegister(TemplateRegistration registration, string zumoInstallationId)
        {
            ValidateTemplateRegistration(registration);
            Assert.IsNotNull(registration.RegistrationId);
            Assert.IsTrue(registration.Tags.Contains(zumoInstallationId));
            Assert.AreEqual(registration.Tags.Count, DefaultTags.Length + 1);
        }
    }
}