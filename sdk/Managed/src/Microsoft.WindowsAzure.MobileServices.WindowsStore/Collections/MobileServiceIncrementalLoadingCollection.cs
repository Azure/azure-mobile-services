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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// An asynchronous data source that implements ISupportIncrementalLoading and 
    /// can wrap the results of a Mobile Services query in a way that's easily 
    /// consumed by Xaml collection controls like ListView, GridView or ListBox.
    /// </summary>
    /// <typeparam name="TTable">Data source element type.</typeparam>
    /// <typeparam name="TCollection">Type of elements ending up in the collection.</typeparam>
    /// <remarks>S
    /// Currently handles asynchronously loading the data, notifying the controls and paging.
    /// Use the <see cref="MobileServiceIncrementalLoadingCollection<T>"/> class if the table and 
    /// collection items are of the same type.
    /// </remarks>
    public class MobileServiceIncrementalLoadingCollection<TTable, TCollection> : 
        MobileServiceCollection<TTable, TCollection>,
        ISupportIncrementalLoading
    {
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification="Overridable method is only used for change notifications")]
        public MobileServiceIncrementalLoadingCollection(IMobileServiceTableQuery<TTable> query, Func<IEnumerable<TTable>, IEnumerable<TCollection>> selector)
            :base(query, selector, 1) 
        {
            this.ThrowExceptionsFromLoading = true;
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
        public MobileServiceIncrementalLoadingCollection(IMobileServiceTableQuery<TTable> query, Func<TTable, TCollection> selector)
            : this(query, ie => ie.Select(selector)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:IncrementalLoadingMobileServiceCollection<TTable, TCollection>"/>
        /// class. This constructior should be used in cases where TTable and TCollection are the same type.
        /// </summary>
        /// <param name="query">
        /// The data source's query which provides the data.
        /// </param>
        public MobileServiceIncrementalLoadingCollection(IMobileServiceTableQuery<TTable> query)
            : this(query, ie => ie.Cast<TCollection>()) { }
        
        /// <summary>
        /// Indicates if more items can be requested by the control.
        /// </summary>
        bool ISupportIncrementalLoading.HasMoreItems 
        { 
            get { return this.HasMoreItems; }
        }

        /// <summary>
        /// Indicates if the exceptions from loading more data should be
        /// thrown or not.
        /// </summary>
        protected bool ThrowExceptionsFromLoading { get; set; }

        /// <summary>
        /// Explicit implementation of ISupportIncrementalLoading.LoadMoreItemsAsync 
        /// which delegates the work to the base class
        /// </summary>
        /// <param name="count">
        /// Number of items requested by the control.
        /// This is ignored since we have our own page size.
        /// </param>
        /// <returns></returns>
        IAsyncOperation<LoadMoreItemsResult> ISupportIncrementalLoading.LoadMoreItemsAsync(uint count)
        {
            // we pass the count argument to override the pageSize
            return AsyncInfo.Run(async (c) => 
            {
                int results = 0;
                try
                {
                    results = await base.LoadMoreItemsAsync(c, (int)count);   
                }
                catch (Exception e)
                {
                    OnExceptionOccurred(e);

                    if (this.ThrowExceptionsFromLoading)
                    {
                        throw;
                    }
                }

                return new LoadMoreItemsResult() { Count = (uint)results };
            });
        }

        /// <summary>
        /// Provided to allow users to handle exceptions resulting from calls to 
        /// LoadMoreItemsAsync performed by controls.
        /// </summary>
        /// <param name="exception">The exception.</param>
        protected virtual void OnExceptionOccurred(Exception exception)
        {
            // Do nothing
        }
    }

    /// <summary>
    /// An asynchronous data source that implements ISupportIncrementalLoading and 
    /// can wrap the results of a Mobile Services query in a way that's easily 
    /// consumed by Xaml collection controls like ListView, GridView or ListBox.
    /// </summary>
    /// <typeparam name="T">Data source and collection element type.</typeparam>
    /// <remarks>
    /// Handles asynchronously loading the data, notifying the controls and paging.
    /// </remarks>
    public class MobileServiceIncrementalLoadingCollection<T> : MobileServiceIncrementalLoadingCollection<T, T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:IncrementalLoadingMobileServiceCollection<T>"/> class.
        /// </summary>
        /// <param name="query">
        /// The data source's query, which provides the data.
        /// </param>
        public MobileServiceIncrementalLoadingCollection(IMobileServiceTableQuery<T> query)
            : base(query, (Func<IEnumerable<T>, IEnumerable<T>>)(t => t))
        {
        }
    }
}
