using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    public static class UriExtensions
    {
        public static IEnumerable<KeyValuePair<string, string>> GetQueryNameValuePairs(this Uri reqUri)
        {
            return reqUri.Query.Split(new[] { '&', '?' }, StringSplitOptions.RemoveEmptyEntries).Select(x =>
            {
                int firstIndex = x.IndexOf('=');
                return new KeyValuePair<string, string>(
                    x.Substring(0, firstIndex).Trim(),
                    Uri.UnescapeDataString(x.Substring(firstIndex + 1).Trim()));
            });
        }
    }
}
