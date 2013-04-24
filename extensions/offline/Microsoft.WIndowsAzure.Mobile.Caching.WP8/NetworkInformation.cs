using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    internal class NetworkInformation : INetworkInformation
    {
        public Task<bool> IsConnectedToInternet()
        {
            return Task.Run(() => Microsoft.Phone.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable());
        }
    }
}
