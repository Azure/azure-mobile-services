using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OfflinePerfCore.Common;

namespace OfflinePerfCore.Setup
{
    class SimpleTypeTableMockHandler : DelegatingHandler
    {
        public SimpleTypeTableMockHandler()
        {
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage result;
            if (request.Method == HttpMethod.Get && request.RequestUri.PathAndQuery.StartsWith("/tables/simpletype", StringComparison.OrdinalIgnoreCase))
            {
                var rndGen = new Random();
                var queryParams = request.RequestUri.Query.TrimStart('?').Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                int? totalCount = null;
                int numberOfItems = 50;
                const string TopParam = "$top=";
                foreach (var queryParam in queryParams)
                {
                    if (queryParam.Equals("$inlinecount=allpages", StringComparison.OrdinalIgnoreCase))
                    {
                        totalCount = 1000000;
                    }
                    else if (queryParam.StartsWith(TopParam))
                    {
                        numberOfItems = int.Parse(queryParam.Substring(TopParam.Length));
                    }
                }

                Stream responseStream = new SimpleTypeStream(rndGen, numberOfItems, totalCount);
                result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new StreamContent(responseStream);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }
            else
            {
                result = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("{\"error\":\"Only GET operations for table 'SimpleType' are supported\"}", Encoding.UTF8, "application/json")
                };
            }

            return Task.FromResult(result);
        }
    }
}
