// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData;
using ZumoE2EServerApp.DataObjects;
using ZumoE2EServerApp.Models;

namespace ZumoE2EServerApp.Utils
{
    public class W8RoundTripDomainManager : MappedEntityDomainManager<RoundTripTableItemFakeStringId, RoundTripTableItem>
    {
        public W8RoundTripDomainManager(SDKClientTestContext context, HttpRequestMessage request, ApiServices services)
            : base(context, request, services)
        {
        }

        public override SingleResult<RoundTripTableItemFakeStringId> Lookup(string id)
        {
            int intId = GetKey<int>(id);
            return LookupEntity(p => p.RoundTripTableItemId == intId);
        }

        public override Task<RoundTripTableItemFakeStringId> UpdateAsync(string id, Delta<RoundTripTableItemFakeStringId> patch)
        {
            int intId = GetKey<int>(id);
            return this.UpdateEntityAsync(patch, intId);
        }

        public override Task<bool> DeleteAsync(string id)
        {
            int intId = GetKey<int>(id);
            return this.DeleteItemAsync(intId);
        }
    }
}