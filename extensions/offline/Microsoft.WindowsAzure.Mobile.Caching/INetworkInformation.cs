using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Interface to provide platform specific internet connectivity information.
    /// </summary>
    public interface INetworkInformation
    {
        /// <summary>
        /// Indicates if an internet connection is available.
        /// </summary>
        /// <returns>true if it has internet connection, otherwise false.</returns>
        Task<bool> IsConnectedToInternet();
    }
}
