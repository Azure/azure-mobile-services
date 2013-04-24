using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    public class CacheHandler : DelegatingHandler
    {
        private readonly ICacheProvider cache;

        private static readonly AsyncLock m_lock = new AsyncLock();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cache"></param>
        public CacheHandler(ICacheProvider cache)
        {
            this.cache = cache;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            HttpContent newContent = null;

            //closure capturing the actual base SendAsync
            OptionalFunc<Uri, HttpContent, HttpMethod, Task<HttpContent>> sendAsync = async (uri, content, method) =>
            {
                //dispose old responses
                if (response != null)
                {
                    response.Dispose();
                }

                uri = uri ?? request.RequestUri;
                method = method ?? request.Method;
                HttpRequestMessage req = new HttpRequestMessage(method, uri);
                req.Content = content;
                foreach (var header in request.Headers)
                {
                    req.Headers.Add(header.Key, header.Value);
                }
                req.Version = request.Version;
                foreach (var property in request.Properties)
                {
                    req.Properties.Add(property.Key, property.Value);
                }

                response = await base.SendAsync(req, cancellationToken);
                // Throw errors for any failing responses
                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    throw new Exception();
                }

                HttpContent responseContent = response.Content;

                // cleanup the request
                req.Dispose();                

                return responseContent;
            };

            using (await m_lock.LockAsync())
            {
                switch (request.Method.Method)
                {
                    case "GET":
                        newContent = await cache.Read(request.RequestUri, sendAsync);
                        break;

                    case "POST":
                        newContent = await cache.Insert(request.RequestUri, request.Content, sendAsync);
                        break;

                    case "PATCH":
                        newContent = await cache.Update(request.RequestUri, request.Content, sendAsync);
                        break;

                    case "DELETE":
                        newContent = await cache.Delete(request.RequestUri, sendAsync);
                        break;
                    default:
                        newContent = request.Content;
                        break;
                }
            }

            if (response == null)
            {
                response = new HttpResponseMessage(HttpStatusCode.OK);
            }
            response.Content = newContent;

            return response;
        }
    }
}
