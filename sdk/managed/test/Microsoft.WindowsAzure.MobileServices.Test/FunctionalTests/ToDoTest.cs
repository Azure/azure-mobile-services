// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    // Repurpose the random empty table
    [DataTable("test_table")]
    public class ToDo
    {
        public long Id { get; set; }

        [JsonProperty(PropertyName = "col1")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "col5")]
        public bool Complete { get; set; }
    }

    [Tag("todo")]
    [Tag("e2e")]
    public class ToDoTest : FunctionalTestBase
    {
        [AsyncTestMethod]
        public async Task Basics()
        {
            // Insert a few records
            IMobileServiceTable<ToDo> table = GetClient().GetTable<ToDo>();
            ToDo first = new ToDo { Title = "ABC", Complete = false };
            await table.InsertAsync(first);
            await table.InsertAsync(new ToDo { Title = "DEF", Complete = true });
            await table.InsertAsync(new ToDo { Title = "GHI", Complete = false });

            // Run a simple query and verify we get all 3 items
            List<ToDo> items = await table.Where(t => t.Id >= first.Id).ToListAsync();
            Assert.AreEqual(3, items.Count);

            // Query and sort ascending
            items = await table.Where(t => t.Id >= first.Id).OrderBy(t => t.Title).ToListAsync();
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual("ABC", items[0].Title);
            Assert.AreEqual("DEF", items[1].Title);
            Assert.AreEqual("GHI", items[2].Title);

            // Query and sort descending
            items = await table.Where(t => t.Id >= first.Id).OrderByDescending(t => t.Title).ToListAsync();
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual("ABC", items[2].Title);
            Assert.AreEqual("DEF", items[1].Title);
            Assert.AreEqual("GHI", items[0].Title);

            // Filter to completed
            items = await table.Where(t => t.Id >= first.Id).Where(t => t.Complete == true).ToListAsync();
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual("DEF", items[0].Title);

            // Verify that inserting into a non-existant collection
            // also throws an error
            try
            {
                await GetClient().GetTable("notreal").ReadAsync(null);
            }
            catch (InvalidOperationException ex)
            {
                Assert.Contains(ex.Message, "notreal");
            }

            // Verify we can insert non-latin characters as a TODO item
            string title = "ÃÇßÑᾆΏ";
            ToDo item = new ToDo { Title = title };
            await table.InsertAsync(item);
            Assert.AreEqual(title, (await table.LookupAsync(item.Id)).Title);
        }

        [AsyncTestMethod]
        public async Task UseToDo()
        {
            // Insert a few records
            IMobileServiceTable<ToDo> table = GetClient().GetTable<ToDo>();
            ToDo first = new ToDo { Title = "Get Milk", Complete = false };
            await table.InsertAsync(first);
            await table.InsertAsync(new ToDo { Title = "Pick up dry cleaning", Complete = false });

            // Run a simple query and verify we get both items
            List<ToDo> items = await table.Where(i => i.Id >= first.Id).ToListAsync();
            Assert.AreEqual(2, items.Count);

            // Add another item
            await table.InsertAsync(new ToDo { Title = "Submit TPS report", Complete = false });

            // Check off the first item
            ToDo milk = items.Where(t => t.Title.Contains("Milk")).FirstOrDefault();
            milk.Complete = true;
            await table.UpdateAsync(milk);

            // Get the remaining items using a LINQ query
            IEnumerable<ToDo> remaining = await table.ReadAsync(
                from t in table
                where t.Complete == false && t.Id >= first.Id
                select t);
            Assert.AreEqual(2, remaining.Count());

            // Delete the first item
            await table.DeleteAsync(milk);
            items = await table.Where(t => t.Id >= first.Id).ToListAsync();
            Assert.AreEqual(2, items.Count);

            // Change the TPS report item without using the object
            // (to simulate the server object being changed by someone
            // else)
            ToDo tps = items.Where(t => t.Title.Contains("TPS")).FirstOrDefault();
            JObject jobject = new JObject();
            jobject["id"] = (int)tps.Id;
            jobject["col1"] = tps.Title + " using the new cover sheet";
            await table.UpdateAsync(jobject);
            await table.RefreshAsync(tps);
            Assert.Contains(tps.Title, "cover sheet");
        }

        [AsyncTestMethod]
        [Tag("TotalCount")]
        public async Task TotalCountBasics()
        {
            // Insert a few records
            IMobileServiceTable<ToDo> table = GetClient().GetTable<ToDo>();
            ToDo first = new ToDo { Title = "ABC", Complete = false };
            await table.InsertAsync(first);
            await table.InsertAsync(new ToDo { Title = "DEF", Complete = true });
            await table.InsertAsync(new ToDo { Title = "GHI", Complete = false });

            ITotalCountProvider countProvider = null;
            IMobileServiceTableQuery<ToDo> query = table.Where(t => t.Id >= first.Id);

            // Run a simple query and verify we get all 3 items, but the
            // TotalCount is not provided.
            List<ToDo> items = await query.ToListAsync();
            countProvider = items as ITotalCountProvider;
            Assert.AreEqual(3, items.Count);
            Assert.IsNotNull(countProvider);
            Assert.AreEqual(-1L, countProvider.TotalCount);

            IEnumerable<ToDo> sequence = await query.ToEnumerableAsync();
            countProvider = sequence as ITotalCountProvider;
            Assert.IsNotNull(countProvider);
            Assert.AreEqual(-1L, countProvider.TotalCount);

            // Now use IncludeTotalCount and make sure we get the expected
            // number of results
            query = query.IncludeTotalCount();
            items = await query.ToListAsync();
            countProvider = items as ITotalCountProvider;
            Assert.AreEqual(3, items.Count);
            Assert.IsNotNull(countProvider);
            Assert.AreEqual(3L, countProvider.TotalCount);

            sequence = await query.ToEnumerableAsync();
            countProvider = sequence as ITotalCountProvider;
            Assert.IsNotNull(countProvider);
            Assert.AreEqual(3L, countProvider.TotalCount);

            // Verify that IncludeTotalCount is correctly propagated with
            // projections
            List<string> titles = await query.Select(t => t.Title).ToListAsync();
            countProvider = titles as ITotalCountProvider;
            Assert.AreEqual(3, titles.Count);
            Assert.IsNotNull(countProvider);
            Assert.AreEqual(3L, countProvider.TotalCount);
        }

        [AsyncTestMethod]
        [Tag("TotalCount")]
        public async Task TotalCountWithTooManyElements()
        {
            // Insert a few records
            IMobileServiceTable<ToDo> table = GetClient().GetTable<ToDo>();

            ToDo first = new ToDo { Title = "TotalCount1", Complete = false };
            await table.InsertAsync(first);

            long totalCount = 65L;
            for (int i = 2; i <= totalCount; i++)
            {
                await table.InsertAsync(new ToDo { Title = "TotalCount" + i, Complete = true });
            }

            // Get the total count and make sure we've got more than just 50
            // items in the total count
            List<ToDo> items = await table.Where(t => t.Id >= first.Id).IncludeTotalCount().ToListAsync();
            ITotalCountProvider countProvider = items as ITotalCountProvider;
            Assert.AreEqual(50, items.Count);
            Assert.IsNotNull(countProvider);
            Assert.AreEqual(totalCount, countProvider.TotalCount);
        }
    }
}
