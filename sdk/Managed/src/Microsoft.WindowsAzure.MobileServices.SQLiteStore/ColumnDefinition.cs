﻿// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore
{
    internal class ColumnDefinition
    {
        public string SqlType { get; private set; }

        public JProperty Property { get; private set; }

        public ColumnDefinition(string sqlType, JProperty property)
        {
            this.SqlType = sqlType;
            this.Property = property;
        }
    }
}
