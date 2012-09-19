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
    /// A class used to store information about users in Mobile Services
    /// </summary>
    [DataTable(Name = "profiles")]
    public class Profile : ViewModel
    {
        private int _id;

        [DataMember(Name = "id")]
        public int Id
        {
            get { return _id; }
            set
            {
                SetValue(ref _id, value, "Id");
            }
        }

        private string _name;

        [DataMember(Name = "name")]
        public string Name
        {
            get { return _name; }
            set
            {
                SetValue(ref _name, value, "Name");
            }
        }

        private string _userId;

        [DataMember(Name = "userId")]
        public string UserId
        {
            get { return _userId; }
            set
            {
                SetValue(ref _userId, value, "UserId");
            }
        }

        private string _city;

        [DataMember(Name = "city")]
        public string City
        {
            get { return _city; }
            set
            {
                SetValue(ref _city, value, "City");
            }
        }

        private string _state;

        [DataMember(Name = "state")]
        public string State
        {
            get { return _state; }
            set
            {
                SetValue(ref _state, value, "State");
            }
        }

        private string _created;

        [DataMember(Name = "created")]
        public string Created
        {
            get { return _created; }
            set
            {
                SetValue(ref _created, value, "Created");
            }
        }

        private string _imageUrl;

        [DataMember(Name = "imageUrl")]
        public string ImageUrl
        {
            get { return _imageUrl; }
            set
            {
                SetValue(ref _imageUrl, value, "ImageUrl");
            }
        }
    }
}
