// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Query;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides operations on a table for a Mobile Service.
    /// </summary>
    /// <typeparam name="T">
    /// The type of instances in the table (which implies the table).
    /// </typeparam>
    internal class MobileServiceTable<T> : MobileServiceTable, IMobileServiceTable<T>
    {
        private MobileServiceTableQueryProvider queryProvider;
        private bool hasIntegerId;

        /// <summary>
        /// Initializes a new instance of the MobileServiceTables class.
        /// </summary>
        /// <param name="tableName">
        /// The name of the table.
        /// </param>
        /// <param name="client">
        /// The <see cref="MobileServiceClient"/> associated with this table.
        /// </param>
        public MobileServiceTable(string tableName, MobileServiceClient client)
            : base(tableName, client)
        {
            this.queryProvider = new MobileServiceTableQueryProvider();
            this.SystemProperties = client.Serializer.GetSystemProperties(typeof(T));
            Type idType = client.Serializer.GetIdPropertyType<T>(throwIfNotFound: false);
            this.hasIntegerId = idType == null || MobileServiceSerializer.IsIntegerId(idType);
        }

        /// <summary>
        /// Returns instances from a table.
        /// </summary>
        /// <returns>
        /// Instances from the table.
        /// </returns>
        /// <remarks>
        /// This call will not handle paging, etc., for you.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "It does not appear nested when used via the async pattern.")]
        public Task<IEnumerable<T>> ReadAsync()
        {
            return ReadAsync(CreateQuery());
        }

        /// <summary>
        /// Returns instances from a table based on a query.
        /// </summary>
        /// <typeparam name="U">
        /// The type of instance returned by the query.
        /// </typeparam>
        /// <param name="query">
        /// The query to execute.
        /// </param>
        /// <returns>
        /// Instances from the table.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Generic are not nested when used via async.")]
        public Task<IEnumerable<U>> ReadAsync<U>(IMobileServiceTableQuery<U> query)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            return query.ToEnumerableAsync();
        }

        /// <summary>
        /// Executes a query against the table.
        /// </summary>
        /// <param name="query">
        /// A query to execute.
        /// </param>
        /// <returns>
        /// A task that will return with results when the query finishes.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Generic are not nested when used via async.")]
        public async Task<IEnumerable<U>> ReadAsync<U>(string query)
        {
            QueryResult result = await base.ReadAsync(query, null, MobileServiceFeatures.TypedTable | MobileServiceFeatures.ReadWithLinkHeader);

            return new QueryResultEnumerable<U>(
                result.TotalCount,
                result.NextLink,
                this.MobileServiceClient.Serializer.Deserialize(result.Values, typeof(U)).Cast<U>());
        }

        /// <summary>
        /// Inserts a new instance into the table.
        /// </summary>
        /// <param name="instance">
        /// The instance to insert.
        /// </param>
        /// <returns>
        /// A task that will complete when the insertion has finished.
        /// </returns>
        public async Task InsertAsync(T instance)
        {
            await this.InsertAsync(instance, null);
        }

        /// <summary>
        /// Inserts a new instance into the table.
        /// </summary>
        /// <param name="instance">
        /// The instance to insert.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in 
        /// the request URI query string.
        /// </param>
        /// <returns>
        /// A task that will complete when the insertion has finished.
        /// </returns>
        public async Task InsertAsync(T instance, IDictionary<string, string> parameters)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            MobileServiceSerializer serializer = this.MobileServiceClient.Serializer;
            JObject value = serializer.Serialize(instance) as JObject;
            if (!this.hasIntegerId)
            {
                string unused;
                value = MobileServiceSerializer.RemoveSystemProperties(value, out unused);
            }

            JToken insertedValue = await TransformHttpException(serializer, () => this.InsertAsync(value, parameters, MobileServiceFeatures.TypedTable));

            serializer.Deserialize<T>(insertedValue, instance);
        }

        /// <summary>
        /// Updates an instance in the table.
        /// </summary>
        /// <param name="instance">
        /// The instance to update.
        /// </param>
        /// <returns>
        /// A task that will complete when the update has finished.
        /// </returns>
        public async Task UpdateAsync(T instance)
        {
            await this.UpdateAsync(instance, null);
        }

        /// <summary>
        /// Updates an instance in the table.
        /// </summary>
        /// <param name="instance">
        /// The instance to update.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in 
        /// the request URI query string.
        /// </param>
        /// <returns>
        /// A task that will complete when the update has finished.
        /// </returns>
        public async Task UpdateAsync(T instance, IDictionary<string, string> parameters)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            MobileServiceSerializer serializer = this.MobileServiceClient.Serializer;
            JObject value = serializer.Serialize(instance) as JObject;

            JToken updatedValue = await TransformHttpException(serializer, () => this.UpdateAsync(value, parameters, MobileServiceFeatures.TypedTable));


            serializer.Deserialize<T>(updatedValue, instance);
        }

        /// <summary>
        /// Deletes an instance from the table.
        /// </summary>
        /// <param name="instance">
        /// The instance to delete.
        /// </param>
        /// <returns>
        /// A task that will complete when the delete has finished.
        /// </returns>
        public async Task DeleteAsync(T instance)
        {
            await this.DeleteAsync(instance, null);
        }

        /// <summary>
        /// Deletes an instance from the table.
        /// </summary>
        /// <param name="instance">
        /// The instance to delete.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in 
        /// the request URI query string.
        /// </param>
        /// <returns>
        /// A task that will complete when the delete has finished.
        /// </returns>
        public async Task DeleteAsync(T instance, IDictionary<string, string> parameters)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            MobileServiceSerializer serializer = this.MobileServiceClient.Serializer;
            JObject value = serializer.Serialize(instance) as JObject;

            await this.TransformHttpException(serializer, () => this.DeleteAsync(value, parameters, MobileServiceFeatures.TypedTable));

            // Clear the instance id since it's no longer associated with that
            // id on the server (note that reflection is goodly enough to turn
            // null into the correct value for us).
            serializer.SetIdToDefault(instance);
        }

        /// <summary>
        /// Get an instance from the table by its id.
        /// </summary>
        /// <param name="id">
        /// The id of the instance.
        /// </param>
        /// <returns>
        /// The desired instance.
        /// </returns>
        public new async Task<T> LookupAsync(object id)
        {
            return await this.LookupAsync(id, null);
        }

        /// <summary>
        /// Get an instance from the table by its id.
        /// </summary>
        /// <param name="id">
        /// The id of the instance.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in 
        /// the request URI query string.
        /// </param>
        /// <returns>
        /// The desired instance.
        /// </returns>
        public new async Task<T> LookupAsync(object id, IDictionary<string, string> parameters)
        {
            // Ensure that the id passed in is assignable to the Id property of T
            this.MobileServiceClient.Serializer.EnsureValidIdForType<T>(id);
            JToken value = await base.LookupAsync(id, parameters, MobileServiceFeatures.TypedTable);
            return this.MobileServiceClient.Serializer.Deserialize<T>(value);
        }

        /// <summary>
        /// Refresh the current instance with the latest values from the
        /// table.
        /// </summary>
        /// <param name="instance">
        /// The instance to refresh.
        /// </param>
        /// <returns>
        /// A task that will complete when the refresh has finished.
        /// </returns>
        public async Task RefreshAsync(T instance)
        {
            await this.RefreshAsync(instance, null);
        }

        /// <summary>
        /// Refresh the current instance with the latest values from the
        /// table.
        /// </summary>
        /// <param name="instance">
        /// The instance to refresh.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in 
        /// the request URI query string.
        /// </param>
        /// <returns>
        /// A task that will complete when the refresh has finished.
        /// </returns>
        public async Task RefreshAsync(T instance, IDictionary<string, string> parameters)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            MobileServiceSerializer serializer = this.MobileServiceClient.Serializer;
            object id = serializer.GetId(instance, allowDefault: true);

            if (MobileServiceSerializer.IsDefaultId(id))
            {
                return;
            }
            if (id is string)
            {
                MobileServiceSerializer.EnsureValidStringId(id, allowDefault: true);
            }

            // Get the latest version of this element
            JObject refreshed = await this.GetSingleValueAsync(id, parameters);

            // Deserialize that value back into the current instance
            serializer.Deserialize<T>(refreshed, instance);
        }

        /// <summary>
        /// Creates a query for the current table.
        /// </summary>
        /// <returns>
        /// A query against the table.
        /// </returns>
        public IMobileServiceTableQuery<T> CreateQuery()
        {
            return this.queryProvider.Create(this, new T[0].AsQueryable(), new Dictionary<string, string>(), false);
        }

        /// <summary>
        /// Creates a query by applying the specified filter predicate.
        /// </summary>
        /// <param name="predicate">
        /// The filter predicate.
        /// </param>
        /// <returns>
        /// A query against the table.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        public IMobileServiceTableQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            return CreateQuery().Where(predicate);
        }

        /// <summary>
        /// Creates a query by applying the specified selection.
        /// </summary>
        /// <typeparam name="U">
        /// Type representing the projected result of the query.
        /// </typeparam>
        /// <param name="selector">
        /// The selector function.
        /// </param>
        /// <returns>
        /// A query against the table.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "U", Justification = "Standard for LINQ")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        public IMobileServiceTableQuery<U> Select<U>(Expression<Func<T, U>> selector)
        {
            return CreateQuery().Select(selector);
        }

        /// <summary>
        /// Creates a query by applying the specified ascending order clause.
        /// </summary>
        /// <typeparam name="TKey">
        /// The type of the member being ordered by.
        /// </typeparam>
        /// <param name="keySelector">
        /// The expression selecting the member to order by.
        /// </param>
        /// <returns>
        /// A query against the table.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        public IMobileServiceTableQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return CreateQuery().OrderBy(keySelector);
        }

        /// <summary>
        /// Creates a query by applying the specified descending order clause.
        /// </summary>
        /// <typeparam name="TKey">
        /// The type of the member being ordered by.
        /// </typeparam>
        /// <param name="keySelector">
        /// The expression selecting the member to order by.
        /// </param>
        /// <returns>
        /// A query against the table.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        public IMobileServiceTableQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return CreateQuery().OrderByDescending(keySelector);
        }

        /// <summary>
        /// Creates a query by applying the specified ascending order clause.
        /// </summary>
        /// <typeparam name="TKey">
        /// The type of the member being ordered by.
        /// </typeparam>
        /// <param name="keySelector">
        /// The expression selecting the member to order by.
        /// </param>
        /// <returns>
        /// A query against the table.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        public IMobileServiceTableQuery<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return CreateQuery().ThenBy(keySelector);
        }

        /// <summary>
        /// Creates a query by applying the specified descending order clause.
        /// </summary>
        /// <typeparam name="TKey">
        /// The type of the member being ordered by.
        /// </typeparam>
        /// <param name="keySelector">
        /// The expression selecting the member to order by.
        /// </param>
        /// <returns>
        /// A query against the table.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        public IMobileServiceTableQuery<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return CreateQuery().ThenByDescending(keySelector);
        }

        /// <summary>
        /// Creates a query by applying the specified skip clause.
        /// </summary>
        /// <param name="count">
        /// The number to skip.
        /// </param>
        /// <returns>
        /// A query against the table.
        /// </returns>
        public IMobileServiceTableQuery<T> Skip(int count)
        {
            return CreateQuery().Skip(count);
        }

        /// <summary>
        /// Creates a query by applying the specified take clause.
        /// </summary>
        /// <param name="count">
        /// The number to take.
        /// </param>
        /// <returns>
        /// A query against the table.
        /// </returns>
        public IMobileServiceTableQuery<T> Take(int count)
        {
            return CreateQuery().Take(count);
        }

        /// <summary>
        /// Creates a query that will ensure it gets the total count for all
        /// the records that would have been returned ignoring any take paging/
        /// limit clause specified by client or server.
        /// </summary>
        /// <returns>
        /// A query against the table.
        /// </returns>
        public IMobileServiceTableQuery<T> IncludeTotalCount()
        {
            return this.CreateQuery().IncludeTotalCount();
        }

        /// <summary>
        /// Applies to the source query the specified string key-value 
        /// pairs to be used as user-defined parameters with the request URI 
        /// query string.
        /// </summary>
        /// <param name="parameters">
        /// The parameters to apply.
        /// </param>
        /// <returns>
        /// The composed query.
        /// </returns>
        public IMobileServiceTableQuery<T> WithParameters(IDictionary<string, string> parameters)
        {
            return this.CreateQuery().WithParameters(parameters);
        }

        /// <summary>
        /// Gets the instances of the table asynchronously.
        /// </summary>
        /// <returns>
        /// Instances from the table.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Not nested when used via async pattern.")]
        public Task<IEnumerable<T>> ToEnumerableAsync()
        {
            return this.ReadAsync();
        }

        /// <summary>
        /// Gets the instances of the table asynchronously and returns the
        /// results in a new List.
        /// </summary>
        /// <returns>
        /// Instances from the table as a List.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Not nested when used via async pattern.")]
        public async Task<List<T>> ToListAsync()
        {
            return new QueryResultList<T>(await this.ReadAsync());
        }

        /// <summary>
        /// Executes a request and transforms a 412 and 409 response to respective exception type.
        /// </summary>
        private async Task<JToken> TransformHttpException(MobileServiceSerializer serializer, Func<Task<JToken>> action)
        {
            try
            {
                return await action();
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                if (ex.Response.StatusCode != HttpStatusCode.PreconditionFailed &&
                    ex.Response.StatusCode != HttpStatusCode.Conflict)
                {
                    throw;
                }

                T item = default(T);
                try
                {
                    item = serializer.Deserialize<T>(ex.Value);
                }
                catch { }

                if (ex.Response.StatusCode == HttpStatusCode.PreconditionFailed)
                {
                    throw new MobileServicePreconditionFailedException<T>(ex, item);
                }
                else if (ex.Response.StatusCode == HttpStatusCode.Conflict)
                {
                    throw new MobileServiceConflictException<T>(ex, item);
                }
                throw;
            }
        }

        /// <summary>
        /// Get an element from a table by its id.
        /// </summary>
        /// <param name="id">
        /// The id of the element.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in
        /// the request URI query string.
        /// </param>
        /// <returns>
        /// The desired element as JSON object.
        /// </returns>
        private async Task<JObject> GetSingleValueAsync(object id, IDictionary<string, string> parameters)
        {
            Debug.Assert(id != null);

            // Create a query for just this item
            string query = string.Format(
                CultureInfo.InvariantCulture,
                "$filter=({0} eq {1})",
                MobileServiceSystemColumns.Id,
                FilterBuildingExpressionVisitor.ToODataConstant(id));

            // Send the query
            QueryResult response = await this.ReadAsync(query, parameters, MobileServiceFeatures.TypedTable);

            return GetSingleValue(response);
        }

        private static JObject GetSingleValue(QueryResult response)
        {
            // Get the first element in the response
            JObject jobject = response.Values.FirstOrDefault() as JObject;

            if (jobject == null)
            {
                string responseStr = response != null ? response.ToString() : "null";
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.MobileServiceTable_NotSingleObject,
                        responseStr));
            }

            return jobject;
        }
    }
}
