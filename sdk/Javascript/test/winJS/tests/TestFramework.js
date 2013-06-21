// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

// Define the WinJS interface to the test framework (to wrap some of the more
// awkward bits of WinRT interop).

// Most of the definitions are defined globally with short $foo names so it's
// very easy to write tests.

// Declare JSHint globals
/*global window:false, Microsoft:false, MobileServiceClient:false, WinJS:false */

// Expose formatting
global.$fmt = function (format) {
    /// <summary>
    /// Format a string.
    /// </summary>
    /// <param name="format" type"string">The string format.</param>
    /// <param parameterArray="true" elementType="Object">Arguments.</param>
    /// <returns type="String">The formatted string.</returns>
    var formatter = Microsoft.Azure.Zumo.Win8.Test.WinJS.Formatter;
    switch (arguments.length) {
        case 0:
        case 1:
            return format;
        case 2:
            return formatter.format(format, arguments[1]);
        case 3:
            return formatter.format(format, arguments[1], arguments[2]);
        case 4:
            return formatter.format(format, arguments[1], arguments[2], arguments[3]);
        case 5:
            return formatter.format(format, arguments[1], arguments[2], arguments[3], arguments[4]);
        case 6:
            return formatter.format(format, arguments[1], arguments[2], arguments[3], arguments[4], arguments[5]);
        default:
            throw $fmt('Cannot format {0} arguments!', arguments.length);
    }
};

// Expose assertions
global.$assert = Microsoft.Azure.Zumo.Win8.Test.Assert;

// It'd be nice to add a throws method to $assert, but WinRT objects marshalled
// into WinJS don't allow expando properties...
global.$assertThrows = function (action, verify, message) {
    /// <summary>
    /// Ensure the action fails.
    /// </summary>
    /// <param name="action" type="Function">
    /// The action that should fail when executed.
    /// </param>
    /// <param name="verify" type="Function" optional="true">
    /// A function which can validate the exception.
    /// </param>
    /// <param name="message" type="String">
    /// A helpful error message if the action doesn't fail.
    /// </param>
    /// <remarks>
    /// This is implemented here instead of the static Assert class because
    /// the interop won't allow us to pass/invoke functions across language
    /// boundaries without uglier workarounds.
    /// </remarks>

    var thrown = true;
    try {
        action();
        thrown = false;
    } catch (ex) {
        if (verify) {
            verify(ex);
        }
    }

    if (!thrown) {
        $assert.fail(message || 'Should have failed!');
    }
};

// Define the global test harness (this should really only be used by the
// application activation to pass initialization arguments)
var harness = new Microsoft.Azure.Zumo.Win8.Test.TestHarness();
global.$harness = harness;

// Setup the test interface
harness.reporter = require('TestInterface').Reporter;

global.$testGroup = function (name) {
    /// <summary>
    /// Declare a new test group.
    /// </summary>
    /// <param name="name" type="String">Name of the test group.</param>
    /// <param parameterArray="true" elementType="Object">
    /// The tests (declared via $test) in this test group.  They can also be
    /// provided via group.test()
    /// </param>
    /// <returns type="Microsoft.Azure.Zumo.Win8.Test.TestGroup">
    /// The new test group's fluent wrapper.
    /// </returns>

    // Create the group and add it to the test harness
    var group = new Microsoft.Azure.Zumo.Win8.Test.TestGroup();
    group.name = name;
    harness.groups.append(group);

    // Add the actual Microsoft.Azure.Zumo.Win8.Test.TestMethods to the
    // group by unwrapping them
    for (var i = 1; i < arguments.length; i++) {
        group.methods.append(arguments[i].getTest());
    }

    // Create a simple fluent interface for defining other attributes of the
    // test group
    var wrapper = {
        getGroup: function () {
            /// <summary>
            /// Unwrap the group and return the core
            /// Microsoft.Azure.Zumo.Win8.Test.TestGroup.
            /// </summary>
            /// <returns type="Microsoft.Azure.Zumo.Win8.Test.TestGroup">
            /// The test group.
            /// </returns>
            return group;
        },

        exclude: function (reason) {
            /// <summary>
            /// Mark the group as excluded from this test run.
            /// </summary>
            /// <param name="reason" type="String">
            /// The reason this group was excluded.
            /// </param>
            /// <returns type="Object">The test group wrapper.</returns>

            // Exclude the existing tests
            group.exclude(reason);

            // Cache the reason for excluding so it can be applied to future
            // tests as they are added
            wrapper.excludeReason = reason;

            return wrapper;
        },

        tag: function () {
            /// <summary>
            /// Mark the test group with the given tag.
            /// </summary>
            /// <returns type="Objet">The test group wrapper.</returns>

            for (var i = 0; i < arguments.length; i++) {
                group.tags.append(arguments[i]);
            }

            return wrapper;
        },

        functional: function() {
            /// <summary>
            /// Mark the test group as comprised of functional tests.
            /// </summary>
            /// <returns type="Objet">The test group wrapper.</returns>

            return wrapper.tag('Functional');
        },

        tests: function () {
            /// <summary>
            /// Add tests to the test group.
            /// </summary>
            /// <param parameterArray="true" elementType="Object">
            /// The tests (declared via $test) in this test group.
            /// </param>
            /// <returns type="Microsoft.Azure.Zumo.Win8.Test.TestGroup">
            /// The test group wrapper.
            /// </returns>

            // Add the actual Microsoft.Azure.Zumo.Win8.Test.TestMethods to
            // the group by unwrapping them
            for (var i = 0; i < arguments.length; i++) {
                var test = arguments[i].getTest();
                group.methods.append(test);

                // If the test group was marked as excluded before this test
                // was added, exclude it too
                if (wrapper.excludeReason && !test.excluded) {
                    test.exclude(wrapper.excludeReason);
                }
            }
            return wrapper;
        }
    };

    return wrapper;
};

global.$test = function (name) {
    /// <summary>
    /// Declare a new test method.
    /// </summary>
    /// <param name="name" type="String">The name of the test.</param>
    /// <returns type="Object">
    /// The new test method (wrapped in an object with helpful method for
    /// defining other attributes of the test).
    /// </returns>

    var test = new Microsoft.Azure.Zumo.Win8.Test.TestMethod();
    test.name = name;

    // Create a simple fluent interface for defining other attributes of the
    // test method (including the actual action to test)
    var wrapper = {
        getTest: function () {
            /// <summary>
            /// Unwrap the test and return the core
            /// Microsoft.Azure.Zumo.Win8.Test.TestMethod.
            /// </summary>
            /// <returns type="Microsoft.Azure.Zumo.Win8.Test.TestMethod">
            /// The test method.
            /// </returns>
            return test;
        },

        description: function (description) {
            /// <summary>
            /// Set the description of the test method.
            /// </summary>
            /// <param name="description" type="String">
            /// A description of the test method.
            /// </param>
            /// <returns type="Object">The test method wrapper.</returns>
            test.description = description;
            return wrapper;
        },

        exclude: function (reason) {
            /// <summary>
            /// Mark the test method as excluded from this test run.
            /// </summary>
            /// <param name="reason" type="String">
            /// The reason this test was excluded.
            /// </param>
            /// <returns type="Object">The test method wrapper.</returns>
            test.exclude(reason);
            return wrapper;
        },

        functional: function () {
            /// <summary>
            /// Mark the test method as a functional test.
            /// </summary>
            /// <returns type="Objet">The test group wrapper.</returns>

            return wrapper.tag('Functional');
        },

        tag: function (tag) {
            /// <summary>
            /// Mark the test method with the given tag.
            /// </summary>
            /// <returns type="Objet">The test group wrapper.</returns>

            for (var i = 0; i < arguments.length; i++) {
                test.tags.append(arguments[i]);                
            }

            return wrapper;
        },

        check: function (action) {
            /// <summary>
            /// Provide the actual test method to invoke.
            /// </summary>
            /// <param name="action" type="Function">
            /// The action to invoke which will perform the tests.
            /// </param>
            /// <returns type="Object">The test method wrapper.</returns>
            
            // We have to wrap the test in an async operation so it can be
            // invoked from C#.
            var delegate = new Microsoft.Azure.Zumo.Win8.Test.WinJS.PromiseAsyncExecution();
            delegate.onexecute = function (args) {
                try {
                    // Invoke the action and return control to the test
                    // framework when complete
                    action();
                    window.setTimeout(function () { args.complete(); }, 0);
                } catch (e) {
                    window.setTimeout(function () { args.error(e.toString()); }, 0);
                }
            };
            test.test = delegate;
            return wrapper;
        },
        checkAsync: function (getPromise) {
            /// <summary>
            /// Provide the actual async test method to invoke.
            /// </summary>
            /// <param name="getPromise" type="Function">
            /// A function which when invoked will return a new async Promise
            /// which the test harness can wait to complete before continuing.
            /// </param>
            /// <returns type="Object">The test method wrapper.</returns>

            var delegate = new Microsoft.Azure.Zumo.Win8.Test.WinJS.PromiseAsyncExecution();
            delegate.onexecute = function (args) {
                // Get the promise (and start the async operation) and return
                // control to the test harness when it's finished.
                getPromise().done(
                    function () { args.complete(); },
                    function (err) { args.error(err.toString()); });
            };
            test.test = delegate;
            return wrapper;
        }
    };

    return wrapper;
};

global.$log = function (message) {
    /// <summary>
    /// Log a message.
    /// </summary>
    /// <param name="message" type="String">The message.</param>

    harness.log(message);
};

global.$run = function () {
    /// <summary>
    /// Start executing the tests.
    /// </summary>
    harness.runAsync();
};

global.$getClient = function () {
    /// <summary>
    /// Create a new client for functional tests.
    /// </summary>
    /// <returns type="MobileServiceClient">MobileServiceClient</returns>

    global.$assert.isTrue(
        global.$harness.settings.custom.MobileServiceRuntimeUrl,
        '$getClient should only be called from functional tests!');
    return new WindowsAzure.MobileServiceClient(global.$harness.settings.custom.MobileServiceRuntimeUrl);
};

global.$chain = function () {
    /// <summary>
    /// Given a list of functions that return promises, return a promise that
    /// will chain them all together sequentially.
    /// </summary>
    var actions = Array.prototype.slice.call(arguments);
    return new WinJS.Promise(function (complete, err) {
        var error = function (ex) {
            err(ex.responseText || JSON.stringify(ex));
        };
        var exec = function (prev) {
            if (actions.length > 0) {
                var next = actions.shift();
                try {
                    var result = next(prev);
                    if (result) {
                        result.then(function (results) {
                            exec(results);
                        }, error);
                    } else if (actions.length === 0) {
                        complete();
                    } else {
                        error('$chain expects all actions except the last to return another Promise');
                    }
                } catch (ex) {
                    error(ex);
                }
            } else {
                complete();
            }
        };
        exec();
    });
};