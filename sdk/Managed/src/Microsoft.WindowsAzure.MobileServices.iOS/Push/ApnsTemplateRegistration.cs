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
    /// Registration is used to define a target that is registered for notifications. A <see cref="ApnsTemplateRegistration"/> allows the client application
    /// to define the format of the registration.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class ApnsTemplateRegistration : ApnsRegistration
    {
        internal ApnsTemplateRegistration()
        {
        }

        /// <summary>
        /// Create a <see cref="ApnsTemplateRegistration"/>
        /// </summary>
        /// <param name="deviceToken">The device token</param>
        /// <param name="jsonTemplate">The template json in string format</param>
        /// <param name="expiry">The string defining the expiry template</param>
        /// <param name="templateName">The template name</param>
        public ApnsTemplateRegistration(string deviceToken, string jsonTemplate, string expiry, string templateName)
            : this(deviceToken, jsonTemplate, expiry, templateName, null)
        {
        }

        /// <summary>
        /// Create a <see cref="ApnsTemplateRegistration"/>
        /// </summary>
        /// <param name="deviceToken">The device token</param>
        /// <param name="jsonTemplate">The template json in string format</param>
        /// <param name="expiry">The string defining the expiry template</param>
        /// <param name="templateName">The template name</param>
        /// <param name="tags">The tags that restrict which notifications this registration will receive</param>
        public ApnsTemplateRegistration(string deviceToken, string jsonTemplate, string expiry, string templateName, IEnumerable<string> tags) 
            : base(deviceToken, tags)
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
            this.Expiry = expiry;
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
        /// Gets or sets expiry as string
        /// </summary>
        [JsonProperty(PropertyName = "expiry")]
        public string Expiry { get; set; }

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