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

        public static bool ShowAlerts = true;
        public const string LogsLocationFile = "done.txt";

        public static string NHWp8RawTemplate = String.Format("<?xml version=\"1.0\" encoding=\"utf-8\"?><wp:Notification xmlns:wp=\"WPNotification\"><wp:Toast><wp:Text1>$(News_{0})</wp:Text1></wp:Toast></wp:Notification>", "French");
        public static string NHWp8ToastTemplate = String.Format("<?xml version=\"1.0\" encoding=\"utf-8\"?><wp:Notification xmlns:wp=\"WPNotification\"><wp:Toast><wp:Text1>$(News_{0})</wp:Text1></wp:Toast></wp:Notification>", "English");
        public static string NHWp8TileTemplate = @"<?xml version=""1.0"" encoding=""utf-8""?>
                                                    <wp:Notification xmlns:wp=""WPNotification"">
                                                       <wp:Tile>
                                                          <wp:BackgroundImage>http://zumotestserver.azurewebsites.net/content/zumo2.png</wp:BackgroundImage>
                                                          <wp:Count>count</wp:Count>
                                                          <wp:Title>$(News_Mandarin)</wp:Title>
                                                       </wp:Tile>
                                                    </wp:Notification>";

        public static string NHToastTemplateName = "newsToastTemplate";
        public static string NHTileTemplateName = "newsTileTemplate";
        public static string NHBadgeTemplateName = "newsBadgeTemplate";
        public static string NHRawTemplateName = "newsRawTemplate";
        public static JObject TemplateNotification = new JObject()
        {
            {"News_English", "World News in English!"},
            {"News_French", "Nouvelles du monde en français!"},
            {"News_Mandarin", "在普通话的世界新闻！"},
            {"News_Badge", "10"}
        };

        public static class RuntimeFeatureNames
        {
            public static string AAD_LOGIN = "azureActiveDictionaryLogin";
            public static string SSO_LOGIN = "singleSignOnLogin";
            public static string LIVE_LOGIN = "liveSDKLogin";
            public static string INT_ID_TABLES = "intIdTables";
            public static string STRING_ID_TABLES = "stringIdTables";
            public static string NET_RUNTIME_ENABLED = "netRuntimeEnabled";
            public static string NH_PUSH_ENABLED = "nhPushEnabled";
        }

        public static Dictionary<string, object> RuntimeFeatures = new Dictionary<string, object>();
        public static bool NHPushEnabled = false;
        public static bool NetRuntimeEnabled = false;

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

        public async Task InitializeClient(string appUrl, string appKey)
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
            }
        }

        public async static Task InitializeFeaturesEnabled()
        {
            var client = ZumoTestGlobals.Instance.Client;
            if (client != null)
            {
                try
                {
                    var response = await client.InvokeApiAsync<Dictionary<string, object>>("runtimeInfo", HttpMethod.Get, null);
                    RuntimeFeatures = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(response["features"]));
                    NHPushEnabled = Convert.ToBoolean(RuntimeFeatures[RuntimeFeatureNames.NH_PUSH_ENABLED]);
                    if (response["runtime"].ToString().Contains("node.js"))
                    {
                        NetRuntimeEnabled = false;
                    }
                    else
                    {
                        NetRuntimeEnabled = true;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }
    }
}
