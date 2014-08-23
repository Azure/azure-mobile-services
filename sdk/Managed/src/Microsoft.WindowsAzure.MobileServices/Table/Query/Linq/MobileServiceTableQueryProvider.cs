// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Query
{
    internal class MobileServiceTableQueryProvider
    {


        /// <summary>
        /// Feature which are sent as telemetry information to the service for all
        /// outgoing calls.
        /// </summary>
        internal MobileServiceFeatures Features { get; set; }

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
        internal async Task<IEnumerable<T>> Execute<T>(IMobileServiceTableQuery<T> query)
        {
            // Compile the query from the underlying IQueryable's expression
            // tree
            MobileServiceTableQueryDescription compiledQuery = this.Compile(query);

            // Send the query
            string odata = compiledQuery.ToODataString();
            QueryResult result = await this.Execute<T>(query, odata);

            return new QueryResultEnumerable<T>(
                result.TotalCount,
                result.NextLink,
                query.Table.MobileServiceClient.Serializer.Deserialize(result.Values, compiledQuery.ProjectionArgumentType).Select(
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

        protected virtual async Task<QueryResult> Execute<T>(IMobileServiceTableQuery<T> query, string odata)
        {
            var table = query.Table as MobileServiceTable;
            if (table != null)
            {
                // Add telemetry information if possible.
                return await table.ReadAsync(odata, query.Parameters, this.Features | MobileServiceFeatures.TypedTable);
            }
            else
            {
                JToken result = await query.Table.ReadAsync(odata, query.Parameters);
                return QueryResult.Parse(result, null, validate: true);
            }
        }

        /// <summary>
        /// Compile the query into a MobileServiceTableQueryDescription.
        /// </summary>
        /// <returns>
        /// The compiled OData query.
        /// </returns>
        internal MobileServiceTableQueryDescription Compile<T>(IMobileServiceTableQuery<T> query)
        {
            // Compile the query from the underlying IQueryable's expression
            // tree
            MobileServiceTableQueryTranslator<T> translator = new MobileServiceTableQueryTranslator<T>(query);
            MobileServiceTableQueryDescription compiledQuery = translator.Translate();

            return compiledQuery;
        }

        internal string ToODataString<T>(IMobileServiceTableQuery<T> query)
        {
            MobileServiceTableQueryDescription description = this.Compile(query);
            return description.ToODataString();
        }
    }
}
