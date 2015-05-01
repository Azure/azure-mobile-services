// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Query;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    internal class PullAction : TableAction
    {
        private static readonly DateTimeOffset Epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        private IDictionary<string, string> parameters;
        private MobileServiceRemoteTableOptions options; // the supported options on remote table 
        private readonly PullCursor cursor;
        private Task pendingAction;
        private PullStrategy strategy;

        public PullAction(MobileServiceTable table,
                          MobileServiceTableKind tableKind,
                          MobileServiceSyncContext context,
                          string queryId,
                          MobileServiceTableQueryDescription query,
                          IDictionary<string, string> parameters,
                          IEnumerable<string> relatedTables,
                          OperationQueue operationQueue,
                          MobileServiceSyncSettingsManager settings,
                          IMobileServiceLocalStore store,
                          MobileServiceRemoteTableOptions options,
                          MobileServiceObjectReader reader,
                          CancellationToken cancellationToken)
            : base(table, tableKind, queryId, query, relatedTables, context, operationQueue, settings, store, cancellationToken)
        {
            this.options = options;
            this.parameters = parameters;
            this.cursor = new PullCursor(query);
            this.Reader = reader ?? new MobileServiceObjectReader();
        }

        public MobileServiceObjectReader Reader { get; private set; }


        protected override Task<bool> HandleDirtyTable()
        {
            // there are pending operations on the same table so defer the action
            this.pendingAction = this.Context.DeferTableActionAsync(this);
            // we need to return in order to give PushAsync a chance to execute so we don't await the pending push
            return Task.FromResult(false);
        }

        protected override Task WaitPendingAction()
        {
            return this.pendingAction ?? Task.FromResult(0);
        }

        protected async override Task ProcessTableAsync()
        {
            await CreatePullStrategy();

            this.Table.SystemProperties |= MobileServiceSystemProperties.Deleted;

            QueryResult result;
            do
            {
                this.CancellationToken.ThrowIfCancellationRequested();

                string query = this.Query.ToODataString();
                if (this.Query.UriPath != null)
                {
                    query = MobileServiceUrlBuilder.CombinePathAndQuery(this.Query.UriPath, query);
                }
                result = await this.Table.ReadAsync(query, MobileServiceTable.IncludeDeleted(parameters), this.Table.Features);
                await this.ProcessAll(result.Values); // process the first batch

                result = await FollowNextLinks(result);
            }
            // if we are not at the end of result and there is no link to get more results                
            while (!this.EndOfResult(result) && await this.strategy.MoveToNextPageAsync());

            await this.strategy.PullCompleteAsync();
        }

        private async Task ProcessAll(JArray items)
        {
            this.CancellationToken.ThrowIfCancellationRequested();

            var deletedIds = new List<string>();
            var upsertList = new List<JObject>();

            foreach (JObject item in items)
            {
                if (!this.cursor.OnNext())
                {
                    break;
                }

                string id = this.Reader.GetId(item);
                if (id == null)
                {
                    continue;
                }

                DateTimeOffset updatedAt = this.Reader.GetUpdatedAt(item).GetValueOrDefault(Epoch).ToUniversalTime();
                strategy.SetUpdateAt(updatedAt);

                if (this.Reader.IsDeleted(item))
                {
                    deletedIds.Add(id);
                }
                else
                {
                    upsertList.Add(item);
                }
            }

            if (upsertList.Any())
            {
                await this.Store.UpsertAsync(this.Table.TableName, upsertList, ignoreMissingColumns: true);
            }

            if (deletedIds.Any())
            {
                await this.Store.DeleteAsync(this.Table.TableName, deletedIds);
            }

            await this.strategy.OnResultsProcessedAsync();
        }

        // follows next links in the query result and returns final result
        private async Task<QueryResult> FollowNextLinks(QueryResult result)
        {
            while (!this.EndOfResult(result) && // if we are not at the end of result
                    IsNextLinkValid(result.NextLink, this.options)) // and there is a valid link to get more results
            {
                this.CancellationToken.ThrowIfCancellationRequested();

                result = await this.Table.ReadAsync(result.NextLink);
                await this.ProcessAll(result.Values); // process the results as soon as we've gotten them
            }
            return result;
        }

        // mongo doesn't support skip and top yet it generates next links with top and skip
        private bool IsNextLinkValid(Uri link, MobileServiceRemoteTableOptions options)
        {
            if (link == null)
            {
                return false;
            }

            IDictionary<string, string> parameters = HttpUtility.ParseQueryString(link.Query);

            bool isValid = ValidateOption(options, parameters, ODataOptions.Top, MobileServiceRemoteTableOptions.Top) &&
                           ValidateOption(options, parameters, ODataOptions.Skip, MobileServiceRemoteTableOptions.Skip) &&
                           ValidateOption(options, parameters, ODataOptions.OrderBy, MobileServiceRemoteTableOptions.OrderBy);

            return isValid;
        }

        private static bool ValidateOption(MobileServiceRemoteTableOptions validOptions, IDictionary<string, string> parameters, string optionKey, MobileServiceRemoteTableOptions option)
        {
            bool hasInvalidOption = parameters.ContainsKey(optionKey) && !validOptions.HasFlag(option);
            return !hasInvalidOption;
        }

        private bool EndOfResult(QueryResult result)
        {
            // if we got as many as we initially wanted 
            // or there are no more results
            // then we're at the end
            return cursor.Complete || result.Values.Count == 0;
        }

        private async Task CreatePullStrategy()
        {
            bool isIncrementalSync = !String.IsNullOrEmpty(this.QueryId);
            if (isIncrementalSync)
            {
                this.strategy = new IncrementalPullStrategy(this.Table, this.Query, this.QueryId, this.Settings, this.cursor, this.options);
            }
            else
            {
                this.strategy = new PullStrategy(this.Query, this.cursor, this.options);
            }
            await this.strategy.InitializeAsync();
        }
    }
}
