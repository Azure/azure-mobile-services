/// <reference path="/MobileServicesJavaScriptClient/MobileServices.js" />
/// <reference path="//Microsoft.WinJS.1.0/js/base.js" />

function createZumoNamespace() {
    var TSPassed = 0;
    var TSFailed = 1;
    var TSNotRun = 2;
    var TSRunning = 3;

    function ZumoTest(name, execution) {
        this.name = name;
        this.execution = execution;
        this.status = TSNotRun;
        this.logs = [];
    }

    ZumoTest.prototype.addLog = function (text) {
        this.logs.push(text);
    }

    ZumoTest.prototype.displayText = function () {
        var result = 'Test: ' + this.name + ' (';
        switch (this.status) {
            case TSFailed:
                result = result + 'failed';
                break;
            case TSPassed:
                result = result + 'passed';
                break;
            case TSNotRun:
                result = result + 'not run';
                break;
            case TSRunning:
                result = result + 'running';
                break;
            default:
                result = result + 'unknown';
                break;
        }

        return result + ')';
    }

    ZumoTest.prototype.getLogs = function () {
        return this.displayText() + '\n' + this.logs.join('\n');
    }

    // testDone is a function which will be called (with a bool indicating
    //    pass or fail) when the test is done
    ZumoTest.prototype.runTest = function (testDone) {
        this.execution(this, testDone);
    }

    function ZumoTestGroup(name, tests) {
        this.name = name;
        this.tests = tests || [];
    }

    ZumoTestGroup.prototype.addTest = function (test) {
        this.tests.push(test);
    }

    // testStarted: function(test, testIndex)
    // testDone: function(test, testIndex)
    // groupDone: function(testsPassed, testsFailed)
    ZumoTestGroup.prototype.runTests = function (testStarted, testDone, groupDone) {
        var group = this;
        var passed = 0;
        var failed = 0;
        var runNextTest = function (index) {
            if (index === group.tests.length) {
                if (groupDone) {
                    groupDone(passed, failed);
                }
            } else {
                var testToRun = group.tests[index];

                if (testStarted) {
                    testStarted(testToRun, index);
                }

                testToRun.status = TSRunning;
                testToRun.runTest(function (result) {
                    testToRun.status = result ? TSPassed : TSFailed;
                    if (result) {
                        passed++;
                    } else {
                        failed++;
                    }

                    if (testDone) {
                        testDone(testToRun, index);
                    }

                    testToRun.addLog('Test ' + (result ? 'passed' : 'failed'));
                    runNextTest(index + 1);
                });
            }
        }

        runNextTest(0);
    }

    ZumoTestGroup.prototype.getLogs = function () {
        var result = 'Logs for test group: ' + this.name + '\n';
        result = result + '=================================\n';
        this.tests.forEach(function (test) {
            result = result + test.getLogs() + '\n';
            result = result + '--------------------------\n';
        });

        return result;
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
                client = new Microsoft.WindowsAzure.MobileServices.MobileServiceClient(appUrl, appKey);
                return true;
            } else {
                new Windows.UI.Popups.MessageDialog('Please enter valid application URL and key', 'Error').showAsync();
                return false;
            }
        } else {
            return true;
        }
    }

    function getClient() {
        return client;
    }

    return {
        testGroups: testGroups,
        currentGroup: currentGroup,
        getClient: getClient,
        initializeClient: initializeClient,
        TSPassed: TSPassed,
        TSFailed: TSFailed,
        TSNotRun: TSNotRun,
        TSRunning: TSRunning,
        Test: ZumoTest,
        Group: ZumoTestGroup,
        tests: {}
    };
}

var zumo = createZumoNamespace();
