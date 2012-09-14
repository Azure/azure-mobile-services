// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.Azure.Zumo.Win8.Test;
using Microsoft.WindowsAzure.MobileServices;

namespace Microsoft.Azure.Zumo.Win8.CSharp.Test
{
    [DataTable(Name = "types")]
    public class DateExample
    {
        public int Id { get; set; }

        [DataMember(Name="DateExampleDate")]
        public DateTime Date { get; set; }
    }

    [DataTable(Name = "types")]
    public class DateOffsetExample
    {
        public int Id { get; set; }

        [DataMember(Name = "DateOffsetExampleDate")]
        public DateTimeOffset Date { get; set; }
    }

    [Tag("Date")]
    public class DateTests : FunctionalTestBase
    {
        [AsyncTestMethod]
        public async Task DateUri()
        {
            TestServiceFilter hijack = new TestServiceFilter();
            MobileServiceClient client = new MobileServiceClient("http://www.test.com").WithFilter(hijack);
            IMobileServiceTable<DateExample> table = client.GetTable<DateExample>();

            hijack.Response.StatusCode = 200;
            hijack.Response.Content = "[]";
            
            // Verify a full UTC date
            DateTime date = new DateTime(2009, 11, 21, 14, 22, 59, 860, DateTimeKind.Utc);
            await table.Where(b => b.Date == date).ToEnumerableAsync();
            Assert.EndsWith(hijack.Request.Uri.ToString(), "$filter=(DateExampleDate eq datetime'2009-11-21T14:22:59.860Z')");

            // Local date is converted to UTC
            date = new DateTime(2009, 11, 21, 14, 22, 59, 860, DateTimeKind.Local);
            await table.Where(b => b.Date == date).ToEnumerableAsync();
            Assert.EndsWith(hijack.Request.Uri.ToString(), "Z')");
        }

        [AsyncTestMethod]
        public async Task DateOffsetUri()
        {
            TestServiceFilter hijack = new TestServiceFilter();
            MobileServiceClient client = new MobileServiceClient("http://www.test.com").WithFilter(hijack);
            IMobileServiceTable<DateOffsetExample> table = client.GetTable<DateOffsetExample>();

            hijack.Response.StatusCode = 200;
            hijack.Response.Content = "[]";

            DateTimeOffset date = new DateTimeOffset(
                new DateTime(2009, 11, 21, 14, 22, 59, 860, DateTimeKind.Utc).ToLocalTime());
            await table.Where(b => b.Date == date).ToEnumerableAsync();
            Assert.EndsWith(hijack.Request.Uri.ToString(), "$filter=(DateOffsetExampleDate eq datetime'2009-11-21T14:22:59.860Z')");
        }

        [AsyncTestMethod]
        public async Task InsertAndQuery()
        {
            IMobileServiceTable<DateExample> table = GetClient().GetTable<DateExample>();

            DateTime date = new DateTime(2009, 10, 21, 14, 22, 59, 860, DateTimeKind.Local);
            Log("Start: {0}", date);

            Log("Inserting instance");
            DateExample instance = new DateExample { Date = date };
            await table.InsertAsync(instance);
            Assert.AreEqual(date, instance.Date);

            Log("Querying for instance");
            List<DateExample> items = await table.Where(i => i.Date == date).Where(i => i.Id >= instance.Id).ToListAsync();
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(date, items[0].Date);

            Log("Finish: {0}", items[0].Date);
        }

        [AsyncTestMethod]
        public async Task InsertAndQueryOffset()
        {
            IMobileServiceTable<DateOffsetExample> table = GetClient().GetTable<DateOffsetExample>();

            DateTimeOffset date = new DateTimeOffset(
                new DateTime(2009, 10, 21, 14, 22, 59, 860, DateTimeKind.Utc).ToLocalTime());
            Log("Start: {0}", date);

            Log("Inserting instance");
            DateOffsetExample instance = new DateOffsetExample { Date = date };
            await table.InsertAsync(instance);
            Assert.AreEqual(date, instance.Date);

            Log("Querying for instance");
            List<DateOffsetExample> items = await table.Where(i => i.Date == date).Where(i => i.Id >= instance.Id).ToListAsync();
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(date, items[0].Date);

            Log("Finish: {0}", items[0].Date);
        }

        [AsyncTestMethod]
        public async Task DateKinds()
        {
            IMobileServiceTable<DateExample> table = GetClient().GetTable<DateExample>();

            DateTime original = new DateTime(2009, 10, 21, 14, 22, 59, 860, DateTimeKind.Local);
            DateExample instance = new DateExample { Date = original };
            
            Log("Start Kind: {0}", instance.Date.Kind);
            await table.InsertAsync(instance);
            Assert.AreEqual(DateTimeKind.Local, instance.Date.Kind);
            Assert.AreEqual(original, instance.Date);

            Log("Change to UTC");
            instance.Date = new DateTime(2010, 5, 21, 0, 0, 0, 0, DateTimeKind.Utc);
            await table.UpdateAsync(instance);
            Assert.AreEqual(DateTimeKind.Local, instance.Date.Kind);

            Log("Change to Local");
            instance.Date = new DateTime(2010, 5, 21, 0, 0, 0, 0, DateTimeKind.Local);
            await table.UpdateAsync(instance);
            Assert.AreEqual(DateTimeKind.Local, instance.Date.Kind);
        }

        [AsyncTestMethod]
        public async Task ChangeCulture()
        {
            IMobileServiceTable<DateExample> table = GetClient().GetTable<DateExample>();

            CultureInfo threadCulture = CultureInfo.DefaultThreadCurrentCulture;
            CultureInfo threadUICulture = CultureInfo.DefaultThreadCurrentUICulture;

            DateTime original = new DateTime(2009, 10, 21, 14, 22, 59, 860, DateTimeKind.Local);
            DateExample instance = new DateExample { Date = original };
            await table.InsertAsync(instance);
            
            Log("Change culture to ar-EG");
            CultureInfo arabic = new CultureInfo("ar-EG");
            CultureInfo.DefaultThreadCurrentCulture = arabic;
            CultureInfo.DefaultThreadCurrentUICulture = arabic;
            DateExample arInstance = await table.LookupAsync(instance.Id);
            Assert.AreEqual(original, arInstance.Date);

            Log("Change culture to zh-CHS");
            CultureInfo chinese = new CultureInfo("zh-CHS");
            CultureInfo.DefaultThreadCurrentCulture = chinese;
            CultureInfo.DefaultThreadCurrentUICulture = chinese;
            DateExample zhInstance = await table.LookupAsync(instance.Id);
            Assert.AreEqual(original, zhInstance.Date);

            Log("Change culture to ru-RU");
            CultureInfo russian = new CultureInfo("ru-RU");
            CultureInfo.DefaultThreadCurrentCulture = russian;
            CultureInfo.DefaultThreadCurrentUICulture = russian;
            DateExample ruInstance = await table.LookupAsync(instance.Id);
            Assert.AreEqual(original, ruInstance.Date);

            CultureInfo.DefaultThreadCurrentCulture = threadCulture;
            CultureInfo.DefaultThreadCurrentUICulture = threadUICulture;
        }
    }
}
