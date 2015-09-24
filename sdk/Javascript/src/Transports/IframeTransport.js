// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="..\Generated\MobileServices.DevIntellisense.js" />

// This transport is for midlevel browsers (IE8-9) that don't support CORS but do support postMessage.
// It creates an invisible <iframe> that loads a special bridge.html page from the runtime domain.
// To issue a request, it uses postMessage to pass the request into the <iframe>, which in turn makes
// a same-domain Ajax request to the runtime. To associate postMessage replies with the original
// request, we track an array of promises that eventually time out if not resolved (see PostMessageExchange).

var Promises = require('../Utilities/Promises'),
    PostMessageExchange = require('../Utilities/PostMessageExchange'),
    loadBridgeFramePromises = [], // One per target proto/host/port triplet
    messageExchange = PostMessageExchange.instance;

exports.name = "../../Transports/IframeTransport";

exports.supportsCurrentRuntime = function () {
    /// <summary>
    /// Determines whether or not this transport is usable in the current runtime.
    /// </summary>

    return typeof global.postMessage !== "undefined";
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

    var originRoot = PostMessageExchange.getOriginRoot(request.url);
    whenBridgeLoaded(originRoot, function (bridgeFrame) {
        var message = {
            type: request.type,
            url: request.url,
            headers: request.headers,
            data: request.data
        };
        messageExchange.request(bridgeFrame.contentWindow, message, originRoot).then(function (reply) {
            fixupAjax(reply);
            callback(null, reply);
        }, function (error) {
            callback(error, null);
        });
    });
};

function fixupAjax(xhr) {
    if (xhr) {
        // IE sometimes converts status 204 into 1223
        // http://stackoverflow.com/questions/10046972/msie-returns-status-code-of-1223-for-ajax-request
        if (xhr.status === 1223) {
            xhr.status = 204;
        }
    }
}

function whenBridgeLoaded(originRoot, callback) {
    /// <summary>
    /// Performs the callback once the bridge iframe has finished loading.
    /// Lazily creates the bridge iframe if necessary. Note that each proto/host/port
    /// triplet (i.e., same-domain origin) needs a separate bridge.
    /// </summary>

    var cacheEntry = loadBridgeFramePromises[originRoot];

    if (!cacheEntry) {
        cacheEntry = loadBridgeFramePromises[originRoot] = new Promises.Promise(function (complete, error) {
            var bridgeFrame = document.createElement("iframe"),
                callerOrigin = PostMessageExchange.getOriginRoot(window.location.href),
                handleBridgeLoaded = function() {
                    complete(bridgeFrame);
                };
            
            if (bridgeFrame.addEventListener) {
                bridgeFrame.addEventListener("load", handleBridgeLoaded, false);
            } else {
                // For IE8
                bridgeFrame.attachEvent("onload", handleBridgeLoaded);
            }

            bridgeFrame.src = originRoot + "/crossdomain/bridge?origin=" + encodeURIComponent(callerOrigin);
            
            // Try to keep it invisible, even if someone does $("iframe").show()
            bridgeFrame.setAttribute("width", 0);
            bridgeFrame.setAttribute("height", 0);
            bridgeFrame.style.display = "none";

            global.document.body.appendChild(bridgeFrame);
        });
    }

    cacheEntry.then(callback);
}

