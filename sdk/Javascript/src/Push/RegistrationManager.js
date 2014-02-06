// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="..\Generated\MobileServices.DevIntellisense.js" />

// Declare JSHint globals
/*global WinJS:false, Windows:false, $__fileVersion__:false, $__version__:false */

exports.RegistrationManager = function (pushHttpClient, storageManager) {
    this.pushHttpClient = pushHttpClient;
    this.localStorageManager = storageManager;
};

RegistrationManager.prototype.register = function (registration) {
    var firstPromise;
    // if localStorage is empty or has different storage version, we need retrieve registrations and refresh local storage
    if (this.localStorageManager.IsRefreshNeeded) {
        var refreshChannelUri = this.localStorageManager.ChannelUri || registration.ChannelUri;
        firstPromise = this.getRegistrationsForChannel(refreshChannelUri)
            .then(function () {
                this.localStorageManager.RefreshFinished(refreshChannelUri);
            });

    } else {
        firstPromise = WinJS.Promise.wrap();
    }

    firstPromise.then(function () { this.localStorageManager.GetRegistration(registration.templateName || '$Default'); })
        .then(function (cached) {
            if (cached != null) {
                registration.RegistrationId = cached.RegistrationId;
                return WinJS.Promise.wrap();
            } else {
                return this.createRegistrationId(registration);
            }
        })
        .then(function () {
            return this.upsertRegistration(registration);
        })
        .then(function () {
            // dead complete function
            return WinJS.Promise.wrap(false);
        },
            function (error) {
                // if we get an RegistrationGoneException (410) from service, we will recreate registration id and will try to do upsert one more time.
                // The likely cause of this is an expired registration in local storage due to a long unused app.
                if (error.request.status === 410) {
                    return true;
                }

                throw error;
            })
        .then(function (retry) {
            if (retry) {
                return this.createRegistrationId(registration);
            }
        })
        .then(function (retry) {
            if (retry) {
                return this.upsertRegistration(registration);
            }
        });
};

RegistrationManager.prototype.getRegistrationsForChannel = function (channelUri) {
    return this.pushHttpClient.listRegistrations(channelUri)
        .then(function(registrations) {
            var count = registrations.Count;
            if (count == 0) {
                this.localStorageManager.clearRegistrations();
            }
            for (var i = 0; i < count; i++) {
                this.localStorageManager.updateRegistrationByRegistrationId(registrations[i]);
            }
        });
};

RegistrationManager.prototype.unregister = function (registrationName) {
    var cached = this.localStorageManager.getRegistration(registrationName);
    if (!cached) {
        return WinJS.Promise.wrap();
    }

    return this.pushHttpClient.unregister(cached.RegistrationId)
        .then(function() {
            this.localStorageManager.deleteRegistrationByName(registrationName);
        });
};

RegistrationManager.prototype.deleteRegistrationsForChannel = function (channelUri) {
    this.pushHttpClient.listRegistrations(channelUri)
        .then(function(registrations) {
            return WinJS.Promise.join(registrations.map(function(registration) {
                return this.pushHttpClient.unregister(registration.RegistrationId)
                    .then(function() {
                        this.localStorageManager.deleteRegistrationByRegistrationId(registration.RegistrationId);
                    });
            }));
        })
        .then(this.localStorageManager.clearRegistrations);
};

RegistrationManager.prototype.createRegistrationId = function (registration) {
    return this.pushHttpClient.createRegistrationId()
        .then(function(registrationId) {
            registration.RegistrationId = registrationId;
            this.localStorageManager.updateRegistrationByName(registration.templateName || '$Default', registration);
        });
};

RegistrationManager.prototype.upsertRegistration = function (registration) {
    return this.pushHttpClient.CreateOrUpdateRegistration(registration)
        .then(function() {
            this.localStorageManager.UpdateRegistrationByName(registration.templateName || '$Default', registration);
        });
};