// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Windows.Data.Json;
using Windows.Storage;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides context regarding the application that is using the Mobile Service.
    /// </summary>
    internal static class MobileServiceApplication
    {
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
        /// The ID used to identify this installation of the
        /// application to provide telemetry data.  It will either be retrieved
        /// from local settings or generated fresh.
        /// </summary>
        private static string installationId = null;

        /// <summary>
        /// Gets the ID used to identify this installation of the
        /// application to provide telemetry data.  It will either be retrieved
        /// from local settings or generated fresh.
        /// </summary>
        public static string InstallationId
        {
            get
            {
                if (installationId == null)
                {
                    // Try to get the AppInstallationId from settings
                    object setting = null;
                    if (ApplicationData.Current.LocalSettings.Values.TryGetValue(ConfigureAsyncInstallationConfigPath, out setting))
                    {
                        JsonValue config = null;
                        if (JsonValue.TryParse(setting as string, out config))
                        {
                            installationId = config.Get(ConfigureAsyncApplicationIdKey).AsString();
                        }
                    }

                    // Generate a new AppInstallationId if we failed to find one
                    if (installationId == null)
                    {
                        installationId = Guid.NewGuid().ToString();
                        string configText =
                            new JsonObject()
                            .Set(ConfigureAsyncApplicationIdKey, installationId)
                            .Stringify();
                        ApplicationData.Current.LocalSettings.Values[ConfigureAsyncInstallationConfigPath] = configText;
                    }
                }

                return installationId;
            }
        }
    }
}
