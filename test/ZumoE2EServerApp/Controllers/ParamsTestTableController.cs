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
        public Task<IQueryable<ParamsTestTableEntity>> GetAllNoDB()
        {
            var retEnt = MainProcessor();
            return Task.FromResult(new ParamsTestTableEntity[] { retEnt }.AsQueryable());
        }

        public Task<ParamsTestTableEntity> GetNoDB(string id)
        {
            return Task.FromResult(MainProcessor(id));
        }

        public Task<ParamsTestTableEntity> PatchNoDB(string id, Delta<ParamsTestTableEntity> patch)
        {
            return Task.FromResult(MainProcessor(id));
        }

        public Task<ParamsTestTableEntity> PostNoDB(ParamsTestTableEntity item)
        {
            return Task.FromResult(MainProcessor());
        }

        public Task<ParamsTestTableEntity> DeleteNoDB(string id)
        {
            return Task.FromResult(MainProcessor(id));
        }

        private ParamsTestTableEntity MainProcessor(string id = null)
        {
            var parameters = this.Request.GetQueryNameValuePairs();
            if (id == null)
            {
                var val = parameters.FirstOrDefault(p => p.Key == "id");
                id = (val.Key == "id") ? val.Value : "1";
            }

            JObject parametersJObj = new JObject();
            foreach (var kvp in parameters)
            {
                parametersJObj[kvp.Key] = kvp.Value;
            }

            var retEnt = new ParamsTestTableEntity()
            {
                Id = int.Parse(id),
                Parameters = JsonConvert.SerializeObject(parametersJObj)
            };
            return retEnt;
        }
    }
}