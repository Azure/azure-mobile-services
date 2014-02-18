// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

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

        internal const string PlatformConstant = "wns";

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

            if (tags != null)
            {
                if (tags.Any(s => s.Contains(",")))
                {
                    throw new ArgumentException(Resources.Push_TagNoCommas, "tags");
                }
            }

            this.ChannelUri = channelUri;
            this.Tags = tags != null ? new HashSet<string>(tags) : new HashSet<string>();
        }

        [JsonProperty(PropertyName = "platform")]
        internal string Platform
        {
            get
            {
                return PlatformConstant;
            }
        }

        /// <summary>
        /// If specified, restricts the notifications that the registration will receive to only those that
        /// are annotated with one of the specified tags. Note that a tag with a comma in it will be split into two tags.
        /// </summary>
        [JsonProperty(PropertyName = "tags")]
        public ISet<string> Tags { get; internal set; }

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

        internal virtual string Name
        {
            get
            {
                return NativeRegistrationName;
            }
        }
    }
}