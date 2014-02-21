// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service.Security;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ZumoE2EServerApp.Utils;

namespace ZumoE2EServerApp.Controllers
{
    [RequiresAuthorization(AuthorizationLevel.Application)]
    public class ApplicationApiController : ApiController
    {
        [Route("api/application")]
        public Task<HttpResponseMessage> Get(JToken body)
        {
            return CustomSharedApi.handleRequest(this.Request, this.User, body);
        }

        [Route("api/application")]
        public Task<HttpResponseMessage> Post(JToken body)
        {
            return CustomSharedApi.handleRequest(this.Request, this.User, body);
        }

        [Route("api/application")]
        public Task<HttpResponseMessage> Put(JToken body)
        {
            return CustomSharedApi.handleRequest(this.Request, this.User, body);
        }

        [Route("api/application")]
        public Task<HttpResponseMessage> Delete(JToken body)
        {
            return CustomSharedApi.handleRequest(this.Request, this.User, body);
        }

        [Route("api/application")]
        public Task<HttpResponseMessage> Patch(JToken body)
        {
            return CustomSharedApi.handleRequest(this.Request, this.User, body);
        }
    }
}
