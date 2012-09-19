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
    /// A class that represents a user's membership of the list. Note that 
    /// there is no 'master' list class or table. The name of the list is stored
    /// redundantly across all listMember records.
    /// </summary>
    [DataTable(Name = "listMembers")]
    public class ListMembership
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "userId")]
        public string UserId { get; set; }

        [DataMember(Name = "listId")]
        public string ListId { get; set; }

    }
}
