// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="..\Generated\MobileServices.DevIntellisense.js" />

// Declare JSHint globals
/*global WinJS:false */

function RegistrationManager(pushHttpClient, storageManager) {
    this.pushHttpClient = pushHttpClient;
    this.localStorageManager = storageManager;
};

exports.RegistrationManager = RegistrationManager;

RegistrationManager.prototype.refreshLocalStorage = function(refreshChannelUri) {
    var refreshPromise;
    var self = this;
    // if localStorage is empty or has different storage version, we need retrieve registrations and refresh local storage
    if (this.localStorageManager.isRefreshNeeded) {
        refreshPromise = this.getRegistrationsForChannel(refreshChannelUri)
            .then(function() {
                self.localStorageManager.refreshFinished(refreshChannelUri);
            });

    } else {
        refreshPromise = WinJS.Promise.wrap();
    }

    return refreshPromise;
};

RegistrationManager.prototype.register = function (registration) {
    var self = this;
    return this.refreshLocalStorage(this.localStorageManager.channelUri || registration.deviceId)
        .then(function () {
            return self.localStorageManager.getRegistration(registration.templateName || '$Default');
        })
        .then(function (cached) {
            if (cached != null) {
                registration.registrationId = cached.registrationId;
                return WinJS.Promise.wrap();
            } else {
                return self.createRegistrationId(registration);
            }
        })
        .then(function () {
            return self.upsertRegistration(registration);
        })
        .then(
            function () {
                // dead complete function
                return WinJS.Promise.wrap();
            },
            function (error) {
                // if we get an RegistrationGoneException (410) from service, we will recreate registration id and will try to do upsert one more time.
                // The likely cause of this is an expired registration in local storage due to a long unused app.
                if (error.request.status === 410) {
                    return self.createRegistrationId(registration)
                        .then(function () {
                            return self.upsertRegistration(registration);
                        });
                }

                throw error;
            }
        );
};

RegistrationManager.prototype.getRegistrationsForChannel = function (channelUri) {
    var self = this;
    return this.pushHttpClient.listRegistrations(channelUri)
        .then(function (registrations) {
            var count = registrations.length;
            if (count === 0) {
                self.localStorageManager.clearRegistrations();
            }

            for (var i = 0; i < count; i++) {
                self.localStorageManager.updateRegistrationByRegistrationId(registrations[i].registrationId, registrations[i].registrationName || '$Default', channelUri);
            }
        });
};

RegistrationManager.prototype.unregister = function (registrationName) {
    var cached = this.localStorageManager.getRegistration(registrationName);
    if (!cached) {
        return WinJS.Promise.wrap();
    }

    var self = this;
    return this.pushHttpClient.unregister(cached.registrationId)
        .then(function () {
            self.localStorageManager.deleteRegistrationByName(registrationName);
        });
};

RegistrationManager.prototype.deleteRegistrationsForChannel = function (channelUri) {
    var self = this;
    return this.pushHttpClient.listRegistrations(channelUri)
        .then(function (registrations) {
            return WinJS.Promise.join(
                registrations.map(function (registration) {
                    return self.pushHttpClient.unregister(registration.registrationId)
                        .then(function () {
                            self.localStorageManager.deleteRegistrationByRegistrationId(registration.registrationId);
                        });
                }));
        })
        .then(function() {
            self.localStorageManager.clearRegistrations();
        });
};

RegistrationManager.prototype.createRegistrationId = function (registration) {
    var self = this;
    return this.pushHttpClient.createRegistrationId()
        .then(function (registrationId) {
            registration.registrationId = registrationId;
            self.localStorageManager.updateRegistrationByRegistrationName(registration.templateName || '$Default', registration.registrationId, registration.deviceId);
        });
};

RegistrationManager.prototype.upsertRegistration = function (registration) {
    var self = this;
    return this.pushHttpClient.createOrUpdateRegistration(registration)
        .then(function () {
            self.localStorageManager.updateRegistrationByRegistrationName(registration.templateName || '$Default', registration.registrationId, registration.deviceId);
        });
};