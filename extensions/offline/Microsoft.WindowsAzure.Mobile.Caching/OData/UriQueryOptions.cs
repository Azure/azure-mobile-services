using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    /// <summary>
    /// This defines a composite OData query options that can be used to perform query composition.
    /// Currently this only supports $filter, $orderby, $top, $skip, and $inlinecount.
    /// </summary>
    public class UriQueryOptions : IQueryOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UriQueryOptions"/> class based on the incoming request.
        /// </summary>
        /// <param name="requestUri">The request message.</param>
        public UriQueryOptions(Uri requestUri)
        {
            if (requestUri == null)
            {
                throw new ArgumentNullException("requestUri");
            }

            RequestUri = requestUri;

            // Parse the query from request Uri
            RawValues = new RawQueryOptions();
            IEnumerable<KeyValuePair<string, string>> queryParameters = requestUri.GetQueryNameValuePairs();
            foreach (KeyValuePair<string, string> kvp in queryParameters)
            {
                switch (kvp.Key)
                {
                    case "$filter":
                        RawValues.Filter = kvp.Value;
                        ThrowIfEmpty(kvp.Value, "$filter");
                        Filter = new FilterQuery(kvp.Value);
                        break;
                    case "$orderby":
                        RawValues.OrderBy = kvp.Value;
                        ThrowIfEmpty(kvp.Value, "$orderby");
                        OrderBy = new OrderByQuery(kvp.Value);
                        break;
                    case "$top":
                        RawValues.Top = kvp.Value;
                        ThrowIfEmpty(kvp.Value, "$top");
                        Top = new TopQuery(kvp.Value);
                        break;
                    case "$skip":
                        RawValues.Skip = kvp.Value;
                        ThrowIfEmpty(kvp.Value, "$skip");
                        Skip = new SkipQuery(kvp.Value);
                        break;
                    case "$inlinecount":
                        RawValues.InlineCount = kvp.Value;
                        ThrowIfEmpty(kvp.Value, "$inlinecount");
                        InlineCount = new InlineCountQuery(kvp.Value);
                        break;                    
                    default:
                        if (kvp.Key.StartsWith("$", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new NotSupportedException("Custom query options starting with $ are not supported.");
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the request message associated with this instance.
        /// </summary>
        public Uri RequestUri { get; private set; }

        /// <summary>
        /// Gets the raw string of all the OData query options
        /// </summary>
        public RawQueryOptions RawValues { get; private set; }

        /// <summary>
        /// Gets the <see cref="FilterQueryOption"/>.
        /// </summary>
        public IQueryOption Filter { get; private set; }

        /// <summary>
        /// Gets the <see cref="OrderByQueryOption"/>.
        /// </summary>
        public IQueryOption OrderBy { get; private set; }

        /// <summary>
        /// Gets the <see cref="SkipQueryOption"/>.
        /// </summary>
        public IQueryOption Skip { get; private set; }

        /// <summary>
        /// Gets the <see cref="TopQueryOption"/>.
        /// </summary>
        public IQueryOption Top { get; private set; }

        /// <summary>
        /// Gets the <see cref="InlineCountQueryOption"/>.
        /// </summary>
        public IQueryOption InlineCount { get; private set; }

        /// <summary>
        /// Check if the given query option is an OData system query option.
        /// </summary>
        /// <param name="queryOptionName">The name of the query option.</param>
        /// <returns>Returns <c>true</c> if the query option is an OData system query option.</returns>
        public static bool IsSystemQueryOption(string queryOptionName)
        {
            return queryOptionName == "$orderby" ||
                 queryOptionName == "$filter" ||
                 queryOptionName == "$top" ||
                 queryOptionName == "$skip" ||
                 queryOptionName == "$inlinecount";
        }

        private static void ThrowIfEmpty(string queryValue, string queryName)
        {
            if (String.IsNullOrWhiteSpace(queryValue))
            {
                throw new ArgumentException(string.Format("{0} query name cannot be empty", queryName));
            }
        }
    }
}
