// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using ZumoE2EServerApp.DataObjects;
using ZumoE2EServerApp.Models;

namespace ZumoE2EServerApp.Controllers
{
    public class StringIdMoviesController : TableController<StringIdMovie>
    {
        private SDKClientTestContext context;

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            context = new SDKClientTestContext(Services.Settings.Name);
            this.DomainManager = new EntityDomainManager<StringIdMovie>(context, Request, Services);
        }

        [Queryable(MaxTop = 1000)]
        public IQueryable<StringIdMovie> GetAllMovies()
        {
            return Query();
        }

        public SingleResult<StringIdMovie> GetMovie(string id)
        {
            return Lookup(id);
        }

        public async Task<AllStringIdMovies> PostMovie(AllStringIdMovies item)
        {
            var table = this.context.Movies;
            if (table.Count() == 0)
            {
                table.AddRange(item.Movies);
                await this.context.SaveChangesAsync();
            }
            return item;
        }
    }
}
