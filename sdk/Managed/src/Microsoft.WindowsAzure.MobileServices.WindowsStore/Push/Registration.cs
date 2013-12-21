// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

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
    [KnownType(typeof(TemplateRegistration))]
    [JsonObject(MemberSerialization.OptIn)]
    public class Registration
    {
        private HashSet<string> tags = new HashSet<string>();

        public const string NativeRegistrationName = "$Default";

        internal Registration()
        {            
        }

        public Registration(string channelUri)
        {
            if (string.IsNullOrWhiteSpace(channelUri))
            {
                throw new ArgumentNullException("channelUri");
            }

            this.ChannelUri = channelUri;
        }

        public Registration(string channelUri, IEnumerable<string> tags)
            : this(channelUri)
        {
            if (tags != null)
            {
                this.Tags = new HashSet<string>(tags);
            }
        }

        [JsonProperty(PropertyName = "platform")]
        internal string Platform
        {
            get
            {
                return "wns";
            }
        }

        /// <summary>
        /// If specified, restricts the notifications that the registration will receive to only those that
        /// are annotated with one of the specified <see cref="TagFilter.Tags"/>.
        /// </summary>
        [JsonProperty(PropertyName = "tags")]
        internal string TagsString
        {
            get
            {
                if (this.tags != null)
                {
                    return this.tags.Count > 0 ? string.Join(",", this.tags) : null;
                }

                return string.Empty;
            }
            set
            {
                this.tags = value != null ? new HashSet<string>(value.Split(',')) : new HashSet<string>();
            }
        }

        public ISet<string> Tags
        {
            get
            {
                return this.tags;
            }
            set
            {
                if (value == null)
                {
                    this.TagsString = null;
                    return;
                }

                this.TagsString = string.Join(",", value);
            }
        }

        /// <summary>
        /// The Uri of the Channel returned by the Push Notification Channel Manager.
        /// </summary>
        [JsonProperty(PropertyName = "channelUri")]
        public string ChannelUri { get; set; }

        /// <summary>
        /// The registration id.
        /// </summary>
        [JsonProperty]
        public string RegistrationId { get; internal set; }

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