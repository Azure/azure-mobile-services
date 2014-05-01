using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using OfflinePerfCore.Common;
using OfflinePerfCore.Setup;
using OfflinePerfCore.Types;

namespace OfflinePerfCore.Tests
{
    public class HeavyInsertUpdateScenarioSingleTable : IPerfTest
    {
        private static readonly int[] ItemsInLocalTable = new[] { 100, 200, 400, 800, 1600 }; //, 3200 };
        private const int OperationsToExecute = 200;

        public async Task<PerfResults> Execute(IPlatformInfo platformInfo, string dbFileName)
        {
            var result = new PerfResults();
            result.ScenarioName = "Heavy insert/update - single table";
            result.Measurements = new List<PerfMeasurement>();
            var client = await TestSetup.CreateClient(dbFileName);
            var syncTable = client.GetSyncTable<SimpleType>();

            foreach (var itemsInLocalTable in ItemsInLocalTable)
            {
                System.GC.Collect();
                var initialMemory = platformInfo.GetAppMemoryUsage();
                var measurement = await PerformUpdates(client, syncTable, itemsInLocalTable, platformInfo);
                result.Measurements.Add(measurement);
            }

            return result;
        }

        private async Task<PerfMeasurement> PerformUpdates(
            MobileServiceClient client, IMobileServiceSyncTable<SimpleType> syncTable, int elementsInLocalTable, IPlatformInfo platform)
        {
            await this.PullItemsLocally(syncTable, elementsInLocalTable);
            var idsInLocalTable = await syncTable.Select(st => st.Id).ToListAsync();
            Random rndGen = new Random();
            var initialApplicationMemory = platform.GetAppMemoryUsage();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < OperationsToExecute; i++)
            {
                if (rndGen.Next(4) == 0)
                {
                    // 25% inserts
                    var item = new SimpleType(rndGen);
                    await syncTable.InsertAsync(item);
                }
                else
                {
                    // 75% updates
                    var id = idsInLocalTable[rndGen.Next(idsInLocalTable.Count)];
                    var item = await syncTable.LookupAsync(id);
                    item.DoubleValue = rndGen.Next() * rndGen.NextDouble();
                    await syncTable.UpdateAsync(item);
                }
            }

            sw.Stop();
            var localOperationsLatency = sw.ElapsedMilliseconds * 1.0 / OperationsToExecute;
            sw.Restart();
            await client.SyncContext.PushAsync();
            sw.Stop();
            var syncLatency = sw.ElapsedMilliseconds;

            return new PerfMeasurement
            {
                Name = "Insert / updates for local table with " + elementsInLocalTable + " elements",
                Timestamp = DateTime.UtcNow,
                XAxisValue = elementsInLocalTable,
                LocalOperationsLatency = localOperationsLatency,
                LocalOperationsApplicationMemoryDelta = platform.GetAppMemoryUsage() - initialApplicationMemory,
                SyncOperationLatency = syncLatency
            };
        }

        private async Task PullItemsLocally(IMobileServiceSyncTable<SimpleType> syncTable, int elementCount)
        {
            await syncTable.PurgeAsync();
            var elementsInLocalTable = 0;
            while (elementsInLocalTable < elementCount)
            {
                var toPull = elementCount - elementsInLocalTable;
                if (toPull > 1000)
                {
                    // Maximum 1000 items can be pulled at once
                    toPull = 1000;
                }

                await syncTable.PullAsync(syncTable.Skip(elementsInLocalTable).Take(toPull).OrderBy(st => st.Id));
                elementsInLocalTable += toPull;
            }
        }
    }
}
