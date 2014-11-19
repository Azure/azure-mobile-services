// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MobileServices.Threading;

namespace Microsoft.WindowsAzure.MobileServices.Test.Unit.Threading
{
    [TestClass]
    public class AsyncLockDictionaryTests
    {
        [TestMethod, Timeout(2000)]
        public async Task Dispose_ReleasesLock()
        {
            var dictionary = new AsyncLockDictionary();
            // first take a lock and wait
            IDisposable releaser1 = await dictionary.Acquire("key", CancellationToken.None);
            // then take the second lock before releasing first so that this one blocks
            Task<IDisposable> releaser2 = dictionary.Acquire("key", CancellationToken.None);
            Assert.AreEqual(TaskStatus.WaitingForActivation, releaser2.Status);
            // release first lock
            releaser1.Dispose();
            // second lock should now be aquired
            await releaser2;
            // finally release the second lock
            releaser2.Dispose();
        }
    }
}
