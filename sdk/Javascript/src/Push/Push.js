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
var LocalStorageManager = require('LocalStorageManager');
var RegistrationManager = require('RegistrationManager');
var PushHttpClient = require('PushHttpClient');

exports.Push = function (mobileServicesClient) {
    this.mobileServicesClient = mobileServicesClient;
    this.registrationManager = new RegistrationManager(
        new PushHttpClient(mobileServicesClient),
        new LocalStorageManager(applicationUrl));
};

Push.prototype.registerNative = function (channelUri, tags) {
    return this.registerTemplate(channelUri, tags);
};

// {
// platform: "wns" // {"wns"|"mpns"|"apns"|"gcm"}
// channelUri: "" // if wns or mpns
// tags: "tag1,tag2,tag3"
// templateBody: '<toast>
//      <visual lang="en-US">
//        <binding template="ToastText01">
//          <text id="1">$(myTextProp1)</text>
//        </binding>
//      </visual>
//    </toast>' // if template registration
// templateName: "" // if template registration
// wnsHeaders: { Key1: 'Value1', Key2: 'Value2'// if wns template registration }
// }
Push.prototype.registerTemplate = function (channelUri, tags, templateBody, templateName, wnsHeaders) {
    var registration = {};

    Validate.isString(channelUri, 'channelUri');
    Validate.notNullOrEmpty(channelUri, 'channelUri');

    registration.channelUri = channelUri;

    if (tags) {
        Validate.isString(tags, 'tags');
        registration.tags = tags;
    }

    if (templateBody) {
        Validate.isString(templateBody, 'templateBody');
        registration.templateBody = templateBody;
        Validate.isString(templateName, 'templateName');
        Validate.notNullOrEmpty(templateName);
        registration.templateName = templateName;

        if (wnsHeaders) {
            Validate.isObject(wnsHeaders);
            registration.headers = wnsHeaders;
        }
    }    
    
    return this.registrationManager.register(registration);
};

Push.prototype.unregisterNative = function () {
    return this.unregisterTemplate('$Default');
};

Push.prototype.unregisterTemplate = function(templateName) {
    Validate.isNullOrEmpty(templateName);
    return this.registrationManager.unregisterAsync(templateName);
};

Push.prototype.unregisterAllAsync = function (channelUri) {
    Validate.isNullOrEmpty(channelUri);
    return this.registrationManager.deleteRegistrationsForChannelAsync(channelUri);
}