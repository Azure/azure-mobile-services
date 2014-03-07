// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service.Tables;
using System;

namespace ZumoE2EServerApp.DataObjects
{
    public class ParamsTestTableEntity : ITableData
    {
        public int Id { get; set; }
        public string Parameters { get; set; }

        public ParamsTestTableEntity()
        {
            this.Id = 1;
        }

        string ITableData.Id { get; set; }

        byte[] ITableData.Version { get; set; }

        DateTimeOffset? ITableData.CreatedAt { get; set; }

        DateTimeOffset? ITableData.UpdatedAt { get; set; }

        bool ITableData.Deleted { get; set; }
    }
}