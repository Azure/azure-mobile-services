// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.WindowsAzure.MobileServices
{
    class PushUtility : IPushUtility
    {
        /// <summary>
        /// A singleton instance of the <see cref="ExpressionUtility"/>.
        /// </summary>
        private static readonly IPushUtility instance = new PushUtility();

        /// <summary>
        /// A singleton instance of the <see cref="ExpressionUtility"/>.
        /// </summary>
        public static IPushUtility Instance
        {
            get
            {
                return instance;
            }
        }

        public RegistrationBase GetNewNativeRegistration()
        {
            return new Registration();
        }

        public RegistrationBase GetNewNativeRegistration(string deviceId, IEnumerable<string> tags)
        {
            return new Registration(deviceId, tags);
        }

        public RegistrationBase GetNewTemplateRegistration()
        {
            return new TemplateRegistration();
        }

        public RegistrationBase GetNewTemplateRegistration(string deviceId, string bodyTemplate, string templateName)
        {
            return new TemplateRegistration(deviceId, bodyTemplate, templateName);
        }

        public string GetPlatform()
        {
            return "mpns";
        }
    }
}
