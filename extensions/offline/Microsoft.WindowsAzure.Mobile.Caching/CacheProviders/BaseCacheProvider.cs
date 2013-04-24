using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    public abstract class BaseCacheProvider : ICacheProvider
    {
        public virtual Task<HttpContent> Read(Uri requestUri, OptionalFunc<Uri, HttpContent, HttpMethod, Task<HttpContent>> getResponse)
        {
            return getResponse(requestUri);
        }

        public virtual Task<HttpContent> Insert(Uri requestUri, HttpContent content, OptionalFunc<Uri, HttpContent, HttpMethod, Task<HttpContent>> getResponse)
        {
            return getResponse(requestUri, content);
        }

        public virtual Task<HttpContent> Update(Uri requestUri, HttpContent content, OptionalFunc<Uri, HttpContent, HttpMethod, Task<HttpContent>> getResponse)
        {
            return getResponse(requestUri, content);
        }

        public virtual Task<HttpContent> Delete(Uri requestUri, OptionalFunc<Uri, HttpContent, HttpMethod, Task<HttpContent>> getResponse)
        {
            return getResponse(requestUri);
        }

        public virtual bool ProvidesCacheForRequest(Uri requestUri)
        {
            return false;
        }
    }
}
