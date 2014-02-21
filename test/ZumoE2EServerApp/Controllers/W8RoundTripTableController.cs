// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public IEnumerable<RoundTripTableItemFakeStringId> GetAllRoundTrips()
        {
            return Query().ToArray();
        }

        public RoundTripTableItemFakeStringId GetRoundTrip(int id)
        {
            var r = Lookup(id.ToString()).Queryable.FirstOrDefault();
            return r;
        }

        public Task<RoundTripTableItemFakeStringId> PatchRoundTrip(int id, Delta<RoundTripTableItemFakeStringId> patch)
        {
            patch.FixShiftedProps(p => p.ComplexType1, p => p.ComplexType1S).
                FixShiftedProps(p => p.ComplexType2, p => p.ComplexType2S);
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