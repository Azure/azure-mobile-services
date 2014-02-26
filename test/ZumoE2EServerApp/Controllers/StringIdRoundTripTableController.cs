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
    public class StringIdRoundTripTableController : TableController<StringIdRoundTripTableItem>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            SDKClientTestContext context = new SDKClientTestContext(Services.Settings.Name);
            this.DomainManager = new StringIdRoundTripDomainManager(context, Request, Services);
        }

        public IQueryable<StringIdRoundTripTableItem> GetAllRoundTrips()
        {
            return Query();
        }

        public SingleResult<StringIdRoundTripTableItem> GetRoundTrip(string id)
        {
            return Lookup(id);
        }

        public Task<StringIdRoundTripTableItem> PatchRoundTrip(string id, Delta<StringIdRoundTripTableItem> patch)
        {
            return UpdateAsync(id, patch);
        }

        public Task<StringIdRoundTripTableItem> PostRoundTrip(StringIdRoundTripTableItem item)
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
