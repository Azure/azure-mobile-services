// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    public sealed class StoreOperationCompletedEvent : StoreChangeEvent
    {
        public const string EventName = "MobileServices.StoreOperationCompleted";

        public StoreOperationCompletedEvent(StoreOperation operation)
        {
            if (operation == null)
            {
                throw new ArgumentNullException("operation");
            }

            Operation = operation;
        }

        public override string Name
        {
            get { return EventName; }
        }

        public StoreOperation Operation { get; private set; }
    }
}
