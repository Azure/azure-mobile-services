//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Simple concurrentDictionary
    /// </summary>
    internal class ConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDisposable
    {
        readonly Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

        ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            return this.WriteOperation<TValue>(() =>
            {
                if (this.dictionary.ContainsKey(key))
                {
                    this.dictionary[key] = updateValueFactory(key, this.dictionary[key]);
                }
                else
                {
                    this.dictionary.Add(key, addValue);
                }

                return this.dictionary[key];
            }
                    );
        }

        public TValue GetOrAdd(TKey key, TValue value)
        {
            return this.WriteOperation<TValue>(() =>
            {
                if (!this.dictionary.ContainsKey(key))
                {
                    this.dictionary.Add(key, value);
                }

                return this.dictionary[key];
            }
                );
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            TValue existValue = default(TValue);
            bool result = this.ReadOperation<bool>(() => this.dictionary.TryGetValue(key, out existValue));
            value = existValue;
            return result;
        }

        public void Add(TKey key, TValue value)
        {
            this.WriteOperation(() => this.dictionary.Add(key, value));
        }

        public bool ContainsKey(TKey key)
        {
            return this.ReadOperation<bool>(() => this.dictionary.ContainsKey(key));
        }

        public ICollection<TKey> Keys
        {
            get { return this.ReadOperation<List<TKey>>(() => new List<TKey>(this.dictionary.Keys)); }
        }

        public bool Remove(TKey key)
        {
            return this.WriteOperation<bool>(() => this.dictionary.Remove(key));
        }

        public ICollection<TValue> Values
        {
            get
            {
                return this.ReadOperation<List<TValue>>(() => new List<TValue>(this.dictionary.Values));
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                return this.ReadOperation<TValue>(() => this.dictionary[key]);
            }
            set
            {
                this.WriteOperation(() => this.dictionary[key] = value);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            this.WriteOperation(() => this.dictionary.Clear());
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return this.ReadOperation<bool>(() => this.dictionary.Contains(item));
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            this.WriteOperation(() =>
            {
                int startIndex = arrayIndex;
                foreach (KeyValuePair<TKey, TValue> item in this)
                {
                    array[startIndex++] = item;
                }
            }
                );
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int Count
        {
            get
            {
                return this.ReadOperation<int>(() => this.dictionary.Count);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return this.WriteOperation<bool>(() => this.dictionary.Remove(item.Key));
        }

        T ReadOperation<T>(Func<T> func)
        {
            this.locker.EnterWriteLock();

            try
            {
                return func();
            }
            finally
            {
                this.locker.ExitWriteLock();
            }
        }

        void WriteOperation(Action operation)
        {
            this.locker.EnterWriteLock();

            try
            {
                operation();
            }
            finally
            {
                this.locker.ExitWriteLock();
            }
        }

        T WriteOperation<T>(Func<T> operation)
        {
            this.locker.EnterWriteLock();

            try
            {
                return operation();
            }
            finally
            {
                this.locker.ExitWriteLock();
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.dictionary.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.dictionary.GetEnumerator();
        }

        public void Dispose()
        {
            if (this.locker != null)
            {
                this.locker.Dispose();
                this.locker = null;
            }
        }
    }
}