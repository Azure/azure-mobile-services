// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Linq;

using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("push")]
    public class PushFunctional : FunctionalTestBase
    {
        readonly IPushTestUtility pushTestUtility;

        public PushFunctional()
        {
            this.pushTestUtility = TestPlatform.Instance.PushTestUtility;
        }

        [TestMethod]
        public void InitialUnregisterAllAsync()
        {
            var channelUri = this.pushTestUtility.GetPushHandle();
            var push = this.GetClient().GetPush();
            push.UnregisterAllAsync(channelUri).Wait();
            var registrations = push.ListRegistrationsAsync(channelUri).Result;
            Assert.IsFalse(registrations.Any(), "Deleting all registrations for a channel should ensure no registrations are returned by List");

            channelUri = this.pushTestUtility.GetUpdatedPushHandle();
            push.UnregisterAllAsync(channelUri).Wait();
            registrations = push.ListRegistrationsAsync(channelUri).Result;
            Assert.IsFalse(registrations.Any(), "Deleting all registrations for a channel should ensure no registrations are returned by List");
        }

        [TestMethod]
        public void RegisterNativeAsyncUnregisterNativeAsync()
        {
            var channelUri = this.pushTestUtility.GetPushHandle();
            var push = this.GetClient().GetPush();
            push.RegisterNativeAsync(channelUri).Wait();
            var registrations = push.ListRegistrationsAsync(channelUri).Result;
            Assert.AreEqual(registrations.Count(), 1, "1 registration should exist after RegisterNativeAsync");
            Assert.AreEqual(
                registrations.First().RegistrationId,
                push.RegistrationManager.LocalStorageManager.GetRegistration(Registration.NativeRegistrationName).RegistrationId,
                "Local storage should have the same RegistrationId as the one returned from service");

            push.UnregisterNativeAsync().Wait();
            registrations = push.ListRegistrationsAsync(channelUri).Result;
            Assert.AreEqual(registrations.Count(), 0, "0 registrations should exist in service after UnregisterNativeAsync");
            Assert.IsNull(
                push.RegistrationManager.LocalStorageManager.GetRegistration(Registration.NativeRegistrationName),
                "Local storage should not contain a native registration after UnregisterNativeAsync.");
        }

        [TestMethod]
        public void RegisterAsyncUnregisterTemplateAsync()
        {
            var mobileClient = this.GetClient();
            var push = mobileClient.GetPush();
            var template = this.pushTestUtility.GetTemplateRegistrationForToast();
            this.pushTestUtility.ValidateTemplateRegistrationBeforeRegister(template);
            push.RegisterAsync(template).Wait();
            var registrations = push.ListRegistrationsAsync(template.PushHandle).Result;
            Assert.AreEqual(registrations.Count(), 1, "1 registration should exist after RegisterNativeAsync");
            var registrationAfter = registrations.First();
            Assert.IsNotNull(registrationAfter, "List and Deserialization of a TemplateRegistration after successful registration should have a value.");
            Assert.AreEqual(
                registrationAfter.RegistrationId,
                push.RegistrationManager.LocalStorageManager.GetRegistration(template.Name).RegistrationId,
                "Local storage should have the same RegistrationId as the one returned from service");

            this.pushTestUtility.ValidateTemplateRegistrationAfterRegister(registrationAfter, mobileClient.applicationInstallationId);

            push.UnregisterTemplateAsync(template.Name).Wait();
            registrations = push.ListRegistrationsAsync(template.PushHandle).Result;
            Assert.AreEqual(registrations.Count(), 0, "0 registrations should exist in service after UnregisterTemplateAsync");
            Assert.IsNull(
                push.RegistrationManager.LocalStorageManager.GetRegistration(template.Name),
                "Local storage should not contain a native registration after UnregisterTemplateAsync.");
        }

        [TestMethod]
        public void RegisterRefreshRegisterWithUpdatedChannel()
        {
            var mobileClient = this.GetClient();
            var push = mobileClient.GetPush();
            var template = this.pushTestUtility.GetTemplateRegistrationForToast();
            this.pushTestUtility.ValidateTemplateRegistrationBeforeRegister(template);
            push.RegisterAsync((Registration)template).Wait();
            var registrations = push.ListRegistrationsAsync(template.PushHandle).Result;
            Assert.AreEqual(registrations.Count(), 1, "1 registration should exist after RegisterNativeAsync");
            var registrationAfter = registrations.First();
            Assert.IsNotNull(registrationAfter, "List and Deserialization of a TemplateRegistration after successful registration should have a value.");
            Assert.AreEqual(
                registrationAfter.RegistrationId,
                push.RegistrationManager.LocalStorageManager.GetRegistration(template.Name).RegistrationId,
                "Local storage should have the same RegistrationId as the one returned from service");

            this.pushTestUtility.ValidateTemplateRegistrationAfterRegister(registrationAfter, mobileClient.applicationInstallationId);
            push.RegistrationManager.LocalStorageManager.IsRefreshNeeded = true;
            template.PushHandle = this.pushTestUtility.GetUpdatedPushHandle();

            push.RegisterAsync(template).Wait();
            registrations = push.ListRegistrationsAsync(template.PushHandle).Result;
            Assert.AreEqual(registrations.Count(), 1, "1 registration should exist after RegisterNativeAsync");
            var registrationAfterUpdate = registrations.First();
            Assert.IsNotNull(registrationAfterUpdate, "List and Deserialization of a TemplateRegistration after successful registration should have a value.");
            Assert.AreEqual(
                registrationAfterUpdate.RegistrationId,
                push.RegistrationManager.LocalStorageManager.GetRegistration(template.Name).RegistrationId,
                "Local storage should have the same RegistrationId as the one returned from service");
            Assert.AreEqual(registrationAfter.RegistrationId, registrationAfterUpdate.RegistrationId, "Expected the same RegistrationId to be used even after the refresh");
            Assert.AreEqual(registrationAfterUpdate.PushHandle, template.PushHandle, "Expected updated channelUri after 2nd register");
            Assert.AreEqual(push.RegistrationManager.LocalStorageManager.PushHandle, template.PushHandle, "Expected local storage to be updaed to the new channelUri after 2nd register");

            Assert.AreEqual(push.ListRegistrationsAsync(registrationAfter.PushHandle).Result.Count(), 0, "Original channel should be gone from service");

            push.UnregisterTemplateAsync(template.Name).Wait();
            registrations = push.ListRegistrationsAsync(template.PushHandle).Result;
            Assert.AreEqual(registrations.Count(), 0, "0 registrations should exist in service after UnregisterTemplateAsync");
            Assert.IsNull(
                push.RegistrationManager.LocalStorageManager.GetRegistration(template.Name),
                "Local storage should not contain a native registration after UnregisterTemplateAsync.");
        }
    }
}