//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal class RegistrationConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            object registration = jsonObject.Property("templateBody") == null
                ? Platform.Instance.PushUtility.GetNewNativeRegistration()
                : Platform.Instance.PushUtility.GetNewTemplateRegistration();
            serializer.Populate(jsonObject.CreateReader(), registration);
            return registration;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Registration).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
        }
    }
}
