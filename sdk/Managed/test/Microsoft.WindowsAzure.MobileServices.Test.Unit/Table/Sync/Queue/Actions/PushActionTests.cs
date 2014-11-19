// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
    public class PushActionTests
    {
        private Mock<OperationQueue> opQueue;
        private Mock<IMobileServiceLocalStore> store;
        private PushAction action;
        private Mock<IMobileServiceSyncHandler> handler;
        private Mock<MobileServiceClient> client;
        private Mock<MobileServiceSyncContext> context;

        [TestInitialize]
        public void Initialize()
        {
            this.store = new Mock<IMobileServiceLocalStore>(MockBehavior.Strict);
            this.opQueue = new Mock<OperationQueue>(MockBehavior.Strict, this.store.Object);
            this.opQueue.Setup(q => q.UpdateAsync(It.IsAny<MobileServiceTableOperation>())).Returns(Task.FromResult(0));
            this.handler = new Mock<IMobileServiceSyncHandler>(MockBehavior.Strict);
            this.client = new Mock<MobileServiceClient>();
            this.client.Object.Serializer = new MobileServiceSerializer();
            this.context = new Mock<MobileServiceSyncContext>(this.client.Object);
            this.context.Setup(c => c.GetTable(It.IsAny<string>())).Returns(Task.FromResult(new MobileServiceTable("test", this.client.Object)));
            this.action = new PushAction(this.opQueue.Object, this.store.Object, MobileServiceTableKind.Table, null, this.handler.Object, this.client.Object, this.context.Object, CancellationToken.None);
        }

        [TestMethod]
        public async Task AbortPush_AbortsThePush()
        {
            MobileServiceTableOperation op = new InsertOperation("abc", MobileServiceTableKind.Table, "abc") { Item = new JObject() }; // putting an item so it won't load it
            // picks up the operation
            this.opQueue.Setup(q => q.PeekAsync(0, MobileServiceTableKind.Table, It.IsAny<IEnumerable<string>>())).Returns(() => Task.FromResult(op));
            // executes the operation via handler
            this.handler.Setup(h => h.ExecuteTableOperationAsync(op))
                        .Callback<IMobileServiceTableOperation>(o =>
                        {
                            o.AbortPush();
                        });

            // loads sync errors
            string syncError = @"[]";
            this.store.Setup(s => s.ReadAsync(It.Is<MobileServiceTableQueryDescription>(q => q.TableName == MobileServiceLocalSystemTables.SyncErrors))).Returns(Task.FromResult(JToken.Parse(syncError)));
            // calls push complete
            this.handler.Setup(h => h.OnPushCompleteAsync(It.IsAny<MobileServicePushCompletionResult>())).Returns(Task.FromResult(0))
                        .Callback<MobileServicePushCompletionResult>(result =>
                        {
                            Assert.AreEqual(result.Status, MobileServicePushStatus.CancelledByOperation);
                            Assert.AreEqual(result.Errors.Count(), 0);
                        });

            // deletes the errors
            this.store.Setup(s => s.DeleteAsync(It.Is<MobileServiceTableQueryDescription>(q => q.TableName == MobileServiceLocalSystemTables.SyncErrors))).Returns(Task.FromResult(0));

            await this.action.ExecuteAsync();

            this.store.VerifyAll();
            this.opQueue.VerifyAll();
            this.handler.VerifyAll();

            var ex = await AssertEx.Throws<MobileServicePushFailedException>(async () => await action.CompletionTask);
            Assert.AreEqual(ex.PushResult.Status, MobileServicePushStatus.CancelledByOperation);
            Assert.AreEqual(ex.PushResult.Errors.Count(), 0);
        }

        [TestMethod]
        public async Task ExecuteAsync_DeletesTheErrors()
        {
            var op = new InsertOperation("table", MobileServiceTableKind.Table, "id") { Item = new JObject() }; // putting an item so it won't load it
            await this.TestExecuteAsync(op, null, null);
        }

        [TestMethod]
        public async Task ExecuteAsync_LoadsTheItem_IfItIsNotPresent()
        {
            var op = new InsertOperation("table", MobileServiceTableKind.Table, "id");
            this.store.Setup(s => s.LookupAsync("table", "id")).Returns(Task.FromResult(new JObject()));
            await this.TestExecuteAsync(op, null, null);
        }

        [TestMethod]
        public async Task ExecuteAsync_SavesTheResult_IfExecuteTableOperationDoesNotThrow()
        {
            var op = new InsertOperation("table", MobileServiceTableKind.Table, "id") { Item = new JObject() };
            await TestResultSave(op, status: null, resultId: "id", saved: true);
        }

        [TestMethod]
        public async Task ExecuteAsync_DoesNotSaveTheResult_IfOperationDoesNotWriteToStore()
        {
            var op = new DeleteOperation("table", MobileServiceTableKind.Table, "id") { Item = new JObject() };
            Assert.IsFalse(op.CanWriteResultToStore);
            await TestResultSave(op, status: null, resultId: "id", saved: false);
        }

        [TestMethod]
        public async Task ExecuteAsync_DoesNotSaveTheResult_IfExecuteTableOperationThrows()
        {
            this.store.Setup(s => s.UpsertAsync(MobileServiceLocalSystemTables.SyncErrors, It.IsAny<JObject[]>(), false)).Returns(Task.FromResult(0));
            var op = new InsertOperation("table", MobileServiceTableKind.Table, "id") { Item = new JObject() };
            await TestResultSave(op, status: HttpStatusCode.PreconditionFailed, resultId: "id", saved: false);
        }

        [TestMethod]
        public async Task ExecuteAsync_DoesNotSaveTheResult_IfPresentButResultDoesNotHaveId()
        {
            this.action = new PushAction(this.opQueue.Object, this.store.Object, MobileServiceTableKind.Table, null, this.handler.Object, this.client.Object, this.context.Object, CancellationToken.None);
            var op = new InsertOperation("table", MobileServiceTableKind.Table, "id") { Item = new JObject() };
            await TestResultSave(op, status: null, resultId: null, saved: false);
        }

        private async Task TestResultSave(MobileServiceTableOperation op, HttpStatusCode? status, string resultId, bool saved)
        {
            var result = new JObject() { { "id", resultId } };
            if (saved)
            {
                this.store.Setup(s => s.UpsertAsync("table", It.Is<JObject[]>(list => list.Any(o => o.ToString() == result.ToString())), true))
                          .Returns(Task.FromResult(0));
            }
            await this.TestExecuteAsync(op, result, status);
        }

        private async Task TestExecuteAsync(MobileServiceTableOperation op, JObject result, HttpStatusCode? errorCode)
        {
            op.Sequence = 1;

            // picks up the operation
            this.opQueue.Setup(q => q.PeekAsync(0, MobileServiceTableKind.Table, It.IsAny<IEnumerable<string>>())).Returns(() => Task.FromResult(op));
            this.opQueue.Setup(q => q.PeekAsync(op.Sequence, MobileServiceTableKind.Table, It.IsAny<IEnumerable<string>>())).Returns(() => Task.FromResult<MobileServiceTableOperation>(null));

            // executes the operation via handler
            if (errorCode == null)
            {
                this.handler.Setup(h => h.ExecuteTableOperationAsync(op)).Returns(Task.FromResult(result));
            }
            else
            {
                this.handler.Setup(h => h.ExecuteTableOperationAsync(op))
                            .Throws(new MobileServiceInvalidOperationException("",
                                                                               null,
                                                                               new HttpResponseMessage(errorCode.Value)
                                                                               {
                                                                                   Content = new StringContent(result.ToString())
                                                                               }));
            }
            // removes the operation from queue only if there is no error
            if (errorCode == null)
            {
                this.opQueue.Setup(q => q.DeleteAsync(It.IsAny<string>(), It.IsAny<long>())).Returns(Task.FromResult(true));
            }
            // loads sync errors
            string syncError = @"[]";
            this.store.Setup(s => s.ReadAsync(It.Is<MobileServiceTableQueryDescription>(q => q.TableName == MobileServiceLocalSystemTables.SyncErrors))).Returns(Task.FromResult(JToken.Parse(syncError)));
            // calls push complete
            this.handler.Setup(h => h.OnPushCompleteAsync(It.IsAny<MobileServicePushCompletionResult>())).Returns(Task.FromResult(0))
                        .Callback<MobileServicePushCompletionResult>(r =>
                        {
                            Assert.AreEqual(r.Status, MobileServicePushStatus.Complete);
                            Assert.AreEqual(r.Errors.Count(), 0);
                        });
            // deletes the errors
            this.store.Setup(s => s.DeleteAsync(It.Is<MobileServiceTableQueryDescription>(q => q.TableName == MobileServiceLocalSystemTables.SyncErrors))).Returns(Task.FromResult(0));

            await this.action.ExecuteAsync();

            this.store.VerifyAll();
            this.opQueue.VerifyAll();
            this.handler.VerifyAll();

            await action.CompletionTask;
        }
    }
}
