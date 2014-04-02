// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace ZumoE2ETestApp.Framework
{
    public class ZumoTestGlobals
    {
        public const string RoundTripTableName = "w8RoundTripTable";
        public const string StringIdRoundTripTableName = "stringIdRoundTripTable";
        public const string MoviesTableName = "intIdMovies";
        public const string StringIdMoviesTableName = "stringIdMovies";
#if !WINDOWS_PHONE
        public const string PushTestTableName = "w8PushTest";
#else
        public const string PushTestTableName = "wp8PushTest";
#endif
        public const string ParamsTestTableName = "ParamsTestTable";

        public const string ClientVersionKeyName = "clientVersion";
        public const string RuntimeVersionKeyName = "x-zumo-version";

        private static ZumoTestGlobals instance = new ZumoTestGlobals();

        private static bool showAlerts = true;
        public static bool ShowAlerts
        {
            get { return showAlerts; }
            set { showAlerts = value; }
        }

        public const string LogsLocationFile = "done.txt";

        public static class RuntimeFeatureNames
        {
            public static string AAD_LOGIN = "azureActiveDirectoryLogin";
            public static string SSO_LOGIN = "singleSignOnLogin";
            public static string LIVE_LOGIN = "liveSDKLogin";
            public static string INT_ID_TABLES = "intIdTables";
            public static string STRING_ID_TABLES = "stringIdTables";
            public static string NH_PUSH_ENABLED = "nhPushEnabled";
        }

        private Dictionary<string, bool> runtimeFeatures;
        public async Task<Dictionary<string, bool>> GetRuntimeFeatures(ZumoTest test)
        {
            if (runtimeFeatures == null)
            {
                RuntimeType = "unknown";
                RuntimeVersion = "unknown";
                IsNetRuntime = false;
                IsNHPushEnabled = false;

                try
                {
                    JToken apiResult = await Client.InvokeApiAsync("runtimeInfo", HttpMethod.Get, null);
                    runtimeFeatures = apiResult["features"].ToObject<Dictionary<string, bool>>();
                    var runtimeInfo = apiResult["runtime"].ToObject<Dictionary<string, string>>();
                    RuntimeType = runtimeInfo["type"];
                    RuntimeVersion = runtimeInfo["version"];

                    IsNetRuntime = RuntimeType.Equals(".NET");
                    IsNHPushEnabled = runtimeFeatures[ZumoTestGlobals.RuntimeFeatureNames.NH_PUSH_ENABLED];
                }
                catch (Exception ex)
                {
                    test.AddLog(ex.Message);
                }

                if (runtimeFeatures.Count > 0)
                {
                    test.AddLog("Runtime: {0}", ZumoTestGlobals.Instance.RuntimeType);
                    test.AddLog("Version: {0}", ZumoTestGlobals.Instance.RuntimeVersion);
                    foreach (var entry in runtimeFeatures)
                    {
                        test.AddLog("Runtime feature: {0} : {1}", entry.Key, entry.Value);
                    }
                }
                else
                {
                    test.AddLog("Could not load the runtime information");
                }
            }

            return runtimeFeatures;
        }

        public string RuntimeType { get; private set; }

        public string RuntimeVersion { get; private set; }
        public bool IsNetRuntime { get; private set; }

        public bool IsNHPushEnabled { get; private set; }

        public MobileServiceClient Client { get; private set; }
        public Dictionary<string, object> GlobalTestParams { get; private set; }

        private ZumoTestGlobals()
        {
            this.GlobalTestParams = new Dictionary<string, object>();
        }

        public static ZumoTestGlobals Instance
        {
            get { return instance; }
        }

        public static void ResetInstance()
        {
            instance = new ZumoTestGlobals();
        }

        public void InitializeClient(string appUrl, string appKey)
        {
            bool needsUpdate = this.Client == null ||
                (this.Client.ApplicationUri.ToString() != appUrl) ||
                (this.Client.ApplicationKey != appKey);

            if (needsUpdate)
            {
                if (string.IsNullOrEmpty(appUrl) || string.IsNullOrEmpty(appKey))
                {
                    throw new ArgumentException("Please enter valid application URL and key.");
                }

                this.Client = new MobileServiceClient(appUrl, appKey);
                this.runtimeFeatures = null;
            }
        }
    }
}
