// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path='C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js' />
/// <reference path='C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js' />
/// <reference path='..\Generated\MobileServices.DevIntellisense.js' />

// Declare JSHint globals
/*global WinJS:false, Windows:false */

var _ = require('Extensions'),
    Validate = require('Validate'),
    Platform = require('Platform'),
    Constants = {
        Version: 'v1.1.0',
        Keys: {
            Version: 'Version',
            PushHandle: 'ChannelUri',
            Registrations: 'Registrations',
            NativeRegistration: '$Default'
        },
    };

function LocalStorageManager(storageKey) {    
    this._registrations = {};
    this._storageKey = 'MobileServices.Push.' + storageKey;

    this._isRefreshNeeded = false;
    Object.defineProperty(this, 'isRefreshNeeded', {
        get: function () {
            /// <summary>
            /// Gets a value indicating whether local storage data needs to be refreshed.
            /// </summary>
            return this._isRefreshNeeded;
        }
    });

    this._pushHandle = null;
    Object.defineProperty(this, 'pushHandle', {
        get: function () {
            /// <summary>
            /// Gets the DeviceId of all registrations in the LocalStorageManager
            /// </summary>  
            return _.isNull(this._pushHandle) ? '' : this._pushHandle;
        },
        set: function (value) {
            Validate.notNullOrEmpty(value, 'pushHandle');

            if (this._pushHandle !== value) {
                this._pushHandle = value;
                this._flushToSettings();
            }
        }
    });

    // Initialize our state
    this._initializeRegistrationInfoFromStorage();
}

exports.LocalStorageManager = LocalStorageManager;

LocalStorageManager.NativeRegistrationName = Constants.Keys.NativeRegistration;

LocalStorageManager.prototype.getRegistrationIds = function () {
    /// <summary>
    /// Gets an array of all registration Ids
    /// </summary>
    /// <returns>
    /// An array of registration Ids in form of ['1','2','3']
    /// </returns>
    var result = [];
    for (var name in this._registrations) {
        if (this._registrations.hasOwnProperty(name)) {
            result.push(this._registrations[name]);
        }
    }
    return result;
};

LocalStorageManager.prototype.getRegistrationIdWithName = function (registrationName) {
    /// <summary>
    /// Get the registration Id from local storage
    /// </summary>
    /// <param name="registrationName">
    /// The name of the registration mapping to search for
    /// </param>
    /// <returns>
    /// The registration Id if it exists or null if it does not.
    /// </returns>

    Validate.notNullOrEmpty(registrationName, 'registrationName');

    return this._registrations[registrationName];
};

LocalStorageManager.prototype.updateAllRegistrations = function (registrations, pushHandle) {
    /// <summary>
    /// Replace all registrations and the pushHandle with those passed in.
    /// </summary>
    /// <param name="registrations">
    /// An array of registrations to update.
    /// </param>
    /// <param name="pushHandle">
    /// The pushHandle to update.
    /// </param>

    Validate.notNull(pushHandle, 'pushHandle');
    if (!registrations) {
        registrations = [];
    }
    this._registrations = {};

    for (var i = 0; i < registrations.length; i++) {
        var name = registrations[i].templateName;
        if (_.isNullOrEmpty(name)) {
            name = Constants.Keys.NativeRegistration;
        }
        
        /// All registrations passed to this method will have registrationId as they
        /// come directly from notification hub where registrationId is the key field.
        this._registrations[name] = registrations[i].registrationId;
    }

    // Need to flush explictly as handle may not have changed
    this._pushHandle = pushHandle;
    this._flushToSettings();
    this._isRefreshNeeded = false;
};

LocalStorageManager.prototype.updateRegistrationWithName = function (registrationName, registrationId, pushHandle) {
    /// <summary>
    /// Update a registration mapping and the deviceId in local storage by registrationName
    /// </summary>
    /// <param name="registrationName">
    /// The name of the registration mapping to update.
    /// </param>
    /// <param name="registrationId">
    /// The registrationId to update.
    /// </param>
    /// <param name="registrationDeviceId">
    /// The device Id to update the ILocalStorageManager to.
    /// </param>

    Validate.notNullOrEmpty(registrationName, 'registrationName');
    Validate.notNullOrEmpty(registrationId, 'registrationId');
    Validate.notNullOrEmpty(pushHandle, 'pushHandle');

    // TODO: We could check if the Id or Name has actually changed
    this._registrations[registrationName] = registrationId;

    this._pushHandle = pushHandle;
    this._flushToSettings();
};

LocalStorageManager.prototype.deleteRegistrationWithName = function (registrationName) {
    /// <summary>
    /// Delete a registration from local storage by name
    /// </summary>
    /// <param name="registrationName">
    /// The name of the registration mapping to delete.
    /// </param>

    Validate.notNullOrEmpty(registrationName, 'registrationName');

    if (this._registrations.hasOwnProperty(registrationName)) {
        delete this._registrations[registrationName];
        this._flushToSettings();
    }
};

LocalStorageManager.prototype.deleteAllRegistrations = function () {
    /// <summary>
    /// Clear all registrations from local storage.
    /// </summary>
    this._registrations = {};
    this._flushToSettings();
};

// Private methods

LocalStorageManager.prototype._flushToSettings = function () {
    /// <summary>
    /// Writes all registrations to storage
    /// </summary>

    var forStorage = {};
    forStorage[Constants.Keys.Version] = Constants.Version;
    forStorage[Constants.Keys.PushHandle] = this._pushHandle;
    forStorage[Constants.Keys.Registrations] = this._registrations;

    Platform.writeSetting(this._storageKey, JSON.stringify(forStorage));
};

LocalStorageManager.prototype._initializeRegistrationInfoFromStorage = function () {
    /// <summary>
    /// Populates registration information from storage
    /// </summary>

    this._registrations = {};

    try {
        // Read push handle
        var data = JSON.parse(Platform.readSetting(this._storageKey));

        this._pushHandle = data[Constants.Keys.PushHandle];
        if (!this._pushHandle) {
            this._isRefreshNeeded = true;
            return;
        }

        // Verify this.storage version
        var version = data[Constants.Keys.Version] || '';
        this._isRefreshNeeded = (Constants.Version !== version.toLowerCase());
        if (this._isRefreshNeeded) {
            return;
        }

        // read registrations
        this._registrations = data[Constants.Keys.Registrations];
        
    } catch (err) {
        // It is possible that local storage is corrupted by users, bugs or other issues.
        // If this occurs, force a full refresh.
        this._isRefreshNeeded = true;
    }
};