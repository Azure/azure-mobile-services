// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Threading
{
    internal sealed class AsyncLock : IDisposable
    {
        private SemaphoreSlim semaphore = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        public async Task<IDisposable> Acquire(CancellationToken cancellationToken)
        {
            await this.semaphore.WaitAsync(cancellationToken)
                                .ConfigureAwait(continueOnCapturedContext: false);

            return new DisposeAction(() => this.semaphore.Release());
        }

        public void Dispose()
        {
            this.semaphore.Dispose();
        }
    }
}
