// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.MobileServices.Test.Functional
{
    class PushHelper
    {
        private const string DefaultChannelUri =
            "https://bn1.notify.windows.com/?token=AgYAAADs42685sa5PFCEy82eYpuG8WCPB098AWHnwR8kNRQLwUwf%2f9p%2fy0r82m4hxrLSQ%2bfl5aNlSk99E4jrhEatfsWgyutFzqQxHcLk0Xun3mufO2G%2fb2b%2ftjQjCjVcBESjWvY%3d";
        const string BodyTemplate = "<toast><visual><binding template=\"ToastText01\"><text id=\"1\">$(message)</text></binding></visual></toast>";
        const string DefaultToastTemplateName = "templateForToastWns";
        static readonly string[] DefaultTags = { "fooWns", "barWns" };
        static readonly ReadOnlyDictionary<string, string> DefaultHeaders = new ReadOnlyDictionary<string, string>(new Dictionary<string, string> { { "x-wns-ttl", "100000" } });
        static readonly ReadOnlyDictionary<string, string> DetectedHeaders = new ReadOnlyDictionary<string, string>(new Dictionary<string, string> { { "x-wns-type", "wns/toast" } }); 

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
                string foundKey = registration.WnsHeaders.Keys.Single(s => s.ToLower() == header.Key.ToLower());
                Assert.IsNotNull(foundKey);
                Assert.AreEqual(registration.WnsHeaders[foundKey], header.Value);
            }

            foreach (KeyValuePair<string, string> header in DetectedHeaders)
            {
                string foundKey = registration.WnsHeaders.Keys.Single(s => s.ToLower() == header.Key.ToLower());
                Assert.IsNotNull(foundKey);
                Assert.AreEqual(registration.WnsHeaders[foundKey], header.Value);
            }

            Assert.AreEqual(registration.WnsHeaders.Count, DefaultHeaders.Count + DetectedHeaders.Count);

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
            // TODO: Uncomment when .Net Runtime implements installationID
            //Assert.IsTrue(registration.Tags.Contains(zumoInstallationId));
            Assert.AreEqual(registration.Tags.Count, DefaultTags.Length + 1);
        }
    }
}
