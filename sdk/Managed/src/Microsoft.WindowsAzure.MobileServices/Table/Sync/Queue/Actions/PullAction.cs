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
        private IDictionary<string, string> parameters;
        private MobileServiceRemoteTableOptions options; // the supported options on remote table 
        private readonly PullCursor cursor;
        private PullStrategy strategy;

        public PullAction(MobileServiceTable table,
                          MobileServiceSyncContext context,
                          string queryKey,
                          MobileServiceTableQueryDescription query,
                          IDictionary<string, string> parameters,
                          OperationQueue operationQueue,
                          MobileServiceSyncSettingsManager settings,
                          IMobileServiceLocalStore store,
                          MobileServiceRemoteTableOptions options,
                          CancellationToken cancellationToken)
            : base(table, queryKey, query, context, operationQueue, settings, store, cancellationToken)
        {
            this.options = options;
            this.parameters = parameters;
            this.cursor = new PullCursor(query);
        }

        protected override bool CanDeferIfDirty
        {
            get { return true; }
        }

        protected async override Task ProcessTableAsync()
        {
            await CreatePullStrategy();

            QueryResult result;
            do
            {
                this.CancellationToken.ThrowIfCancellationRequested();

                string odata = this.Query.ToODataString();
                result = await this.Table.ReadAsync(odata, MobileServiceTable.IncludeDeleted(parameters), this.Table.Features);
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

                string id = (string)item[MobileServiceSystemColumns.Id];
                if (id == null)
                {
                    continue;
                }

                DateTimeOffset updatedAt = GetUpdatedAt(item).ToUniversalTime();
                strategy.SetUpdateAt(updatedAt);

                if (IsDeleted(item))
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
                await this.Store.UpsertAsync(this.Table.TableName, upsertList, fromServer: true);
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

        private static bool IsDeleted(JObject item)
        {
            JToken deletedToken = item[MobileServiceSystemColumns.Deleted];
            bool isDeleted = deletedToken != null && deletedToken.Value<bool>();
            return isDeleted;
        }

        private static DateTimeOffset GetUpdatedAt(JObject item)
        {
            DateTimeOffset updatedAt = DateTimeOffset.MinValue;
            JToken updatedAtToken = item[MobileServiceSystemColumns.UpdatedAt];
            if (updatedAtToken != null)
            {
                updatedAt = updatedAtToken.ToObject<DateTimeOffset>();
            }
            return updatedAt;
        }

        private async Task CreatePullStrategy()
        {
            bool isIncrementalSync = !String.IsNullOrEmpty(this.QueryKey);
            if (isIncrementalSync)
            {
                this.strategy = new IncrementalPullStrategy(this.Table, this.Query, this.QueryKey, this.Settings, this.cursor, this.options);
            }
            else
            {
                this.strategy = new PullStrategy(this.Query, this.cursor, this.options);
            }
            await this.strategy.InitializeAsync();
        }
    }
}
