using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    public interface IStructuredStorage
    {
        Task<IEnumerable<IDictionary<string, JToken>>> GetStoredData(string tableName, IQueryOptions query);

        Task StoreData(string tableName, IEnumerable<IDictionary<string, JToken>> data);

        Task RemoveStoredData(string tableName, IEnumerable<string> guids);
    }
}
