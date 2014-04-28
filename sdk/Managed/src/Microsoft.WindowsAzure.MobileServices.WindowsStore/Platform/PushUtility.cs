// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------


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

        public RegistrationBase GetNewTemplateRegistration()
        {
            return new TemplateRegistration();
        }

        public string GetPlatform()
        {
            return "wns";
        }
    }
}
