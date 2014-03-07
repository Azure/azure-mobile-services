// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service.Tables;
using System;

namespace ZumoE2EServerApp.DataObjects
{
    public class TestUser : ITableData
    {
        public string Name { get; set; }

        public string Id { get; set; }

        public string UserId { get; set; }

        public string[] Identities { get; set; }

        public byte[] Version { get; set; }

        public DateTimeOffset? CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public bool Deleted { get; set; }
    }
}