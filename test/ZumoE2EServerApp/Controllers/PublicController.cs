// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using ZumoE2EServerApp.DataObjects;
using ZumoE2EServerApp.Utils;

namespace ZumoE2EServerApp.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    public class PublicController : TableController<TestUser>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            this.DomainManager = new InMemoryDomainManager<TestUser>();
        }

        public virtual Task<IQueryable<TestUser>> GetAllTestUser()
        {
            return Task.FromResult(Query());
        }

        public virtual Task<SingleResult<TestUser>> GetTestUser(string id)
        {
            return Task.FromResult(Lookup(id));
        }

        public virtual async Task<HttpResponseMessage> PatchTestUser(string id, Delta<TestUser> patch)
        {
            return this.Request.CreateResponse(HttpStatusCode.OK, await UpdateAsync(id, patch));
        }

        public virtual async Task<HttpResponseMessage> PostTestUser(TestUser item)
        {
            return this.Request.CreateResponse(HttpStatusCode.Created, await InsertAsync(item));
        }

        public virtual async Task<HttpResponseMessage> DeleteTestUser(string id)
        {
            await this.DeleteAsync(id);
            return this.Request.CreateResponse(HttpStatusCode.NoContent);
        }
    }
}