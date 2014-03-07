// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service.Tables;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using ZumoE2EServerApp.DataObjects;

namespace ZumoE2EServerApp.Models
{
    public class SDKClientTestContext : DbContext
    {
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx

        public SDKClientTestContext(string schema)
            : base("Name=MS_TableConnectionString")
        {
            Schema = schema;
        }

        public string Schema { get; set; }

        public DbSet<StringIdMovie> Movies { get; set; }

        public DbSet<RoundTripTableItem> RoundTripTableItems { get; set; }

        public DbSet<StringIdRoundTripTableItemForDB> StringIdRoundTripTableItemForDBs { get; set; }

        public DbSet<W8JSRoundTripTableItemForDB> W8JSRoundTripTableForDBs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            if (Schema != null)
            {
                modelBuilder.HasDefaultSchema(Schema);
            }

            modelBuilder.Conventions.Add(
                new AttributeToColumnAnnotationConvention<TableColumnAttribute, string>(
                    "ServiceTableColumn", (property, attributes) => attributes.Single().ColumnType.ToString()));
        }
    }
}