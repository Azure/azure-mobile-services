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
/// <param name="pushChannel" type="string">
/// The push channel identifier or URI.
/// </param>
/// <param name="templates" type="string">
/// An object containing template definitions. Template objects should contain body, headers and tags properties.
/// </param>
/// <param name="secondaryTiles" type="string">
/// An object containing template definitions to be used with secondary tiles when using WNS.
/// </param>
Push.prototype.register = Platform.async(
    function (platform, pushChannel, templates, secondaryTiles, callback) {
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
            pushChannel: pushChannel,
            platform: platform,
            templates: stringifyTemplateBodies(templates),
            secondaryTiles: stringifyTemplateBodies(secondaryTiles)
        };

        executeRequest(this.client, 'PUT', pushChannel, requestContent, this.installationId, callback);
    }
);

/// <summary>
/// Unregister a push channel with the Mobile Apps backend to stop receiving notifications.
/// </summary>
/// <param name="pushChannel" type="string">
/// The push channel identifier or URI.
/// </param>
Push.prototype.unregister = Platform.async(
    function (pushChannel, callback) {
        executeRequest(this.client, 'DELETE', pushChannel, null, this.installationId, callback);
    }
);

function executeRequest(client, method, pushChannel, content, installationId, callback) {
    Validate.isString(pushChannel, 'pushChannel');
    Validate.notNullOrEmpty(pushChannel, 'pushChannel');

    client._request(
        method,
        'push/installations/' + encodeURIComponent(installationId),
        content,
        null,
        { 'If-Modified-Since': 'Mon, 27 Mar 1972 00:00:00 GMT' },
        callback
    );
}

function stringifyTemplateBodies(templates) {
    var result = {};
    for (var templateName in templates) {
        if (templates.hasOwnProperty(templateName)) {
            // clone the template so we are not modifying the original
            var template = _.extend({}, templates[templateName]);
            if (typeof template.body !== 'string') {
                template.body = JSON.stringify(template.body);
            }
            result[templateName] = template;
        }
    }
    return result;
}