﻿// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Registration is used to define a target that is registered for notifications
    /// </summary>
    public class MpnsRegistration : Registration
    {
        internal MpnsRegistration()
        {
        }

        /// <summary>
        /// Create a default Registration for a channelUri
        /// </summary>
        /// <param name="channelUri">The channel uri</param>
        public MpnsRegistration(string channelUri)
            : this(channelUri, null)
        {
        }

        /// <summary>
        /// Create a default Registration for a channelUri with specific tags
        /// </summary>
        /// <param name="channelUri">The channel uri</param>
        /// <param name="tags">The tags to register to receive notifications from</param>
        public MpnsRegistration(string channelUri, IEnumerable<string> tags)
            : base(channelUri, tags)
        {
            if (string.IsNullOrWhiteSpace(channelUri))
            {
                throw new ArgumentNullException("channelUri");
            }
        }

        /// <summary>
        /// The Uri of the Channel returned by the Push Notification Channel Manager.
        /// </summary>
        public string ChannelUri
        {
            get
            {
                return this.PushHandle;
            }

            set
            {
                this.PushHandle = value;
            }
        }
    }
}