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
    public class HeavyReadOffline : IPerfTest
    {
        private const int OperationsToExecute = 20;
        private static readonly int[] ElementCounts = new[] { 100, 200, 400, 800, 1600 }; //, 3200 };

        public async Task<PerfResults> Execute(IPlatformInfo platformInfo, string dbFileName)
        {
            var result = new PerfResults();
            result.ScenarioName = "Read operations";
            result.Measurements = new List<PerfMeasurement>();
            var client = await TestSetup.CreateClient(dbFileName);
            var syncTable = client.GetSyncTable<SimpleType>();

            // Warm up...
            var discarded = await PerformReads(syncTable, 100, platformInfo);

            foreach (var elementCount in ElementCounts)
            {
                System.GC.Collect();
                var measurement = await PerformReads(syncTable, elementCount, platformInfo);
                result.Measurements.Add(measurement);
            }

            client.Dispose();
            return result;
        }

        private async Task<PerfMeasurement> PerformReads(IMobileServiceSyncTable<SimpleType> syncTable, int elementsInLocalTable, IPlatformInfo platform)
        {
            var pullLatency = await this.PullItemsLocally(syncTable, elementsInLocalTable);
            Random rndGen = new Random();
            Stopwatch sw = new Stopwatch();
            var initialApplicationMemory = platform.GetAppMemoryUsage();
            sw.Start();
            for (var i = 0; i < OperationsToExecute; i++)
            {
                await syncTable.Where(st => st.IntValue == rndGen.Next(Constants.MinNumber, Constants.MaxNumber)).ToListAsync();
            }

            sw.Stop();
            return new PerfMeasurement
            {
                Name = "Reads for local table with " + elementsInLocalTable + " elements",
                Timestamp = DateTime.UtcNow,
                XAxisValue = elementsInLocalTable,
                LocalOperationsLatency = sw.ElapsedMilliseconds * 1.0 / OperationsToExecute,
                LocalOperationsApplicationMemoryDelta = platform.GetAppMemoryUsage() - initialApplicationMemory,
                PullOperationLatency = pullLatency
            };
        }

        private async Task<double> PullItemsLocally(IMobileServiceSyncTable<SimpleType> syncTable, int elementCount)
        {
            await syncTable.PurgeAsync();
            var elementsInLocalTable = 0;
            var sw = new Stopwatch();
            sw.Start();
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

            sw.Stop();
            var totalItems = await syncTable.IncludeTotalCount().Take(1).ToListAsync();
            System.Diagnostics.Debug.WriteLine("Count: " + ((ITotalCountProvider)totalItems).TotalCount);
            return sw.ElapsedMilliseconds;
        }
    }
}
