// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

$testGroup('Push',

    /* deleteRegistrationWithName */

    $test('Push.apns success')
    .description('Verify apns object can be retrieved')
    .tag('apns')
    .check(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");

        client = client.withFilter(function (req, next, callback) {
            $assert.fail('No http call should have been made');
            callback(null, { status: 500, responseText: 'Bad Test!' });
        });

        try {
        	var apns = client.push.apns;
        	$assert.isTrue(typeof apns == "object");
        	$assert.isTrue(typeof apns.registerNative == "function");
        	$assert.isTrue(typeof apns.registerTemplate == "function");
        	$assert.isTrue(typeof apns.unregisterNative == "function");
        	$assert.isTrue(typeof apns.unregisterTemplate == "function");
        	$assert.isTrue(typeof apns.unregisterAll == "function");
        } catch (e) {
        	$assert.fail('unexpected error');
        }
    }),

    $test('apns.register uppercases token')
    .description('Verify apns device token is uppercased in calls')
    .tag('apns')
    .check(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");

        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.url, 'http://www.test.com/push/registrations?platform=apns&deviceId=123456ABCDEFG');
            callback(null, { status: 500, responseText: 'Stop the chain' });
        });

        return client.push.apns.registerNative('abcdefg1234').then(null, function (error) {
            $assert.areEqual(error.response.responseText, 'Stop the chain');
        });
    }),


    $test('Push.gcm success')
    .description('Verify gcm object can be retrieved')
    .tag('gcm')
    .check(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");

        client = client.withFilter(function (req, next, callback) {
            $assert.fail('No http call should have been made');
            callback(null, { status: 500, responseText: 'Bad Test!' });
        });

        try {
        	var gcm = client.push.gcm;
        	$assert.isTrue(typeof gcm == "object");
        	$assert.isTrue(typeof gcm.registerNative == "function");
        	$assert.isTrue(typeof gcm.registerTemplate == "function");
        	$assert.isTrue(typeof gcm.unregisterNative == "function");
        	$assert.isTrue(typeof gcm.unregisterTemplate == "function");
        	$assert.isTrue(typeof gcm.unregisterAll == "function");
        } catch (e) {
        	$assert.fail('unexpected error');
        }
    })
);