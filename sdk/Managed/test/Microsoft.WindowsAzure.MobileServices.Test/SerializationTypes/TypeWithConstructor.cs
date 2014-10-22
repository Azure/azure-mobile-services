// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------


namespace Microsoft.WindowsAzure.MobileServices.Test
{
    public class TypeWithConstructor
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public TypeWithConstructor(string id, string name)
        {
            this.Id = id;
            this.Name = name;
        }
    }
}
