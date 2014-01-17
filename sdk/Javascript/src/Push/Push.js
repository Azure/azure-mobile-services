// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="..\Generated\MobileServices.DevIntellisense.js" />

// Declare JSHint globals
/*global WinJS:false, Windows:false, $__fileVersion__:false, $__version__:false */

var _ = require('Extensions');
var Validate = require('Validate');
var Platform = require('Platform');

exports.Push = function(mobileServicesClient) {
    this.mobileServicesClient = mobileServicesClient;
};

// {
// platform: "wns" // {"wns"|"mpns"|"apns"|"gcm"}
// channelUri: "" // if wns or mpns
// tags: "tag1,tag2,tag3"
// bodyTemplate: '<toast>
//      <visual lang="en-US">
//        <binding template="ToastText01">
//          <text id="1">$(myTextProp1)</text>
//        </binding>
//      </visual>
//    </toast>' // if template registration
// templateName: "" // if template registration
// wnsHeaders: { // if wns template registration }
// }
exports.register = function(channelUri, tags, template, templateName, wnsHeaders) {
    var registration = {};

    Validate.isString(channelUri, 'channelUri');
    Validate.notNullOrEmpty(channelUri, 'channelUri');

    registration.channelUri = channelUri;

    if (tags) {
        Validate.isString(tags, 'tags');
        registration.tags = tags;
    }

    if (template) {
        Validate.isString(template, 'template');
        registration.template = template;
        Validate.isString(templateName, 'templateName');
        Validate.notNullOrEmpty(templateName);
        registration.templateName = templateName;
    }

    if (wnsHeaders) {
        Validate.isObject(wnsHeaders);
        // TODO: wnsHeaders is object with key/value pairs?
        registration.wnsHeaders = wnsHeaders;
    }
};

function LocalStorageManager(applicationUri, tileId) {
    if (!tileId || (tileId && _.isNullOrEmpty(tileId))) {
        tileId = '$Primary';
    }
    
    var name = _.format("{0}-PushContainer-{1}-{2}", Windows.ApplicationModel.Package.current.Id.Name, applicationUri, tileId);
    this.settings = Windows.Storage.ApplicationData.current.localSettings.createContainer(name, Windows.Storage.ApplicationDataCreateDisposition.always).values;
    this.isRefreshNeeded = false;
    this.channelUri = null;
    this.InitializeRegistrionInfoFromStorage();
}

LocalStorageManager.prototype.GetRegistration = function(registrationName) {
    return this.localSettingsContainer[registrationName];
};

LocalStorageManager.prototype.DeleteRegistration = function(registrationName) {
    if (Platform.tryRemoveSetting(registrationName, this.settings)) {
        this.FlushToSettings();
        return true;
    }

    return false;
};

LocalStorageManager.prototype.UpdateRegistrationByRegistrationName = function (registrationName, registrationId, channelUri) {
    var cachedReg = {};
    cachedReg.registrationName = registrationName;
    cachedReg.registrationId = registrationId;
    this.localSettingsContainer[registrationName] = cachedReg;
    this.channelUri = channelUri;
    this.FlushToSettings();
};

LocalStorageManager.prototype.UpdateRegistrationByRegistrationId = function(registrationId, registrationName, channelUri) {
    var found = false;
    // update registration if registrationId is in cached registartions, otherwise create new one
    for (var i = 0; i < this.localSettingsContainer.values.size; i++) {
        if (this.localSettingsContainer.values[i].registrationId === registrationId) {
            found = this.localSettingsContainer.values[i];
            break;
        }
    }

    if (found) {
        this.UpdateRegistrationByRegistrationName(found.registrationName, found.registrationId, channelUri);
    } else {
        this.UpdateRegistrationByRegistrationName(registrationName, found.registrationId, channelUri);
    }
};

LocalStorageManager.prototype.ClearRegistrations = function() {
    this.localSettingsContainer.clear();
    this.FlushToSettings();
};

LocalStorageManager.prototype.RefreshFinished = function(refreshedChannelUri) {
    this.ChannelUri = refreshedChannelUri;
    this.IsRefreshNeeded = false;
};

LocalStorageManager.prototype.FlushToSettings = function() {
    this.localSettingsContainer.values[KeyNameVersion] = StorageVersion;
    this.localSettingsContainer.values[KeyNameChannelUri] = this.channelUri

    var str = '';
    if (this.registrations != null) {
        var entries = this.registrations.Select(v =>
        v.Value.ToString())
        ;
        str = string.Join(";", entries);
    }

    SetContent(this.StorageValues, KeyNameRegistrations, str);
}
};