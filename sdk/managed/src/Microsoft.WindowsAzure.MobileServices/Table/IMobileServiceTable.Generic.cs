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
    /// The type of instances in the table (which implies the table).
    /// </typeparam>
    public interface IMobileServiceTable<T> : IMobileServiceTable
    {
        /// <summary>
        /// Returns instances from a table using a query.
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
        [SuppressMessage("Microsoft.Naming", "CA1715:IdentifiersShouldHaveCorrectPrefix", MessageId = "T", Justification = "Part of the LINQ query pattern")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "U", Justification = "Part of the LINQ query pattern")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Generic are not nested when used via async.")]
        Task<IEnumerable<U>> ReadAsync<U>(IMobileServiceTableQuery<U> query);

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
        Task<IEnumerable<T>> ReadAsync();

        /// <summary>
        /// Lookup an instance from a table by its id.
        /// </summary>
        /// <param name="id">
        /// The id of the instance.
        /// </param>
        /// <returns>
        /// The desired instance.
        /// </returns>
        new Task<T> LookupAsync(object id);

        /// <summary>
        /// Lookup an instance from a table by its id.
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
        new Task<T> LookupAsync(object id, IDictionary<string, string> parameters);

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
        Task RefreshAsync(T instance);

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
        Task RefreshAsync(T instance, IDictionary<string, string> parameters);

        /// <summary>
        /// Insert a new instance into the table.
        /// </summary>
        /// <param name="instance">
        /// The instance to insert.
        /// </param>
        /// <returns>
        /// A task that will complete when the insertion has finished.
        /// </returns>
        Task InsertAsync(T instance);

        /// <summary>
        /// Insert a new instance into the table.
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
        Task InsertAsync(T instance, IDictionary<string, string> parameters);

        /// <summary>
        /// Updates an instance in the table.
        /// </summary>
        /// <param name="instance">
        /// The instance to update.
        /// </param>
        /// <returns>
        /// A task that will complete when the update has finished.
        /// </returns>
        Task UpdateAsync(T instance);

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
        Task UpdateAsync(T instance, IDictionary<string, string> parameters);

        /// <summary>
        /// Delete an instance from the table.
        /// </summary>
        /// <param name="instance">
        /// The instance to delete.
        /// </param>
        /// <returns>
        /// A task that will complete when the delete has finished.
        /// </returns>
        Task DeleteAsync(T instance);

        /// <summary>
        /// Delete an instance from the table.
        /// </summary>
        /// <param name="instance">The instance to delete.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in 
        /// the request URI query string.
        /// </param>
        /// <returns>
        /// A task that will complete when the delete has finished.
        /// </returns>
        Task DeleteAsync(T instance, IDictionary<string, string> parameters);

        /// <summary>
        /// Creates a query for the current table.
        /// </summary>
        /// <returns>
        /// A query against the table.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        IMobileServiceTableQuery<T> CreateQuery(); 

        /// <summary>
        /// Creates a query that will ensure it gets the total count for all
        /// the records that would have been returned ignoring any take paging/
        /// limit clause specified by client or server.
        /// </summary>
        /// <returns>
        /// A query against the table.
        /// </returns>
        IMobileServiceTableQuery<T> IncludeTotalCount();

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
        IMobileServiceTableQuery<T> Where(Expression<Func<T, bool>> predicate);        

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
        [SuppressMessage("Microsoft.Naming", "CA1715:IdentifiersShouldHaveCorrectPrefix", MessageId = "T", Justification = "Standard for LINQ")]
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Select", Justification = "Part of the LINQ query pattern.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "U", Justification = "Standard for LINQ")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Part of the LINQ query pattern.")]
        IMobileServiceTableQuery<U> Select<U>(Expression<Func<T, U>> selector);        

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
        IMobileServiceTableQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);        

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
        IMobileServiceTableQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
        
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
        IMobileServiceTableQuery<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector);
        
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
        IMobileServiceTableQuery<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
        
        /// <summary>
        /// Creates a query by applying the specified skip clause.
        /// </summary>
        /// <param name="count">
        /// The number to skip.
        /// </param>
        /// <returns>
        /// A query against the table.
        /// </returns>
        IMobileServiceTableQuery<T> Skip(int count);
        
        /// <summary>
        /// Creates a query by applying the specified take clause.
        /// </summary>
        /// <param name="count">
        /// The number to take.
        /// </param>
        /// <returns>
        /// A query against the table.
        /// </returns>
        IMobileServiceTableQuery<T> Take(int count);

        /// <summary>
        /// Gets the elements of the table asynchronously.
        /// </summary>
        /// <returns>
        /// The table elements results as a sequence.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Not nested when used via async pattern.")]
        Task<IEnumerable<T>> ToEnumerableAsync();
        
        /// <summary>
        /// Gets the elements of the table asynchronously and return the
        /// results in a new List.
        /// </summary>
        /// <returns>
        /// The table elements results as a List.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Not nested when used via async pattern.")]
        Task<List<T>> ToListAsync();   
    }
}
