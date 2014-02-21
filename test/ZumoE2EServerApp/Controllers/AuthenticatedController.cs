// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service.Security;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData;
using ZumoE2EServerApp.DataObjects;

namespace ZumoE2EServerApp.Controllers
{
    [RequiresAuthorization(AuthorizationLevel.User)]
    public class AuthenticatedController : PublicController
    {
        public override IQueryable<TestUser> GetAllTestUser()
        {
            ServiceUser user = (ServiceUser)this.User;
            var all = base.GetAllTestUser().Where(p => p.UserId == user.Id).ToArray();
            var identities = user.Identities.Select(q => q.Claims.First(p => p.Type == "urn:microsoft:credentials").Value).ToArray();
            foreach (var item in all)
            {
                item.Identities = identities;
            }

            return all.AsQueryable();
        }

        public override SingleResult<TestUser> GetTestUser(string id)
        {
            return SingleResult.Create(GetAllTestUser().Where(p => p.Id == id));
        }

        public override async Task<IHttpActionResult> PatchTestUser(string id, Delta<TestUser> patch)
        {
            ServiceUser user = (ServiceUser)this.User;
            var all = base.GetAllTestUser().Where(p => p.UserId == user.Id).ToArray();
            if (all.Length == 0)
            {
                return this.NotFound();
            }
            else if (all[0].UserId != user.Id)
            {
                return this.BadRequest("Mismatching user id");
            }
            else
            {
                return await base.PatchTestUser(id, patch);
            }
        }

        public override Task<HttpResponseMessage> PostTestUser(TestUser item)
        {
            ServiceUser user = (ServiceUser)this.User;
            item.UserId = user.Id;
            return base.PostTestUser(item);
        }

        public override async Task<HttpResponseMessage> DeleteTestUser(string id)
        {
            ServiceUser user = (ServiceUser)this.User;
            var all = base.GetAllTestUser().Where(p => p.UserId == user.Id).ToArray();
            if (all.Length == 0)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }
            else if (all[0].UserId != user.Id)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
            {
                return await base.DeleteTestUser(id);
            }
        }
    }
}