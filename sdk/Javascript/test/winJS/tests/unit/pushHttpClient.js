// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="..\..\js\MobileServices.Internals.js" />
/// <reference path="..\..\generated\Tests.js" />

// Declare JSHint globals
/*global MobileServiceClient:false, PushHttpClient:false */

$testGroup('PushHttpClient',
    $test('ListRegistrationsWithResultsReturnsSuccess')
    .description('Verify listRegistrations parses an array')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            pushHandle = 'ABC';

        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'GET');
            $assert.areEqual(req.url, 'http://www.test.com/push/registrations?platform=fake&deviceId=' + pushHandle);
            callback(null, { status: 200, responseText: '[{"registrationName":"$Default","registrationId":"123456"}]' });
        });

        var pushHttp = new PushHttpClient.PushHttpClient(client);
        return Platform.async(pushHttp.listRegistrations).call(pushHttp, pushHandle, 'fake').then(function (results) {
            $assert.areEqual(results.length, 1);
            $assert.areEqual(results[0].registrationId, '123456');
            $assert.areEqual(results[0].registrationName, '$Default');
        });
    }),

    $test('ListRegistrationsEmptyReturnsSuccess')
    .description('Verify listRegistrations parses an empty array')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            pushHandle = 'ABC';

        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'GET');
            $assert.areEqual(req.url, 'http://www.test.com/push/registrations?platform=fake&deviceId=' + pushHandle);
            callback(null, { status: 200, responseText: '[]' });
        });

        var pushHttp = new PushHttpClient.PushHttpClient(client);
        return Platform.async(pushHttp.listRegistrations).call(pushHttp, pushHandle, 'fake').then(function (results) {
            $assert.areEqual(results.length, 0);
        });
    }),

    $test('CreateRegistrationIdReturnsSuccess')
    .description('Verify createRegistrationId succeeds')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            pushHandle = 'ABC';

        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            $assert.areEqual(req.url, 'http://www.test.com/push/registrationIds');
            callback(null, { 
                status: 200, 
                getResponseHeader: function (header) { 
                    return header == 'Location' ? 
                        'http://philtotesting.azure-mobile.net/push/registrations/6989338662585603267-4084702197021366041-7' : 
                        null; 
            }, responseText: '' });
        });

        var pushHttp = new PushHttpClient.PushHttpClient(client);
        return Platform.async(pushHttp.createRegistrationId).call(pushHttp).then(function (registrationId) {
            $assert.areEqual(registrationId, '6989338662585603267-4084702197021366041-7');
        });
    }),

    $test('UpsertRegistrationReturnsSuccess')
    .description('Verify upsertRegistration succeeds with test wns data')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            pushHandle = 'ABC',
            registration = {
                registrationId: '6989338662585603267-4084702197021366041-7',
                platform: 'wns',
                deviceId: 'https://bn2.notify.windows.com/?token=AgYAAADs42685sa5PFCEy82eYpuG8WCPB098AWHnwR8kNRQLwUwf%2f9p%2fy0r82m4hxrLSQ%2bfl5aNlSk99E4jrhEatfsWgyutFzqQxHcLk0Xun3mufO2G%2fb2b%2ftjQjCjVcBESjWvY%3d'
            };

        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'PUT');
            $assert.areEqual(req.url, 'http://www.test.com/push/registrations/' + registration.registrationId);
            $assert.areEqual(req.data, JSON.stringify(registration));
            callback(null, { status: 204 });
        });

        var pushHttp = new PushHttpClient.PushHttpClient(client);
        return Platform.async(pushHttp.upsertRegistration).call(pushHttp, registration);
    }),

    $test('UpsertRegistrationExpiredReturnsFailure')
    .description('Verify server returning expired (410) is treated as a success')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            pushHandle = 'ABC',
            registration = {
                registrationId: '6989338662585603267-4084702197021366041-7',
                platform: 'wns',
                deviceId: 'https://bn2.notify.windows.com/?token=AgYAAADs42685sa5PFCEy82eYpuG8WCPB098AWHnwR8kNRQLwUwf%2f9p%2fy0r82m4hxrLSQ%2bfl5aNlSk99E4jrhEatfsWgyutFzqQxHcLk0Xun3mufO2G%2fb2b%2ftjQjCjVcBESjWvY%3d'
            };

        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'PUT');
            $assert.areEqual(req.url, 'http://www.test.com/push/registrations/' + registration.registrationId);
            $assert.areEqual(req.data, JSON.stringify(registration));
            callback(null, { status: 410 });
        });

        var pushHttp = new PushHttpClient.PushHttpClient(client);
        return Platform.async(pushHttp.upsertRegistration).call(pushHttp, registration).then(function () {
            $assert.fail('Success should not have been called');
        }, function (error) {
            $assert.areEqual(error.request.status, 410);
        })
    }),

    $test('UnregisterReturnsSuccess')
    .description('Verify unregister success')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            registrationId = '6989338662585603267-4084702197021366041-7';

        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'DELETE');
            $assert.areEqual(req.url, 'http://www.test.com/push/registrations/' + registrationId);
            callback(null, { status: 204 });
        });

        var pushHttp = new PushHttpClient.PushHttpClient(client);
        return Platform.async(pushHttp.unregister).call(pushHttp, registrationId);
    }),

    $test('UnregisterNotFoundReturnsSuccess')
    .description('Verify unregister success when not found')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            registrationId = '6989338662585603267-4084702197021366041-7';

        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'DELETE');
            $assert.areEqual(req.url, 'http://www.test.com/push/registrations/' + registrationId);
            callback(null, { status: 404, responseText: '{"error":"no registration found"}' });
        });

        var pushHttp = new PushHttpClient.PushHttpClient(client);
        return Platform.async(pushHttp.unregister).call(pushHttp, registrationId);
    })
);
