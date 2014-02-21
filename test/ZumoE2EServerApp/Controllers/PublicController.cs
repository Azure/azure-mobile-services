// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    [RequiresAuthorization(AuthorizationLevel.Anonymous)]
    public class PublicController : TableController<TestUser>
    {
        public static JsonSerializer customSerializer = JsonSerializer.Create(new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore
        });

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            this.DomainManager = new InMemoryDomainManager<TestUser>(true);
        }

        public virtual IQueryable<TestUser> GetAllTestUser()
        {
            return Query();
        }

        public virtual SingleResult<TestUser> GetTestUser(string id)
        {
            return Lookup(id);
        }

        public virtual async Task<IHttpActionResult> PatchTestUser(string id, Delta<TestUser> patch)
        {
            return this.Ok(await UpdateAsync(id, patch));
        }

        public virtual async Task<HttpResponseMessage> PostTestUser(TestUser item)
        {
            var ret = await InsertAsync(item);
            var ret2 = JObject.FromObject(ret, customSerializer);
            return this.Request.CreateResponse(HttpStatusCode.Created, ret2);
        }

        public virtual async Task<HttpResponseMessage> DeleteTestUser(string id)
        {
            await  this.DeleteAsync(id);
            return this.Request.CreateResponse(HttpStatusCode.NoContent);
        }
    }
}