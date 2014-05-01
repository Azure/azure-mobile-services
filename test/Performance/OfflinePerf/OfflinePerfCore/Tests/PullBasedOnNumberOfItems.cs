using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices;
using OfflinePerfCore.Common;
using OfflinePerfCore.Setup;
using OfflinePerfCore.Types;

namespace OfflinePerfCore.Tests
{
    public class PullBasedOnNumberOfItems : IPerfTest
    {
        private const int TotalItemsToPull = 20000;
        private const int ItemsToPullEachOperation = 100;
        public async Task<PerfResults> Execute(IPlatformInfo platformInfo, string dbFileName)
        {
            var result = new PerfResults();
            result.ScenarioName = "Pull on growing table";
            var numberOfOperations = TotalItemsToPull / ItemsToPullEachOperation;
            var measurements = Enumerable.Range(0, numberOfOperations).Select(_ => new PerfMeasurement()).ToArray();
            var client = await TestSetup.CreateClient(dbFileName);
            try
            {
                var syncTable = client.GetSyncTable<SimpleType>();
                var itemsInLocalTable = 0;
                await syncTable.PurgeAsync();

                // Warm up
                await syncTable.PullAsync(syncTable.Take(1));
                await syncTable.PurgeAsync();

                var items2 = await syncTable.Take(1).IncludeTotalCount().ToEnumerableAsync();
                var totalCount2 = ((ITotalCountProvider)items2).TotalCount;
                System.Diagnostics.Debug.WriteLine("" + totalCount2);

                for (var i = 0; i < numberOfOperations; i++)
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    await syncTable.PullAsync(syncTable.Skip(itemsInLocalTable).Take(ItemsToPullEachOperation));
                    sw.Stop();

                    //var items = await syncTable.Take(1).IncludeTotalCount().ToEnumerableAsync();
                    //var totalCount = ((ITotalCountProvider)items).TotalCount;

                    measurements[i].PullOperationLatency = sw.ElapsedMilliseconds;
                    measurements[i].XAxisValue = itemsInLocalTable;
                    measurements[i].Timestamp = DateTime.UtcNow;
                    measurements[i].Name = string.Format("Pulling {0} items into a table with {1} rows", ItemsToPullEachOperation, itemsInLocalTable);
                    itemsInLocalTable += ItemsToPullEachOperation;

                    //if (totalCount != itemsInLocalTable)
                    //{
                    //    throw new Exception(string.Format("Error, already inserted {0} items in the local table, but there are only {1}", itemsInLocalTable, totalCount));
                    //}
                }
            }
            finally
            {
                if (client != null)
                {
                    client.Dispose();
                }
            }

            result.Measurements = measurements.ToList();
            return result;
        }
    }
}
