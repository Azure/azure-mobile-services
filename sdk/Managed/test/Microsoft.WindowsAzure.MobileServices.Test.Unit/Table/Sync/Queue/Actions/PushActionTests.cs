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
    public class PushActionTests
    {
        private Mock<OperationQueue> opQueue;
        private Mock<IMobileServiceLocalStore> store;
        private PushAction action;
        private Mock<IMobileServiceSyncHandler> handler;
        BookmarkOperation bookmark;

        [TestInitialize]
        public void Initialize()
        {
            this.store = new Mock<IMobileServiceLocalStore>(MockBehavior.Strict);
            this.opQueue = new Mock<OperationQueue>(MockBehavior.Strict, this.store.Object);
            this.handler = new Mock<IMobileServiceSyncHandler>(MockBehavior.Strict);
            this.bookmark = new BookmarkOperation();
            this.action = new PushAction(this.opQueue.Object, this.store.Object, this.handler.Object, new MobileServiceJsonSerializerSettings(), CancellationToken.None, this.bookmark);
        }

        [TestMethod]
        public async Task PushAbort_AbortsThePush()
        {
            var op = new InsertOperation("abc", "abc") { Item = new JObject() }; // putting an item so it won't load it
            var peek = new Queue<MobileServiceTableOperation>(new[] { op, null });
            // picks up the operation
            this.opQueue.Setup(q => q.Peek()).Returns(() => peek.Dequeue());
            // executes the operation via handler
            this.handler.Setup(h => h.ExecuteTableOperationAsync(op)).Callback<IMobileServiceTableOperation>(o =>
            {
                o.AbortPush();
            });
            
            // loads sync errors
            string syncError = @"[]";
            this.store.Setup(s => s.ReadAsync(It.Is<MobileServiceTableQueryDescription>(q => q.TableName == LocalSystemTables.SyncErrors))).Returns(Task.FromResult(JToken.Parse(syncError)));
            // calls push complete
            this.handler.Setup(h => h.OnPushCompleteAsync(It.IsAny<MobileServicePushCompletionResult>())).Returns(Task.FromResult(0))
                        .Callback<MobileServicePushCompletionResult>(result =>
                        {
                            Assert.AreEqual(result.Status, MobileServicePushStatus.CancelledByOperation);
                            Assert.AreEqual(result.Errors.Count(), 0);
                        });
            
            // deletes the errors
            this.store.Setup(s => s.DeleteAsync(It.Is<MobileServiceTableQueryDescription>(q => q.TableName == LocalSystemTables.SyncErrors))).Returns(Task.FromResult(0));

            await this.action.ExecuteAsync();

            this.store.VerifyAll();
            this.opQueue.VerifyAll();
            this.handler.VerifyAll();

            var ex = await AssertEx.Throws<MobileServicePushFailedException>(async () => await action.CompletionTask);
            Assert.AreEqual(ex.PushResult.Status, MobileServicePushStatus.CancelledByOperation);
            Assert.AreEqual(ex.PushResult.Errors.Count(), 0);
            Assert.IsNull(op.Result, "Result should be reset");
        }

        [TestMethod]
        public async Task ExecuteAsync_DeletesTheErrors()
        {
            var op = new InsertOperation("abc", "abc") { Item = new JObject() }; // putting an item so it won't load it
            var peek = new Queue<MobileServiceTableOperation>(new[]{ op, null });
            // picks up the operation
            this.opQueue.Setup(q => q.Peek()).Returns(() => peek.Dequeue());            
            // executes the operation via handler
            this.handler.Setup(h => h.ExecuteTableOperationAsync(op)).Returns(Task.FromResult(0));
            // removes the operation from queue
            this.opQueue.Setup(q => q.DequeueAsync()).Returns(Task.FromResult<MobileServiceTableOperation>(null)); 
            // loads sync errors
            string syncError = @"[]";
            this.store.Setup(s => s.ReadAsync(It.Is<MobileServiceTableQueryDescription>(q => q.TableName == LocalSystemTables.SyncErrors))).Returns(Task.FromResult(JToken.Parse(syncError)));
            // calls push complete
            this.handler.Setup(h => h.OnPushCompleteAsync(It.IsAny<MobileServicePushCompletionResult>())).Returns(Task.FromResult(0))
                        .Callback<MobileServicePushCompletionResult>(result =>
                        {
                            Assert.AreEqual(result.Status, MobileServicePushStatus.Complete);
                            Assert.AreEqual(result.Errors.Count(), 0);
                        });
            // deletes the errors
            this.store.Setup(s => s.DeleteAsync(It.Is<MobileServiceTableQueryDescription>(q => q.TableName == LocalSystemTables.SyncErrors))).Returns(Task.FromResult(0));

            await this.action.ExecuteAsync();

            this.store.VerifyAll();
            this.opQueue.VerifyAll();
            this.handler.VerifyAll();

            await action.CompletionTask;
        }
    }
}
