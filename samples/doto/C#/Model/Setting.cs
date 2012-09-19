using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Doto.Model
{
    /// <summary>
    /// A class used to Store simple app settings in Mobile Services
    /// </summary>
    [DataTable(Name = "settings")]
    public class Setting
    {
        public int Id { get; set; }

        [DataMember(Name = "key")]
        public string Key { get; set; }

        [DataMember(Name = "value")]
        public string Value { get; set; }
    }
}
