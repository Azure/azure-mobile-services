// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    public class JsonPropertyType
    {
        public JsonPropertyType()
            : this(null)
        {
        }

        public JsonPropertyType(string valueToSet, bool onlySetSerializableMembers = false)
        {
            this.InternalFieldNamedJsonProperty = null;
            this.PrivateFieldNamedJsonProperty = null;
            this.InternalField = null;
            this.PrivateField = null;
            this.InternalFieldSansAttribute = null;
            this.PrivateFieldSansAttribute = null;

            if (valueToSet != null)
            {
                this.InternalField = "InternalField" + valueToSet;
                this.InternalFieldNamedJsonProperty = "InternalFieldNamedJsonProperty" + valueToSet;             
                this.InternalProperty = "InternalProperty" + valueToSet;
                this.InternalPropertyNamedJsonProperty = "InternalPropertyNamedJsonProperty" + valueToSet;           
                this.PrivateField = "PrivateField" + valueToSet;
                this.PrivateFieldNamedJsonProperty = "PrivateFieldNamedJsonProperty" + valueToSet;        
                this.PrivateProperty = "PrivateProperty" + valueToSet;
                this.PrivatePropertyNamedJsonProperty = "PrivatePropertyNamedJsonProperty" + valueToSet;               
                this.PublicField = "PublicField" + valueToSet;
                this.PublicFieldNamedJsonProperty = "PublicFieldNamedJsonProperty" + valueToSet;
                this.PublicFieldSansAttribute = "PublicFieldSansAttribute" + valueToSet;
                this.PublicProperty = "PublicProperty" + valueToSet;
                this.PublicPropertyNamedJsonProperty = "PublicPropertyNamedJsonProperty" + valueToSet;
                this.PublicPropertySansAttribute = "PublicPropertySansAttribute" + valueToSet;

                if (!onlySetSerializableMembers)
                {
                    this.InternalFieldSansAttribute = "InternalFieldSansAttribute" + valueToSet;
                    this.InternalPropertySansAttribute = "InternalPropertySansAttribute" + valueToSet;
                    this.PrivateFieldSansAttribute = "PrivateFieldSansAttribute" + valueToSet;
                    this.PrivatePropertySansAttribute = "PrivatePropertySansAttribute" + valueToSet;
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

        public void SetPrivates(string valueToSet)
        {
            this.PrivateField = "PrivateField" + valueToSet;
            this.PrivatePropertyNamedJsonProperty = "PrivatePropertyNamedJsonProperty" + valueToSet;
            this.PrivateFieldNamedJsonProperty = "PrivateFieldNamedJsonProperty" + valueToSet;
            this.PrivateProperty = "PrivateProperty" + valueToSet;
        }

        [JsonProperty]
        public long Id { get; set; }

        [JsonProperty(PropertyName = "PublicPropertyJsonProperty")]
        public string PublicPropertyNamedJsonProperty { get; set; }

        [JsonProperty(PropertyName = "InternalPropertyJsonProperty")]
        internal string InternalPropertyNamedJsonProperty { get; set; }

        // Windows Store and Windows Phone will serialize private members in a different order; We'll explicitly
        // set the order just to making testing consistent accross the two platforms.
        [JsonProperty(PropertyName = "PrivatePropertyJsonProperty", Order=999)]
        private string PrivatePropertyNamedJsonProperty { get; set; }

        [JsonProperty(PropertyName = "PublicFieldJsonProperty")]
        public string PublicFieldNamedJsonProperty;

        [JsonProperty(PropertyName = "InternalFieldJsonProperty")]
        internal string InternalFieldNamedJsonProperty;

        // Windows Store and Windows Phone will serialize private members in a different order; We'll explicitly
        // set the order just to making testing consistent accross the two platforms.
        [JsonProperty(PropertyName = "PrivateFieldJsonProperty", Order = 999)]
        private string PrivateFieldNamedJsonProperty;

        [JsonProperty]
        public string PublicProperty { get; set; }

        [JsonProperty]
        internal string InternalProperty { get; set; }

        // Windows Store and Windows Phone will serialize private members in a different order; We'll explicitly
        // set the order just to making testing consistent accross the two platforms.
        [JsonProperty(Order = 999)]
        private string PrivateProperty { get; set; }

        [JsonProperty]
        public string PublicField;

        [JsonProperty]
        internal string InternalField;

        // Windows Store and Windows Phone will serialize private members in a different order; We'll explicitly
        // set the order just to making testing consistent accross the two platforms.
        [JsonProperty(Order = 999)]
        private string PrivateField;

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
