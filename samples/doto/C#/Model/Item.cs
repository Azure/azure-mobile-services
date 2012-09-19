using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;

namespace Doto.Model
{
    /// <summary>
    /// Represents a Todo (or Doto) item stored in Mobile Services
    /// </summary>
    [DataTable(Name = "items")]
    public class Item
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "listId")]
        public string ListId { get; set; }

        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "created")]
        public DateTime Created { get; set; }

        [DataMember(Name = "createdBy")]
        public string CreatedBy { get; set; }
    }
}
