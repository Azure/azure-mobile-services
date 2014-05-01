using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfflinePerfCore.Common
{
    public class PerfMeasurement
    {
        public string Name { get; set; }

        public DateTime Timestamp { get; set; }

        public double LocalOperationsLatency { get; set; }

        public long LocalOperationsApplicationMemoryDelta { get; set; }

        public double? SyncOperationLatency { get; set; }

        public double? PullOperationLatency { get; set; }

        public int? XAxisValue { get; set; }
    }
}
