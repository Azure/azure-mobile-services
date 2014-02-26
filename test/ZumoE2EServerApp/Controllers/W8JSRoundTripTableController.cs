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

namespace ZumoE2EServerApp.Tables
{
    public class W8JSRoundTripTableController : TableController<W8JSRoundTripTableItem>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            SDKClientTestContext context = new SDKClientTestContext(Services.Settings.Name);
            this.DomainManager = new W8JSRoundTripDomainManager(context, Request, Services);
        }

        public IQueryable<W8JSRoundTripTableItem> GetAllRoundTrips()
        {
            return Query();
        }

        public SingleResult<W8JSRoundTripTableItem> GetRoundTrip(string id)
        {
            return Lookup(id);
        }

        public Task<W8JSRoundTripTableItem> PatchRoundTrip(string id, Delta<W8JSRoundTripTableItem> patch)
        {
            return UpdateAsync(id, patch);
        }

        public Task<W8JSRoundTripTableItem> PostRoundTrip(W8JSRoundTripTableItem item)
        {
            if (string.IsNullOrEmpty(item.Id))
            {
                item.Id = System.Guid.NewGuid().ToString();
            }

            return InsertAsync(item);
        }

        public Task DeleteRoundTrip(string id)
        {
            return DeleteAsync(id);
        }
    }
}
