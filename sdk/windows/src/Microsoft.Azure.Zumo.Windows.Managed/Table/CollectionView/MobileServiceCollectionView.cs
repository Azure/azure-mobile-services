// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// An asynchronous data source that can wrap the results of a Mobile
    /// Services query in a way that's easily consumed by Xaml collection
    /// controls like ListView.
    /// </summary>
    /// <typeparam name="T">Data source element type.</typeparam>
    /// <remarks>
    /// This currently just handles asynchronously loading the data and
    /// notifying the controls, but we'd eventually like to support paging,
    /// UI virtualization, and a host of other features.
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "CollectionView is a more appropriate suffix.")]
    public sealed class MobileServiceCollectionView<T> :
        // Note that ICollectionView includes IList<object> which prevents us
        // from implementing IList<T> as well (since the two could unify for
        // some types).  We've worked around this by explicitly implementing
        // the IList<object> interface and providing all the IList<T> methods
        // without actually implementing it.  It will make it easy enough to
        // use as a strongly typed collection, but you'll need to explicitly
        // cast to IList<object> or IList before passing it into a collection.
        ICollectionView,
        System.Collections.IList,
        INotifyPropertyChanged,
        INotifyCollectionChanged,
        ISupportIncrementalLoading
    {
        /// <summary>
        /// The table associated with the query represented by this data
        /// source.
        /// </summary>
        private MobileServiceTable<T> table;

        /// <summary>
        /// The cached copy of our data that provides a basic implementation
        /// of our collection interfaces.
        /// </summary>
        private List<T> data = null;

        /// <summary>
        /// The query that when evaluated will populate the data souce with
        /// data.  We'll evaluate the query once per page while data
        /// virtualizing.
        /// </summary>
        private MobileServiceTableQueryDescription query = null;

        /// <summary>
        /// Initializes a new instance of the MobileServiceCollectionView
        /// class.
        /// </summary>
        /// <param name="table">
        /// Table associated with the data source's query.
        /// </param>
        /// <param name="query">The query the data source represents.</param>
        internal MobileServiceCollectionView(MobileServiceTable<T> table, MobileServiceTableQueryDescription query)
        {
            Debug.Assert(table != null, "table cannot be null!");
            Debug.Assert(query != null, "query cannot be null!");

            this.table = table;
            this.query = query;

            // Evaluate the query immediately and start loading the data for it
            this.data = new List<T>();
            this.EvaluateQueryAsync();
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Occurs then the collection changes.
        /// </summary>
        public event VectorChangedEventHandler<object> VectorChanged;
        
        /// <summary>
        /// Occurs before the current item is changed.
        /// </summary>
        public event CurrentChangingEventHandler CurrentChanging;

        /// <summary>
        /// Occurs after the current item has been changed.
        /// </summary>
        public event EventHandler<object> CurrentChanged;

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        public int Count
        {
            get { return this.data.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether there are more items that can be
        /// loaded incrementally.
        /// </summary>
        public bool HasMoreItems
        {
            // TODO: Implement this once we support paging.
            get { return false; }
        }

        /// <summary>
        /// Gets the collection groups associated with the collection view.
        /// </summary>
        public IObservableVector<object> CollectionGroups
        {
            // TODO: Decide if we want to support grouping - or at least return
            // an empty collection instead of null.
            get { return null; }
        }

        /// <summary>
        /// Gets the position of the current item within the collection view.
        /// </summary>
        public int CurrentPosition { get; private set; }

        /// <summary>
        /// Gets the current item in the collection view.
        /// </summary>
        public object CurrentItem
        {
            get
            {
                return (this.CurrentPosition >= 0 && this.CurrentPosition < this.data.Count) ?
                    (object)this.data[this.CurrentPosition] :
                    null;
            }
        }
    
        /// <summary>
        /// Gets a value indicating whether the current item is before the
        /// beginning of the collection.
        /// </summary>
        public bool IsCurrentBeforeFirst
        {
            get { return this.CurrentPosition < 0; }
        }

        /// <summary>
        /// Gets a value indicating whether the current item is after the end
        /// of the collection.
        /// </summary>
        public bool IsCurrentAfterLast
        {
            // TODO: This will of course need to change once we support paging.
            get { return this.CurrentPosition >= this.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether or not the current item is within
        /// the bounds of the collection view.
        /// </summary>
        private bool IsCurrentInView
        {
            get { return !this.IsCurrentBeforeFirst && !this.IsCurrentAfterLast; }
        }

        #region Collection members
        /// <summary>
        /// Gets the number of items in the collection.
        /// </summary>
        int System.Collections.ICollection.Count
        {
            get { return this.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the collection is synchronized.
        /// </summary>
        bool System.Collections.ICollection.IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// Gets an object used to synchronize the collection.
        /// </summary>
        object System.Collections.ICollection.SyncRoot
        {
            get { return null; }
        }

        /// <summary>
        /// Gets a value indicating whether the list has a fixed size.
        /// </summary>
        bool System.Collections.IList.IsFixedSize
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the collection is readonly.
        /// </summary>
        bool System.Collections.IList.IsReadOnly
        {
            get { return this.IsReadOnly; }
        }
        #endregion Collection members

        /// <summary>
        /// Gets or sets the item at the specified index.
        /// </summary>
        /// <param name="index">Index of the item to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        public T this[int index]
        {
            get { return this.data[index]; }
            set
            {
                T old = this.data[index];
                this.data[index] = value;
                this.OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace,
                        value,
                        old,
                        index));
            }
        }

        /// <summary>
        /// Gets or sets an element at a given index.
        /// </summary>
        /// <param name="index">The index to retrieve.</param>
        /// <returns>The element at that index.</returns>
        object System.Collections.IList.this[int index]
        {
            get { return this[index]; }
            set { this[index] = (T)value; }
        }

        /// <summary>
        /// Gets or sets an element at a given index.
        /// </summary>
        /// <param name="index">The index to retrieve.</param>
        /// <returns>The element at that index.</returns>
        object IList<object>.this[int index]
        {
            get { return this[index]; }
            set { this[index] = (T)value; }
        }

        /// <summary>
        /// Occurs when a property changes.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region Data virtualization
        /// <summary>
        /// Evaluate the query and repopulate our collection with the results.
        /// </summary>
        private async void EvaluateQueryAsync()
        {
            // Invoke the query on the server and get our data
            IEnumerable<T> data = await MobileServiceTable<T>.EvaluateQueryAsync<T>(this.table, this.query);

            // Reset the collection with the new data
            this.data.Clear();
            this.data.AddRange(data);

            this.OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Load more items asynchronously.
        /// </summary>
        /// <param name="count">The number of items to load.</param>
        /// <returns>The result of loading the items.</returns>
        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            // TODO: Implement once we support paging
            throw new NotImplementedException();
        }
        #endregion Data virtualization

        #region Currency
        /// <summary>
        /// Raise the CurrentChanging event.
        /// </summary>
        /// <returns>
        /// A value indicating whether the change should be cancelled.
        /// </returns>
        private bool RaiseCurrentChanging()
        {
            bool cancel = false;

            CurrentChangingEventHandler handler = this.CurrentChanging;
            if (handler != null)
            {
                CurrentChangingEventArgs args = new CurrentChangingEventArgs(true);
                handler(this, args);
                cancel = args.Cancel;
            }

            return cancel;
        }

        /// <summary>
        /// Raise the CurrentChanged event.
        /// </summary>
        private void RaiseCurrentChanged()
        {
            EventHandler<object> handler = this.CurrentChanged;
            if (handler != null)
            {
                handler(this, this.CurrentItem);
            }
        }

        /// <summary>
        /// Sets the specified position as the current item in the collection
        /// view.
        /// </summary>
        /// <param name="index">Index of the current item.</param>
        /// <returns>
        /// A value indicating whether the current item is within the
        /// collection view.
        /// </returns>
        public bool MoveCurrentToPosition(int index)
        {
            if (this.RaiseCurrentChanging())
            {
                this.CurrentPosition = index;
                this.RaiseCurrentChanged();
            }

            return this.IsCurrentInView;
        }

        /// <summary>
        /// Sets the first instance of the specified item in our currentoy
        /// loaded data as the current item in the collection view.
        /// </summary>
        /// <param name="item">The item to search for.</param>
        /// <returns>
        /// A value indicating whether the current item is within the
        /// collection view.
        /// </returns>
        public bool MoveCurrentTo(object item)
        {
            // Lookup the (first) index of the item in our currently cached
            // values and move to its position
            int index = this.data.IndexOf((T)item);
            return this.MoveCurrentToPosition(index);
        }

        /// <summary>
        /// Move the current item to the first item in the collection view.
        /// </summary>
        /// <returns>
        /// A value indicating whether the current item is within the
        /// collection view.
        /// </returns>
        public bool MoveCurrentToFirst()
        {
            if (this.RaiseCurrentChanging())
            {
                this.CurrentPosition = this.Count > 0 ? 0 : -1;
                this.RaiseCurrentChanged();
            }

            return this.IsCurrentInView;
        }

        /// <summary>
        /// Move the current item to the last item in the collection view.
        /// </summary>
        /// <returns>
        /// A value indicating whether the current item is within the
        /// collection view.
        /// </returns>
        public bool MoveCurrentToLast()
        {
            if (this.RaiseCurrentChanging())
            {
                this.CurrentPosition = this.Count > 0 ?
                    this.Count - 1 :
                    0;
                this.RaiseCurrentChanged();
            }

            return this.IsCurrentInView;
        }

        /// <summary>
        /// Move the current item to the next item in the collection view.
        /// </summary>
        /// <returns>
        /// A value indicating whether the current item is within the
        /// collection view.
        /// </returns>
        public bool MoveCurrentToNext()
        {
            if (this.RaiseCurrentChanging())
            {
                if (!this.IsCurrentAfterLast)
                {
                    this.CurrentPosition++;
                }
                this.RaiseCurrentChanged();
            }

            return this.IsCurrentInView;
        }

        /// <summary>
        /// Move the current item to the previous item in the collection view.
        /// </summary>
        /// <returns>
        /// A value indicating whether the current item is within the
        /// collection view.
        /// </returns>
        public bool MoveCurrentToPrevious()
        {
            if (this.RaiseCurrentChanging())
            {
                if (!this.IsCurrentBeforeFirst)
                {
                    this.CurrentPosition--;
                }
                this.RaiseCurrentChanged();
            }

            return this.IsCurrentInView;
        }
        #endregion Currency

        #region Collection members
        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        /// <param name="args">Collection changed arguments.</param>
        private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            Debug.Assert(args != null, "args cannot be null.");

            NotifyCollectionChangedEventHandler handler = this.CollectionChanged;
            if (handler != null)
            {
                handler(this, args);
            }

            // Also raise VectorChanged using the information passed to us via
            // the CollectionChanged args.
            VectorChangedEventHandler<object> vectorHandler = this.VectorChanged;
            if (vectorHandler != null)
            {
                CollectionChange change;
                uint index;
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        change = CollectionChange.ItemInserted;
                        index = (uint)args.NewStartingIndex;
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        change = CollectionChange.ItemRemoved;
                        index = (uint)args.OldStartingIndex;
                        break;
                    
                    case NotifyCollectionChangedAction.Move:
                    case NotifyCollectionChangedAction.Replace:
                        change = CollectionChange.ItemChanged;
                        index = (uint)args.NewStartingIndex;
                        break;
                    case NotifyCollectionChangedAction.Reset:
                    default:
                        change = CollectionChange.Reset;
                        index = 0;
                        break;
                }

                vectorHandler(this, new VectorChangedEventArgs(change, index));
            }
        }

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="item">The item to add to the collection.</param>
        public void Add(T item)
        {
            this.data.Add(item);
            this.OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    item,
                    this.data.Count - 1));
        }

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="item">The item to add to the collection.</param>
        void ICollection<object>.Add(object item)
        {
            this.Add((T)item);
        }

        /// <summary>
        /// Adds an item the collection.
        /// </summary>
        /// <param name="value">Item to add.</param>
        /// <returns>The position the item was inserted.</returns>
        int System.Collections.IList.Add(object value)
        {
            this.Add((T)value);
            return this.Count - 1;
        }

        /// <summary>
        /// Insert an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The index to insert at.</param>
        /// <param name="item">The item to insert.</param>
        public void Insert(int index, T item)
        {
            this.data.Insert(index, item);
            this.OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    item,
                    index));
        }

        /// <summary>
        /// Insert an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The index to insert at.</param>
        /// <param name="item">The item to insert.</param>
        void IList<object>.Insert(int index, object item)
        {
            this.Insert(index, (T)item);
        }

        /// <summary>
        /// Insert an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The index to insert at.</param>
        /// <param name="value">The item to insert.</param>
        void System.Collections.IList.Insert(int index, object value)
        {
            this.Insert(index, (T)value);
        }
        
        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">Index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            T old = this.data[index];
            this.data.RemoveAt(index);

            this.OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    old,
                    index));
        }

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">Index of the item to remove.</param>
        void System.Collections.IList.RemoveAt(int index)
        {
            this.RemoveAt(index);
        }

        /// <summary>
        /// Removes the first occurence of the specified item from the list.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>True if removed, false otherwise.</returns>
        public bool Remove(T item)
        {
            // Calling _data.Remove directly caused memory corruption when we
            // raised the notification changed event (likely because the native
            // INCC event listener was't happy being passed an item that was
            // no longer in the collection).

            int index = this.data.IndexOf(item);
            if (index >= 0)
            {
                this.data.RemoveAt(index);
                this.OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove,
                        item,
                        index));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the first occurence of the specified item from the list.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>True if removed, false otherwise.</returns>
        bool ICollection<object>.Remove(object item)
        {
            return this.Remove((T)item);
        }

        /// <summary>
        /// Removes the first occurence of the specified item from the list.
        /// </summary>
        /// <param name="value">The item to remove.</param>
        void System.Collections.IList.Remove(object value)
        {
            this.Remove((T)value);
        }
        
        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public void Clear()
        {
            this.data.Clear();
            this.OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Removes all the items from the list.
        /// </summary>
        void System.Collections.IList.Clear()
        {
            this.Clear();
        }

        /// <summary>
        /// Determines the index of a specific item in the list.
        /// </summary>
        /// <param name="item">The item to locate.</param>
        /// <returns>The index of the item.</returns>
        public int IndexOf(T item)
        {
            return this.data.IndexOf(item);
        }

        /// <summary>
        /// Determines the index of a specific item in the list.
        /// </summary>
        /// <param name="item">The item to locate.</param>
        /// <returns>The index of the item.</returns>
        int IList<object>.IndexOf(object item)
        {
            return this.IndexOf((T)item);
        }

        /// <summary>
        /// Determines the index of a specified item in the list.
        /// </summary>
        /// <param name="value">The value to search for.</param>
        /// <returns>Index of the desired vaue or -1.</returns>
        int System.Collections.IList.IndexOf(object value)
        {
            return this.IndexOf((T)value);
        }

        /// <summary>
        /// Determines whether the collection contains a given element.
        /// </summary>
        /// <param name="item">The element to check for.</param>
        /// <returns>Whether the collection contains the element.</returns>
        public bool Contains(T item)
        {
            return this.data.Contains(item);
        }

        /// <summary>
        /// Determines whether the collection contains a given element.
        /// </summary>
        /// <param name="item">The element to check for.</param>
        /// <returns>Whether the collection contains the element.</returns>
        bool ICollection<object>.Contains(object item)
        {
            return this.Contains((T)item);
        }

        /// <summary>
        /// Determines whether a list contains a specified value.
        /// </summary>
        /// <param name="value">The object to find in the list.</param>
        /// <returns>Whether the value was found in the list.</returns>
        bool System.Collections.IList.Contains(object value)
        {
            return this.Contains((T)value);
        }

        /// <summary>
        /// Copies the elements of the collection to an array.
        /// </summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">Starting index for the copy.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            this.data.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Copies the elements of the collection to an array.
        /// </summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">Starting index for the copy.</param>
        void ICollection<object>.CopyTo(object[] array, int arrayIndex)
        {
            this.CopyTo(array.Cast<T>().ToArray(), arrayIndex);
        }

        /// <summary>
        /// Copy the elements of the collection to an array.
        /// </summary>
        /// <param name="array">Destination of the copy.</param>
        /// <param name="index">Starting index of the copy.</param>
        void System.Collections.ICollection.CopyTo(Array array, int index)
        {
            this.CopyTo((T[])array, index);
        }

        /// <summary>
        /// Get an enumerator for the data source.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return this.data.GetEnumerator();
        }

        /// <summary>
        /// Get an enumerator for the data source.
        /// </summary>
        /// <returns>The enumerator.</returns>
        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            return this.Cast<object>().GetEnumerator();
        }

        /// <summary>
        /// Enumerate the items in the collection.
        /// </summary>
        /// <returns>Enumerator for the colletion.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion Collection members
    }
}
