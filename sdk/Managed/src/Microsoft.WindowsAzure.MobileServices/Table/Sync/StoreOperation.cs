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
        /// <summary>
        /// Creates an instance of <see cref="StoreOperation"/> class.
        /// </summary>
        /// <param name="tableName">The name of the table that is the target of this operation.</param>
        /// <param name="recordId">The target record identifier.</param>
        /// <param name="kind">The kind of operation.</param>
        /// <param name="source">The source that triggered this operation.</param>
        /// <param name="batchId">The id of the batch this operation belongs to.</param>
        public StoreOperation(string tableName, string recordId, LocalStoreOperationKind kind, StoreOperationSource source, string batchId)
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

        /// <summary>
        /// The ID of the batch this operation belongs to.
        /// </summary>
        public string BatchId { get; set; }

        /// <summary>
        /// The name of the table this operation was executed against.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// The ID of the record this operation that was the target of this operation.
        /// </summary>
        public string RecordId { get; set; }

        /// <summary>
        /// The kind of operation.
        /// </summary>
        public LocalStoreOperationKind Kind { get; set; }

        /// <summary>
        /// Describes the source this operation was triggered from.
        /// </summary>
        public StoreOperationSource Source { get; set; }
    }
}
