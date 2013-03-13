// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    public class JsonPropertyDerivedJsonPropertyType : JsonPropertyType
    {
        public JsonPropertyDerivedJsonPropertyType()
            : this(null)
        {
        }

        public JsonPropertyDerivedJsonPropertyType(string valueToSet, bool onlySetSerializableMembers = false)
            : base(valueToSet, onlySetSerializableMembers)
        {
            this.DerivedInternalFieldNamedJsonProperty = null;
            this.DerivedPrivateFieldNamedJsonProperty = null;
            this.DerivedInternalField = null;
            this.DerivedPrivateField = null;
            this.DerivedInternalFieldSansAttribute = null;
            this.DerivedPrivateFieldSansAttribute = null;

            if (valueToSet != null)
            {
                this.DerivedInternalField = "DerivedInternalField" + valueToSet;
                this.DerivedInternalFieldNamedJsonProperty = "DerivedInternalFieldNamedJsonProperty" + valueToSet;
                this.DerivedInternalProperty = "DerivedInternalProperty" + valueToSet;
                this.DerivedInternalPropertyNamedJsonProperty = "DerivedInternalPropertyNamedJsonProperty" + valueToSet;
                this.DerivedPrivateField = "DerivedPrivateField" + valueToSet;
                this.DerivedPrivateFieldNamedJsonProperty = "DerivedPrivateFieldNamedJsonProperty" + valueToSet;
                this.DerivedPrivateProperty = "DerivedPrivateProperty" + valueToSet;
                this.DerivedPrivatePropertyNamedJsonProperty = "DerivedPrivatePropertyNamedJsonProperty" + valueToSet;
                this.DerivedPublicField = "DerivedPublicField" + valueToSet;
                this.DerivedPublicFieldNamedJsonProperty = "DerivedPublicFieldNamedJsonProperty" + valueToSet;
                this.DerivedPublicFieldSansAttribute = "DerivedPublicFieldSansAttribute" + valueToSet;
                this.DerivedPublicProperty = "DerivedPublicProperty" + valueToSet;
                this.DerivedPublicPropertyNamedJsonProperty = "DerivedPublicPropertyNamedJsonProperty" + valueToSet;
                this.DerivedPublicPropertySansAttribute = "DerivedPublicPropertySansAttribute" + valueToSet;

                if (!onlySetSerializableMembers)
                {
                    this.DerivedInternalFieldSansAttribute = "DerivedInternalFieldSansAttribute" + valueToSet;
                    this.DerivedInternalPropertySansAttribute = "DerivedInternalPropertySansAttribute" + valueToSet;
                    this.DerivedPrivateFieldSansAttribute = "DerivedPrivateFieldSansAttribute" + valueToSet;
                    this.DerivedPrivatePropertySansAttribute = "DerivedPrivatePropertySansAttribute" + valueToSet;              
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

        [JsonProperty(PropertyName = "DerivedPublicPropertyJsonProperty")]
        public string DerivedPublicPropertyNamedJsonProperty { get; set; }

        [JsonProperty(PropertyName = "DerivedInternalPropertyJsonProperty")]
        internal string DerivedInternalPropertyNamedJsonProperty { get; set; }

        [JsonProperty(PropertyName = "DerivedPrivatePropertyJsonProperty")]
        private string DerivedPrivatePropertyNamedJsonProperty { get; set; }

        [JsonProperty(PropertyName = "DerivedPublicFieldJsonProperty")]
        public string DerivedPublicFieldNamedJsonProperty;

        [JsonProperty(PropertyName = "DerivedInternalFieldJsonProperty")]
        internal string DerivedInternalFieldNamedJsonProperty;

        [JsonProperty(PropertyName = "DerivedPrivateFieldJsonProperty")]
        private string DerivedPrivateFieldNamedJsonProperty;

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
