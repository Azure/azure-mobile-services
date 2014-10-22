// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
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
        HttpResponseMessage nullResponse;
        int responseIndex = 0;

        public TestHttpHandler()
        {            
            this.Requests = new List<HttpRequestMessage>();
            this.Responses = new List<HttpResponseMessage>();
            this.RequestContents = new List<string>();

            this.nullResponse = CreateResponse(String.Empty);
        }

        public HttpRequestMessage Request
        {
            get { return this.Requests.Count == 0 ? null : this.Requests[this.Requests.Count - 1]; }
            set
            {
                this.Requests.Clear();
                this.Requests.Add(value);
            }
        }

        public List<HttpRequestMessage> Requests { get; set; }
        public List<string> RequestContents { get; set; }

        public HttpResponseMessage Response
        {
            get { return this.Responses.Count == 0 ? null : this.Responses[this.Responses.Count - 1]; }
            set
            {
                this.responseIndex = 0;
                this.Responses.Clear();
                this.Responses.Add(value);
            }
        }

        public List<HttpResponseMessage> Responses { get; set; }        

        public Func<HttpRequestMessage, Task<HttpRequestMessage>> OnSendingRequest { get; set; }

        public void SetResponseContent(string content)
        {
            this.Response = CreateResponse(content); 
        }

        public void AddResponseContent(string content)
        {
            this.Responses.Add(CreateResponse(content));
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string content = request.Content == null ? null : await request.Content.ReadAsStringAsync();
            this.RequestContents.Add(content);

            if (this.OnSendingRequest != null)
            {
                this.Requests.Add(await this.OnSendingRequest(request));                
            }
            else
            {
                this.Requests.Add(request);                
            }
            
            if (responseIndex < this.Responses.Count)
            {
                return Responses[responseIndex++];
            }

            return nullResponse;
        }

        public static HttpResponseMessage CreateResponse(string content, HttpStatusCode code = HttpStatusCode.OK)
        {
            return new HttpResponseMessage(code)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };
        }

    }
}
