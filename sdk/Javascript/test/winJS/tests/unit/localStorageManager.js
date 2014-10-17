// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="..\..\js\MobileServices.Internals.js" />
/// <reference path="..\..\generated\Tests.js" />

// Declare JSHint globals
/*global Platform:false,LocalStorageManager:false */

$testGroup('LocalStorageManager',
    $test('store.getRegistrationIds with empty store')
    .description('Verify listRegistrations parses an array')
    .check(function () {
        clearStorage();

        var localStore = new LocalStorageManager.LocalStorageManager(StorageKey),
            idList = localStore.getRegistrationIds();

        $assert.isTrue(Array.isArray(idList));
        $assert.areEqual(idList.length, 0);
        $assert.isTrue(localStore.isRefreshNeeded);
    }),

    $test('store.getRegistrationIds with existing ids')
    .description('Verify listRegistrations parses an array')
    .check(function () {
        populateStorage();
        var localStore = new LocalStorageManager.LocalStorageManager(StorageKey),
            idList = localStore.getRegistrationIds();

        $assert.isTrue(Array.isArray(idList));
        $assert.areEqual(idList.length, 3);
        $assert.areEqual(idList[0], '1');
        $assert.areEqual(idList[1], '2');
        $assert.areEqual(idList[2], '3');
        $assert.isFalse(localStore.isRefreshNeeded);

        clearStorage();
    }),

    $test('store.getRegistrationIds version changed')
    .description('Verify listRegistrations needs refreshed on version change')
    .check(function () {
        Platform.writeSetting('MobileServices.Push.' + StorageKey, JSON.stringify({
            Version: 'v1.0.0',
            ChannelUri: 'oldChannel',
            Registrations: {
                'A': JSON.stringify({
                    registrationId: '1',
                    registrationName: 'A'
                }),
                'B': JSON.stringify({
                    registrationId: '2',
                    registrationName: 'B'
                })
            }}));

        var localStore = new LocalStorageManager.LocalStorageManager(StorageKey),
            idList = localStore.getRegistrationIds();

        $assert.isTrue(Array.isArray(idList));
        $assert.areEqual(idList.length, 0);
        $assert.isTrue(localStore.isRefreshNeeded);
        $assert.areEqual(localStore.pushHandle, 'oldChannel');

        clearStorage();
    }),

    $test('store.getRegistrationIdWithName success')
    .description('Verify getRegistrationIdWithName works with valid name')
    .check(function () {
        populateStorage();
        var localStore = new LocalStorageManager.LocalStorageManager(StorageKey),
            registrationId = localStore.getRegistrationIdWithName('B');

        $assert.isNotNull(registrationId);
        $assert.areEqual(registrationId, '2');

        clearStorage();
    }),

    $test('store.getRegistrationIdWithName for valid name but not present')
    .description('Verify getRegistrationIdWithName works with valid name')
    .check(function () {
        populateStorage();
        var localStore = new LocalStorageManager.LocalStorageManager(StorageKey),
            registrationId = localStore.getRegistrationIdWithName('D');

        $assert.isNull(registrationId);

        clearStorage();
    }),

    $test('store.getRegistrationIdWithName throws')
    .description('Verify getRegistrationIdWithName throws when no name given')
    .check(function () {
        populateStorage();
        var localStore = new LocalStorageManager.LocalStorageManager(StorageKey);

        try {
            var registrationId = localStore.getRegistrationIdWithName();
            $assert.fail('Expected error');
        } catch (e) {
            $assert.isNotNull(e);
        }
        clearStorage();
    }),

    $test('store.updateAllRegistrations throws')
    .description('Verify updateAllRegistrations throws when no name given')
    .check(function () {
        var storage = populateStorage(),
            localStore = new LocalStorageManager.LocalStorageManager(StorageKey);

        try {
            var registrationId = localStore.updateAllRegistrations();
            $assert.fail('Expected error');
        } catch (e) {
            $assert.isNotNull(e);
        }

        clearStorage();
    }),

    $test('store.updateAllRegistrations clears')
    .description('Verify updateAllRegistrations removes all when passed null')
    .check(function () {
        populateStorage();
        var localStore = new LocalStorageManager.LocalStorageManager(StorageKey);

        localStore.updateAllRegistrations(null, 'myhandle');

        var idList = localStore.getRegistrationIds();
        $assert.areEqual(idList.length, 0);
        $assert.areEqual(Platform.readSetting(FullStorageKey), '{"Version":"v1.1.0","ChannelUri":"myhandle","Registrations":{}}');

        clearStorage();
    }),

    $test('store.updateAllRegistrations populates list')
    .description('Verify updateAllRegistrations updates all when passed list')
    .check(function () {
        populateStorage();
        var localStore = new LocalStorageManager.LocalStorageManager(StorageKey),
            newRegistrations = [{
                registrationId: '25',
                templateName: 'Y'
            }, {
                registrationId: '26',
            }];

        localStore.updateAllRegistrations(newRegistrations, 'myhandle');

        var idList = localStore.getRegistrationIds();
        $assert.areEqual(idList.length, 2);
        $assert.areEqual(localStore.getRegistrationIdWithName('Y'), '25');
        $assert.areEqual(localStore.getRegistrationIdWithName('$Default'), '26');
        $assert.areEqual(Platform.readSetting(FullStorageKey), '{"Version":"v1.1.0","ChannelUri":"myhandle","Registrations":{"Y":"25","$Default":"26"}}');

        clearStorage();
    }),

    $test('store.updateRegistrationWithName updates a registration')
    .description('Verify updateRegistrationWithName updates reg and handle')
    .check(function () {
        storage = populateStorage();
        var localStore = new LocalStorageManager.LocalStorageManager(StorageKey);

        localStore.updateRegistrationWithName('A', '_1', 'handle');

        $assert.areEqual(localStore.getRegistrationIds().length, 3);
        $assert.areEqual(localStore.getRegistrationIdWithName('A'), '_1');        
        $assert.areEqual(localStore.pushHandle, 'handle');
        $assert.areEqual(Platform.readSetting(FullStorageKey), '{"Version":"v1.1.0","ChannelUri":"handle","Registrations":{"A":"_1","B":"2","C":"3"}}');

        clearStorage(storage);
    }),

    $test('store.updateRegistrationWithName throws')
    .description('Verify updateRegistrationWithName throws when no name given')
    .check(function () {
        populateStorage();
        var localStore = new LocalStorageManager.LocalStorageManager(StorageKey),
            registrationId;

        try {
            registrationId = localStore.updateRegistrationWithName(null, '_1', 'handle');
            $assert.fail('Expected error');
        } catch (e) {
            $assert.isNotNull(e);
        }

        try {
            registrationId = localStore.updateRegistrationWithName('A', '', 'handle');
            $assert.fail('Expected error');
        } catch (e) {
            $assert.isNotNull(e);
        }

        try {
            registrationId = localStore.updateRegistrationWithName('A', '_1', null);
            $assert.fail('Expected error');
        } catch (e) {
            $assert.isNotNull(e);
        }

        clearStorage();
    }),

    $test('store.deleteRegistrationWithName success')
    .description('Verify deleteRegistrationWithName removes registration')
    .check(function () {
        populateStorage();
        var localStore = new LocalStorageManager.LocalStorageManager(StorageKey);

        localStore.deleteRegistrationWithName('C');
        $assert.areEqual(localStore.getRegistrationIds().length, 2);
        $assert.isNull(localStore.getRegistrationIdWithName('C'));
        $assert.areEqual(Platform.readSetting(FullStorageKey), '{"Version":"v1.1.0","ChannelUri":"myhandle","Registrations":{"A":"1","B":"2"}}');

        clearStorage();
    }),

    $test('store.deleteRegistrationWithName success when no registration')
    .description('Verify deleteRegistrationWithName succeeds when no registration found')
    .check(function () {
        populateStorage();
        var localStore = new LocalStorageManager.LocalStorageManager(StorageKey);

        localStore.deleteRegistrationWithName('D');
        $assert.areEqual(localStore.getRegistrationIds().length, 3);
        $assert.areEqual(Platform.readSetting(FullStorageKey), '{"Version":"v1.1.0","ChannelUri":"myhandle","Registrations":{"A":"1","B":"2","C":"3"}}');

        clearStorage();
    }),

    $test('store.deleteRegistrationWithName throws')
    .description('Verify deleteRegistrationWithName throws when no name given')
    .check(function () {
        populateStorage();
        var localStore = new LocalStorageManager.LocalStorageManager(StorageKey),
            registrationId;

        try {
            registrationId = localStore.deleteRegistrationWithName();
            $assert.fail('Expected error');
        } catch (e) {
            $assert.isNotNull(e);
        }

        try {
            registrationId = localStore.deleteRegistrationWithName('');
            $assert.fail('Expected error');
        } catch (e) {
            $assert.isNotNull(e);
        }

        clearStorage();
    }),

    $test('store.deleteAllRegistrations success')
    .description('Verify deleteAllRegistrations removes all registration')
    .check(function () {
        populateStorage();
        var localStore = new LocalStorageManager.LocalStorageManager(StorageKey);

        localStore.deleteAllRegistrations();
        $assert.areEqual(localStore.getRegistrationIds().length, 0);
        $assert.areEqual(localStore.pushHandle, 'myhandle');
        $assert.areEqual(Platform.readSetting(FullStorageKey), '{"Version":"v1.1.0","ChannelUri":"myhandle","Registrations":{}}');

        clearStorage();
    })
);

var StorageKey = 'mystoragekey',
    FullStorageKey = 'MobileServices.Push.' + StorageKey;

function populateStorage() {
    Platform.writeSetting(FullStorageKey, JSON.stringify({
        Version: 'v1.1.0',
        ChannelUri: 'myhandle',
        Registrations:  {
            'A': '1',
            'B': '2',
            'C': '3'
        }
    }));

}

function clearStorage() {
    Platform.writeSetting(FullStorageKey, null);
}