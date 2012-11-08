// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.Azure.Zumo.WindowsPhone8.Test;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Zumo.WindowsPhone8.CSharp.Test
{
    // Repurpose the random empty table
    [DataTable(Name = "test_table")]
    public class ToDoV2 : ICustomMobileServiceTableSerialization
    {
        public long Id { get; set; }

        [DataMember(Name = "col1")]
        public string Title { get; set; }

        [IgnoreDataMember]
        public string Tag { get; set; }

        [DataMember(Name = "col5")]
        public bool Complete { get; set; }

        public JToken Serialize()
        {
            JContainer obj = new JObject().Set("col1", Tag + ": " + Title);
            if (Id != 0)
            {
                obj.Set("id", (int)Id);
            }
            return obj;
        }

        public void Deserialize(JToken value)
        {
            // Get the ID and Age properties
            MobileServiceTableSerializer.Deserialize(value, this, true);

            if (Title != null)
            {
                string[] parts = Title.Split(':');
                if (parts.Length == 2)
                {
                    Tag = parts[0];
                    Title = parts[1].Trim();
                }
            }
        }
    }

    public class VersioningTest : FunctionalTestBase
    {
        [AsyncTestMethod]
        public async Task Version()
        {
            // Create the untyped collection
            MobileServiceClient client = GetClient();
            IMobileServiceTable<ToDo> table = client.GetTable<ToDo>();
            IMobileServiceTable<ToDoV2> tableV2 = client.GetTable<ToDoV2>();

            // Get some V1 data
            ToDo first = new ToDo { Title = "Foo" };
            await table.InsertAsync(first);

            // Get the data in V2 format
            ToDoV2 second = await tableV2.LookupAsync(first.Id);
            Assert.AreEqual(first.Title, second.Title);

            // Add a tag
            second.Tag = "Bar";
            await tableV2.UpdateAsync(second);
            await table.RefreshAsync(first);
            Assert.AreEqual("Bar: Foo", first.Title);

            // Chagne the tag
            first.Title = "Baz: Quux";
            await table.UpdateAsync(first);
            await tableV2.RefreshAsync(second);
            Assert.AreEqual("Baz", second.Tag);
            Assert.AreEqual("Quux", second.Title);
        }
    }
}
