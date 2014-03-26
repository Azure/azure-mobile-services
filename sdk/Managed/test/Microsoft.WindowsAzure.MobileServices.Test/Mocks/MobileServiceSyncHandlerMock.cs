// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    class MobileServiceSyncHandlerMock: MobileServiceSyncHandler
    {
        public Func<IMobileServiceTableOperation, Task<JObject>> TableOperationAction { get; set; }
        public Func<MobileServicePushCompletionResult, Task> PushCompleteAction { get; set; }

        public MobileServicePushCompletionResult PushCompletionResult { get; set; }

        public override Task OnPushCompleteAsync(MobileServicePushCompletionResult result)
        {
            this.PushCompletionResult = result;
            if (this.PushCompleteAction != null)
            {
                return PushCompleteAction(result);
            }
            else
            {
                return base.OnPushCompleteAsync(result);
            }
        }

        public override Task<JObject> ExecuteTableOperationAsync(IMobileServiceTableOperation operation)
        {
            if (TableOperationAction != null)
            {
                return this.TableOperationAction(operation);
            }
            else
            {
                return base.ExecuteTableOperationAsync(operation);
            }
        }
    }
}
