// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Windows.Data.Xml.Dom;

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
    [JsonObject]
    public sealed class TemplateRegistration : Registration
    {
        private const string WnsTypeName = "X-WNS-Type";

        public TemplateRegistration(string channelUri, string bodyTemplate, string templateName)
            : this(channelUri, bodyTemplate, templateName, null, null)
        {
        }

        public TemplateRegistration(string channelUri, string bodyTemplate, string templateName, IEnumerable<string> tags)
            : this(channelUri, bodyTemplate, templateName, tags, null)
        {
        }

        public TemplateRegistration(string channelUri, string bodyTemplate, string templateName, IEnumerable<string> tags, IEnumerable<KeyValuePair<string, string>> additionalHeaders)
            : base(channelUri, tags)
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new ArgumentNullException("templateName");
            }

            if (string.IsNullOrWhiteSpace(bodyTemplate))
            {
                throw new ArgumentNullException("bodyTemplate");
            }

            if (templateName.Equals(Registration.NativeRegistrationName))
            {
                throw new ArgumentException("TODO");
            }

            if (templateName.Contains(":") || templateName.Contains(";"))
            {
                throw new ArgumentException("TODO");
            }

            this.TemplateName = templateName;

            this.WnsHeaders = new WnsHeaderCollection();
            if (additionalHeaders != null)
            {
                foreach (var item in additionalHeaders)
                {
                    this.WnsHeaders.Add(item.Key, item.Value);
                }
            }

            this.BodyTemplate = bodyTemplate;

            // We only support xml as bodyTemplate even for wns/raw
            if (!this.WnsHeaders.ContainsKey(WnsTypeName))
            {
                XmlDocument xmlDocument = new XmlDocument();
                try
                {
                    xmlDocument.LoadXml(bodyTemplate);                    
                }
                catch (Exception e)
                {
                    throw new ArgumentException("Cannot autodetect X-WNS type from bodyTemplate: provide a body template with a valid toast/tile/badge content or specify a X-WNS-Type header.", e);
                }

                var payloadType = TemplateRegistration.DetectBodyType(xmlDocument);
                if (payloadType != null)
                {
                    this.WnsHeaders.Add(WnsTypeName, payloadType);
                }
                else
                {
                    throw new ArgumentException("Cannot autodetect X-WNS type from bodyTemplate: provide a body template with a valid toast/tile/badge content or specify a X-WNS-Type header.");
                }
            }
        }

        internal TemplateRegistration(string channelUri)
            : base(channelUri)
        {
            this.WnsHeaders = new WnsHeaderCollection();
        }

        /// <summary>
        /// Gets or Sets headers that should be sent to WNS with the notification
        /// </summary>
        [JsonProperty(PropertyName = "wnsheaders")]
        public WnsHeaderCollection WnsHeaders { get; set; }        

        /// <summary>
        /// Get or set templateName
        /// </summary>
        [JsonProperty(PropertyName = "templatename")]
        public string TemplateName { get; set; }

        /// <summary>
        /// Gets or sets bodyTemplate as string
        /// </summary>
        [JsonProperty(PropertyName = "bodytemplate")]
        public string BodyTemplate { get; set; }

        private static string DetectBodyType(XmlDocument template)
        {
            TemplateRegistrationType registrationType;
            if (template.FirstChild == null ||
                !Enum.TryParse(template.FirstChild.NodeName, true, out registrationType))
            {
                throw new ArgumentException("TODO");
            }

            return "wns/" + template.FirstChild.NodeName.ToLowerInvariant();
        }

        internal override string Name
        {
            get
            {
                return this.TemplateName;
            }
        }
    }

    enum TemplateRegistrationType
    {
        Toast,
        Tile,
        Badge
    }
}