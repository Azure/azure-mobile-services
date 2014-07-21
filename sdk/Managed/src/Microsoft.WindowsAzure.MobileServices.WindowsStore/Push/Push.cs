//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using Windows.ApplicationModel;

namespace Microsoft.WindowsAzure.MobileServices
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.Data.Xml.Dom;

    /// <summary>
    /// Define a class help to create/update/delete notification registrations
    /// </summary>
    public sealed class Push
    {
        internal readonly RegistrationManager RegistrationManager;

        private const string PrimaryChannelId = "$Primary";

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
            if (string.IsNullOrEmpty(tileId))
            {
                tileId = PrimaryChannelId;
            }

            string name = string.Format("{0}-PushContainer-{1}-{2}", Package.Current.Id.Name, client.ApplicationUri.Host, tileId);
            var storageManager = new LocalStorageManager(name);
            var pushHttpClient = new PushHttpClient(client);
            this.RegistrationManager = new RegistrationManager(pushHttpClient, storageManager);

            this.SecondaryTiles = tiles ?? new SecondaryTilesList(this);
        }

        /// <summary>
        /// The TileId associated with this specific object. String.Empty is default if not created via SecondaryTiles property.
        /// </summary>
        public string TileId { get; private set; }

        /// <summary>
        /// Access this member with an indexer to access secondary tile registrations. Example: <code>push.SecondaryTiles["tileName"].RegisterNativeAsync("mychannelUri");</code>
        /// </summary>
        public IDictionary<string, Push> SecondaryTiles { get; set; }

        private MobileServiceClient Client { get; set; }

        /// <summary>
        /// Register a particular channelUri
        /// </summary>
        /// <param name="channelUri">The channelUri to register</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterNativeAsync(string channelUri)
        {
            return this.RegisterNativeAsync(channelUri, null);
        }

        /// <summary>
        /// Register a particular channelUri
        /// </summary>
        /// <param name="channelUri">The channelUri to register</param>
        /// <param name="tags">The tags to register to receive notifications from</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterNativeAsync(string channelUri, IEnumerable<string> tags)
        {
            if (string.IsNullOrWhiteSpace(channelUri))
            {
                throw new ArgumentNullException("channelUri");
            }

            var registration = new WnsRegistration(channelUri, tags);
            return this.RegistrationManager.RegisterAsync(registration);
        }

        /// <summary>
        /// Register a particular channelUri with a template
        /// </summary>
        /// <param name="channelUri">The channelUri to register</param>
        /// <param name="xmlTemplate">The XmlDocument defining the template</param>
        /// <param name="templateName">The template name</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterTemplateAsync(string channelUri, XmlDocument xmlTemplate, string templateName)
        {
            return this.RegisterTemplateAsync(channelUri, xmlTemplate, templateName, tags: null);
        }

        /// <summary>
        /// Register a particular channelUri with a template
        /// </summary>
        /// <param name="channelUri">The channelUri to register</param>
        /// <param name="xmlTemplate">The XmlDocument defining the template</param>
        /// <param name="templateName">The template name</param>
        /// <param name="tags">The tags to register to receive notifications from</param>
        /// <returns>Task that completes when registration is complete</returns>        
        public Task RegisterTemplateAsync(string channelUri, XmlDocument xmlTemplate, string templateName, IEnumerable<string> tags)
        {
            if (xmlTemplate == null)
            {
                throw new ArgumentNullException("xmlTemplate");
            }

            return this.RegisterTemplateAsync(channelUri, xmlTemplate.GetXml(), templateName, tags);
        }

        /// <summary>
        /// Register a particular channelUri with a template
        /// </summary>
        /// <param name="channelUri">The channelUri to register</param>
        /// <param name="xmlTemplate">The string defining the template</param>
        /// <param name="templateName">The template name</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterTemplateAsync(string channelUri, string xmlTemplate, string templateName)
        {
            return this.RegisterTemplateAsync(channelUri, xmlTemplate, templateName, null);
        }

        /// <summary>
        /// Register a particular channelUri with a template
        /// </summary>
        /// <param name="channelUri">The channelUri to register</param>
        /// <param name="xmlTemplate">The string defining the template</param>
        /// <param name="templateName">The template name</param>
        /// <param name="tags">The tags to register to receive notifications from</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterTemplateAsync(string channelUri, string xmlTemplate, string templateName, IEnumerable<string> tags)
        {
            if (string.IsNullOrWhiteSpace(channelUri))
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

            var registration = new WnsTemplateRegistration(channelUri, xmlTemplate, templateName, tags);
            return this.RegistrationManager.RegisterAsync(registration);
        }

        /// <summary>
        /// Unregister any registrations previously registered from this device
        /// </summary>
        /// <returns>Task that completes when unregister is complete</returns>
        public Task UnregisterNativeAsync()
        {
            return this.UnregisterTemplateAsync(Registration.NativeRegistrationName);
        }

        /// <summary>
        /// Unregister any registrations with given templateName registered from this device
        /// </summary>
        /// <param name="templateName">The template name</param>
        /// <returns>Task that completes when unregister is complete</returns>
        public Task UnregisterTemplateAsync(string templateName)
        {
            return this.RegistrationManager.UnregisterAsync(templateName);
        }

        /// <summary>
        /// Unregister any registrations with given channelUri
        /// </summary>
        /// <param name="channelUri">The channel Uri</param>
        /// <returns>Task that completes when unregister is complete</returns>
        public Task UnregisterAllAsync(string channelUri)
        {
            if (string.IsNullOrWhiteSpace(channelUri))
            {
                throw new ArgumentNullException("channelUri");
            }

            return this.RegistrationManager.DeleteRegistrationsForChannelAsync(channelUri);
        }

        /// <summary>
        /// Register for notifications
        /// </summary>
        /// <param name="registration">The object defining the registration</param>
        /// <returns>Task that will complete when the registration is completed</returns>
        public Task RegisterAsync(Registration registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException("registration");
            }

            if (string.IsNullOrWhiteSpace(registration.PushHandle))
            {
                throw new ArgumentNullException("registration.ChannelUri");
            }

            return this.RegistrationManager.RegisterAsync(registration);
        }

        /// <summary>
        /// DEBUG-ONLY: List the registrations made with the service for a channelUri
        /// </summary>
        /// <param name="channelUri">The channelUri to check for</param>
        /// <returns>List of registrations</returns>
        public Task<List<Registration>> ListRegistrationsAsync(string channelUri)
        {
            return this.RegistrationManager.ListRegistrationsAsync(channelUri);
        }

        /// <summary>
        /// Collection of Push objects for secondary tiles
        /// </summary>
        private class SecondaryTilesList : IDictionary<string, Push>
        {
            private readonly Push parent;
            private readonly ConcurrentDictionary<string, Push> hubForTiles = new ConcurrentDictionary<string, Push>();

            internal SecondaryTilesList(Push parent)
            {
                this.parent = parent;
            }                        

            public ICollection<string> Keys
            {
                get { return this.hubForTiles.Keys; }
            }

            public ICollection<Push> Values
            {
                get { return this.hubForTiles.Values; }
            }

            public int Count
            {
                get { return this.hubForTiles.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            /// <summary>
            /// Indexer for creating/looking up tileId-specific Push objects
            /// </summary>
            /// <param name="tileId">The tileId of a secondary tile</param>
            /// <returns>Push object for performing registrations on</returns>
            public Push this[string tileId]
            {
                get
                {
                    if (string.IsNullOrWhiteSpace(tileId))
                    {
                        throw new ArgumentNullException("tileId");
                    }

                    if (this.hubForTiles.ContainsKey(tileId))
                    {
                        return this.hubForTiles[tileId];
                    }

                    var hubForTile = new Push(this.parent.Client, tileId, this);
                    return this.hubForTiles.GetOrAdd(tileId, hubForTile);
                }

                set
                {
                    throw new NotSupportedException();
                }
            }

            public void Add(string key, Push value)
            {
                throw new NotSupportedException();
            }

            public bool ContainsKey(string tileId)
            {
                return this.hubForTiles.ContainsKey(tileId);
            }            

            public bool Remove(string tileId)
            {
                Push hub;
                return this.hubForTiles.TryRemove(tileId, out hub);
            }

            public bool TryGetValue(string tileId, out Push value)
            {
                return this.hubForTiles.TryGetValue(tileId, out value);
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
        }
    }
}