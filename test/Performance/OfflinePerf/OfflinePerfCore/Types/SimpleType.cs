using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OfflinePerfCore.Setup;

namespace OfflinePerfCore.Types
{
    public class SimpleType
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("stringValue")]
        public string StringValue { get; set; }

        [JsonProperty("boolValue")]
        public bool BoolValue { get; set; }

        [JsonProperty("doubleValue")]
        public double DoubleValue { get; set; }

        [JsonProperty("intValue")]
        public int IntValue { get; set; }

        [JsonProperty("dateTimeValue")]
        public DateTime DateTimeValue { get; set; }

        public SimpleType() { }

        public SimpleType(Random rndGen)
        {
            this.StringValue = CreateRandomString(rndGen);
            this.BoolValue = rndGen.Next(2) == 0;
            this.DoubleValue = rndGen.Next() * rndGen.NextDouble();
            this.IntValue = rndGen.Next();
            this.DateTimeValue = CreateRandomDate(rndGen);
        }

        private DateTime CreateRandomDate(Random rndGen)
        {
            return new DateTime(
                rndGen.Next(1900, 2100),
                rndGen.Next(1, 13),
                rndGen.Next(1, 29),
                rndGen.Next(0, 24),
                rndGen.Next(0, 60),
                rndGen.Next(0, 60),
                rndGen.Next(0, 1000),
                DateTimeKind.Utc);
        }

        private const string Letters = "abcdefghijklmnopqrstuvwxyz";
        private static string CreateRandomString(Random rndGen)
        {
            return new string(Enumerable.Range(0, rndGen.Next(5, 10)).Select(_ => Letters[rndGen.Next(Letters.Length)]).ToArray());
        }
    }
}
