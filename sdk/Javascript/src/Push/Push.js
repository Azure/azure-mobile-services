// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="..\Generated\MobileServices.DevIntellisense.js" />

// Declare JSHint globals
/*global WinJS:false, Windows:false, $__fileVersion__:false, $__version__:false */

var Validate = require('Validate');
var LocalStorageManager = require('LocalStorageManager').LocalStorageManager;
var RegistrationManager = require('RegistrationManager').RegistrationManager;
var PushHttpClient = require('PushHttpClient').PushHttpClient;

function Push(mobileServicesClient, tileId) {
    var localStorage = new LocalStorageManager(mobileServicesClient.applicationUrl, tileId);
    this.registrationManager = new RegistrationManager(
        new PushHttpClient(mobileServicesClient),
        localStorage
        );
};

exports.Push = Push;

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
// headers: { Key1: 'Value1', Key2: 'Value2'// if wns template registration }
// }
Push.prototype.registerTemplate = function (channelUri, tags, templateBody, templateName, headers) {
    var registration = {};

    registration.platform = 'wns';

    Validate.isString(channelUri, 'channelUri');
    Validate.notNullOrEmpty(channelUri, 'channelUri');

    registration.deviceId = channelUri;

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

        if (headers) {
            Validate.isObject(headers);
            registration.headers = headers;
        }
    }

    return this.registrationManager.register(registration);
};

Push.prototype.unregisterNative = function () {
    return this.unregisterTemplate('$Default');
};

Push.prototype.unregisterTemplate = function (templateName) {
    Validate.notNullOrEmpty(templateName);
    return this.registrationManager.unregister(templateName);
};

Push.prototype.unregisterAll = function (channelUri) {
    Validate.notNullOrEmpty(channelUri);
    return this.registrationManager.deleteRegistrationsForChannel(channelUri);
};

Push.prototype.getSecondaryTile = function (tileId) {
    return new Push(this.mobileServicesClient, tileId);
};