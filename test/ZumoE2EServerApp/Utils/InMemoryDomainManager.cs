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
    // Note: This is not thread safe, but should be good enough for running simple tests.
    public class InMemoryDomainManager<TData> : IDomainManager<TData> where TData : class, ITableData
    {
        private static List<TData> store;
        private static int counter = 1;
        private bool isSingleThreaded;

        public InMemoryDomainManager(bool isSingleThreaded, List<TData> initial = null)
        {
            this.isSingleThreaded = isSingleThreaded;
            if (store == null)
            {
                store = (initial != null ? initial : new List<TData>());
            }
        }

        public IQueryable<TData> Query()
        {
            return store.AsQueryable();
        }

        public SingleResult<TData> Lookup(string id)
        {
            return SingleResult.Create(store.Where(p => p.Id == id).AsQueryable());
        }

        public Task<IEnumerable<TData>> QueryAsync(ODataQueryOptions query)
        {
            throw new NotImplementedException();
        }

        public Task<SingleResult<TData>> LookupAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<TData> InsertAsync(TData data)
        {
            data.Id = counter.ToString();
            counter++;
            AddWorker(data);
            await Task.Delay(0);
            return data;
        }

        public async Task<TData> UpdateAsync(string id, Delta<TData> patch)
        {
            TData item = GetItem(id);
            patch.Patch(item);
            await Task.Delay(0);
            return item;
        }

        private static TData GetItem(string id)
        {
            TData item;
            item = store.FirstOrDefault(p => p.Id == id);
            return item;
        }

        public async Task<TData> ReplaceAsync(string id, TData data)
        {
            await DeleteAsync(id);
            data.Id = id;
            AddWorker(data);
            await Task.Delay(0);
            return data;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            bool ret = false;
            var item = GetItem(id);
            if (item != null)
            {
                RemoveWorker(item);
                ret = true;
            }
            await Task.Delay(0);
            return ret;
        }

        private void AddWorker(TData data)
        {
            if (isSingleThreaded)
            {
                // Not threadsafe
                store.Add(data);
            }
            else
            {
                // Don't actually add.
            }
        }

        private void RemoveWorker(TData data)
        {
            if (isSingleThreaded)
            {
                // Not threadsafe
                store.Remove(data);
            }
            else
            {
                // Don't actually add.
            }
        }
    }
}