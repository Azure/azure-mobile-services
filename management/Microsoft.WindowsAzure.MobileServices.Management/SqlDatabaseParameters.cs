using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Management
{
    public class SqlDatabaseParameters
    {
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public string Location { get; set; }
        public string AdministratorLogin { get; set; }
        public bool IsFreeDB { get; set; }

        public SqlDatabaseParameters()
        { 
        }
    }
}
