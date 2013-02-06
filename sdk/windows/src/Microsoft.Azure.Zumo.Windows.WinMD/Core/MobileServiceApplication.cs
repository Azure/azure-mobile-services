// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Globalization;
using Windows.ApplicationModel;
using Windows.Data.Json;
using Windows.Storage;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides context regarding the application that is using the Mobile Service.
    /// </summary>
    internal class MobileServiceApplication
    {
        /// <summary>
        /// The text used when a value is unknown.
        /// </summary>
        private const string UnknownValue = "--";

        /// <summary>
        /// The text used to identify the operating System as Windows 8
        /// </summary>
        private const string Windows8OperatingSystemName = "Windows 8";

        /// <summary>
        /// The text used to identify the SDK as Zumo
        /// </summary>
        private const string ZumoSdkName = "ZUMO";

        /// <summary>
        /// The text used to identify the SDK language as Managed
        /// </summary>
        private const string ZumoSdkManaged = "Managed";

        /// <summary>
        /// Name of the config setting that stores the installation ID.
        /// </summary>
        private const string ConfigureAsyncInstallationConfigPath = "MobileServices.Installation.config";

        /// <summary>
        /// Name of the JSON member in the config setting that stores the
        /// installation ID.
        /// </summary>
        private const string ConfigureAsyncApplicationIdKey = "applicationInstallationId";

        /// <summary>
        /// A singleton instance of the MobileServiceApplication
        /// </summary>
        private static readonly MobileServiceApplication current = new MobileServiceApplication();

        /// <summary>
        /// Creates a new instance of the MobileServiceApplication.
        /// </summary>
        private MobileServiceApplication()
        {
            // Try to get the AppInstallationId from settings
            object setting = null;
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(ConfigureAsyncInstallationConfigPath, out setting))
            {
                JsonValue config = null;
                if (JsonValue.TryParse(setting as string, out config))
                {
                    this.InstallationId = config.Get(ConfigureAsyncApplicationIdKey).AsString();
                }
            }

            // Generate a new AppInstallationId if we failed to find one
            if (this.InstallationId == null)
            {
                this.InstallationId = Guid.NewGuid().ToString();
                string configText =
                    new JsonObject()
                    .Set(ConfigureAsyncApplicationIdKey, this.InstallationId)
                    .Stringify();
                ApplicationData.Current.LocalSettings.Values[ConfigureAsyncInstallationConfigPath] = configText;
            }

            // Set the architecture
            this.OperatingSystemArchitecture = Package.Current.Id.Architecture.ToString();

            // Use hardcoded values for the OS and OS version as the Windows Store APIs don't provide the OS and OS version
            this.OperatingSystemName = Windows8OperatingSystemName;
            this.OperatingSystemVersion = UnknownValue; 

            // Set the SDK values; no way to read the SdkVersion so use UnknownValue
            this.SdkName = ZumoSdkName;
            this.SdkVersion = UnknownValue;
            this.SdkLanguage = ZumoSdkManaged;

            this.UserAgentHeaderValue = GetUserAgentHeaderValue();
        }

        /// <summary>
        /// Gets the ID used to identify this installation of the
        /// application to provide telemetry data.  It will either be retrieved
        /// from local settings or generated fresh.
        /// </summary>
        public string InstallationId { get; private set; }

        /// <summary>
        /// The architecture of the device the application is running on.
        /// </summary>
        public string OperatingSystemArchitecture { get; private set; }

        /// <summary>
        /// The name of the operating system the application is running on.
        /// </summary>
        public string OperatingSystemName { get; private set; }

        /// <summary>
        /// The version of the operating system the application is running on.
        /// </summary>
        public string OperatingSystemVersion { get; private set; }

        /// <summary>
        /// The name of this SDK.
        /// </summary>
        public string SdkName { get; private set; }

        /// <summary>
        /// The version of this SDK.
        /// </summary>
        public string SdkVersion { get; private set; }

        /// <summary>
        /// The language of this SDK.
        /// </summary>
        public string SdkLanguage { get; private set; }

        /// <summary>
        /// The HTTP User-Agent string value that provides the SDK and operating system 
        /// information about the client.
        /// </summary>
        public string UserAgentHeaderValue { get; private set; }

        /// <summary>
        /// The current instance of the MobileServiceApplication for the application.
        /// </summary>
        public static MobileServiceApplication Current
        {
            get
            {
                return current;
            }
        }

        private string GetVersionString(PackageVersion packageVersion)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}.{1}.{2}.{3}",
                packageVersion.Major,
                packageVersion.Minor,
                packageVersion.Build,
                packageVersion.Revision);
        }

        private string GetUserAgentHeaderValue()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}/{1}; (lang={2}; os={3}; os_version={4}; arch={5})",
                this.SdkName,
                this.SdkVersion,
                this.SdkLanguage,
                this.OperatingSystemName,
                this.OperatingSystemVersion,
                this.OperatingSystemArchitecture);
        }
    }
}
