using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OfflinePerfCore.Common
{
    public class PerfResults
    {
        public string Id { get; set; }

        public string ScenarioName { get; set; }

        public List<PerfMeasurement> Measurements { get; set; }
    }
}
