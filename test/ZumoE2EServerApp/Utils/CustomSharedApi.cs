// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ZumoE2EServerApp.Utils
{
    public static class CustomSharedApi
    {
        internal static Task<HttpResponseMessage> handleRequest(
            HttpRequestMessage req,
            ServiceUser user,
            JToken body = null)
        {
            var query = req.GetQueryNameValuePairs();

            var format = GetQueryParamOrDefault(query, "format", "json");
            var status = int.Parse(GetQueryParamOrDefault(query, "status", "200"));
            var output = new Output() { Method = req.Method.Method };
            foreach (var q in query.Where(q => q.Key != "format" && q.Key != "status"))
            {
                output.Query[q.Key] = q.Value;
            }

            if (output.Query.Keys.Count == 0)
            {
                output.Query = null;
            }

            var reqHeaders = req.Headers;
            var outputHeaders = new Dictionary<string, IEnumerable<string>>();
            foreach (var reqHeader in reqHeaders.Where(p => !p.Key.Contains("x-test-zumo-")))
            {
                outputHeaders.Add(reqHeader.Key, reqHeader.Value);
            }

            output.Body = body;
            output.User = new NodeUser(user);

            string outputS = "";
            switch (format)
            {
                case "json":
                    // Convert to JSON.
                    outputS = JsonConvert.SerializeObject(output);
                    break; // nothing to do
                case "xml":
                    outputHeaders.Add("Content-Type", new string[] { "text/xml" });
                    outputS = objToXml(output);
                    break;
                default:
                    outputHeaders.Add("Content-Type", new string[] { "text/plain" });
                    outputS = JsonConvert.SerializeObject(output)
                        .Replace("{", "__{__")
                        .Replace("}", "__}__")
                        .Replace("[", "__[__")
                        .Replace("]", "__]__");
                    break;
            }

            var resp = req.CreateResponse((HttpStatusCode)status, output);
            foreach (var h in outputHeaders)
            {
                if (!resp.Headers.Contains(h.Key))
                {
                    resp.Headers.Add(h.Key, h.Value);
                }
            }

            return Task.FromResult(resp);
        }

        private static string GetQueryParamOrDefault(IEnumerable<KeyValuePair<string, string>> query, string key, string def)
        {
            var val = query.FirstOrDefault(p => p.Key == key);
            return (val.Key == key) ? val.Value : def;
        }

        private static string objToXml(object obj)
        {
            return "<root>" + jsToXml(obj) + "</root>";
        }

        private static string jsToXml(object value)
        {
            if (value == null) return "null";
            var type = value.GetType();
            var result = "";
            return result;
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
    }
}