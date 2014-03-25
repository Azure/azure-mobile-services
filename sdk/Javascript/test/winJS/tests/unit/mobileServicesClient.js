// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="..\..\js\MobileServices.Internals.js" />
/// <reference path="..\..\generated\Tests.js" />

// Declare JSHint globals
/*global MobileServiceClient:false, Platform:false, Windows:false, Resources:false */

$testGroup('MobileServiceClient.js',

    $test('constructor')
    .description('Verify the constructor correctly initializes the client.')
    .check(function () {
        $assertThrows(function () { new WindowsAzure.MobileServiceClient(); });
        $assertThrows(function () { new WindowsAzure.MobileServiceClient(null); });
        $assertThrows(function () { new WindowsAzure.MobileServiceClient(''); });
        $assertThrows(function () { new WindowsAzure.MobileServiceClient(2); });
        $assertThrows(function () { new WindowsAzure.MobileServiceClient('uri', 2); });

        var uri = "http://www.test.com";
        var key = "123456abcdefg";
        var client = new WindowsAzure.MobileServiceClient(uri, key);
        $assert.areEqual(uri, client.applicationUrl);
        $assert.areEqual(key, client.applicationKey);
        $assert.isTrue(client.getTable);
    }),

    $test('withFilter chaining')
    .description('Verify withFilter correctly chains filters')
    .checkAsync(function () {
        var descend = '';
        var rise = '';
        var createFilter = function(letter) { 
            return function(req, next, callback) {
                descend += letter;
                next(req, function (err, resp) {
                    rise += letter;
                    callback(err, resp);                 
                });
            };
        };

        var reachableHost = typeof Windows === "object" ? "http://www.windowsazure.com/" : $getClient().applicationUrl,
            reachablePath = typeof Windows === "object" ? "en-us/develop/overview/" : "crossdomain/bridge?origin=http://localhost";
        var client = new WindowsAzure.MobileServiceClient(reachableHost, "123456abcdefg");
        client = client.withFilter(createFilter('A')).withFilter(createFilter('B')).withFilter(createFilter('C'));
        
        return Platform.async(client._request).call(client, 'GET', reachablePath, null).then(function (rsp) {
            $assert.areEqual(descend, 'CBA');
            $assert.areEqual(rise, 'ABC');
        });
    }),

    $test('withFilter')
    .description('Verify withFilter intercepts calls')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            callback(null, { status: 200, responseText: '{"authenticationToken":"zumo","user":{"userId":"bob"}}' });
        });

        client._login.ignoreFilters = false;

        return client.login('token.a.b').then(function (result) {
            $assert.areEqual(result.userId, 'bob');
        });
    }),
    
    $test('login')
    .description('Verify login mechanics')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            $assert.contains(req.url, "login");
            $assert.areEqual(req.data, '{"authenticationToken":"token.a.b"}');
            callback(null, { status: 200, responseText: '{"authenticationToken":"zumo","user":{"userId":"bob"}}' });
        });

        client._login.ignoreFilters = false;

        return client.login('token.a.b').then(function (currentUser) {
            $assert.areEqual(client.currentUser.userId, 'bob');
            $assert.areEqual(client.currentUser.mobileServiceAuthenticationToken, 'zumo');            
        });
    }),

    $test('logout')
    .description('Verify Authentication.logout undoes the effects of logging in')
    .check(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client.currentUser = { userId: 'bob', mobileServiceAuthenticationToken: 'abcd' };

        client.logout();
        $assert.areEqual(client.currentUser, null);
    }),

    $test('static initialization of appInstallId')
    .description('Verify the app installation id is created statically.')
    .check(function() {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            settingsKey = "MobileServices.Installation.config",
            settings = typeof Windows === "object" ? Windows.Storage.ApplicationData.current.localSettings.values[settingsKey]
                                                   : Platform.readSetting(settingsKey);
        $assert.isTrue(settings);
    }),

    $test('CustomAPI - error response as json object')
    .description('Verify the custom API error messages')
    .check(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            $assert.areEqual(req.url, 'http://www.test.com/api/checkins/post');
            callback(null, { status: 400, responseText: '{"error":"bad robot"}', getResponseHeader: function () { return 'application/json'; } });
        });
        client.invokeApi("checkins/post").done(function (response) {
            $assert.fail("api call failed");
        },
        function (error) {
            $assert.areEqual(error.message, "bad robot");
        });
    }),

    $test('CustomAPI - error response as json string')
    .description('Verify the custom API error messages')
    .check(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            $assert.areEqual(req.url, 'http://www.test.com/api/checkins/post');
            callback(null, { status: 400, responseText: '"bad robot"', getResponseHeader: function () { return 'application/json'; } });
        });
        client.invokeApi("checkins/post").done(function (response) {
            $assert.fail("api call failed");
        },
        function (error) {
            $assert.areEqual(error.message, "bad robot");
        });
    }),

    $test('CustomAPI - error as text')
    .description('Verify the custom API error messages')
    .check(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            $assert.areEqual(req.url, 'http://www.test.com/api/checkins/post');
            callback(null, { status: 400, responseText: 'bad robot', getResponseHeader: function () { return 'text/html'; } });
        });
        client.invokeApi("checkins/post").done(function (response) {
            $assert.fail("api call failed");
        },
        function (error) {
            $assert.areEqual(error.message, "bad robot");
        });
    }),

    $test('CustomAPI - just api name')
    .description('Verify the custom API url formatting')
    .check(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            $assert.areEqual(req.url, 'http://www.test.com/api/checkins/post');
            callback(null, { status: 200, responseText: '{"result":3 }', getResponseHeader: function () { return 'application/json'; } });
        });
        client.invokeApi("checkins/post").done(function (response) {
            $assert.areEqual(response.result.result, 3);
        },
        function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - name and content')
    .description('Verify the custom API url formatting')
    .check(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            $assert.areEqual(req.url, 'http://www.test.com/api/scenarios/verifyRequestAccess');
            $assert.areEqual(req.data, '{\"data\":\"one\"}');
            callback(null, { status: 200, getResponseHeader: function () { return 'application/xml'; }, responseText: '' });
        });

        client.invokeApi("scenarios/verifyRequestAccess", { body: { 'data': 'one' } }).done(function (response) {
        }, function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - string content')
    .description('Verify sending string content')
    .check(function () {
        var client = new MobileServiceClient.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.data, 'apples');
            callback(null, { status: 200, responseText: '{"result":3}', getResponseHeader: function () { return 'application/json'; } });
        });

        client.invokeApi("scenarios/verifyRequestAccess", { body: "apples" }).done(function (response) {
        }, function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - boolean content')
    .description('Verify sending boolean content')
    .check(function () {
        var client = new MobileServiceClient.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.data, "true");
            callback(null, { status: 200, responseText: '{"result":3}', getResponseHeader: function () { return 'application/json'; } });
        });

        client.invokeApi("scenarios/verifyRequestAccess", { body: true }).done(function (response) {
        }, function (error) {
            $assert.fail("api call failed");
        });
    }),
    
    $test('CustomAPI - date object')
    .description('Verify sending date object')
    .check(function () {
        var client = new MobileServiceClient.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.data, "\"2014-03-06T09:59:00.000Z\"");
            callback(null, { status: 200, responseText: '{"result":3}', getResponseHeader: function () { return 'application/json'; } });
        });

        var date = new Date(2013, 14, 6, 1, 59);
        client.invokeApi("scenarios/verifyRequestAccess", { body: date }).done(function (response) {
        }, function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - array content')
    .description('Verify sending array content')
    .check(function () {
        var client = new MobileServiceClient.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.data, '[\"a\",\"b\",\"c\"]');
            callback(null, { status: 200, responseText: '{"result":3 }', getResponseHeader: function () { return 'application/json'; } });
        });

        client.invokeApi("scenarios/verifyRequestAccess", { body: ['a','b','c'] }).done(function (response) {
        }, function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - name with querystring and method')
    .description('Verify the custom API url formatting')
    .check(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'GET');
            $assert.areEqual(req.url, 'http://www.test.com/api/calculator/add?a=1&b=2');
            callback(null, { status: 200, responseText: '{"result":3 }', getResponseHeader: function () { return 'application/json'; } });
        });
        client.invokeApi("calculator/add?a=1&b=2", { method: "GET" }).done(function (response) {
            $assert.areEqual(response.result.result, 3);
        },
        function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - name and method with param')
    .description('Verify the custom API url formatting')
    .check(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'GET');
            $assert.areEqual(req.url, 'http://www.test.com/api/calculator/add?a=1&b=2');
            callback(null, { status: 200, responseText: '{"result":3 }', getResponseHeader: function () { return 'application/json'; } });
        });
        client.invokeApi("calculator/add", { method: "GET", parameters: { 'a': 1, 'b': 2 }}).done(function (response) {
            $assert.areEqual(response.result.result, 3);
        },
        function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - Return XML')
    .description('Verify the custom API url formatting')
    .check(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'GET');
            $assert.areEqual(req.url, 'http://www.test.com/api/scenarios/getXmlResponse');
            callback(null, { status: 200, responseText: '<foo>bar</foo>', getResponseHeader: function () { return 'application/xml'; } });
        });
        client.invokeApi("scenarios/getXmlResponse", { method: "GET"}).done(function (response) {
            $assert.areEqual(response.responseText, "<foo>bar</foo>");
        },
        function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - name, body, method, and headers')
    .description('Verify the custom API url formatting')
    .check(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            $assert.areEqual(req.url, 'http://www.test.com/api/scenarios/verifyRequestAccess');
            $assert.areEqual(req.headers['x-zumo-testing'], 'test');
            $assert.areEqual(req.data, '{\"data\":\"one\"}');
            callback(null, { status: 200, getResponseHeader: function () { return 'application/xml'; }, responseText: '' });
        });
        var headers = { 'x-zumo-testing': 'test' };
        client.invokeApi("scenarios/verifyRequestAccess", { body: { 'data': 'one' }, method: "POST", headers: headers }).done(function (response) {
            $assert.isNull(headers['X-ZUMO-VERSION']);
        }, function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - Custom headers and return XML not JSON')
    .description('Verify the custom API url formatting')
    .check(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            $assert.areEqual(req.url, 'http://www.test.com/api/scenarios/verifyRequestAccess');
            $assert.areEqual(req.headers['Content-Type'], 'application/xml');
            $assert.areEqual(req.data, '<foo>bar</foo>'); //no json encoded...
            callback(null, { status: 200, getResponseHeader: function () { return 'application/xml'; } });
        });
        client.invokeApi("scenarios/verifyRequestAccess", { body: '<foo>bar</foo>', method: "POST", headers: { 'Content-Type': 'application/xml' }}).done(function (response) {

        }, function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - Send content-type instead of Content-Type')
    .description('Verify the custom API url formatting')
    .check(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            $assert.areEqual(req.url, 'http://www.test.com/api/scenarios/verifyRequestAccess');
            $assert.areEqual(req.headers['content-type'], 'application/xml');
            $assert.areEqual(req.data, '<foo>bar</foo>'); //no json encoded...
            callback(null, { status: 200, getResponseHeader: function () { return 'application/xml'; } });
        });
        client.invokeApi("scenarios/verifyRequestAccess", { body: '<foo>bar</foo>', method: "POST", headers: { 'content-type': 'application/xml' } }).done(function (response) {

        }, function (error) {
            $assert.fail("api call failed");
        });
    })
);
