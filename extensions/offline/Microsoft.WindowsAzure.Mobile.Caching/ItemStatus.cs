using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    public enum ItemStatus
    {
        Unchanged = 0,
        Inserted = 1,
        Changed = 2,
        Deleted = 3,
    }
}
