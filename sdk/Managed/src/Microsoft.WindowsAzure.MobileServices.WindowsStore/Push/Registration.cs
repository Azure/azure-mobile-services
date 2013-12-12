// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.MobileServices
{
    [KnownType(typeof(TemplateRegistration))]
    [DataContract(Name = "WindowsRegistrationDescription", Namespace = "http://schemas.microsoft.com/netservices/2010/10/servicebus/connect")]
    public class Registration
    {
        private HashSet<string> tags = new HashSet<string>();

        public const string NativeRegistrationName = "$Default";

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

        /// <summary>
        /// If specified, restricts the notifications that the registration will receive to only those that
        /// are annotated with one of the specified <see cref="TagFilter.Tags"/>.
        /// </summary>
        [DataMember(Order = 3, Name = "Tags", IsRequired = false, EmitDefaultValue = false)]
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

        [IgnoreDataMember]
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
        [DataMember(Order = 4, Name = "ChannelUri", IsRequired = true)]
        public string ChannelUri { get; set; }

        /// <summary>
        /// The registration name .
        /// </summary>
        [DataMember(Order = 2, Name = "RegistrationId", IsRequired = false, EmitDefaultValue = false)]
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