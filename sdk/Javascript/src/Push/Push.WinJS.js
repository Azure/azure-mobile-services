// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="..\Generated\MobileServices.DevIntellisense.js" />

var _ = require('Extensions'),
    Validate = require('Validate'),
    Platform = require('Platform'),
    RegistrationManager = require('RegistrationManager').RegistrationManager;

function Push(mobileServicesClient, tileId) {
    tileId = tileId || '$Primary';
    var packageName = Windows.ApplicationModel.Package.current.id.name,
        name = _.format('{0}-PushContainer-{1}-{2}', packageName, mobileServicesClient.applicationUrl, tileId);

    this._registrationManager = new RegistrationManager(mobileServicesClient, 'wns', name);
}

exports.Push = Push;

Push.prototype.registerNative = function (channelUri, tags) {
    /// <summary>
    /// Register for native notification
    /// </summary>
    /// <param name="channelUri">
    /// The channelUri to register
    /// </param>
    /// <param name="tags" mayBeNull="true">
    /// Array containing the tags for this registeration
    /// </param>
    /// <returns>
    /// A promise that will complete when the registration is complete.
    /// </returns>
    
    var registration = makeCoreRegistration(channelUri, tags);        
    return this._registrationManager.upsertRegistration(registration);
};

Push.prototype.registerTemplate = function (channelUri, templateBody, templateName, headers, tags) {
    /// <summary>
    /// Register for template notification
    /// </summary>
    /// <param name="channelUri">The channelUri to register</param>
    /// <param name="templateBody">The xml body to register</param>
    /// <param name="templateName">The name of the template</param>
    /// <param name="headers">Object containing key/value pairs for the template to provide to WNS. X-WNS-Type is required. Example: { 'X-WNS-Type' : 'wns/toast' }</param>
    /// <param name="tags">Array containing the tags for this registeration</param>
    /// <returns>
    /// A promise that will complete when the registration is complete.
    /// </returns>

    var registration = makeCoreRegistration(channelUri, tags);
    
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

    return this._registrationManager.upsertRegistration(registration);
};

Push.prototype.unregisterNative = function () {
    /// <summary>
    /// Unregister for native notification
    /// </summary>
    /// <returns>Promise that will complete when the unregister is completed</returns>
    
    return this.unregisterTemplate(RegistrationManager.NativeRegistrationName);
};

Push.prototype.unregisterTemplate = function (templateName) {
    /// <summary>
    /// Unregister for template notification
    /// </summary>
    /// <param name="templateName">
    /// The name of the template
    /// </param>
    /// <returns>Promise that will complete when the unregister is completed</returns>
    
    Validate.notNullOrEmpty(templateName, 'templateName');

    return this._registrationManager.deleteRegistrationWithName(templateName);
};

Push.prototype.unregisterAll = function (channelUri) {
    /// <summary>
    /// DEBUG-ONLY: Unregister all notifications for a specfic channelUri
    /// </summary>
    /// <param name="channelUri">The channelUri to unregister</param>
    /// <returns>Promise that will complete when the unregistration of all registrations at the channelUri is completed</returns>
    
    Validate.notNullOrEmpty(channelUri, 'channelUri');

    return this._registrationManager.deleteAllRegistrations(channelUri);
};

Push.prototype.getSecondaryTile = function (tileId) {
    // TODO: move below to function
    var packageName = Windows.ApplicationModel.Package.current.id.name,
        name = _.format('{0}-PushContainer-{1}-{2}', packageName, mobileServicesClient.applicationUrl, tileId),
        localStorage = new LocalStorageManager(name);

    return new Push(name);
};

function makeCoreRegistration(channelUri, tags) {
    var registration = {};

    registration.platform = 'wns';

    Validate.isString(channelUri, 'channelUri');
    Validate.notNullOrEmpty(channelUri, 'channelUri');

    registration.deviceId = channelUri;

    if (tags) {
        Validate.isArray(tags, 'tags');
        registration.tags = tags;
    }

    return registration;
}