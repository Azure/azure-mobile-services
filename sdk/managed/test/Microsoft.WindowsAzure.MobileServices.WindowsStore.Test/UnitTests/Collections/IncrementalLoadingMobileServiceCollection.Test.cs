﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Test;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Windows.UI.Xaml.Data;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("collection")]
    [Tag("unit")]
    public class MobileServiceCollectionTest : TestBase
    {
        [AsyncTestMethod]
        public async Task MobileServiceCollectionLoadMoreItemsAsyncExceptionsCanBeHandled()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();
            query.EnumerableAsyncThrowsException = true;

            MobileServiceIncrementalLoadingCollection<Book, Book> collection = new MobileServiceIncrementalLoadingCollection<Book, Book>(query);
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            Exception ex = null;

            try
            {
                await ((ISupportIncrementalLoading)collection).LoadMoreItemsAsync(10);
            }
            catch (Exception e)
            {
                ex = e;
            }

            Assert.IsNotNull(ex);
        }        
    }
}
