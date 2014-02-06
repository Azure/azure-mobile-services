// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="..\Generated\MobileServices.DevIntellisense.js" />

// Declare JSHint globals
/*global WinJS:false, Windows:false, $__fileVersion__:false, $__version__:false */

exports.PushHttpClient = function (mobileServicesClient) {
    this.mobileServicesClient = mobileServicesClient;
};

PushHttpClient.prototype.listRegistrationsAsync = function(channelUri) {
    return this._request('GET', '/push/registrations?platform=wns&deviceId=' + channelUri)
        .then(function(response) {
            return JSON.parse(response);
        });
};

PushHttpClient.prototype.unregisterAsync = function(registrationId)
{
    return this._request('DELETE', '/push/registrations/' + registrationId);
};

PushHttpClient.prototype.createRegistrationIdAsync = function() {
    return this._request('POST', '/push/registrationIds')
        .then(function(response) {
            return response.headers.Location.split('/').last();
        });
};

PushHttpClient.prototype.createOrUpdateRegistrationAsync = function(registration) {
    return this._request('PUT', '/push/registrations/' + registration.registrationId, registration);
};

// Actual method params:
// function(method, uriFragment, content, ignoreFilters, headers)
PushHttpClient.prototype._request = function() {
    return Platform.async(this.mobileServicesClient._request);
};