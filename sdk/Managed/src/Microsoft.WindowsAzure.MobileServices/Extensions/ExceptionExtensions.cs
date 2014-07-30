// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal static class ExceptionExtensions
    {
        public static bool IsNetworkError(this Exception ex)
        {
            return ex is HttpRequestException;
        }

        public static bool IsAuthenticationError(this Exception ex)
        {
            var ioEx = ex as MobileServiceInvalidOperationException;
            bool result = ioEx != null && ioEx.Response != null && ioEx.Response.StatusCode == HttpStatusCode.Unauthorized;
            return result;
        }
    }
}
