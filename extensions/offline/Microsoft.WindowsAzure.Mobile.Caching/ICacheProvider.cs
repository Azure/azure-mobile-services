using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    public interface ICacheProvider
    {
        bool ProvidesCacheForRequest(Uri requestUri);

        Task<HttpContent> Read(Uri requestUri, OptionalFunc<Uri, HttpContent, HttpMethod, Task<HttpContent>> getResponse);

        Task<HttpContent> Insert(Uri requestUri, HttpContent content, OptionalFunc<Uri, HttpContent, HttpMethod, Task<HttpContent>> getResponse);

        Task<HttpContent> Update(Uri requestUri, HttpContent content, OptionalFunc<Uri, HttpContent, HttpMethod, Task<HttpContent>> getResponse);

        Task<HttpContent> Delete(Uri requestUri, OptionalFunc<Uri, HttpContent, HttpMethod, Task<HttpContent>> getResponse);
    }
}
