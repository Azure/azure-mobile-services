// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="..\Generated\MobileServices.DevIntellisense.js" />

// This transport is for modern browsers - it uses XMLHttpRequest with Cross-Origin Resource Sharing (CORS)

exports.name = "DirectAjaxTransport";

exports.supportsCurrentRuntime = function () {
    /// <summary>
    /// Determines whether or not this transport is usable in the current runtime.
    /// </summary>

    // Feature-detect support for CORS (for IE, it's in version 10+)
    return (typeof global.XMLHttpRequest !== "undefined") &&
           ('withCredentials' in new global.XMLHttpRequest());
};

exports.performRequest = function (request, callback) {
    /// <summary>
    /// Make a web request.
    /// </summary>
    /// <param name="request" type="Object">
    /// Object describing the request (in the WinJS.xhr format).
    /// </param>
    /// <param name="callback" type="Function">
    /// The callback to execute when the request completes.
    /// </param>

    var headers = request.headers || {},
        url = request.url.replace(/#.*$/, ""), // Strip hash part of URL for consistency across browsers
        httpMethod = request.type ? request.type.toUpperCase() : "GET",
        xhr = new global.XMLHttpRequest();

    xhr.onreadystatechange = function () {
        if (xhr.readyState === 4) {
            callback(null, xhr);
        }
    };

    xhr.open(httpMethod, url);

    for (var key in headers) {
        if (request.headers.hasOwnProperty(key)) {
            xhr.setRequestHeader(key, request.headers[key]);
        }
    }

    xhr.send(request.data);
};