// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("user")]
    [Tag("unit")]
    public class ZumoUserTests : TestBase
    {
        [TestMethod]
        public void CreateUser()
        {
            string id = "qwrdsjjjd8";
            MobileServiceUser user = new MobileServiceUser(id);
            Assert.AreEqual(id, user.UserId);

            new MobileServiceUser(null);
            new MobileServiceUser("");
        }
    }
}
