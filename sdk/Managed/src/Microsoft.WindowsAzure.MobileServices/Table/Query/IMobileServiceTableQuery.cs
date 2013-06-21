// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
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
    public interface IMobileServiceTableQuery<T>
    {
        /// <summary>
        /// Gets a value indicating whether the query will request the total
        /// count for all the records that would have been returned ignoring
        /// any take paging/limit clause specified by client or server.
        /// </summary>
        bool RequestTotalCount { get; }

        /// <summary>
        /// The user-defined query string parameters to include with the query.
        /// </summary>
        IDictionary<string, string> Parameters { get; }

        /// <summary>
        /// Ensure the query will get the total count for all the records that
        /// would have been returned ignoring any take paging/limit clause
        /// specified by client or server.
        /// </summary>
        /// <returns>
        /// The query object.
        /// </returns>
        IMobileServiceTableQuery<T> IncludeTotalCount();

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
        IMobileServiceTableQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);

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
        IMobileServiceTableQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);

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
        IMobileServiceTableQuery<U> Select<U>(Expression<Func<T, U>> selector);

        /// <summary>
        /// Applies the specified skip clause to the source query.
        /// </summary>
        /// <param name="count">
        /// The number to skip.
        /// </param>
        /// <returns>
        /// The composed query.
        /// </returns>
        IMobileServiceTableQuery<T> Skip(int count);

        /// <summary>
        /// Gets the MobileServiceTable being queried.
        /// </summary>
        IMobileServiceTable<T> Table { get; }

        /// <summary>
        /// Applies the specified take clause to the source query.
        /// </summary>
        /// <param name="count">
        /// The number to take.
        /// </param>
        /// <returns>
        /// The composed query.
        /// </returns>
        IMobileServiceTableQuery<T> Take(int count);

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
        IMobileServiceTableQuery<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector);

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
        IMobileServiceTableQuery<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector);

        /// <summary>
        /// Evalute the query asynchronously and return the results.
        /// </summary>
        /// <returns>
        /// The evaluated query results.
        /// </returns>
        Task<IEnumerable<T>> ToEnumerableAsync();

        /// <summary>
        /// Evalute the query asynchronously and return the results in a new
        /// List.
        /// </summary>
        /// <returns>
        /// The evaluated query results as a List.
        /// </returns>
        Task<List<T>> ToListAsync();

        /// <summary>
        /// Applies the specified filter predicate to the source query.
        /// </summary>
        /// <param name="predicate">
        /// The filter predicate.
        /// </param>
        /// <returns>
        /// The composed query.
        /// </returns>
        IMobileServiceTableQuery<T> Where(Expression<Func<T, bool>> predicate);

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
        IMobileServiceTableQuery<T> WithParameters(IDictionary<string, string> parameters);
    }
}
