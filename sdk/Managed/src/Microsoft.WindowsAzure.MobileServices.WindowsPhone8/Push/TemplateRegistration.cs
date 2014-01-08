// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Xml.Linq;

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
    /// <summary>
    /// Registration is used to define a target that is registered for notifications. A TemplateRegistration allows the client application
    /// to define the format of the registration.
    /// </summary>
    [JsonObject]
    public sealed class TemplateRegistration : Registration
    {
        /// <summary>
        /// Name of the MpnsHeader key for Windows Phone Notification Target
        /// </summary>
        internal const string NotificationType = "X-WindowsPhone-Target";

        /// <summary>
        /// Name of the MpnsHeader key for Windows Phone Notification Class
        /// </summary>
        internal const string NotificationClass = "X-NotificationClass";

        internal const string NamespaceName = "WPNotification";
        internal const string Tile = "token";
        internal const string Toast = "toast";

        internal const string TileClass = "1";
        internal const string ToastClass = "2";
        internal const string RawClass = "3";

        /// <summary>
        /// Create a TemplateRegistration
        /// </summary>
        /// <param name="channelUri">The channel uri</param>
        /// <param name="bodyTemplate">The template xml in string format</param>
        /// <param name="templateName">The template name</param>
        public TemplateRegistration(string channelUri, string bodyTemplate, string templateName)
            : this(channelUri, bodyTemplate, templateName, null, null)
        {
        }

        /// <summary>
        /// Create a TemplateRegistration
        /// </summary>
        /// <param name="channelUri">The channel uri</param>
        /// <param name="bodyTemplate">The template xml in string format</param>
        /// <param name="templateName">The template name</param>
        /// <param name="tags">The tags that restrict which notifications this registration will receive</param>
        public TemplateRegistration(string channelUri, string bodyTemplate, string templateName, IEnumerable<string> tags)
            : this(channelUri, bodyTemplate, templateName, tags, null)
        {
        }

        /// <summary>
        /// Create a TemplateRegistration
        /// </summary>
        /// <param name="channelUri">The channel uri</param>
        /// <param name="bodyTemplate">The template xml in string format</param>
        /// <param name="templateName">The template name</param>
        /// <param name="tags">The tags that restrict which notifications this registration will receive</param>
        /// <param name="additionalHeaders">Additional headers</param>
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
                throw new ArgumentException("Resource.ConflictWithReservedName");
            }

            if (templateName.Contains(":") || templateName.Contains(";"))
            {
                throw new ArgumentException("Resource.InvalidTemplateName");
            }

            this.TemplateName = templateName;

            this.MpnsHeaders = new MpnsHeaderCollection();
            if (additionalHeaders != null)
            {
                foreach (var item in additionalHeaders)
                {
                    this.MpnsHeaders.Add(item.Key, item.Value);
                }
            }

            this.BodyTemplate = bodyTemplate;
            this.DetectBodyType();
        }

        internal TemplateRegistration(string channelUri)
            : base(channelUri)
        {
            this.MpnsHeaders = new MpnsHeaderCollection();
        }

        internal TemplateRegistration(XElement content)
            : base(content)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            this.MpnsHeaders = content.GetElementValue<MpnsHeaderCollection>("MpnsHeaders");
            this.BodyTemplate = content.GetElementValueAsString("BodyTemplate");
            this.TemplateName = content.GetElementValueAsString("TemplateName");
        }

        /// <summary>
        /// Gets headers that should be sent to WNS with the notification
        /// </summary>
        [JsonProperty(PropertyName = "mpnsheaders")]
        public MpnsHeaderCollection MpnsHeaders { get; private set; }

        /// <summary>
        /// Get or set templateName
        /// </summary>
        [JsonProperty(PropertyName = "templatename")]
        public string TemplateName { get; private set; }

        /// <summary>
        /// Gets or sets bodyTemplate as string
        /// </summary>
        [JsonProperty(PropertyName = "bodytemplate")]
        public string BodyTemplate { get; private set; }

        private void DetectBodyType()
        {
            if (this.MpnsHeaders.ContainsKey(NotificationClass) &&
                this.MpnsHeaders[NotificationClass].Equals(RawClass, StringComparison.OrdinalIgnoreCase))
            {
                // no further check for raw format
                return;
            }

            XElement body = null;
            try
            {
                var elements = XElement.Parse(this.BodyTemplate).Elements();
                foreach (var element in elements)
                {
                    if (element.NodeType == System.Xml.XmlNodeType.Element)
                    {
                        body = element;
                        break;
                    }
                }
            }
            catch (Exception)
            {
                throw new ArgumentException("Resource.NotSupportedXMLFormatAsBodyTemplate");
            }

            if (body == null)
            {
                throw new ArgumentException("Resource.NotSupportedXMLFormatAsBodyTemplate");
            }

            this.MpnsHeaders.Remove(NotificationType);

            TemplateRegistrationType registrationType;
            if (string.Equals(body.Name.Namespace.NamespaceName, NamespaceName, StringComparison.OrdinalIgnoreCase) &&
                Enum.TryParse(body.Name.LocalName, true, out registrationType))
            {
                switch (registrationType)
                {
                    case TemplateRegistrationType.Toast:
                        this.MpnsHeaders.Add(NotificationType, Toast);
                        this.MpnsHeaders.Add(NotificationClass, ToastClass);
                        break;
                    case TemplateRegistrationType.Tile:
                        this.MpnsHeaders.Add(NotificationType, Tile);
                        this.MpnsHeaders.Add(NotificationClass, TileClass);
                        break;
                    default:
                        throw new NotSupportedException(registrationType.ToString());
                }
            }
            else
            {
                throw new ArgumentException("Resource.NotSupportedXMLFormatAsBodyTemplate");
            }
        }

        internal override string Name
        {
            get
            {
                return this.TemplateName;
            }
        }

        enum TemplateRegistrationType
        {
            Toast,
            Tile
        }
    }
}