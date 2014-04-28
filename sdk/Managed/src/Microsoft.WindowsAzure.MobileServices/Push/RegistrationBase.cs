// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// RegistrationBase is used as the base class for common properties existing in all Registration types to define a target that is registered for notifications.
    /// </summary>
    public class RegistrationBase
    {
        internal RegistrationBase()
        {
        }

        /// <summary>
        /// Common Registration constructor for common properties
        /// </summary>
        internal RegistrationBase(string deviceId, IEnumerable<string> tags)
        {            
            if (tags != null)
            {
                if (tags.Any(s => s.Contains(",")))
                {
                    throw new ArgumentException(Resources.Push_TagNoCommas, "tags");
                }
            }

            this.DeviceId = deviceId;
            this.Tags = tags != null ? new HashSet<string>(tags) : new HashSet<string>();
        }

        [JsonProperty(PropertyName = "platform")]
        internal string Platform {
            get
            {
                return Microsoft.WindowsAzure.MobileServices.Platform.Instance.PushUtility.GetPlatform();
            }
        }

        /// <summary>
        /// If specified, restricts the notifications that the registration will receive to only those that
        /// are annotated with one of the specified tags. Note that a tag with a comma in it will be split into two tags.
        /// </summary>
        [JsonProperty(PropertyName = "tags")]
        public ISet<string> Tags { get; private set; }

        /// <summary>
        /// The unique identifier for the device
        /// </summary>
        [JsonProperty(PropertyName = "deviceId")]
        internal string DeviceId { get; set; }

        /// <summary>
        /// The registration id.
        /// </summary>
        [JsonProperty(PropertyName = "registrationId")]
        public string RegistrationId { get; internal set; }

        /// <summary>
        /// Internal--Helper method hinting to Json.Net that RegistrationId should not be serialized
        /// </summary>
        /// <returns>false</returns>
        public bool ShouldSerializeRegistrationId() { return false; }

        internal virtual string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
