using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;

namespace OfflinePerfCore.Common
{
    public interface IPerfTest
    {
        Task<PerfResults> Execute(IPlatformInfo platformInfo, string dbFileName);
    }
}
