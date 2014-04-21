using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Query
{
    /// <summary>
    /// Represents the result of odata query returned from Mobile Service
    /// </summary>
    internal class QueryResult
    {
        /// <summary>
        /// The name of the results key in an inline count response object.
        /// </summary>
        private const string InlineCountResultsKey = "results";

        /// <summary>
        /// The name of the count key in an inline count response object.
        /// </summary>
        private const string InlineCountCountKey = "count";

        /// <summary>
        /// Count of total rows that match the query without skip and top
        /// </summary>
        public long TotalCount { get; private set; }

        /// <summary>
        /// Items in query result 
        /// </summary>
        public JArray Values { get; private set; }

        /// <summary>
        /// Parse a JSON response into <see cref="QueryResult"/> object 
        /// that contains sequence of elements and the count of objects.  
        /// This method abstracts out the differences between a raw array response and 
        /// an inline count response.
        /// </summary>
        /// <param name="response">
        /// The JSON response.
        /// </param>
        public static QueryResult Parse(JToken response)
        {
            Debug.Assert(response != null);

            var result = new QueryResult();

            long? inlineCount = null;

            // Try and get the values as an array
            result.Values = response as JArray;
            if (result.Values == null)
            {
                // Otherwise try and get the values from the results property
                // (which is the case when we retrieve the count inline)
                result.Values = response[InlineCountResultsKey] as JArray;
                inlineCount = (long)response[InlineCountCountKey];
                if (result.Values == null)
                {
                    string responseStr = response != null ? response.ToString() : "null";
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                                                                      Resources.MobileServiceTable_ExpectedArray,
                                                                      responseStr));
                }
            }

            // Get the count via the inline count or default an unspecified count to -1
            result.TotalCount = inlineCount != null ? inlineCount.Value : -1L;

            return result;
        }
    }
}
