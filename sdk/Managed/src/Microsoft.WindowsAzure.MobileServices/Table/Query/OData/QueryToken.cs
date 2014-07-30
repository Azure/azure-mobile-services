// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Query
{
    internal class QueryToken
    {
        public int Position { get; set; }
        public string Text { get; set; }
        public QueryTokenKind Kind { get; set; }
    }
}
