using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    public class ToggableNetworkInformation : INetworkInformation
    {
        public async Task<bool> IsConnectedToInternet()
        {
            return IsOnline;
        }

        public bool IsOnline { get; set; }
    }
}
