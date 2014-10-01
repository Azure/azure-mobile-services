// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="..\..\js\MobileServices.Internals.js" />
/// <reference path="..\..\generated\Tests.js" />

// Declare JSHint globals
/*global Platform:false,RegistrationManager:false */

$testGroup('RegistrationManager',

    /* deleteRegistrationWithName */

    $test('DeleteRegistrationWithNameNoCacheSuccess')
    .description('Verify RegistrationManager delete does a noop when no cached registration')
    .tag('deleteRegistrationWithName')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");

        client = client.withFilter(function (req, next, callback) {
            $assert.fail('No http call should have been made');
            callback(null, { status: 500, responseText: 'Bad Test!' });
        });

        var registrationManager = new RegistrationManager.RegistrationManager(client, 'platform', StorageKey);

        return registrationManager.deleteRegistrationWithName('M');
    }),

    $test('DeleteRegistrationWithNameCachedSuccess')
    .description('Verify RegistrationManager deletes when a cached registration was found')
    .tag('deleteRegistrationWithName')    
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        CreateStorage();
            
        client = client.withFilter(function (req, next, callback) {
            // We expect a delete for the cached registration (and no others)
            $assert.areEqual(req.type, 'DELETE');
            $assert.contains(req.url, 'http://www.test.com/push/registrations/123');
            // push http tests cover that a 404 is the same as a 204 (aka a success, so we don't need an explicit test here)
            callback(null, { status: 204, responseText: '' });
        });

        var registrationManager = new RegistrationManager.RegistrationManager(client, 'platform', StorageKey);

        return registrationManager.deleteRegistrationWithName('$Default').then(function () {
            var testStore = new LocalStorageManager.LocalStorageManager(StorageKey);

            $assert.areEqual(testStore.getRegistrationIdWithName('testtemplate'), '456');
            $assert.isNull(testStore.getRegistrationIdWithName('$Default'));

            DeleteStorage(testStore);
        });
    }),

    $test('DeleteRegistrationWithNameServerErrorFails')
    .tag('deleteRegistrationWithName')    
    .description('Verify RegistrationManager deletes when a cached registration was found but server returns an error')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
            localStore = CreateStorage();
            
        client = client.withFilter(function (req, next, callback) {
            // We expect a delete for the cached registration (and no others)
            $assert.areEqual(req.type, 'DELETE');
            $assert.contains(req.url, 'http://www.test.com/push/registrations/123');
            callback(null, { status: 500, responseText: 'oops we broke' });
        });

        var registrationManager = new RegistrationManager.RegistrationManager(client, 'platform', StorageKey);

        return registrationManager.deleteRegistrationWithName('$Default').then(function () {
            $assert.fail('Error should have been returned to user');
        }, function (error) {
            $assert.isNotNull(error.message);
            DeleteStorage(localStore);            
        });
    }),

    /* deleteAllRegistrations */

    $test('DeleteAllRegistrationsExistingHandleSuccess')
    .description('Verify RegistrationManager deletes all from server when using a cached handle')
    .tag('deleteAllRegistrations')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            deletedResources = {};

        CreateStorage();
        client = client.withFilter(function (req, next, callback) {
            if (req.type === 'GET' && req.url === 'http://www.test.com/push/registrations?platform=platform&deviceId=testhandle') {
                callback(null, { status: 200, responseText: '[{"registrationId":"1","templateName":"A"},{"registrationId":"2","templateName":"B"}]' });
            } else if (req.type === 'DELETE' && 
                (req.url === 'http://www.test.com/push/registrations/1' ||
                req.url === 'http://www.test.com/push/registrations/2')) {

                // Ensure we only hit each URL once
                if (deletedResources[req.url]) {
                    callback(null, { status: 500, responseText: 'Oops, bad test case!' });
                } else {
                    deletedResources[req.url] = true;
                    callback(null, { status: 204, responseText: '' });                    
                }
            } else {
                callback(null, { status: 500, responseText: 'Oops, bad test case!' });                
            }
        });

        var registrationManager = new RegistrationManager.RegistrationManager(client, 'platform', StorageKey);

        // Refresh (passed pushHandle == cached handle)
        return registrationManager.deleteAllRegistrations('testhandle').then(function () {
            // LocalStoreManager already tests cached !== returned results, so  no need to branch here
            // Then it will delete the returned ones only
            $assert.isTrue(deletedResources['http://www.test.com/push/registrations/1']);
            $assert.isTrue(deletedResources['http://www.test.com/push/registrations/2']);

            // Now check storage is empty
            var testStore = new LocalStorageManager.LocalStorageManager(StorageKey);
            $assert.areEqual(JSON.stringify(testStore.getRegistrationIds()), "[]");
            DeleteStorage(testStore);
        });
    }),

    $test('DeleteAllRegistrationsErrorDuringDeleteFails')
    .description('Verify RegistrationManager attempts to deletes all from server but one errors out fails')
    .tag('deleteAllRegistrations')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            deletedResources = {},
            isFirstCall = true;
    
        CreateStorage();
        client = client.withFilter(function (req, next, callback) {
            if (req.type === 'GET' && req.url === 'http://www.test.com/push/registrations?platform=platform&deviceId=testhandle') {
                callback(null, { status: 200, responseText: '[{"registrationId":"1","templateName":"A"},{"registrationId":"2","templateName":"B"}]' });
            } else if (req.type === 'DELETE' && 
                (req.url === 'http://www.test.com/push/registrations/1' ||
                req.url === 'http://www.test.com/push/registrations/2')) {

                // Ensure we only hit each URL once
                if (deletedResources[req.url]) {
                    callback(null, { status: 500, responseText: 'Oops, bad test case!' });
                } else if (isFirstCall) {
                    callback(null, { status: 500, responseText: 'Expected delete error for this test' });
                    isFirstCall = false;
                } else {
                    callback(null, { status: 204, responseText: '' });
                }                
                deletedResources[req.url] = true;                
            } else {
                $assert.fail('Invalid url requested');
                callback(null, { status: 500, responseText: 'Oops, bad test case!' });
            }
        });

        var registrationManager = new RegistrationManager.RegistrationManager(client, 'platform', StorageKey);

        // Refresh (passed pushHandle == cached handle)
        return registrationManager.deleteAllRegistrations('testhandle').then(function () {
            $assert.fail('Should have failed');            
        }, function (error) {
            $assert.isNotNull(error.message);

            // All registrations ids should have been issued deletes
            $assert.isTrue(!!deletedResources['http://www.test.com/push/registrations/1']);
            $assert.isTrue(!!deletedResources['http://www.test.com/push/registrations/2']);

            // Now check storage is still populated with ids from server
            var testStore = new LocalStorageManager.LocalStorageManager(StorageKey);
            $assert.areEqual(JSON.stringify(testStore.getRegistrationIds()), '["1","2"]');
            DeleteStorage(testStore);
        });
    }),

    $test('DeleteAllRegistrationsServerEmptySuccess')
    .description('Verify RegistrationManager success when no ids')
    .tag('deleteAllRegistrations')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        CreateStorage();

        client = client.withFilter(function (req, next, callback) {
            if (req.type === 'GET' && req.url === 'http://www.test.com/push/registrations?platform=platform&deviceId=testhandle') {
                callback(null, { status: 200, responseText: '[]' });
            } else {
                callback(null, { status: 500, responseText: 'Oops, bad test case!' });                
            }
        });

        var registrationManager = new RegistrationManager.RegistrationManager(client, 'platform', StorageKey);

        // Refresh (passed pushHandle == cached handle)
        return registrationManager.deleteAllRegistrations('testhandle').then(function () {
            // Now check storage is empty
            var testStore = new LocalStorageManager.LocalStorageManager(StorageKey);
            $assert.areEqual(JSON.stringify(testStore.getRegistrationIds()), '[]');
            DeleteStorage(testStore);
        });
    }),

    $test('DeleteAllRegistrationsErrorGettingRegistrationFails')
    .description('Verify RegistrationManager attempts to delete all from server but fails if an issue occurs getting the lists for a handle')
    .tag('deleteAllRegistrations')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        CreateStorage();

        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'GET');
            $assert.areEqual(req.url, 'http://www.test.com/push/registrations?platform=platform&deviceId=testhandle');
            callback(null, { status: 500, responseText: 'Oops error' });
        });

        var registrationManager = new RegistrationManager.RegistrationManager(client, 'platform', StorageKey);

        // Refresh (passed pushHandle == cached handle)
        return registrationManager.deleteAllRegistrations('testhandle').then(function () {
            $assert.fail('Should have failed');
        }, function (error) {
            $assert.isNotNull(error.message);

            // Now check storage is still populated with ids it started with
            var testStore = new LocalStorageManager.LocalStorageManager(StorageKey);
            $assert.areEqual(JSON.stringify(testStore.getRegistrationIds()), '["123","456"]');
            DeleteStorage(testStore);
        });
    }),

    $test('DeleteAllRegistrationsWithNewHandleSuccess')
    .description('Verify RegistrationManager deletes all from server for both handles')
    .tag('deleteAllRegistrations')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            deletedResources = {},
            listCalls = 0;

        CreateStorage();
        client = client.withFilter(function (req, next, callback) {
            if (req.type === 'GET' && req.url === 'http://www.test.com/push/registrations?platform=platform&deviceId=testhandle' && listCalls == 0) {
                listCalls++;
                callback(null, { status: 200, responseText: '[{"registrationId":"1","templateName":"A"},{"registrationId":"2","templateName":"B"}]' });
            } else if (req.type === 'GET' && req.url === 'http://www.test.com/push/registrations?platform=platform&deviceId=testhandle2' && listCalls == 1) {
                listCalls++;
                callback(null, { status: 200, responseText: '[{"registrationId":"3","templateName":"A"},{"registrationId":"4","templateName":"B"}]' });
            } else if (req.type === 'DELETE' && 
                (req.url === 'http://www.test.com/push/registrations/1' ||
                req.url === 'http://www.test.com/push/registrations/2' || 
                req.url === 'http://www.test.com/push/registrations/3' ||
                req.url === 'http://www.test.com/push/registrations/4')) {

                // Ensure we only hit each URL once
                if (deletedResources[req.url]) {
                    callback(null, { status: 500, responseText: 'Oops, bad test case!' });
                } else {
                    deletedResources[req.url] = true;
                    callback(null, { status: 204, responseText: '' });                    
                }
            } else {
                callback(null, { status: 500, responseText: 'Oops, bad test case!' });                
            }
        });

        var registrationManager = new RegistrationManager.RegistrationManager(client, 'platform', StorageKey);

        // Refresh (passed pushHandle == cached handle)
        return registrationManager.deleteAllRegistrations('testhandle2').then(function () {
            // LocalStoreManager already tests cached !== returned results, so  no need to branch here
            // Then it will delete the returned ones only
            $assert.areEqual(listCalls, 2);
            $assert.isTrue(deletedResources['http://www.test.com/push/registrations/1']);
            $assert.isTrue(deletedResources['http://www.test.com/push/registrations/2']);
            $assert.isTrue(deletedResources['http://www.test.com/push/registrations/3']);
            $assert.isTrue(deletedResources['http://www.test.com/push/registrations/4']);

            // Now check storage is empty
            var testStore = new LocalStorageManager.LocalStorageManager(StorageKey);
            $assert.areEqual(JSON.stringify(testStore.getRegistrationIds()), '[]');
            DeleteStorage(testStore);
        });
    }),

    /* upsertRegistration */

    $test('UpsertRegistrationWithRefreshInsertSuccess')
    .description('Verify upsertRegistration updates new record when refresh is needed')
    .tag('upsertRegistration')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            testRegistration = {
                deviceId: 'testhandle',
                platform: 'platform',
                registrationId: '6989338662585603267-4084702197021366041-7',
            };

        client = client.withFilter(function (req, next, callback) {
            if (req.type === 'GET' && req.url === 'http://www.test.com/push/registrations?platform=platform&deviceId=testhandle') {
                callback(null, { status: 200, responseText: '[]' });
            } else if (req.type === 'POST' && req.url === 'http://www.test.com/push/registrationIds') {
                callback(null, 
                    { 
                        status: 200, 
                        getResponseHeader: function (header) { 
                            return header == 'Location' ? 
                                'http://philtotesting.azure-mobile.net/push/registrations/6989338662585603267-4084702197021366041-7' : 
                                null; 
                    }, 
                    responseText: '' });
            } else if (req.type === 'PUT' && req.url === 'http://www.test.com/push/registrations/6989338662585603267-4084702197021366041-7') {
                // Check the body?
                $assert.areEqual(req.data, JSON.stringify(testRegistration));
                callback(null, { status: 200, responseText: '' });
            } else {
                callback(null, { status: 500, responseText: 'Oops, bad test case!' });                
            }            
        });

        // We want to test no local storage in this case
        // Refresh (id not present), Create (OK), Upsert (OK)
        var registrationManager = new RegistrationManager.RegistrationManager(client, 'platform', StorageKey);
        return registrationManager.upsertRegistration({ deviceId: 'testhandle', platform: 'platform'}).then(function () {
            // Now check storage is populated
            var testStore = new LocalStorageManager.LocalStorageManager(StorageKey);
            $assert.areEqual(testStore.pushHandle, 'testhandle');
            $assert.areEqual(testStore.getRegistrationIdWithName('$Default'), '6989338662585603267-4084702197021366041-7');
            DeleteStorage(testStore);
        });
    }),

    $test('UpsertRegistrationWithTemplateRefreshInsertSuccess')
    .description('Verify upsertRegistration updates new record when refresh is needed')
    .tag('upsertRegistration')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            testRegistration = {
                deviceId: 'testhandle',
                templateName: 'Test',
                templateBody: '{something:$(somethingelse)}',
                platform: 'platform',
                registrationId: '6989338662585603267-4084702197021366041-7',
            };

        client = client.withFilter(function (req, next, callback) {
            if (req.type === 'GET' && req.url === 'http://www.test.com/push/registrations?platform=platform&deviceId=testhandle') {
                callback(null, { status: 200, responseText: '[]' });
            } else if (req.type === 'POST' && req.url === 'http://www.test.com/push/registrationIds') {
                callback(null, 
                    { 
                        status: 200, 
                        getResponseHeader: function (header) { 
                            return header == 'Location' ? 
                                'http://philtotesting.azure-mobile.net/push/registrations/6989338662585603267-4084702197021366041-7' : 
                                null; 
                    }, 
                    responseText: '' });
            } else if (req.type === 'PUT' && req.url === 'http://www.test.com/push/registrations/6989338662585603267-4084702197021366041-7') {
                $assert.areEqual(req.data, JSON.stringify(testRegistration));
                callback(null, { status: 200, responseText: '' });
            } else {
                callback(null, { status: 500, responseText: 'Oops, bad test case!' });                
            }            
        });

        // We want to test no local storage in this case
        // Refresh (id not present), Create (OK), Upsert (OK)
        var registrationManager = new RegistrationManager.RegistrationManager(client, 'platform', StorageKey);
        return registrationManager.upsertRegistration({ deviceId: 'testhandle', templateName: 'Test', templateBody: '{something:$(somethingelse)}', platform: 'platform'}).then(function () {
            // Now check storage is populated
            var testStore = new LocalStorageManager.LocalStorageManager(StorageKey);
            $assert.areEqual(testStore.pushHandle, 'testhandle');            
            $assert.areEqual(testStore.getRegistrationIdWithName('Test'), '6989338662585603267-4084702197021366041-7');
            DeleteStorage(testStore);
        });
    }),

    $test('UpsertRegistrationWithRefreshListErrorFails')
    .description('Verify upsertRegistration fails when refresh goes bad')
    .tag('upsertRegistration')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");

        DeleteStorage();
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.url, 'http://www.test.com/push/registrations?platform=platform&deviceId=testhandle');
            callback(null, { status: 401, responseText: 'Invalid application key or something' });
        });

        // We want to test no local storage in this case
        // Refresh (id not present), Create (OK), Upsert (OK)
        var registrationManager = new RegistrationManager.RegistrationManager(client, 'platform', StorageKey);
        return registrationManager.upsertRegistration({ deviceId: 'testhandle', templateName: 'Test', platform: 'platform'}).then(function () {
            $assert.fail('Error should have happened');
        }, function (error) {
            $assert.isNotNull(error.message);

            // Now check storage still requires a refresh
            var testStore = new LocalStorageManager.LocalStorageManager(StorageKey);
            $assert.areEqual(testStore.pushHandle, '');
            $assert.areEqual(testStore.isRefreshNeeded, true);
            DeleteStorage(testStore);
        });
    }),

    $test('UpsertRegistrationNoCacheInsertSuccess')
    .description('Verify upsertRegistration adds new record, no refresh')
    .tag('upsertRegistration')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            testRegistration = {
                deviceId: 'testhandle',
                templateName: 'Test',
                platform: 'platform',
                registrationId: '6989338662585603267-4084702197021366041-7',
            };
        CreateStorage();

        client = client.withFilter(function (req, next, callback) {
            if (req.type === 'POST' && req.url === 'http://www.test.com/push/registrationIds') {
                callback(null, 
                    { 
                        status: 200, 
                        getResponseHeader: function (header) { 
                            return header == 'Location' ? 
                                'http://philtotesting.azure-mobile.net/push/registrations/6989338662585603267-4084702197021366041-7' : 
                                null; 
                    }, 
                    responseText: '' });
            } else if (req.type === 'PUT' && req.url === 'http://www.test.com/push/registrations/6989338662585603267-4084702197021366041-7') {
                $assert.areEqual(req.data, JSON.stringify(testRegistration));
                callback(null, { status: 200, responseText: '' });
            } else {
                callback(null, { status: 500, responseText: 'Oops, bad test case!' });                
            }            
        });

        // We want to test no local storage in this case
        // Refresh (id not present), Create (OK), Upsert (OK)
        var registrationManager = new RegistrationManager.RegistrationManager(client, 'platform', StorageKey);
        return registrationManager.upsertRegistration({ deviceId: 'testhandle', templateName: 'Test', platform: 'platform'}).then(function () {
            // Now check storage is populated
            var testStore = new LocalStorageManager.LocalStorageManager(StorageKey);
            $assert.areEqual(testStore.pushHandle, 'testhandle');
            $assert.areEqual(testStore.getRegistrationIdWithName('Test'), '6989338662585603267-4084702197021366041-7');
            $assert.areEqual(testStore.getRegistrationIdWithName('testtemplate'), '456');
            $assert.areEqual(testStore.getRegistrationIdWithName('$Default'), '123');
            DeleteStorage(testStore);
        });
    }),

    $test('UpsertRegistrationNoCacheInsertFails')
    .description('Verify upsertRegistration attempts to add a new record, but fails on create server call')
    .tag('upsertRegistration')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        CreateStorage();

        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.url, 'http://www.test.com/push/registrationIds');
            callback(null, { status: 401, responseText: 'Haha! No permission!' });
        });

        // We want to test no local storage in this case
        // Refresh (id not present), Create (OK), Upsert (OK)
        var registrationManager = new RegistrationManager.RegistrationManager(client, 'platform', StorageKey);
        return registrationManager.upsertRegistration({ deviceId: 'testhandle2', templateName: 'Test', platform: 'platform'}).then(null, function (error) {
            $assert.isNotNull(error.message);

            // Now check storage
            var testStore = new LocalStorageManager.LocalStorageManager(StorageKey);
            $assert.areEqual(testStore.pushHandle, 'testhandle');
            $assert.isNull(testStore.getRegistrationIdWithName('Test'));
            DeleteStorage(testStore);
        });
    }),

    $test('UpsertRegistrationWithCacheUpdateSuccess')
    .description('Verify upsertRegistration updates existing record')
    .tag('upsertRegistration')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            testRegistration = {
                deviceId: 'testhandle2',
                templateName: 'testtemplate',
                platform: 'platform',
                registrationId: '456',
            };
        CreateStorage();

        client = client.withFilter(function (req, next, callback) {
            if (req.type === 'PUT' && req.url === 'http://www.test.com/push/registrations/456') {
                // Check the body?
                $assert.areEqual(req.data, JSON.stringify(testRegistration));
                callback(null, { status: 200, responseText: '' });
            } else {
                callback(null, { status: 500, responseText: 'Oops, bad test case!' });                
            }            
        });

        // We want to test no local storage in this case
        // Refresh (id not present), Create (OK), Upsert (OK)
        var registrationManager = new RegistrationManager.RegistrationManager(client, 'platform', StorageKey);
        return registrationManager.upsertRegistration({ deviceId: 'testhandle2', templateName: 'testtemplate', platform: 'platform'}).then(function () {
            // Now check storage is populated
            var testStore = new LocalStorageManager.LocalStorageManager(StorageKey);
            $assert.areEqual(testStore.pushHandle, 'testhandle2');
            $assert.areEqual(testStore.getRegistrationIdWithName('testtemplate'), '456');
            $assert.areEqual(testStore.getRegistrationIdWithName('$Default'), '123');
            DeleteStorage(testStore);
        });
    }),

    $test('UpsertRegistrationWithCacheUpdateFails')
    .description('Verify upsertRegistration updates errors out on upsert fail (non 410)')
    .tag('upsertRegistration')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            testRegistration = {
                deviceId: 'testhandle2',
                templateName: 'testtemplate',
                platform: 'platform',
                registrationId: '456',
            };

        CreateStorage();
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'PUT');
            $assert.areEqual(req.url, 'http://www.test.com/push/registrations/456');
            callback(null, { status: 401, responseText: 'You really need to sign in, this is getting old' });
        });

        var registrationManager = new RegistrationManager.RegistrationManager(client, 'platform', StorageKey);
        return registrationManager.upsertRegistration({ deviceId: 'testhandle2', templateName: 'testtemplate', platform: 'platform'}).then(null, function (error) {
            $assert.isNotNull(error.message);

            // Now check storage is still as it was
            var testStore = new LocalStorageManager.LocalStorageManager(StorageKey);
            $assert.areEqual(testStore.pushHandle, 'testhandle');
            $assert.areEqual(testStore.getRegistrationIdWithName('testtemplate'), '456');
            $assert.areEqual(testStore.getRegistrationIdWithName('$Default'), '123');
            DeleteStorage(testStore);
        });
    }),

    $test('UpsertRegistrationExpiredSuccess')
    .description('Verify upsertRegistration updates and recreates on expired')
    .tag('upsertRegistration')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            testRegistration = {
                deviceId: 'testhandle2',
                templateName: 'testtemplate',
                platform: 'platform',
                registrationId: '456',
            },
            finalTestRegistration = {
                deviceId: 'testhandle2',
                templateName: 'testtemplate',
                platform: 'platform',
                registrationId: '789',
            };

        CreateStorage();
        client = client.withFilter(function (req, next, callback) {
            if (req.type === 'POST' && req.url === 'http://www.test.com/push/registrationIds') {
                callback(null, 
                    { 
                        status: 200, 
                        getResponseHeader: function (header) { 
                            return header == 'Location' ? 
                                'http://philtotesting.azure-mobile.net/push/registrations/789' : 
                                null; 
                    }, 
                    responseText: '' });
            } else if (req.type === 'PUT' && req.url === 'http://www.test.com/push/registrations/456') {
                $assert.areEqual(req.data, JSON.stringify(testRegistration));
                callback(null, { status: 410, responseText: 'Sorry this registration is expired' });
            } else if (req.type === 'PUT' && req.url === 'http://www.test.com/push/registrations/789') {
                $assert.areEqual(req.data, JSON.stringify(finalTestRegistration));
                callback(null, { status: 200, responseText: '' });
            } else {
                callback(null, { status: 500, responseText: 'Oops, bad test case!' });                
            }            
        });

        // We want to test no local storage in this case
        // Refresh (id not present), Create (OK), Upsert (OK)
        var registrationManager = new RegistrationManager.RegistrationManager(client, 'platform', StorageKey);
        return registrationManager.upsertRegistration({ deviceId: 'testhandle2', templateName: 'testtemplate', platform: 'platform'}).then(function () {
            // Now check storage is populated and updated
            var testStore = new LocalStorageManager.LocalStorageManager(StorageKey);
            $assert.areEqual(testStore.pushHandle, 'testhandle2');
            $assert.areEqual(testStore.getRegistrationIdWithName('testtemplate'), '789');
            $assert.areEqual(testStore.getRegistrationIdWithName('$Default'), '123');
            DeleteStorage(testStore);
        });
    }),

    $test('UupsertRegistrationExpiredCreateFails')
    .description('Verify upsertRegistration updates and recreates on expired and handles error on create')
    .tag('upsertRegistration')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            firstPost = true;

        CreateStorage();
        client = client.withFilter(function (req, next, callback) {
            if (firstPost && req.type === 'POST' && req.url === 'http://www.test.com/push/registrationIds') {
                firstPost = false;
                callback(null, { status: 401, responseText: 'Still no auth' });
            } else if (req.type === 'PUT' && req.url === 'http://www.test.com/push/registrations/456') {
                callback(null, { status: 410, responseText: 'Sorry this registration is expired' });
            } else {
                $assert.fail('unexpected request');
                callback(null, { status: 500, responseText: 'Oops, bad test case!' });                
            }            
        });

        // We want to test no local storage in this case
        // Refresh (id not present), Create (OK), Upsert (OK)
        var registrationManager = new RegistrationManager.RegistrationManager(client, 'platform', StorageKey);
        return registrationManager.upsertRegistration({ deviceId: 'testhandle2', templateName: 'testtemplate', platform: 'platform'}).then(null, function (error) {
            $assert.isNotNull(error.message);

            // Now check storage is populated and updated
            var testStore = new LocalStorageManager.LocalStorageManager(StorageKey);
            $assert.areEqual(testStore.pushHandle, 'testhandle');
            $assert.areEqual(testStore.getRegistrationIdWithName('testtemplate'), '456');
            $assert.areEqual(testStore.getRegistrationIdWithName('$Default'), '123');
            DeleteStorage(testStore);
        });
    }),

    $test('UpsertRegistrationExpiredUpdateFails')
    .description('Verify upsertRegistration updates and recreates on expired and handles error on update')
    .tag('upsertRegistration')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            finalTestRegistration = {
                deviceId: 'testhandle2',
                templateName: 'testtemplate',
                platform: 'platform',
                registrationId: '789',
            },
            firstPost = true;

        CreateStorage();
        client = client.withFilter(function (req, next, callback) {
            if (firstPost && req.type === 'POST' && req.url === 'http://www.test.com/push/registrationIds') {
                callback(null, 
                    { 
                        status: 200, 
                        getResponseHeader: function (header) { 
                            return header == 'Location' ? 
                                'http://philtotesting.azure-mobile.net/push/registrations/789' : 
                                null; 
                    }, 
                    responseText: '' });
                firstPost = false;
            } else if (req.type === 'PUT' && req.url === 'http://www.test.com/push/registrations/456') {
                callback(null, { status: 410, responseText: 'Sorry this registration is expired' });
            } else if (req.type === 'PUT' && req.url === 'http://www.test.com/push/registrations/789') {
                $assert.areEqual(req.data, JSON.stringify(finalTestRegistration));
                callback(null, { status: 401, responseText: 'Too slow, we expired your access' });
            } else {
                $assert.fail('unexpected request');
                callback(null, { status: 500, responseText: 'Oops, bad test case!' });                
            }            
        });

        // We want to test no local storage in this case
        // Refresh (id not present), Create (OK), Upsert (OK)
        var registrationManager = new RegistrationManager.RegistrationManager(client, 'platform', StorageKey);
        return registrationManager.upsertRegistration({ deviceId: 'testhandle2', templateName: 'testtemplate', platform: 'platform'}).then(null, function (error) {
            $assert.isNotNull(error.message);

            // Now check storage is populated and updated
            var testStore = new LocalStorageManager.LocalStorageManager(StorageKey);
            $assert.areEqual(testStore.pushHandle, 'testhandle2');
            $assert.areEqual(testStore.getRegistrationIdWithName('testtemplate'), '789');
            $assert.areEqual(testStore.getRegistrationIdWithName('$Default'), '123');
            DeleteStorage(testStore);
        });
    })
);

var StorageKey = 'storageKey';

function CreateStorage() {
    var localStore = new LocalStorageManager.LocalStorageManager(StorageKey);
    localStore.updateAllRegistrations([{ registrationId: '123' }, { templateName: 'testtemplate', registrationId: '456' }], 'testhandle');

    return localStore;
}

function DeleteStorage(localStore) {
    if (!localStore) {
        localStore = new LocalStorageManager.LocalStorageManager(StorageKey);
    }

    localStore._pushHandle = null;
    localStore.deleteAllRegistrations();
}