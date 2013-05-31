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
using System.Web;
using System.Xml.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Management
{
    public class MobileServicesManagementClient
    {
        static readonly string WamsConfigurationEndpoint = "http://go.microsoft.com/fwlink/?LinkId=290890&clcid=0x409";
        static readonly string DefaultMobileServiceManagementEndpoint = "https://management.core.windows.net";
        static readonly string DefaultSqlManagementEndpoint = "https://management.core.windows.net:8443";
        static readonly string WindowsAzureNs = "http://schemas.microsoft.com/windowsazure";
        static readonly string SqlAzureNs = "http://schemas.microsoft.com/sqlazure/2010/12/";
        static readonly string DefaultSqlDatabaseHostSuffix = ".database.windows.net";
        static readonly string DefaultMobileServiceHostSuffix = ".azure-mobile.net";

        static readonly string FreeDBSizeInBytes = "20971520";

        public string SubscriptionId { get; set; }
        public X509Certificate2 ManagementCertificate { get; set; }
        public string MobileServiceManagementEndpoint { get; set; }
        public string SqlManagementEndpoint { get; set; }
        public string SqlDatabaseHostSuffix { get; set; }
        public string MobileServiceHostSuffix { get; set; }
        public string UserAgent { get; set; }

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
            this.SqlDatabaseHostSuffix = DefaultSqlDatabaseHostSuffix;
            this.MobileServiceHostSuffix = DefaultMobileServiceHostSuffix;
        }

        public async Task SetMobileServiceManagementEndpointAsync(string endpoint)
        {
            Uri uri = new Uri(endpoint, UriKind.Absolute);
            string lookupKey = uri.AbsoluteUri.ToLowerInvariant();
            JObject configuration;

            try
            {
                HttpClient client = new HttpClient();
                if (!string.IsNullOrEmpty(this.UserAgent))
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(this.UserAgent);
                }

                string response = await client.GetStringAsync(WamsConfigurationEndpoint);
                configuration = JObject.Parse(response);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, 
                    Resources.UnableToAccessConfiguration,
                    WamsConfigurationEndpoint),
                    e);
            }

            JObject endpoints = configuration[lookupKey] as JObject;

            if (endpoints == null)
            {
                throw new InvalidOperationException(string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.InvalidManagementEndpoint,
                    endpoint));
            }

            this.MobileServiceManagementEndpoint = (string)endpoints["mobileServiceManagementEndpoint"];
            this.SqlManagementEndpoint = (string)endpoints["sqlManagementEndpoint"];
            this.SqlDatabaseHostSuffix = (string)endpoints["sqlDatabaseHostSuffix"];
            this.MobileServiceHostSuffix = (string)endpoints["mobileServiceHostSuffix"];

            return;
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
            if (!string.IsNullOrEmpty(this.UserAgent))
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd(this.UserAgent);
            }

            HttpRequestMessage request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri(
                "https://" + sqlServerName + this.SqlDatabaseHostSuffix +
                "/v1/ManagementService.svc/GetAccessToken");
            request.Headers.Add("sqlauthorization", "Basic " +
                Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + CreateMobileServiceParameters.ConvertToUnsecureString(password))));
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.Gone)
            {
                throw new ManagementEndpointDeprecatedException(request.RequestUri.ToString());
            }

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
            bool hasFreeDB = false;

            HttpRequestMessage request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri(
                this.SqlManagementEndpoint +
                "/" + this.SubscriptionId +
                "/services/sqlservers/servers");
            request.Headers.Add("x-ms-version", "2012-03-01");
            HttpResponseMessage response = await this.MakeManagementRequestAsync(request);
            XElement body = await this.ProcessXmlResponseAsync(response);

            List<SqlDatabaseParameters> result = new List<SqlDatabaseParameters>();
            foreach (XElement server in body.Elements())
            {
                string name = server.Element(XName.Get("Name", SqlAzureNs)).Value;
                string login = server.Element(XName.Get("AdministratorLogin", SqlAzureNs)).Value;
                string location = server.Element(XName.Get("Location", SqlAzureNs)).Value;

                XElement dbs = await this.GetRawSqlDatabasesAsync(name);
                foreach (XElement db in dbs.Elements())
                {
                    string dbName = db.Element(XName.Get("Name", WindowsAzureNs)).Value;
                    if (!string.Equals(dbName, "master", StringComparison.InvariantCultureIgnoreCase))
                    {
                        SqlDatabaseParameters parameters = new SqlDatabaseParameters();
                        parameters.Location = location;
                        parameters.ServerName = name;
                        parameters.DatabaseName = dbName;
                        parameters.AdministratorLogin = login;

                        string dbSize = db.Element(XName.Get("MaxSizeBytes", WindowsAzureNs)).Value;
                        parameters.IsExistingDatabase = true;
                        if (string.Equals(dbSize, FreeDBSizeInBytes, StringComparison.InvariantCultureIgnoreCase))
                        {
                            hasFreeDB = true;
                            parameters.DatabaseType = DatabaseTypes.FreeDB;
                        }
                        else 
                        {
                            parameters.DatabaseType = DatabaseTypes.Standard;
                        }

                        result.Add(parameters);
                    }
                }
            }

            //append on the "creation" options
            SqlDatabaseParameters newDatabaseParameters = new SqlDatabaseParameters();
            newDatabaseParameters.DatabaseName = "Create New";
            newDatabaseParameters.IsExistingDatabase = false;
            newDatabaseParameters.DatabaseType = DatabaseTypes.Standard;
            result.Add(newDatabaseParameters);

            if (!hasFreeDB)
            {
                newDatabaseParameters = new SqlDatabaseParameters();
                newDatabaseParameters.DatabaseName = "Create New Free Database";
                newDatabaseParameters.IsExistingDatabase = false;
                newDatabaseParameters.DatabaseType = DatabaseTypes.FreeDB;
                result.Add(newDatabaseParameters);
            }

            return result;
        }

        async Task<XElement> GetRawSqlDatabasesAsync(string sqlServerName)
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

            return body;
        }

        public async Task<List<string>> GetSqlDatabasesAsync(string sqlServerName)
        {
            if (string.IsNullOrEmpty(sqlServerName))
            {
                throw new ArgumentNullException("sqlServerName");
            }

            XElement body = await this.GetRawSqlDatabasesAsync(sqlServerName);
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

            // attempt to register MobileService with RDFE
            await this.MakeMobileServiceRegistrationRequestAsync();

            // create the service specification
            JObject spec = this.CreateMobileServiceSpecification(parameters);
            string applicationSpec = this.CreateMobileApplicationSpecification(spec, parameters);
            
            // create the service with a call to management APIs
            await this.MakeMobileApplicationManagementRequestAsync(HttpMethod.Post, null, applicationSpec, true);

            // get the details of the created service to confirm creation status and extract information about created resources

            XElement createdApplication = await this.MakeMobileApplicationManagementRequestAsync(
                HttpMethod.Get, parameters.ServiceName + "mobileservice", null, false);
            CreateMobileServiceResult result = ProcessMobileApplicationManagementResponse(createdApplication);
            
            //Delete the service if it comes back as unhealthy
            //We will not reach this point if the service already existed (a conflict http response code would have come back
            //and been thrown as an error.)
            if (!result.IsSuccess()) 
            {
                try
                {
                    await this.DeleteMobileServiceAsync(parameters.ServiceName, true, true);
                }
                catch(Exception e)
                {
                    //Do nothing, we are just trying to clean up an error state.                    
                }
            }

            return result;
        }

        public async Task<bool> DeleteMobileServiceAsync(string serviceName, bool deleteData = false)
        {
            return await DeleteMobileServiceAsync(serviceName, deleteData, false);
        }

        async Task<bool> DeleteMobileServiceAsync(string serviceName, bool deleteData = false, bool forceDelete = false)
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                throw new ArgumentNullException("serviceName");
            }

            string query = "";
            if (deleteData) 
            {
                query="?deletedata=true";
            }

            //delete the mobile service, success or not found are valid answers
            HttpRequestMessage httpRequest = this.CreateMobileServiceManagementRequest(HttpMethod.Delete, "mobileservices/" + serviceName + query, null);
            HttpResponseMessage httpResponse = await this.MakeManagementRequestAsync(httpRequest);
            
            if (!httpResponse.IsSuccessStatusCode && httpResponse.StatusCode != HttpStatusCode.NotFound && !forceDelete)
            {
                throw new InvalidOperationException(string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.ManagementServiceReturnedHttpError,
                    httpResponse.StatusCode));
            }

            await this.MakeMobileApplicationManagementRequestAsync(HttpMethod.Delete, serviceName + "mobileservice", null, true);

            return true;
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

                if (string.Equals("unhealthy", result.State, StringComparison.InvariantCultureIgnoreCase))
                {
                    string mobileServiceState = result.MobileServiceState;
                    result.FaultMessages.Add(string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.ServiceUnhealthy,
                        result.MobileServiceState));
                }

                foreach (XElement faultCode in response.Descendants(XName.Get("FailureCode", WindowsAzureNs)))
                {
                    if (string.IsNullOrEmpty(faultCode.Value))
                    {
                        continue;
                    }

                    try
                    {
                        string faultXml = HttpUtility.HtmlDecode(faultCode.Value);
                        XElement fault = XElement.Parse(faultXml);
                        bool foundMessage = false;
                        foreach (XElement descendent in fault.Descendants())
                        {
                            if (descendent.Name.LocalName == "Message")
                            {
                                foundMessage = true;
                                result.FaultMessages.Add(descendent.Value);
                                break;
                            }
                        }

                        if (!foundMessage)
                        {
                            result.FaultMessages.Add(faultCode.Value);
                        }
                    }
                    catch
                    {
                        result.FaultMessages.Add(faultCode.Value);
                    }
                }
                
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
                string sizeKey;
                string sizeValue;
                if (parameters.SQLDatabaseType == DatabaseTypes.FreeDB)
                {
                    sizeKey = "MaxSizeInBytes";
                    sizeValue = FreeDBSizeInBytes;
                }
                else
                {
                    sizeKey = "MaxSizeInGB";
                    sizeValue = "1";
                }

                // create a new SQL database
                dbSpec = new JObject(
                    new JProperty("ProvisioningParameters", new JObject(
                        new JProperty("Name", parameters.ServiceName + "_db"),
                        new JProperty("Edition", "WEB"),
                        new JProperty(sizeKey, sizeValue),
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
            if (!string.IsNullOrEmpty(this.UserAgent))
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd(this.UserAgent);
            }

            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.Gone)
            {
                throw new ManagementEndpointDeprecatedException(request.RequestUri.ToString());
            }

            return response;
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
        
        /// <summary>
        /// This function creates a request pointing towards the service registration endpoint
        /// </summary>
        /// <param name="serviceName">The service name to register, MobileService for us, newer
        /// providers may use the format ProviderName.ResourceType
        /// </param>
        /// <param name="action">Either register or unregister</param>
        /// <returns>A configured HTTPRequest</returns>
        HttpRequestMessage CreateServiceRegistrationRequest(string serviceName, string action)
        {
            HttpRequestMessage result = new HttpRequestMessage();
            result.Method = HttpMethod.Put;
            result.RequestUri = new Uri(
                this.MobileServiceManagementEndpoint +
                "/" + this.SubscriptionId +
                "/services?service=" + serviceName + "&action=" + action);

            result.Headers.Add("x-ms-version", "2012-03-01");
            result.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

            return result;
        }

        async Task<bool> MakeMobileServiceRegistrationRequestAsync()
        {
            HttpRequestMessage httpRequest = this.CreateServiceRegistrationRequest("MobileService", "register");
            HttpResponseMessage httpResponse = await this.MakeManagementRequestAsync(httpRequest);

            // The API will return 409 if the subscription is already registered
            // and a success code if all’s well and it was registered
            if (httpResponse.StatusCode == HttpStatusCode.Conflict || httpResponse.IsSuccessStatusCode)
            {
                return true;
            } 
            else 
            {
                //just return the response error
                throw new InvalidOperationException(string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.ManagementServiceReturnedHttpError,
                    httpResponse.StatusCode));
            }
        }
    }
}
