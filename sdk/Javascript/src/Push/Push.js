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

exports.Push = function (mobileServicesClient) {
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
Push.prototype.register = function (channelUri, tags, template, templateName, wnsHeaders) {
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

Push.prototype.unregisterNative = function() {
};

Push.prototype.unregisterTemplate = function(templateName) {

};

