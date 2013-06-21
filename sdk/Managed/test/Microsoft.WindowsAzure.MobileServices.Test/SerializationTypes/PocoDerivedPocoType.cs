// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    public class PocoDerivedPocoType : PocoType
    {
        public PocoDerivedPocoType() :this(null)
        {
        }

        public PocoDerivedPocoType(string valueToSet, bool onlySetSerializableMembers = false) 
            : base(valueToSet, onlySetSerializableMembers)
        {
            this.DerivedInternalField = null;
            this.DerivedPrivateField = null;

            if (valueToSet != null)
            {
                this.DerivedPublicField = "DerivedPublicField" + valueToSet;
                this.DerivedPublicProperty = "DerivedPublicProperty" + valueToSet;
                
                if (!onlySetSerializableMembers)
                {
                    this.DerivedInternalField = "DerivedInternalField" + valueToSet;
                    this.DerivedInternalProperty = "DerivedInternalProperty" + valueToSet;
                    this.DerivedPrivateField = "DerivedPrivateField" + valueToSet;
                    this.DerivedPrivateProperty = "DerivedPrivateProperty" + valueToSet;
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

        public string DerivedPublicProperty { get; set; }

        internal string DerivedInternalProperty { get; set; }

        private string DerivedPrivateProperty { get; set; }

        public string DerivedPublicPropertyGetOnly { get { return "DerivedPublicPropertyGetOnly"; } }

        internal string DerivedInternalPropertyGetOnly { get { return "DerivedInternalPropertyGetOnly"; } }

        private string DerivedPrivatePropertyGetOnly { get { return "DerivedPrivatePropertyGetOnly"; } }

        public string DerivedPublicField;

        internal string DerivedInternalField;

        private string DerivedPrivateField;
    }
}
