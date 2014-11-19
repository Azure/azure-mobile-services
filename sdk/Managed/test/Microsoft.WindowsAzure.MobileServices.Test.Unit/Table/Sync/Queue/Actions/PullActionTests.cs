// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MobileServices.Query;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Moq;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test.Unit.Table.Sync.Queue.Actions
{
    [TestClass]
    public class PullActionTests
    {
        private Mock<OperationQueue> opQueue;
        private Mock<IMobileServiceLocalStore> store;
        private Mock<MobileServiceSyncSettingsManager> settings;
        private Mock<IMobileServiceSyncHandler> handler;
        private Mock<MobileServiceClient> client;
        private Mock<MobileServiceSyncContext> context;
        private Mock<MobileServiceTable<ToDoWithSystemPropertiesType>> table;

        [TestInitialize]
        public void Initialize()
        {
            this.store = new Mock<IMobileServiceLocalStore>(MockBehavior.Strict);
            this.settings = new Mock<MobileServiceSyncSettingsManager>(MockBehavior.Strict);
            this.opQueue = new Mock<OperationQueue>(MockBehavior.Strict, this.store.Object);
            this.handler = new Mock<IMobileServiceSyncHandler>(MockBehavior.Strict);
            this.client = new Mock<MobileServiceClient>();
            this.client.Object.Serializer = new MobileServiceSerializer();
            this.context = new Mock<MobileServiceSyncContext>(this.client.Object);
            this.table = new Mock<MobileServiceTable<ToDoWithSystemPropertiesType>>("test", this.client.Object);
        }

        [TestMethod]
        public async Task DoesNotUpsertAnObject_IfItDoesNotHaveAnId()
        {
            var query = new MobileServiceTableQueryDescription("test");
            var action = new PullAction(this.table.Object, MobileServiceTableKind.Table, this.context.Object, null, query, null, null, this.opQueue.Object, this.settings.Object, this.store.Object, MobileServiceRemoteTableOptions.All, null, CancellationToken.None);

            var itemWithId = new JObject() { { "id", "abc" }, { "text", "has id" } };
            var itemWithoutId = new JObject() { { "text", "no id" } };
            var result = new JArray(new[]{
                itemWithId,
                itemWithoutId
            });
            this.opQueue.Setup(q => q.LockTableAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult<IDisposable>(null));
            this.opQueue.Setup(q => q.CountPending(It.IsAny<string>())).Returns(Task.FromResult(0L));
            this.table.SetupSequence(t => t.ReadAsync(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<MobileServiceFeatures>()))
                      .Returns(Task.FromResult(QueryResult.Parse(result, null, false)))
                      .Returns(Task.FromResult(QueryResult.Parse(new JArray(), null, false)));
            this.store.Setup(s => s.UpsertAsync("test", It.IsAny<IEnumerable<JObject>>(), true))
                      .Returns(Task.FromResult(0))
                      .Callback<string, IEnumerable<JObject>, bool>((tableName, items, fromServer) =>
                        {
                            Assert.AreEqual(1, items.Count());
                            Assert.AreEqual(itemWithId, items.First());
                        });

            await action.ExecuteAsync();

            store.VerifyAll();
            opQueue.VerifyAll();
            table.VerifyAll();

            store.Verify(s => s.DeleteAsync("test", It.IsAny<IEnumerable<string>>()), Times.Never(), "There shouldn't be any call to delete");
        }

        [TestMethod]
        public async Task SavesTheMaxUpdatedAt_IfQueryIdIsSpecified_WithoutFilter()
        {
            var query = new MobileServiceTableQueryDescription("test");

            var result = new JArray(new[]
            {
                new JObject() { { "id", "abc" }, { "text", "has id"}, { "__updatedAt", "1985-07-17" } },
                new JObject() { { "id", "abc" }, { "text", "has id"}, { "__updatedAt", "2014-07-09" } }
            });
            string firstQuery = "$filter=(__updatedAt ge datetimeoffset'2013-01-01T00%3A00%3A00.0000000%2B00%3A00')&$orderby=__updatedAt&$skip=0&$top=50";
            string secondQuery = "$filter=(__updatedAt ge datetimeoffset'2014-07-09T07%3A00%3A00.0000000%2B00%3A00')&$orderby=__updatedAt&$skip=0&$top=50";
            await TestIncrementalSync(query, result, new DateTime(2014, 07, 09), savesMax: true, firstQuery: firstQuery, secondQuery: secondQuery);
        }

        [TestMethod]
        public async Task SavesTheMaxUpdatedAt_IfQueryIdIsSpecified()
        {
            var query = new MobileServiceTableQueryDescription("test");
            query.Filter = new BinaryOperatorNode(BinaryOperatorKind.Equal, new ConstantNode(4), new ConstantNode(3));
            query.Ordering.Add(new OrderByNode(new MemberAccessNode(null, "text"), OrderByDirection.Descending));
            var result = new JArray(new[]
            {
                new JObject() { { "id", "abc" }, { "text", "has id"}, { "__updatedAt", "1985-07-17" } },
                new JObject() { { "id", "abc" }, { "text", "has id"}, { "__updatedAt", "2014-07-09" } }
            });
            string firstQuery = "$filter=((4 eq 3) and (__updatedAt ge datetimeoffset'2013-01-01T00%3A00%3A00.0000000%2B00%3A00'))&$orderby=__updatedAt&$skip=0&$top=50";
            string secondQuery = "$filter=((4 eq 3) and (__updatedAt ge datetimeoffset'2014-07-09T07%3A00%3A00.0000000%2B00%3A00'))&$orderby=__updatedAt&$skip=0&$top=50";
            await TestIncrementalSync(query, result, new DateTime(2014, 07, 09), savesMax: true, firstQuery: firstQuery, secondQuery: secondQuery);
        }

        [TestMethod]
        public async Task DoesNotSaveTheMaxUpdatedAt_IfThereAreNoResults()
        {
            var query = new MobileServiceTableQueryDescription("test");
            var result = new JArray();
            string expectedOdata = "$filter=(__updatedAt ge datetimeoffset'2013-01-01T00%3A00%3A00.0000000%2B00%3A00')&$orderby=__updatedAt&$skip=0&$top=50";
            await TestIncrementalSync(query, result, DateTime.MinValue, savesMax: false, firstQuery: expectedOdata, secondQuery: null);
        }

        [TestMethod]
        public async Task DoesNotSaveTheMaxUpdatedAt_IfResultsHaveOlderUpdatedAt()
        {
            var query = new MobileServiceTableQueryDescription("test");
            query.Filter = new BinaryOperatorNode(BinaryOperatorKind.Equal, new ConstantNode(4), new ConstantNode(3));
            var result = new JArray(new[]
            {
                new JObject() { { "id", "abc" }, { "text", "has id"}, { "__updatedAt", "1985-07-17" } },
            });
            string firstQuery = "$filter=((4 eq 3) and (__updatedAt ge datetimeoffset'2013-01-01T00%3A00%3A00.0000000%2B00%3A00'))&$orderby=__updatedAt&$skip=0&$top=50";
            string secondQuery = "$filter=((4 eq 3) and (__updatedAt ge datetimeoffset'2013-01-01T00%3A00%3A00.0000000%2B00%3A00'))&$orderby=__updatedAt&$skip=1&$top=50";
            await TestIncrementalSync(query, result, new DateTime(2014, 07, 09), savesMax: false, firstQuery: firstQuery, secondQuery: secondQuery);
        }

        [TestMethod]
        public async Task DoesNotSaveTheMaxUpdatedAt_IfResultsDoNotHaveUpdatedAt()
        {
            var query = new MobileServiceTableQueryDescription("test");
            query.Filter = new BinaryOperatorNode(BinaryOperatorKind.Equal, new ConstantNode(4), new ConstantNode(3));
            var result = new JArray(new[]
            {
                new JObject() { { "id", "abc" }, { "text", "has id"} },
                new JObject() { { "id", "abc" }, { "text", "has id"} }
            });
            string firstQuery = "$filter=((4 eq 3) and (__updatedAt ge datetimeoffset'2013-01-01T00%3A00%3A00.0000000%2B00%3A00'))&$orderby=__updatedAt&$skip=0&$top=50";
            string secondQuery = "$filter=((4 eq 3) and (__updatedAt ge datetimeoffset'2013-01-01T00%3A00%3A00.0000000%2B00%3A00'))&$orderby=__updatedAt&$skip=2&$top=50";
            await TestIncrementalSync(query, result, new DateTime(2014, 07, 09), savesMax: false, firstQuery: firstQuery, secondQuery: secondQuery);
        }

        private async Task TestIncrementalSync(MobileServiceTableQueryDescription query, JArray result, DateTime maxUpdatedAt, bool savesMax, string firstQuery, string secondQuery)
        {
            var action = new PullAction(this.table.Object, MobileServiceTableKind.Table, this.context.Object, "latestItems", query, null, null, this.opQueue.Object, this.settings.Object, this.store.Object, MobileServiceRemoteTableOptions.All, null, CancellationToken.None);

            this.opQueue.Setup(q => q.LockTableAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult<IDisposable>(null));
            this.opQueue.Setup(q => q.CountPending(It.IsAny<string>())).Returns(Task.FromResult(0L));
            this.table.Setup(t => t.ReadAsync(firstQuery, It.IsAny<IDictionary<string, string>>(), It.IsAny<MobileServiceFeatures>()))
                      .Returns(Task.FromResult(QueryResult.Parse(result, null, false)));

            if (result.Any())
            {
                this.table.Setup(t => t.ReadAsync(secondQuery, It.IsAny<IDictionary<string, string>>(), It.IsAny<MobileServiceFeatures>()))
                          .Returns(Task.FromResult(QueryResult.Parse(new JArray(), null, false)));
            }

            if (result.Any())
            {
                this.store.Setup(s => s.UpsertAsync("test", It.IsAny<IEnumerable<JObject>>(), true)).Returns(Task.FromResult(0));
            }

            this.settings.Setup(s => s.GetDeltaTokenAsync("test", "latestItems")).Returns(Task.FromResult(new DateTimeOffset(2013, 1, 1, 0, 0, 0, TimeSpan.Zero)));
            if (savesMax)
            {
                this.settings.Setup(s => s.SetDeltaTokenAsync("test", "latestItems", maxUpdatedAt)).Returns(Task.FromResult(0));
            }

            await action.ExecuteAsync();

            this.store.VerifyAll();
            this.opQueue.VerifyAll();
            this.table.VerifyAll();
            this.settings.VerifyAll();

            store.Verify(s => s.DeleteAsync("test", It.IsAny<IEnumerable<string>>()), Times.Never(), "There shouldn't be any call to delete");
        }
    }
}
