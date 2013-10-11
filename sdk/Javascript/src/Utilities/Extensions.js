// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="..\Generated\MobileServices.DevIntellisense.js" />

// Declare JSHint globals
/*global XMLHttpRequest:false */

var Validate = require('Validate');
var Platform = require('Platform');
var _ = exports;

exports.isNull = function (value) {
    /// <summary>
    /// Gets a value indicating whether the provided value is null (or
    /// undefined).
    /// </summary>
    /// <param name="value" type="Object" mayBeNull="true">
    /// The value to check.
    /// </param>
    /// <returns type="Boolean">
    /// A value indicating whether the provided value is null (or undefined).
    /// </returns>
    
    return value === null || value === undefined;
};

exports.isNullOrZero = function (value) {
    /// <summary>
    /// Gets a value indicating whether the provided value is null (or
    /// undefined) or zero / empty string
    /// </summary>
    /// <param name="value" type="Object" mayBeNull="true">
    /// The value to check.
    /// </param>
    /// <returns type="Boolean">
    /// A value indicating whether the provided value is null (or undefined) or zero or empty string.
    /// </returns>

    return value === null || value === undefined || value === 0 || value === '';
};

exports.isNullOrEmpty = function (value) {
    /// <summary>
    /// Gets a value indicating whether the provided value is null (or
    /// undefined) or empty.
    /// </summary>
    /// <param name="value" type="Object" mayBeNull="true">
    /// The value to check.
    /// </param>
    /// <returns type="Boolean">
    /// A value inHdicating whether the provided value is null (or undefined).
    /// </returns>

    return _.isNull(value) || value.length === 0;
};

exports.format = function (message) {
    /// <summary>
    /// Format a string by replacing all of its numbered arguments with
    /// parameters to the method. Arguments are of the form {0}, {1}, ..., like
    /// in .NET.
    /// </summary>
    /// <param name="message" type="string" mayBeNull="false">
    /// The format string for the message.
    /// </param>
    /// <param name="arguments" type="array" optional="true">
    /// A variable number of arguments that can be used to format the message.
    /// </param>
    /// <returns type="string">The formatted string.</returns>

    Validate.isString(message, 'message');

    // Note: There are several flaws in this implementation that we are
    // ignoring for simplicity as it's only used internally.  Examples that
    // could be handled better include:
    //    format('{0} {1}', 'arg') => 'arg {1}'
    //    format('{0} {1}', '{1}', 'abc') => 'abc abc'
    //    format('{0}', '{0}') => <stops responding>

    if (!_.isNullOrEmpty(message) && arguments.length > 1) {
        for (var i = 1; i < arguments.length; i++) {
            var pattern = '{' + (i - 1) + '}';
            while (message.indexOf(pattern) !== -1) {
                message = message.replace(pattern, arguments[i]);
            }
        }
    }

    return message;
};

exports.has = function (value, key) {
    /// <summary>
    /// Determine if an object defines a given property.
    /// </summary>
    /// <param name="value" type="Object">The object to check.</param>
    /// <param name="key" type="String">
    /// The name of the property to check for.
    /// </param>
    /// <returns type="Boolean">
    /// A value indicating whether the object defines the property.
    /// </returns>

    Validate.notNull(key, 'key');
    Validate.isString(key, 'key');

    return !_.isNull(value) && value.hasOwnProperty(key);
};

exports.hasProperty = function (object, properties) {
    /// <summary>
    /// Determines if an object has any of the passed in properties
    /// </summary>
    /// <returns type="boolean">True if it contains any one of the properties
    /// </returns>
    for (var i = 0; i < properties.length; i++) {
        if (_.has(object, properties[i])) {
            return true;
        }
    }
    return false;
};

exports.extend = function extend(target, members) {
    /// <summary>
    /// Extends the target with the members of the members object.
    /// </summary>
    /// <param name="target" type="Object">The target object to extend.</param>
    /// <param name="members" type="Object">The members object to add to the target.</param>
    /// <returns type="Object">The target object extended with the members.
    /// </returns>
    for (var member in members) {
        if (members.hasOwnProperty(member)) {
            target[member] = members[member];
        }
    }
    return target;
};

exports.isObject = function (value) {
    /// <summary>
    /// Determine if a value is an object.
    /// </summary>
    /// <param name="value" type="Object">The value to check.</param>
    /// <returns type="boolean">
    /// True if the value is an object (or null), false othwerise.
    /// </returns>

    return _.isNull(value) || (typeof value === 'object' && !_.isDate(value));
};

exports.isValidId = function (value) {
    /// <summary>
    /// Determine if a value is an acceptable id for use by the mobile service
    /// </summary>
    /// <param name="value" type="Object">The value to check.</param>
    /// <returns type="boolean">
    /// True if the value is a string or number, meeting all criteria, or false othwerise.
    /// </returns>
    if (_.isNullOrZero(value)) {
        return false;
    }

    if (_.isString(value)) {
        // Strings must contain at least one non whitespace character
        if (value.length === 0 || value.length > 255) {
            return false;
        }

        var ex = /[+"/?`\\]|[\u0000-\u001F]|[\u007F-\u009F]|^\.{1,2}$/;
        if (value.match(ex) !== null) {
            return false;
        }

        return true;

    } else if (_.isNumber(value)) {
        return value > 0;
    }

    return false;
}

exports.isString = function (value) {
    /// <summary>
    /// Determine if a value is a string.
    /// </summary>
    /// <param name="value" type="Object">The value to check.</param>
    /// <returns type="boolean">
    /// True if the value is a string (or null), false othwerise.
    /// </returns>

    return _.isNull(value) || (typeof value === 'string');
};

exports.isNumber = function (value) {
    /// <summary>
    /// Determine if a value is a number.
    /// </summary>
    /// <param name="value" type="Object">The value to check.</param>
    /// <returns type="boolean">
    /// True if the value is a number, false othwerise.
    /// </returns>

    return !_.isNull(value) && (typeof value === 'number');
};

exports.isBool = function (value) {
    /// <summary>
    /// Determine if a value is a boolean.
    /// </summary>
    /// <param name="value" type="Object">The value to check.</param>
    /// <returns type="boolean">
    /// True if the value is a boolean, false othwerise.
    /// </returns>
    return !_.isNull(value) && (typeof value == 'boolean');
};

function classOf(value) {
    return Object.prototype.toString.call(value).slice(8, -1).toLowerCase();
}

exports.isDate = function (value) {
    /// <summary>
    /// Determine if a value is a date.
    /// </summary>
    /// <param name="value" type="Object">The value to check.</param>
    /// <returns type="boolean">
    /// True if the value is a date, false othwerise.
    /// </returns>
    return !_.isNull(value) && (classOf(value) == 'date');
};

exports.toJson = function (value) {
    /// <summary>
    /// Convert an object into JSON format.
    /// </summary>
    /// <param name="value" type="Object">The value to convert.</param>
    /// <returns type="String">The value as JSON.</returns>

    return Platform.toJson(value);
};

exports.fromJson = function (value) {
    /// <summary>
    /// Convert an object from JSON format.
    /// </summary>
    /// <param name="value" type="String">The value to convert.</param>
    /// <returns type="Object">The value as an object.</returns>

    Validate.isString(value, 'value');

    // We're wrapping this so we can hook the process and perform custom JSON
    // conversions
    return JSON.parse(
        value,
        function (k, v) {
            // Try to convert the value as a Date
            if (_.isString(v) && !_.isNullOrEmpty(v)) {
                var date = exports.tryParseIsoDateString(v);
                if (!_.isNull(date)) {
                    return date;
                }
            }

            // TODO: Convert geolocations once they're supported
            // TODO: Expose the ability for developers to convert custom types
            
            // Return the original value if we couldn't do anything with it
            return v;
        });
};

exports.createUniqueInstallationId = function () {
    /// <summary>
    /// Create a unique identifier that can be used for the installation of
    /// the current application.
    /// </summary>
    /// <returns type="String">Unique identifier.</returns>

    var pad4 = function (str) { return "0000".substring(str.length) + str; };
    var hex4 = function () { return pad4(Math.floor(Math.random() * 0x10000 /* 65536 */).toString(16)); };

    return (hex4() + hex4() + "-" + hex4() + "-" + hex4() + "-" + hex4() + "-" + hex4() + hex4() + hex4());
};

exports.mapProperties = function (instance, action) {
    /// <summary>
    /// Map a function over the key/value pairs in an instance.
    /// </summary>
    /// <param name="instance" type="Object">
    /// The instance to map over.
    /// </param>
    /// <param name="action" type="function (key, value)">
    /// The action to map over the key/value pairs.
    /// </param>
    /// <returns elementType="object">Mapped results.</returns>

    var results = [];
    if (!_.isNull(instance)) {
        var key = null;
        for (key in instance) {
            results.push(action(key, instance[key]));
        }
    }
    return results;
};

exports.pad = function (value, length, ch) {
    /// <summary>
    /// Pad the a value with a given character until it reaches the desired
    /// length.
    /// </summary>
    /// <param name="value" type="Object">The value to pad.</param>
    /// <param name="length" type="Number">The desired length.</param>
    /// <param name="ch" type="String">The character to pad with.</param>
    /// <returns type="String">The padded string.</returns>

    Validate.notNull(value, 'value');
    Validate.isInteger(length, 'length');
    Validate.isString(ch, 'ch');
    Validate.notNullOrEmpty(ch, 'ch');
    Validate.length(ch, 1, 'ch');

    var text = value.toString();
    while (text.length < length) {
        text = ch + text;
    }
    return text;
};

exports.trimEnd = function (text, ch) {
    /// <summary>
    /// Trim all instance of a given characher from the end of a string.
    /// </summary>
    /// <param name="text" type="String" mayBeNull="false">
    /// The string to trim.
    /// <param name="ch" type="String" mayBeNull="false">
    /// The character to trim.
    /// </param>
    /// <returns type="String">The trimmed string.</returns>

    Validate.isString(text, 'text');
    Validate.notNull(text, 'text');
    Validate.isString(ch, 'ch');
    Validate.notNullOrEmpty('ch', 'ch');
    Validate.length(ch, 1, 'ch');

    var end = text.length - 1;
    while (end >= 0 && text[end] === ch) {
        end--;
    }

    return end >= 0 ?
        text.substr(0, end + 1) :
        '';
};

exports.trimStart = function (text, ch) {
    /// <summary>
    /// Trim all instance of a given characher from the start of a string.
    /// </summary>
    /// <param name="text" type="String" mayBeNull="false">
    /// The string to trim.
    /// </param>
    /// <param name="ch" type="String" mayBeNull="false">
    /// The character to trim.
    /// </param>
    /// <returns type="String">The trimmed string.</returns>

    Validate.isString(text, 'text');
    Validate.notNull(text, 'text');
    Validate.isString(ch, 'ch');
    Validate.notNullOrEmpty(ch, 'ch');
    Validate.length(ch, 1, 'ch');

    var start = 0;
    while (start < text.length && text[start] === ch) {
        start++;
    }

    return start < text.length ?
        text.substr(start, text.length - start) :
        '';
};

exports.compareCaseInsensitive = function (first, second) {
    /// <summary>
    /// Compare two strings for equality while igorning case.
    /// </summary>
    /// <param name="first" type="String">First value.</param>
    /// <param name="second" type="String">Second value.</param>
    /// <returns type="Boolean">Whether the strings are the same.</returns>

    // NOTE: We prefer uppercase on Windows for historical reasons where it was
    // possible to have alphabets where several uppercase characters mapped to
    // the same lowercase character.

    if (_.isString(first) && !_.isNullOrEmpty(first)) {
        first = first.toUpperCase();
    }

    if (_.isString(first) && !_.isNullOrEmpty(second)) {
        second = second.toUpperCase();
    }

    return first === second;
};

/// <field name="url" type="Object">
/// Path specific utilities for working with URIs.
/// </field>
exports.url = {
    /// <field name="separator" type="String">
    /// The path separator character used for combining path segments.
    /// </field>
    separator: '/',

    combinePathSegments: function () {
        /// <summary>
        /// Combine several segments into a path.
        /// </summary>
        /// <param parameterArray="true" elementType="String">
        /// The segments of the path that should be combined.
        /// </param>
        /// <returns type="String">The combined path.</returns>

        // Normalize the segements
        var segments = [];
        var i = 0;
        Validate.notNullOrEmpty(arguments, 'arguments');
        for (i = 0; i < arguments.length; i++) {
            var segment = arguments[i];
            Validate.isString(segment, _.format('argument[{0}]', i));

            if (i !== 0) {
                segment = _.trimStart(segment || '', _.url.separator);
            }
            if (i < arguments.length - 1) {
                segment = _.trimEnd(segment || '', _.url.separator);
            }

            segments.push(segment);
        }

        // Combine the segments
        return segments.reduce(
            function (a, b) { return a + _.url.separator + b; });
    },

    getQueryString: function (parameters) {
        /// <summary>
        /// Converts an Object instance into a query string
        /// </summary>
        /// <param name="parameters" type="Object">The parameters from which to create a query string.</param>
        /// <returns type="String">A query string</returns>
        
        Validate.notNull(parameters, 'parameters');
        Validate.isObject(parameters, 'parameters');

        var pairs = [];
        for (var parameter in parameters) {
            var value = parameters[parameter];
            if (exports.isObject(value)) {
                value = exports.toJson(value);
            }
            pairs.push(encodeURIComponent(parameter) + "=" + encodeURIComponent(value));
        }

        return pairs.join("&");
    },

    combinePathAndQuery: function (path, queryString) {
        /// <summary>
        /// Concatenates the URI query string to the URI path.
        /// </summary>
        /// <param name="path" type="String>The URI path</param>
        /// <param name="queryString" type="String>The query string.</param>
        /// <returns type="String>The concatenated URI path and query string.</returns>
        Validate.notNullOrEmpty(path, 'path');
        Validate.isString(path, 'path');
        if (_.isNullOrEmpty(queryString)) {
            return path;
        }
        Validate.isString(queryString, 'queryString');

        if (path.indexOf('?') >= 0) {
            return path + '&' + exports.trimStart(queryString, '?');
        } else {
            return path + '?' + exports.trimStart(queryString, '?');
        }
    }
};

exports.tryParseIsoDateString = function (text) {
    /// <summary>
    /// Try to parse an ISO date string.
    /// </summary>
    /// <param name="text" type="String">The text to parse.</param>
    /// <returns type="Date">The parsed Date or null.</returns>

    return Platform.tryParseIsoDateString(text);
};

exports.createError = function (exceptionOrMessage, request) {
    /// <summary>
    /// Wrap an error thrown as an exception.
    /// </summary>
    /// <param name="exceptionOrMessage">
    /// The exception or message to throw.
    /// </param>
    /// <param name="request">
    /// The failing request.
    /// </param>
    /// <returns>An object with error details</returns>

    // Create an error object to return
    var error = { message: Platform.getResourceString("Extensions_DefaultErrorMessage") };
    error.toString = function () {
        return error.message;
    };

    if (request) {
        error.request = request;
        if (request.status === 0) {
            // Provide a more helpful message for connection failures
            error.message = Platform.getResourceString("Extensions_ConnectionFailureMessage");
        } else {
            // Try to pull out an error message from the response before
            // defaulting to the status
            try {
                var response = JSON.parse(request.responseText);
                error.message =
                    response.error ||
                    response.description ||
                    request.statusText ||
                    Platform.getResourceString("Extensions_DefaultErrorMessage");
            } catch (ex) {
                error.message =
                    request.statusText ||
                    Platform.getResourceString("Extensions_DefaultErrorMessage");
            }
        }
    } else if (_.isString(exceptionOrMessage) && !_.isNullOrEmpty(exceptionOrMessage)) {
        // If it's a string, just use that as the message
        error.message = exceptionOrMessage;
    } else if (!_.isNull(exceptionOrMessage)) {
        // Otherwise we'll use the object as an exception and leave the
        // default error message
        error.exception = exceptionOrMessage;
    }

    return error;
};
