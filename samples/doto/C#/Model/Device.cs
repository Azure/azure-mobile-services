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
    /// Used to store the channel url for each user and installation in Mobile Services
    /// </summary>
    [DataTable(Name = "devices")]
    public class Device
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "userId")]
        public string UserId { get; set; }

        [DataMember(Name = "installationId")]
        public string InstallationId { get; set; }

        [DataMember(Name = "channelUri")]
        public string ChannelUri { get; set; }

    }
}
