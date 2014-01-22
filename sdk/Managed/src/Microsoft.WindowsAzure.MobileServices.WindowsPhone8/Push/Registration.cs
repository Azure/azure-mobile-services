// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Linq;

using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices
{
    // {
    // platform: "wns" // {"wns"|"mpns"|"apns"|"gcm"}
    // channelUri: "" // if wns or mpns
    // deviceToken: "" // if apns
    // gcmRegistrationId: "" // if gcm
    // tags: "tag"|["a","b"] // non-empty string or array of tags (optional)
    // bodyTemplate: '<toast>
    //      <visual lang="en-US">
    //        <binding template="ToastText01">
    //          <text id="1">$(myTextProp1)</text>
    //        </binding>
    //      </visual>
    //    </toast>' // if template registration
    // templateName: "" // if template registration
    // wnsHeaders: { // if wns template registration }
    // mpnsHeaders: { // if mpns template //}
    // expiry: "" // if apns template//
    // }
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
            if (string.IsNullOrWhiteSpace(channelUri))
            {
                throw new ArgumentNullException("channelUri");
            }

            this.ChannelUri = channelUri;
            this.Tags = tags != null ? new HashSet<string>(tags) : new HashSet<string>();
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
    }
}