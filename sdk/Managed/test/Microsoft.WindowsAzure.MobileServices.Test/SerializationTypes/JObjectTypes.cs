
using System;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    public class JObjectTypes
    {
        public static JObject GetObjectWithAllTypes()
        {
            return new JObject()
            {
                { "Object", new JObject() },
                { "Array", new JArray() },
                { "Integer", 0L },
                { "Float", 0f },
                { "String", String.Empty },
                { "Boolean", false },
                { "Date", DateTime.MinValue },
                { "Bytes", new byte[0] },
                { "Guid", Guid.Empty },
                { "TimeSpan", TimeSpan.Zero }
            };
        }
    }
}
