// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Windows.Data.Xml.Dom;

namespace Microsoft.WindowsAzure.MobileServices
{
    [DataContract(Name = "WindowsTemplateRegistrationDescription", Namespace = "http://schemas.microsoft.com/netservices/2010/10/servicebus/connect")]
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

            this.BodyTemplateData = new CDataMember(bodyTemplate);

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
            this.BodyTemplateData = new CDataMember();
        }

        /// <summary>
        /// Gets or Sets headers that should be sent to WNS with the notification
        /// </summary>
        [DataMember(Order = 5, Name = "WnsHeaders", IsRequired = true)]
        public WnsHeaderCollection WnsHeaders { get; set; }

        /// <summary>
        /// Gets or Sets an xml fragment of the notification with placeholder expressions
        /// </summary>
        [DataMember(Order = 4, Name = "BodyTemplate", IsRequired = true)]
        internal CDataMember BodyTemplateData { get; set; }

        /// <summary>
        /// Get or set templateName
        /// </summary>
        [DataMember(Order = 6, Name = "TemplateName", IsRequired = false)]
        public string TemplateName { get; set; }

        /// <summary>
        /// Gets or sets bodyTemplate as string
        /// </summary>
        public string BodyTemplate
        {
            get
            {
                if (this.BodyTemplateData != null)
                {
                    return this.BodyTemplateData.Value;
                }

                return null;
            }

            set
            {
                this.BodyTemplateData = new CDataMember(value);
            }
        }

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
