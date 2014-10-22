// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Threading
{
    internal class AsyncReaderWriterLock
    {
        private readonly Task<DisposeAction> readerReleaser;
        private readonly Task<DisposeAction> writerReleaser;
        private readonly Queue<TaskCompletionSource<DisposeAction>> waitingWriters = new Queue<TaskCompletionSource<DisposeAction>>(); 
        private TaskCompletionSource<DisposeAction> waitingReader = new TaskCompletionSource<DisposeAction>(); 
        private int readersWaiting;

        private int lockStatus; // -1 means write lock, >=0 no. of read locks

        public AsyncReaderWriterLock()
        {
            this.readerReleaser = Task.FromResult(new DisposeAction(this.ReaderRelease));
            this.writerReleaser = Task.FromResult(new DisposeAction(this.WriterRelease)); 
        }

        public Task<DisposeAction> ReaderLockAsync() 
        {
            lock (this.waitingWriters) 
            {
                bool hasPendingReaders = this.lockStatus >= 0;
                bool hasNoPendingWritiers = this.waitingWriters.Count == 0;
                if (hasPendingReaders && hasNoPendingWritiers) 
                {
                    ++this.lockStatus;
                    return this.readerReleaser; 
                } 
                else 
                {
                    ++this.readersWaiting;
                    return this.waitingReader.Task.ContinueWith(t => t.Result);
                } 
            } 
        }

        public Task<DisposeAction> WriterLockAsync() 
        {
            lock (this.waitingWriters) 
            {
                bool hasNoPendingReaders = this.lockStatus == 0;
                if (hasNoPendingReaders) 
                {
                    this.lockStatus = -1;
                    return this.writerReleaser; 
                } 
                else 
                { 
                    var waiter = new TaskCompletionSource<DisposeAction>();
                    this.waitingWriters.Enqueue(waiter); 
                    return waiter.Task; 
                } 
            } 
        }

        private void ReaderRelease() 
        { 
            TaskCompletionSource<DisposeAction> toWake = null;

            lock (this.waitingWriters) 
            {
                --this.lockStatus;
                if (this.lockStatus == 0 && this.waitingWriters.Count > 0) 
                {
                    this.lockStatus = -1;
                    toWake = this.waitingWriters.Dequeue(); 
                } 
            }

            if (toWake != null) 
            {
                toWake.SetResult(new DisposeAction(this.WriterRelease)); 
            }
        }
        private void WriterRelease()
        {
            TaskCompletionSource<DisposeAction> toWake = null;
            Action wakeupAction = this.ReaderRelease;

            lock (this.waitingWriters)
            {
                if (this.waitingWriters.Count > 0)
                {
                    toWake = this.waitingWriters.Dequeue();
                    wakeupAction = this.WriterRelease;
                }
                else if (this.readersWaiting > 0)
                {
                    toWake = this.waitingReader;
                    this.lockStatus = this.readersWaiting;
                    this.readersWaiting = 0;
                    this.waitingReader = new TaskCompletionSource<DisposeAction>();
                }
                else
                {
                    this.lockStatus = 0;
                }
            }

            if (toWake != null)
            {
                toWake.SetResult(new DisposeAction(wakeupAction));
            }
        }
    }
}
