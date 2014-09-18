// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Registration is used to define a target that is registered for notifications
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ApnsRegistration : Registration
    {
        internal ApnsRegistration()
        {
        }

        /// <summary>
        /// Create a default Registration for a deviceToken
        /// </summary>
        /// <param name="deviceToken">The device token</param>
        public ApnsRegistration(string deviceToken)
            : this(deviceToken, null)
        {
        }

        /// <summary>
        /// Create a default Registration for a deviceToken with specific tags
        /// </summary>
        /// <param name="deviceToken">The device token</param>
        /// <param name="tags">The tags to register to receive notifications from</param>
        public ApnsRegistration(string deviceToken, IEnumerable<string> tags)
            : base(deviceToken, tags)
        {
            if (string.IsNullOrWhiteSpace(deviceToken))
            {
                throw new ArgumentNullException("deviceToken");
            }
        }

        /// <summary>
        /// The deviceToken
        /// </summary>
        public string deviceToken
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