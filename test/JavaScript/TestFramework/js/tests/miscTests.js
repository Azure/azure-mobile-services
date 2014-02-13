// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="../../../ZumoE2ETestAppJs/ZumoE2ETestAppJs/js/MobileServices.js" />
/// <reference path="../testFramework.js" />

function defineMiscTestsNamespace() {
    var tests = [];
    var i;
    var roundTripTableName = 'w8jsRoundTripTable';
    var paramsTableName = 'ParamsTestTable';
    var stringIdTableName = 'stringIdRoundTripTable';

    tests.push(new zumo.Test('Filter does not modify client', function (test, done) {
        var client = zumo.getClient();
        var filtered = client.withFilter(function (request, next, callback) {
            throw "This is an error";
        });
        var table = client.getTable(stringIdTableName);
        table.take(1).read().done(function (items) {
            test.addLog('Retrieved data successfully: ', items);
            done(true);
        }, function (err) {
            test.addLog('Unexpected error: ', err);
            done(false);
        });
    }, zumo.runtimeFeatureNames.STRING_ID_TABLES));

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
                if (!error && response && response.getResponseHeader) { // IE9 not support response.getResponseHeader
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
        var table = filtered.getTable(stringIdTableName);
        var randomValue = Math.floor(Math.random() * 0x100000000).toString(16);
        var item = { name: randomValue };
        table.insert(item).done(function () {
            table.where({ name: randomValue }).select('name').read().done(function (results) {
                var expectedResult = [];
                for (i = 0; i < numberOfRequests; i++) expectedResult.push({ name: randomValue });
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
    }, zumo.runtimeFeatureNames.STRING_ID_TABLES));

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

    tests.push(new zumo.Test('Using filters to access optimistic concurrency features', function (test, done) {
        var client = zumo.getClient();

        var ocFilter = function (req, next, callback) {
            var url = req.url;
            url = addSystemProperties(url);
            req.url = url;

            removeSystemPropertiesFromBody(req);
            next(req, function (error, response) {
                if (response && response.getResponseHeader) {
                    var etag = response.getResponseHeader('ETag');
                    if (etag) {
                        if (etag.substring(0, 1) === '\"') {
                            etag = etag.substring(1);
                        }
                        if (etag.substring(etag.length - 1) === '\"') {
                            etag = etag.substring(0, etag.length - 1);
                        }
                        var body = JSON.parse(response.responseText);
                        body['__version'] = etag;
                        response.responseText = JSON.stringify(body);
                    }
                }

                callback(error, response);
            });

            function addSystemProperties(url) {
                var queryIndex = url.indexOf('?');
                var query, urlNoQuery;
                if (queryIndex >= 0) {
                    urlNoQuery = url.substring(0, queryIndex);
                    query = url.substring(queryIndex + 1) + '&';
                } else {
                    urlNoQuery = url;
                    query = '';
                }
                query = query + '__systemProperties=*';
                return urlNoQuery + '?' + query;
            }

            function removeSystemPropertiesFromBody(request) {
                var method = request.type;
                var data = request.data;
                if (method === 'PATCH' || method === 'PUT') {
                    var body = JSON.parse(data);
                    if (typeof body === 'object') {
                        var toRemove = [];
                        for (var k in body) {
                            if (k.indexOf('__') === 0) {
                                toRemove.push(k);
                                if (k === '__version') {
                                    var etag = '\"' + body[k] + '\"';
                                    req.headers['If-Match'] = etag;
                                }
                            }
                        }

                        if (toRemove.length) {
                            for (var i = 0; i < toRemove.length; i++) {
                                delete body[toRemove[i]];
                            }
                            req.data = JSON.stringify(body);
                        }
                    }
                }
            }
        };

        client = client.withFilter(ocFilter);
        var table = client.getTable(stringIdTableName);

        var errFunction = function (err) {
            test.addLog('Error: ', err);
            done(false);
        };
        table.insert({ name: 'John Doe', number: 123 }).done(function (inserted) {
            test.addLog('Inserted: ', inserted);
            inserted.name = 'Jane Roe';
            table.update(inserted).done(function (updated) {
                test.addLog('Updated: ', updated);
                test.addLog('Now updating with incorrect version');
                updated['__version'] = 'incorrect';
                table.update(updated).done(function (updated2) {
                    test.addLog('Updated again (should not happen): ', updated2);
                    done(false);
                }, function (err) {
                    test.addLog('Got (expected) error: ', err);
                    done(true);
                });
            }, errFunction);
        }, errFunction);
    }, zumo.runtimeFeatureNames.STRING_ID_TABLES));

    function createFilterCaptureTest(successfulRequest) {
        var testName = 'Filter can be used to trace request / ' + (successfulRequest ? 'successful' : 'error') + ' response'
        return new zumo.Test(testName, function (test, done) {
            var client = zumo.getClient();
            var filter = createLoggingFilter();
            var filtered = client.withFilter(filter);
            var table = filtered.getTable(stringIdTableName);
            var item = { name: 'hello world' };
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
        }, zumo.runtimeFeatureNames.STRING_ID_TABLES);
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
