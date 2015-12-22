// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("unit")]
    [Tag("push")]
    [Tag("notNetFramework")]
    public class LocalStorageManagerTest : TestBase
    {
        const string DefaultChannelUri = "http://channelUri.com/a b";
        const string TestLocalStore = "TestLocalStore";
        const string TestRegistrationName = "TestRegistratioName";
        const string DefaultRegistrationId = "7313155627197174428-6522078074300559092-1";

        [TestMethod]
        public void UpdateLocalStorageRegistrationByName()
        {
            LocalStorageManager localStorageManager = new LocalStorageManager(TestLocalStore);
            localStorageManager.UpdateRegistrationByName(TestRegistrationName, DefaultRegistrationId, DefaultChannelUri);
            StoredRegistrationEntry storedRegistrationEntry = localStorageManager.GetRegistration(TestRegistrationName);
            Assert.AreEqual(storedRegistrationEntry.RegistrationName, TestRegistrationName);
            Assert.AreEqual(storedRegistrationEntry.RegistrationId, DefaultRegistrationId);
            Assert.AreEqual(localStorageManager.PushHandle, DefaultChannelUri);
            localStorageManager.DeleteRegistrationByName(TestRegistrationName);
        }

        [TestMethod]
        public void UpdateLocalStorageRegistrationByName_RegistrationNameNull()
        {
            LocalStorageManager localStorageManager = new LocalStorageManager(TestLocalStore);
            localStorageManager.UpdateRegistrationByName(null, DefaultRegistrationId, DefaultChannelUri);
            StoredRegistrationEntry storedRegistrationEntry = localStorageManager.GetRegistration(Registration.NativeRegistrationName);
            Assert.AreEqual(storedRegistrationEntry.RegistrationName,Registration.NativeRegistrationName);
            Assert.AreEqual(storedRegistrationEntry.RegistrationId, DefaultRegistrationId);
            Assert.AreEqual(localStorageManager.PushHandle, DefaultChannelUri);
            localStorageManager.DeleteRegistrationByName(Registration.NativeRegistrationName);
        }

        [TestMethod]
        public void UpdateLocalStorageRegistrationById()
        {
            LocalStorageManager localStorageManager = new LocalStorageManager(TestLocalStore);
            localStorageManager.UpdateRegistrationByRegistrationId(DefaultRegistrationId,TestRegistrationName,DefaultChannelUri);
            StoredRegistrationEntry storedRegistrationEntry = localStorageManager.GetRegistration(TestRegistrationName);
            Assert.AreEqual(storedRegistrationEntry.RegistrationName, TestRegistrationName);
            Assert.AreEqual(storedRegistrationEntry.RegistrationId, DefaultRegistrationId);
            Assert.AreEqual(localStorageManager.PushHandle, DefaultChannelUri);
            localStorageManager.DeleteRegistrationByRegistrationId(DefaultRegistrationId);
        }

        [TestMethod]
        public void UpdateLocalStorageRegistrationById_RegistrationNameNull()
        {
            LocalStorageManager localStorageManager = new LocalStorageManager(TestLocalStore);
            localStorageManager.UpdateRegistrationByRegistrationId(DefaultRegistrationId, null, DefaultChannelUri);
            StoredRegistrationEntry storedRegistrationEntry = localStorageManager.GetRegistration(Registration.NativeRegistrationName);
            Assert.AreEqual(storedRegistrationEntry.RegistrationName, Registration.NativeRegistrationName);
            Assert.AreEqual(storedRegistrationEntry.RegistrationId, DefaultRegistrationId);
            Assert.AreEqual(localStorageManager.PushHandle, DefaultChannelUri);
            localStorageManager.DeleteRegistrationByRegistrationId(DefaultRegistrationId);
        }
    }
}