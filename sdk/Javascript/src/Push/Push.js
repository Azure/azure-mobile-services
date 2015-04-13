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

Push.prototype.register = Platform.async(
    function (platform, channelUri, templates, secondaryTiles, callback) {
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
            pushChannel: channelUri,
            platform: platform,
            templates: templates,
            secondaryTiles: secondaryTiles
        };

        executeRequest(this.client, 'PUT', channelUri, requestContent, this.installationId, callback);
    }
);

Push.prototype.unregister = Platform.async(
    function (channelUri, callback) {
        executeRequest(this.client, 'DELETE', channelUri, null, this.installationId, callback);
    }
);

function executeRequest(client, method, channelUri, content, installationId, callback) {
    Validate.isString(channelUri, 'channelUri');
    Validate.notNullOrEmpty(channelUri, 'channelUri');

    client._request(
        method,
        'push/installations/' + encodeURIComponent(installationId),
        content,
        null,
        { 'If-Modified-Since': 'Mon, 27 Mar 1972 00:00:00 GMT' },
        callback
    );
}