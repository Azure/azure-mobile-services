// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="..\Generated\MobileServices.DevIntellisense.js" />

// Declare JSHint globals
/*global WinJS:false, Windows:false, $__fileVersion__:false, $__version__:false */

var _ = require('Extensions');
var Validate = require('Validate');

exports.async = function async(func) {
    /// <summary>
    /// Wrap a function that takes a callback into platform specific async
    /// operation (i.e., keep using callbacks or switch to promises).
    /// </summary>
    /// <param name="func" type="Function">
    /// An async function with a callback as its last parameter 
    /// </param>
    /// <returns type="Function">
    /// Function that when invoked will return a WinJS.Promise.
    /// </returns>

    return function () {
        // Capture the context of the original call
        var that = this;
        var args = arguments;

        // Create a new promise that will wrap the async call
        return new WinJS.Promise(function (complete, error) {

            // Add a callback to the args which will call the appropriate
            // promise handlers
            var callback = function (err) {
                if (_.isNull(err)) {
                    // Call complete with all the args except for err
                    complete.apply(null, Array.prototype.slice.call(arguments, 1));
                } else {
                    error(err);
                }
            };
            Array.prototype.push.call(args, callback);

            try {
                // Invoke the async method which will in turn invoke our callback
                // which will in turn invoke the promise's handlers
                func.apply(that, args);
            } catch (ex) {
                // Thread any immediate errors like parameter validation
                // through the the callback
                callback(_.createError(ex));
            }
        });
    };
};

exports.addToMobileServicesClientNamespace = function (declarations) {
    /// <summary>
    /// Define a collection of declarations in the Mobile Services Client namespace.
    /// </summary>
    /// <param name="declarations" type="Object">
    /// Object consisting of names and values to define in the namespace.
    /// </param>

    try {
        // The following namespace is retained for backward compatibility, but
        // may soon change to 'WindowsAzure'
        WinJS.Namespace.define('WindowsAzure', declarations);
    } catch (ex) {
        // This can fail due to a WinRT issue where another assembly defining a
        // non-JavaScript type with a Microsoft namespace.  The wrapper object
        // created to represent the namespace doesn't allow expando
        // properties...  so it will never let any additional JavaScript
        // namespaces be defined under it.  We only see this with our test
        // library at the moment, but it could also appear if customers are
        // using other Microsoft libraries in WinJS.        
    }
};

exports.readSetting = function readSetting(name) {
    /// <summary>
    /// Read a setting from a global configuration store.
    /// </summary>
    /// <param name="name" type="String">
    /// Name of the setting to read.
    /// </param>
    /// <returns type="String" mayBeNull="true">
    /// The value of the setting or null if not set.
    /// </returns>

    var localSettings = Windows.Storage.ApplicationData.current.localSettings;
    return !_.isNull(localSettings) ?
        localSettings.values[name] :
        null;
};

exports.writeSetting = function writeSetting(name, value) {
    /// <summary>
    /// Write a setting to a global configuration store.
    /// </summary>
    /// <param name="name" type="String">
    /// Name of the setting to write.
    /// </param>
    /// <param name="value" type="String" mayBeNull="true">
    /// The value of the setting.
    /// </returns>

    var localSettings = Windows.Storage.ApplicationData.current.localSettings;
    if (!_.isNull(localSettings)) {
        localSettings.values[name] = value;
    }
};

exports.webRequest = function (request, callback) {
    /// <summary>
    /// Make a web request.
    /// </summary>
    /// <param name="request" type="Object">
    /// Object describing the request (in the WinJS.xhr format).
    /// </param>
    /// <param name="callback" type="Function">
    /// The callback to execute when the request completes.
    /// </param>

    WinJS.xhr(request).done(
        function (response) { callback(null, response); },
        function (error) { callback(null, error); });
};

exports.login = function (startUri, endUri, callback) {
    /// <summary>
    /// Log a user into a Mobile Services application by launching a
    /// browser-based control that will allow the user to enter their credentials
    /// with a given provider.
    /// </summary>
    /// <param name="startUri" type="string">
    /// The absolute URI to which the login control should first navigate to in order to
    /// start the login process flow.
    /// </param>
    /// <param name="endUri" type="string" mayBeNull="true">
    /// The absolute URI that indicates login is complete. Once the login control navigates
    /// to this URI, it will execute the callback.
    /// </param>
    /// <param name="callback" type="Function" mayBeNull="true">
    /// The callback to execute when the login completes: callback(error, endUri).
    /// </param>

    // Account for absent optional arguments
    if (_.isNull(callback) && typeof endUri === 'function') {
        callback = endUri;
        endUri = null;
    }

    Validate.notNullOrEmpty(startUri, 'startUri');
    Validate.isString(startUri, 'startUri');

    var windowsWebAuthBroker = Windows.Security.Authentication.Web.WebAuthenticationBroker;
    var noneWebAuthOptions = Windows.Security.Authentication.Web.WebAuthenticationOptions.none;
    var successWebAuthStatus = Windows.Security.Authentication.Web.WebAuthenticationStatus.success;

    var webAuthBrokerSuccessCallback = null;
    var webAuthBrokerErrorCallback = null;
    if (!_.isNull(callback)) {
        webAuthBrokerSuccessCallback = function (result) {
            var error = null;
            var token = null;

            if (result.responseStatus !== successWebAuthStatus) {
                error = result;
            }
            else {
                var callbackEndUri = result.responseData;
                var tokenAsJson = null;
                if (_.isNull(error)) {
                    var i = callbackEndUri.indexOf('#token=');
                    if (i > 0) {
                        tokenAsJson = decodeURIComponent(callbackEndUri.substring(i + 7));
                    }
                    else {
                        i = callbackEndUri.indexOf('#error=');
                        if (i > 0) {
                            error = decodeURIComponent(callbackEndUri.substring(i + 7));
                        }
                    }
                }

                if (!_.isNull(tokenAsJson)) {
                    try {
                        token = _.fromJson(tokenAsJson);
                    }
                    catch (e) {
                        error = e;
                    }
                }
            }

            callback(error, token);
        };

        webAuthBrokerErrorCallback = function (error) {
            callback(error, null);
        };
    }

    if (!_.isNull(endUri)) {
        var windowsStartUri = new Windows.Foundation.Uri(startUri);
        var windowsEndUri = new Windows.Foundation.Uri(endUri);
        windowsWebAuthBroker.authenticateAsync(noneWebAuthOptions, windowsStartUri, windowsEndUri)
                            .done(webAuthBrokerSuccessCallback, webAuthBrokerErrorCallback);
    }
    else {
        // If no endURI was given, then we'll use the single sign-on overload of the 
        // windowsWebAuthBroker. Single sign-on requires that the application's Package SID 
        // be registered with the Windows Azure Mobile Service, but it provides a better 
        // experience as HTTP cookies are supported so that users do not have to
        // login in everytime the application is launched.
        var redirectUri = windowsWebAuthBroker.getCurrentApplicationCallbackUri().absoluteUri;
        var startUriWithRedirect = startUri + "?sso_end_uri=" + encodeURIComponent(redirectUri);
        var windowsStartUriWithRedirect = new Windows.Foundation.Uri(startUriWithRedirect);
        windowsWebAuthBroker.authenticateAsync(noneWebAuthOptions, windowsStartUriWithRedirect)
                            .done(webAuthBrokerSuccessCallback, webAuthBrokerErrorCallback);
    }
};

exports.getOperatingSystemInfo = function () {

    var architecture = "Unknown";

    // The Windows API provides the architecture as an enum, so we have to 
    // lookup the string value
    var archValue = Windows.ApplicationModel.Package.current.id.architecture;
    switch (archValue) {
        case 0: architecture = "X86"; break;
        case 5: architecture = "Arm"; break;
        case 9: architecture = "X64"; break;
        case 11: architecture = "Neutral"; break;
    }

    return {
        name: "Windows 8",
        version: "--",
        architecture: architecture
    };
};

exports.getSdkInfo = function () {
    return {
        language: "WinJS",
        fileVersion: $__fileVersion__        
    };
};

exports.getUserAgent = function () {
    // The User-Agent header can not be set in WinJS
    return null;
};

exports.toJson = function (value) {
    /// <summary>
    /// Convert an object into JSON format.
    /// </summary>
    /// <param name="value" type="Object">The value to convert.</param>
    /// <returns type="String">The value as JSON.</returns>

    // We're wrapping this so we can hook the process and perform custom JSON
    // conversions.  Note that we don't have to add a special hook to correctly
    // serialize dates in ISO8061 because JSON.stringify does that by defualt.
    // TODO: Convert geolocations once they're supported
    // TODO: Expose the ability for developers to convert custom types
    return JSON.stringify(value);
};

exports.tryParseIsoDateString = function (text) {
    /// <summary>
    /// Try to parse an ISO date string.
    /// </summary>
    /// <param name="text" type="String">The text to parse.</param>
    /// <returns type="Date">The parsed Date or null.</returns>

    Validate.isString(text);

    // Check against a lenient regex
    if (/^(\d{4})-(\d{2})-(\d{2})T(\d{2})\:(\d{2})\:(\d{2})(\.(\d{1,3}))?Z$/.test(text)) {
        // Try and parse - it will return NaN if invalid
        var ticks = Date.parse(text);
        if (!isNaN(ticks)) {
            // Convert to a regular Date
            return new Date(ticks);
        }
    }

    // Return null if not found
    return null;
};

exports.getResourceString = function (resourceName) {
    var resourceManager = Windows.ApplicationModel.Resources.Core.ResourceManager.current;
    var resource = resourceManager.mainResourceMap.getValue("MobileServices/Resources/" + resourceName);
    return resource.valueAsString;
};

exports.allowPlatformToMutateOriginal = function (original, updated) {
    /// <summary>
    /// Patch an object with the values returned by from the server.  Given
    /// that it's possible for the server to change values on an insert/update,
    /// we want to make sure the client object reflects those changes.
    /// </summary>
    /// <param name="original" type="Object">The original value.</param>
    /// <param name="updated" type="Object">The updated value.</param>
    /// <returns type="Object">The patched original object.</returns>
    if (!_.isNull(original) && !_.isNull(updated)) {
        var key = null;
        var binding = WinJS.Binding.as(original);

        for (key in updated) {
            if (key in original) {
                binding[key] = updated[key];
            } else {
                binding.addProperty(key, updated[key]);
            }
        }

        // TODO: Should we also delete any fields on the original object that
        // aren't also on the updated object?  Is that a scenario for scripts?
    }

    return original;
};