// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [DataContract(Name="DataContractNameFromAttributeType")]
    public class DataContractType
    {
        public DataContractType()
            : this(null)
        {
        }

        public DataContractType(string valueToSet, bool onlySetSerializableMembers = false)
        {
            this.InternalFieldNamedDataMember = null;
            this.PrivateFieldNamedDataMember = null;
            this.InternalField = null;
            this.PrivateField = null;
            this.InternalFieldSansAttribute = null;
            this.PrivateFieldSansAttribute = null;

            if (valueToSet != null)
            {
                this.InternalField = "InternalField" + valueToSet;
                this.InternalFieldNamedDataMember = "InternalFieldNamedDataMember" + valueToSet;               
                this.InternalProperty = "InternalProperty" + valueToSet;
                this.InternalPropertyNamedDataMember = "InternalPropertyNamedDataMember" + valueToSet;              
                this.PrivateField = "PrivateField" + valueToSet;
                this.PrivateFieldNamedDataMember = "PrivateFieldNamedDataMember" + valueToSet;                
                this.PrivateProperty = "PrivateProperty" + valueToSet;
                this.PrivatePropertyNamedDataMember = "PrivatePropertyNamedDataMember" + valueToSet;               
                this.PublicField = "PublicField" + valueToSet;
                this.PublicFieldNamedDataMember = "PublicFieldNamedDataMember" + valueToSet;              
                this.PublicProperty = "PublicProperty" + valueToSet;
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

        public void SetPrivates(string valueToSet)
        {
            this.PrivateField = "PrivateField" + valueToSet;
            this.PrivatePropertyNamedDataMember = "PrivatePropertyDataMember" + valueToSet;
            this.PrivateFieldNamedDataMember = "PrivateFieldDataMember" + valueToSet;
            this.PrivateProperty = "PrivateProperty" + valueToSet;
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
        public long Id { get; set; }

        [DataMember (Name="PublicPropertyDataMember")]
        public string PublicPropertyNamedDataMember { get; set; }

        [DataMember(Name = "InternalPropertyDataMember")]
        internal string InternalPropertyNamedDataMember { get; set; }

        // Windows Store and Windows Phone will serialize private members in a different order; We'll explicitly
        // set the order just to making testing consistent accross the two platforms.
        [DataMember(Name = "PrivatePropertyDataMember", Order = 999)]
        private string PrivatePropertyNamedDataMember { get; set; }

        [DataMember(Name = "PublicFieldDataMember")]
        public string PublicFieldNamedDataMember;

        [DataMember(Name = "InternalFieldDataMember")]
        internal string InternalFieldNamedDataMember;

        // Windows Store and Windows Phone will serialize private members in a different order; We'll explicitly
        // set the order just to making testing consistent accross the two platforms.
        [DataMember(Name = "PrivateFieldDataMember", Order = 999)] 
        private string PrivateFieldNamedDataMember;

        [DataMember]
        public string PublicProperty { get; set; }

        [DataMember]
        internal string InternalProperty { get; set; }

        // Windows Store and Windows Phone will serialize private members in a different order; We'll explicitly
        // set the order just to making testing consistent accross the two platforms.
        [DataMember(Order = 999)]
        private string PrivateProperty { get; set; }

        [DataMember]
        public string PublicField;

        [DataMember]
        internal string InternalField;

        // Windows Store and Windows Phone will serialize private members in a different order; We'll explicitly
        // set the order just to making testing consistent accross the two platforms.
        [DataMember(Order = 999)]
        private string PrivateField;

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
