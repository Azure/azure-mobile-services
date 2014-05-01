using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;

namespace OfflinePerfCore.Setup
{
    class BypassNetworkSyncHandler : IMobileServiceSyncHandler
    {
        public Task<JObject> ExecuteTableOperationAsync(IMobileServiceTableOperation operation)
        {
            JObject result;
            switch (operation.Kind)
            {
                case MobileServiceTableOperationKind.Update:
                case MobileServiceTableOperationKind.Insert:
                    result = operation.Item as JObject;
                    break;
                default:
                    result = null;
                    break;
            }

            return Task.FromResult(result);
        }

        public Task OnPushCompleteAsync(MobileServicePushCompletionResult result)
        {
            return Task.FromResult(0);
        }
    }
}
