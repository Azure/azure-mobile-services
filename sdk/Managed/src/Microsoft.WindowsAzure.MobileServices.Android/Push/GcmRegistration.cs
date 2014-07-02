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
    public class GcmRegistration : Registration
    {
        internal GcmRegistration()
        {
        }

        /// <summary>
        /// Create a default Registration for a deviceId
        /// </summary>
        /// <param name="deviceId">The device id</param>
        public GcmRegistration(string deviceId)
            : this(deviceId, null)
        {
        }

        /// <summary>
        /// Create a default Registration for a deviceId with specific tags
        /// </summary>
        /// <param name="deviceId">The device id</param>
        /// <param name="tags">The tags to register to receive notifications from</param>
        public GcmRegistration(string deviceId, IEnumerable<string> tags)
            : base(deviceId, tags)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                throw new ArgumentNullException("deviceId");
            }
        }

        /// <summary>
        /// The Uri of the Channel returned by the Push Notification Channel Manager.
        /// </summary>
        public string DeviceId
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