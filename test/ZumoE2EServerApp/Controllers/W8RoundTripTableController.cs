// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using ZumoE2EServerApp.DataObjects;
using ZumoE2EServerApp.Models;
using ZumoE2EServerApp.Utils;

namespace ZumoE2EServerApp.Controllers
{
    public class W8RoundTripTableController : TableController<RoundTripTableItemFakeStringId>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            var context = new SDKClientTestContext(Services.Settings.Name);
            this.DomainManager = new W8RoundTripDomainManager(context, Request, Services);
        }

        public IQueryable<RoundTripTableItemFakeStringId> GetAllRoundTrips()
        {
            return Query();
        }

//        public SingleResult<RoundTripTableItemFakeStringId> GetRoundTrip(int id)
        public RoundTripTableItemFakeStringId GetRoundTrip(int id)
        {
//            return Lookup(id.ToString());
            return Lookup(id.ToString()).Queryable.FirstOrDefault();
        }

        public Task<RoundTripTableItemFakeStringId> PatchRoundTrip(int id, Delta<RoundTripTableItemFakeStringId> patch)
        {
            return UpdateAsync(id.ToString(), patch);
        }

        public Task<RoundTripTableItemFakeStringId> PostRoundTrip(RoundTripTableItemFakeStringId item)
        {
            return InsertAsync(item);
        }

        public Task DeleteRoundTrip(int id)
        {
            return DeleteAsync(id.ToString());
        }
    }
}