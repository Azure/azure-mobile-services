// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Globalization;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    public class PullOptions
    {
        private int _maxPageSize;

        /// <summary>
        /// Constructor
        /// </summary>
        public PullOptions()
        {
            MaxPageSize = 50;
        }

        /// <summary>
        /// Maximum allowed size of a page while performing a pull operation.
        /// </summary>
        public int MaxPageSize
        {
            get { return _maxPageSize; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                        "Tried to set MaxPageSize to invalid value {0}", value));
                }

                _maxPageSize = value;
            }
        }
    }
}
