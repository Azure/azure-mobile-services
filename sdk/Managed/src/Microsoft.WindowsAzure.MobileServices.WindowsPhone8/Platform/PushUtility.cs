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

        public Registration GetNewTemplateRegistration()
        {
            return new MpnsTemplateRegistration();
        }

        public string GetPlatform()
        {
            return "mpns";
        }
    }
}
