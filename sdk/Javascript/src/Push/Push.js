// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var Validate = require('Validate'),
    Platform = require('Platform'),
    _ = require('Extensions');

exports.Push = Push;

function Push(client, installationId) {
    this.client = client;
    this.installationId = installationId;
}

/// <summary>
/// Register a push channel with the Mobile Apps backend to start receiving notifications.
/// </summary>
/// <param name="platform" type="string">
/// The device platform being used - wns, gcm or apns.
/// </param>
/// <param name="channelId" type="string">
/// The push channel identifier or URI.
/// </param>
/// <param name="templates" type="string">
/// An object containing template definitions. Template objects should contain body, headers and tags properties.
/// </param>
/// <param name="secondaryTiles" type="string">
/// An object containing template definitions to be used with secondary tiles when using WNS.
/// </param>
Push.prototype.register = Platform.async(
    function (platform, channelId, templates, secondaryTiles, callback) {
        Validate.isString(platform, 'platform');
        Validate.notNullOrEmpty(platform, 'platform');

        // in order to support the older callback style completion, we need to check optional parameters
        if (_.isNull(callback) && (typeof templates === 'function')) {
            callback = templates;
            templates = null;
        }

        if (_.isNull(callback) && (typeof secondaryTiles === 'function')) {
            callback = secondaryTiles;
            secondaryTiles = null;
        }

        var requestContent = {
            installationId: this.installationId,
            pushChannel: channelId,
            platform: platform,
            templates: templates,
            secondaryTiles: secondaryTiles
        };

        executeRequest(this.client, 'PUT', channelId, requestContent, this.installationId, callback);
    }
);

/// <summary>
/// Unregister a push channel with the Mobile Apps backend to stop receiving notifications.
/// </summary>
/// <param name="channelId" type="string">
/// The push channel identifier or URI.
/// </param>
Push.prototype.unregister = Platform.async(
    function (channelId, callback) {
        executeRequest(this.client, 'DELETE', channelId, null, this.installationId, callback);
    }
);

function executeRequest(client, method, channelId, content, installationId, callback) {
    Validate.isString(channelId, 'channelId');
    Validate.notNullOrEmpty(channelId, 'channelId');

    client._request(
        method,
        'push/installations/' + encodeURIComponent(installationId),
        content,
        null,
        { 'If-Modified-Since': 'Mon, 27 Mar 1972 00:00:00 GMT' },
        callback
    );
}