// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    public class MobileServiceTableQueryMock<T> : IMobileServiceTableQuery<T>
    {
        public bool EnumerableAsyncThrowsException { get; set; }
        
        public IMobileServiceTableQuery<T> IncludeTotalCount()
        {
            throw new NotImplementedException();
        }

        public IMobileServiceTableQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            throw new NotImplementedException();
        }

        public IMobileServiceTableQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            throw new NotImplementedException();
        }

        public IMobileServiceTableQuery<U> Select<U>(Expression<Func<T, U>> selector)
        {
            throw new NotImplementedException();
        }

        private int skip;

        public IMobileServiceTableQuery<T> Skip(int count)
        {
            skip += count;
            return this;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public IMobileServiceTable<T> Table
        {
            get { throw new NotImplementedException(); }
        }

        private int take;

        public IMobileServiceTableQuery<T> Take(int count)
        {
            take = count;
            return this;
        }

        public IMobileServiceTableQuery<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            throw new NotImplementedException();
        }

        public IMobileServiceTableQuery<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> ToEnumerableAsync()
        {
            if (EnumerableAsyncThrowsException)
            {
                throw new Exception();
            }
            return TaskEx.Run(() => TaskEx.Delay(500).ContinueWith(t => (IEnumerable<T>)new TotalCountEnumerable<T>(2, new T[] { Activator.CreateInstance<T>(), Activator.CreateInstance<T>() })));
        }

        public Task<List<T>> ToListAsync()
        {
            throw new NotImplementedException();
        }

        public IMobileServiceTableQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public IMobileServiceTableQuery<T> WithParameters(IDictionary<string, string> parameters)
        {
            throw new NotImplementedException();
        }

        public bool RequestTotalCount
        {
            get { throw new NotImplementedException(); }
        }

        public IDictionary<string, string> Parameters
        {
            get { throw new NotImplementedException(); }
        }
    }
}
