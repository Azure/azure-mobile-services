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
    using Newtonsoft.Json.Linq;
    using Windows.Data.Xml.Dom;

    /// <summary>
    /// Define a class help to create/update/delete notification registrations
    /// </summary>
    public sealed class Push
    {
        internal readonly PushHttpClient PushHttpClient;

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

            this.PushHttpClient = new PushHttpClient(client);

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

        /// <summary>
        /// Installation Id used to register the device with Notification Hubs
        /// </summary>
        public string InstallationId
        {
            get
            {
                return this.Client.applicationInstallationId;
            }
        }

        private MobileServiceClient Client { get; set; }

        /// <summary>
        /// Register an Installation with particular channelUri
        /// </summary>
        /// <param name="channelUri">The channelUri to register</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterAsync(string channelUri)
        {
            return this.RegisterAsync(channelUri, null, null);
        }

        /// <summary>
        /// Register an Installation with particular channelUri and templates
        /// </summary>
        /// <param name="channelUri">The channelUri to register</param>
        /// <param name="templates">JSON with one more templates to register</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterAsync(string channelUri, JObject templates)
        {
            return this.RegisterAsync(channelUri, templates, null);
        }

        /// <summary>
        /// Register an Installation with particular channelUri, templates and secondaryTiles
        /// </summary>
        /// <param name="channelUri">The channelUri to register</param>
        /// <param name="templates">JSON with one more templates to register</param>
        /// <param name="secondaryTiles">JSON with one more templates to register secondaryTiles</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterAsync(string channelUri, JObject templates, JObject secondaryTiles)
        {
            if (string.IsNullOrWhiteSpace(channelUri))
            {
                throw new ArgumentNullException("channelUri");
            }
            JObject installation = new JObject();
            installation[PushInstallationProperties.PUSHCHANNEL] = channelUri;
            installation[PushInstallationProperties.PLATFORM] = Platform.Instance.PushUtility.GetPlatform();
            if (templates != null)
            {
                installation[PushInstallationProperties.TEMPLATES] = templates;
            }
            if (secondaryTiles != null)
            {
                installation[PushInstallationProperties.SECONDARYTILES] = secondaryTiles;
            }
            return this.PushHttpClient.CreateOrUpdateInstallationAsync(installation);
        }

        /// <summary>
        /// Unregister any installations previously registered from this device
        /// </summary>
        /// <returns>Task that completes when unregister is complete</returns>
        public Task UnregisterAsync()
        {
            return this.PushHttpClient.DeleteInstallationAsync();
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