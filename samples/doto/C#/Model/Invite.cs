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
    /// Represents an invite requesting another user to join a list
    /// as stored in Mobile Services
    /// </summary>
    [DataTable(Name = "invites")]
    public class Invite
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "fromUserId")]
        public string FromUserId { get; set; }

        [DataMember(Name = "toUserId")]
        public string ToUserId { get; set; }

        [DataMember(Name = "fromUserName")]
        public string FromUserName { get; set; }

        [DataMember(Name = "fromImageUrl")]
        public string FromImageUrl { get; set; }

        [DataMember(Name = "toUserName")]
        public string ToUserName { get; set; }

        [DataMember(Name = "listName")]
        public string ListName { get; set; }

        [DataMember(Name = "listId")]
        public string ListId { get; set; }

        [DataMember(Name = "approved")]
        public bool Approved { get; set; }

    }
}
