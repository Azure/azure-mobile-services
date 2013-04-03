using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Management
{
    public class ManagementEndpointDeprecatedException : Exception
    {
        public string Uri { get; set; }

        public ManagementEndpointDeprecatedException(string uri)
            : base(string.Format(CultureInfo.InvariantCulture, Resources.ManagemendEndpointDeprecated, uri))
        {
            this.Uri = uri;
        }
    }
}
