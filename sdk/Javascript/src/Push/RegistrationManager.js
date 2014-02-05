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

exports.RegistrationManager = function(pushHttpClient, storageManager) {
    this.pushHttpClient = pushHttpClient;
    this.localStorageManager = storageManager;
};

RegistrationManager.prototype.Register = function(registration) {
    // if localStorage is empty or has different storage version, we need retrieve registrations and refresh local storage
    if (this.localStorageManager.IsRefreshNeeded) {
        var refreshChannelUri = this.localStorageManager.ChannelUri || registration.ChannelUri;
        this.GetRegistrationsForChannelAsync(refreshChannelUri);
        this.localStorageManager.RefreshFinished(refreshChannelUri);
    }

    var cached = this.localStorageManager.GetRegistration(registration.Name);
    if (cached != null) {
        registration.RegistrationId = cached.RegistrationId;
    } else {
        this.CreateRegistrationIdAsync(registration);
    }

    try {
        this.UpsertRegistration(registration);
        return;
    } catch(e)
    {
        // if we get an RegistrationGoneException (410) from service, we will recreate registration id and will try to do upsert one more time.
        // The likely cause of this is an expired registration in local storage due to a long unused app.
        //if (e.Response.StatusCode != HttpStatusCode.Gone) {
        //    throw;
        //}
    }

    //// recreate registration id if we encountered a previously expired registrationId
    this.CreateRegistrationIdAsync(registration);
    this.UpsertRegistration(registration);
};

RegistrationManager.prototype.GetRegistrationsForChannelAsync = function(channelUri) {
    var registrations = this.pushHttpClient.ListRegistrationsAsync(channelUri);
    var count = registrations.Count;
    if (count == 0) {
        this.localStorageManager.ClearRegistrations();
    }

    for (var i = 0; i < count ; i++)
    {
        this.localStorageManager.UpdateRegistrationByRegistrationId(registrations[i]);
    }
};

RegistrationManager.prototype.UnregisterAsync = function(registrationName) {
    Validate.notNullOrEmpty(registrationName, 'registrationName');

    var cached = this.localStorageManager.GetRegistration(registrationName);
    if (!cached) {
        return;
    }

    this.pushHttpClient.UnregisterAsync(cached.RegistrationId);
    this.localStorageManager.DeleteRegistrationByName(registrationName);
};

RegistrationManager.prototype.DeleteRegistrationsForChannelAsync = function(channelUri) {
    var registrations = this.pushHttpClient.ListRegistrationsAsync(channelUri);

    registrations.forEach(function(registration) {
        this.pushHttpClient.UnregisterAsync(registration.RegistrationId);
        this.localStorageManager.DeleteRegistrationByRegistrationId(registration.RegistrationId);
    });

    // clear local storage
    this.localStorageManager.ClearRegistrations();
};

RegistrationManager.prototype.CreateRegistrationIdAsync = function(registration) {
    registration.RegistrationId = this.pushHttpClient.CreateRegistrationIdAsync();
    this.localStorageManager.UpdateRegistrationByName(registration.Name, registration);
    return registration;
};

RegistrationManager.prototype.UpsertRegistration = function(registration) {
    this.pushHttpClient.CreateOrUpdateRegistrationAsync(registration);
    this.localStorageManager.UpdateRegistrationByName(registration.Name, registration);
};