// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal sealed class CDataMember : IXmlSerializable
    {
        public CDataMember(string value)
        {
            this.Value = value;
        }

        internal CDataMember()
        {
        }

        public string Value { get; set; }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                this.Value = "";
            }
            else
            {
                reader.Read();

                switch (reader.NodeType)
                {
                    case XmlNodeType.EndElement:
                        this.Value = "";
                        break;

                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                        this.Value = reader.ReadContentAsString();
                        break;
                    default:
                        string errorString = string.Format(CultureInfo.InvariantCulture, "TODO", reader.NodeType);
                        throw new InvalidOperationException(errorString);
                }
            }
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            if (!string.IsNullOrWhiteSpace(this.Value))
            {
                writer.WriteCData(this.Value);
            }
        }
    }
}
