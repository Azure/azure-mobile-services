//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Collections.Concurrent;

namespace Microsoft.WindowsAzure.MobileServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.Data.Xml.Dom;

    /// <summary>
    /// Define a class help to create/update/query/delete notification registrations
    /// </summary>
    /// Exceptions: 
    /// ArgumentException: when argument is not valid.
    /// RegistrationNotFoundException: When try to query/delete not existing registration(s).
    /// RegistrationAuthorizationException: When there is authorization error.
    /// RegistrationException: generatal registration operation error.
    public sealed class Push
    {
        private readonly RegistrationManager registrationManager;        

        internal Push(MobileServiceClient client)
            : this(client, string.Empty, null)
        {
        }

        private Push(MobileServiceClient client, string tileId, SecondaryTilesList tiles)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            this.Client = client;
            this.TileId = tileId;

            var storageManager = new LocalStorageManager(client.ApplicationUri.AbsoluteUri, tileId);
            var pushHttpClient = new PushHttpClient(client.HttpClient, client.Serializer);
            this.registrationManager = new RegistrationManager(pushHttpClient, storageManager);

            this.SecondaryTiles = tiles ?? new SecondaryTilesList(this);
        }

        public string TileId { get; private set; }

        public IDictionary<string, Push> SecondaryTiles { get; set; }

        private MobileServiceClient Client { get; set; }

        public Task RegisterNativeAsync(string channelUri)
        {
            return this.RegisterNativeAsync(channelUri, null);
        }

        public Task RegisterNativeAsync(string channelUri, IEnumerable<string> tags)
        {
            if (string.IsNullOrEmpty(channelUri))
            {
                throw new ArgumentNullException("channelUri");
            }

            var registration = new Registration(channelUri, tags);
            return registrationManager.RegisterAsync(registration);
        }

        public Task RegisterTemplateAsync(string channelUri, XmlDocument xmlTemplate, string templateName)
        {
            return this.RegisterTemplateAsync(channelUri, xmlTemplate, templateName, tags: null);
        }

        public Task RegisterTemplateAsync(string channelUri, XmlDocument xmlTemplate, string templateName, IEnumerable<string> tags)
        {
            if (xmlTemplate == null)
            {
                throw new ArgumentNullException("xmlTemplate");
            }

            return this.RegisterTemplateAsync(channelUri, xmlTemplate.GetXml(), templateName, tags);
        }

        public Task RegisterTemplateAsync(string channelUri, string xmlTemplate, string templateName)
        {
            return this.RegisterTemplateAsync(channelUri, xmlTemplate, templateName, null);
        }

        public Task RegisterTemplateAsync(string channelUri, string xmlTemplate, string templateName, IEnumerable<string> tags)
        {
            if (string.IsNullOrEmpty(channelUri))
            {
                throw new ArgumentNullException("channelUri");
            }

            if (string.IsNullOrWhiteSpace(xmlTemplate))
            {
                throw new ArgumentNullException("xmlTemplate");
            }

            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new ArgumentNullException("templateName");
            }

            var registration = new TemplateRegistration(channelUri, xmlTemplate, templateName, tags, null);
            return this.registrationManager.RegisterAsync(registration);

        }

        public Task UnregisterNativeAsync()
        {
            return this.UnregisterTemplateAsync(Registration.NativeRegistrationName);
        }

        public Task UnregisterTemplateAsync(string templateName)
        {
            return this.registrationManager.UnregisterAsync(templateName);
        }

        public Task UnregisterAllAsync(string channelUri)
        {
            if (string.IsNullOrEmpty(channelUri))
            {
                throw new ArgumentNullException("channelUri");
            }

            return this.registrationManager.DeleteRegistrationsForChannelAsync(channelUri);
        }

        public Task RegisterAsync(Registration registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException("registration");
            }

            if (string.IsNullOrEmpty(registration.ChannelUri))
            {
                throw new ArgumentNullException("registration.ChannelUri");
            }

            return this.registrationManager.RegisterAsync(registration);
        }

        public Task UnregisterAsync(Registration registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException("registration");
            }

            if (string.IsNullOrEmpty(registration.ChannelUri))
            {
                throw new ArgumentNullException("registration.ChannelUri");
            }

            return this.registrationManager.UnregisterAsync(registration.Name);
        }

        /// <summary>
        /// Collection of notificationHub for secondary tiles
        /// </summary>
        private class SecondaryTilesList : IDictionary<string, Push>
        {
            readonly Push parent;
            internal SecondaryTilesList(Push parent)
            {
                this.parent = parent;
            }

            readonly ConcurrentDictionary<string, Push> hubForTiles = new ConcurrentDictionary<string, Push>();

            public Push this[string tileId]
            {
                get
                {
                    if (string.IsNullOrEmpty(tileId))
                    {
                        throw new ArgumentNullException("tileId");
                    }

                    if (hubForTiles.ContainsKey(tileId))
                    {
                        return hubForTiles[tileId];
                    }
                    
                    var hubForTile = new Push(this.parent.Client, tileId, this);
                    return hubForTiles.GetOrAdd(tileId, hubForTile);
                }

                set
                {
                    throw new NotSupportedException();
                }
            }

            #region IDictionary interface

            public void Add(string key, Push value)
            {
                throw new NotSupportedException();
            }

            public bool ContainsKey(string key)
            {
                return hubForTiles.ContainsKey(key);
            }

            public ICollection<string> Keys
            {
                get { return hubForTiles.Keys; }
            }

            public bool Remove(string key)
            {
                Push hub;
                return this.hubForTiles.TryRemove(key, out hub);
            }

            public bool TryGetValue(string key, out Push value)
            {
                return this.hubForTiles.TryGetValue(key, out value);
            }

            public ICollection<Push> Values
            {
                get { return this.hubForTiles.Values; }
            }

            public void Add(KeyValuePair<string, Push> item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                this.hubForTiles.Clear();
            }

            public bool Contains(KeyValuePair<string, Push> item)
            {
                return this.hubForTiles.Contains(item);
            }

            public void CopyTo(KeyValuePair<string, Push>[] array, int arrayIndex)
            {
                foreach (KeyValuePair<string, Push> item in this.hubForTiles)
                {
                    array[arrayIndex++] = new KeyValuePair<string, Push>(item.Key, item.Value);
                }
            }

            public int Count
            {
                get { return this.hubForTiles.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool Remove(KeyValuePair<string, Push> item)
            {
                return this.Remove(item.Key);
            }

            public IEnumerator<KeyValuePair<string, Push>> GetEnumerator()
            {
                return this.hubForTiles.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.hubForTiles.GetEnumerator();
            }
            #endregion
        }
    }
}