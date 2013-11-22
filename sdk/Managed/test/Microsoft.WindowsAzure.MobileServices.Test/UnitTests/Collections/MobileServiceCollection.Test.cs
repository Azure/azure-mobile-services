// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Test;
using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("collection")]
    [Tag("unit")]
    public class MobileServiceCollectionTest : TestBase
    {
        [AsyncTestMethod]
        public async Task MobileServiceCollectionMultipleLoadItemsAsyncShouldThrow()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();

            MobileServiceCollection<Book> collection = new MobileServiceCollection<Book>(query);
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            Exception ex = null;

            try
            {
                await TaskEx.WhenAll(collection.LoadMoreItemsAsync(tokenSource.Token), collection.LoadMoreItemsAsync(tokenSource.Token));
            }
            catch (InvalidOperationException e)
            {
                ex = e;
            }

            Assert.IsNotNull(ex);
        } 

        [TestMethod]
        public void MobileServiceCollectionItemsCanBeAddedAndNotifies()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();
            query.EnumerableAsyncThrowsException = true;

            MobileServiceCollection<Book, Book> collection = new MobileServiceCollection<Book, Book>(query);

            List<string> properties = new List<string>();
            List<string> expectedProperties = new List<string>() { "Count", "Item[]" };
            List<NotifyCollectionChangedAction> actions = new List<NotifyCollectionChangedAction>();
            List<NotifyCollectionChangedAction> expectedActions = new List<NotifyCollectionChangedAction>() { NotifyCollectionChangedAction.Add };

            Book book = new Book();

            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => properties.Add(e.PropertyName);
            collection.CollectionChanged += (s, e) => actions.Add(e.Action);
            collection.Add(book);

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(book, collection[0]);
            Assert.IsTrue(properties.SequenceEqual(expectedProperties));
            Assert.IsTrue(actions.SequenceEqual(expectedActions));
        }

        [TestMethod]
        public void MobileServiceCollectionCanClearAndNotifies()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();
            query.EnumerableAsyncThrowsException = true;

            MobileServiceCollection<Book> collection = new MobileServiceCollection<Book>(query);

            List<string> properties = new List<string>();
            List<string> expectedProperties = new List<string>() { "Count", "Item[]" };
            List<NotifyCollectionChangedAction> actions = new List<NotifyCollectionChangedAction>();
            List<NotifyCollectionChangedAction> expectedActions = new List<NotifyCollectionChangedAction>() { NotifyCollectionChangedAction.Reset };

            Book book = new Book();
            collection.Add(book);

            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => properties.Add(e.PropertyName);
            collection.CollectionChanged += (s, e) => actions.Add(e.Action);
            collection.Clear();

            Assert.AreEqual(0, collection.Count);
            Assert.IsTrue(properties.SequenceEqual(expectedProperties));
            Assert.IsTrue(actions.SequenceEqual(expectedActions));
        }

        [TestMethod]
        public void MobileServiceCollectionCanContainsAndNotNotifies()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();
            query.EnumerableAsyncThrowsException = true;

            MobileServiceCollection<Book, Book> collection = new MobileServiceCollection<Book, Book>(query);

            List<string> properties = new List<string>();
            List<string> expectedProperties = new List<string>() {  };
            List<NotifyCollectionChangedAction> actions = new List<NotifyCollectionChangedAction>();
            List<NotifyCollectionChangedAction> expectedActions = new List<NotifyCollectionChangedAction>() {  };

            Book book = new Book();
            collection.Add(book);

            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => properties.Add(e.PropertyName);
            collection.CollectionChanged += (s, e) => actions.Add(e.Action);
            bool contains = collection.Contains(book);

            Assert.AreEqual(true, contains);
            Assert.IsTrue(properties.SequenceEqual(expectedProperties));
            Assert.IsTrue(actions.SequenceEqual(expectedActions));
        }

        [TestMethod]
        public void MobileServiceCollectionCanCopyToAndNotNotifies()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();
            query.EnumerableAsyncThrowsException = true;

            MobileServiceCollection<Book> collection = new MobileServiceCollection<Book>(query);

            List<string> properties = new List<string>();
            List<string> expectedProperties = new List<string>() {  };
            List<NotifyCollectionChangedAction> actions = new List<NotifyCollectionChangedAction>();
            List<NotifyCollectionChangedAction> expectedActions = new List<NotifyCollectionChangedAction>() {  };

            Book book = new Book();
            collection.Add(book);
            Book[] books = new Book[1];

            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => properties.Add(e.PropertyName);
            collection.CollectionChanged += (s, e) => actions.Add(e.Action);
            collection.CopyTo(books, 0);

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(1, books.Count());
            Assert.AreEqual(collection[0], books[0]);
            Assert.IsTrue(properties.SequenceEqual(expectedProperties));
            Assert.IsTrue(actions.SequenceEqual(expectedActions));
        }

        [TestMethod]
        public void MobileServiceCollectionCanIndexOfAndNotNotifies()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();
            query.EnumerableAsyncThrowsException = true;

            MobileServiceCollection<Book, Book> collection = new MobileServiceCollection<Book, Book>(query);

            List<string> properties = new List<string>();
            List<string> expectedProperties = new List<string>() { };
            List<NotifyCollectionChangedAction> actions = new List<NotifyCollectionChangedAction>();
            List<NotifyCollectionChangedAction> expectedActions = new List<NotifyCollectionChangedAction>() { };

            Book book = new Book();
            collection.Add(book);

            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => properties.Add(e.PropertyName);
            collection.CollectionChanged += (s, e) => actions.Add(e.Action);
            int indexof = collection.IndexOf(book);

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(0, indexof);
            Assert.IsTrue(properties.SequenceEqual(expectedProperties));
            Assert.IsTrue(actions.SequenceEqual(expectedActions));
        }

        [TestMethod]
        public void MobileServiceCollectionCanInsertAndNotifies()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();
            query.EnumerableAsyncThrowsException = true;

            MobileServiceCollection<Book> collection = new MobileServiceCollection<Book>(query);

            List<string> properties = new List<string>();
            List<string> expectedProperties = new List<string>() { "Count", "Item[]" };
            List<NotifyCollectionChangedAction> actions = new List<NotifyCollectionChangedAction>();
            List<NotifyCollectionChangedAction> expectedActions = new List<NotifyCollectionChangedAction>() { NotifyCollectionChangedAction.Add };

            Book book = new Book();

            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => properties.Add(e.PropertyName);
            collection.CollectionChanged += (s, e) => actions.Add(e.Action);
            collection.Insert(0, book);

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(book, collection[0]);
            Assert.IsTrue(properties.SequenceEqual(expectedProperties));
            Assert.IsTrue(actions.SequenceEqual(expectedActions));
        }
        
        [TestMethod]
        public void MobileServiceCollectionCanRemoveAndNotifies()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();
            query.EnumerableAsyncThrowsException = true;

            MobileServiceCollection<Book, Book> collection = new MobileServiceCollection<Book, Book>(query);

            List<string> properties = new List<string>();
            List<string> expectedProperties = new List<string>() { "Count", "Item[]" };
            List<NotifyCollectionChangedAction> actions = new List<NotifyCollectionChangedAction>();
            List<NotifyCollectionChangedAction> expectedActions = new List<NotifyCollectionChangedAction>() { NotifyCollectionChangedAction.Remove };

            Book book = new Book();
            collection.Add(book);

            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => properties.Add(e.PropertyName);
            collection.CollectionChanged += (s, e) => actions.Add(e.Action);
            collection.Remove(book);

            Assert.AreEqual(0, collection.Count);
            Assert.IsTrue(properties.SequenceEqual(expectedProperties));
            Assert.IsTrue(actions.SequenceEqual(expectedActions));
        }
                
        [TestMethod]
        public void MobileServiceCollectionCanRemoveAtAndNotifies()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();
            query.EnumerableAsyncThrowsException = true;

            MobileServiceCollection<Book> collection = new MobileServiceCollection<Book>(query);

            List<string> properties = new List<string>();
            List<string> expectedProperties = new List<string>() { "Count", "Item[]" };
            List<NotifyCollectionChangedAction> actions = new List<NotifyCollectionChangedAction>();
            List<NotifyCollectionChangedAction> expectedActions = new List<NotifyCollectionChangedAction>() { NotifyCollectionChangedAction.Remove };

            Book book = new Book();
            collection.Add(book);

            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => properties.Add(e.PropertyName);
            collection.CollectionChanged += (s, e) => actions.Add(e.Action);
            collection.RemoveAt(0);

            Assert.AreEqual(0, collection.Count);
            Assert.IsTrue(properties.SequenceEqual(expectedProperties));
            Assert.IsTrue(actions.SequenceEqual(expectedActions));
        }

        [TestMethod]
        public void MobileServiceCollectionCanReplaceAndNotifies()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();
            query.EnumerableAsyncThrowsException = true;

            MobileServiceCollection<Book, Book> collection = new MobileServiceCollection<Book, Book>(query);

            List<string> properties = new List<string>();
            List<string> expectedProperties = new List<string>() { "Item[]" };
            List<NotifyCollectionChangedAction> actions = new List<NotifyCollectionChangedAction>();
            List<NotifyCollectionChangedAction> expectedActions = new List<NotifyCollectionChangedAction>() { NotifyCollectionChangedAction.Replace };

            Book book = new Book();
            Book book2 = new Book();
            collection.Add(book);

            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => properties.Add(e.PropertyName);
            collection.CollectionChanged += (s, e) => actions.Add(e.Action);
            collection[0] = book2;

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(book2, collection[0]);
            Assert.IsTrue(properties.SequenceEqual(expectedProperties));
            Assert.IsTrue(actions.SequenceEqual(expectedActions));
        }

        [TestMethod]
        public void MobileServiceCollectionHasMoreItemsInitiallyTrue()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();
            query.EnumerableAsyncThrowsException = true;

            MobileServiceCollection<Book> collection = new MobileServiceCollection<Book>(query);
            
            Assert.IsTrue(collection.HasMoreItems);
        }

        [AsyncTestMethod]
        public async Task MobileServiceCollectionTotalCountSet()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();

            List<string> properties = new List<string>();
            List<string> expectedProperties = new List<string>() { "HasMoreItems", "Count", "Item[]", "Count", "Item[]", "TotalCount" };
            List<NotifyCollectionChangedAction> actions = new List<NotifyCollectionChangedAction>();
            List<NotifyCollectionChangedAction> expectedActions = new List<NotifyCollectionChangedAction>() { NotifyCollectionChangedAction.Add, NotifyCollectionChangedAction.Add };

            MobileServiceCollection<Book, Book> collection = new MobileServiceCollection<Book, Book>(query);
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => properties.Add(e.PropertyName);
            collection.CollectionChanged += (s, e) => actions.Add(e.Action);
            int result = await collection.LoadMoreItemsAsync(tokenSource.Token);

            Assert.AreEqual((long)2, collection.TotalCount);
            Assert.IsTrue(properties.SequenceEqual(expectedProperties));
            Assert.IsTrue(actions.SequenceEqual(expectedActions));
        }

        [AsyncTestMethod]
        public async Task MobileServiceCollectionHasMoreItemsShouldBeFalseAfterRetrievingDataWhenNoPaging()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();

            MobileServiceCollection<Book> collection = new MobileServiceCollection<Book>(query);
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            List<string> properties = new List<string>();
            List<string> expectedProperties = new List<string>() { "HasMoreItems", "Count", "Item[]", "Count", "Item[]", "TotalCount" };
            List<NotifyCollectionChangedAction> actions = new List<NotifyCollectionChangedAction>();
            List<NotifyCollectionChangedAction> expectedActions = new List<NotifyCollectionChangedAction>() { NotifyCollectionChangedAction.Add, NotifyCollectionChangedAction.Add };

            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => properties.Add(e.PropertyName);
            collection.CollectionChanged += (s, e) => actions.Add(e.Action);
            int result = await collection.LoadMoreItemsAsync(tokenSource.Token);

            Assert.IsFalse(collection.HasMoreItems);
            Assert.IsTrue(properties.SequenceEqual(expectedProperties));
            Assert.IsTrue(actions.SequenceEqual(expectedActions));
        }
    }
}
