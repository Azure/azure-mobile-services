// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service;
using Newtonsoft.Json.Linq;

namespace ZumoE2EServerApp.DataObjects
{
    public class W8PushTestEntity : EntityData
    {
        public string NHNotificationType { get; set; }
        public string Payload { get; set; }
        public string XmlPayload { get; set; }
        public string PushResponse { get; set; }
        public JToken TemplateNotification { get; set; }
    }
}