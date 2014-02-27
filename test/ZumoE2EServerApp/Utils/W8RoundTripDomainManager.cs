// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service;
using System.Net.Http;
using ZumoE2EServerApp.DataObjects;
using ZumoE2EServerApp.Models;

namespace ZumoE2EServerApp.Utils
{
    internal class W8RoundTripDomainManager : ComplexTypeDomainManager<RoundTripTableItemFakeStringId, RoundTripTableItem, int>
    {
        public W8RoundTripDomainManager(SDKClientTestContext context, HttpRequestMessage request, ApiServices services)
            : base(context, request, services)
        {
        }
    }
}