using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace Microsoft.WindowsAzure.MobileServices
{
    public class NetworkInformation : INetworkInformation
    {
        public Task<bool> IsConnectedToInternet()
        {
            return Task.Run(() =>
            {
                ConnectionProfile connections = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();
                bool internet = connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
                return internet;
            });
        }
    }
}
