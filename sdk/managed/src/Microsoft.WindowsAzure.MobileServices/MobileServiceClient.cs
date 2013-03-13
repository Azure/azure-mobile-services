// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides basic access to a Windows Azure Mobile Service.
    /// </summary>
    public class MobileServiceClient : IMobileServiceClient, IDisposable
    {
        /// <summary>
        /// Name of the config setting that stores the installation ID.
        /// </summary>
        private const string ConfigureAsyncInstallationConfigPath = "MobileServices.Installation.config";

        /// <summary>
        /// Name of the JSON member in the config setting that stores the
        /// installation ID.
        /// </summary>
        private const string ConfigureAsyncApplicationIdKey = "applicationInstallationId";
        
        /// <summary>
        /// The id used to identify this installation of the application to 
        /// provide telemetry data.
        /// </summary>
        internal static string applicationInstallationId = GetApplicationInstallationId();

        /// <summary>
        /// Gets the Uri to the Mobile Services application that is provided by
        /// the call to MobileServiceClient(...).
        /// </summary>
        public Uri ApplicationUri { get; private set; }

        /// <summary>
        /// Gets the Mobile Services application's name that is provided by the
        /// call to MobileServiceClient(...).
        /// </summary>
        public string ApplicationKey { get; private set; }

        /// <summary>
        /// The current authenticated user provided after a successful call to
        /// MobileServiceClient.Login().
        /// </summary>
        public MobileServiceUser CurrentUser { get; set; }

        /// <summary>
        /// Gets or sets the settings used for serialization.
        /// </summary>
        public MobileServiceJsonSerializerSettings SerializerSettings
        {
            get
            {
                return this.Serializer.SerializerSettings;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.Serializer.SerializerSettings = value;
            }
        }

        /// <summary>
        /// Gets the serializer that is used with the table.
        /// </summary>
        internal MobileServiceSerializer Serializer { get; private set; }

        /// <summary>
        /// Gets the <see cref="MobileServiceHttpClient"/> associated with this client.
        /// </summary>
        internal MobileServiceHttpClient HttpClient { get; private set; }
        
        /// <summary>
        /// Initializes a new instance of the MobileServiceClient class.
        /// </summary>
        /// <param name="applicationUrl">
        /// The URI for the Windows Azure Mobile Service.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "Enables easier copy/paste getting started workflow")]
        public MobileServiceClient(string applicationUrl)
            : this(new Uri(applicationUrl))
        {
        }

        /// <summary>
        /// Initializes a new instance of the MobileServiceClient class.
        /// </summary>
        /// <param name="applicationUri">
        /// The URI for the Windows Azure Mobile Service.
        /// </param>
        public MobileServiceClient(Uri applicationUri)
            : this(applicationUri, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MobileServiceClient class.
        /// </summary>
        /// <param name="applicationUrl">
        /// The URI for the Windows Azure Mobile Service.
        /// </param>
        /// <param name="applicationKey">
        /// The application key for the Windows Azure Mobile Service.
        /// </param>
        /// <param name="handlers">
        /// Chain of <see cref="HttpMessageHandler" /> instances. 
        /// All but the last should be <see cref="DelegatingHandler"/>s. 
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "Enables easier copy/paste getting started workflow")]
        public MobileServiceClient(string applicationUrl, string applicationKey, params HttpMessageHandler[] handlers)
            : this(new Uri(applicationUrl), applicationKey, handlers)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MobileServiceClient class.
        /// </summary>
        /// <param name="applicationUri">
        /// The URI for the Windows Azure Mobile Service.
        /// </param>
        /// <param name="applicationKey">
        /// The application key for the Windows Azure Mobile Service.
        /// </param> 
        /// <param name="handlers">
        /// Chain of <see cref="HttpMessageHandler" /> instances. 
        /// All but the last should be <see cref="DelegatingHandler"/>s. 
        /// </param>
        public MobileServiceClient(Uri applicationUri, string applicationKey, params HttpMessageHandler[] handlers)
        {
            if (applicationUri == null)
            {
                throw new ArgumentNullException("applicationUri");
            }

            this.ApplicationUri = applicationUri;
            this.ApplicationKey = applicationKey;

            this.HttpClient = new MobileServiceHttpClient(this, handlers.CreatePipeline());
            this.Serializer = new MobileServiceSerializer();
        }

        /// <summary>
        /// Returns a <see cref="IMobileServiceTable"/> instance, which provides 
        /// untyped data operations for that table.
        /// </summary>
        /// <param name="tableName">
        /// The name of the table.
        /// </param>
        /// <returns>
        /// The table.
        /// </returns>
        public IMobileServiceTable GetTable(string tableName)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }
            
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.MobileServiceClient_EmptyArgument,
                        "tableName"));
            }

            return new MobileServiceTable(tableName, this);
        }

        /// <summary>
        /// Returns a <see cref="IMobileServiceTable{T}"/> instance, which provides 
        /// strongly typed data operations for that table.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the instances in the table.
        /// </typeparam>
        /// <returns>
        /// The table.
        /// </returns>
        public IMobileServiceTable<T> GetTable<T>()
        {
            string tableName = this.SerializerSettings.ContractResolver.ResolveTableName(typeof(T));
            return new MobileServiceTable<T>(tableName, this);
        }

        /// <summary>
        /// Logs a user into a Windows Azure Mobile Service with the provider and optional token object.
        /// </summary>
        /// <param name="provider">
        /// Authentication provider to use.
        /// </param>
        /// <param name="token">
        /// Provider specific object with existing OAuth token to log in with.
        /// </param>
        /// <remarks>
        /// The token object needs to be formatted depending on the specific provider. These are some
        /// examples of formats based on the providers:
        /// <list type="bullet">
        ///   <item>
        ///     <term>MicrosoftAccount</term>
        ///     <description><code>{"authenticationToken":"&lt;the_authentication_token&gt;"}</code></description>
        ///   </item>
        ///   <item>
        ///     <term>Facebook</term>
        ///     <description><code>{"access_token":"&lt;the_access_token&gt;"}</code></description>
        ///   </item>
        ///   <item>
        ///     <term>Google</term>
        ///     <description><code>{"access_token":"&lt;the_access_token&gt;"}</code></description>
        ///   </item>
        /// </list>
        /// </remarks>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification = "Login is more appropriate than LogOn for our usage.")]
        public Task<MobileServiceUser> LoginAsync(MobileServiceAuthenticationProvider provider, JObject token)
        {
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }

            MobileServiceTokenAuthentication auth = new MobileServiceTokenAuthentication(this, provider, token);
            return auth.LoginAsync();
        }

        /// <summary>
        /// Log a user out.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Logout", Justification = "Logout is preferred by design")]
        public void Logout()
        {
            this.CurrentUser = null;
        }        

        /// <summary>
        /// Implemenation of <see cref="IDisposable"/>
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implemenation of <see cref="IDisposable"/> for
        /// derived classes to use.
        /// </summary>
        /// <param name="disposing">
        /// Indicates if being called from the Dispose() method
        /// or the finalizer.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                this.HttpClient.Dispose();
            }
        }

        /// <summary>
        /// Gets the ID used to identify this installation of the
        /// application to provide telemetry data.  It will either be retrieved
        /// from local settings or generated fresh.
        /// </summary>
        /// <returns>
        /// An installation ID.
        /// </returns>
        private static string GetApplicationInstallationId()
        {
            // Try to get the AppInstallationId from settings
            string installationId = null;
            object setting = null;

            IApplicationStorage applicationStorage = Platform.Instance.ApplicationStorage;

            if (applicationStorage.TryReadSetting(ConfigureAsyncInstallationConfigPath, out setting))
            {
                JToken config = null;
                try
                {
                    config = JToken.Parse(setting as string);
                    installationId = (string)config[ConfigureAsyncApplicationIdKey];
                }
                catch (Exception)
                {
                }
            }

            // Generate a new AppInstallationId if we failed to find one
            if (installationId == null)
            {
                installationId = Guid.NewGuid().ToString();
                JObject jobject = new JObject();
                jobject[ConfigureAsyncApplicationIdKey] = installationId;
                string configText = jobject.ToString();
                applicationStorage.WriteSetting(ConfigureAsyncInstallationConfigPath, configText);
            }

            return installationId;
        }        
    }
}
