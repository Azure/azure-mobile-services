// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="..\Generated\MobileServices.DevIntellisense.js" />

var _ = require('Extensions'),
    Validate = require('Validate'),
    Platform = require('Platform'),
    RegistrationManager = require('RegistrationManager').RegistrationManager,
	apns = function (push) {
	    this._push = push;
	},
	gcm = function (push) {
	    this._push = push;
	};

function Push(mobileServicesClient) {
    this._apns = null;
    this._gcm = null;
    this._registrationManager = null;

    Object.defineProperties(this, {
        'apns': {
            get: function () {
                if (!this._apns) {
                    var name = _.format('MS-PushContainer-apns-{0}', mobileServicesClient.applicationUrl);
                    this._registrationManager = new RegistrationManager(mobileServicesClient, 'apns', name);
                    this._apns = new apns(this);
                }
                return this._apns;
            }
        },
        'gcm': {
            get: function () {
                if (!this._gcm) {
                    var name = _.format('MS-PushContainer-gcm-{0}', mobileServicesClient.applicationUrl);
                    this._registrationManager = new RegistrationManager(mobileServicesClient, 'apns', name);
                    this._gcm = new gcm(this);
                }
                return this._gcm;
            }
        }
    });
}

exports.Push = Push;

Push.prototype._register = function (platform, pushHandle, tags) {
    /// <summary>
    /// Register for native notification
    /// </summary>
    /// <param name="deviceToken">
    /// The deviceToken to register
    /// </param>
    /// <param name="tags" mayBeNull="true">
    /// Array containing the tags for this registeration
    /// </param>
    /// <returns>
    /// Promise that will complete when the nregister is completed
    /// </returns>

    var registration = makeCoreRegistration(pushHandle, platform, tags);
    return this._registrationManager.upsertRegistration(registration);
};

Push.prototype._registerTemplate = function (platform, deviceToken, name, bodyTemplate, expiryTemplate, tags) {
    /// <summary>
    /// Register for template notification
    /// </summary>
    /// <param name="deviceToken">The deviceToken to register</param>
    /// <param name="name">The name of the template</param>
    /// <param name="bodyTemplate">
    /// The json body to register
    /// </param>
    /// <param name="expiryTemplate">
    /// The json body to register
    /// </param>
    /// <param name="tags">
    /// Array containing the tags for this registeration
    /// </param>
    /// <returns>
    /// Promise that will complete when the register is completed
    /// </returns>

    Validate.notNullOrEmpty(name, 'name');
    Validate.notNullOrEmpty(bodyTemplate, 'bodyTemplate');

    var templateAsString = bodyTemplate,
        registration = makeCoreRegistration(deviceToken, platform, tags);
    
    if (!_.isString(templateAsString)) {
        templateAsString = JSON.stringify(templateAsString);
    }

    registration.templateName = name;
    registration.templateBody = templateAsString;
    if (expiryTemplate) {
        registration.expiry = expiryTemplate;
    }

    return this._registrationManager.upsertRegistration(registration);
};

Push.prototype._unregister = function (templateName) {
    /// <summary>
    /// Unregister for template notification
    /// </summary>
    /// <param name="templateName">
    /// The name of the template
    /// </param>
    /// <returns>
    /// Promise that will complete when the unregister is completed
    /// </returns>

    Validate.notNullOrEmpty(templateName, 'templateName');

    return this._registrationManager.deleteRegistrationWithName(templateName);
};

Push.prototype._unregisterAll = function (pushHandle) {
    /// <summary>
    /// Unregister for all notifications
    /// </summary>
    /// <param name="pushHandle">
    /// The push handle to unregister everything for
    /// </param>
    /// <returns>
    /// Promise that will complete when the unregister is completed
    /// </returns>

    Validate.notNullOrEmpty(pushHandle, 'pushHandle');

    return this._registrationManager.deleteAllRegistrations(pushHandle);
};


apns.prototype.registerNative = function (deviceToken, tags) {
    /// <summary>
    /// Register for native notification
    /// </summary>
    /// <param name="deviceToken">
    /// The deviceToken to register
    /// </param>
    /// <param name="tags" mayBeNull="true">
    /// Array containing the tags for this registeration
    /// </param>
    /// <returns>
    /// Promise that will complete when the register is completed
    /// </returns>
    
    return this._push._register('apns', deviceToken, tags);
};

apns.prototype.registerTemplate = function (deviceToken, name, bodyTemplate, expiryTemplate, tags) {
    /// <summary>
    /// Register for template notification
    /// </summary>
    /// <param name="deviceToken">The deviceToken to register</param>
    /// <param name="name">The name of the template</param>
    /// <param name="bodyTemplate">
    /// String or json object defining the body of the template register
    /// </param>
    /// <param name="expiryTemplate">
    /// String defining the datatime or template expresession that evaluates to a date time
    /// string to use for the expiry of the message
    /// </param>
    /// <param name="tags">
    /// Array containing the tags for this registeration
    /// </param>
    /// <returns>
    /// Promise that will complete when the unregister is completed
    /// </returns>

    if (_.isNull(tags) && !_.isNull(expiryTemplate) && Array.isArray(expiryTemplate)) {
        tags = expiryTemplate;
        expiryTemplate = null;
    }

    return this._push._registerTemplate('apns', deviceToken, name, bodyTemplate, expiryTemplate, tags);
};

apns.prototype.unregisterNative = function () {
    /// <summary>
    /// Unregister for native notification
    /// </summary>
    /// <returns>
    /// Promise that will complete when the unregister is completed
    /// </returns>

    return this._push._unregister(RegistrationManager.NativeRegistrationName);
};

apns.prototype.unregisterTemplate = function (templateName) {
    /// <summary>
    /// Unregister for template notification
    /// </summary>
    /// <param name="templateName">
    /// The name of the template
    /// </param>
    /// <returns>
    /// Promise that will complete when the unregister is completed
    /// </returns>

    return this._push._unregister(templateName);
};

apns.prototype.unregisterAll = function (deviceToken) {
    /// <summary>
    /// DEBUG-ONLY: Unregisters all registrations for the given device token
    /// </summary>
    /// <param name="deviceToken">
    /// The device token
    /// </param>
    /// <returns>
    /// Promise that will complete once all registrations are deleted
    /// </returns>

    return this._push._unregisterAll(deviceToken);
};

gcm.prototype.registerNative = function (deviceId, tags) {
    /// <summary>
    /// Register for native notification
    /// </summary>
    /// <param name="deviceId">
    /// The deviceToken to register
    /// </param>
    /// <param name="tags" mayBeNull="true">
    /// Array containing the tags for this registeration
    /// </param>
    /// <returns>
    /// Promise that will complete when the unregister is completed
    /// </returns>

    return this._push._register('gcm', deviceId, tags);
};

gcm.prototype.registerTemplate = function (deviceId, name, bodyTemplate, tags) {
    /// <summary>
    /// Register for template notification
    /// </summary>
    /// <param name="deviceId">
    /// The deviceId to register
    /// </param>
    /// <param name="name">
    /// The name of the template
    /// </param>
    /// <param name="bodyTemplate">
    /// String or json object defining the body to register
    /// </param>
    /// <param name="tags">
    /// Array containing the tags for this registeration
    /// </param>
    /// <returns>
    /// Promise that will complete when the unregister is completed
    /// </returns>

    return this._push._registerTemplate('gcm', deviceId, name, bodyTemplate, null, tags);
};

gcm.prototype.unregisterNative = function () {
    /// <summary>
    /// Unregister for native notification
    /// </summary>
    /// <returns>
    /// Promise that will complete when the register is completed
    /// </returns>

    return this._push._unregister(RegistrationManager.NativeRegistrationName);
};

gcm.prototype.unregisterTemplate = function (templateName) {
    /// <summary>
    /// Unregister for template notification
    /// </summary>
    /// <param name="templateName">
    /// The name of the template
    /// </param>
    /// <returns>
    /// Promise that will complete when the register is completed
    /// </returns>
    return this._push._unregister(templateName);
};

gcm.prototype.unregisterAll = function (deviceId) {
    /// <summary>
    /// DEBUG-ONLY: Unregisters all registrations for the given device token
    /// </summary>
    /// <param name="deviceId">
    /// The device id
    /// </param>
    /// <returns>
    /// Promise that will complete once all registrations are deleted
    /// </returns>

    return this._push._unregisterAll(deviceId);
};


function makeCoreRegistration(pushHandle, platform, tags) {
    Validate.notNullOrEmpty(pushHandle, 'pushHandle');
    Validate.isString(pushHandle, 'pushHandle');

    if (platform == 'apns') {
        pushHandle = pushHandle.toUpperCase();
    }

    var registration = {
        platform: platform,
        deviceId: pushHandle
    };

    if (tags) {
        Validate.isArray(tags, 'tags');
        registration.tags = tags;
    }

    return registration;
}