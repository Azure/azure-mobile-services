// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// Represents an operation against the local data store.
    /// </summary>
    public class StoreOperation
    {
        public StoreOperation(string tableName, string recordId, StoreOperationKind kind, StoreOperationSource source, string batchId)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }
            if (recordId == null)
            {
                throw new ArgumentNullException("recordId");
            }
            if(batchId == null)
            {
                throw new ArgumentNullException("batchId");
            }
            
            this.TableName = tableName;
            this.RecordId = recordId;
            this.Kind = kind;
            this.Source = source;
            this.BatchId = batchId;
        }

        public string BatchId { get; set; }

        public string TableName { get; set; }

        public string RecordId { get; set; }

        public StoreOperationKind Kind { get; set; }

        public StoreOperationSource Source { get; set; }
    }
}
