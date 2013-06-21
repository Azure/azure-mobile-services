// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="..\Generated\MobileServices.DevIntellisense.js" />
/*global $__fileVersion__:false, $__version__:false */

var _ = require('Extensions');
var Validate = require('Validate');
var Promises = require('Promises');
var Resources = require('Resources');
var inMemorySettingStore = {};
if (window.localStorage) {
    inMemorySettingStore = window.localStorage;
}
var bestAvailableTransport = null;
var knownTransports = [ // In order of preference
    require('DirectAjaxTransport'),
    require('IframeTransport')
];
var knownLoginUis = [ // In order of preference
    require('CordovaPopup'),
    require('BrowserPopup')
];

// Matches an ISO date and separates out the fractional part of the seconds
// because IE < 10 has quirks parsing fractional seconds
var isoDateRegex = /^(\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2})(?:\.(\d*))?Z$/;

// Feature-detect IE8's date serializer
var dateSerializerOmitsDecimals = !JSON.stringify(new Date(100)).match(/\.100Z"$/);

exports.async = function async(func) {
    /// <summary>
    /// Wrap a function that takes a callback into platform specific async
    /// operation (i.e., keep using callbacks or switch to promises).
    /// </summary>
    /// <param name="func" type="Function">
    /// An async function with a callback as its last parameter 
    /// </param>
    /// <returns type="Function">
    /// Function that when invoked will return a promise.
    /// </returns>

    return function () {
        // Capture the context of the original call
        var that = this;
        var args = arguments;

        // Create a new promise that will wrap the async call
        return new Promises.Promise(function (complete, error) {

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

    // First ensure our 'WindowsAzure' namespace exists
    var namespaceObject = global.WindowsAzure = global.WindowsAzure || {};
    
    // Now add each of the declarations to the namespace
    for (var key in declarations) {
        if (declarations.hasOwnProperty(key)) {
            namespaceObject[key] = declarations[key];
        }
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

    return inMemorySettingStore[name];
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

    inMemorySettingStore[name] = value;
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

    return getBestTransport().performRequest(request, callback);
};

exports.getUserAgent = function () {
    // Browsers don't allow you to set a custom user-agent in ajax requests. Trying to do so
    // will cause an exception. So we don't.
    return null;
};

exports.getOperatingSystemInfo = function () {
    return {
        name: "--",
        version: "--",
        architecture: "--"
    };
};

exports.getSdkInfo = function () {
    return {
        language: "Web",
        fileVersion: $__fileVersion__
    };
};

exports.login = function (startUri, endUri, callback) {
    // Force logins to go over HTTPS because the runtime is hardcoded to redirect
    // the server flow back to HTTPS, and we need the origin to match.
    var findProtocol = /^[a-z]+:/,
        requiredProtocol = 'https:';
    startUri = startUri.replace(findProtocol, requiredProtocol);
    endUri = endUri.replace(findProtocol, requiredProtocol);

    return getBestProvider(knownLoginUis).login(startUri, endUri, callback);
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
    return JSON.stringify(value, function (key, stringifiedValue) {
        if (dateSerializerOmitsDecimals && this && _.isDate(this[key])) {
            // IE8 doesn't include the decimal part in its serialization of dates
            // For consistency, we extract the non-decimal part from the string
            // representation, and then append the expected decimal part.
            var msec = this[key].getMilliseconds(),
                msecString = String(msec + 1000).substring(1);
            return stringifiedValue.replace(isoDateRegex, function (all, datetime) {
                return datetime + "." + msecString + "Z";
            });
        } else {
            return stringifiedValue;
        }
    });
};

exports.tryParseIsoDateString = function (text) {
    /// <summary>
    /// Try to parse an ISO date string.
    /// </summary>
    /// <param name="text" type="String">The text to parse.</param>
    /// <returns type="Date">The parsed Date or null.</returns>

    Validate.isString(text);

    // Check against a lenient regex
    var matchedDate = isoDateRegex.exec(text);
    if (matchedDate) {
        // IE9 only handles precisely 0 or 3 decimal places when parsing ISO dates,
        // and IE8 doesn't parse them at all. Fortunately, all browsers can handle 
        // 'yyyy/mm/dd hh:MM:ss UTC' (without fractional seconds), so we can rewrite
        // the date to that format, and the apply fractional seconds.
        var dateWithoutFraction = matchedDate[1],
            fraction = matchedDate[2] || "0",
            milliseconds = Math.round(1000 * Number("0." + fraction)); // 6 -> 600, 65 -> 650, etc.
        dateWithoutFraction = dateWithoutFraction
            .replace(/\-/g, "/")   // yyyy-mm-ddThh:mm:ss -> yyyy/mm/ddThh:mm:ss
            .replace("T", " ");    // yyyy/mm/ddThh:mm:ss -> yyyy/mm/dd hh:mm:ss

        // Try and parse - it will return NaN if invalid
        var ticks = Date.parse(dateWithoutFraction + " UTC");
        if (!isNaN(ticks)) {
            return new Date(ticks + milliseconds); // ticks are just milliseconds since 1970/01/01
        }
    }

    // Doesn't look like a date
    return null;
};

exports.getResourceString = function (resourceName) {
    // For now, we'll just always use English
    return Resources["en-US"][resourceName];
};


exports.allowPlatformToMutateOriginal = function (original, updated) {
    // For the Web/HTML client, we don't modify the original object.
    // This is the more typical arrangement for most JavaScript data access.
    return updated;
};

function getBestTransport() {
    // We cache this just because it gets called such a lot
    if (!bestAvailableTransport) {
        bestAvailableTransport = getBestProvider(knownTransports);
    }

    return bestAvailableTransport;
}

function getBestProvider(providers) {
    /// <summary>
    /// Given an array of objects which each have a 'supportsCurrentRuntime' function,
    /// returns the first instance where that function returns true.
    /// </summary>

    for (var i = 0; i < providers.length; i++) {
        if (providers[i].supportsCurrentRuntime()) {
            return providers[i];
        }
    }

    throw new Error("Unsupported browser - no suitable providers are available.");
}