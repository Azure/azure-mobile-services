// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Registration is used to define a target that is registered for notifications. A <see cref="GcmTemplateRegistration"/> allows the client application
    /// to define the format of the registration.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class GcmTemplateRegistration : GcmRegistration
    {
        internal GcmTemplateRegistration()
        {
        }

        /// <summary>
        /// Create a <see cref="GcmTemplateRegistration"/>
        /// </summary>
        /// <param name="deviceId">The device id</param>
        /// <param name="jsonTemplate">The template json in string format</param>
        /// <param name="templateName">The template name</param>
        public GcmTemplateRegistration(string deviceId, string jsonTemplate, string templateName)
            : this(deviceId, jsonTemplate, templateName, null)
        {
        }

        /// <summary>
        /// Create a <see cref="GcmTemplateRegistration"/>
        /// </summary>
        /// <param name="deviceId">The device id</param>
        /// <param name="jsonTemplate">The template json in string format</param>
        /// <param name="templateName">The template name</param>
        /// <param name="tags">The tags that restrict which notifications this registration will receive</param>
        public GcmTemplateRegistration(string deviceId, string jsonTemplate, string templateName, IEnumerable<string> tags)
            : base(deviceId, tags)
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new ArgumentNullException("templateName");
            }

            if (templateName.Equals(Registration.NativeRegistrationName))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.Push_ConflictWithReservedName, Registration.NativeRegistrationName));
            }

            if (templateName.Contains(":") || templateName.Contains(";"))
            {
                throw new ArgumentException(Resources.Push_InvalidTemplateName);
            }

            if (string.IsNullOrWhiteSpace(jsonTemplate))
            {
                throw new ArgumentNullException("jsonTemplate");
            }

            this.TemplateName = templateName;
            this.BodyTemplate = jsonTemplate;
        }

        /// <summary>
        /// Get templateName
        /// </summary>
        [JsonProperty(PropertyName = "templateName")]
        public string TemplateName { get; internal set; }

        /// <summary>
        /// Gets jsonTemplate as string
        /// </summary>
        [JsonProperty(PropertyName = "templateBody")]
        public string BodyTemplate { get; internal set; }

        /// <summary>
        /// The name of the registration used in local storage.
        /// </summary>
        public override string Name
        {
            get
            {
                return this.TemplateName;
            }
        }
    }
}