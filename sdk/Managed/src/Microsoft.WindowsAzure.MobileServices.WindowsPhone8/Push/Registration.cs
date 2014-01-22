// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Linq;

using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Registration is used to define a target that is registered for notifications
    /// </summary>
    [KnownType(typeof(TemplateRegistration))]
    [JsonObject(MemberSerialization.OptIn)]
    public class Registration
    {
        internal const string NativeRegistrationName = "$Default";

        internal Registration()
        {            
        }

        /// <summary>
        /// Create a default Registration for a channelUri
        /// </summary>
        /// <param name="channelUri">The channel uri</param>
        public Registration(string channelUri)
            : this(channelUri, null)
        {            
        }

        /// <summary>
        /// Create a default Registration for a channelUri with specific tags
        /// </summary>
        /// <param name="channelUri">The channel uri</param>
        /// <param name="tags">The tags to register to receive notifications from</param>
        public Registration(string channelUri, IEnumerable<string> tags)            
        {
            this.ChannelUri = channelUri;
            this.Tags = tags != null ? new HashSet<string>(tags) : new HashSet<string>();

            this.Validate();
        }

        [JsonProperty(PropertyName = "platform")]
        internal string Platform
        {
            get
            {
                return "mpns";
            }
        }                

        /// <summary>
        /// If specified, restricts the notifications that the registration will receive to only those that
        /// are annotated with one of the specified tags.
        /// </summary>
        [JsonProperty(PropertyName = "tags")]
        public ISet<string> Tags { get; set; }
        
        /// <summary>
        /// The Uri of the Channel returned by the Push Notification Channel Manager.
        /// </summary>
        [JsonProperty(PropertyName = "deviceId")]
        public string ChannelUri { get; set; }

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
                return NativeRegistrationName;
            }
        }

        internal virtual void Validate()
        {
            if (string.IsNullOrWhiteSpace(this.ChannelUri))
            {
                throw new ArgumentNullException("channelUri");
            }

            if (this.Tags != null)
            {
                if (this.Tags.Any(s => s.Contains(',')))
                {
                    // TODO: Resource
                    throw new ArgumentException("Tags must not contain ','.");
                }
            }

            if (this.Name.Contains(':'))
            {
                // TODO: Resource
                throw new ArgumentException("Name must not contain a ':'.");
            }

            if (this.Name.Contains(';'))
            {
                // TODO: Resource
                throw new ArgumentException("Name must not contain a ';'.");
            }
        }
    }
}