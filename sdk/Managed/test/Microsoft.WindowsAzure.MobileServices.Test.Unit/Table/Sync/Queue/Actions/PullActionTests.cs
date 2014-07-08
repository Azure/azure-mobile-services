// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private Mock<IMobileServiceSyncHandler> handler;
        private Mock<MobileServiceClient> client;
        private Mock<MobileServiceSyncContext> context;
        private Mock<MobileServiceTable> table;

        [TestInitialize]
        public void Initialize()
        {
            this.store = new Mock<IMobileServiceLocalStore>(MockBehavior.Strict);
            this.opQueue = new Mock<OperationQueue>(MockBehavior.Strict, this.store.Object);
            this.handler = new Mock<IMobileServiceSyncHandler>(MockBehavior.Strict);
            this.client = new Mock<MobileServiceClient>();
            this.client.Object.Serializer = new MobileServiceSerializer();
            this.context = new Mock<MobileServiceSyncContext>(this.client.Object);
            this.table = new Mock<MobileServiceTable>("test", this.client.Object);
        }

        [TestMethod]
        public async Task DoesNotUpsertAnObject_IfItDoesNotHaveAnId()
        {
            var query = new MobileServiceTableQueryDescription("test");
            var action = new PullAction(this.table.Object, this.context.Object, query, null, this.opQueue.Object, this.store.Object, CancellationToken.None);

            var itemWithId = new JObject(){{"id", "abc"}, {"text", "has id"}};
            var itemWithoutId = new JObject() { { "text", "no id" } };
            var result = new JArray(new[]{
                itemWithId,
                itemWithoutId
            });
            this.opQueue.Setup(q => q.LockTableAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult<IDisposable>(null));
            this.opQueue.Setup(q => q.CountPending(It.IsAny<string>())).Returns(Task.FromResult(0L));            
            this.table.Setup(t => t.ReadAsync(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>())).Returns(Task.FromResult<JToken>(result));
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
    }
}
