// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="..\..\js\MobileServices.Internals.js" />
/// <reference path="..\..\generated\Tests.js" />

$testGroup('Push')
    .functional()
    .tag('push')
    .tests(
        $test('InitialUnregisterAll')
        .description('Unregister all registrations with both the default and updated channel. Ensure no registrations still exist for either.')
        .checkAsync(function () {
            var client = $getClient();
            var channelUri = defaultChannel;

            return client.push.unregisterAll(channelUri)
                .then(
                    function () {
                        return client.push.registrationManager.pushHttpClient.listRegistrations(channelUri);
                    })
                .then(
                    function (registrations) {
                        $assert.isTrue(Array.isArray(registrations), 'Expect to get an array from listRegistrations');
                        $assert.areEqual(registrations.length, 0, 'Expect no registrations to be returned after unregisterAll');
                        $assert.areEqual(client.push.registrationManager.localStorageManager.registrations.size, 0, 'Expect local storage to contain no registrations after unregisterAll');
                        channelUri = updatedChannel;
                        return client.push.unregisterAll(channelUri);
                    })
                .then(
                    function () {
                        return client.push.registrationManager.pushHttpClient.listRegistrations(channelUri);
                    })
                .then(
                    function (registrations) {
                        $assert.isTrue(Array.isArray(registrations), 'Expect to get an array from listRegistrations');
                        $assert.areEqual(registrations.length, 0, 'Expect no registrations to be returned after unregisterAll');
                        $assert.areEqual(client.push.registrationManager.localStorageManager.registrations.size, 0, 'Expect local storage to contain no registrations after unregisterAll');
                        return WinJS.Promise.wrap();
                    });
        }),
        $test('RegisterNativeUnregisterNative')
        .description('Register a native channel followed by unregistering it.')
        .checkAsync(function () {
            var client = $getClient();
            var channelUri = defaultChannel;

            return client.push.registerNative(channelUri)
                .then(
                    function () {
                        return client.push.registrationManager.pushHttpClient.listRegistrations(channelUri);
                    })
                .then(
                    function (registrations) {
                        $assert.isTrue(Array.isArray(registrations), 'Expect to get an array from listRegistrations');
                        $assert.areEqual(registrations.length, 1, 'Expect 1 registration to be returned after register');
                        $assert.areEqual(client.push.registrationManager.localStorageManager.registrations.size, 1, 'Expect local storage to contain 1 registration after register');
                        var localRegistration = client.push.registrationManager.localStorageManager.getFirstRegistrationByRegistrationId(registrations[0].registrationId);
                        $assert.isTrue(localRegistration, 'Expect local storage to have the registrationId returned from service');
                        $assert.areEqual(client.push.registrationManager.localStorageManager.channelUri, registrations[0].deviceId, 'Local storage should have channelUri from returned registration');

                        return client.push.unregisterNative();
                    })
                .then(
                    function () {
                        return client.push.registrationManager.pushHttpClient.listRegistrations(channelUri);
                    })
                .then(
                    function (registrations) {
                        $assert.isTrue(Array.isArray(registrations), 'Expect to get an array from listRegistrations');
                        $assert.areEqual(registrations.length, 0, 'Expect no registrations to be returned after unregisterNative');
                        $assert.areEqual(client.push.registrationManager.localStorageManager.registrations.size, 0, 'Expect local storage to contain no registrations after unregisterNative');
                        return WinJS.Promise.wrap();
                    });
        }),
        $test('RegisterTemplateUnregisterTemplate')
        .description('Register a template followed by unregistering it.')
        .checkAsync(function () {
            var client = $getClient();
            var channelUri = defaultChannel;

            return client.push.registerTemplate(channelUri, templateBody, templateName, defaultHeaders, defaultTags)
                .then(
                    function () {
                        return client.push.registrationManager.pushHttpClient.listRegistrations(channelUri);
                    })
                .then(
                    function (registrations) {
                        $assert.isTrue(Array.isArray(registrations), 'Expect to get an array from listRegistrations');
                        $assert.areEqual(registrations.length, 1, 'Expect 1 registration to be returned after register');
                        $assert.areEqual(client.push.registrationManager.localStorageManager.registrations.size, 1, 'Expect local storage to contain 1 registration after register');
                        var localRegistration = client.push.registrationManager.localStorageManager.getFirstRegistrationByRegistrationId(registrations[0].registrationId);
                        $assert.isTrue(localRegistration, 'Expect local storage to have the registrationId returned from service');
                        $assert.areEqual(client.push.registrationManager.localStorageManager.channelUri, registrations[0].deviceId, 'Local storage should have channelUri from returned registration');
                        $assert.areEqual(registrations[0].deviceId, channelUri, 'Returned registration should use channelUri sent from registered template');
                        Object.getOwnPropertyNames(registrations[0].headers).forEach(function (header) {
                            $assert.areEqual(registrations[0].headers[header], defaultHeaders[header], 'Each header returned by registration should match what was registered.');
                        });
                        $assert.areEqual(Object.getOwnPropertyNames(registrations[0].headers).length, Object.getOwnPropertyNames(defaultHeaders).length, 'Returned registration should contain same number of headers sent from registered template');
                        $assert.areEqual(registrations[0].tags.length, $isDotNet() ? defaultTags.length : defaultTags.length + 1, 'Returned registration should contain tags sent from registered template and 1 extra for installationId');
                        // TODO: Re-enable when .Net runtime supports installationID in service
                        //$assert.isTrue(registrations[0].tags.indexOf(WindowsAzure.MobileServiceClient._applicationInstallationId) > -1, 'Expected the installationID in the tags');
                        $assert.areEqual(registrations[0].templateName, templateName, 'Expected returned registration to use templateName it was fed');
                        $assert.areEqual(registrations[0].templateBody, templateBody, 'Expected returned registration to use templateBody it was fed');
                        $assert.areEqual(client.push.registrationManager.localStorageManager.getRegistration(templateName).registrationId, registrations[0].registrationId, 'Expected the stored registrationId to equal the one returned from service');

                        return client.push.unregisterTemplate(templateName);
                    })
                .then(
                    function () {
                        return client.push.registrationManager.pushHttpClient.listRegistrations(channelUri);
                    })
                .then(
                    function (registrations) {
                        $assert.isTrue(Array.isArray(registrations), 'Expect to get an array from listRegistrations');
                        $assert.areEqual(registrations.length, 0, 'Expect no registrations to be returned after unregisterTemplate');
                        $assert.areEqual(client.push.registrationManager.localStorageManager.registrations.size, 0, 'Expect local storage to contain no registrations after unregisterTemplate');
                    });
        }),
        $test('RegisterRefreshRegisterWithUpdatedChannel')
        .description('Register a template followed by a refresh of the client local storage followed by updated register of same template name.')
        .checkAsync(function () {
            var client = $getClient();
            var channelUri = defaultChannel;

            return client.push.registerTemplate(channelUri, templateBody, templateName, defaultHeaders, defaultTags)
                .then(
                    function () {
                        return client.push.registrationManager.pushHttpClient.listRegistrations(channelUri);
                    })
                .then(
                    function (registrations) {
                        $assert.isTrue(Array.isArray(registrations), 'Expect to get an array from listRegistrations');
                        $assert.areEqual(registrations.length, 1, 'Expect 1 registration to be returned after register');
                        $assert.areEqual(client.push.registrationManager.localStorageManager.registrations.size, 1, 'Expect local storage to contain 1 registration after register');
                        var localRegistration = client.push.registrationManager.localStorageManager.getFirstRegistrationByRegistrationId(registrations[0].registrationId);
                        $assert.isTrue(localRegistration, 'Expect local storage to have the registrationId returned from service');
                        $assert.areEqual(client.push.registrationManager.localStorageManager.channelUri, registrations[0].deviceId, 'Local storage should have channelUri from returned registration');
                        $assert.areEqual(registrations[0].deviceId, channelUri, 'Returned registration should use channelUri sent from registered template');
                        Object.getOwnPropertyNames(registrations[0].headers).forEach(function (header) {
                            $assert.areEqual(registrations[0].headers[header], defaultHeaders[header], 'Each header returned by registration should match what was registered.');
                        });
                        $assert.areEqual(Object.getOwnPropertyNames(registrations[0].headers).length, Object.getOwnPropertyNames(defaultHeaders).length, 'Returned registration should contain same number of headers sent from registered template');
                        $assert.areEqual(registrations[0].tags.length, $isDotNet() ? defaultTags.length : defaultTags.length + 1, 'Returned registration should contain tags sent from registered template and 1 extra for installationId');
                        // TODO: Re-enable when .Net runtime supports installationID in service
                        //$assert.isTrue(registrations[0].tags.indexOf(WindowsAzure.MobileServiceClient._applicationInstallationId) > -1, 'Expected the installationID in the tags');
                        $assert.areEqual(registrations[0].templateName, templateName, 'Expected returned registration to use templateName it was fed');
                        $assert.areEqual(registrations[0].templateBody, templateBody, 'Expected returned registration to use templateBody it was fed');
                        $assert.areEqual(client.push.registrationManager.localStorageManager.getRegistration(templateName).registrationId, registrations[0].registrationId, 'Expected the stored registrationId to equal the one returned from service');

                        client.push.registrationManager.localStorageManager.isRefreshNeeded = true;
                        channelUri = updatedChannel;

                        return client.push.registerTemplate(channelUri, templateBody, templateName, defaultHeaders, defaultTags);
                    })
                .then(
                    function () {
                        return client.push.registrationManager.pushHttpClient.listRegistrations(channelUri);
                    })
                .then(
                    function (registrations) {
                        $assert.isTrue(Array.isArray(registrations), 'Expect to get an array from listRegistrations');
                        $assert.areEqual(registrations.length, 1, 'Expect 1 registration to be returned after register');
                        $assert.areEqual(client.push.registrationManager.localStorageManager.registrations.size, 1, 'Expect local storage to contain 1 registration after register');
                        var localRegistration = client.push.registrationManager.localStorageManager.getFirstRegistrationByRegistrationId(registrations[0].registrationId);
                        $assert.isTrue(localRegistration, 'Expect local storage to have the registrationId returned from service');
                        $assert.areEqual(client.push.registrationManager.localStorageManager.channelUri, registrations[0].deviceId, 'Local storage should have channelUri from returned registration');
                        $assert.areEqual(registrations[0].deviceId, channelUri, 'Returned registration should use channelUri sent from registered template');
                        Object.getOwnPropertyNames(registrations[0].headers).forEach(function (header) {
                            $assert.areEqual(registrations[0].headers[header], defaultHeaders[header], 'Each header returned by registration should match what was registered.');
                        });
                        $assert.areEqual(Object.getOwnPropertyNames(registrations[0].headers).length, Object.getOwnPropertyNames(defaultHeaders).length, 'Returned registration should contain same number of headers sent from registered template');
                        $assert.areEqual(registrations[0].tags.length, $isDotNet() ? defaultTags.length : defaultTags.length + 1, 'Returned registration should contain tags sent from registered template and 1 extra for installationId');
                        // TODO: Re-enable when .Net runtime supports installationID in service
                        //$assert.isTrue(registrations[0].tags.indexOf(WindowsAzure.MobileServiceClient._applicationInstallationId) > -1, 'Expected the installationID in the tags');
                        $assert.areEqual(registrations[0].templateName, templateName, 'Expected returned registration to use templateName it was fed');
                        $assert.areEqual(registrations[0].templateBody, templateBody, 'Expected returned registration to use templateBody it was fed');
                        $assert.areEqual(client.push.registrationManager.localStorageManager.getRegistration(templateName).registrationId, registrations[0].registrationId, 'Expected the stored registrationId to equal the one returned from service');
                        $assert.areEqual(registrations[0].deviceId, updatedChannel, 'Expected the return channelUri to be the updated one');
                        $assert.areEqual(client.push.registrationManager.localStorageManager.channelUri, updatedChannel, 'Expected localstorage channelUri to be the updated one');

                        return client.push.unregisterTemplate(templateName);
                    })
                .then(
                    function () {
                        return client.push.registrationManager.pushHttpClient.listRegistrations(channelUri);
                    })
                .then(
                    function (registrations) {
                        $assert.isTrue(Array.isArray(registrations), 'Expect to get an array from listRegistrations');
                        $assert.areEqual(registrations.length, 0, 'Expect no registrations to be returned after unregisterTemplate');
                        $assert.areEqual(client.push.registrationManager.localStorageManager.registrations.size, 0, 'Expect local storage to contain no registrations after unregisterTemplate');
                    });
        })
);

var defaultChannel = 'https://bn2.notify.windows.com/?token=AgYAAADs42685sa5PFCEy82eYpuG8WCPB098AWHnwR8kNRQLwUwf%2f9p%2fy0r82m4hxrLSQ%2bfl5aNlSk99E4jrhEatfsWgyutFzqQxHcLk0Xun3mufO2G%2fb2b%2ftjQjCjVcBESjWvY%3d';
var updatedChannel = 'https://bn2.notify.windows.com/?token=BgYAAADs42685sa5PFCEy82eYpuG8WCPB098AWHnwR8kNRQLwUwf%2f9p%2fy0r82m4hxrLSQ%2bfl5aNlSk99E4jrhEatfsWgyutFzqQxHcLk0Xun3mufO2G%2fb2b%2ftjQjCjVcBESjWvY%3d';
var templateBody = '<toast><visual><binding template=\"ToastText01\"><text id=\"1\">$(message)</text></binding></visual></toast>';
var templateName = 'templateForToastWinJS';
var defaultTags = ['fooWinJS', 'barWinJS'];
var defaultHeaders = { 'x-wns-type': 'wns/toast', 'x-wns-ttl': '100000' };