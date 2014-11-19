// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Microsoft.WindowsAzure.MobileServices.Query;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// An object to represent the current position of pull in full query resullt
    /// </summary>
    internal class PullCursor
    {
        private readonly int maxRead; // used to limit the next link navigation because table storage and sql in .NET backend always return a link and also to obey $top if present
        private int initialSkip;
        private int totalRead; // used to track how many we have read so far since the last delta
        public int Remaining { get; private set; }

        public int Position
        {
            get { return this.initialSkip + this.totalRead; }
        }

        public bool Complete
        {
            get { return this.Remaining <= 0; }
        }

        public PullCursor(MobileServiceTableQueryDescription query)
        {
            this.Remaining = this.maxRead = query.Top.GetValueOrDefault(Int32.MaxValue);
            this.initialSkip = query.Skip.GetValueOrDefault();
        }

        /// <summary>
        /// Called when ever an item is processed from result
        /// </summary>
        /// <returns>True if cursor is still open, False when it is completed.</returns>
        public bool OnNext()
        {
            if (this.Complete)
            {
                return false;
            }

            this.Remaining--;
            this.totalRead++;

            return true;
        }

        /// <summary>
        /// Called when delta token is modified
        /// </summary>
        public void Reset()
        {
            this.initialSkip = 0;
            this.totalRead = 0;
        }
    }
}
