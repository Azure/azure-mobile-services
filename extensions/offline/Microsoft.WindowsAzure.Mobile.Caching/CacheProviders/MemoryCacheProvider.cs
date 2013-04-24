using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    /// <summary>
    /// Cache provider which caches request in memory for the current session.
    /// </summary>
    public class MemoryCacheProvider : BaseCacheProvider
    {
        private Dictionary<Uri, Tuple<DateTime,HttpContent>> memCache;
        private TimeSpan expirationTime;

        /// <summary>
        /// Creates a new instance of <see cref="MemoryCacheProvider"/>.
        /// </summary>
        /// <param name="expirationTime">A <see cref="TimeSpan"/> indicating the time a response should be cached.</param>
        public MemoryCacheProvider(TimeSpan expirationTime)
        {
            this.expirationTime = expirationTime;

            this.memCache = new Dictionary<Uri, Tuple<DateTime, HttpContent>>();
        }

        /// <summary>
        /// Cache reads in a dictionary.
        /// </summary>
        /// <param name="requestUri">The Uri of the request.</param>
        /// <param name="getResponse">A function to actually call the server.</param>
        /// <returns>A response possibly cached.</returns>
        public override async Task<HttpContent> Read(Uri requestUri, OptionalFunc<Uri, HttpContent, HttpMethod, Task<HttpContent>> getResponse)
        {
            Tuple<DateTime, HttpContent> cachedValue;
            //look for existing item and check if not expired
            if (memCache.TryGetValue(requestUri, out cachedValue))
            {
                if (cachedValue.Item1 + this.expirationTime < DateTime.Now)
                {
                    return cachedValue.Item2;
                }
            }

            //return and cache a fresh response
            HttpContent content = await base.Read(requestUri, getResponse);
            memCache[requestUri] = new Tuple<DateTime, HttpContent>(DateTime.Now, content);
            return content;
        }
    }
}
