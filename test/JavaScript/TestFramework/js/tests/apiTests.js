// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="../../../ZumoE2ETestAppJs/ZumoE2ETestAppHTML/js/platformSpecificFunctions.js" />
/// <reference path="../../../ZumoE2ETestAppJs/ZumoE2ETestAppJs/js/MobileServices.js" />
/// <reference path="../testFramework.js" />

function defineApiTestsNamespace() {
    var tests = [];
    var i;
    var publicApiName = 'public';
    var applicationApiName = 'application';
    var userApiName = 'user';
    var adminApiName = 'admin';

    var dataTypeJson = 'json';
    var dataTypeXml = 'xml';
    var dataTypeOther = 'other';

    tests.push(createCustomApiTest('Simple object - POST', applicationApiName, 'POST', { name: 'value' }, null, null, 200));
    tests.push(createCustomApiTest('Simple call - GET', applicationApiName, 'GET', null, null, { param: 'value' }));
    tests.push(createCustomApiTest('Simple object - PUT', applicationApiName, 'PUT', { array: [1, true, 'str'] }, null, { method: 'PUT' }));
    tests.push(createCustomApiTest('Simple object - PATCH', applicationApiName, 'PATCH', { array: [1, true, 'str'] }, null, { method: 'PATCH' }));
    tests.push(createCustomApiTest('Simple call - DELETE', applicationApiName, 'DELETE', null, null, { method: 'DELETE' }));

    tests.push(createCustomApiTest('POST - array body', applicationApiName, 'POST', [1, false, 2], null, null, 200));
    tests.push(createCustomApiTest('POST - empty array body', applicationApiName, 'POST', [], null, null, 200));
    tests.push(createCustomApiTest('POST - empty object body', applicationApiName, 'POST', {}, null, null, 200));

    tests.push(createCustomApiTest('GET - custom headers', applicationApiName, 'GET', null, { 'x-test-zumo-first': 'header value' }));
    tests.push(createCustomApiTest('PATCH - query parameters', applicationApiName, 'PATCH', [1, 2, 3], null, { x: '6', y: '7' }));
    tests.push(createCustomApiTest('PUT - non-ASCII query parameters', applicationApiName, 'PUT', [1, 2, 3], null, { latin: 'Łåţıñ', arabic: 'الكتاب على الطاولة' }));

    tests.push(createCustomApiTest('GET - 500 response', applicationApiName, 'GET', null, { 'x-test-zumo-1': 'header value' }, { x: '4' }, 500));
    tests.push(createCustomApiTest('POST - 400 response', applicationApiName, 'POST', { a: { b: [] } }, { 'x-test-zumo-1': 'header value' }, { x: '6' }, 400));
    tests.push(createCustomApiTest('DELETE - 201 response', applicationApiName, 'DELETE', null, { 'x-test-zumo-1': 'header value' }, { x: '9' }, 201));

    tests.push(createCustomApiTest('POST - JSON input, XML output', applicationApiName, 'POST', [{ number: 1, word: 'one' }], null, null, 200, dataTypeXml));
    tests.push(createCustomApiTest('PUT - XML input, text output', applicationApiName, 'PUT', '<text id="1">hello world</text>', { 'Content-Type': 'text/xml' }, null, 200, dataTypeOther));
    tests.push(createCustomApiTest('PATCH - text input, JSON output', applicationApiName, 'PATCH', 'This is a text input', { 'Content-Type': 'text/plain' }, null, 200, dataTypeJson));
    tests.push(createCustomApiTest('PUT - JSON input, XML output, custom headers', applicationApiName, 'PUT', [1], { 'x-test-zumo-1': 'header value' }, null, 200, dataTypeXml));
    tests.push(createCustomApiTest('POST - XML input, text output, custom query parameters', applicationApiName, 'PUT', '<hello>world</hello>', { 'content-type': 'text/xml' }, { name: 'value' }, 200, dataTypeOther));
    tests.push(createCustomApiTest('GET - XML output, 500 response', applicationApiName, 'GET', null, null, { name: 'value' }, 500, dataTypeXml));
    tests.push(createCustomApiTest('DELETE - Text output, 400 response', applicationApiName, 'DELETE', null, null, null, 400, dataTypeOther));

    return {
        name: 'Custom API',
        tests: tests
    };

    function createCustomApiTest(testName, apiName, httpMethod, body, headers, query, status, outputFormat) {
        /// <summary>
        /// Creates a custom API test
        /// </summary>
        /// <param name="testName" type="String" optional="false">Name of the test to be created</param>
        /// <param name="apiName" type="String" optional="false">Name of the API to be called</param>
        /// <param name="httpMethod" type="String" optional="false">Name of the test to be created</param>
        /// <param name="body" optional="false">Body of the request. Can be null for requests with no body.</param>
        /// <param name="headers" type="Object" optional="false">HTTP headers. Can be null. For non-JSON input,
        ///       the 'Content-Type' header must be specified.</param>
        /// <param name="query" type="Object" optional="false">Additional query string name/value pairs to
        ///       be sent on the API call.</param>
        /// <param name="status" type="Number" optional="true">Value to be passed to the 'status' query
        ///       parameter, which will cause the response to have that status code. Default = 200.</param>
        /// <param name="outputFormat" type="Number" optional="true">Value of the to be passed to the 'format' query
        ///       parameter, which will cause the response to have that format. Default = 'json'.</param>
        var queryParameters = {};
        query = query || {};
        headers = headers || {};
        status = status || 200;

        for (var key in query) {
            if (query.hasOwnProperty(key)) {
                queryParameters[key] = query[key];
            }
        }
        if (status && status !== 200) {
            queryParameters.status = status;
        }
        if (outputFormat) {
            queryParameters.format = outputFormat;
        }
        
        return new zumo.Test(testName, function (test, done) {
            var client = zumo.getClient();
            var options = { method: httpMethod };
            if (queryParameters) {
                options.parameters = queryParameters;
            }
            if (body) {
                options.body = body;
            }
            if (headers) {
                options.headers = headers;
            }

            var expectedResultBody = { method: httpMethod };
            if (client.currentUser) {
                expectedResultBody.user = { level: 'authenticated', userId: client.currentUser.userId };
            } else {
                expectedResultBody.user = { level: 'anonymous' };
            }
            if (query && !isEmptyObject(query)) {
                expectedResultBody.query = query;
            }
            if (body) {
                expectedResultBody.body = body;
            }

            client.invokeApi(apiName, options).done(function (response) {
                var xhr = response;
                if (!validateStatus(test, xhr, status) ||
                    !validateHeaders(test, headers, xhr) ||
                    !validateBody(test, response.result, xhr, expectedResultBody, outputFormat)) {
                    done(false);
                    return;
                }

                test.addLog('  - All validations succeeded');
                done(true);
            }, function (error) {
                var xhr = error.request;
                if (!validateStatus(test, xhr, status) ||
                    !validateHeaders(test, headers, xhr) ||
                    !validateBody(test, null, xhr, expectedResultBody, outputFormat)) {
                    done(false);
                    return;
                }

                test.addLog('  - All validations succeeded');
                done(true);
            });

            function objToXml(obj) {
                return '<root>' + jsToXml(obj) + '</root>';
            }

            function jsToXml(value) {
                if (value === null) return 'null';
                var type = typeof value;
                var result = '';
                var i = 0;
                switch (type.toLowerCase()) {
                    case 'string':
                    case 'boolean':
                    case 'number':
                        return value.toString();
                    case 'function':
                    case 'object':
                        if (Object.prototype.toString.call(value) === '[object Array]') {
                            result = result + '<array>';
                            for (i = 0; i < value.length; i++) {
                                result = result + '<item>' + jsToXml(value[i]) + '</item>';
                            }
                            result = result + '</array>';
                        } else {
                            var k;
                            var keys = [];
                            for (k in value) {
                                if (value.hasOwnProperty(k)) {
                                    if (typeof value[k] !== 'function') {
                                        keys.push(k);
                                    }
                                }
                            }
                            keys.sort();
                            for (i = 0; i < keys.length; i++) {
                                k = keys[i];
                                result = result + '<' + k + '>' + jsToXml(value[k]) + '</' + k + '>';
                            }
                        }
                }
                return result;
            }

            function validateBody(test, resultObject, xhr, expectedBodyObject, outputFormat) {
                outputFormat = outputFormat || dataTypeJson;
                var responseText = xhr.responseText;
                if (outputFormat === dataTypeXml) {
                    var expectedBodyString = objToXml(expectedBodyObject);
                    if (expectedBodyString !== responseText) {
                        test.addLog('Error comparing response. Expected: ', expectedBodyString, '; actual: ', responseText);
                        return false;
                    }                    
                } else {
                    if (outputFormat == dataTypeOther) {
                        test.addLog('Unescaping response. Original: ', responseText);
                        responseText = responseText.replace(/__\[__/g, '[')
                                                   .replace(/__\]__/g, ']')
                                                   .replace(/__\{__/g, '{')
                                                   .replace(/__\}__/g, '}');
                        test.addLog('Unescaped: ', responseText);
                    }

                    resultObject = resultObject || JSON.parse(responseText);

                    var errors = [];
                    if (!zumo.util.compare(expectedBodyObject, resultObject, errors)) {
                        test.addLog('Error comparing objects:');
                        for (var i = 0; i < errors.length; i++) {
                            test.addLog('  ', errors[i]);
                        }

                        return false;
                    }
                }

                test.addLog('  - Received expected response body');
                return true;
            }

            function validateStatus(test, xhr, expectedStatus) {
                expectedStatus = expectedStatus || 200;
                if (xhr.status !== expectedStatus) {
                    test.addLog('Error, expected ', expectedStatus, ' received ', xhr.status);
                    traceFullResponse(test, xhr);
                    return false;
                } else {
                    test.addLog('  - Received expected status code');
                    return true;
                }
            }

            function validateHeaders(test, requestHeaders, xhr) {
                test.addLog('are we running in pure HTML?', testPlatform.IsHTMLApplication);
                if (testPlatform.IsHTMLApplication) {
                    test.addLog('XMLHttpRequest does not expose custom response headers; skipping this verification');
                    return true;
                }

                if (requestHeaders) {
                    for (var headerName in requestHeaders) {
                        if (requestHeaders.hasOwnProperty(headerName) && headerName.indexOf('x-test-zumo-') === 0) {
                            var headerValue = requestHeaders[headerName];
                            var responseHeader = xhr.getResponseHeader(headerName);
                            if (headerValue !== responseHeader) {
                                test.addLog('Error validating header ', headerName, '. Expected: ', headerValue, ', received: ', responseHeader);
                                traceFullResponse(test, xhr);
                                return false;
                            }
                        }
                    }

                    test.addLog('  - Validated response headers');
                }

                return true;
            }

            function traceFullResponse(test, xhr) {
                test.addLog('Response status: ', xhr.status);
                test.addLog('Response headers: ', xhr.getAllResponseHeaders());
                test.addLog('Response body: ', xhr.responseText);
            }

            function isEmptyObject(obj) {
                for (var key in obj) {
                    if (obj.hasOwnProperty(key)) {
                        return false;
                    }
                }
                return true;
            }
        });
    }
}

zumo.tests.api = defineApiTestsNamespace();
