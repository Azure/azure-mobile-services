// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="//Microsoft.WinJS.1.0/js/base.js" />
/// <reference path="../../ZumoE2ETestAppJs/ZumoE2ETestAppJs/js/MobileServices.js" />
/// <reference path="../../ZumoE2EHTMLApp/ZumoE2EHTMLApp/../../TestFramework/js/platformSpecificFunctions.js" />

function createZumoNamespace() {
    var TSPassed = 0;
    var TSFailed = 1;
    var TSNotRun = 2;
    var TSRunning = 3;
    var TSSkipped = 4;
    var AllTestsGroupName = "All tests";
    var AllTestsUnattendedGroupName = AllTestsGroupName + ' (unattended)';
    var ClientVersionKey = 'client-version';
    var ServerVersionKey = 'server-version';
    var RuntimeFeaturesKey = 'runtime-features';

    function ZumoTest(name, execution, requiredRuntimeFeatures) {
        /// <summary>
        /// Creates a new test object.
        /// </summary>
        /// <param name="name" type="String">The name of the test.</param>
        /// <param name="execution" type="function(test, done)">The function to be called when the
        ///   test needs to be executed. The function is passed two parameters: the test itself, and
        ///   a callback function to be called when the test is done. The callback function should be
        ///   passed <code>true</code> if the test passed; <code>false</code> if the test failed, or
        ///   <code>null</code> if the test should be skipped.</param>
        /// <param name="requiredRuntimeFeatures" optional="true">Any runtime features required for this
        ///   test to run. Can be a single feature name, or an array of feature names.</param>
        this.name = name;
        this.execution = execution;
        this.status = TSNotRun;
        this.canRunUnattended = true;
        this.logs = [];
        this.requiredFeatures = [];
        if (requiredRuntimeFeatures) {
            if (typeof requiredRuntimeFeatures === 'string') {
                requiredRuntimeFeatures = [requiredRuntimeFeatures];
            }
            var that = this;
            requiredRuntimeFeatures.forEach(function (feature) {
                that.requiredFeatures.push(feature);
            });
        }
    }

    ZumoTest.prototype.addFullLog = function (text, args) {
        /// <summary>
        /// Adds a new log entry to the test
        /// </summary>
        /// <param name="text" type="String">The text to be added to the log</param>
        /// <param name="args" optional="true">Any additional arguments, which will be
        ///       JSON.stringify'ed and concatenated with the text.</param>
        for (var i = 1; i < arguments.length; i++) {
            var arg = arguments[i];
            if (typeof arg === 'string') {
                text = text + arg;
            } else {
                text = text + JSON.stringify(arg);
            }
        }
        text = addTimestamp(text);
        this.logs.push(text);
    }

    function addTimestamp(text) {
        var now = new Date();
        text = '[' + dateToString(now) + '] ' + text;
        return text;
    }

    ZumoTest.prototype.shouldBeSkipped = function () {
        /// <summary>
        /// Determines whether this test should be skipped, given the features enabled in the runtime.
        /// </summary>
        /// <remarks>
        /// If the test client does not have any runtime information, the test will not be skipped;
        ///   if the runtime info is present, then if there are any required features in the test which are
        ///   not enabled in the runtime, the test will be skipped.
        /// </remarks>
        /// <returns><code>true</code> if the test should be skipped; <code>false</code> otherwise.</returns>
        var runtimeFeatures = zumo.util.globalTestParams[zumo.constants.RUNTIME_FEATURES_KEY];
        if (!runtimeFeatures) {
            return false;
        }
        for (var i = 0; i < this.requiredFeatures.length; i++) {
            var isEnabled = runtimeFeatures[this.requiredFeatures[i]];
            if (!isEnabled) {
                return true;
            }
        }
        return false;
    }

    ZumoTest.prototype.addRequiredFeature = function (featureName) {
        /// <summary>
        /// Determines that this test can only run if the runtime with which
        /// the client is talking to has the given feature enabled.
        /// </summary>
        /// <param name="featureName" type="String">The name of the feature required by this test.</param>
        this.requiredFeatures.push(featureName);
    }

    ZumoTest.prototype.addLog = function (text, args) {
        /// <summary>
        /// Adds a new log entry to the test, capped at a maximum of 500 characters
        /// </summary>
        /// <param name="text" type="String">The text to be added to the log</param>
        /// <param name="args" optional="true">Any additional arguments, which will be
        ///       JSON.stringify'ed and concatenated with the text.</param>
        for (var i = 1; i < arguments.length; i++) {
            var arg = arguments[i];
            if (typeof arg === 'string') {
                text = text + arg;
            } else {
                text = text + JSON.stringify(arg);
            }
        }
        text = addTimestamp(text);

        if (text.length > 500) {
            text = text.substring(0, 500) + '... (truncated)';
        }

        this.logs.push(text);
    }

    ZumoTest.prototype.displayText = function () {
        var result = 'Test: ' + this.name + ' (';
        result = result + this.statusText() + ')';
        return result;
    }

    ZumoTest.prototype.statusText = function () {
        switch (this.status) {
            case TSFailed:
                return 'failed';
            case TSPassed:
                return 'passed';
            case TSNotRun:
                return 'not run';
            case TSRunning:
                return 'running';
            case TSSkipped:
                return 'skipped';
            default:
                return 'unknown';
        }
    }

    ZumoTest.prototype.getLogs = function () {
        return this.displayText() + '\n' + this.logs.join('\n');
    }

    // testDone is a function which will be called (with a boolean value indicating
    //    pass or fail) when the test is done. If the value is null (or undefined),
    //    the test is considered to be skipped.
    ZumoTest.prototype.runTest = function (testDone) {
        var that = this;
        that.startTime = new Date();
        if (that.shouldBeSkipped()) {
            that.endTime = new Date();
            testDone(null);
        } else {
            that.execution(this, function (passed) {
                that.endTime = new Date();
                testDone(passed)
            });
        }
    }

    ZumoTest.prototype.reset = function () {
        this.status = TSNotRun;
        this.startTime = null;
        this.endTime = null;
        this.logs = [];
    }

    function ZumoTestGroup(name, tests) {
        this.name = name;
        this.startTime = null;
        this.endTime = null;
        this.tests = tests || [];
    }

    ZumoTestGroup.prototype.addTest = function (test) {
        /// <summary>
        /// Adds a new test to this group.
        /// </summary>
        /// <param name="test" type="zumo.Test">The test to be added.</param>
        this.tests.push(test);
    }

    // testStarted: function(test, testIndex)
    // testDone: function(test, testIndex)
    // groupDone: function(testsPassed, testsFailed, testsSkipped)
    ZumoTestGroup.prototype.runTests = function (testStarted, testDone, groupDone) {
        var group = this;
        var passed = 0;
        var failed = 0;
        var skipped = 0;
        this.startTime = new Date();
        var that = this;
        var runNextTest = function (index) {
            if (index === group.tests.length) {
                that.endTime = new Date();
                if (groupDone) {
                    groupDone(passed, failed, skipped);
                }
            } else {
                var testToRun = group.tests[index];

                if (testStarted) {
                    testStarted(testToRun, index);
                }

                testToRun.status = TSRunning;
                try {
                    testToRun.runTest(function (result) {
                        var isSkipped = result === null || result === undefined;
                        if (isSkipped) {
                            testToRun.status = TSSkipped;
                            skipped++;
                        } else {
                            testToRun.status = result ? TSPassed : TSFailed;
                            if (result) {
                                passed++;
                            } else {
                                failed++;
                            }
                        }

                        if (testDone) {
                            testDone(testToRun, index);
                        }

                        testToRun.addLog('Test ' + testToRun.statusText());
                        runNextTest(index + 1);
                    });
                } catch (ex) {
                    testToRun.addLog('Caught exception running test: ' + JSON.stringify(ex));
                    testToRun.status = TSFailed;
                    failed++;
                    testToRun.addLog('Test failed');
                    if (testDone) {
                        testDone(testToRun, index);
                    }
                    runNextTest(index + 1);
                }
            }
        }

        runNextTest(0);
    }

    ZumoTestGroup.prototype.getLogs = function () {
        var lines = [];
        lines.push('[' + dateToString(this.startTime) + '] Tests for group \'' + this.name + '\'');
        lines.push('----------------------------');
        this.tests.forEach(function (test) {
            if (test.status !== TSSkipped) {
                lines.push('[' + dateToString(test.startTime) + '] Logs for test ' + test.name + ' (' + test.statusText() + ')');
                lines.push(test.getLogs());
                lines.push('[' + dateToString(test.endTime) + '] -*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-');
            }
        });

        return lines.join('\n');
    }

    var client = null;
    var testGroups = [];
    var currentGroup = -1;

    // Returns 'true' if the initialization was successful; false otherwise
    function initializeClient(appUrl, appKey) {
        var mustInitialize = true;
        if (client && client.applicationUrl === appUrl && client.applicationKey === appKey) {
            mustInitialize = false;
        }
        
        if (mustInitialize) {
            if (appUrl && appKey) { 
                client = new WindowsAzure.MobileServiceClient(appUrl, appKey);
                return true;
            } else {
                testPlatform.alert('Please enter valid application URL and key', 'Error', function () {
                    return false;
                });
            }
        } else {
            return true;
        }
    }

    function getClient() {
        /// <summary>
        /// Returns the shared MobileServiceClient instance.
        /// </summary>
        /// <returns type="Microsoft.WindowsAzure.MobileServices.MobileServiceClient">
        /// The shared cliens instance.
        /// </returns>
        return client;
    }
    function getIEBrowserVersion() {
        // Get Version of IE browser
        var rv = -1;
        if (navigator.appName == 'Microsoft Internet Explorer') {
            var ua = navigator.userAgent;
            var re = new RegExp("MSIE ([0-9]{1,}[\.0-9]{0,})");
            if (re.exec(ua) != null)
                rv = parseFloat(RegExp.$1);
        }
        return rv;
    }

    function compareValues(expected, actual, errors) {
        var i, key;
        if (expected === actual) {
            // easy case
            return true;
        }

        if ((expected === null) != (actual === null)) {
            return false;
        }

        if (expected === null) {
            return true;
        }

        function getValueType(value) {
            var result = typeof value;
            if (result === 'object') {
                var objType = Object.prototype.toString.call(value);
                if (objType === '[object Date]') {
                    result = 'date';
                } else if (objType === '[object Array]') {
                    result = 'array';
                }
            }

            return result;
        }

        var expectedType = getValueType(expected);
        var actualType = getValueType(actual);
        if (expectedType !== actualType) {
            if (errors && errors.push) {
                errors.push('Different types: expected: ' + expectedType + ', actual: ' + actualType);
                return false;
            }
        }

        switch (expectedType) {
            case 'boolean':
            case 'string':
                // not the same, comparison done in easy case
                if (errors && errors.push) {
                    errors.push('Expected: ' + expected + ', actual: ' + actual);
                }
                return false;
            case 'date':
                var dExpected = expected.getTime();
                var dActual = actual.getTime();
                if (dExpected !== dActual) {
                    if (errors && errors.push) {
                        errors.push('Expected: ' + expected + '(' + dExpected + '), actual: ' + actual + '(' + dActual + ')');
                    }
                    return false;
                } else {
                    return true;
                }
            case 'number':
                var acceptableDelta = 1e-8;
                var delta = 1 - expected / actual;
                if (Math.abs(delta) > acceptableDelta) {
                    if (errors && errors.push) {
                        errors.push('Numbers differ by more than the allowed difference: ' + expected + ' - ' + actual);
                    }
                    return false;
                } else {
                    return true;
                }
            case 'array':
                if (expected.length !== actual.length) {
                    if (errors && errors.push) {
                        errors.push('Size of arrays are different: ' + expected.length + ' - ' + actual.length);
                    }
                    return false;
                }

                for (i = 0; i < expected.length; i++) {
                    if (!compareValues(expected[i], actual[i], errors)) {
                        if (errors && errors.push) {
                            errors.push('Difference in array at index ' + i);
                        }
                        return false;
                    }
                }

                return true;
            case 'object':
                for (key in expected) {
                    if (expected.hasOwnProperty(key)) {
                        var actualValue = actual[key];
                        var expectedValue = expected[key];
                        if (expectedValue === undefined) {
                            if (errors && errors.push) {
                                errors.push('Expected object has member with key ' + key + ', actual does not.');
                            }
                            return false;
                        }

                        if (!compareValues(expectedValue, actualValue, errors)) {
                            if (errors && errors.push) {
                                errors.push('Difference in object member with key: ' + key);
                            }
                            return false;
                        }
                    }
                }

                return true;
            default:
                if (errors && errors.push) {
                    errors.push('Don\'t know how to compare object with type ' + expectedType);
                }
                return false;
        }
    }

    function traceResponse(test, xhr) {
        if (xhr) {
            test.addLog('Response info:');
            test.addLog('  Status code: ' + xhr.status);

            if (xhr.getAllResponseHeaders) {
                test.addLog('  Headers: ' + xhr.getAllResponseHeaders());
            }

            test.addLog('  Body: ' + xhr.responseText);
        } else {
            test.addLog('No XMLHttpRequest information');
        }
    }

    function createSeparatorTest(testName) {
        /// <summary>
        /// Creates a test which doesn't do anything, used only to separate groups of tests
        /// </summary>
        /// <param name="name">The test name.</param>
        /// <returns>A test which always passes without doing anything.</returns>
        return new zumo.Test(testName, function (test, done) {
            done(true);
        });
    }

    function dateToString(date) {
        /// <param name="date" type="Date">The date to convert to string</param>

        function padLeft0(number, size) {
            number = number.toString();
            while (number.length < size) number = '0' + number;
            return number;
        }

        date = date || new Date(Date.UTC(1900, 0, 1, 0, 0, 0, 0));

        var result =
            padLeft0(date.getUTCFullYear(), 4) + '-' +
            padLeft0(date.getUTCMonth() + 1, 2) + '-' +
            padLeft0(date.getUTCDate(), 2) + ' ' +
            padLeft0(date.getUTCHours(), 2) + ':' +
            padLeft0(date.getUTCMinutes(), 2) + ':' +
            padLeft0(date.getUTCSeconds(), 2) + '.' +
            padLeft0(date.getUTCMilliseconds(), 3);

        return result;
    }

    function randomDate() {
        var year = Math.floor(Math.random() * 9999) + 1;
        var month = Math.floor(Math.random() * 12);
        var day = Math.floor(Math.random() * 31);
        var hour = Math.floor(Math.random() * 24);
        var minute = Math.floor(Math.random() * 60);
        var second = Math.floor(Math.random() * 60);
        var milliseconds = Math.floor(Math.random() * 100);
        return new Date(Date.UTC(year, month, day, hour, minute, second, milliseconds));
    }

    return {
        testGroups: testGroups,
        currentGroup: currentGroup,
        getClient: getClient,
        getIEBrowserVersion: getIEBrowserVersion,
        initializeClient: initializeClient,
        TSPassed: TSPassed,
        TSFailed: TSFailed,
        TSNotRun: TSNotRun,
        TSRunning: TSRunning,
        TSSkipped: TSSkipped,
        AllTestsGroupName: AllTestsGroupName,
        AllTestsUnattendedGroupName: AllTestsUnattendedGroupName,
        constants: {
            CLIENT_VERSION_KEY: ClientVersionKey,
            SERVER_VERSION_KEY: ServerVersionKey,
            RUNTIME_FEATURES_KEY: RuntimeFeaturesKey
        },
        runtimeFeatureNames: {
            INT_ID_TABLES: 'intIdTables',
            STRING_ID_TABLES: 'stringIdTables',
            NH_PUSH_ENABLED: 'nhPushEnabled'
        },
        Test: ZumoTest,
        Group: ZumoTestGroup,
        tests: {},
        util: {
            createSeparatorTest: createSeparatorTest,
            compare: compareValues,
            traceResponse: traceResponse,
            dateToString: dateToString,
            randomDate: randomDate,
            globalTestParams: {}
        }
    };
}

var zumo = createZumoNamespace();
