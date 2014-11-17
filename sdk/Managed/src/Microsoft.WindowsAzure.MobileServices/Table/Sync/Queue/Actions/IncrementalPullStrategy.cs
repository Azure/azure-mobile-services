// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Query;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    internal class IncrementalPullStrategy : PullStrategy
    {
        private static readonly QueryNode updatedAtNode = new MemberAccessNode(null, MobileServiceSystemColumns.UpdatedAt);
        private static readonly OrderByNode orderByUpdatedAtNode = new OrderByNode(updatedAtNode, OrderByDirection.Ascending);

        private readonly QueryNode originalFilter; // filter before the delta token was applied
        private readonly MobileServiceTable table;
        private readonly string queryId;
        private readonly MobileServiceSyncSettingsManager settings;
        private readonly bool ordered;

        private DateTimeOffset maxUpdatedAt;
        private DateTimeOffset deltaToken;

        public IncrementalPullStrategy(MobileServiceTable table,
                                       MobileServiceTableQueryDescription query,
                                       string queryId,
                                       MobileServiceSyncSettingsManager settings,
                                       PullCursor cursor,
                                       MobileServiceRemoteTableOptions options)
            : base(query, cursor, options)
        {
            this.table = table;
            this.originalFilter = query.Filter;
            this.queryId = queryId;
            this.settings = settings;
            this.ordered = options.HasFlag(MobileServiceRemoteTableOptions.OrderBy);
        }


        public override async Task InitializeAsync()
        {
            this.table.SystemProperties = this.table.SystemProperties | MobileServiceSystemProperties.UpdatedAt;
            this.maxUpdatedAt = await this.settings.GetDeltaTokenAsync(this.Query.TableName, this.queryId);
            this.UpdateDeltaToken();

            await base.InitializeAsync();
        }


        public override async Task OnResultsProcessedAsync()
        {
            if (this.ordered)
            {
                await this.SaveMaxUpdatedAtAsync();
            }
        }

        public override void SetUpdateAt(DateTimeOffset updatedAt)
        {
            if (updatedAt > this.maxUpdatedAt)
            {
                this.maxUpdatedAt = updatedAt;
            }
        }

        public override async Task<bool> MoveToNextPageAsync()
        {
            if (this.ordered && this.maxUpdatedAt > this.deltaToken)
            {
                this.UpdateDeltaToken();
                await this.SaveMaxUpdatedAtAsync();

                // reset the cursor because deltatoken has changed
                this.Cursor.Reset();
                if (this.SupportsSkip)
                {
                    this.Query.Skip = 0;
                }
                this.ReduceTop();

                return true;
            }
            return await base.MoveToNextPageAsync();
        }


        public override Task PullCompleteAsync()
        {
            return this.SaveMaxUpdatedAtAsync();
        }

        private async Task SaveMaxUpdatedAtAsync()
        {
            if (this.maxUpdatedAt > this.deltaToken)
            {
                await this.settings.SetDeltaTokenAsync(this.Query.TableName, this.queryId, this.maxUpdatedAt);
            }
        }

        private void UpdateDeltaToken()
        {
            this.deltaToken = this.maxUpdatedAt;

            if (this.ordered)
            {
                this.Query.Ordering.Clear();
                this.Query.Ordering.Add(orderByUpdatedAtNode);
            }
            // .NET runtime system properties are of datetimeoffset type so we'll use the datetimeoffset odata token
            QueryNode tokenNode = new ConstantNode(deltaToken);
            QueryNode greaterThanDeltaNode = new BinaryOperatorNode(BinaryOperatorKind.GreaterThanOrEqual, updatedAtNode, tokenNode);
            if (this.originalFilter == null)
            {
                this.Query.Filter = greaterThanDeltaNode;
            }
            else
            {
                var originalFilterAndGreaterThanDeltaNode = new BinaryOperatorNode(BinaryOperatorKind.And, this.originalFilter, greaterThanDeltaNode);
                this.Query.Filter = originalFilterAndGreaterThanDeltaNode;
            }
        }
    }
}
