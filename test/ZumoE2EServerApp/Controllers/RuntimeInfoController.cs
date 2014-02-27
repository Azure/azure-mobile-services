// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Config;
using Newtonsoft.Json.Linq;
using System.Web.Http;

namespace ZumoE2EServerApp.Controllers
{
    public class RuntimeInfoController : ApiController
    {
        public ApiServices Services { get; set; }

        [Route("api/runtimeInfo")]
        public JObject GetFeatures()
        {
            string version;
            this.Services.Settings.TryGetValue(ServiceSettingsKeys.SiteExtensionVersion, out version);
            return new JObject(
                new JProperty("runtime", new JObject(
                    new JProperty("type", ".NET"),
                    new JProperty("version", version)
                )),
                new JProperty("features", new JObject(
                    new JProperty("intIdTables", false),
                    new JProperty("stringIdTables", true),
                    new JProperty("nhPushEnabled", true),
                    new JProperty("queryExpandSupport", true),
                    new JProperty("usersEnabled", false),
                    new JProperty("liveSDKLogin", false),
                    new JProperty("singleSignOnLogin", false),
                    new JProperty("azureActiveDictionaryLogin", false)
                ))
            );
        }
    }
}