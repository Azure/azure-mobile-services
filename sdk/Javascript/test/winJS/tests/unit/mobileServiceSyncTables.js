// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.2.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.2.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.2.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.2.0\js\ui.js" />
/// <reference path="..\..\js\MobileServices.Internals.js" />
/// <reference path="..\..\generated\Tests.js" />

// Declare JSHint globals
/*global MobileServiceClient:false */

var testData = require("constants");

var testServiceUrl = "http://test.com";
var testServiceKey = "key";
var testTableName = "items";

function createMobileServiceClient() {
    return new WindowsAzure.MobileServiceClient(testServiceUrl, testServiceKey);
}

$testGroup('MobileServiceSyncTables.js',

    $test('Successful lookup')
    .description('Verify lookup of an inserted item succeeds')
    .checkAsync(function() {

        var client = createMobileServiceClient();
        client.syncContext.initialize();

        var syncTable = client.getSyncTable(testTableName);

        var rowObject = {
            "id": "pen",
            "description": "reynolds",
            "price": 51
        };

        return syncTable.insert(rowObject).then(function() {
            return syncTable.lookup(rowObject.id);
        }, function() {
            $assert.fail("Expected insert to be successful");
        })
        .then(function(result) {
            $assert.isTrue(JSON.stringify(rowObject) === JSON.stringify(result), "MobileServiceSyncTable lookup failed");
        }, function () {
            $assert.fail("Lookup should have succeeded");
        });
    }),

    $test('Lookup a non existent id')
    .description('Verify lookup of an item that does not exist succeeds')
    .checkAsync(function () {

        var client = createMobileServiceClient();
        client.syncContext.initialize();

        var syncTable = client.getSyncTable(testTableName);

        var rowObject = {
            "id": "pen",
            "description": "reynolds",
            "price": 51
        };

        // Lookup a non existent id.
        return syncTable.insert(rowObject).then(function () {
            return syncTable.lookup("some non existent id");
        }, function () {
            $assert.fail('Insert should have succeeded');
        })
        .then(function (result) {
            $assert.isTrue(result === undefined);
        }, function () {
            $assert.fail("Expected lookup to be successful");
        });
    }),

    $test('Lookup in a non existent table')
    .description('Verify lookup in a non existent table fails')
    .checkAsync(function () {

        var client = createMobileServiceClient();
        client.syncContext.initialize();

        var syncTable = client.getSyncTable("some non existent table name");

        // Perform lookup in a table that hasn't been created yet
        return syncTable.lookup("some item id").then(function() {
            $assert.fail('Lookup should have failed');
        }, function (error) {
            $assert.isTrue(error !== undefined);
            $assert.isTrue(error.message !== undefined);
        });
    })
);
