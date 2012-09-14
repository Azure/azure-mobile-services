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
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides operations on tables for a Mobile Service.
    /// </summary>
    /// <typeparam name="T">
    /// The type of elements in the table (which implies the table).
    /// </typeparam>
    internal partial class MobileServiceTable<T> : MobileServiceTable, IMobileServiceTable<T>
    {
        /// <summary>
        /// Initializes a new instance of the MobileServiceTables class.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="client">
        /// Reference to the MobileServiceClient associated with this table.
        /// </param>
        public MobileServiceTable(string tableName, MobileServiceClient client)
            : base(tableName, client)
        {
        }

        /// <summary>
        /// Get the elements of a table based using a query.
        /// </summary>
        /// <typeparam name="U">
        /// The type of element returned by the query.
        /// </typeparam>
        /// <param name="query">The query to execute.</param>
        /// <returns>Elements of the table.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Generic are not nested when used via async.")]
        public Task<IEnumerable<U>> ReadAsync<U>(MobileServiceTableQuery<U> query)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            return EvaluateQueryAsync<U>(this, query.Compile());
        }

        /// <summary>
        /// Evaluate a query and return its results.
        /// </summary>
        /// <typeparam name="U">
        /// The type of element returned by the query.
        /// </typeparam>
        /// <param name="table">
        /// Access to MobileServices table operations.
        /// </param>
        /// <param name="query">
        /// The description of the query to evaluate and get the results for.
        /// </param>
        /// <returns>Results of the query.</returns>
        internal static async Task<IEnumerable<U>> EvaluateQueryAsync<U>(IMobileServiceTable<T> table, MobileServiceTableQueryDescription query)
        {
            Debug.Assert(query != null, "query cannot be null!");
            Debug.Assert(table != null, "table cannot be null!");

            // Send the query
            string odata = query.ToString();
            IJsonValue response = await table.ReadAsync(odata);

            // Parse the results
            long totalCount;
            JsonArray values = MobileServiceTable<T>.GetResponseSequence(response, out totalCount);
            return new TotalCountEnumerable<U>(
                totalCount,
                values.Select(
                    item =>
                    {
                        // Create and fill a new instance of the type we should
                        // deserialize (which is either T or the input param
                        // to the projection that will modify T).
                        object obj = Activator.CreateInstance(query.ProjectionArgumentType ?? typeof(U));
                        MobileServiceTableSerializer.Deserialize(item, obj);

                        // Apply the projection to the instance transforming it
                        // as desired
                        if (query.Projection != null)
                        {
                            obj = query.Projection.DynamicInvoke(obj);
                        }

                        return (U)obj;
                    }));
        }

        /// <summary>
        /// Get the elements in a table.
        /// </summary>
        /// <returns>The elements in the table.</returns>
        /// <remarks>
        /// This call will not handle paging, etc., for you.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "It does not appear nested when used via the async pattern.")]
        public async Task<IEnumerable<T>> ReadAsync()
        {
            // Need to declare an empty string so the compiler can distinguish
            // between a null query and a null string
            string odata = null;
            IJsonValue response = await this.ReadAsync(odata);

            // Get the response as an array
            long totalCount;
            JsonArray values = GetResponseSequence(response, out totalCount);

            // Deserialize the values and associate the query context
            return new TotalCountEnumerable<T>(
                totalCount,
                values.Select(MobileServiceTableSerializer.Deserialize<T>));
        }

        /// <summary>
        /// Parse a JSON response into a sequence of elements and also return
        /// the count of objects.  This method abstracts out the differences
        /// between a raw array response and an inline count response.
        /// </summary>
        /// <param name="response">The JSON response.</param>
        /// <param name="totalCount">
        /// The total count as requested via the IncludeTotalCount method.
        /// </param>
        /// <returns>The response as a JSON array.</returns>
        internal static JsonArray GetResponseSequence(IJsonValue response, out long totalCount)
        {
            double? inlineCount = null;

            // Try and get the values as an array
            JsonArray values = response.AsArray();
            if (values == null)
            {
                // Otherwise try and get the values from the results property
                // (which is the case when we retrieve the count inline)
                values = response.Get(InlineCountResultsKey).AsArray();
                inlineCount = response.Get(InlineCountCountKey).AsNumber();
                if (values == null)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.MobileServiceTables_GetResponseSequence_ExpectedArray,
                            (response ?? JsonExtensions.Null()).Stringify()));
                }
            }

            // Get the count via the inline count or default an unspecified
            // count to -1
            totalCount = inlineCount != null ?
                (long)inlineCount.Value :
                -1L;

            return values;
        }

        /// <summary>
        /// Get an element from a table by its ID.
        /// </summary>
        /// <param name="id">The ID of the element.</param>
        /// <returns>The desired element.</returns>
        public async Task<T> LookupAsync(object id)
        {
            // TODO: At some point in the future this will be involved in our
            // caching story and relationships across tables via foreign
            // keys.

            IJsonValue value = await this.SendLookupAsync(id);
            return MobileServiceTableSerializer.Deserialize<T>(value.AsObject());
        }

        /// <summary>
        /// Refresh the current instance with the latest values from the
        /// table.
        /// </summary>
        /// <param name="instance">The instance to refresh.</param>
        /// <returns>
        /// A task that will complete when the refresh has finished.
        /// </returns>
        public async Task RefreshAsync(T instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            // Only refresh if it's already on the server
            SerializableType type = SerializableType.Get(typeof(T));
            object id = type.IdMember.GetValue(instance);
            if (!SerializableType.IsDefaultIdValue(id))
            {
                // Get the latest version of this element
                JsonObject obj = await this.GetSingleValueAsync(id);

                // Deserialize that value back into the current instance
                MobileServiceTableSerializer.Deserialize(obj, instance);
            }
        }

        /// <summary>
        /// Get an element from a table by its ID.
        /// </summary>
        /// <param name="id">The ID of the element.</param>
        /// <returns>The desired element as JSON object.</returns>
        private async Task<JsonObject> GetSingleValueAsync(object id)
        {
            // Create a query for just this item
            string query = string.Format(
                CultureInfo.InvariantCulture,
                "$filter={0} eq {1}",
                IdPropertyName,
                TypeExtensions.ToODataConstant(id));

            // Send the query
            IJsonValue response = await this.ReadAsync(query);

            // Get the first element in the response
            JsonObject obj = response.AsObject();
            if (obj == null)
            {
                JsonArray array = response.AsArray();
                if (array != null && array.Count > 0)
                {
                    obj = array.FirstOrDefault().AsObject();
                }
            }

            if (obj == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.MobileServiceTables_GetSingleValueAsync_NotSingleObject,
                        (response ?? JsonExtensions.Null()).Stringify()));
            }

            return obj;
        }

        /// <summary>
        /// Insert a new instance into the table.
        /// </summary>
        /// <param name="instance">The instance to insert.</param>
        /// <returns>
        /// A task that will complete when the insertion has finished.
        /// </returns>
        public async Task InsertAsync(T instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            // Serialize the instance
            JsonObject value = MobileServiceTableSerializer.Serialize(instance).AsObject();

            // Send the request
            IJsonValue response = await this.InsertAsync(value);

            // Deserialize the response back into the instance in case any
            // server scripts changed values of the instance.
            MobileServiceTableSerializer.Deserialize(response, instance);
        }

        /// <summary>
        /// Updates an instance in the table.
        /// </summary>
        /// <param name="instance">The instance to update.</param>
        /// <returns>
        /// A task that will complete when the update has finished.
        /// </returns>
        public async Task UpdateAsync(T instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            // Serialize the instance
            JsonObject value = MobileServiceTableSerializer.Serialize(instance).AsObject();

            // Send the request
            IJsonValue response = await this.UpdateAsync(value);

            // Deserialize the response back into the instance in case any
            // server scripts changed values of the instance.
            MobileServiceTableSerializer.Deserialize(response, instance);
        }

        /// <summary>
        /// Delete an instance from the table.
        /// </summary>
        /// <param name="instance">The instance to delete.</param>
        /// <returns>
        /// A task that will complete when the delete has finished.
        /// </returns>
        public async Task DeleteAsync(T instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            // Serialize the instance
            JsonObject value = MobileServiceTableSerializer.Serialize(instance).AsObject();

            // Send the request
            await this.DeleteAsync(value);

            // Clear the instance ID since it's no longer associated with that
            // ID on the server (note that reflection is goodly enough to turn
            // null into the correct value for us).
            SerializableType type = SerializableType.Get(typeof(T));
            type.IdMember.SetValue(instance, null);
        }

        /// <summary>
        /// Creates a query by applying the specified filter predicate.
        /// </summary>
        /// <param name="predicate">The filter predicate.</param>
        /// <returns>A query against the table.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        public MobileServiceTableQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            return new MobileServiceTableQuery<T>(this).Where(predicate);
        }

        /// <summary>
        /// Creates a query by applying the specified selection.
        /// </summary>
        /// <typeparam name="U">
        /// Type representing the projected result of the query.
        /// </typeparam>
        /// <param name="selector">The selector function.</param>
        /// <returns>A query against the table.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "U", Justification = "Standard for LINQ")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        public MobileServiceTableQuery<U> Select<U>(Expression<Func<T, U>> selector)
        {
            return new MobileServiceTableQuery<T>(this).Select(selector);
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
        /// <returns>A query against the table.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        public MobileServiceTableQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return new MobileServiceTableQuery<T>(this).OrderBy(keySelector);
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
        /// <returns>A query against the table.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        public MobileServiceTableQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return new MobileServiceTableQuery<T>(this).OrderByDescending(keySelector);
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
        /// <returns>A query against the table.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        public MobileServiceTableQuery<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return new MobileServiceTableQuery<T>(this).ThenBy(keySelector);
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
        /// <returns>A query against the table.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        public MobileServiceTableQuery<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return new MobileServiceTableQuery<T>(this).ThenByDescending(keySelector);
        }

        /// <summary>
        /// Creates a query by applying the specified skip clause.
        /// </summary>
        /// <param name="count">The number to skip.</param>
        /// <returns>A query against the table.</returns>
        public MobileServiceTableQuery<T> Skip(int count)
        {
            return new MobileServiceTableQuery<T>(this).Skip(count);
        }

        /// <summary>
        /// Creates a query by applying the specified take clause.
        /// </summary>
        /// <param name="count">The number to take.</param>
        /// <returns>A query against the table.</returns>
        public MobileServiceTableQuery<T> Take(int count)
        {
            return new MobileServiceTableQuery<T>(this).Take(count);
        }

        /// <summary>
        /// Creates a query that will ensure it gets the total count for all
        /// the records that would have been returned ignoring any take paging/
        /// limit clause specified by client or server.
        /// </summary>
        /// <returns>A query against the table.</returns>
        public MobileServiceTableQuery<T> IncludeTotalCount()
        {
            return new MobileServiceTableQuery<T>(this, true);
        }

        /// <summary>
        /// Gets the elements of the table asynchronously.
        /// </summary>
        /// <returns>The table element results.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Not nested when used via async pattern.")]
        public Task<IEnumerable<T>> ToEnumerableAsync()
        {
            return this.ReadAsync();
        }

        /// <summary>
        /// Gets the elements of the table asynchronously and return the
        /// results in a new List.
        /// </summary>
        /// <returns>The table elements results as a List.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Not nested when used via async pattern.")]
        public async Task<List<T>> ToListAsync()
        {
            return new TotalCountList<T>(await this.ReadAsync());
        }

        /// <summary>
        /// Create a new collection view based on the query.
        /// </summary>
        /// <returns>The collection view.</returns>
        public MobileServiceCollectionView<T> ToCollectionView()
        {
            return new MobileServiceTableQuery<T>(this).ToCollectionView();
        }
    }
}
