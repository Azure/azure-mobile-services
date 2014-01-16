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
    /// <summary>
    /// Queue for executing asynchronous tasks in a first-in-first-out fashion.
    /// </summary>
    internal class ActionBlock
    {
        private object syncRoot = new object();
        private Task lastTask;

        public ActionBlock()
        {
            lastTask = Task.FromResult(0);
        }

        public void Post(Func<Task> action)
        {
            lock (syncRoot)
            {
                this.lastTask = this.lastTask.ContinueWith(_ => action());
            }
        }
    }
}
