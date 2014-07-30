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
    internal class AsyncLockDictionary
    {
        sealed class LockEntry: IDisposable
        {
            public int Count;
            public readonly AsyncLock Lock = new AsyncLock();

            public void Dispose()
            {
                this.Lock.Dispose();
            }
        }

        Dictionary<string, LockEntry> locks = new Dictionary<string, LockEntry>();

        public async Task<IDisposable> Acquire(string key, CancellationToken cancellationToken)
        {
            LockEntry entry;

            lock(locks)
            {
                if (!locks.TryGetValue(key, out entry))
                {
                    locks[key] = entry = new LockEntry();
                }
                entry.Count++;                
            }

            IDisposable releaser = await entry.Lock.Acquire(cancellationToken);

            return new DisposeAction(() =>
            {
                lock(locks)
                {
                    entry.Count--;
                    releaser.Dispose();
                    if (entry.Count == 0)
                    {
                        this.locks.Remove(key);
                        entry.Dispose();
                    }
                }
            });
        }
    }
}
