// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// Responsible for executing a batch of operations from queue until bookmark is found
    /// </summary>
    internal class PushAction: SyncAction
    {
        private IMobileServiceSyncHandler syncHandler;
        private MobileServiceClient client;
        private MobileServiceSyncContext context;

        public PushAction(OperationQueue operationQueue, 
                          IMobileServiceLocalStore store, 
                          IMobileServiceSyncHandler syncHandler,
                          MobileServiceClient client,
                          MobileServiceSyncContext context,
                          CancellationToken cancellationToken): base(operationQueue, store, cancellationToken)
        {
            this.client = client;
            this.syncHandler = syncHandler;
            this.context = context;
        }        

        public override async Task ExecuteAsync()
        {
            var batch = new OperationBatch(this.syncHandler, this.Store, this.context);
            List<MobileServiceTableOperationError> syncErrors = new List<MobileServiceTableOperationError>();
            MobileServicePushStatus batchStatus = MobileServicePushStatus.InternalError;

            try
            {
                batchStatus = await ExecuteBatchAsync(batch, syncErrors);
            }
            catch (Exception ex)
            {
                batch.OtherErrors.Add(ex);
            }

            await FinalizePush(batch, batchStatus, syncErrors);
        }

        private async Task<MobileServicePushStatus> ExecuteBatchAsync(OperationBatch batch, List<MobileServiceTableOperationError> syncErrors)
        {
            // when cancellation is requested, abort the batch
            this.CancellationToken.Register(() => batch.Abort(MobileServicePushStatus.CancelledByToken));

            try
            {
                await ExecuteAllOperationsAsync(batch);
            }
            catch (Exception ex)
            {
                batch.OtherErrors.Add(ex);
            }

            MobileServicePushStatus batchStatus = batch.AbortReason.GetValueOrDefault(MobileServicePushStatus.Complete);
            try
            {
                syncErrors.AddRange(await batch.LoadSyncErrorsAsync(this.client.SerializerSettings));
            }
            catch (Exception ex)
            {

                batch.OtherErrors.Add(new MobileServiceLocalStoreException(Resources.SyncStore_FailedToLoadError, ex));
            }
            return batchStatus;
        }

        private async Task FinalizePush(OperationBatch batch, MobileServicePushStatus batchStatus, IEnumerable<MobileServiceTableOperationError> syncErrors)
        {
            var result = new MobileServicePushCompletionResult(syncErrors, batchStatus);

            try
            {
                await batch.SyncHandler.OnPushCompleteAsync(result);

                // now that we've successfully given the errors to user, we can delete them from store
                await batch.DeleteErrorsAsync();
            }
            catch (Exception ex)
            {
                batch.OtherErrors.Add(ex);
            }

            if (batchStatus != MobileServicePushStatus.Complete || batch.HasErrors(syncErrors))
            {
                List<MobileServiceTableOperationError> unhandledSyncErrors = syncErrors.Where(e => !e.Handled).ToList();

                Exception inner;
                if (batch.OtherErrors.Count == 1)
                {
                    inner = batch.OtherErrors[0];
                }
                else
                {
                    inner = batch.OtherErrors.Any() ? new AggregateException(batch.OtherErrors) : null;
                }

                // create a new result with only unhandled errors
                result = new MobileServicePushCompletionResult(unhandledSyncErrors, batchStatus);
                this.TaskSource.TrySetException(new MobileServicePushFailedException(result, inner));
            }
            else
            {
                this.TaskSource.SetResult(0);
            }
        }

        private async Task ExecuteAllOperationsAsync(OperationBatch batch)
        {
            MobileServiceTableOperation operation = await this.OperationQueue.PeekAsync(0);

            // keep taking out operations and executing them until queue is empty or operation finds the bookmark or batch is aborted 
            while (operation != null)
            {
                bool success = await this.ExecuteOperationAsync(operation, batch);

                if (batch.AbortReason.HasValue)
                {
                    break;
                }

                if (success)
                {
                    // we successfuly executed an operation so remove it from queue
                    await this.OperationQueue.DeleteAsync(operation.Id);
                }

                // get next operation
                operation = await this.OperationQueue.PeekAsync(operation.Sequence);
            }
        }

        private async Task<bool> ExecuteOperationAsync(MobileServiceTableOperation operation, OperationBatch batch)
        {
            using (await this.OperationQueue.LockItemAsync(operation.ItemId, this.CancellationToken))
            {
                if (operation.IsCancelled || this.CancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                Exception error = null;

                operation.Table = await this.GetTable(operation.TableName);
                await this.LoadOperationItem(operation, batch);

                if (this.CancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                // strip out system properties before executing the operation
                operation.Item = MobileServiceSyncTable.RemoveSystemPropertiesKeepVersion(operation.Item);

                JObject result = null;
                try
                {
                    result = await batch.SyncHandler.ExecuteTableOperationAsync(operation);                    
                }
                catch (Exception ex)
                {
                    if (TryAbortBatch(batch, ex))
                    {
                        // there is no error to save in sync error and no result to capture
                        // this operation will be executed again next time the push happens
                        return false;
                    }

                    error = ex;
                }                

                // save the result if ExecuteTableOperation did not throw
                if (error == null && result.IsValidItem() && operation.CanWriteResultToStore)
                {
                    try
                    {
                        await this.Store.UpsertAsync(operation.TableName, result, fromServer: true);
                    }
                    catch (Exception ex)
                    {
                        batch.Abort(MobileServicePushStatus.CancelledBySyncStoreError);
                        throw new MobileServiceLocalStoreException(Resources.SyncStore_FailedToUpsertItem, ex);
                    }
                }
                else if (error != null)
                {
                    HttpStatusCode? statusCode = null;
                    string rawResult = null;
                    var iox = error as MobileServiceInvalidOperationException;
                    if (iox != null && iox.Response != null)
                    {
                        statusCode = iox.Response.StatusCode;
                        Tuple<string, JToken> content = await MobileServiceTable.ParseContent(iox.Response, this.client.SerializerSettings);
                        rawResult = content.Item1;
                        result = content.Item2.ValidItemOrNull();
                    }
                    var syncError = new MobileServiceTableOperationError(statusCode,
                                                                         operation.Id,
                                                                         operation.Kind,
                                                                         operation.TableName,
                                                                         operation.Item,
                                                                         rawResult,
                                                                         result)
                                                                         {
                                                                             Context = this.context
                                                                         };
                    await batch.AddSyncErrorAsync(syncError);
                }

                bool success = error == null;
                return success;
            }
        }

        private async Task<MobileServiceTable> GetTable(string tableName)
        {
            var table = this.client.GetTable(tableName) as MobileServiceTable;
            JObject value = await this.Store.LookupAsync(MobileServiceLocalSystemTables.Config, tableName + "_systemProperties");
            if (value == null)
            {
                table.SystemProperties = MobileServiceSystemProperties.Version;
            }
            else
            {
                table.SystemProperties = (MobileServiceSystemProperties)value.Value<int>("value");
            }
            table.AddRequestHeader(MobileServiceHttpClient.ZumoFeaturesHeader, MobileServiceFeatures.Offline);

            return table;
        }

        private async Task LoadOperationItem(MobileServiceTableOperation operation, OperationBatch batch)
        {
            // only read the item from store if it is not in the operation already
            if (operation.Item == null)
            {
                try
                {
                    operation.Item = await this.Store.LookupAsync(operation.TableName, operation.ItemId) as JObject;
                }
                catch (Exception ex)
                {
                    batch.Abort(MobileServicePushStatus.CancelledBySyncStoreError);
                    throw new MobileServiceLocalStoreException(Resources.SyncStore_FailedToReadItem, ex);
                }
            }
        }

        private bool TryAbortBatch(OperationBatch batch, Exception ex)
        {
            if (ex.IsNetworkError())
            {                
                batch.Abort(MobileServicePushStatus.CancelledByNetworkError);
            }
            else if (ex.IsAuthenticationError())
            {
                batch.Abort(MobileServicePushStatus.CancelledByAuthenticationError);
            }
            else if (ex is MobileServicePushAbortException)
            {
                batch.Abort(MobileServicePushStatus.CancelledByOperation);
            }
            else
            {
                return false; // not a known exception that should abort the batch
            }

            return true;
        }
    }
}
