using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;

namespace OfflinePerfCore.Common
{
    public class ResultsRepository
    {
        public static async Task UploadResults(string appUrl, string appKey, PerfResults results, IPlatformInfo platformInfo)
        {
            var client = new MobileServiceClient(appUrl, appKey);
            var table = client.GetTable("PerfResults");
            foreach (var measurement in results.Measurements)
            {
                var item = new JObject(
                    new JProperty("scenario", results.ScenarioName),
                    new JProperty("name", measurement.Name),
                    new JProperty("pullLatency", measurement.PullOperationLatency),
                    new JProperty("localOperationLatency", measurement.LocalOperationsLatency),
                    new JProperty("localOperationAppMemory", measurement.LocalOperationsApplicationMemoryDelta),
                    new JProperty("syncOperationLatency", measurement.SyncOperationLatency),
                    new JProperty("platform", platformInfo.PlatformName)
                    );
                if (measurement.XAxisValue.HasValue)
                {
                    item.Add("xAxisValue", measurement.XAxisValue.Value);
                }

                await table.InsertAsync(item);
            }
        }

        public static async Task UploadError(string appUrl, string appKey, JObject error)
        {
            var client = new MobileServiceClient(appUrl, appKey);
            var table = client.GetTable("errors");
            await table.InsertAsync(error);
        }
    }
}
