// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

// In WinJS, we use WinJS.Promise.
// There's no native equivalent for regular JavaScript in the browser, so we implement it here.
// This implementation conforms to Promises/A+, making it compatible with WinJS.Promise.

// Note: There is a standard Promises/A+ test suite, to which this implementation conforms.
// See test\Microsoft.Azure.Zumo.Web.Test\promiseTests

// Declare JSHint globals
/*global setTimeout:false */

(function (exports) {
    "use strict";

    var resolutionState = { success: {}, error: {} },
        bind = function (func, target) { return function () { func.apply(target, arguments); }; }, // Older browsers lack Function.prototype.bind
        isGenericPromise = function (obj) { return obj && (typeof obj.then === "function"); };

    function Promise(init) {
        this._callbackFrames = [];
        this._resolutionState = null;
        this._resolutionValueOrError = null;
        this._resolveSuccess = bind(this._resolveSuccess, this);
        this._resolveError = bind(this._resolveError, this);

        if (init) {
            init(this._resolveSuccess, this._resolveError);
        }
    }

    Promise.prototype.then = function (success, error) {
        var callbackFrame = { success: success, error: error, chainedPromise: new Promise() };

        // If this promise is already resolved, invoke callbacks immediately. Otherwise queue them.
        if (this._resolutionState) {
            this._invokeCallback(callbackFrame);
        } else {
            this._callbackFrames.push(callbackFrame);
        }

        return callbackFrame.chainedPromise;
    };

    Promise.prototype._resolveSuccess = function (val) { this._resolve(resolutionState.success, val); };
    Promise.prototype._resolveError = function (err) { this._resolve(resolutionState.error, err); };

    Promise.prototype._resolve = function (state, valueOrError) {
        if (this._resolutionState) {
            // Can't affect resolution state when already resolved. We silently ignore the request, without throwing an error,
            // to prevent concurrent resolvers from affecting each other during race conditions.
            return;
        }

        this._resolutionState = state;
        this._resolutionValueOrError = valueOrError;

        // Notify all queued callbacks
        for (var i = 0, j = this._callbackFrames.length; i < j; i++) {
            this._invokeCallback(this._callbackFrames[i]);
        }
    };

    Promise.prototype._invokeCallback = function (frame) {
        var callbackToInvoke = this._resolutionState === resolutionState.success ? frame.success : frame.error;
        if (typeof callbackToInvoke === "function") {
            // Call the supplied callback either to transform the result (for success) or to handle the error (for error)
            // The setTimeout ensures handlers are always invoked asynchronosly, even if the promise was already resolved,
            // to avoid callers having to differentiate between sync/async cases
            setTimeout(bind(function () {
                var passthroughValue, passthroughState, callbackDidNotThrow = true;
                try {
                    passthroughValue = callbackToInvoke(this._resolutionValueOrError);
                    passthroughState = resolutionState.success;
                } catch (ex) {
                    callbackDidNotThrow = false;
                    passthroughValue = ex;
                    passthroughState = resolutionState.error;
                }

                if (callbackDidNotThrow && isGenericPromise(passthroughValue)) {
                    // By returning a futher promise from a callback, you can insert it into the chain. This is the basis for composition.
                    // This rule is in the Promises/A+ spec, but not Promises/A.
                    passthroughValue.then(frame.chainedPromise._resolveSuccess, frame.chainedPromise._resolveError);
                } else {
                    frame.chainedPromise._resolve(passthroughState, passthroughValue);
                }
            }, this), 1);
        } else {
            // No callback of the applicable type, so transparently pass existing state/value down the chain
            frame.chainedPromise._resolve(this._resolutionState, this._resolutionValueOrError);
        }
    };

    // -----------
    // Everything from here on is extensions beyond the Promises/A+ spec intended to ease code
    // sharing between WinJS and browser-based Mobile Services apps

    Promise.prototype.done = function (success, error) {
        this.then(success, error).then(null, function(err) {
            // "done" throws any final errors as global uncaught exceptions. The setTimeout
            // ensures the exception won't get caught in the Promises machinery or user code.
            setTimeout(function () { throw new Error(err); }, 1);
        });
        return undefined; // You can't chain onto a .done()
    };

    // Note that we're not implementing any of the static WinJS.Promise.* functions because
    // the Mobile Services client doesn't even expose any static "Promise" object that you
    // could reference static functions on. Developers who want to use any of the WinJS-style
    // static functions (any, join, theneach, ...) can use any Promises/A-compatible library
    // such as when.js.
    //
    // Additionally, we don't implement .cancel() yet because Mobile Services operations don't
    // support cancellation in WinJS yet either. This could be added to both WinJS and Web
    // client libraries in the future.

    exports.Promise = Promise;
})(exports);