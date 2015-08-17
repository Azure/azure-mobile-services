// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.2.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.2.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.2.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.2.0\js\ui.js" />
/// <reference path="..\..\js\MobileServices.Internals.js" />
/// <reference path="..\..\generated\Tests.js" />

// Declare JSHint globals
/*global MobileServiceClient:false, Platform:false */

$testGroup('Dates')
    .tests(
        $test('DateUri')
        .description('Verify the date URI is sent in the correct format')
        .checkAsync(function () {            
            var client = new WindowsAzure.MobileServiceClient("http://www.windowsazure.com/");
            client = client.withFilter(function (req, next, callback) {
                $assert.areEqual(req.url, "http://www.windowsazure.com/tables/books?$filter=(date eq datetime'2009-11-21T14:22:59.860Z')");
                callback(null, { status: 200, responseText: null });
            });
            var table = client.getTable('books');
            var dateValue = new Date(Date.UTC(2009, 10, 21, 14, 22, 59, 860));
            return table.where({ date: dateValue }).read();
        }),

        $test('DateUri - getUTCDate test')
        .description('Verify query is correct using UTC functions')
        .checkAsync(function () {
            var client = new WindowsAzure.MobileServiceClient("http://www.windowsazure.com/");
            client = client.withFilter(function (req, next, callback) {
                $assert.areEqual(req.url, "http://www.windowsazure.com/tables/books?$filter=(day(date) eq 1)");
                callback(null, { status: 200, responseText: null });
            });
            var table = client.getTable('books');
            return table.where(function () { return this.date.getUTCDate() === 1; }).read().then(function (results) {

            });
        }),

        $test('DateUri - getUTCMonth test')
        .description('Verify query is correct using UTC month functions')
        .checkAsync(function () {
            var client = new WindowsAzure.MobileServiceClient("http://www.windowsazure.com/");
            client = client.withFilter(function (req, next, callback) {
                $assert.areEqual(req.url, "http://www.windowsazure.com/tables/books?$filter=((month(date) sub 1) eq 9)");
                callback(null, { status: 200, responseText: null });
            });
            var table = client.getTable('books');
            return table.where(function () { return this.date.getUTCMonth() === 9; }).read().then(function (results) {

            });
        }),

        $test('DateUri - getUTCYear test')
        .description('Verify query is correct using UTC year functions')
        .checkAsync(function () {
            var client = new WindowsAzure.MobileServiceClient("http://www.windowsazure.com/");
            client = client.withFilter(function (req, next, callback) {
                $assert.areEqual(req.url, "http://www.windowsazure.com/tables/books?$filter=(year(date) eq 2013)");
                callback(null, { status: 200, responseText: null });
            });
            var table = client.getTable('books');
            return table.where(function () { return this.date.getUTCFullYear() === 2013; }).read().then(function (results) {

            });
        }),

        $test('DateUri - getYear test')
        .description('Verify query is correct using get year function')
        .checkAsync(function () {
            var client = new WindowsAzure.MobileServiceClient("http://www.windowsazure.com/");
            client = client.withFilter(function (req, next, callback) {
                $assert.areEqual(req.url, "http://www.windowsazure.com/tables/books?$filter=((year(date) sub 1900) eq 113)");
                callback(null, { status: 200, responseText: null });
            });
            var table = client.getTable('books');
            return table.where(function () { return this.date.getYear() === 113; }).read().then(function (results) {

            });
        }),

        $test('InsertAndQuery')
        .description('Insert a date, query for it, and verify the result')
        .functional()
        .checkAsync(function () {
            var client = $getClient().withFilter(function (req, next, callback) {
                $log($fmt('>>> {0} {1}   {2}', req.type, req.url, req.data));
                next(req, function (err, rsp) {
                    $log($fmt('<<< {0} {1}   {2}', rsp.status, rsp.statusText, rsp.responseText));
                    callback(err, rsp);
                });
            });

            var table = client.getTable('test_table');
            var dateValue = new Date(2009, 10, 21, 14, 22, 59, 860);
            $log($fmt('Start: {0}', dateValue));

            return $chain(
                function () {
                    $log('Inserting instance');
                    return table.insert({ date: dateValue });
                },
                function (item) {
                    $log('Verifying insert response date');
                    $assert.areEqual(dateValue.valueOf(), item.date.valueOf());

                    $log('Querying for instance');
                    return table.where({ date: dateValue }).where($isDotNet() ? 'Id eq \'' + item.id + '\'' : 'id ge ' + item.id).read();
                },
                function (items) {
                    $log('Verifying query response date');
                    $assert.areEqual(1, items.length);
                    $assert.areEqual(dateValue.valueOf(), items[0].date.valueOf());
                    $log($fmt('Finish: {0}', items[0].date));
                });
        })
    );