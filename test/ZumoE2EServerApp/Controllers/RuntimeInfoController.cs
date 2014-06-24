// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using Microsoft.WindowsAzure.Mobile.Service.Config;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Web.Http;
using System.Linq;
using System.Reflection;

namespace ZumoE2EServerApp.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    public class RuntimeInfoController : ApiController
    {
        public ApiServices Services { get; set; }

        [Route("api/runtimeInfo")]
        public JObject GetFeatures()
        {
            string version;
            if (!this.Services.Settings.TryGetValue(ServiceSettingsKeys.SiteExtensionVersion, out version))
            {
                // Get here if the site ext. version is missing. That should never happen when deployed,
                // but is expected whe running locally. If local, try to get the version info 
                // from the dll directly.
                try
                {
                    var afva = typeof(ApiServices).Assembly.CustomAttributes.FirstOrDefault(p => p.AttributeType == typeof(AssemblyFileVersionAttribute));
                    if (afva != null)
                    {
                        version = afva.ConstructorArguments[0].Value as string;
                    }
                }
                catch (Exception)
                {
                    version = "unknown";
                }
            }

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
                    new JProperty("azureActiveDirectoryLogin", false)
                ))
            );
        }
    }
}