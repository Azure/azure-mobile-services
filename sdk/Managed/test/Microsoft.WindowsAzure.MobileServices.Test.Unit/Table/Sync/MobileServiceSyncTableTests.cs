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
            var testCases = new[] { "-myitems", "_myitems", "|myitems", "s|myitems", "asdf@#$!@" };
            foreach (var queryId in testCases)
            {
                var ex = AssertEx.Throws<ArgumentException>(() => MobileServiceSyncTable.ValidateQueryId(queryId));
                Assert.AreEqual(ex.Message, "The query id must start with a letter and can contain only letters, digits, hyphen or underscore.");
            }
        }

        [TestMethod]
        public void ValidateQueryId_Succeeds_OnValidId()
        {
            var testCases = new[] { "myitems1", "myItems_yourItems1", "my-items123" };
            foreach (var queryId in testCases)
            {
                MobileServiceSyncTable.ValidateQueryId(queryId);
            }
        }
    }
}
