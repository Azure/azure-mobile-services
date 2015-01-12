using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MobileServices.Sync;

namespace Microsoft.WindowsAzure.MobileServices.Test.Unit.Table.Sync
{
    [TestClass]
    public class MobileServiceSyncTableTests
    {
        [TestMethod]
        public void ValidateQueryId_Throws_OnInvalidId()
        {
            var testCases = new[] { "|myitems", "s|myitems", "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz" };
            foreach (var queryId in testCases)
            {
                var ex = AssertEx.Throws<ArgumentException>(() => MobileServiceSyncTable.ValidateQueryId(queryId));
                Assert.AreEqual(ex.Message, "The query id must not contain pipe character and should be less than 50 characters in length.");
            }
        }

        [TestMethod]
        public void ValidateQueryId_Succeeds_OnValidId()
        {
            var testCases = new[] { "myitems1", "myItems_yourItems1", "my-items123", "-myitems", "_myitems", "asdf@#$!/:^" };
            foreach (var queryId in testCases)
            {
                MobileServiceSyncTable.ValidateQueryId(queryId);
            }
        }
    }
}
