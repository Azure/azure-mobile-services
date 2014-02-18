// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Diagnostics;

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
            return string.Format("{0}:{1}", this.RegistrationName, this.RegistrationId);
        }

        public static StoredRegistrationEntry CreateFromString(string str)
        {
            var fields = str.Split(':');
            Debug.Assert(fields.Length == 2, "content from local storage is invalid.");
            return new StoredRegistrationEntry(fields[0], fields[1]);
        }
    }
}