// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;

namespace ZumoE2EServerApp.Utils
{
    internal class InMemoryDomainManager<TData> : IDomainManager<TData> where TData : class, ITableData
    {
        private static List<TData> store;
        private static object lockObj = new object();
        private static int counter = 1;

        public InMemoryDomainManager(List<TData> initial = null)
        {
            if (store == null)
            {
                lock (lockObj)
                {
                    if (store == null)
                    {
                        store = (initial != null ? initial : new List<TData>());
                    }
                }
            }
        }

        public IQueryable<TData> Query()
        {
            lock (lockObj)
            {
                return store.AsQueryable();
            }
        }

        public SingleResult<TData> Lookup(string id)
        {
            lock (lockObj)
            {
                return SingleResult.Create(store.Where(p => p.Id == id).AsQueryable());
            }
        }

        public Task<IEnumerable<TData>> QueryAsync(ODataQueryOptions query)
        {
            throw new NotImplementedException();
        }

        public Task<SingleResult<TData>> LookupAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<TData> InsertAsync(TData data)
        {
            lock (lockObj)
            {
                data.Id = (counter++).ToString();
                store.Add(data);
                return Task.FromResult(data);
            }
        }

        public Task<TData> UpdateAsync(string id, Delta<TData> patch)
        {
            lock (lockObj)
            {
                TData data = store.First(p => p.Id == id);
                patch.Patch(data);
                return Task.FromResult(data);
            }
        }

        public Task<TData> ReplaceAsync(string id, TData data)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(string id)
        {
            lock (lockObj)
            {
                bool ret = false;
                var data = store.FirstOrDefault(p => p.Id == id);
                if (data != null)
                {
                    store.Remove(data);
                    ret = true;
                }
                return Task.FromResult(ret);
            }
        }
    }
}