// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    public class PocoDerivedJsonPropertyType : PocoType
    {
        public PocoDerivedJsonPropertyType() :this(null)
        {
        }

        public PocoDerivedJsonPropertyType(string valueToSet, bool onlySetSerializableMembers = false) 
            : base(valueToSet, onlySetSerializableMembers)
        {
            this.DerivedInternalField = null;
            this.DerivedPrivateField = null;
            this.InternalFieldNamedJsonProperty = null;
            this.PrivateFieldNamedJsonProperty = null;
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

                this.InternalFieldNamedJsonProperty = "InternalFieldNamedJsonProperty" + valueToSet;
                this.InternalPropertyNamedJsonProperty = "InternalPropertyNamedJsonProperty" + valueToSet;
                this.PrivateFieldNamedJsonProperty = "PrivateFieldNamedJsonProperty" + valueToSet;
                this.PrivatePropertyNamedJsonProperty = "PrivatePropertyNamedJsonProperty" + valueToSet;
                this.PublicFieldNamedJsonProperty = "PublicFieldNamedJsonProperty" + valueToSet;
                this.PublicPropertyNamedJsonProperty = "PublicPropertyNamedJsonProperty" + valueToSet;
                this.PublicFieldSansAttribute = "PublicFieldSansAttribute" + valueToSet;
                this.PublicPropertySansAttribute = "PublicPropertySansAttribute" + valueToSet;

                if (!onlySetSerializableMembers)
                {           
                    this.InternalFieldSansAttribute = "InternalFieldSansAttribute" + valueToSet;
                    this.PrivateFieldSansAttribute = "PrivateFieldSansAttribute" + valueToSet;
                    this.PrivatePropertySansAttribute = "PrivatePropertySansAttribute" + valueToSet;
                    this.InternalPropertySansAttribute = "InternalPropertySansAttribute" + valueToSet;            
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

        [JsonProperty(PropertyName = "PublicPropertyJsonProperty")]
        public string PublicPropertyNamedJsonProperty { get; set; }

        [JsonProperty(PropertyName = "InternalPropertyJsonProperty")]
        internal string InternalPropertyNamedJsonProperty { get; set; }

        [JsonProperty(PropertyName = "PrivatePropertyJsonProperty")]
        private string PrivatePropertyNamedJsonProperty { get; set; }

        [JsonProperty(PropertyName = "PublicFieldJsonProperty")]
        public string PublicFieldNamedJsonProperty;

        [JsonProperty(PropertyName = "InternalFieldJsonProperty")]
        internal string InternalFieldNamedJsonProperty;

        [JsonProperty(PropertyName = "PrivateFieldJsonProperty")]
        private string PrivateFieldNamedJsonProperty;

        [JsonProperty]
        public string DerivedPublicProperty { get; set; }

        [JsonProperty]
        internal string DerivedInternalProperty { get; set; }

        [JsonProperty]
        private string DerivedPrivateProperty { get; set; }

        [JsonProperty]
        public string DerivedPublicField;

        [JsonProperty]
        internal string DerivedInternalField;

        [JsonProperty]
        private string DerivedPrivateField;

        public string PublicPropertySansAttribute { get; set; }
        internal string InternalPropertySansAttribute { get; set; }
        private string PrivatePropertySansAttribute { get; set; }

        public string PublicPropertyGetOnlySansAttribute { get { return "PublicPropertyGetOnlySansAttribute"; } }
        internal string InternalPropertyGetOnlySansAttribute { get { return "InternalPropertyGetOnlySansAttribute"; } }
        private string PrivatePropertyGetOnlySansAttribute { get { return "PrivatePropertyGetOnlySansAttribute"; } }

        public string PublicFieldSansAttribute;
        internal string InternalFieldSansAttribute;
        private string PrivateFieldSansAttribute;
    }
}
