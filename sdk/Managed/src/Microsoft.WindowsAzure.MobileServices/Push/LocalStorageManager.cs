// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices
{
    

    /// <summary>
    /// The value will be stored in the following keys:
    ///     Version: the storage version
    ///     ChannelUri: the latest channelUri used for creation.
    ///     Registrations: {registrationName1}:{registrationId1};{registrationName2}:{registrationId2}
    ///  
    /// When create/delete is called, channelUri will be update.
    /// When create/update/get/delete registrations, registrations value will be updated.
    /// </summary>
    internal class LocalStorageManager : ILocalStorageManager
    {
        internal const string StorageVersion = "v1.1.0";

        internal const string KeyNameVersion = "Version";
        internal const string KeyNameChannelUri = "ChannelUri";
        internal const string KeyNameRegistrations = "Registrations";

        private string channelUri;

        private IDictionary<string, StoredRegistrationEntry> registrations;

        private readonly IApplicationStorage storage;

        public LocalStorageManager(string name)
        {
            this.storage = Platform.Instance.GetNamedApplicationStorage(name);

            this.InitializeRegistrationInfoFromStorage();
        }

        public bool IsRefreshNeeded { get; internal set; }

        public string ChannelUri
        {
            get
            {
                return this.channelUri;
            }
            set
            {
                if (this.channelUri == null || !this.channelUri.Equals(value))
                {
                    this.channelUri = value;
                    this.Flush();
                }
            }
        }

        public StoredRegistrationEntry GetRegistration(string registrationName)
        {
            lock (this.registrations)
            {
                StoredRegistrationEntry reg;
                if (this.registrations.TryGetValue(registrationName, out reg))
                {
                    return reg;
                }
            }

            return null;
        }

        public void DeleteRegistrationByName(string registrationName)
        {
            lock (this.registrations)
            {
                if (this.registrations.Remove(registrationName))
                {
                    this.Flush();
                }
            }
        }

        public void DeleteRegistrationByRegistrationId(string registrationId)
        {
            lock (this.registrations)
            {
                var found = registrations.FirstOrDefault(v => v.Value.RegistrationId.Equals(registrationId));
                if (!found.Equals(default(KeyValuePair<string, StoredRegistrationEntry>)))
                {
                    this.DeleteRegistrationByName(found.Key);
                }
            }
        }

        public void UpdateRegistrationByName(string registrationName, string registrationId, string registrationChannelUri)
        {
            StoredRegistrationEntry cacheReg = new StoredRegistrationEntry(registrationName, registrationId);

            lock (this.registrations)
            {
                this.registrations[registrationName] = cacheReg;
                this.channelUri = registrationChannelUri;
                this.Flush();
            }
        }

        public void UpdateRegistrationByRegistrationId(string registrationId, string registrationName, string registrationChannelUri)
        {
            lock (this.registrations)
            {
                // update registration is registrationId is in cached registrations, otherwise create new one
                var found = registrations.FirstOrDefault(v => v.Value.RegistrationId.Equals(registrationId));
                if (!found.Equals(default(KeyValuePair<string, StoredRegistrationEntry>)))
                {
                    this.UpdateRegistrationByName(found.Key, found.Value.RegistrationId, registrationChannelUri);
                }
                else
                {
                    this.UpdateRegistrationByName(registrationName, registrationId, registrationChannelUri);
                }
            }
        }

        public void ClearRegistrations()
        {
            lock (this.registrations)
            {
                this.registrations.Clear();
                Flush();
            }
        }

        public void RefreshFinished(string refreshedChannelUri)
        {
            this.ChannelUri = refreshedChannelUri;
            this.IsRefreshNeeded = false;
        }

        private void Flush()
        {
            lock (this.registrations)
            {
                this.storage.WriteSetting(KeyNameVersion, StorageVersion);
                this.storage.WriteSetting(KeyNameChannelUri, this.channelUri);

                string str = JsonConvert.SerializeObject(this.registrations);
                this.storage.WriteSetting(KeyNameRegistrations, str);

                this.storage.Save();
            }
        }

        private void InitializeRegistrationInfoFromStorage()
        {
            // This method is only called from the constructor. As long as this is true, no locks are needed in this method.
            this.registrations = new Dictionary<string, StoredRegistrationEntry>();

            // Read channelUri
            object channelObject;
            if (this.storage.TryReadSetting(KeyNameChannelUri, out channelObject))
            {
                this.channelUri = (string)channelObject;
                this.IsRefreshNeeded = true;
                return;
            }

            // Verify storage version
            object versionObject;
            string version;
            if (this.storage.TryReadSetting(KeyNameVersion, out versionObject))
            {
                version = (string)channelObject;
            }
            else
            {
                this.IsRefreshNeeded = true;
                return;
            }

            if (!string.Equals(version, StorageVersion, System.StringComparison.OrdinalIgnoreCase))
            {
                this.IsRefreshNeeded = true;
                return;
            }

            this.IsRefreshNeeded = false;

            // read registrations
            object registrationsObject;
            string regsStr;
            if (this.storage.TryReadSetting(KeyNameRegistrations, out registrationsObject))
            {
                regsStr = (string)channelObject;
            }
            else
            {
                throw new Exception("Something is really really wrong");
            }

            if (!string.IsNullOrEmpty(regsStr))
            {
                this.registrations = JsonConvert.DeserializeObject<IDictionary<string, StoredRegistrationEntry>>(regsStr);
            }
        }
    }
}
