// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// An asynchronous data source that can wrap the results of a Mobile
    /// Services query in a way that's easily consumed by Xaml collection
    /// controls like ListView, GridView or ListBox.
    /// </summary>
    /// <typeparam name="TTable">Data source element type.</typeparam>
    /// <typeparam name="TCollection">Type of elements ending up in the collection.</typeparam>
    /// <remarks>
    /// Currently handles asynchronously loading the data, notifying the controls and paging. 
    /// Use the <see cref="MobileServiceCollection<T>"/> class if the table and collection items
    /// are of the same type.
    /// </remarks>
    public class MobileServiceCollection<TTable, TCollection> : 
        ObservableCollection<TCollection>,
        ITotalCountProvider
    {
        private bool busy = false;
        
        /// <summary>
        /// The query that when evaluated will populate the data souce with
        /// data.  We'll evaluate the query once per page while data
        /// virtualizing.
        /// </summary>
        private IMobileServiceTableQuery<TTable> query = null;

        /// <summary>
        /// A selector function which will be appied to the data when it comes back from the server.
        /// </summary>
        protected Func<IEnumerable<TTable>, IEnumerable<TCollection>> selectorFunction;

        /// <summary>
        /// Numbers of items that will be retrieved per page. 0 means no paging.
        /// </summary>
        private int pageSize;

        /// <summary>
        /// Number of items already received from the server.
        /// </summary>
        private int itemsReceived;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:IncrementalLoadingMobileServiceCollection<TTable, TCollection>"/>
        /// class.
        /// </summary>
        /// <param name="query">
        /// The data source's query which provides the data.
        /// </param>
        /// <param name="selector">
        /// A selector function to provide client side projections.
        /// </param>
        /// <param name="pageSize">
        /// The number of items requested per request.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification="Overridable method is only used for change notifications")]
        public MobileServiceCollection(IMobileServiceTableQuery<TTable> query, Func<IEnumerable<TTable>, IEnumerable<TCollection>> selector, int pageSize = 0)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }
            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }
            if (pageSize < 0)
            {
                throw new ArgumentOutOfRangeException("pageSize");
            }

            this.query = query;
            //by default try to Cast
            this.selectorFunction = selector;
            this.pageSize = pageSize;

            this.HasMoreItems = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:IncrementalLoadingMobileServiceCollection<TTable, TCollection>"/>
        /// class.
        /// </summary>
        /// <param name="query">
        /// The data source's query which provides the data.
        /// </param>
        /// <param name="selector">
        /// A selector function to provide client side projections.
        /// </param>
        /// <param name="pageSize">
        /// The number of items requested per request.
        /// </param>
        public MobileServiceCollection(IMobileServiceTableQuery<TTable> query, Func<TTable, TCollection> selector, int pageSize = 0)
            : this(query, ie => ie.Select(selector), pageSize) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:IncrementalLoadingMobileServiceCollection<TTable, TCollection>"/>
        /// class. This constructior should be used in cases where TTable and TCollection are the same type.
        /// </summary>
        /// <param name="query">
        /// The data source's query which provides the data.
        /// </param>
        /// <param name="pageSize">
        /// The number of items requested per request.
        /// </param>
        public MobileServiceCollection(IMobileServiceTableQuery<TTable> query, int pageSize = 0)
            : this(query, ie => ie.Cast<TCollection>(), pageSize) { }

        /// <summary>
        /// The page size specified in the constructor.
        /// </summary>
        public int PageSize
        {
            get { return this.pageSize; }
        }

        /// <summary>
        /// The total count for all the records that would have been
        /// returned ignoring any take paging/limit clause specified by client
        /// or server. Value is -1 if the query did not request a total count. 
        /// </summary>
        private long totalCount = -1;

        /// <summary>
        /// Gets the total count for all the records that would have been
        /// returned ignoring any take paging/limit clause specified by client
        /// or server.
        /// </summary>
        public long TotalCount
        {
            get { return this.totalCount; }
            private set
            {
                if (this.totalCount != value)
                {
                    this.totalCount = value;
                    this.OnPropertyChanged();
                }
            }
        }

        #region Data virtualization

        /// <summary>
        /// Evaluates the query and adds the result to the collection.
        /// </summary>
        /// <param name="token">A token to cancel the operation.</param>
        /// <param name="query">The query to evaluate.</param>
        /// <returns>A task representing the ongoing operation.</returns>
        protected async virtual Task<int> ProcessQueryAsync(CancellationToken token, IMobileServiceTableQuery<TTable> query)
        {
            // Invoke the query on the server and get our data
            TotalCountEnumerable<TTable> data = await query.ToEnumerableAsync() as TotalCountEnumerable<TTable>;

            //check for cancellation
            if (token.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }

            foreach (var item in this.PrepareDataForCollection(data))
            {
                this.Add(item);
            }

            this.TotalCount = data.TotalCount;

            return data.Count();
        }

        #endregion

        #region Virtual methods

        /// <summary>
        /// Transforms the data from the query into data for the collection
        /// using the provided selector function.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns>The transformed data.</returns>
        public virtual IEnumerable<TCollection> PrepareDataForCollection(IEnumerable<TTable> items)
        {
            return selectorFunction(items);
        }

        /// <summary>
        /// Transforms one item into an item for the collection
        /// using the provided selector function.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The transformed item.</returns>
        public TCollection PrepareDataForCollection(TTable item)
        {
            return selectorFunction(new TTable[] { item }).FirstOrDefault();
        }

        #endregion

        #region IncrementalLoading

        private bool hasMoreItems;
        /// <summary>
        /// Gets a value indicating whether there are more items that can be
        /// loaded incrementally.
        /// </summary>
        public bool HasMoreItems
        {
            get { return this.hasMoreItems; }
            set
            {
                if (this.hasMoreItems != value)
                {
                    this.hasMoreItems = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Load more items asynchronously.
        /// Controls which support incremental loading on such as GridView on Windows 8 
        /// call this method automatically.
        /// In other cases you should call this method yourself.
        /// </summary>
        /// <param name="count">
        /// The number of items to load.
        /// This parameter overrides the pageSize specified in the constructore.
        /// </param>
        /// <returns>The result of loading the items.</returns>
        public Task<int> LoadMoreItemsAsync(int count = 0)
        {
            return this.LoadMoreItemsAsync(CancellationToken.None, count);
        }

        /// <summary>
        /// Load more items asynchronously.
        /// Controls which support incremental loading on such as GridView on Windows 8 
        /// call this method automatically.
        /// In other cases you should call this method yourself.
        /// </summary>
        /// <param name="token">
        /// The cancellation token to be used to cancel the task.
        /// </param>
        /// <param name="count">
        /// The number of items to load.
        /// This parameter overrides the pageSize specified in the constructore.
        /// </param>
        /// <returns>The result of loading the items.</returns>
        public async Task<int> LoadMoreItemsAsync(CancellationToken token, int count = 0)
        {
            if (busy)
            {
                throw new InvalidOperationException(Resources.MobileServiceCollection_LoadInProcess);
            }
            busy = true;

            try
            {
                //check for cancellation
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }

                IMobileServiceTableQuery<TTable> query = this.query;
                if (count == 0)
                {
                    count = PageSize;
                }
                //if still 0, PageSize was 0
                if (count != 0)
                {
                    query = query.Skip(itemsReceived).Take(count);
                }
                else
                {
                    //disable paging if pagesize is 0
                    this.HasMoreItems = false;
                }

                int results = await this.ProcessQueryAsync(token, query);

                if (results == 0)
                {
                    this.HasMoreItems = false;
                }
                else
                {
                    this.itemsReceived += results;
                }
                //safe conversion since there can't be negative results
                return results;
            }
            catch
            {
                // in case of error don't automatically try again
                this.HasMoreItems = false;
                throw;
            }
            finally
            {
                busy = false;
            }
        }

        #endregion

        #region INotifyPropertyChanged

        /// <summary>
        /// Invokes the PropertyChanged event for the <paramref name="propertyName"/> property.
        /// Provides a way for subclasses to override the event invocation behavior.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property that has changed.
        /// </param>
        /// <remarks>
        /// The CallerMemberName attribute will supply the value if no explicit value is provided.
        /// For more info see http://msdn.microsoft.com/en-us/library/hh534540.aspx
        /// We still need the null check, because you can still pass null as an explicit parameter.
        /// </remarks>
        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            // this looks weird but propertyName must always been assigned to because of 
            // the CallerMemberName attribute
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    /// <summary>
    /// An asynchronous data source that can wrap the results of a Mobile
    /// Services query in a way that's easily consumed by Xaml collection
    /// controls like ListView, GridView or ListBox.
    /// </summary>
    /// <typeparam name="T">Data source and collection element type.</typeparam>
    /// <remarks>
    /// This currently handles asynchronously loading the data, notifying the 
    /// controls and paging.
    /// </remarks>
    public class MobileServiceCollection<T> : MobileServiceCollection<T, T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:IncrementalLoadingMobileServiceCollection<T>"/> class.
        /// </summary>
        /// <param name="query">
        /// The data source's query which provides the data.
        /// </param>
        /// <param name="pageSize">
        /// The number of items requested per request.
        /// </param>
        public MobileServiceCollection(IMobileServiceTableQuery<T> query, int pageSize = 0)
            : base(query, (Func<IEnumerable<T>,IEnumerable<T>>)(t => t), pageSize)
        {
        }
    }
}
