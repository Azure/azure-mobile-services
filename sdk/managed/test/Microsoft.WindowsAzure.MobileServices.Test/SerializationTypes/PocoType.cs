// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    public class PocoType
    {
        public PocoType() :this(null)
        {
        }

        public PocoType(string valueToSet, bool onlySetSerializableMembers = false)
        {
            this.InternalField = null;
            this.PrivateField = null;

            if (valueToSet != null)
            {
                this.PublicField = "PublicField" + valueToSet;
                this.PublicProperty = "PublicProperty" + valueToSet;

                if (!onlySetSerializableMembers)
                {
                    this.InternalField = "InternalField" + valueToSet;
                    this.InternalProperty = "InternalProperty" + valueToSet;

                    this.PrivateField = "PrivateField" + valueToSet;
                    this.PrivateProperty = "PrivateProperty" + valueToSet;
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

        public long Id { get; set; }

        public string PublicProperty { get; set; }

        internal string InternalProperty { get; set; }

        private string PrivateProperty { get; set; }

        public string PublicPropertyGetOnly  { get { return "PublicPropertyGetOnly"; } }

        internal string InternalPropertyGetOnly { get { return "InternalPropertyGetOnly"; } }

        private string PrivatePropertyGetOnly { get { return "PrivatePropertyGetOnly"; } }

        public string PublicField;

        internal string InternalField;

        private string PrivateField;
    }
}
