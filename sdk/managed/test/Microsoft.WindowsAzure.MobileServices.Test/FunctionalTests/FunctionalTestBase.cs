// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    /// <summary>
    /// Base class for functional tests.
    /// </summary>
    [FunctionalTest]
    public class FunctionalTestBase : TestBase
    {
        /// <summary>
        /// Get a client pointed at the test server without request logging.
        /// </summary>
        /// <returns>The test client.</returns>
        public MobileServiceClient GetClient()
        {
            string runtimeUrl = this.GetTestSetting("MobileServiceRuntimeUrl");
            string runtimeKey = this.GetTestSetting("MobileServiceRuntimeKey");
            return new MobileServiceClient(runtimeUrl, runtimeKey, new LoggingHttpHandler(this));
        }
    }

    class LoggingHttpHandler : DelegatingHandler
    {
        public TestBase Test { get; private set; }

        public LoggingHttpHandler(TestBase test)
        {
            Test = test;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Test.Log("    >>> {0} {1} {2}", request.Method, request.RequestUri, request.Content);
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            Test.Log("    <<< {0} {1} {2}", response.StatusCode, response.ReasonPhrase, response.Content);
            return response;
        }
    }
}
