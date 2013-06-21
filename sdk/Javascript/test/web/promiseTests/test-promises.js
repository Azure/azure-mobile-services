// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

// This adapter file tells promises-tests how to construct and resolve our promises. This is needed because
// construction/resolution isn't defined by Promises/A, and each implementation works differently.

var promiseTests = require("promises-aplus-tests"),
    mobileServices = require("../js/MobileServices.Web.Internals"),
    ensureNotNull = function (val) {
        // Like WinJS, we don't support null/undefined errors, so transparently replace usage in tests with some dummy error value
        return val === null || val === undefined ? {} : val;
    },

    adapter = {
        fulfilled: function (value) {
            return mobileServices.Platform.async(function (callback) {
                callback(null, value);
            })();
        },
        rejected: function (error) {
            return mobileServices.Platform.async(function (callback) {
                callback(ensureNotNull(error));
            })();
        },
        pending: function () {
            // Returns a promise that is still waiting, along with resolution callbacks
            var capturedCallback,
                promise = mobileServices.Platform.async(function (callback) {
                    capturedCallback = callback;
                })();
            return {
                promise: promise,
                fulfill: function (val) { capturedCallback(null, val); },
                reject: function (err) { capturedCallback(ensureNotNull(err)); }
            };
        }
    };

promiseTests(adapter, function () { /* Output goes to console */ });