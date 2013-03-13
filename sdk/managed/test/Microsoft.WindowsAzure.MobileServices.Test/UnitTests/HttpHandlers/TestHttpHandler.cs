// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    /// <summary>
    /// ServiceFilter that allows a test to control the HTTP pipeline and
    /// analyze a request and provide a set response.
    /// </summary>
    public class TestHttpHandler : DelegatingHandler
    {
        public TestHttpHandler()
        {
            Response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };
        }

        public HttpRequestMessage Request { get; set; }
        public HttpResponseMessage Response { get; set; }

        public void SetResponseContent(string content)
        {
            this.Response.Content = new StringContent(content, Encoding.UTF8, "application/json");
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            this.Request = request;
            TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>();
            tcs.SetResult(this.Response);
            return tcs.Task;
        }
    }
}
