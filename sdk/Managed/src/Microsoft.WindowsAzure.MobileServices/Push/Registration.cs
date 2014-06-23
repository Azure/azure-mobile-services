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
    [JsonObject(MemberSerialization.OptIn)]
    public class Registration
    {
        internal const string NativeRegistrationName = "$Default";

        internal Registration()
        {
        }

        /// <summary>
        /// Common Registration constructor for common properties
        /// </summary>
        public Registration(string deviceId, IEnumerable<string> tags)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                throw new ArgumentNullException("deviceId");
            }

            if (tags != null)
            {
                if (tags.Any(s => s.Contains(",")))
                {
                    throw new ArgumentException(Resources.Push_TagNoCommas, "tags");
                }
            }

            this.PushHandle = deviceId;
            this.Tags = tags != null ? new HashSet<string>(tags) : new HashSet<string>();
        }

        /// <summary>
        /// The platform's name for its push notifcation system.
        /// </summary>
        [JsonProperty(PropertyName = "platform")]
        public string Platform
        {
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
        public IEnumerable<string> Tags { get; private set; }

        /// <summary>
        /// The push handle used to address the device by the push notification service (Possibly nonunique)
        /// </summary>
        [JsonProperty(PropertyName = "deviceId")]
        public string PushHandle { get; internal set; }

        /// <summary>
        /// The registration id.
        /// </summary>
        [JsonProperty(PropertyName = "registrationId")]
        public string RegistrationId { get; internal set; }        

        /// <summary>
        /// The name of the registration is stored locally with the registrationId
        /// </summary>
        public virtual string Name
        {
            get
            {
                return NativeRegistrationName;
            }
        }

        /// <summary>
        /// Internal--Helper method hinting to Json.Net that RegistrationId should not be serialized
        /// </summary>
        /// <returns>false</returns>
        public bool ShouldSerializeRegistrationId()
        {
            return false;
        }
    }
}
