// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Linq;

using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Registration is used to define a target that is registered for notifications. A TemplateRegistration allows the client application
    /// to define the format of the registration.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class MpnsTemplateRegistration : MpnsRegistration
    {
        /// <summary>
        /// Name of the <see cref="MpnsHeaders"/> key for Windows Phone Notification Target
        /// </summary>
        internal const string NotificationType = "X-WindowsPhone-Target";

        /// <summary>
        /// Name of the <see cref="MpnsHeaders"/> key for Windows Phone Notification Class
        /// </summary>
        internal const string NotificationClass = "X-NotificationClass";

        internal const string NamespaceName = "WPNotification";
        internal const string Tile = "token";
        internal const string Toast = "toast";

        internal const string TileClass = "1";
        internal const string ToastClass = "2";
        internal const string RawClass = "3";

        internal MpnsTemplateRegistration()
        {
        }

        /// <summary>
        /// Create a TemplateRegistration
        /// </summary>
        /// <param name="channelUri">The channel uri</param>
        /// <param name="bodyTemplate">The template xml in string format</param>
        /// <param name="templateName">The template name</param>
        public MpnsTemplateRegistration(string channelUri, string bodyTemplate, string templateName)
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
        public MpnsTemplateRegistration(string channelUri, string bodyTemplate, string templateName, IEnumerable<string> tags)
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
        public MpnsTemplateRegistration(string channelUri, string bodyTemplate, string templateName, IEnumerable<string> tags, IEnumerable<KeyValuePair<string, string>> additionalHeaders)
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

            if (templateName.Equals(MpnsRegistration.NativeRegistrationName))
            {
                throw new ArgumentException(Resources.Push_ConflictWithReservedName);
            }

            if (templateName.Contains(":") || templateName.Contains(";"))
            {
                throw new ArgumentException(Resources.Push_InvalidTemplateName);
            }

            this.TemplateName = templateName;

            this.MpnsHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (additionalHeaders != null)
            {
                foreach (var item in additionalHeaders)
                {
                    this.MpnsHeaders.Add(item.Key, item.Value);
                }
            }

            this.BodyTemplate = bodyTemplate;
            this.DetectBodyType();

            this.MpnsHeaders = new ReadOnlyDictionary<string, string>(this.MpnsHeaders);
        }

        private enum TemplateRegistrationType
        {
            Toast,
            Tile
        }

        /// <summary>
        /// Gets headers that should be sent to WNS with the notification
        /// </summary>
        [JsonProperty(PropertyName = "headers")]
        public IDictionary<string, string> MpnsHeaders { get; private set; }

        /// <summary>
        /// Get templateName
        /// </summary>
        [JsonProperty(PropertyName = "templateName")]
        public string TemplateName { get; private set; }

        /// <summary>
        /// Gets or sets bodyTemplate as string
        /// </summary>
        [JsonProperty(PropertyName = "templateBody")]
        public string BodyTemplate { get; private set; }

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

        private void DetectBodyType()
        {
            if (this.MpnsHeaders.ContainsKey(NotificationClass) &&
                this.MpnsHeaders[NotificationClass].Equals(RawClass, StringComparison.OrdinalIgnoreCase))
            {
                // no further check for raw format
                return;
            }

            if (!this.MpnsHeaders.ContainsKey(NotificationType))
            {
                // AutoDetectType
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
                    throw new ArgumentException(Resources.Push_BodyTemplateMustBeXml, "bodyTemplate");
                }

                if (body == null)
                {
                    throw new ArgumentException(Resources.Push_BodyTemplateMustContainElement, "bodyTemplate");
                }

                TemplateRegistrationType registrationType;
                if (string.Equals(body.Name.Namespace.NamespaceName, NamespaceName, StringComparison.OrdinalIgnoreCase)
                    && Enum.TryParse(body.Name.LocalName, true, out registrationType))
                {
                    switch (registrationType)
                    {
                        case TemplateRegistrationType.Toast:
                            this.MpnsHeaders.Add(NotificationType, Toast);
                            break;
                        case TemplateRegistrationType.Tile:
                            this.MpnsHeaders.Add(NotificationType, Tile);
                            break;
                    }
                }
            }

            if (!this.MpnsHeaders.ContainsKey(NotificationClass) && this.MpnsHeaders.ContainsKey(NotificationType))
            {
                switch (this.MpnsHeaders[NotificationType])
                {
                    case Toast:
                        this.MpnsHeaders.Add(NotificationClass, ToastClass);
                        break;
                    case Tile:
                        this.MpnsHeaders.Add(NotificationClass, TileClass);
                        break;
                }
            }
        }        
    }
}