// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="..\..\js\MobileServices.Internals.js" />
/// <reference path="..\..\generated\Tests.js" />

// Delcare JSHint globals
/*global MobileServiceClient:false */

$testGroup('Functional Basics')
    .functional()
    .tests(
        $test('Create Client')
        .description('Verify creation of the client with the server uri')
        .check(function () {
            $getClient();
        }),

        $test('Get elements')
        .description('Request some elements from the table and make sure we get something')
        .checkAsync(function () {
            var client = $getClient();
            var books = client.getTable('books');
            return books.read().then(function (results) {
                $assert.isTrue(results, 'expected results!');
                $assert.isTrue(results.length > 0, 'expected non-empty results!');
            });
        }),

        $test('Simple query')
        .description('Perform a simple query with a few operations')
        .checkAsync(function () {
            var client = $getClient();
            var books = client.getTable('books');
            return books.select('price').where('price gt 0').orderBy('price').take(5).read().then(function (results) {
                $assert.isTrue(results, 'expected results!');
                $assert.isTrue(results.length, 5, 'expected 5 results!');
                $assert.isTrue(results[0].title === undefined, 'ensure selected fields');
                $assert.isTrue(results[0].price <= results[1].price, 'ensure ordered');
            });
        }),

        $test('offline').exclude('This test will only pass when there is no network connection!')
        .description('Verify the offline error message.')        
        .checkAsync(function() {
            var client = new WindowsAzure.MobileServiceClient('http://www.test.com');
            var books = client.getTable('books');
            return books.read().then(function (results) {
                $assert.fail('Should have failed - please make sure there is no network connection for this test.');
            }, function(error) {
                $assert.areEqual(error.message, 'Unexpected connection failure.');
                $assert.areEqual(error.request.status, 0);
            });
        })
    );
