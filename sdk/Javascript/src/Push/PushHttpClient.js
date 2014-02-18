// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="..\Generated\MobileServices.DevIntellisense.js" />

var _ = require('Extensions');
var Platform = require('Platform');
var noCacheHeader = { 'If-Modified-Since': 'Mon, 27 Mar 1972 00:00:00 GMT' };
function PushHttpClient(mobileServicesClient) {
    this.mobileServicesClient = mobileServicesClient;
}

exports.PushHttpClient = PushHttpClient;

PushHttpClient.prototype.listRegistrations = function (channelUri) {
    return this._request('GET', '/push/registrations?platform=wns&deviceId=' + encodeURIComponent(channelUri), null, null, noCacheHeader)
        .then(function (request) {
            return JSON.parse(request.response);
        });
};

PushHttpClient.prototype.unregister = function (registrationId) {
    return this._request('DELETE', '/push/registrations/' + encodeURIComponent(registrationId), null, null, noCacheHeader);
};

PushHttpClient.prototype.createRegistrationId = function () {
    return this._request('POST', '/push/registrationIds', null, null, noCacheHeader)
        .then(function (response) {
            var locationHeader = response.getResponseHeader('Location');
            return locationHeader.slice(locationHeader.lastIndexOf('/') + 1);
        });
};

PushHttpClient.prototype.createOrUpdateRegistration = function (registration) {
    return this._request('PUT', '/push/registrations/' + encodeURIComponent(registration.registrationId), registration, null, noCacheHeader);
};

PushHttpClient.prototype._request = Platform.async(
    function (method, uriFragment, content, ignoreFilters, headers, callback) {
        this.mobileServicesClient._request(method, uriFragment, content, ignoreFilters, headers, callback);
    });