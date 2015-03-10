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

        public Registration GetTemplateRegistrationForToast()
        {
            var deviceToken = GetPushHandle();
            return new ApnsTemplateRegistration(deviceToken, BodyTemplate, null, DefaultToastTemplateName, DefaultTags);
        }

        public void ValidateTemplateRegistration(Registration registration)
        {
            var apnsTemplateRegistration = (ApnsTemplateRegistration)registration;
            Assert.AreEqual(apnsTemplateRegistration.BodyTemplate, BodyTemplate);

            foreach (string tag in DefaultTags)
            {
                Assert.IsTrue(registration.Tags.Contains(tag));
            }

            Assert.AreEqual(apnsTemplateRegistration.Name, DefaultToastTemplateName);
            Assert.AreEqual(apnsTemplateRegistration.TemplateName, DefaultToastTemplateName);
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
            // TODO: Uncomment when .Net Runtime implements installationID
            //Assert.IsTrue(registration.Tags.Contains(zumoInstallationId));
            //Assert.AreEqual(registration.Tags.Count(), DefaultTags.Length + 1);
        }

        public Registration GetNewNativeRegistration(string deviceId, IEnumerable<string> tags)
        {
            return new ApnsRegistration(deviceId, tags);
        }

        public Registration GetNewTemplateRegistration(string deviceId, string bodyTemplate, string templateName)
        {
            return new ApnsTemplateRegistration(deviceId, bodyTemplate, templateName, null);
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

        public JObject GetInstallation(string installationId, bool includeTemplates = false, string defaultChannelUri = null)
        {
            JObject installation = new JObject();
            installation[PushInstallationProperties.PUSHCHANNEL] = defaultChannelUri ?? ApnsRegistration.ParseDeviceToken(DefaultDeviceToken);
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
    }
}