// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

using Newtonsoft.Json;

using Windows.Data.Xml.Dom;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal enum TemplateRegistrationType
    {
        Toast,
        Tile,
        Badge
    }

    /// <summary>
    /// Registration is used to define a target that is registered for notifications. A <see cref="WnsTemplateRegistration"/> allows the client application
    /// to define the format of the registration.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class WnsTemplateRegistration : WnsRegistration
    {
        private const string WnsTypeName = "X-WNS-Type";

        internal WnsTemplateRegistration()
        {
        }

        /// <summary>
        /// Create a <see cref="WnsTemplateRegistration"/>
        /// </summary>
        /// <param name="channelUri">The channel uri</param>
        /// <param name="bodyTemplate">The template xml in string format</param>
        /// <param name="templateName">The template name</param>
        public WnsTemplateRegistration(string channelUri, string bodyTemplate, string templateName)
            : this(channelUri, bodyTemplate, templateName, null, null)
        {
        }

        /// <summary>
        /// Create a <see cref="WnsTemplateRegistration"/>
        /// </summary>
        /// <param name="channelUri">The channel uri</param>
        /// <param name="bodyTemplate">The template xml in string format</param>
        /// <param name="templateName">The template name</param>
        /// <param name="tags">The tags that restrict which notifications this registration will receive</param>
        public WnsTemplateRegistration(string channelUri, string bodyTemplate, string templateName, IEnumerable<string> tags)
            : this(channelUri, bodyTemplate, templateName, tags, null)
        {
        }

        /// <summary>
        /// Create a <see cref="WnsTemplateRegistration"/>
        /// </summary>
        /// <param name="channelUri">The channel uri</param>
        /// <param name="bodyTemplate">The template xml in string format</param>
        /// <param name="templateName">The template name</param>
        /// <param name="tags">The tags that restrict which notifications this registration will receive</param>
        /// <param name="additionalHeaders">Additional headers</param>
        public WnsTemplateRegistration(string channelUri, string bodyTemplate, string templateName, IEnumerable<string> tags, IEnumerable<KeyValuePair<string, string>> additionalHeaders)
            : base(channelUri, tags)
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

            if (string.IsNullOrWhiteSpace(bodyTemplate))
            {
                throw new ArgumentNullException("bodyTemplate");
            }

            this.TemplateName = templateName;
            this.WnsHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (additionalHeaders != null)
            {
                foreach (var item in additionalHeaders)
                {
                    this.WnsHeaders.Add(item.Key, item.Value);
                }
            }

            this.BodyTemplate = bodyTemplate;

            if (!this.WnsHeaders.ContainsKey(WnsTypeName))
            {
                // Because there are no headers, it is not raw
                // This means it must be XML
                XmlDocument xmlDocument = new XmlDocument();
                try
                {
                    xmlDocument.LoadXml(bodyTemplate);
                }
                catch (Exception e)
                {
                    throw new ArgumentException(Resources.Push_BodyTemplateMustBeXml, "bodyTemplate", e);
                }

                var payloadType = WnsTemplateRegistration.DetectBodyType(xmlDocument);
                this.WnsHeaders.Add(WnsTypeName, payloadType);
            }

            this.WnsHeaders = new ReadOnlyDictionary<string, string>(this.WnsHeaders);
        }

        /// <summary>
        /// Gets headers that should be sent to WNS with the notification
        /// </summary>
        [JsonProperty(PropertyName = "headers")]
        public IDictionary<string, string> WnsHeaders { get; internal set; }

        /// <summary>
        /// Get templateName
        /// </summary>
        [JsonProperty(PropertyName = "templateName")]
        public string TemplateName { get; internal set; }

        /// <summary>
        /// Gets bodyTemplate as string
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

        private static string DetectBodyType(XmlDocument template)
        {
            TemplateRegistrationType registrationType;
            if (template.FirstChild == null ||
                !Enum.TryParse(template.FirstChild.NodeName, true, out registrationType))
            {
                // First node of the body template should be toast/tile/badge
                throw new ArgumentException(Resources.Push_NotSupportedXMLFormatAsBodyTemplateWin8);
            }

            return "wns/" + template.FirstChild.NodeName.ToLowerInvariant();
        }
    }    
}