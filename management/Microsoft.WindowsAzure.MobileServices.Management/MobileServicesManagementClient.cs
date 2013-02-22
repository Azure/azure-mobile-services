using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Management
{
    public class MobileServicesManagementClient
    {
        static readonly string DefaultMobileServiceManagementEndpoint = "https://management.core.windows.net";
        static readonly string DefaultSqlManagementEndpoint = "https://management.core.windows.net:8443";
        static readonly string DefaultSqlManagementEndpoint2 = "https://management.database.windows.net:8443";
        static readonly string WindowsAzureNs = "http://schemas.microsoft.com/windowsazure";
        static readonly string SqlAzureNs = "http://schemas.microsoft.com/sqlazure/2010/12/";
        static readonly string DefaultSqlDatabaseHostSuffix = ".database.windows.net";

        public string SubscriptionId { get; set; }
        public X509Certificate2 ManagementCertificate { get; set; }
        public string MobileServiceManagementEndpoint { get; set; }
        public string SqlManagementEndpoint { get; set; }
        public string SqlManagementEndpoint2 { get; set; }
        public string SqlDatabaseHostSuffix { get; set; }

        public MobileServicesManagementClient()
            : this(null, null)
        {
            // empty
        }

        public MobileServicesManagementClient(string subscriptionId, X509Certificate2 managementCertificate)
        {
            this.SubscriptionId = subscriptionId;
            this.ManagementCertificate = managementCertificate;
            this.MobileServiceManagementEndpoint = DefaultMobileServiceManagementEndpoint;
            this.SqlManagementEndpoint = DefaultSqlManagementEndpoint;
            this.SqlManagementEndpoint2 = DefaultSqlManagementEndpoint2;
            this.SqlDatabaseHostSuffix = DefaultSqlDatabaseHostSuffix;
        }

        public async Task TestDatabaseConnectionAsync(string sqlServerName, string username, SecureString password)
        {
            if (string.IsNullOrEmpty(sqlServerName))
            {
                throw new ArgumentNullException("sqlServerName");
            }

            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username");
            }

            if (password == null || password.Length == 0)
            {
                throw new ArgumentNullException("password");
            }

            WebRequestHandler handler = new WebRequestHandler();
            handler.CookieContainer = new CookieContainer();
            HttpClient client = new HttpClient(handler);

            HttpRequestMessage request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri(
                "https://" + sqlServerName + this.SqlDatabaseHostSuffix +
                "/v1/ManagementService.svc/GetAccessToken");
            request.Headers.Add("sqlauthorization", "Basic " +
                Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + CreateMobileServiceParameters.ConvertToUnsecureString(password))));
            HttpResponseMessage response = await client.SendAsync(request);
            XElement body = await this.ProcessXmlResponseAsync(response);
            string accessToken = body.Value;

            request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri(
                "https://" + sqlServerName + this.SqlDatabaseHostSuffix +
                "/v1/ManagementService.svc/Server2('" + sqlServerName + "')/Servers()?$top=1");
            request.Headers.Add("AccessToken", accessToken);
            response = await client.SendAsync(request);
            await this.ProcessXmlResponseAsync(response);
        }

        public async Task<List<SqlDatabaseParameters>> GetSqlDatabasesAsync()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri(
                this.SqlManagementEndpoint2 +
                "/" + this.SubscriptionId +
                "/servers");
            request.Headers.Add("x-ms-version", "1.0");
            HttpResponseMessage response = await this.MakeManagementRequestAsync(request);
            XElement body = await this.ProcessXmlResponseAsync(response);

            List<SqlDatabaseParameters> result = new List<SqlDatabaseParameters>();
            foreach (XElement server in body.Elements())
            {
                string name = server.Element(XName.Get("Name", SqlAzureNs)).Value;
                string login = server.Element(XName.Get("AdministratorLogin", SqlAzureNs)).Value;
                string location = server.Element(XName.Get("Location", SqlAzureNs)).Value;
                List<string> dbs = await this.GetSqlDatabasesAsync(name);
                result.AddRange(dbs.FindAll((db) => { return db != "master"; }).Select((db) => {
                    SqlDatabaseParameters parameters = new SqlDatabaseParameters();
                    parameters.Location = location;
                    parameters.ServerName = name;
                    parameters.DatabaseName = db;
                    parameters.AdministratorLogin = login;
                    return parameters;
                }));
            }

            return result;
        }

        public async Task<List<string>> GetSqlDatabasesAsync(string sqlServerName)
        {
            if (string.IsNullOrEmpty(sqlServerName))
            {
                throw new ArgumentNullException("sqlServerName");
            }

            HttpRequestMessage request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri(
                this.SqlManagementEndpoint +
                "/" + this.SubscriptionId +
                "/services/sqlservers/servers" +
                "/" + sqlServerName +
                "/databases?contentview=generic");
            request.Headers.Add("x-ms-version", "2012-03-01");
            HttpResponseMessage response = await this.MakeManagementRequestAsync(request);
            XElement body = await this.ProcessXmlResponseAsync(response);

            try
            {
                List<string> result = body.Elements().Select((element) => element.Element(XName.Get("Name", WindowsAzureNs)).Value).ToList();
                return result;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.UnexpectedResponse,
                    body.ToString()), e);
            }
        }

        public async Task<CreateMobileServiceResult> CreateMobileServiceAsync(CreateMobileServiceParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            this.Validate();
            parameters.Validate();

            if (string.IsNullOrEmpty(parameters.ServiceLocation))
            {
                parameters.ServiceLocation = await this.GetDefaultServiceLocationAsync();
            }

            if (string.IsNullOrEmpty(parameters.SqlServerLocation))
            {
                parameters.SqlServerLocation = parameters.ServiceLocation;
            }

            // create the service specification

            JObject spec = this.CreateMobileServiceSpecification(parameters);
            string applicationSpec = this.CreateMobileApplicationSpecification(spec, parameters);
            
            // create the service with a call to management APIs

            await this.MakeMobileApplicationManagementRequestAsync(HttpMethod.Post, null, applicationSpec, true);

            // get the details of the created service to confirm creation status and extract information about created resources

            XElement createdApplication = await this.MakeMobileApplicationManagementRequestAsync(
                HttpMethod.Get, parameters.ServiceName + "mobileservice", null, false);
            CreateMobileServiceResult result = ProcessMobileApplicationManagementResponse(createdApplication);
            
            return result;
        }

        static CreateMobileServiceResult ProcessMobileApplicationManagementResponse(XElement response)
        {
            if (response == null)
            {
                throw new InvalidOperationException(Resources.NoResponse);
            }

            try
            {
                CreateMobileServiceResult result = new CreateMobileServiceResult();
                result.Details = response;
                result.State = response.Element(XName.Get("State", WindowsAzureNs)).Value;
                ProcessApplicationResources(result, response.Element(XName.Get("InternalResources", WindowsAzureNs))
                    .Elements(XName.Get("InternalResource", WindowsAzureNs)));
                ProcessApplicationResources(result, response.Element(XName.Get("ExternalResources", WindowsAzureNs))
                    .Elements(XName.Get("ExternalResource", WindowsAzureNs))); 
                
                return result;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.UnexpectedResponse,
                    response.ToString()), e);
            }
        }

        static string ElementValueOrNull(XElement element, string name)
        {
            XElement child = element.Element(XName.Get(name, WindowsAzureNs));
            return child == null ? null : child.Value;
        }

        static void ProcessApplicationResources(CreateMobileServiceResult result, IEnumerable<XElement> resources)
        {
            foreach (XElement resource in resources)
            {
                string state = ElementValueOrNull(resource, "State");
                string type = ElementValueOrNull(resource, "Type");
                string name = ElementValueOrNull(resource, "Name");

                if (type == "Microsoft.WindowsAzure.MobileServices.MobileService")
                {
                    result.MobileServiceState = state;
                }
                else if (type == "Microsoft.WindowsAzure.SQLAzure.DataBase")
                {
                    result.SqlDbName = name;
                    result.SqlDbState = state;
                }
                else if (type == "Microsoft.WindowsAzure.SQLAzure.Server")
                {
                    result.SqlServerName = name;
                    result.SqlServerState = state;
                }
            }
        }

        string CreateMobileApplicationSpecification(JObject mobileServiceSpecification, CreateMobileServiceParameters parameters)
        {
            string encodedSpec = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(mobileServiceSpecification)));
            return string.Format(Resources.CreateMobileServiceApplicationTemplate,
                parameters.ServiceName + "mobileservice",
                parameters.ServiceName,
                parameters.ServiceName,
                encodedSpec);
        }

        JObject CreateMobileServiceSpecification(CreateMobileServiceParameters parameters)
        {
            string serverRefName = "ZumoSqlServer_" + Guid.NewGuid().ToString("N");
            JObject serverSpec;
            string unsecuredPassword = CreateMobileServiceParameters.ConvertToUnsecureString(parameters.SqlAdminPassword);

            if (parameters.ExistingSqlServer != null)
            {
                // use existing SQL server

                serverSpec = new JObject(
                    new JProperty("Name", serverRefName),
                    new JProperty("Type", "Microsoft.WindowsAzure.SQLAzure.Server"),
                    new JProperty("URI", this.SqlManagementEndpoint + "/" +
                        this.SubscriptionId + "/services/sqlservers/servers/" + parameters.ExistingSqlServer)
                );
            }
            else
            {
                // create a new SQL server

                serverSpec = new JObject(
                    new JProperty("ProvisioningParameters", new JObject(
                        new JProperty("AdministratorLogin", parameters.SqlAdminUsername),
                        new JProperty("AdministratorLoginPassword", unsecuredPassword),
                        new JProperty("Location", parameters.SqlServerLocation)
                    )),
                    new JProperty("ProvisioningConfigParameters", new JObject(
                        new JProperty("FirewallRules", new JArray(
                            new JObject(
                                new JProperty("Name", "AllowAllWindowsAzureIps"),
                                new JProperty("StartIPAddress", "0.0.0.0"),
                                new JProperty("EndIPAddress", "0.0.0.0")
                            )
                        ))
                    )),
                    new JProperty("Version", "1.0"),
                    new JProperty("Name", serverRefName),
                    new JProperty("Type", "Microsoft.WindowsAzure.SQLAzure.Server")
                );
            }

            string dbRefName = "ZumoSqlDatabase_" + Guid.NewGuid().ToString("N");
            JObject dbSpec;

            if (parameters.ExistingSqlDatabase != null) {
                // use existing SQL database

                dbSpec = new JObject(
                    new JProperty("Name", dbRefName),
                    new JProperty("Type", "Microsoft.WindowsAzure.SQLAzure.DataBase"),
                    new JProperty("URI", this.SqlManagementEndpoint + "/" +
                        this.SubscriptionId + "/services/sqlservers/servers/" + parameters.ExistingSqlServer +
                        "/databases/" + parameters.ExistingSqlDatabase)
                );            
            }
            else {
                // create a new SQL database

                dbSpec = new JObject(
                    new JProperty("ProvisioningParameters", new JObject(
                        new JProperty("Name", parameters.ServiceName + "_db"),
                        new JProperty("Edition", "WEB"),
                        new JProperty("MaxSizeInGB", "1"),
                        new JProperty("DBServer", new JObject(
                            new JProperty("ResourceReference", serverRefName + ".Name")
                        )),
                        new JProperty("CollationName", "SQL_Latin1_General_CP1_CI_AS")
                    )),
                    new JProperty("Version", "1.0"),
                    new JProperty("Name", dbRefName),
                    new JProperty("Type", "Microsoft.WindowsAzure.SQLAzure.DataBase")
                );
            }

            JObject spec = new JObject(
                new JProperty("SchemaVersion", "2012-05.1.0"),
                new JProperty("Location", "West US"),
                new JProperty("ExternalResources", new JObject()),
                new JProperty("InternalResources", new JObject(
                    new JProperty("ZumoMobileService", new JObject(
                        new JProperty("ProvisioningParameters", new JObject(
                            new JProperty("Name", parameters.ServiceName),
                            new JProperty("Location", parameters.ServiceLocation)
                        )),
                        new JProperty("ProvisioningConfigParameters", new JObject(
                            new JProperty("Server", new JObject(
                                new JProperty("StringConcat", new JArray(
                                    new JObject(
                                        new JProperty("ResourceReference", serverRefName + ".Name") 
                                    ),
                                    this.SqlDatabaseHostSuffix
                                ))
                            )),
                            new JProperty("Database", new JObject(
                                new JProperty("ResourceReference", dbRefName + ".Name")
                            )),
                            new JProperty("AdministratorLogin", parameters.SqlAdminUsername),
                            new JProperty("AdministratorLoginPassword", unsecuredPassword)
                        )),
                        new JProperty("Version", "2012-05-21.1.0"),
                        new JProperty("Name", "ZumoMobileService"),
                        new JProperty("Type", "Microsoft.WindowsAzure.MobileServices.MobileService")
                    ))
                ))
            );

            if (parameters.ExistingSqlServer != null)
            {
                ((JObject)spec["ExternalResources"]).Add(serverRefName, serverSpec);
            }
            else
            {
                ((JObject)spec["InternalResources"]).Add(serverRefName, serverSpec);
            }

            if (parameters.ExistingSqlDatabase != null)
            {
                ((JObject)spec["ExternalResources"]).Add(dbRefName, dbSpec);
            }
            else
            {
                ((JObject)spec["InternalResources"]).Add(dbRefName, dbSpec);
            }

            return spec;
        }

        async Task<string> GetDefaultServiceLocationAsync()
        {
            JArray response = await this.MakeMobileServiceManagementRequestAsync<JArray>(HttpMethod.Get, "regions", null);
            if (response.Count == 0)
            {
                throw new InvalidOperationException(Resources.ErrorGettingDefaultServiceLocation);
            }

            return (string)response[0]["region"];
        }

        HttpRequestMessage CreateMobileServiceManagementRequest(HttpMethod method, string path, object request)
        {
            HttpRequestMessage result = new HttpRequestMessage();
            result.Method = method;
            result.RequestUri = new Uri(
                this.MobileServiceManagementEndpoint +
                "/" + this.SubscriptionId +
                "/services/mobileservices/" +
                path);
            result.Headers.Add("x-ms-version", "2012-03-01");
            result.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (request != null)
            {
                result.Content = new StringContent(JsonConvert.SerializeObject(request));
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            return result;
        }

        HttpRequestMessage CreateMobileApplicationManagementRequest(HttpMethod method, string path, string request)
        {
            HttpRequestMessage result = new HttpRequestMessage();
            result.Method = method;
            result.RequestUri = new Uri(
                this.MobileServiceManagementEndpoint +
                "/" + this.SubscriptionId +
                "/applications" +
                (path != null ? ("/" + path) : ""));
            result.Headers.Add("x-ms-version", "2012-03-01");
            result.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            if (request != null)
            {
                result.Content = new StringContent(request);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
            }

            return result;
        }

        async Task<TResult> ProcessJsonResponseAsync<TResult>(HttpResponseMessage response)
            where TResult: JToken
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.ManagementServiceReturnedHttpError,
                    response.StatusCode));
            }

            if (response.Content == null)
            {
                return null;
            }

            string responseText = await response.Content.ReadAsStringAsync();
            JToken result = JToken.Parse(responseText);
            if (typeof(TResult) == typeof(JObject) && result.Type == JTokenType.Object
                || typeof(TResult) == typeof(JArray) && result.Type == JTokenType.Array)
            {
                return (TResult)result;
            }
            else 
            {
                Type actualType = result.GetType();
                throw new InvalidOperationException(string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.UnexpectedTypeOfResponse,
                    typeof(TResult),
                    actualType));
            }
        }

        async Task<XElement> ProcessXmlResponseAsync(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.ManagementServiceReturnedHttpError,
                    response.StatusCode));
            }

            if (response.Content == null)
            {
                return null;
            }

            string responseText = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(responseText))
            {
                return null;
            }
            else
            {
                try
                {
                    return XElement.Parse(responseText);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.UnableToProcessResponse,
                            responseText),
                        e);
                }
            }
        }

        async Task<TResult> MakeMobileServiceManagementRequestAsync<TResult>(HttpMethod method, string path, object request)
            where TResult: JToken
        {
            HttpRequestMessage httpRequest = this.CreateMobileServiceManagementRequest(method, path, request);
            HttpResponseMessage httpResponse = await this.MakeManagementRequestAsync(httpRequest);
            return await this.ProcessJsonResponseAsync<TResult>(httpResponse);
        }

        async Task<XElement> MakeMobileApplicationManagementRequestAsync(HttpMethod method, string path, string request, bool async)
        {
            HttpRequestMessage httpRequest = this.CreateMobileApplicationManagementRequest(method, path, request);
            HttpResponseMessage httpResponse = await this.MakeManagementRequestAsync(httpRequest);
            XElement result = await this.ProcessXmlResponseAsync(httpResponse);
            if (!async)
            {
                return result;
            }
            else
            {
                IEnumerable<string> ids;
                if (httpResponse.Headers.TryGetValues("x-ms-request-id", out ids))
                {
                    await this.TrackAsyncOperationAsync(ids.FirstOrDefault());
                    return result;
                }
                else
                {
                    throw new InvalidOperationException(Resources.NoOperationIdInResponse);
                }   
            }
        }

        async Task<HttpResponseMessage> TrackAsyncOperationAsync(string operationId)
        {
            while (true)
            {
                HttpRequestMessage request = new HttpRequestMessage();
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(
                    this.MobileServiceManagementEndpoint +
                    "/" + this.SubscriptionId +
                    "/operations/" +
                    operationId);
                request.Headers.Add("x-ms-version", "2012-03-01");
                HttpResponseMessage response = await this.MakeManagementRequestAsync(request);
                XElement body = await this.ProcessXmlResponseAsync(response);
                string status;
                try
                {
                    status = body.Element(XName.Get("Status", WindowsAzureNs)).Value;
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.UnexpectedAsyncOperationStatus,
                        (body == null ? "<empty>" : body.ToString())), e);
                }

                if (status == "Succeeded")
                {
                    return response;
                }
                else if (status == "Failed")
                {
                    throw new InvalidOperationException(string.Format(
                        CultureInfo.CurrentCulture, 
                        Resources.AsyncOperationFailed, 
                        operationId));
                }
                else if (status != "InProgress")
                {
                    throw new InvalidOperationException(string.Format(
                        CultureInfo.CurrentCulture, 
                        Resources.UnexpectedAsyncOperationStatus, 
                        status));
                }
                
                await Task.Delay(5000);
            }
        }

        async Task<HttpResponseMessage> MakeManagementRequestAsync(HttpRequestMessage request)
        {
            WebRequestHandler handler = new WebRequestHandler();
            handler.UseDefaultCredentials = false;
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ClientCertificates.Add(this.ManagementCertificate);
            HttpClient client = new HttpClient(handler);
            return await client.SendAsync(request);
        }

        void Validate()
        {
            if (string.IsNullOrEmpty(this.SubscriptionId))
            {
                throw new InvalidOperationException(Resources.MissingSubscriptionId);
            }

            if (this.ManagementCertificate == null)
            {
                throw new InvalidOperationException(Resources.MissingManagementCertificate);
            }

            if (!this.ManagementCertificate.HasPrivateKey)
            {
                throw new InvalidOperationException(Resources.ManagementCertificateWithoutPrivateKey);
            }

            if (string.IsNullOrEmpty(this.MobileServiceManagementEndpoint))
            {
                throw new InvalidOperationException(Resources.MissingMobileServiceManagementEndpoint);
            }
        }
    }
}
