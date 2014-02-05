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

exports.LocalStorageManager = function(applicationUri, tileId) {
    if (!tileId || (tileId && _.isNullOrEmpty(tileId))) {
        tileId = '$Primary';
    }

    var name = _.format("{0}-PushContainer-{1}-{2}", Windows.ApplicationModel.Package.current.Id.Name, applicationUri, tileId);
    this.settings = Windows.Storage.ApplicationData.current.localSettings.createContainer(name, Windows.Storage.ApplicationDataCreateDisposition.always).values;
    this.isRefreshNeeded = false;
    this.channelUri = null;    
    
    // TOOD: Is there a better place to put constants?
    this.StorageVersion = "v1.0.0";
    this.PrimaryChannelId = "$Primary";
    this.KeyNameVersion = "Version";
    this.KeyNameChannelUri = "ChannelUri";
    this.KeyNameRegistrations = "Registrations";
        
    this.InitializeRegistrionInfoFromStorage();
};

LocalStorageManager.prototype.GetChannelUri = function() {
    return this.channelUri;
};

LocalStorageManager.prototype.SetChannelUri = function(channelUri) {
    this.channelUri = channelUri;
    this.FlushToSettings();
};

LocalStorageManager.prototype.GetRegistration = function(registrationName) {
    return this.registrations[registrationName];
};

LocalStorageManager.prototype.DeleteRegistrationByName = function(registrationName) {
    if (Platform.tryRemoveSetting(registrationName, this.registrations)) {
        this.FlushToSettings();
        return true;
    }

    return false;
};

LocalStorageManager.prototype.DeleteRegistrationByRegistrationId = function(registrationId) {
    var returnValue = false;
    Object.keys(this.registrations.values).forEach(function (key) {
        // Delete only the first registration with matching registrationId
        if (!returnValue && (this.registrations.values[key] === registrationId)) {
            returnValue = this.DeleteRegistrationByName(key);
        }        
    });

    return returnValue;
};

LocalStorageManager.prototype.UpdateRegistrationByRegistrationName = function (registrationName, registrationId, channelUri) {
    var cacheReg = {};
    cacheReg.registrationName = registrationName;
    cacheReg.registrationId = registrationId;
    this.registrations[registrationName] = cacheReg;
    this.channelUri = channelUri;
    this.FlushToSettings();
};

LocalStorageManager.prototype.UpdateRegistrationByRegistrationId = function(registrationId, registrationName, channelUri) {
    var found = false;
    // update registration if registrationId is in cached registartions, otherwise create new one
    Object.keys(this.registrations.values).forEach(function (key) {
        // Delete only the first registration with matching registrationId
        if (!found && (this.registrations.values[key].registrationId === registrationId)) {
            found = this.registrations.values[key];
        }        
    });

    if (found) {
        this.UpdateRegistrationByRegistrationName(found.registrationName, found.registrationId, channelUri);
    } else {
        this.UpdateRegistrationByRegistrationName(registrationName, registrationId, channelUri);
    }
};

LocalStorageManager.prototype.ClearRegistrations = function() {
    this.registrations.clear();
    this.FlushToSettings();
};

LocalStorageManager.prototype.RefreshFinished = function(refreshedChannelUri) {
    this.SetChannelUri(refreshedChannelUri);
    this.IsRefreshNeeded = false;
};

LocalStorageManager.prototype.FlushToSettings = function() {
    this.storage.values[KeyNameVersion] = StorageVersion;
    this.storage.values[KeyNameChannelUri] = this.channelUri;

    var str = '';
    if (this.registrations != null) {
        str = JSON.stringify(this.registrations.values);
    }

    this.storage.values[KeyNameRegistrations] = str;
};

LocalStorageManager.prototype.InitializeRegistrationInfoFromStorage = function() {
    this.registrations = new Windows.Foundation.Collections.PropertySet();

    // Read channelUri
    this.channelUri = readContent(this.storage, KeyNameChannelUri);

    // Verify storage version
    var version = readContent(this.storage, KeyNameVersion);
    if (StorageVersion === version.toLowerCase()) {
        this.IsRefreshNeeded = true;
        return;
    }

    this.IsRefreshNeeded = false;

    // read registrations
    var self = this;
    var regsStr = readContent(this.storageValues, KeyNameRegistrations);
    var entries = JSON.parse(regsStr);
    entries.forEach(function (reg) {
        self.registrations[reg.registrationName] = reg;
    });
};

function readContent(propertySet, key) {
    if (propertySet.hasKey(key)) {
        return propertySet[key];
    }
    return '';
}