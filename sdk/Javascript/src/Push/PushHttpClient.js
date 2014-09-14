// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="..\Generated\MobileServices.DevIntellisense.js" />

var Platform = require('Platform'),
    noCacheHeader = { 'If-Modified-Since': 'Mon, 27 Mar 1972 00:00:00 GMT' };

function PushHttpClient(mobileServicesClient) {
    this.mobileServicesClient = mobileServicesClient;
}

exports.PushHttpClient = PushHttpClient;

PushHttpClient.prototype.listRegistrations = function (pushHandle, platform, callback) {
    this.mobileServicesClient._request(
        'GET', 
        '/push/registrations?platform=' + encodeURIComponent(platform) + '&deviceId=' + encodeURIComponent(pushHandle), 
        null, 
        null, 
        noCacheHeader, 
        function (error, request) {
            if (error) {
                callback(error);
            } else {
                callback(null, JSON.parse(request.responseText));
            }
        });
};

PushHttpClient.prototype.unregister = function (registrationId, callback) {
    this.mobileServicesClient._request(
        'DELETE', 
        '/push/registrations/' + encodeURIComponent(registrationId), 
        null, 
        null, 
        noCacheHeader, 
        function (error) {
            if (error && error.request && error.request.status === 404) {
                callback();
                return;
            }
            callback(error);
        });
};

PushHttpClient.prototype.createRegistrationId = function (callback) {
    this.mobileServicesClient._request(
        'POST', 
        '/push/registrationIds', 
        null, 
        null, 
        noCacheHeader, 
        function (error, request) {
            if (error) {
                callback(error);
                return;
            }

            var locationHeader = request.getResponseHeader('Location');
            callback(null, locationHeader.slice(locationHeader.lastIndexOf('/') + 1));
        });
};

PushHttpClient.prototype.upsertRegistration = function (registration, callback) {
    this.mobileServicesClient._request(
        'PUT', 
        '/push/registrations/' + encodeURIComponent(registration.registrationId), 
        registration, 
        null, 
        noCacheHeader, 
        callback);
};