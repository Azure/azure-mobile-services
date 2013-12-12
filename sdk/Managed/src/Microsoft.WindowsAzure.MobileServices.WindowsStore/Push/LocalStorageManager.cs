// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Windows.ApplicationModel;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// The value will be stored in the following keys:
    ///     Version: the storage version
    ///     ChannelUri: the latest channelUri used for creation.
    ///     Registrations: {registartionName1}:{registartionId1};{registartionName2}:{registartionId2}
    ///  
    /// When create/delete is called, channelUri will be udpate.
    /// When create/update/get/delete registations, registartions value will be updated.
    /// </summary>
    internal class LocalStorageManager
    {
        internal const string StorageVersion = "v1.0.0";

        internal const string PrimaryChannelId = "$Primary";
        internal const string KeyNameVersion = "Version";
        internal const string KeyNameChannelUri = "ChannelUri";
        internal const string KeyNameRegistrations = "Registrations";

        private ConcurrentDictionary<string, StoredRegistrationEntry> registrations;

        private readonly object flushLock = new object();
        private string channelUri;

        internal static IStaticLocalStorage StaticStorage;

        public LocalStorageManager(string applicationUri, string tileId)
        {
            if (string.IsNullOrEmpty(tileId))
            {
                tileId = PrimaryChannelId;
            }

            string name = string.Format("{0}-PushContainer-{1}-{2}", Package.Current.Id.Name, applicationUri, tileId);
            this.StorageValues = StaticStorage != null ? StaticStorage.CreateContainer(name) : ApplicationData.Current.LocalSettings.CreateContainer(name, ApplicationDataCreateDisposition.Always).Values;

            this.ReadContent();
        }

        private IPropertySet StorageValues
        {
            get;
            set;
        }

        public bool IsRefreshNeeded
        {
            get;
            private set;
        }

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
            StoredRegistrationEntry reg;
            if (this.registrations.TryGetValue(registrationName, out reg))
            {
                return reg;
            }

            return null;
        }

        public bool DeleteRegistration(string registrationName)
        {
            StoredRegistrationEntry reg;
            if (this.registrations.TryRemove(registrationName, out reg))
            {
                this.Flush();
                return true;
            }

            return false;
        }

        public void UpdateRegistration<T>(string registrationName, ref T registration) where T : Registration
        {
            if (registration == null)
            {
                this.DeleteRegistration(registrationName);
                return;
            }

            StoredRegistrationEntry cacheReg = new StoredRegistrationEntry(registrationName, registration.RegistrationId);
            this.registrations.AddOrUpdate(registrationName, cacheReg, (key, oldValue) => cacheReg);

            this.channelUri = registration.ChannelUri;
            this.Flush();
        }

        public void UpdateRegistration(Registration registration)
        {
            // update registation is registartionId is in cached registartions, otherwise create new one
            var found = registrations.FirstOrDefault(v => v.Value.RegistrationId.Equals(registration.RegistrationId));
            if (!found.Equals(default(KeyValuePair<string, StoredRegistrationEntry>)))
            {
                this.UpdateRegistration(found.Key, ref registration);
            }
            else
            {
                this.UpdateRegistration(registration.Name, ref registration);
            }
        }

        public void ClearRegistrations()
        {
            this.registrations.Clear();
            Flush();
        }

        public void RefreshFinished(string refreshedChannelUri)
        {
            this.ChannelUri = refreshedChannelUri;
            this.IsRefreshNeeded = false;
        }

        public void Flush()
        {
            lock (this.flushLock)
            {
                SetContent(this.StorageValues, KeyNameVersion, StorageVersion);
                SetContent(this.StorageValues, KeyNameChannelUri, this.channelUri);

                string str = string.Empty;
                if (this.registrations != null)
                {
                    var entries = this.registrations.Select(v => v.Value.ToString());
                    str = string.Join(";", entries);
                }

                SetContent(this.StorageValues, KeyNameRegistrations, str);
            }
        }

        private void ReadContent()
        {
            this.registrations = new ConcurrentDictionary<string, StoredRegistrationEntry>();

            // Read channelUri
            this.channelUri = ReadContent(this.StorageValues, KeyNameChannelUri);

            // Verify storage version
            var version = ReadContent(this.StorageValues, KeyNameVersion);
            if (!string.Equals(version, StorageVersion, System.StringComparison.OrdinalIgnoreCase))
            {
                this.IsRefreshNeeded = true;
                return;
            }

            this.IsRefreshNeeded = false;

            // read registrations
            var regsStr = ReadContent(this.StorageValues, KeyNameRegistrations);
            if (!string.IsNullOrEmpty(regsStr))
            {
                var entries = regsStr.Split(';');
                foreach (string entry in entries)
                {
                    var reg = StoredRegistrationEntry.CreateFromString(entry);
                    this.registrations.AddOrUpdate(reg.RegistrationName, reg, (key, oldValue) => reg);
                }
            }
        }

        internal static string ReadContent(IPropertySet set, string key)
        {
            if (set.ContainsKey(key))
            {
                return set[key] as string;
            }

            return string.Empty;
        }

        internal static void SetContent(IPropertySet set, string key, string value)
        {
            if (set.ContainsKey(key))
            {
                set[key] = value;
            }
            else
            {
                set.Add(key, value);
            }
        }
    }
}
