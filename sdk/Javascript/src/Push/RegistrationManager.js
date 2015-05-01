// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="..\Generated\MobileServices.DevIntellisense.js" />

// Declare JSHint globals
/*global WinJS:false */

var _ = require('Extensions'),
    Validate = require('Validate'),
    Platform = require('Platform'),
    LocalStorageManager = require('LocalStorageManager').LocalStorageManager,
    PushHttpClient = require('PushHttpClient').PushHttpClient;

function RegistrationManager(mobileServicesClient, platform, storageKey) {
    Validate.notNull(mobileServicesClient, 'mobileServicesClient');

    this._platform = platform || 'wns';
    this._pushHttpClient = new PushHttpClient(mobileServicesClient);
    this._storageManager = new LocalStorageManager(storageKey || mobileServicesClient.applicationUrl);
}

exports.RegistrationManager = RegistrationManager;

RegistrationManager.NativeRegistrationName = LocalStorageManager.NativeRegistrationName;

RegistrationManager.prototype.upsertRegistration = Platform.async(
    function (registration, finalCallback) {
        Validate.notNull(registration, 'registration');
        Validate.notNull(finalCallback, 'callback');

        var self = this,
            expiredRegistration = function (callback) {
                createRegistration(function (error) {
                    if (error) {
                        callback(error);
                        return;
                    }

                    upsertRegistration(false, callback);
                });
            },
            upsertRegistration = function (retry, callback) {
                self._pushHttpClient.upsertRegistration(registration, function (error) {
                    if (retry && error && error.request && error.request.status === 410) {
                        expiredRegistration(callback);
                        return;
                    } else if (!error) {
                        self._storageManager.pushHandle = registration.deviceId;                        
                    }

                    callback(error);
                });
            },
            createRegistration = function (callback) {
                self._pushHttpClient.createRegistrationId(function (error, registrationId) {
                    if (error) {
                        callback(error);
                        return;
                    }

                    registration.registrationId = registrationId;

                    self._storageManager.updateRegistrationWithName(
                        registration.templateName || LocalStorageManager.NativeRegistrationName,
                        registration.registrationId,
                        registration.deviceId);

                    callback();
                });
            },
            firstRegistration = function (callback) {
                var name = registration.templateName || LocalStorageManager.NativeRegistrationName,
                    cachedRegistrationId = self._storageManager.getRegistrationIdWithName(name);

                if (!_.isNullOrEmpty(cachedRegistrationId)) {
                    registration.registrationId = cachedRegistrationId;
                    upsertRegistration(true, callback);
                } else {                
                    createRegistration(function (error) {
                        if (error) {
                            callback(error);
                            return;
                        }
                        upsertRegistration(true, callback);
                    });
                }
            };

        if (this._storageManager.isRefreshNeeded) {
            // We want the existing handle to win (if present), and slowly update them to the new handle
            // So use cached value over the passed in value
            this._refreshRegistrations(this._storageManager.pushHandle || registration.deviceId, function (error) {
                if (error) {
                    finalCallback(error);
                    return;
                }

                firstRegistration(finalCallback);
            });        
        } else {
            firstRegistration(finalCallback);
        }
    });

RegistrationManager.prototype.deleteRegistrationWithName = Platform.async(
    function (registrationName, callback) {
        var cachedRegistrationId = this._storageManager.getRegistrationIdWithName(registrationName),
            self = this;

        if (_.isNullOrEmpty(cachedRegistrationId)) {
            callback();
            return;
        }

        this._pushHttpClient.unregister(cachedRegistrationId, function (error) {
            if (!error) {
                self._storageManager.deleteRegistrationWithName(registrationName);
            }
            callback(error);
        });
    });

RegistrationManager.prototype.deleteAllRegistrations = Platform.async(
    function (pushHandle, callback) {
        var self = this,
            currentHandle = this._storageManager.pushHandle,
            deleteRegistrations = function (error, deleteCallback) {
                if (error) {
                    deleteCallback(error);
                    return;
                }

                var registrationIds = self._storageManager.getRegistrationIds(),
                    remaining = registrationIds.length,
                    errors = [];

                if (remaining === 0) {
                    self._storageManager.deleteAllRegistrations();
                    deleteCallback();
                    return;
                }

                registrationIds.map(function (registrationId) {
                    self._pushHttpClient.unregister(registrationId, function (error) {
                        remaining--;

                        if (error) {
                            errors.push(error);
                        }

                        if (remaining <= 0) {
                            if (errors.length === 0) {
                                self._storageManager.deleteAllRegistrations();
                                deleteCallback();
                            } else {
                                deleteCallback(_.createError('Failed to delete registrations for ' + pushHandle));
                            }
                        }                
                    });
                });
            };

        Validate.notNull(pushHandle, 'pushHandle');

        // Try to refresh with the local storage copy first, then if different use the requested one
        this._refreshRegistrations(currentHandle || pushHandle, function (error) {
            if (_.isNullOrEmpty(currentHandle) || pushHandle === currentHandle) {
                deleteRegistrations(error, callback);
            } else {
                // Delete the current handle's registrations
                deleteRegistrations(error, function (error) {
                    // Now delete the current handle's registrations as well
                    // This requires the deleteAllRegistrations() call to clear the cached handle
                    self._refreshRegistrations(pushHandle, function (error) {
                        deleteRegistrations(error, callback);
                    });
                });
            }
        });
    });


RegistrationManager.prototype.listRegistrations = Platform.async(
    function (pushHandle, callback) {
        /// <summary>
        /// Retrives a list of all registrations from the server
        /// </summary>

        Validate.notNullOrEmpty(pushHandle);

        this._pushHttpClient.listRegistrations(pushHandle, this._platform, callback);
    });

RegistrationManager.prototype._refreshRegistrations = function (pushHandle, callback) {
    /// <summary>
    /// Reloads all registrations for the pushHandle passed in
    /// </summary>

    var self = this;

    Validate.notNull(pushHandle, 'pushHandle');
    Validate.notNull(callback, 'callback');

    this._pushHttpClient.listRegistrations(pushHandle, this._platform, function (error, registrations) {
        if (!error) {
            self._storageManager.updateAllRegistrations(registrations, pushHandle);
        }

        callback(error);
    });
};

