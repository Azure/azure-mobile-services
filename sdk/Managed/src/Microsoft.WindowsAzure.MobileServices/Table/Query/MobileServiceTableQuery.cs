// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Represents a query that can be evaluated against a Mobile Services
    /// table. MobileServiceTableQuery instances can be obtained via
    /// MobileServiceClient.Query(of T)().
    /// </summary>
    /// <remarks>
    /// Rather than implenting IQueryable directly, we've implemented the
    /// portion of the LINQ query pattern we support on MobileServiceTableQuery
    /// objects.  MobileServiceTableQuery instances are used to build up
    /// IQueryables from LINQ query operations.
    /// </remarks>
    internal class MobileServiceTableQuery<T> : IMobileServiceTableQuery<T>
    {
        /// <summary>
        /// Initializes a new instance of the MobileServiceTableQuery class.
        /// </summary>
        /// <param name="table">
        /// The table being queried.
        /// </param>
        /// <param name="queryProvider">
        /// The <see cref="MobileServiceTableQueryProvider"/> associated with this 
        /// <see cref="T:MobileServiceTableQuery`1{T}"/>
        /// </param>
        /// <param name="query">
        /// The encapsulated <see cref="IQueryable"/>.
        /// </param>
        /// <param name="includeTotalCount">
        /// A value that if set will determine whether the query will request
        /// the total count for all the records that would have been returned
        /// ignoring any take paging/limit clause specified by client or
        /// server.
        /// </param>
        /// <param name="parameters">
        /// The optional user-defined query string parameters to include with the query.
        /// </param>
        internal MobileServiceTableQuery(IMobileServiceTable<T> table,
                                         MobileServiceTableQueryProvider queryProvider,
                                         IQueryable<T> query,
                                         IDictionary<string, string> parameters,
                                         bool includeTotalCount)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }

            this.Table = table;
            this.RequestTotalCount = includeTotalCount;
            this.Parameters = parameters;
            this.Query = query;
            this.QueryProvider = queryProvider;
        }

        /// <summary>
        /// Gets the MobileServiceTable being queried.
        /// </summary>
        public IMobileServiceTable<T> Table { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the query will request the total
        /// count for all the records that would have been returned ignoring
        /// any take paging/limit clause specified by client or server.
        /// </summary>
        public bool RequestTotalCount { get; private set; }

        /// <summary>
        /// The user-defined query string parameters to include with the query.
        /// </summary>
        public IDictionary<string, string> Parameters { get; private set; }

        /// <summary>
        /// Gets the underlying IQueryable associated with this query.
        /// </summary>
        public IQueryable<T> Query { get; set; }

        /// <summary>
        /// Gets the associated Query Provider capable of executing a <see cref="T:MobileServiceTableQuery`1{T}"/>.
        /// </summary>
        public MobileServiceTableQueryProvider QueryProvider { get; set; }

        /// <summary>
        /// Applies the specified filter predicate to the source query.
        /// </summary>
        /// <param name="predicate">
        /// The filter predicate.
        /// </param>
        /// <returns>
        /// The composed query.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        public IMobileServiceTableQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }

            return this.QueryProvider.Create<T>(this.Table, Queryable.Where(this.Query, predicate), this.Parameters, this.RequestTotalCount);
        }

        /// <summary>
        /// Applies the specified selection to the source query.
        /// </summary>
        /// <typeparam name="U">
        /// Type representing the projected result of the query.
        /// </typeparam>
        /// <param name="selector">
        /// The selector function.
        /// </param>
        /// <returns>
        /// The composed query.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1715:IdentifiersShouldHaveCorrectPrefix", MessageId = "T", Justification = "Part of the LINQ query pattern.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "U", Justification = "Standard for LINQ")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        public IMobileServiceTableQuery<U> Select<U>(Expression<Func<T, U>> selector)
        {
            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }

            // Create a new table with the same name/client but
            // pretending to be of a different type since the query needs to
            // have the same type as the table.  This won't cause any issues
            // since we're only going to use it to evaluate the query and it'll
            // never leak to users.
            MobileServiceTable<U> table = new MobileServiceTable<U>(
                this.Table.TableName,
                this.Table.MobileServiceClient,
                this.Table.MobileServiceClient.RemoteStorage);

            return this.QueryProvider.Create(table,
                                             Queryable.Select(this.Query, selector),
                                             MobileServiceTable.AddSystemProperties(Table.SystemProperties, this.Parameters),
                                             this.RequestTotalCount);
        }

        /// <summary>
        /// Applies the specified ascending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">
        /// The type of the member being ordered by.
        /// </typeparam>
        /// <param name="keySelector">
        /// The expression selecting the member to order by.
        /// </param>
        /// <returns>
        /// The composed query.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        public IMobileServiceTableQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }

            return this.QueryProvider.Create(this.Table, Queryable.OrderBy(this.Query, keySelector), this.Parameters, this.RequestTotalCount);
        }

        /// <summary>
        /// Applies the specified descending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">
        /// The type of the member being ordered by.
        /// </typeparam>
        /// <param name="keySelector">
        /// The expression selecting the member to order by.
        /// </param>
        /// <returns>
        /// The composed query.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        public IMobileServiceTableQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }

            return this.QueryProvider.Create(this.Table, Queryable.OrderByDescending(this.Query, keySelector), this.Parameters, this.RequestTotalCount);
        }

        /// <summary>
        /// Applies the specified ascending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">
        /// The type of the member being ordered by.
        /// </typeparam>
        /// <param name="keySelector">
        /// The expression selecting the member to order by.
        /// </param>
        /// <returns>
        /// The composed query.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        public IMobileServiceTableQuery<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }

            return this.QueryProvider.Create(this.Table, Queryable.ThenBy((IOrderedQueryable<T>)this.Query, keySelector), this.Parameters, this.RequestTotalCount);
        }

        /// <summary>
        /// Applies the specified descending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">
        /// The type of the member being ordered by.
        /// </typeparam>
        /// <param name="keySelector">
        /// The expression selecting the member to order by.
        /// </param>
        /// <returns>
        /// The composed query.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        public IMobileServiceTableQuery<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }

            return this.QueryProvider.Create(this.Table, Queryable.ThenByDescending((IOrderedQueryable<T>)this.Query, keySelector), this.Parameters, this.RequestTotalCount);
        }

        /// <summary>
        /// Applies the specified skip clause to the source query.
        /// </summary>
        /// <param name="count">
        /// The number to skip.
        /// </param>
        /// <returns>
        /// The composed query.
        /// </returns>
        public IMobileServiceTableQuery<T> Skip(int count)
        {
            return this.QueryProvider.Create(this.Table, Queryable.Skip(this.Query, count), this.Parameters, this.RequestTotalCount);
        }

        /// <summary>
        /// Applies the specified take clause to the source query.
        /// </summary>
        /// <param name="count">
        /// The number to take.
        /// </param>
        /// <returns>
        /// The composed query.
        /// </returns>
        public IMobileServiceTableQuery<T> Take(int count)
        {
            return this.QueryProvider.Create(this.Table, Queryable.Take(this.Query, count), this.Parameters, this.RequestTotalCount);
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
            if (parameters != null)
            {
                // Make sure to replace any existing value for the key
                foreach (KeyValuePair<string, string> pair in parameters)
                {
                    this.Parameters[pair.Key] = pair.Value;
                }
            }

            return this.QueryProvider.Create(this.Table, this.Query, this.Parameters, this.RequestTotalCount);
        }

        /// <summary>
        /// Ensure the query will get the total count for all the records that
        /// would have been returned ignoring any take paging/limit clause
        /// specified by client or server.
        /// </summary>
        /// <returns>
        /// The query object.
        /// </returns>
        public IMobileServiceTableQuery<T> IncludeTotalCount()
        {
            return this.QueryProvider.Create(this.Table, this.Query, this.Parameters, true);
        }

        /// <summary>
        /// Evalute the query asynchronously and return the results.
        /// </summary>
        /// <returns>
        /// The evaluated query results.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Not nested when used via async pattern.")]
        public Task<IEnumerable<T>> ToEnumerableAsync()
        {
            return this.QueryProvider.Execute(this);
        }

        /// <summary>
        /// Evalute the query asynchronously and return the results in a new
        /// List.
        /// </summary>
        /// <returns>
        /// The evaluated query results as a List.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Not nested when used via async pattern.")]
        public async Task<List<T>> ToListAsync()
        {
            IEnumerable<T> items = await this.QueryProvider.Execute(this);
            return new TotalCountList<T>(items);
        }
    }
}
