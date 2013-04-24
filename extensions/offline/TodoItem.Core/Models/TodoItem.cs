using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Todo
{
    public class TodoItem
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("guid", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Guid RealId { get; set; }

        [JsonProperty("timestamp", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Timestamp { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("complete")]
        public bool Complete { get; set; }
    }
}
