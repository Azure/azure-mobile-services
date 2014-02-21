// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service.Security;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData;
using ZumoE2EServerApp.DataObjects;

namespace ZumoE2EServerApp.Controllers
{
    [RequiresAuthorization(AuthorizationLevel.Application)]
    public class ApplicationController : PublicController
    {
        public override IQueryable<TestUser> GetAllTestUser()
        {
            return base.GetAllTestUser();
        }

        public override SingleResult<TestUser> GetTestUser(string id)
        {
            return base.GetTestUser(id);
        }

        public override Task<IHttpActionResult> PatchTestUser(string id, Delta<TestUser> patch)
        {
            return base.PatchTestUser(id, patch);
        }

        public override Task<HttpResponseMessage> PostTestUser(TestUser item)
        {
            return base.PostTestUser(item);
        }

        public override Task<HttpResponseMessage> DeleteTestUser(string id)
        {
            return base.DeleteTestUser(id);
        }
    }
}