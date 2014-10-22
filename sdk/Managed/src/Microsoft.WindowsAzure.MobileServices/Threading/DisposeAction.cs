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
    internal struct DisposeAction: IDisposable
    {
        bool isDisposed;
        private Action action;

        public DisposeAction(Action action)
        {
            this.isDisposed = false;
            this.action = action;
        }

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;
            this.action();
        }
    }
}
