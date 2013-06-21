// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [DataContract]
    public class PocoDerivedDataContractType : PocoType
    {
        public PocoDerivedDataContractType() :this(null)
        {
        }

        public PocoDerivedDataContractType(string valueToSet, bool onlySetSerializableMembers = false) 
        {
            this.DerivedInternalField = null;
            this.DerivedPrivateField = null;
            this.InternalFieldNamedDataMember = null;
            this.PrivateFieldNamedDataMember = null;
            this.InternalField = null;
            this.InternalFieldSansAttribute = null;
            this.PrivateFieldSansAttribute = null;

            if (valueToSet != null)
            {
                this.DerivedInternalField = "DerivedInternalField" + valueToSet;
                this.DerivedInternalProperty = "DerivedInternalProperty" + valueToSet;
                this.DerivedPrivateField = "DerivedPrivateField" + valueToSet;
                this.DerivedPrivateProperty = "DerivedPrivateProperty" + valueToSet;
                this.DerivedPublicField = "DerivedPublicField" + valueToSet;
                this.DerivedPublicProperty = "DerivedPublicProperty" + valueToSet;
                this.InternalFieldNamedDataMember = "InternalFieldNamedDataMember" + valueToSet;
                this.InternalPropertyNamedDataMember = "InternalPropertyNamedDataMember" + valueToSet;
                this.PrivateFieldNamedDataMember = "PrivateFieldNamedDataMember" + valueToSet;
                this.PrivatePropertyNamedDataMember = "PrivatePropertyNamedDataMember" + valueToSet;
                this.PublicFieldNamedDataMember = "PublicFieldNamedDataMember" + valueToSet;
                this.PublicPropertyNamedDataMember = "PublicPropertyNamedDataMember" + valueToSet;
                
                if (!onlySetSerializableMembers)
                {
                    this.InternalFieldSansAttribute = "InternalFieldSansAttribute" + valueToSet;
                    this.InternalPropertySansAttribute = "InternalPropertySansAttribute" + valueToSet;              
                    this.PrivateFieldSansAttribute = "PrivateFieldSansAttribute" + valueToSet;
                    this.PrivatePropertySansAttribute = "PrivatePropertySansAttribute" + valueToSet;
                    this.PublicFieldSansAttribute = "PublicFieldSansAttribute" + valueToSet;
                    this.PublicPropertySansAttribute = "PublicPropertySansAttribute" + valueToSet;              
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

        [DataMember]
        public new long Id { get; set; }

        [DataMember(Name = "PublicPropertyDataMember")]
        public string PublicPropertyNamedDataMember { get; set; }

        [DataMember(Name = "InternalPropertyDataMember")]
        internal string InternalPropertyNamedDataMember { get; set; }

        [DataMember(Name = "PrivatePropertyDataMember")]
        private string PrivatePropertyNamedDataMember { get; set; }

        [DataMember(Name = "PublicFieldDataMember")]
        public string PublicFieldNamedDataMember;

        [DataMember(Name = "InternalFieldDataMember")]
        internal string InternalFieldNamedDataMember;

        [DataMember(Name = "PrivateFieldDataMember")]
        private string PrivateFieldNamedDataMember;

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

        public string PublicPropertySansAttribute { get; set; }
        internal string InternalPropertySansAttribute { get; set; }
        private string PrivatePropertySansAttribute { get; set; }

        public string PublicPropertyGetOnlySansAttribute { get { return "PublicPropertyGetOnlySansAttribute"; } }
        internal string InternalPropertyGetOnlySansAttribute { get { return "InternalPropertyGetOnlySansAttribute"; } }
        private string PrivatePropertyGetOnlySansAttributer { get { return "PrivatePropertyGetOnlySansAttributer"; } }

        public string PublicFieldSansAttribute;
        internal string InternalFieldSansAttribute;
        private string PrivateFieldSansAttribute;
    }
}
