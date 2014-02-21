// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.OData;
using ZumoE2EServerApp.DataObjects;

namespace ZumoE2EServerApp.Controllers
{
    public class ParamsTestTableController : TableController<ParamsTestTableEntity>
    {
        public IQueryable<ParamsTestTableEntity> GetAllNoDB()
        {
            var parameters = this.Request.GetQueryNameValuePairs();
            var id = GetQueryParamOrDefault(parameters, "id", "1");
            var ps = NewMethod(parameters);
            var retEnt = new ParamsTestTableEntity() { Id = int.Parse(id), Parameters = ps };
            return new ParamsTestTableEntity[] { retEnt }.AsQueryable();
        }

        public Task<ParamsTestTableEntity> GetNoDB(string id)
        {
            return MainProcessor(id);
        }

        public Task<ParamsTestTableEntity> PatchNoDB(string id, Delta<ParamsTestTableEntity> patch)
        {
            return MainProcessor(id);
        }

        public Task<ParamsTestTableEntity> PostNoDB(ParamsTestTableEntity item)
        {
            return MainProcessor("1");
        }

        public Task<ParamsTestTableEntity> DeleteNoDB(string id)
        {
            return MainProcessor(id);
        }

        private Task<ParamsTestTableEntity> MainProcessor(string id)
        {
            var parameters = this.Request.GetQueryNameValuePairs();
            var ps = NewMethod(parameters);
            var retEnt = new ParamsTestTableEntity() { Id = int.Parse(id), Parameters = ps };
            return Task.FromResult(retEnt);
        }

        private static string NewMethod(IEnumerable<KeyValuePair<string, string>> parameters)
        {
            JObject ps = new JObject();
            foreach (var kvp in parameters)
            {
                ps[kvp.Key] = kvp.Value;
            }

            var ps2 = JsonConvert.SerializeObject(ps);
            return ps2;
        }

        private static string GetQueryParamOrDefault(IEnumerable<KeyValuePair<string, string>> query, string key, string def)
        {
            var val = query.FirstOrDefault(p => p.Key == key);
            return (val.Key == key) ? val.Value : def;
        }
    }
}