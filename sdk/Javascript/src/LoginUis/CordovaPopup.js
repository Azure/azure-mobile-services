// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

// Note: Cordova is PhoneGap.
// This login UI implementation uses the InAppBrowser plugin,
// to install the plugin use the following command
//   cordova plugin add org.apache.cordova.inappbrowser

var requiredCordovaVersion = { major: 3, minor: 0 };

exports.supportsCurrentRuntime = function () {
    /// <summary>
    /// Determines whether or not this login UI is usable in the current runtime.
    /// </summary>

    // When running application inside of Ripple emulator, InAppBrowser functionality is not supported.
    // We should use Browser popup login method instead.
    return !!currentCordovaVersion() && !isRunUnderRippleEmulator();
};

exports.login = function (startUri, endUri, callback) {
    /// <summary>
    /// Displays the login UI and calls back on completion
    /// </summary>

    // Ensure it's a sufficiently new version of Cordova, and if not fail synchronously so that
    // the error message will show up in the browser console.
    var foundCordovaVersion = currentCordovaVersion();
    if (!isSupportedCordovaVersion(foundCordovaVersion)) {
        var message = "Not a supported version of Cordova. Detected: " + foundCordovaVersion +
                    ". Required: " + requiredCordovaVersion.major + "." + requiredCordovaVersion.minor;
        throw new Error(message);
    }
    if (!hasInAppBrowser) {
        var message = 'A required plugin: "org.apache.cordova.inappbrowser" was not detected.';
        throw new Error(message);
    }

    // Initially we show a page with a spinner. This stays on screen until the login form has loaded.
    var redirectionScript = "<script>location.href = unescape('" + window.escape(startUri) + "')</script>",
        startPage = "data:text/html," + encodeURIComponent(getSpinnerMarkup() + redirectionScript);

    // iOS inAppBrowser issue requires this wrapping
    setTimeout(function () {
        var loginWindow = window.open(startPage, "_blank", "location=no"),
            flowHasFinished = false,
            loadEventHandler = function (evt) {
                if (!flowHasFinished && evt.url.indexOf(endUri) === 0) {
                    flowHasFinished = true;
                    setTimeout(function () {
                        loginWindow.close();
                    }, 500);
                    var result = parseOAuthResultFromDoneUrl(evt.url);
                    callback(result.error, result.oAuthToken);
                }
            };

        // Ideally we'd just use loadstart because it happens earlier, but it randomly skips
        // requests on iOS, so we have to listen for loadstop as well (which is reliable).
        loginWindow.addEventListener('loadstart', loadEventHandler);
        loginWindow.addEventListener('loadstop', loadEventHandler);

        loginWindow.addEventListener('exit', function (evt) {
            if (!flowHasFinished) {
                flowHasFinished = true;
                callback("UserCancelled", null);
            }
        });
    }, 500);
};

function isRunUnderRippleEmulator () {
    // Returns true when application runs under Ripple emulator 
    return window.parent && !!window.parent.ripple;
}

function currentCordovaVersion() {
    // If running inside Cordova, returns a string similar to "3.5.0". Otherwise, returns a falsey value.
    // Note: We can only detect Cordova after its deviceready event has fired, so don't call login until then.
    return window.cordova && window.cordova.version;
}

function isSupportedCordovaVersion(version) {
    var versionParts = currentCordovaVersion().match(/^(\d+).(\d+)./);
    if (versionParts) {
        var major = Number(versionParts[1]),
            minor = Number(versionParts[2]),
            required = requiredCordovaVersion;
        return (major > required.major) ||
               (major === required.major && minor >= required.minor);
    }
    return false;
}

function hasInAppBrowser() {
    return !window.open;
}

function parseOAuthResultFromDoneUrl(url) {
    var successMessage = extractMessageFromUrl(url, "#token="),
        errorMessage = extractMessageFromUrl(url, "#error=");
    return {
        oAuthToken: successMessage ? JSON.parse(successMessage) : null,
        error: errorMessage
    };
}

function extractMessageFromUrl(url, separator) {
    var pos = url.indexOf(separator);
    return pos < 0 ? null : decodeURIComponent(url.substring(pos + separator.length));
}

function getSpinnerMarkup() {
    // The default InAppBrowser experience isn't ideal, as it just shows the user a blank white screen
    // until the login form appears. This might take 10+ seconds during which it looks broken.
    // Also on iOS it's possible for the InAppBrowser to initially show the results of the *previous*
    // login flow if the InAppBrowser was dismissed before completion, which is totally undesirable.
    // To fix both of these problems, we display a simple "spinner" graphic via a data: URL until
    // the current login screen has loaded. We generate the spinner via CSS rather than referencing
    // an animated GIF just because this makes the client library smaller overall.
    var vendorPrefix = "webkitTransform" in document.documentElement.style ? "-webkit-" : "",
        numSpokes = 12,
        spokesMarkup = "";
    for (var i = 0; i < numSpokes; i++) {
        spokesMarkup += "<div style='-prefix-transform: rotateZ(" + (180 + i * 360 / numSpokes) + "deg);" +
                                    "-prefix-animation-delay: " + (0.75 * i / numSpokes) + "s;'></div>";
    }
    return [
        "<!DOCTYPE html><html>",
        "<head><meta name='viewport' content='width=device-width, initial-scale=1, maximum-scale=1'></head>",
        "<body><div id='spinner'>" + spokesMarkup + "</div>",
        "<style type='text/css'>",
        "    #spinner { position: absolute; top: 50%; left: 50%; -prefix-animation: spinner 10s linear infinite; }",
        "    #spinner > div {",
        "        background: #333; opacity: 0; position: absolute; top: 11px; left: -2px; width: 4px; height: 21px; border-radius: 2px;",
        "        -prefix-transform-origin: 50% -11px; -prefix-animation: spinner-spoke 0.75s linear infinite;",
        "    }",
        "    @-prefix-keyframes spinner { 0% { -prefix-transform: rotateZ(0deg); } 100% { -prefix-transform: rotateZ(-360deg); } }",
        "    @-prefix-keyframes spinner-spoke { 0% { opacity: 0; } 5% { opacity: 1; } 70% { opacity: 0; } 100% { opacity: 0; } }",
        "</style>",
        "</body></html>"
    ].join("").replace(/-prefix-/g, vendorPrefix);
}