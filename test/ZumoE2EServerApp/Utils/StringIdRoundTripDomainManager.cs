// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service;
using System.Net.Http;
using ZumoE2EServerApp.DataObjects;
using ZumoE2EServerApp.Models;

namespace ZumoE2EServerApp.Utils
{
    internal class StringIdRoundTripDomainManager : ComplexTypeDomainManager<StringIdRoundTripTableItem, StringIdRoundTripTableItemForDB, string>
    {
        public StringIdRoundTripDomainManager(SDKClientTestContext context, HttpRequestMessage request, ApiServices services)
            : base(context, request, services)
        {
        }
    }
}