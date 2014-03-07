// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ZumoE2EServerApp.Utils
{
    public static class CustomSharedApi
    {
        internal static async Task<HttpResponseMessage> handleRequest(
            HttpRequestMessage request,
            ServiceUser user)
        {
            var query = request.GetQueryNameValuePairs();

            var format = GetQueryParamOrDefault(query, "format", "json");
            var status = int.Parse(GetQueryParamOrDefault(query, "status", "200"));
            var output = new Output() { Method = request.Method.Method };
            foreach (var q in query.Where(q => q.Key != "format" && q.Key != "status"))
            {
                output.Query[q.Key] = q.Value;
            }

            if (output.Query.Keys.Count == 0)
            {
                output.Query = null;
            }

            var reqHeaders = request.Headers;
            var outputHeaders = new Dictionary<string, IEnumerable<string>>();
            foreach (var reqHeader in reqHeaders.Where(p => p.Key.Contains("x-test-zumo-")))
            {
                outputHeaders.Add(reqHeader.Key, reqHeader.Value);
            }

            if (request.Content != null)
            {
                var requestBody = await request.Content.ReadAsStringAsync();
                if (request.Content.Headers.ContentType != null && !string.IsNullOrEmpty(requestBody))
                {
                    if (request.Content.Headers.ContentType.MediaType.Contains("/json"))
                    {
                        output.Body = JToken.Parse(requestBody);
                    }
                    else
                    {
                        output.Body = requestBody;
                    }
                }
            }

            //output.Body = body;
            output.User = new NodeUser(user);

            string responseMediaType;
            string responseBodyString = "";
            switch (format)
            {
                case "json":
                    // Convert to JSON.
                    responseBodyString = JsonConvert.SerializeObject(output);
                    responseMediaType = "application/json";
                    break; // nothing to do
                case "xml":
                    responseMediaType = "text/xml";
                    responseBodyString = output.ToXml();
                    break;
                default:
                    responseMediaType = "text/plain";
                    responseBodyString = JsonConvert.SerializeObject(output)
                        .Replace("{", "__{__")
                        .Replace("}", "__}__")
                        .Replace("[", "__[__")
                        .Replace("]", "__]__");
                    break;
            }

            var responseContent = new StringContent(responseBodyString, Encoding.UTF8, responseMediaType);
            var resp = new HttpResponseMessage((HttpStatusCode)status) { Content = responseContent };
            foreach (var h in outputHeaders)
            {
                if (!resp.Headers.Contains(h.Key))
                {
                    resp.Headers.Add(h.Key, h.Value);
                }
            }

            return resp;
        }

        private static string GetQueryParamOrDefault(IEnumerable<KeyValuePair<string, string>> query, string key, string def)
        {
            var val = query.FirstOrDefault(p => p.Key == key);
            return (val.Key == key) ? val.Value : def;
        }
    }

    class NodeUser
    {
        public NodeUser(ServiceUser user)
        {
            this.Id = user.Id;
            if (user.Level == AuthorizationLevel.User)
            {
                this.Level = "authenticated";
                this.Id = user.Id;
            }
            else
            {
                this.Level = "anonymous";
            }
        }

        [JsonProperty(PropertyName = "userId")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "level")]
        public string Level { get; set; }

    }
    class Output
    {
        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "query")]
        public Dictionary<string, string> Query = new Dictionary<string, string>();

        [JsonProperty(PropertyName = "user")]
        public NodeUser User { get; set; }

        [JsonProperty(PropertyName = "body")]
        public object Body { get; set; }

        public string ToXml()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<root>");
            this.ConvertBodyToXml(sb);
            sb.AppendFormat("<method>{0}</method>", this.Method);
            this.ConvertQueryToXml(sb);
            this.ConvertUserToXml(sb);
            sb.Append("</root>");
            return sb.ToString();
        }

        private void ConvertBodyToXml(StringBuilder sb)
        {
            if (this.Body != null)
            {
                sb.Append("<body>");
                var strBody = this.Body as string;
                if (strBody != null)
                {
                    sb.Append(strBody);
                }
                else
                {
                    var jtokenBody = this.Body as JToken;
                    if (jtokenBody != null)
                    {
                        JsonToXml(jtokenBody, sb);
                    }
                    else
                    {
                        sb.AppendFormat("ERROR! Invalid body type: {0} - {1}", this.Body.GetType().FullName, this.Body);
                    }
                }

                sb.Append("</body>");
            }
        }

        private static void JsonToXml(JToken json, StringBuilder sb)
        {
            if (json == null)
            {
                json = "";
            }

            switch (json.Type)
            {
                case JTokenType.Null:
                    sb.Append("null");
                    break;
                case JTokenType.Boolean:
                    sb.Append(json.ToString().ToLowerInvariant());
                    break;
                case JTokenType.Float:
                case JTokenType.Integer:
                    sb.Append(json.ToString());
                    break;
                case JTokenType.String:
                    sb.Append(json.ToObject<string>());
                    break;
                case JTokenType.Array:
                    sb.Append("<array>");
                    JArray array = (JArray)json;
                    for (int i = 0; i < array.Count; i++)
                    {
                        sb.Append("<item>");
                        JsonToXml(array[i], sb);
                        sb.Append("</item>");
                    }

                    sb.Append("</array>");
                    break;
                case JTokenType.Object:
                    JObject obj = (JObject)json;
                    var keys = obj.Properties().Select(p => p.Name).ToArray();
                    Array.Sort(keys);
                    foreach (var key in keys)
                    {
                        sb.Append("<" + key + ">");
                        JsonToXml(obj[key], sb);
                        sb.Append("</" + key + ">");
                    }

                    break;
                default:
                    throw new ArgumentException("Type " + json.Type + " is not supported");
            }
        }

        private void ConvertUserToXml(StringBuilder sb)
        {
            sb.Append("<user>");
            sb.AppendFormat("<level>{0}</level>", this.User.Level.ToLowerInvariant());
            if (this.User.Id != null)
            {
                sb.AppendFormat("<userId>{0}</userId>", this.User.Id);
            }

            sb.Append("</user>");
        }

        private void ConvertQueryToXml(StringBuilder sb)
        {
            if (this.Query != null && this.Query.Count > 0)
            {
                sb.Append("<query>");
                foreach (var key in this.Query.Keys.OrderBy(k => k))
                {
                    sb.AppendFormat("<{0}>", key);
                    sb.Append(this.Query[key]);
                    sb.AppendFormat("</{0}>", key);
                }

                sb.Append("</query>");
            }
        }
    }
}