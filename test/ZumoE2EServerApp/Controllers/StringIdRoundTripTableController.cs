// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using ZumoE2EServerApp.DataObjects;
using ZumoE2EServerApp.Models;

namespace ZumoE2EServerApp.Tables
{
    public class StringIdRoundTripTableController : TableController<StringIdRoundTripTableItem>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            SDKClientTestContext context = new SDKClientTestContext(Services.Settings.Name);
            this.DomainManager = new EntityDomainManager<StringIdRoundTripTableItem>(context, Request, Services);
        }

        public IEnumerable<StringIdRoundTripTableItem> GetAllRoundTrips()
        {
            return Query().ToArray();
        }

        public  HttpResponseMessage GetRoundTrip(string id)
        {
            var item = Lookup(id).Queryable.FirstOrDefault();
            if (item != null)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, item);
            }
            else
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        public Task<StringIdRoundTripTableItem> PatchRoundTrip(string id, Delta<StringIdRoundTripTableItem> patch)
        {
            patch.FixShiftedProps(p => p.ComplexType, p => p.ComplexTypeS)
                .FixShiftedProps(p => p.Complex, p => p.ComplexS);

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
