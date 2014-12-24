// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

#if __UNIFIED__
using Foundation;
#else
using MonoTouch.Foundation;
#endif

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
        /// PLEASE USE NSData overload of this constructor!! Create a default Registration for a deviceToken
        /// </summary>
        /// <param name="deviceToken">The device token</param>
        public ApnsRegistration(string deviceToken)
            : this(deviceToken, null)
        {
        }

        /// <summary>
        /// Create a default Registration for a deviceToken
        /// </summary>
        /// <param name="deviceToken">The device token</param>
        public ApnsRegistration(NSData deviceToken)
            : this(deviceToken, null)
        {
        }        

        /// <summary>
        /// PLEASE USE NSData overload of this constructor!! Create a default Registration for a deviceToken with specific tags
        /// </summary>
        /// <param name="deviceToken">The device token</param>
        /// <param name="tags">The tags to register to receive notifications from</param>
        public ApnsRegistration(string deviceToken, IEnumerable<string> tags)
            : base(TrimDeviceToken(deviceToken), tags)
        {
        }

        /// <summary>
        /// Create a default Registration for a deviceToken with specific tags
        /// </summary>
        /// <param name="deviceToken">The device token</param>
        /// <param name="tags">The tags to register to receive notifications from</param>
        public ApnsRegistration(NSData deviceToken, IEnumerable<string> tags)
            : base(ApnsRegistration.ParseDeviceToken(deviceToken), tags)
        {            
        }

        internal static string TrimDeviceToken(string deviceToken)
        {
            if (deviceToken == null)
            {
                throw new ArgumentNullException("deviceToken");
            }

            return deviceToken.Trim('<','>').Replace(" ", string.Empty).ToUpperInvariant();
        }

        internal static string ParseDeviceToken(NSData deviceToken)
        {
            if (deviceToken == null)
            {
                throw new ArgumentNullException("deviceToken");
            }

            return TrimDeviceToken(deviceToken.Description);
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
                this.PushHandle = TrimDeviceToken(value);
            }
        }
    }
}