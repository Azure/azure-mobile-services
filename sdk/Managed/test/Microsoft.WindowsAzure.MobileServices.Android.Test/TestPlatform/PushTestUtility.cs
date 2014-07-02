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
        private const string DefaultChannelUri =
            "17BA0791499DB908433B80F37C5FBC89B870084B";
        const string BodyTemplate = "{\"first prop\":\"first value\", \"second prop\":\"($message)\"}";
        const string DefaultToastTemplateName = "templateForToastWns";
        readonly string[] DefaultTags = { "fooWns", "barWns" };        

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
            return new GcmTemplateRegistration(channel, BodyTemplate, DefaultToastTemplateName, DefaultTags);
        }

        public void ValidateTemplateRegistration(Registration registration)
        {
            var gcmTemplateRegistration = (GcmTemplateRegistration)registration;
            Assert.AreEqual(gcmTemplateRegistration.BodyTemplate, BodyTemplate);            

            foreach (string tag in DefaultTags)
            {
                Assert.IsTrue(registration.Tags.Contains(tag));
            }

            Assert.AreEqual(gcmTemplateRegistration.Name, DefaultToastTemplateName);
            Assert.AreEqual(gcmTemplateRegistration.TemplateName, DefaultToastTemplateName);
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
            Assert.AreEqual(registration.Tags.Count(), DefaultTags.Length + 1);
        }

        public Registration GetNewNativeRegistration(string deviceId, IEnumerable<string> tags)
        {
            return new GcmRegistration(deviceId, tags);
        }        

        public Registration GetNewTemplateRegistration(string deviceId, string bodyTemplate, string templateName)
        {
            return new GcmTemplateRegistration(deviceId, bodyTemplate, templateName);
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