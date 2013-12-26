//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.Xml.Linq;

    internal static class XElementHelper
    {
        internal static string GetElementValueAsString(this XElement content, string name)
        {
            var element = content.Elements().FirstOrDefault(i => i.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (element != null)
            {
                return element.Value;
            }

            return null;
        }

        internal static DateTime? GetElementValueAsDateTime(this XElement content, string name)
        {
            var element = content.Elements().FirstOrDefault(i => i.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (element != null)
            {
                DateTime dateTime;
                if (DateTime.TryParse(element.Value, out dateTime))
                {
                    return dateTime;
                }
            }

            return null;
        }

        internal static T GetElementValue<T>(this XElement content, string name)
        {
            var element = content.Elements().FirstOrDefault(i => i.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (element == null)
            {
                return default(T);
            }

            string value = element.ToString();

            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            using (StringReader stringReader = new StringReader(value))
            {
                XmlReader xmlReader = XmlReader.Create(stringReader);
                var deserializedObject = (T)serializer.ReadObject(xmlReader);
                return deserializedObject;
            }
        }
    }
}
