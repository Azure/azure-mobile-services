// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides operations on a table for a Mobile Service.
    /// </summary>
    /// <typeparam name="T">
    /// The type of elements in the table (which implies the table).
    /// </typeparam>
    public partial interface IMobileServiceTable<T> : IMobileServiceTable
    {
        /// <summary>
        /// Get the elements of a table based using a query.
        /// </summary>
        /// <typeparam name="U">
        /// The type of element returned by the query.
        /// </typeparam>
        /// <param name="query">The query to execute.</param>
        /// <returns>Elements of the table.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1715:IdentifiersShouldHaveCorrectPrefix", MessageId = "T", Justification = "Part of the LINQ query pattern")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "U", Justification = "Part of the LINQ query pattern")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Generic are not nested when used via async.")]
        Task<IEnumerable<U>> ReadAsync<U>(MobileServiceTableQuery<U> query);

        /// <summary>
        /// Get the elements in a table.
        /// </summary>
        /// <returns>The elements in the table.</returns>
        /// <remarks>
        /// This call will not handle paging, etc., for you.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "It does not appear nested when used via the async pattern.")]
        Task<IEnumerable<T>> ReadAsync();

        /// <summary>
        /// Lookup an element from a table by its ID.
        /// </summary>
        /// <param name="id">The ID of the element.</param>
        /// <returns>The desired element.</returns>
        Task<T> LookupAsync(object id);

        /// <summary>
        /// Refresh the current instance with the latest values from the
        /// table.
        /// </summary>
        /// <param name="instance">The instance to refresh.</param>
        /// <returns>
        /// A task that will complete when the refresh has finished.
        /// </returns>
        Task RefreshAsync(T instance);

        /// <summary>
        /// Insert a new instance into the table.
        /// </summary>
        /// <param name="instance">The instance to insert.</param>
        /// <returns>
        /// A task that will complete when the insertion has finished.
        /// </returns>
        Task InsertAsync(T instance);

        /// <summary>
        /// Updates an instance in the table.
        /// </summary>
        /// <param name="instance">The instance to update.</param>
        /// <returns>
        /// A task that will complete when the update has finished.
        /// </returns>
        Task UpdateAsync(T instance);

        /// <summary>
        /// Delete an instance from the table.
        /// </summary>
        /// <param name="instance">The instance to delete.</param>
        /// <returns>
        /// A task that will complete when the delete has finished.
        /// </returns>
        Task DeleteAsync(T instance);

        /// <summary>
        /// Creates a query by applying the specified filter predicate.
        /// </summary>
        /// <param name="predicate">The filter predicate.</param>
        /// <returns>A query against the table.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        MobileServiceTableQuery<T> Where(Expression<Func<T, bool>> predicate);        

        /// <summary>
        /// Creates a query by applying the specified selection.
        /// </summary>
        /// <typeparam name="U">
        /// Type representing the projected result of the query.
        /// </typeparam>
        /// <param name="selector">The selector function.</param>
        /// <returns>A query against the table.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1715:IdentifiersShouldHaveCorrectPrefix", MessageId = "T", Justification = "Standard for LINQ")]
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Select", Justification = "Part of the LINQ query pattern.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "U", Justification = "Standard for LINQ")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        MobileServiceTableQuery<U> Select<U>(Expression<Func<T, U>> selector);        

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
        MobileServiceTableQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);        

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
        MobileServiceTableQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
        
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
        MobileServiceTableQuery<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector);
        
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
        MobileServiceTableQuery<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
        
        /// <summary>
        /// Creates a query by applying the specified skip clause.
        /// </summary>
        /// <param name="count">The number to skip.</param>
        /// <returns>A query against the table.</returns>
        MobileServiceTableQuery<T> Skip(int count);
        
        /// <summary>
        /// Creates a query by applying the specified take clause.
        /// </summary>
        /// <param name="count">The number to take.</param>
        /// <returns>A query against the table.</returns>
        MobileServiceTableQuery<T> Take(int count);

        /// <summary>
        /// Gets the elements of the table asynchronously.
        /// </summary>
        /// <returns>The table elements results as a sequence.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Not nested when used via async pattern.")]
        Task<IEnumerable<T>> ToEnumerableAsync();
        
        /// <summary>
        /// Gets the elements of the table asynchronously and return the
        /// results in a new List.
        /// </summary>
        /// <returns>The table elements results as a List.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Not nested when used via async pattern.")]
        Task<List<T>> ToListAsync();
        
        /// <summary>
        /// Create a new collection view based on the query.
        /// </summary>
        /// <returns>The collection view.</returns>
        MobileServiceCollectionView<T> ToCollectionView();        
    }
}
