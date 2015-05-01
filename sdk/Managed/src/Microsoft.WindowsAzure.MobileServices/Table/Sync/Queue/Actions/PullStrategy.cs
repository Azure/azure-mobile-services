// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Query;
namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    internal class PullStrategy
    {
        private const int DefaultTop = 50;

        protected MobileServiceTableQueryDescription Query { get; private set; }
        protected PullCursor Cursor { get; private set; }
        protected bool SupportsSkip { get; private set; }
        protected bool SupportsTop { get; private set; }

        public PullStrategy(MobileServiceTableQueryDescription query,
                            PullCursor cursor,
                            MobileServiceRemoteTableOptions options)
        {
            this.Query = query;
            this.Cursor = cursor;
            this.SupportsSkip = options.HasFlag(MobileServiceRemoteTableOptions.Skip);
            this.SupportsTop = options.HasFlag(MobileServiceRemoteTableOptions.Top);
        }

        public virtual Task InitializeAsync()
        {
            if (this.SupportsTop)
            {
                if (this.SupportsSkip) // mongo requires skip if top is given but table storage doesn't support skip
                {
                    this.Query.Skip = this.Query.Skip.GetValueOrDefault();
                }

                // always download in batches of 50 or less for efficiency 
                this.Query.Top = Math.Min(this.Query.Top.GetValueOrDefault(DefaultTop), DefaultTop);
            }
            return Task.FromResult(0);
        }

        public virtual Task OnResultsProcessedAsync()
        {
            return Task.FromResult(0);
        }

        public virtual void SetUpdateAt(DateTimeOffset updatedAt)
        {
        }

        /// <summary>
        /// Tries to move the cursor forward by changing where it starts from to be the latest result
        /// </summary>
        /// <returns>True if cursor can be shifted forward, False otherwise</returns>
        public virtual Task<bool> MoveToNextPageAsync()
        {
            bool shifted = false;
            if (this.SupportsSkip)
            {
                // then we continue downloading the changes using skip and top
                this.Query.Skip = this.Cursor.Position;
                this.ReduceTop();

                shifted = true;
            }
            return Task.FromResult(shifted);
        }

        protected void ReduceTop()
        {
            if (this.SupportsTop)
            {
                // only read as many as we want
                this.Query.Top = Math.Min(this.Query.Top.Value, Cursor.Remaining);
            }
        }

        public virtual Task PullCompleteAsync()
        {
            return Task.FromResult(0);
        }
    }
}
