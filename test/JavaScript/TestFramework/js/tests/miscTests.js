﻿// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="../../../ZumoE2ETestAppJs/ZumoE2ETestAppJs/js/MobileServices.js" />
/// <reference path="../testFramework.js" />

function defineMiscTestsNamespace() {
    var tests = [];
    var i;
    var roundTripTableName = 'w8jsRoundTripTable';
    var paramsTableName = 'ParamsTestTable';

    tests.push(new zumo.Test('Filter does not modify client', function (test, done) {
        var client = zumo.getClient();
        var filtered = client.withFilter(function (request, next, callback) {
            throw "This is an error";
        });
        var table = client.getTable(roundTripTableName);
        table.take(1).read().done(function (items) {
            test.addLog('Retrieved data successfully: ', items);
            done(true);
        }, function (err) {
            test.addLog('Unexpected error: ', err);
            done(false);
        });
    }));

    var createLoggingFilter = function () {
        var filter = function (request, next, callback) {
            filter.request = request;
            var clientVersion = request.headers['X-ZUMO-VERSION'];
            if (clientVersion && clientVersion.length) {
                if (clientVersion[clientVersion.length - 1] == ')') {
                    clientVersion = clientVersion.substring(0, clientVersion.length - 1);
                }

                var equalsIndex = clientVersion.lastIndexOf('=');
                if (equalsIndex >= 0) {
                    clientVersion = clientVersion.substring(equalsIndex + 1);
                    zumo.util.globalTestParams[zumo.constants.CLIENT_VERSION_KEY] = clientVersion;
                }
            }

            next(request, function (error, response) {
                filter.error = error;
                filter.response = response;
                if (typeof response.getResponseHeader !== 'undefined' && !error && response) {
                    var serverVersion = response.getResponseHeader('x-zumo-version');
                    if (serverVersion) {
                        zumo.util.globalTestParams[zumo.constants.SERVER_VERSION_KEY] = serverVersion;
                    }
                }

                callback(error, response);
            });
        };

        return filter;
    }

    tests.push(createFilterCaptureTest(true));
    tests.push(createFilterCaptureTest(false));

    tests.push(new zumo.Test('Filter can bypass service', function (test, done) {
        var client = zumo.getClient();
        var filter = function (request, next, callback) {
            var data = JSON.parse(request.data);
            data.id = 1;
            callback(null, {
                status: 201,
                responseText: JSON.stringify(data)
            });
        };
        var filtered = client.withFilter(filter);
        var table = filtered.getTable('TableWhichDoesNotExist');
        var item = { name: 'John Doe', age: 33 };
        table.insert(item).done(function (inserted) {
            var expected = { name: item.name, age: item.age, id: 1 };
            var errors = [];
            if (zumo.util.compare(expected, inserted, errors)) {
                done(true);
            } else {
                for (var error in errors) {
                    test.addLog(error);
                }
                test.addLog('Comparison error. Expected: ', expected, ', actual: ', inserted);
                done(false);
            }
        }, function (err) {
            test.addLog('Error on insert: ', err);
            done(false);
        });
    }));

    tests.push(new zumo.Test('Filter can send multiple requests', function (test, done) {
        var client = zumo.getClient();
        var numberOfRequests = 2 + Math.floor(Math.random() * 4);
        test.addLog('Filter will send ', numberOfRequests, ' requests');
        var filter = function (request, next, callback) {
            var sendRequest = function (requestsSent, error, response) {
                if (requestsSent === numberOfRequests) {
                    test.addLog('Already sent ', numberOfRequests, ' requests, invoking the callback.');
                    callback(error, response);
                } else {
                    test.addLog('Sending the request with index ', requestsSent);
                    next(request, function (nextError, nextResponse) {
                        sendRequest(requestsSent + 1, nextError, nextResponse);
                    });
                }
            }

            sendRequest(0);
        };
        var filtered = client.withFilter(filter);
        var table = filtered.getTable(roundTripTableName);
        var randomValue = Math.floor(Math.random() * 0x100000000).toString(16);
        var item = { string1: randomValue };
        table.insert(item).done(function () {
            table.where({ string1: randomValue }).select('string1').read().done(function (results) {
                var expectedResult = [];
                for (i = 0; i < numberOfRequests; i++) expectedResult.push({ string1: randomValue });
                var errors = [];
                if (zumo.util.compare(expectedResult, results, errors)) {
                    done(true);
                } else {
                    for (var error in errors) {
                        test.addLog(error);
                    }
                    test.addLog('Comparison error. Expected: ', expectedResult, ', actual: ', results);
                    done(false);
                }
            }, function (err) {
                test.addLog('Error on read: ', err);
                done(false);
            });
        }, function (err) {
            test.addLog('Error on insert: ', err);
            done(false);
        });
    }));

    tests.push(new zumo.Test('Passing additional parameters in CRUD operations', function (test, done) {
        var client = zumo.getClient();
        var table = client.getTable(paramsTableName);
        var dict = {
            item: 'simple',
            empty: '',
            spaces: 'with spaces',
            specialChars: '`!@#$%^&*()-=[]\\;\',./~_+{}|:\"<>?',
            latin: 'ãéìôü ÇñÑ',
            arabic: 'الكتاب على الطاولة',
            chinese: '这本书在桌子上',
            japanese: '本は机の上に',
            hebrew: 'הספר הוא על השולחן',
            russian: 'Книга лежит на столе',
            'name+with special&chars': 'should just work'
        };

        var handleError = function (operation) {
            return function (err) {
                test.addLog('Error for ', operation, ': ', err);
                done(false);
            }
        };

        var validateParameters = function (operation, expected, actual) {
            test.addLog('Called ', operation, ', now validating the parameters');
            var errors = [];
            if (zumo.util.compare(expected, actual, errors)) {
                test.addLog('Parameter passing for operation ', operation, ' succeeded');
                return true;
            } else {
                test.addLog('Error validating parameters');
                for (var error in errors) {
                    test.addLog(error);
                }
                test.addLog('Expected: ', expected);
                test.addLog('Actual: ', actual);
                return false;
            }
        }

        var item = { string1: 'hello' };
        var testPassed = true;
        dict.operation = 'insert';
        table.insert(item, dict).done(function (inserted) {
            testPassed = testPassed && validateParameters('insert', dict, JSON.parse(inserted.parameters));
            dict.operation = 'update';
            var id = inserted.id || 1;
            table.update({ id: id, string1: item.string1 }, dict).done(function (updated) {
                testPassed = testPassed && validateParameters('update', dict, JSON.parse(updated.parameters));
                dict.operation = 'lookup';
                table.lookup(id, dict).done(function (retrieved) {
                    testPassed = testPassed && validateParameters('lookup', dict, JSON.parse(retrieved.parameters));
                    dict.operation = 'read';
                    table.read(dict).done(function (retrievedItems) {
                        testPassed = testPassed && validateParameters('read', dict, JSON.parse(retrievedItems[0].parameters));

                        // response to delete operations don't get passed to the callback;
                        // using a filter to check the values.
                        var filter = createLoggingFilter();
                        var filteredTable = client.withFilter(filter).getTable(paramsTableName);
                        dict.operation = 'delete';
                        filteredTable.del({ id: id }, dict).done(function () {
                            var response = JSON.parse(filter.response.responseText);
                            testPassed = testPassed && validateParameters('delete', dict, JSON.parse(response.parameters));
                            done(testPassed);
                        }, handleError('delete'));
                    }, handleError('read'));
                }, handleError('lookup'));
            }, handleError('update'));
        }, handleError('insert'));
    }));

    function createFilterCaptureTest(successfulRequest) {
        var testName = 'Filter can be used to trace request / ' + (successfulRequest ? 'successful' : 'error') + ' response'
        return new zumo.Test(testName, function (test, done) {
            var client = zumo.getClient();
            var filter = createLoggingFilter();
            var filtered = client.withFilter(filter);
            var table = filtered.getTable(roundTripTableName);
            var item = { string1: 'hello world' };
            if (!successfulRequest) {
                item.unsupported = { arr: [1, 3, 4] };
            }
            var expectedRequestBody = JSON.stringify(item);
            var traceAndValidateRequestBody = function () {
                test.addLog('Request from the filter: ', filter.request);
                if (!filter.request || filter.request.data !== expectedRequestBody) {
                    test.addLog('Request body not the expected value');
                    return false;
                } else {
                    return true;
                }
            }
            var traceAndValidateResponse = function () {
                var error = filter.error;
                var response = filter.response;
                test.addLog('Error: ', error);
                test.addLog('Response(' + Object.prototype.toString.call(response) + '): ' + response.status + ' - ' + response.responseText);
                if (successfulRequest) {
                    if (error) {
                        test.addLog('Request should have succeeded, but an error was captured by the filter: ', error);
                        return false;
                    }

                    if (!response) {
                        test.addLog('Request should have succeeded, but no response was captured by the filter');
                        return false;
                    }

                    if (response.status !== 200 && response.status !== 201) {
                        test.addLog('Invalid response status');
                        return false;
                    }
                } else {
                    if (!response) {
                        test.addLog('Request should have failed on the server, but a response was not captured by the filter.');
                        return false;
                    }

                    if (response.status < 400) {
                        test.addLog('Invalid response status');
                        return false;
                    }
                }

                return true;
            }
            table.insert(item).done(function (item) {
                test.addLog('Inserted: ', item);
                if (!traceAndValidateRequestBody()) {
                    done(false);
                    return;
                }

                if (!successfulRequest) {
                    test.addLog('Error, request should have failed');
                    done(false);
                    return;
                }

                if (!traceAndValidateResponse()) {
                    done(false);
                    return;
                }

                done(true);
            }, function (err) {
                test.addLog('Error during insert: ', err);
                if (!traceAndValidateRequestBody()) {
                    test.addLog('Error validating request');
                    done(false);
                    return;
                }

                if (successfulRequest) {
                    test.addLog('Error, request should have succeeded');
                    done(false);
                    return;
                }

                if (!traceAndValidateResponse()) {
                    test.addLog('Error validating response');
                    done(false);
                    return;
                }

                done(true);
            });
        });
    }

    function createBypassingFilter(statusCode, body) {
        return function (request, next, callback) {
            callback();
        }
    }

    return {
        name: 'Misc',
        tests: tests
    };
}

zumo.tests.misc = defineMiscTestsNamespace();
