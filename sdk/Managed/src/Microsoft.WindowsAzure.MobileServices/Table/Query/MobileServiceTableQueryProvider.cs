// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal class MobileServiceTableQueryProvider
    {
        /// <summary>
        /// The name of the results key in an inline count response object.
        /// </summary>
        protected const string InlineCountResultsKey = "results";
        
        /// <summary>
        /// The name of the count key in an inline count response object.
        /// </summary>
        protected const string InlineCountCountKey = "count";

        /// <summary>
        /// Create a new query based off a table and and a new
        /// queryable. This is used via MobileServiceTableQueryable's
        /// combinators to construct new queries from simpler base queries.
        /// </summary>
        /// <param name="table">
        /// The table.
        /// </param>
        /// <param name="query">
        /// The new queryable.
        /// </param>
        /// <param name="parameters">
        /// The optional user-defined query string parameters to include with the query.
        /// </param>
        /// <param name="includeTotalCount">
        /// A value that if set will determine whether the query will request
        /// the total count for all the records that would have been returned
        /// ignoring any take paging/limit clause specified by client or
        /// server.  If this value is not set, we'll use the baseQuery's
        /// RequestTotalProperty instead (this is specifically so that our
        /// IncludeTotalCount method will preserve state on the old query).
        /// </param>
        /// <returns>
        /// The new query.
        /// </returns>
        internal IMobileServiceTableQuery<T> Create<T>(IMobileServiceTable<T> table,
                                                        IQueryable<T> query,
                                                        IDictionary<string, string> parameters,
                                                        bool includeTotalCount)
        {
            Debug.Assert(table != null, "table cannot be null!");
            Debug.Assert(query != null, "query cannot be null!");
            Debug.Assert(parameters != null, "parameters cannot be null!");

            // NOTE: Make sure any changes to this logic are reflected in the
            // Select method below which has its own version of this code to
            // work around type changes for its projection.
            return new MobileServiceTableQuery<T>(
                table,
                this,
                query,
                parameters,
                includeTotalCount);
        }

        /// <summary>
        /// Execute a query and return its results.
        /// </summary>
        /// <typeparam name="T">
        /// The type of element returned by the query.
        /// </typeparam>
        /// <param name="query">
        /// The query to evaluate and get the results for.
        /// </param>
        /// <returns>
        /// Results of the query.
        /// </returns>
        internal async Task<IEnumerable<T>> Execute<T>(MobileServiceTableQuery<T> query)
        {
            // Compile the query from the underlying IQueryable's expression
            // tree
            MobileServiceTableQueryDescription compiledQuery = this.Compile(query);

            // Send the query
            string odata = compiledQuery.ToQueryString();
            JToken response = await query.Table.ReadAsync(odata, query.Parameters);

            // Parse the results
            long totalCount;
            JArray values = this.GetResponseSequence(response, out totalCount);

            return new TotalCountEnumerable<T>(
                totalCount,
                query.Table.MobileServiceClient.Serializer.Deserialize(values, compiledQuery.ProjectionArgumentType).Select(
                    value =>
                    {
                        // Apply the projection to the instance transforming it
                        // as desired
                        foreach (Delegate projection in compiledQuery.Projections)
                        {
                            value = projection.DynamicInvoke(value);
                        }

                        return (T)value;
                    }));
        }

        /// <summary>
        /// Compile the query into a MobileServiceTableQueryDescription.
        /// </summary>
        /// <returns>
        /// The compiled OData query.
        /// </returns>
        internal MobileServiceTableQueryDescription Compile<T>(MobileServiceTableQuery<T> query)
        {
            // Compile the query from the underlying IQueryable's expression
            // tree
            MobileServiceTableQueryTranslator<T> translator = new MobileServiceTableQueryTranslator<T>(query);
            MobileServiceTableQueryDescription compiledQuery = translator.Translate();

            return compiledQuery;
        }

        /// <summary>
        /// Parse a JSON response into a sequence of elements and also return
        /// the count of objects.  This method abstracts out the differences
        /// between a raw array response and an inline count response.
        /// </summary>
        /// <param name="response">
        /// The JSON response.
        /// </param>
        /// <param name="totalCount">
        /// The total count as requested via the IncludeTotalCount method.
        /// If the response does not include totalcount, it is set to -1.
        /// </param>
        /// <returns>
        /// The response as a JSON array.
        /// </returns>
        internal JArray GetResponseSequence(JToken response, out long totalCount)
        {
            Debug.Assert(response != null);

            long? inlineCount = null;

            // Try and get the values as an array
            JArray values = response as JArray;
            if (values == null)
            {
                // Otherwise try and get the values from the results property
                // (which is the case when we retrieve the count inline)
                values = response[InlineCountResultsKey] as JArray;
                inlineCount = (long)response[InlineCountCountKey];
                if (values == null)
                {
                    string responseStr = response != null ? response.ToString() : "null";
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.MobileServiceTable_ExpectedArray,
                            responseStr));
                }
            }

            // Get the count via the inline count or default an unspecified
            // count to -1
            totalCount = inlineCount != null ?
                inlineCount.Value :
                -1L;

            return values;
        }
    }
}
