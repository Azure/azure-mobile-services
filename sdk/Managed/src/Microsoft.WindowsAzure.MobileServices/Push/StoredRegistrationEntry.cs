// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal class StoredRegistrationEntry
    {
        public StoredRegistrationEntry(string name, string id)
        {
            this.RegistrationName = name;
            this.RegistrationId = id;
        }

        public string RegistrationName { get; set; }
        public string RegistrationId { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}