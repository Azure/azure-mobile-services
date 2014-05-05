// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal class PushUtility : IPushUtility
    {
        /// <summary>
        /// A singleton instance of the <see cref="PushUtility"/>.
        /// </summary>
        private static readonly IPushUtility instance = new PushUtility();

        /// <summary>
        /// A singleton instance of the <see cref="PushUtility"/>.
        /// </summary>
        public static IPushUtility Instance
        {
            get
            {
                return instance;
            }
        }

        public Registration GetNewNativeRegistration()
        {
            return new MpnsRegistration();
        }

        public Registration GetNewNativeRegistration(string deviceId, IEnumerable<string> tags)
        {
            return new MpnsRegistration(deviceId, tags);
        }

        public Registration GetNewTemplateRegistration()
        {
            return new MpnsTemplateRegistration();
        }

        public Registration GetNewTemplateRegistration(string deviceId, string bodyTemplate, string templateName)
        {
            return new MpnsTemplateRegistration(deviceId, bodyTemplate, templateName);
        }

        public string GetPlatform()
        {
            return "mpns";
        }
    }
}
