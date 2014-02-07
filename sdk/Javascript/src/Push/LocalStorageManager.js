// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="..\Generated\MobileServices.DevIntellisense.js" />

// Declare JSHint globals
/*global WinJS:false, Windows:false, $__fileVersion__:false, $__version__:false */

var _ = require('Extensions');
var Platform = require('Platform');

function LocalStorageManager(applicationUri, tileId) {
    if (!tileId) {
        tileId = '$Primary';
    }

    var name = _.format("{0}-PushContainer-{1}-{2}", Windows.ApplicationModel.Package.current.id.name, applicationUri, tileId);
    this.settings = Windows.Storage.ApplicationData.current.localSettings.createContainer(name, Windows.Storage.ApplicationDataCreateDisposition.always).values;
    this.isRefreshNeeded = false;
    this.channelUri = null;

    this.storageVersion = "v1.0.0";
    this.primaryChannelId = "$Primary";
    this.keyNameVersion = "Version";
    this.keyNameChannelUri = "ChannelUri";
    this.keyNameRegistrations = "Registrations";

    this.initializeRegistrationInfoFromStorage();
};

exports.LocalStorageManager = LocalStorageManager;

LocalStorageManager.prototype.getChannelUri = function () {
    return this.channelUri;
};

LocalStorageManager.prototype.setChannelUri = function (channelUri) {
    this.channelUri = channelUri;
    this.flushToSettings();
};

LocalStorageManager.prototype.getRegistration = function (registrationName) {
    return this.readRegistration(registrationName);
};

LocalStorageManager.prototype.deleteRegistrationByName = function (registrationName) {
    if (Platform.tryRemoveSetting(registrationName, this.registrations)) {
        this.flushToSettings();
        return true;
    }

    return false;
};

LocalStorageManager.prototype.deleteRegistrationByRegistrationId = function (registrationId) {
    var registration = this.getFirstRegistrationByRegistrationId(registrationId);

    if (registration) {
        this.deleteRegistrationByName(registration.registrationName);
        return true;
    }

    return false;
};

LocalStorageManager.prototype.getFirstRegistrationByRegistrationId = function (registrationId) {
    var returnValue = null;
    for (var regName in this.registrations) {
        if (this.registrations.hasOwnProperty(regName)) {
            // Update only the first registration with matching registrationId
            var registration = this.readRegistration(regName);
            if (!returnValue && registration && (registration.registrationId === registrationId)) {
                returnValue = registration;
            }
        }
    }

    return returnValue;
};

LocalStorageManager.prototype.updateRegistrationByRegistrationName = function (registrationName, registrationId, channelUri) {
    var cacheReg = {};
    cacheReg.registrationName = registrationName;
    cacheReg.registrationId = registrationId;
    this.writeRegistration(registrationName, cacheReg);
    this.channelUri = channelUri;
    this.flushToSettings();
};

LocalStorageManager.prototype.writeRegistration = function (registrationName, cacheReg) {
    var cachedRegForPropertySet = JSON.stringify(cacheReg);
    this.registrations.insert(registrationName, cachedRegForPropertySet);
};

LocalStorageManager.prototype.readRegistration = function (registrationName) {
    if (this.registrations.hasKey(registrationName)) {
        var cachedRegFromPropertySet = this.registrations[registrationName];
        return JSON.parse(cachedRegFromPropertySet);
    } else {
        return null;
    }
};

LocalStorageManager.prototype.updateRegistrationByRegistrationId = function (registrationId, registrationName, channelUri) {
    var registration = this.getFirstRegistrationByRegistrationId(registrationId);

    if (registration) {
        this.updateRegistrationByRegistrationName(registration.registrationName, registration.registrationId, channelUri);
    } else {
        this.updateRegistrationByRegistrationName(registrationName, registrationId, channelUri);
    }
};

LocalStorageManager.prototype.clearRegistrations = function () {
    this.registrations.clear();
    this.flushToSettings();
};

LocalStorageManager.prototype.refreshFinished = function (refreshedChannelUri) {
    this.setChannelUri(refreshedChannelUri);
    this.isRefreshNeeded = false;
};

LocalStorageManager.prototype.flushToSettings = function () {
    this.settings.insert(this.keyNameVersion, this.storageVersion);
    this.settings.insert(this.keyNameChannelUri, this.channelUri);

    var str = '';
    if (this.registrations != null) {
        str = JSON.stringify(this.registrations);
    }

    this.settings.insert(this.keyNameRegistrations, str);
};

LocalStorageManager.prototype.initializeRegistrationInfoFromStorage = function () {
    this.registrations = new Windows.Foundation.Collections.PropertySet();

    // Read channelUri
    this.channelUri = readContent(this.settings, this.keyNameChannelUri);

    // Verify this.storage version
    var version = readContent(this.settings, this.keyNameVersion);
    if (this.storageVersion !== version.toLowerCase()) {
        this.isRefreshNeeded = true;
        return;
    }

    this.isRefreshNeeded = false;

    // read registrations
    var regsStr = readContent(this.settings, this.keyNameRegistrations);
    if (regsStr) {
        var entries = JSON.parse(regsStr);

        for (var reg in entries) {
            if (entries.hasOwnProperty(reg)) {
                this.registrations.insert(reg, entries[reg]);
            }
        }
    }
};

function readContent(propertySet, key) {
    if (propertySet.hasKey(key)) {
        return propertySet[key];
    }
    return '';
}