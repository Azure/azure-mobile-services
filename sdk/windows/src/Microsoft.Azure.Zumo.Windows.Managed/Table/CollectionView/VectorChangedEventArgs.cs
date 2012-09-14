// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Windows.Foundation.Collections;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides information about changes to an observable vector.
    /// </summary>
    internal class VectorChangedEventArgs : IVectorChangedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the VectorChangedEventArgs class.
        /// </summary>
        /// <param name="change">The change to the vector.</param>
        /// <param name="index">Index of the changed element.</param>
        public VectorChangedEventArgs(CollectionChange change, uint index)
        {
            this.CollectionChange = change;
            this.Index = index;
        }

        /// <summary>
        /// Gets the change to the vector.
        /// </summary>
        public CollectionChange CollectionChange { get; private set; }

        /// <summary>
        /// Gets the index of the changed element.
        /// </summary>
        public uint Index { get; private set; }
    }
}
