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
    ///     DeviceId: the latest deviceId used for creation.
    ///     Registrations: Json representation of Dictionary of StoredRegistrationEntry. (Registration name and RegistrationId)
    /// When create/delete is called, deviceId will be updated.
    /// When create/update/get/delete registrations, registrations value will be updated.
    /// </summary>
    internal class LocalStorageManager : ILocalStorageManager
    {
        internal const string StorageVersion = "v1.1.0";
        internal const string KeyNameVersion = "Version";
        internal const string KeyNamePushHandle = "Channel";
        internal const string KeyNameRegistrations = "Registrations";

        private readonly IApplicationStorage storage;

        private string pushHandle;
        private IDictionary<string, StoredRegistrationEntry> registrations;

        public LocalStorageManager(string name)
        {
            this.storage = Platform.Instance.GetNamedApplicationStorage(name);

            this.InitializeRegistrationInfoFromStorage();
        }

        public bool IsRefreshNeeded { get; set; }

        public string PushHandle
        {
            get
            {
                return this.pushHandle;
            }

            private set
            {
                if (this.pushHandle == null || !this.pushHandle.Equals(value))
                {
                    this.pushHandle = value;
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
                var found = this.registrations.FirstOrDefault(v => v.Value.RegistrationId.Equals(registrationId));
                if (!found.Equals(default(KeyValuePair<string, StoredRegistrationEntry>)))
                {
                    this.DeleteRegistrationByName(found.Key);
                }
            }
        }

        public void UpdateRegistrationByName(string registrationName, string registrationId, string registrationDeviceId)
        {
            StoredRegistrationEntry cacheReg = new StoredRegistrationEntry(registrationName, registrationId);

            lock (this.registrations)
            {
                this.registrations[registrationName] = cacheReg;
                this.pushHandle = registrationDeviceId;
                this.Flush();
            }
        }

        public void UpdateRegistrationByRegistrationId(string registrationId, string registrationName, string registrationDeviceId)
        {
            lock (this.registrations)
            {
                // update registration is registrationId is in cached registrations, otherwise create new one
                var found = this.registrations.FirstOrDefault(v => v.Value.RegistrationId.Equals(registrationId));
                if (!found.Equals(default(KeyValuePair<string, StoredRegistrationEntry>)))
                {
                    this.UpdateRegistrationByName(found.Key, found.Value.RegistrationId, registrationDeviceId);
                }
                else
                {
                    this.UpdateRegistrationByName(registrationName, registrationId, registrationDeviceId);
                }
            }
        }

        public void ClearRegistrations()
        {
            lock (this.registrations)
            {
                this.registrations.Clear();
                this.Flush();
            }
        }

        public void RefreshFinished(string refreshedDeviceId)
        {
            this.PushHandle = refreshedDeviceId;
            this.IsRefreshNeeded = false;
        }

        private void Flush()
        {
            lock (this.registrations)
            {
                this.storage.WriteSetting(KeyNameVersion, StorageVersion);
                this.storage.WriteSetting(KeyNamePushHandle, this.pushHandle);

                string str = JsonConvert.SerializeObject(this.registrations);
                this.storage.WriteSetting(KeyNameRegistrations, str);

                this.storage.Save();
            }
        }

        private void InitializeRegistrationInfoFromStorage()
        {
            // This method is only called from the constructor. As long as this is true, no locks are needed in this method.
            this.registrations = new Dictionary<string, StoredRegistrationEntry>();

            // Read deviceId
            object channelObject;
            if (this.storage.TryReadSetting(KeyNamePushHandle, out channelObject))
            {
                this.pushHandle = (string)channelObject;
            }
            else
            {
                this.IsRefreshNeeded = true;
                return;
            }

            // Verify storage version
            object versionObject;
            string version;
            if (this.storage.TryReadSetting(KeyNameVersion, out versionObject))
            {
                version = (string)versionObject;
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

            // read registrations
            object registrationsObject;
            string regsStr = null;
            if (this.storage.TryReadSetting(KeyNameRegistrations, out registrationsObject))
            {
                regsStr = (string)registrationsObject;
                this.IsRefreshNeeded = false;
            }

            if (!string.IsNullOrEmpty(regsStr))
            {
                this.registrations = JsonConvert.DeserializeObject<IDictionary<string, StoredRegistrationEntry>>(regsStr);
            }
        }
    }
}
