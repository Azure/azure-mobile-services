// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices.TestFramework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Registration GetTemplateRegistrationForToast()
        {
            var channel = GetPushHandle();
            return new MpnsTemplateRegistration(channel, BodyTemplate, DefaultToastTemplateName, DefaultTags, DefaultHeaders);
        }

        public void ValidateTemplateRegistration(Registration registration)
        {
            var mpnsTemplateRegistration = (MpnsTemplateRegistration)registration;
            Assert.AreEqual(mpnsTemplateRegistration.BodyTemplate, BodyTemplate);

            foreach (KeyValuePair<string, string> header in DefaultHeaders)
            {
                Assert.IsTrue(mpnsTemplateRegistration.MpnsHeaders.ContainsKey(header.Key));
                Assert.AreEqual(mpnsTemplateRegistration.MpnsHeaders[header.Key], header.Value);
            }

            foreach (KeyValuePair<string, string> header in DetectedHeaders)
            {
                Assert.IsTrue(mpnsTemplateRegistration.MpnsHeaders.ContainsKey(header.Key));
                Assert.AreEqual(mpnsTemplateRegistration.MpnsHeaders[header.Key], header.Value);
            }

            Assert.AreEqual(mpnsTemplateRegistration.MpnsHeaders.Count, DefaultHeaders.Count + DetectedHeaders.Count);

            foreach (string tag in DefaultTags)
            {
                Assert.IsTrue(registration.Tags.Contains(tag));
            }

            Assert.AreEqual(mpnsTemplateRegistration.Name, DefaultToastTemplateName);
            Assert.AreEqual(mpnsTemplateRegistration.TemplateName, DefaultToastTemplateName);
        }

        public void ValidateTemplateRegistrationBeforeRegister(Registration registration)
        {
            ValidateTemplateRegistration(registration);
            Assert.AreEqual(registration.Tags.Count(), DefaultTags.Length);
            Assert.IsNull(registration.RegistrationId);
        }

        public void ValidateTemplateRegistrationAfterRegister(Registration registration, string zumoInstallationId)
        {
            ValidateTemplateRegistration(registration);
            Assert.IsNotNull(registration.RegistrationId);
            Assert.IsTrue(registration.Tags.Contains(zumoInstallationId));
            Assert.AreEqual(registration.Tags.Count(), DefaultTags.Length + 1);
        }

        public Registration GetNewNativeRegistration(string deviceId, IEnumerable<string> tags)
        {
            return new MpnsRegistration(deviceId, tags);
        }

        public Registration GetNewTemplateRegistration(string deviceId, string bodyTemplate, string templateName)
        {
            return new MpnsTemplateRegistration(deviceId, bodyTemplate, templateName);
        }

        public string GetListNativeRegistrationResponse()
        {
            return "[{\"registrationId\":\"7313155627197174428-6522078074300559092-1\",\"tags\":[\"fooWns\",\"barWns\",\"4de2605e-fd09-4875-a897-c8c4c0a51682\"],\"deviceId\":\"http://channelUri.com/a b\"}]";
        }

        public string GetListTemplateRegistrationResponse()
        {
            return "[{\"registrationId\":\"7313155627197174428-6522078074300559092-1\",\"tags\":[\"fooWns\",\"barWns\",\"4de2605e-fd09-4875-a897-c8c4c0a51682\"],\"deviceId\":\"http://channelUri.com/a b\",\"templateBody\":\"cool template body\",\"templateName\":\"cool name\"}]";
        }

        public string GetListMixedRegistrationResponse()
        {
            return "[{\"registrationId\":\"7313155627197174428-6522078074300559092-1\",\"tags\":[\"fooWns\",\"barWns\",\"4de2605e-fd09-4875-a897-c8c4c0a51682\"],\"deviceId\":\"http://channelUri.com/a b\"}, " +
            "{\"registrationId\":\"7313155627197174428-6522078074300559092-1\",\"tags\":[\"fooWns\",\"barWns\",\"4de2605e-fd09-4875-a897-c8c4c0a51682\"],\"deviceId\":\"http://channelUri.com/a b\",\"templateBody\":\"cool template body\",\"templateName\":\"cool name\"}]";
        }
    }
}
