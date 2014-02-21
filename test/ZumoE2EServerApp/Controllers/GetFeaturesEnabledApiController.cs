// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using System.Linq;
using System.Web.Http;

namespace ZumoE2EServerApp.Controllers
{
    public class GetFeaturesEnabledApiController : ApiController
    {
        public ApiServices Services { get; set; }

        [RequiresAuthorization(AuthorizationLevel.Anonymous)]
        [Route("api/getfeaturesenabled")]
        public Features GetFeatures()
        {
            var features = new string[] { "nhPushEnabled", "netRuntimeEnabled" };
            Services.Log.Info("sdfjgl;ksdfjg");
            return new Features()
            {
                Message = string.Join(",", features),
                Settings = Services.Settings.Select(x => x.Key.ToString() + " " + x.Value.ToString()).ToArray(),
                Properties = Services.Properties.Select(x => x.Key.ToString() + " " + x.Value.ToString()).ToArray(),
                Connections = Services.Settings.Connections.Select(x => x.Key.ToString() +
                    " Name:" + x.Value.Name +
                    " Provider:" + x.Value.Provider +
                    " ConnectionString:" + x.Value.ConnectionString).ToArray(),
            };
        }

        public class Features
        {
            public string Message { get; set; }
            public string[] Settings { get; set; }
            public string[] Properties { get; set; }
            public string[] Connections { get; set; }
        }
    }
}