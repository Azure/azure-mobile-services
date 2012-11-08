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
    /// table.  MobileServiceTableQuery instances can be obtained via
    /// MobileServiceClient.Query(of T)().
    /// </summary>
    /// <remarks>
    /// Rather than implenting IQueryable directly, we've implemented the
    /// portion of the LINQ query pattern we support on MobileServiceTableQuery
    /// objects.  MobileServiceTableQuery instances are used to build up
    /// IQueryables from LINQ query operations.
    /// </remarks>
    public sealed class MobileServiceTableQuery<T>
    {
        /// <summary>
        /// Initializes a new instance of the MobileServiceTableQuery class.
        /// </summary>
        /// <param name="table">
        /// The table being queried.
        /// </param>
        /// <param name="includeTotalCount">
        /// A value that if set will determine whether the query will request
        /// the total count for all the records that would have been returned
        /// ignoring any take paging/limit clause specified by client or
        /// server.
        /// </param>
        internal MobileServiceTableQuery(MobileServiceTable<T> table, bool includeTotalCount = false)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }

            this.Table = table;
            this.RequestTotalCount = includeTotalCount;

            // Create a default queryable to serve as the root
            this.Query = new T[0].AsQueryable();
        }

        /// <summary>
        /// Gets the MobileServiceTable being queried.
        /// </summary>
        internal MobileServiceTable<T> Table { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the query will request the total
        /// count for all the records that would have been returned ignoring
        /// any take paging/limit clause specified by client or server.
        /// </summary>
        internal bool RequestTotalCount { get; private set; }

        /// <summary>
        /// Gets the underlying IQueryable associated with this query.
        /// </summary>
        internal IQueryable<T> Query { get; private set; }

        /// <summary>
        /// Create a new query based off an existing query and and a new
        /// queryable.  This is used via MobileServiceTableQueryable's
        /// combinators to construct new queries from simpler base queries.
        /// </summary>
        /// <param name="baseQuery">The base query.</param>
        /// <param name="query">The new queryable.</param>
        /// <param name="includeTotalCount">
        /// A value that if set will determine whether the query will request
        /// the total count for all the records that would have been returned
        /// ignoring any take paging/limit clause specified by client or
        /// server.  If this value is not set, we'll use the baseQuery's
        /// RequestTotalProperty instead (this is specifically so that our
        /// IncludeTotalCount method will preserve state on the old query).
        /// </param>
        /// <returns>The new query.</returns>
        internal static MobileServiceTableQuery<T> Create(MobileServiceTableQuery<T> baseQuery, IQueryable<T> query, bool? includeTotalCount = null)
        {
            Debug.Assert(baseQuery != null, "baseQuery cannot be null!");
            Debug.Assert(query != null, "query cannot be null!");

            // NOTE: Make sure any changes to this logic are reflected in the
            // Select method below which has its own version of this code to
            // work around type changes for its projection.
            return new MobileServiceTableQuery<T>(
                baseQuery.Table,
                includeTotalCount ?? baseQuery.RequestTotalCount)
                {
                    Query = query
                };
        }

        /// <summary>
        /// Compile the query into a MobileServiceTableQueryDescription.
        /// </summary>
        /// <returns>The compiled OData query.</returns>
        internal MobileServiceTableQueryDescription Compile()
        {
            // Compile the query from the underlying IQueryable's expression
            // tree
            MobileServiceTableQueryDescription compiledQuery = MobileServiceTableQueryTranslator.Translate(this.Query.Expression);
            
            // Forward along the request for the total count
            compiledQuery.IncludeTotalCount = this.RequestTotalCount;

            // Associate the current table with the compiled query
            if (string.IsNullOrEmpty(compiledQuery.TableName))
            {
                SerializableType type = SerializableType.Get(
                    compiledQuery.ProjectionArgumentType ?? typeof(T));
                compiledQuery.TableName = type.TableName;
            }

            return compiledQuery;
        }

        /// <summary>
        /// Applies the specified filter predicate to the source query.
        /// </summary>
        /// <param name="predicate">The filter predicate.</param>
        /// <returns>The composed query.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        public MobileServiceTableQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }

            return Create(this, Queryable.Where(this.Query, predicate));
        }

        /// <summary>
        /// Applies the specified selection to the source query.
        /// </summary>
        /// <typeparam name="U">
        /// Type representing the projected result of the query.
        /// </typeparam>
        /// <param name="selector">The selector function.</param>
        /// <returns>The composed query.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1715:IdentifiersShouldHaveCorrectPrefix", MessageId = "T", Justification = "Part of the LINQ query pattern.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "U", Justification = "Standard for LINQ")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        public MobileServiceTableQuery<U> Select<U>(Expression<Func<T, U>> selector)
        {
            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }

            // HACK: Create a new table with the same name/client but
            // pretending to be of a different type since the query needs to
            // have the same type as the table.  This won't cause any issues
            // since we're only going to use it to evaluate the query and it'll
            // never leak to users.
            MobileServiceTable<U> table = new MobileServiceTable<U>(
                this.Table.TableName,
                this.Table.MobileServiceClient);
            return new MobileServiceTableQuery<U>(
                table,
                this.RequestTotalCount)
                {
                    Query = Queryable.Select(this.Query, selector)
                };
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
        /// <returns>The composed query.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        public MobileServiceTableQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }

            return Create(this, Queryable.OrderBy(this.Query, keySelector));
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
        /// <returns>The composed query.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        public MobileServiceTableQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }

            return Create(this, Queryable.OrderByDescending(this.Query, keySelector));
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
        /// <returns>The composed query.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        public MobileServiceTableQuery<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }

            return Create(this, Queryable.ThenBy((IOrderedQueryable<T>)this.Query, keySelector));
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
        /// <returns>The composed query.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        public MobileServiceTableQuery<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }

            return Create(this, Queryable.ThenByDescending((IOrderedQueryable<T>)this.Query, keySelector));
        }

        /// <summary>
        /// Applies the specified skip clause to the source query.
        /// </summary>
        /// <param name="count">The number to skip.</param>
        /// <returns>The composed query.</returns>
        public MobileServiceTableQuery<T> Skip(int count)
        {
            return Create(this, Queryable.Skip(this.Query, count));
        }

        /// <summary>
        /// Applies the specified take clause to the source query.
        /// </summary>
        /// <param name="count">The number to take.</param>
        /// <returns>The composed query.</returns>
        public MobileServiceTableQuery<T> Take(int count)
        {
            return Create(this, Queryable.Take(this.Query, count));
        }

        /// <summary>
        /// Ensure the query will get the total count for all the records that
        /// would have been returned ignoring any take paging/limit clause
        /// specified by client or server.
        /// </summary>
        /// <returns>The query object.</returns>
        public MobileServiceTableQuery<T> IncludeTotalCount()
        {
            return Create(this, this.Query, true);
        }

        /// <summary>
        /// Evalute the query asynchronously and return the results.
        /// </summary>
        /// <returns>The evaluated query results.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Not nested when used via async pattern.")]
        public Task<IEnumerable<T>> ToEnumerableAsync()
        {
            return this.Table.ReadAsync(this);
        }

        /// <summary>
        /// Evalute the query asynchronously and return the results in a new
        /// List.
        /// </summary>
        /// <returns>The evaluated query results as a List.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Not nested when used via async pattern.")]
        public async Task<List<T>> ToListAsync()
        {
            IEnumerable<T> items = await this.Table.ReadAsync(this);
            return new TotalCountList<T>(items);
        }

        /// <summary>
        /// Create a new collection view based on the query.
        /// </summary>
        /// <returns>The collection view.</returns>
        public MobileServiceCollectionView<T> ToCollectionView()
        {
            MobileServiceTableQueryDescription query = this.Compile();
            return new MobileServiceCollectionView<T>(this.Table, query);
        }
    }
}
