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
    [AuthorizeLevel(AuthorizationLevel.Application)]
    public class ApplicationApiController : ApiController
    {
        [Route("api/application")]
        public Task<HttpResponseMessage> Get()
        {
            return CustomSharedApi.handleRequest(this.Request, (ServiceUser)this.User);
        }

        [Route("api/application")]
        public Task<HttpResponseMessage> Post()
        {
            return CustomSharedApi.handleRequest(this.Request, (ServiceUser)this.User);
        }

        [Route("api/application")]
        public Task<HttpResponseMessage> Put()
        {
            return CustomSharedApi.handleRequest(this.Request, (ServiceUser)this.User);
        }

        [Route("api/application")]
        public Task<HttpResponseMessage> Delete()
        {
            return CustomSharedApi.handleRequest(this.Request, (ServiceUser)this.User);
        }

        [Route("api/application")]
        public Task<HttpResponseMessage> Patch()
        {
            return CustomSharedApi.handleRequest(this.Request, (ServiceUser)this.User);
        }
    }
}
