// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

var PostMessageExchange = require('PostMessageExchange');

exports.supportsCurrentRuntime = function () {
    /// <summary>
    /// Determines whether or not this login UI is usable in the current runtime.
    /// </summary>
    return true;
};

exports.login = function (startUri, endUri, callback) {
    /// <summary>
    /// Displays the login UI and calls back on completion
    /// </summary>

    // Tell the runtime which form of completion signal we are looking for,
    // and which origin should be allowed to receive the result (note that this
    // is validated against whitelist on the server; we are only supplying this
    // origin to indicate *which* of the whitelisted origins to use).
    var completionOrigin = PostMessageExchange.getOriginRoot(window.location.href),
        runtimeOrigin = PostMessageExchange.getOriginRoot(startUri),
        // IE does not support popup->opener postMessage calls, so we have to
        // route the message via an iframe
        useIntermediateIframe = window.navigator.userAgent.indexOf("MSIE") >= 0,
        intermediateIframe = useIntermediateIframe && createIntermediateIframeForLogin(runtimeOrigin, completionOrigin),
        completionType = useIntermediateIframe ? "iframe" : "postMessage";
    startUri += "?completion_type=" + completionType + "&completion_origin=" + encodeURIComponent(completionOrigin);

    // Browsers don't allow postMessage to a file:// URL (except by setting origin to "*", which is unacceptable)
    // so abort the process early with an explanation in that case.
    if (!(completionOrigin && (completionOrigin.indexOf("http:") === 0 || completionOrigin.indexOf("https:") === 0))) {
        var error = "Login is only supported from http:// or https:// URLs. Please host your page in a web server.";
        callback(error, null);
        return;
    }

    var loginWindow = window.open(startUri, "_blank", "location=no"),
        complete = function(errorValue, oauthValue) {
            // Clean up event handlers, windows, frames, ...
            window.clearInterval(checkForWindowClosedInterval);
            loginWindow.close();
            if (window.removeEventListener) {
                window.removeEventListener("message", handlePostMessage);
            } else {
                // For IE8
                window.detachEvent("onmessage", handlePostMessage);
            }
            if (intermediateIframe) {
                intermediateIframe.parentNode.removeChild(intermediateIframe);
            }
            
            // Finally, notify the caller
            callback(errorValue, oauthValue);
        },
        handlePostMessage = function(evt) {
            // Validate source
            var expectedSource = useIntermediateIframe ? intermediateIframe.contentWindow : loginWindow;
            if (evt.source !== expectedSource) {
                return;
            }

            // Parse message
            var envelope;
            try {
                envelope = JSON.parse(evt.data);
            } catch(ex) {
                // Not JSON - it's not for us. Ignore it and keep waiting for the next message.
                return;
            }

            // Process message only if it's for us
            if (envelope && envelope.type === "LoginCompleted" && (envelope.oauth || envelope.error)) {
                complete(envelope.error, envelope.oauth);
            }
        },
        checkForWindowClosedInterval = window.setInterval(function() {
            // We can't directly catch any "onclose" event from the popup because it's usually on a different
            // origin, but in all the mainstream browsers we can poll for changes to its "closed" property
            if (loginWindow && loginWindow.closed === true) {
                complete("canceled", null);
            }
        }, 250);

    if (window.addEventListener) {
        window.addEventListener("message", handlePostMessage, false);
    } else {
        // For IE8
        window.attachEvent("onmessage", handlePostMessage);
    }
    
    // Permit cancellation, e.g., if the app tries to login again while the popup is still open
    return {
        cancelCallback: function () {
            complete("canceled", null);
            return true; // Affirm that it was cancelled
        }
    };
};

function createIntermediateIframeForLogin(runtimeOrigin, completionOrigin) {
    var frame = document.createElement("iframe");
    frame.name = "zumo-login-receiver"; // loginviaiframe.html specifically looks for this name
    frame.src = runtimeOrigin +
        "/crossdomain/loginreceiver?completion_origin=" + encodeURIComponent(completionOrigin);
    frame.setAttribute("width", 0);
    frame.setAttribute("height", 0);
    frame.style.display = "none";
    document.body.appendChild(frame);
    return frame;
}