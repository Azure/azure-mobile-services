// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore
{
    internal class TableDefinition: Dictionary<string, ColumnDefinition>
    {
        public TableDefinition()
        {
        }

        public TableDefinition(IDictionary<string, ColumnDefinition> definition): base(definition, StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}
