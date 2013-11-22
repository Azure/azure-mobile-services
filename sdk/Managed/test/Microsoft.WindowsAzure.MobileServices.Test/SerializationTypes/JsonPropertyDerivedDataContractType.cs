// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [DataContract]
    public class JsonPropertyDerivedDataContractType : JsonPropertyType
    {
        public JsonPropertyDerivedDataContractType()
            : this(null)
        {
        }

        public JsonPropertyDerivedDataContractType(string valueToSet, bool onlySetSerializableMembers = false)
        {
            this.DerivedInternalFieldNamedDataMember = null;
            this.DerivedPrivateFieldNamedDataMember = null;
            this.DerivedInternalField = null;
            this.DerivedPrivateField = null;
            this.DerivedInternalFieldSansAttribute = null;
            this.DerivedPrivateFieldSansAttribute = null;

            if (valueToSet != null)
            {
                this.DerivedInternalField = "DerivedInternalField" + valueToSet;
                this.DerivedInternalFieldNamedDataMember = "DerivedInternalFieldNamedDataMember" + valueToSet;
                this.DerivedInternalProperty = "DerivedInternalProperty" + valueToSet;
                this.DerivedInternalPropertyNamedDataMember = "DerivedInternalPropertyNamedDataMember" + valueToSet;
                this.DerivedPrivateField = "DerivedPrivateField" + valueToSet;
                this.DerivedPrivateFieldNamedDataMember = "DerivedPrivateFieldNamedDataMember" + valueToSet;
                this.DerivedPrivateProperty = "DerivedPrivateProperty" + valueToSet;
                this.DerivedPrivatePropertyNamedDataMember = "DerivedPrivatePropertyNamedDataMember" + valueToSet;
                this.DerivedPublicField = "DerivedPublicField" + valueToSet;
                this.DerivedPublicFieldNamedDataMember = "DerivedPublicFieldNamedDataMember" + valueToSet;
                this.DerivedPublicProperty = "DerivedPublicProperty" + valueToSet;
                this.DerivedPublicPropertyNamedDataMember = "DerivedPublicPropertyNamedDataMember" + valueToSet;
                
                if (!onlySetSerializableMembers)
                {
                    this.DerivedInternalFieldSansAttribute = "DerivedInternalFieldSansAttribute" + valueToSet;
                    this.DerivedInternalPropertySansAttribute = "DerivedInternalPropertySansAttribute" + valueToSet;
                    this.DerivedPrivateFieldSansAttribute = "DerivedPrivateFieldSansAttribute" + valueToSet;
                    this.DerivedPrivatePropertySansAttribute = "DerivedPrivatePropertySansAttribute" + valueToSet;
                    this.DerivedPublicFieldSansAttribute = "DerivedPublicFieldSansAttribute" + valueToSet;
                    this.DerivedPublicPropertySansAttribute = "DerivedPublicPropertySansAttribute" + valueToSet;
                }
            }
        }

        public override bool Equals(object obj)
        {
            return SerializationTypeUtility.AreEqual(obj, this);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [DataMember(Name = "DerivedPublicPropertyDataMember")]
        public string DerivedPublicPropertyNamedDataMember { get; set; }

        [DataMember(Name = "DerivedInternalPropertyDataMember")]
        internal string DerivedInternalPropertyNamedDataMember { get; set; }

        [DataMember(Name = "DerivedPrivatePropertyDataMember")]
        private string DerivedPrivatePropertyNamedDataMember { get; set; }

        [DataMember(Name = "DerivedPublicFieldDataMember")]
        public string DerivedPublicFieldNamedDataMember;

        [DataMember(Name = "DerivedInternalFieldDataMember")]
        internal string DerivedInternalFieldNamedDataMember;

        [DataMember(Name = "DerivedPrivateFieldDataMember")]
        private string DerivedPrivateFieldNamedDataMember;

        [DataMember]
        public string DerivedPublicProperty { get; set; }

        [DataMember]
        internal string DerivedInternalProperty { get; set; }

        [DataMember]
        private string DerivedPrivateProperty { get; set; }

        [DataMember]
        public string DerivedPublicField;

        [DataMember]
        internal string DerivedInternalField;

        [DataMember]
        private string DerivedPrivateField;

        public string DerivedPublicPropertySansAttribute { get; set; }
        internal string DerivedInternalPropertySansAttribute { get; set; }
        private string DerivedPrivatePropertySansAttribute { get; set; }

        public string DerivedPublicPropertyGetOnlySansAttribute { get { return "DerivedPublicPropertyGetOnlySansAttribute"; } }
        internal string DerivedInternalPropertyGetOnlySansAttribute { get { return "DerivedInternalPropertyGetOnlySansAttribute"; } }
        private string DerivedPrivatePropertyGetOnlySansAttributer { get { return "DerivedPrivatePropertyGetOnlySansAttributer"; } }

        public string DerivedPublicFieldSansAttribute;
        internal string DerivedInternalFieldSansAttribute;
        private string DerivedPrivateFieldSansAttribute;
    }
}
