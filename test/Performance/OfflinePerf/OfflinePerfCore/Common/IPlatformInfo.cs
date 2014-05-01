using System;

namespace OfflinePerfCore.Common
{
    public interface IPlatformInfo
    {
        long GetAppMemoryUsage();

        string PlatformName { get; }
    }
}
