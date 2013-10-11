// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="..\..\js\MobileServices.Internals.js" />
/// <reference path="..\..\generated\Tests.js" />

// Declare JSHint globals
/*global MobileServiceClient:false */

var validStringIds = [
            "id",
            "true",
            "false",
            "00000000-0000-0000-0000-000000000000",
            "aa4da0b5-308c-4877-a5d2-03f274632636",
            "69C8BE62-A09F-4638-9A9C-6B448E9ED4E7",
            "{EC26F57E-1E65-4A90-B949-0661159D0546}",
            "87D5B05C93614F8EBFADF7BC10F7AE8C",
            "someone@someplace.com",
            "id with spaces",
            "...",
            " .",
            "'id' with single quotes",
            "id with 255 characters " + new Array(257-24).join('A'),
            "id with Japanese 私の車はどこですか？",
            "id with Arabic أين هو سيارتي؟",
            "id with Russian Где моя машина",
            "id with some URL significant characters % # &",
            "id with allowed ascii characters  !#$%&'()*,-.0123456789:;<=>@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_abcdefghijklmnopqrstuvwxyz{|}",
            "id with allowed extended ascii characters ¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþ"
        ];

var emptyStringIds = [""];
var invalidStringIds = [
            ".",
            "..",
            "id with 256 characters " + new Array(257 - 23).join('A'),
            "\r",
            "\n",
            "\t",
            "id\twith\ttabs",
            "id\rwith\rreturns",
            "id\nwith\n\newline",
            "id with backslash \\",
            "id with forwardslash \/",
            "1/8/2010 8:00:00 AM",
            "\"idWithQuotes\"",
            "?",
            "\\",
            "\/",
            "`",
            "+"
            //0-32 characters
            //127-160 chatacters
    ];

var validIntIds = [1, 925000];
var invalidIntIds = [-1];
var nonStringNonIntValidJsonIds = [
    true,
    false,
    1.0,
    -1.0,
    0.0,
    ];

var nonStringNonIntIds = [
    new Date(2010, 1, 8),
    {},
    1.0,
    "aa4da0b5-308c-4877-a5d2-03f274632636"
];

$testGroup('MobileServiceTables.js',
    $test('table.read() with no id results')
    .description('Verify MobileTableService.get returns the results')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'GET');
            $assert.areEqual(req.url, 'http://www.test.com/tables/books?$filter=price eq 100');
            callback(null, { status: 200, responseText: '{"title":"test"}' });
        });

        var table = client.getTable('books');
        return table.read('$filter=price eq 100').then(function (results) {
            $assert.areEqual(results.title, 'test');
        });
    }),

    $test('table.read() with null id results')
    .description('Verify MobileTableService.get returns the results')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            callback(null, { status: 200, responseText: '{"id":null, "title":"test"}' });
        });

        var table = client.getTable('books');
        return table.read().then(function (results) {
            $assert.areEqual(results.id, null);
            $assert.areEqual(results.title, 'test');
        });
    }),

    $test('table.read() with any id response content')
    .description('Verify table.read returns the results')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            testIdData = validStringIds.concat(emptyStringIds).concat(invalidStringIds).concat(validIntIds).concat(invalidIntIds).concat(nonStringNonIntValidJsonIds),
            testCases = [];

        testIdData.forEach(function (stringId) {
            testCases.push(function () {
                client = client.withFilter(function (req, next, callback) {
                    callback(null, { status: 200, responseText: '[{"id":' + JSON.stringify(stringId) + '}]' });
                });

                return client.getTable('books').read().then(function (results) {
                    $assert.areEqual(results.length, 1);
                    $assert.areEqual(results[0].id, stringId);
                });
            });
        });
        return $chain.apply(null, testCases);
    }),

    $test('table.read() with string id filter')
    .description('Verify table.read returns the results')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            testIdData = validStringIds.concat(emptyStringIds).concat(invalidStringIds),
            testCases = [];

        testIdData.forEach(function (testId) {
            testCases.push(function () {
                client = client.withFilter(function (req, next, callback) {
                    $assert.areEqual(req.url, "http://www.test.com/tables/books?$filter=id eq \'" + encodeURIComponent(testId) + "\'");
                    callback(null, { status: 200, responseText: '[{"id":' + JSON.stringify(testId) + '}]' });
                });

                var filter = "$filter=id eq '" + encodeURIComponent(testId) + "'";
                return client.getTable('books').read(filter).then(function (results) {
                    $assert.areEqual(results.length, 1);
                });
            });
        });
        return $chain.apply(null, testCases);
    }),

    $test('table.read() with null id filter')
    .description('Verify MobileTableService.get returns the results')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.url, "http://www.test.com/tables/books?$filter=id eq null");
            callback(null, { status: 200, responseText: '{"id":null, "title":"test"}' });
        });

        var table = client.getTable('books');
        return table.read('$filter=id eq null').then(function (results) {
            $assert.areEqual(results.id, null);
            $assert.areEqual(results.title, 'test');
        });
    }),

    $test('query via table.read(query)')
    .description('Verify MobileTableService.query created a correct deferred query')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.url, 'http://www.test.com/tables/books?$filter=(price eq 100)&$orderby=title&$select=price');
            callback(null, { status: 200, responseText: '{"title":"test"}' });
        });
        
        var table = client.getTable('books');
        var query2 = table.where({ showComments: true });
        var query = table.where(function (context) { return (this.price == context.price); }, { price: 100 }).orderBy('title').select('price');
        return table.read(query).then(function (results) {
            $assert.areEqual(results.title, 'test');
        });
    }),

    $test('query table with empty select()')
    .description('Verify getTable.select with null string does not throw an error')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.url, 'http://www.test.com/tables/books');
            callback(null, { status: 200, responseText: '{"title":"test"}' });
        });

        return client.getTable('books').select('').read().then(function (results) {
            $assert.areEqual(results.title, 'test');
        }, function (error) {
            $assert.fail('Call should not have failed');
        });
    }),
    
    $test('query via table.read(query)')
    .description('Verify MobileTableService.query created a URL encoded query')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.url, 'http://www.test.com/tables/books?$filter=(title eq \'How%20to%20dial%20this%20%23%20%26%20such%20\'\'stuff\'\'%3F\')');
            callback(null, { status: 200, responseText: '{"title":"test"}' });
        });

        var table = client.getTable('books');
        var query = table.where(function () { return (this.title == 'How to dial this # & such \'stuff\'?'); });
        return table.read(query).then(function (results) {
            $assert.areEqual(results.title, 'test');
        });
    }),

    $test('query via table.read(query) with user-defined query parameters')
    .description('Verify MobileTableService.query created a correct deferred query')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.url, 'http://www.test.com/tables/books?$filter=(price eq 100)&$orderby=title&$select=price&state=CA&tags=%23pizza%20%23beer');
            callback(null, { status: 200, responseText: '{"title":"test"}' });
        });

        var table = client.getTable('books');
        var query2 = table.where({ showComments: true });
        var query = table.where(function () { return this.price == 100; }).orderBy('title').select('price');
        return table.read(query, { state: 'CA', tags: '#pizza #beer' }).then(function (results) {
            $assert.areEqual(results.title, 'test');
        });
    }),

    $test('query via table.read(query) with invalid user-defined query parameters')
    .description('Verify MobileTableService.query fails with incorrect user-defined parameters')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            callback(null, { status: 200, responseText: '{"title":"test"}' });
        });

        var table = client.getTable('books');
        var query2 = table.where({ showComments: true });
        var query = table.where(function () { return this.price == 100; }).orderBy('title').select('price');
        return table.read(query, { state: 'CA', $invalid: 'since it starts with a \'$\'' }).then(function (results) {
            $assert.fail('Should have failed.');
        }, function (error) {
            $assert.contains(error.message, 'User-defined query string');
        });
    }),

    $test('query via table.read()')
    .description('Verify MobileServiceTable.read implies a default query')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.url, 'http://www.test.com/tables/books');
            callback(null, { status: 200, responseText: '{"title":"test"}' });
        });

        var table = client.getTable('books');
        return table.read().then(function (results) {
            $assert.areEqual(results.title, 'test');
        });
    }),

    $test('query via table.read() with user-defined parameters')
    .description('Verify MobileServiceTable.read implies a default query even when used with user-defined query string parmeters')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.url, 'http://www.test.com/tables/books?state=PA');
            callback(null, { status: 200, responseText: '{"title":"test"}' });
        });

        var table = client.getTable('books');
        return table.read({ state: 'PA' }).then(function (results) {
            $assert.areEqual(results.title, 'test');
        });
    }),

    $test('query via query.read()')
    .description('Verify MobileTableService.query created a correct deferred query')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.url, 'http://www.test.com/tables/books?$filter=(price eq 100)&$orderby=title&$select=price');
            callback(null, { status: 200, responseText: '{"title":"test"}' });
        });

        var table = client.getTable('books');
        var query =
            table
            .where(function () { return this.price == 100; })
            .orderBy('title')
            .select('price')
            .read()
            .then(function (results) {
                $assert.areEqual(results.title, 'test');
            });

        // Note: separate return statement because of awesome JS syntax quirk
        // where a "return" on a line by itself will be considered "return;"
        return query;
    }),

    $test('query via table.read()')
    .description('Verify MobileServiceTable.read sends a request for the table')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.url, 'http://www.test.com/tables/books');
            callback(null, { status: 200, responseText: '{"title":"test"}' });
        });

        var table = client.getTable('books');
        return table.read().then(function (results) {
            $assert.areEqual(results.title, 'test');
        });
    }),

    $test('query matches table')
    .description('Verify that a query created from one table is not used with another table')
    .check(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        
        var t1 = client.getTable('books');
        var t2 = client.getTable('users');

        var q1 = t1.orderBy('price');
        return t2.read(q1).then(
            function () { },
            function (ex) {
                $assert.contains(ex.message, 'books');
                $assert.contains(ex.message, 'users');
            });
    }),

    $test('query projection')
    .description('Verify MobileTableService.query correctly applied a projection')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.url, 'http://www.test.com/tables/books?$filter=(price eq 100)&$orderby=title');
            callback(null, { status: 200, responseText: '[{"title":"test"}]' });
        });

        var table = client.getTable('books');
        var query =
            table
            .where(function () { return this.price == 100; })
            .orderBy('title')
            .select(function () {
                this.title = this.title.toUpperCase();
                return this;
            });
        return table.read(query).then(function (results) {
            $assert.areEqual(results[0].title, 'TEST');
        });
    }),

    $test('query with two methods')
    .description('Verify where clauses using two javascript methods work correctly')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.url, "http://www.test.com/tables/books?$filter=(indexof(tolower(Title),'pirate') ge 0)");
            callback(null, { status: 200, responseText: '[{"title":"test"}]' });
        });

        var table = client.getTable('books');
        var query = table.where(function () { return ((this.Title.toLowerCase().indexOf('pirate')) >= 0); });
        return table.read(query).then(function (results) {
            $assert.areEqual(results[0].title, 'test');
        });
    }),

    $test('insert')
    .description('Verify MobileTableService.insert')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            $assert.areEqual(req.url, 'http://www.test.com/tables/books?state=CA');
            $assert.areEqual(req.data, '{"price":100}');
            callback(null, { status: 200, responseText: '{"title":"test"}' });
        });

        var table = client.getTable('books');
        var originalModelObject = { price: 100 };
        return table.insert(originalModelObject, { state: 'CA' }).then(function (results) {
            var isWinJs = typeof Windows === "object";
            if (isWinJs) {
                // For backward compatibility, the WinJS client mutates the original model
                // object by adding/overwriting properties from the server response
                $assert.isTrue(results === originalModelObject);
                $assert.areEqual(results.price, 100);    // We have the original properties
                $assert.areEqual(results.title, 'test'); // Combined with the server response properties
            } else {
                // The web client simply returns the server response without mutating your
                // original model, which is a lot more useful if the server response might
                // not be the same kind of object
                $assert.isFalse(results === originalModelObject);
                $assert.areEqual(results.title, 'test');    // We have the server response properties
                $assert.areEqual(results.price, undefined); // But not the original model properties
                $assert.areEqual(originalModelObject.title, undefined); // And the original is unchanged
                $assert.areEqual(originalModelObject.price, 100);
            }
        });
    }),

    $test('table.insert() success with valid ids (string only)')
    .description('Verify table.insert works with all valid ids')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            testIdData = validStringIds,
            testCases = [];
            
        testIdData.forEach(function (testId) {
            testCases.push(function () {
                var originalModelObject = { id: testId, price: 100 };

                client = client.withFilter(function (req, next, callback) {
                    $assert.areEqual(req.type, 'POST');
                    $assert.areEqual(req.url, 'http://www.test.com/tables/books');
                    $assert.areEqual(req.data, '{"id":' + JSON.stringify(testId) + ',"price":100}');
                    callback(null, { status: 200, responseText: '{"id":' + JSON.stringify(testId) + ', "title":"test"}' });
                });

                return client.getTable('books').insert(originalModelObject).then(function (result) {
                    $assert.areEqual(result.id, testId);
                });
            });
        });
        return $chain.apply(null, testCases);
    }),

    $test('table.insert() throws with invalid ids')
    .description('Verify table.insert fails with non string ids or invalid string ids')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            testIdData = validIntIds.concat(invalidStringIds).concat(invalidIntIds),
            testCases = [];

        testIdData.forEach(function (testId) {
            testCases.push(function () {
                var originalModelObject = { id: testId, price: 100 };

                client = client.withFilter(function (req, next, callback) {
                    callback(null, { status: 200, responseText: req.data });
                });

                return client.getTable('books').insert(originalModelObject).then(function (result) {
                    $assert.fail('Should have failed');
                }, function (error) {
                    $assert.isTrue(error.message.indexOf('member is already set') !== -1 ||
                                   error.message.indexOf('is not valid') !== -1)
                });
            });
        });
        return $chain.apply(null, testCases);
    }),

    $test('table.insert() can return any possible id')
    .description('Verify table.insert can return all valid ids')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            testIdData = validStringIds.concat(validIntIds).concat(invalidIntIds).concat(invalidStringIds),
            testCases = [];

        testIdData.forEach(function (testId) {
            testCases.push(function () {
                var originalModelObject = { id: 'magic', price: 100 };

                client = client.withFilter(function (req, next, callback) {
                    callback(null, { status: 200, responseText: '{"id":' + JSON.stringify(testId) + '}' });
                });

                return client.getTable('books').insert(originalModelObject).then(function (result) {
                    $assert.areEqual(result.id, testId);
                });
            });
        });
        return $chain.apply(null, testCases);
    }),


    $test('insertThrows')
    .description('Verify MobileTableService.insert throws on an existing id')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            callback(null, { status: 200, responseText: '{"title":"test"}' });
        });

        var table = client.getTable('books');
        return table.insert({ id: 100 }).then(function (results) {
            $assert.fail('Should have failed.');
        }, function (error) {
            $assert.contains(error.message, 'id');
        });
    }),

    $test('insertThrows IncorrectCaseID Check')
    .description('Verify MobileTableService.insert throws on incorrect case of id')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            callback(null, { status: 200, responseText: '{"title":"test"}' });
        });

        var table = client.getTable('books');
        return table.insert({ ID: 100 }).then(function (results) {
            $assert.fail('Should have failed.');
        }, function (error) {
            $assert.contains(error.message, 'id');
        });
    }),

    $test('insert success with Id of zero')
    .description('Verify MobileTableService.insert succeeds on an id of 0')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            callback(null, { status: 200, responseText: '{"title":"test"}' });
        });

        var table = client.getTable('books');
        return table.insert({ id: 0 }).then(function (results) {
            $assert.areEqual(results.title, 'test');
        }, function (error) {
            $assert.fail('Should have succeeded.');
        });
    }),

    $test('insert success with empty string as Id')
    .description('Verify MobileTableService.insert succeeds on an id of empty string')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            callback(null, { status: 200, responseText: '{"title":"test"}' });
        });

        var table = client.getTable('books');
        return table.insert({ id: '' }).then(function (results) {
            $assert.areEqual(results.title, 'test');
        }, function (error) {
            $assert.fail('Should have succeeded.');
        });
    }),

    $test('insert success with a string as Id')
    .description('Verify MobileTableService.insert succeeds on an id of empty string')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            callback(null, { status: 200, responseText: '{"title":"test"}' });
        });

        var table = client.getTable('books');
        return table.insert({ id: 'foo' }).then(function (results) {
            $assert.areEqual(results.title, 'test');
        }, function (error) {
            $assert.fail('Should have succeeded.');
        });
    }),

    $test('insert success can return string Id')
    .description('Verify MobileTableService.insert succeeds on a string id')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            callback(null, { status: 200, responseText: '{"id":"alpha", "title":"test"}' });
        });

        var table = client.getTable('books');
        return table.insert({ title: 'test' }).then(function (results) {
            $assert.areEqual(results.id, 'alpha');
        }, function (error) {
            $assert.fail('Should have succeeded.');
        });
    }),

    $test('update')
    .description('Verify MobileTableService.update')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'PATCH');
            $assert.areEqual(req.url, 'http://www.test.com/tables/books/2?state=AL');
            $assert.areEqual(req.data, '{"id":2,"price":100}');
            callback(null, { status: 200, responseText: '{"title":"test"}' });
        });

        var table = client.getTable('books');
        var originalModelObject = { id: 2, price: 100 };
        return table.update(originalModelObject, { state: 'AL' }).then(function (results) {
            var isWinJs = typeof Windows === "object";
            if (isWinJs) {
                // For backward compatibility, the WinJS client mutates the original model
                // object by adding/overwriting properties from the server response
                $assert.isTrue(results === originalModelObject);
                $assert.areEqual(results.id, 2);         // We have the original properties
                $assert.areEqual(results.price, 100);
                $assert.areEqual(results.title, 'test'); // Combined with the server response properties
            } else {
                // The web client simply returns the server response without mutating your
                // original model, which is a lot more useful if the server response might
                // not be the same kind of object
                $assert.isFalse(results === originalModelObject);
                $assert.areEqual(results.title, 'test');     // We have the server response properties
                $assert.areEqual(results.id, undefined);     // But not the original model properties
                $assert.areEqual(results.price, undefined);
                $assert.areEqual(originalModelObject.id, 2); // And the original is unchanged
                $assert.areEqual(originalModelObject.price, 100);
                $assert.areEqual(originalModelObject.title, undefined);
            }
        });
    }),

    $test('table.update() success with valid ids')
    .description('Verify table.update works with all valid ids')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            testIdData = validStringIds.concat(validIntIds),
            testCases = [];

        testIdData.forEach(function (testId) {
            testCases.push(function () {
                var originalModelObject = { id: testId, price: 100 };

                client = client.withFilter(function (req, next, callback) {
                    $assert.areEqual(req.type, 'PATCH');
                    $assert.areEqual(req.url, 'http://www.test.com/tables/books/' + encodeURIComponent(testId));
                    $assert.areEqual(req.data, '{"id":' + JSON.stringify(testId) + ',"price":100}');
                    callback(null, { status: 200, responseText: '{"id":' + JSON.stringify(testId) + '}' });
                });

                return client.getTable('books').update(originalModelObject).then(function (result) {
                    $assert.areEqual(result.id, testId);
                });
            });
        });
        return $chain.apply(null, testCases);
    }),

    $test('table.update() throws with invalid ids')
    .description('Verify table.update fails with invalid ids')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            testIdData = invalidIntIds.concat(invalidStringIds),
            testCases = [];

        testIdData.forEach(function (testId) {
            testCases.push(function () {
                var originalModelObject = { id: testId, price: 100 };

                client = client.withFilter(function (req, next, callback) {
                    callback(null, { status: 200, responseText: req.data });
                });

                return client.getTable('books').update(originalModelObject).then(function (result) {
                    $assert.fail('Should have failed');
                }, function (error) {
                    $assert.contains(error.message, 'is not valid');
                });
            });
        });
        return $chain.apply(null, testCases);
    }),

    $test('table.update() can return any possible id')
    .description('Verify table.update can return all ids')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            testIdData = validStringIds.concat(validIntIds).concat(invalidIntIds).concat(invalidStringIds),
            testCases = [];
            
        testIdData.forEach(function (testId) {
            testCases.push(function () {
                var originalModelObject = { id: 1, price: 100 };

                client = client.withFilter(function (req, next, callback) {
                    callback(null, { status: 200, responseText: '{"id":' + JSON.stringify(testId) + '}' });
                });

               return client.getTable('books').update(originalModelObject).then(function (result) {
                    $assert.areEqual(result.id, testId);
                });
            });
        });
        return $chain.apply(null, testCases);
    }),

    $test('update throws no id')
    .description('Verify MobileTableService.update throws when no id')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            callback(null, { status: 200, responseText: '{"title":"test"}' });
        });

        var table = client.getTable('books');
        return table.update({ Value: 100 }).then(function (results) {
            $assert.fail('Should have failed.');
        }, function (error) {
            $assert.contains(error.message, 'id');
        });
    }),

    $test('update throws id of 0')
    .description('Verify MobileTableService.update throws on id of 0')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            callback(null, { status: 200, responseText: '{"title":"test"}' });
        });

        var table = client.getTable('books');
        return table.update({ id: 0 }).then(function (results) {
            $assert.fail('Should have failed.');
        }, function (error) {
            $assert.contains(error.message, 'id');
        });
    }),

    $test('update throws empty string id')
    .description('Verify MobileTableService.update throws empty string id')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            callback(null, { status: 200, responseText: '{"title":"test"}' });
        });

        var table = client.getTable('books');
        return table.update({ id: '' }).then(function (results) {
            $assert.fail('Should have failed.');
        }, function (error) {
            $assert.contains(error.message, 'id');
        });
    }),

    $test('table.lookup() with static string id and any id response content')
    .description('Verify table.lookup returns the results')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            testIdData = validStringIds.concat(emptyStringIds).concat(invalidStringIds).concat(validIntIds).concat(invalidIntIds).concat(nonStringNonIntValidJsonIds);
            testCases = [];
            
        testIdData.forEach(function (testId) {
            testCases.push(function () {
                client = client.withFilter(function (req, next, callback) {
                    $assert.areEqual(req.url, 'http://www.test.com/tables/books/id');
                    callback(null, { status: 200, responseText: '{"id":' + JSON.stringify(testId) + '}' });
                });

                return client.getTable('books').lookup('id').then(function (result) {
                    $assert.areEqual(result.id, testId);
                }, function (error) {
                    $assert.fail("Should have succeeded");
                });
            });
        });
        return $chain.apply(null, testCases);
    }),

    $test('table.lookup() with string id and null response type')
    .description('Verify table.lookup returns the results')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.url, 'http://www.test.com/tables/books/id');
            callback(null, { status: 200, responseText: '{"id": null }' });
        });

        var table = client.getTable('books');
        return table.lookup('id').then(function (result) {
            $assert.areEqual(result.id, null);
        }, function (error) {
            $assert.fail("Should have succeeded");
        });
    }),

    $test('table.lookup() with string id and no id response type')
    .description('Verify table.lookup returns the results')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.url, 'http://www.test.com/tables/books/id');
            callback(null, { status: 200, responseText: '{"string": "hey" }' });
        });

        var table = client.getTable('books');
        return table.lookup('id').then(function (result) {
            $assert.areEqual(result.string, 'hey');
        }, function (error) {
            $assert.fail("Should have succeeded");
        });
    }),

    $test('table.lookup() with valid ids')
    .description('Verify table.lookup returns the results')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            testIdData = validStringIds.concat(validIntIds),
            testCases = [];

        testIdData.forEach(function (testId) {
            testCases.push(function () {
                client = client.withFilter(function (req, next, callback) {
                    $assert.areEqual(req.url, 'http://www.test.com/tables/books/' + encodeURIComponent(testId));
                    callback(null, { status: 200, responseText: '{"id":' + JSON.stringify(testId) + '}' });
                });

                return client.getTable('books').lookup(testId).then(function (result) {
                    $assert.areEqual(result.id, testId);
                }, function (error) {
                    $assert.fail("Should have succeeded");
                });
            });
        });
        return $chain.apply(null, testCases);
    }),

    $test('table.lookup() throws with invalid id types')
    .description('Verify table.lookup throws when given an invalid id')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            testIdData = invalidStringIds.concat(emptyStringIds).concat(invalidIntIds);
        testCases = [];

        testIdData.forEach(function (testId) {
            testCases.push(function () {
                client = client.withFilter(function (req, next, callback) {
                    callback(null, { status: 200, responseText: '{"id":' + JSON.stringify(testId) + '}' });
                });

                return client.getTable('books').lookup(testId).then(function (result) {
                    $assert.fail('Should have failed');
                }, function (error) {
                    $assert.contains(error.message, "is not valid.");
                });
            });
        });
        return $chain.apply(null, testCases);
    }),

    $test('lookup with parameter')
    .description('Verify MobileTableService.lookup returns the result')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'GET');
            $assert.areEqual(req.url, 'http://www.test.com/tables/books/5?state=FL');
            callback(null, { status: 200, responseText: '{"title":"test"}' });
        });

        var table = client.getTable('books');
        return table.lookup(5, { state: 'FL' }).then(function (results) {
            $assert.areEqual(results.title, 'test');
        });
    }),

    $test('lookup throws no id')
    .description('Verify MobileTableService.lookup throws if no id')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'GET');
            $assert.areEqual(req.url, 'http://www.test.com/tables/books/5?state=FL');
            callback(null, { status: 200, responseText: '{"title":"test"}' });
        });

        var table = client.getTable('books');
        return table.lookup(null, { state: 'WY' }).then(function (results) {
            $assert.fail('Should have failed.');
        }, function (error) {
            $assert.contains(error.message, 'id');
        });
    }),

    $test('lookup throws id zero')
    .description('Verify MobileTableService.lookup throws when id is 0')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'GET');
            $assert.areEqual(req.url, 'http://www.test.com/tables/books/5?state=FL');
            callback(null, { status: 200, responseText: '{"title":"test"}' });
        });

        var table = client.getTable('books');
        return table.lookup(0, { state: 'WY' }).then(function (results) {
            $assert.fail('Should have failed.');
        }, function (error) {
            $assert.contains(error.message, 'id');
        });
    }),

    $test('table.del() sucess with valid ids')
    .description('Verify table.del works with all valid ids')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            testIdData = validStringIds.concat(validIntIds),
            testCases = [];

        testIdData.forEach(function (testId) {
            testCases.push(function (prevResults) {
                client = client.withFilter(function (req, next, callback) {
                    $assert.areEqual(req.type, 'DELETE');
                    $assert.areEqual(req.url, 'http://www.test.com/tables/books/' + encodeURIComponent(testId));
                    $assert.isNull(req.data);
                    callback(null, { status: 200, responseText: null });
                });

                return table = client.getTable('books').del({ id: testId });
            });
        });
        return $chain.apply(null, testCases);
    }),

    $test('table.del() throws with invalid ids')
    .description('Verify table.del fails with all invalid ids')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            testIdData = invalidStringIds.concat(invalidIntIds),
            testCases = [];

        testIdData.forEach(function (testId) {
            testCases.push(function (prevResults) {
                client = client.withFilter(function (req, next, callback) {
                    $assert.areEqual(req.type, 'DELETE');
                    $assert.areEqual(req.url, 'http://www.test.com/tables/books/' + encodeURIComponent(testId));
                    $assert.isNull(req.data);
                    callback(null, { status: 200, responseText: null });
                });

                return client.getTable('books').del({ id: testId }).then(function (result) {
                    $assert.fail('Should have failed');
                }, function (error) {
                    $assert.contains(error.message, 'is not valid.');
                });
            });
        });
        return $chain.apply(null, testCases);
    }),

    $test('del')
    .description('Verify MobileTableService.del')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'DELETE');
            $assert.areEqual(req.url, 'http://www.test.com/tables/books/2?state=WY');
            $assert.isNull(req.data);
            callback(null, { status: 200, responseText: null });
        });

        var table = client.getTable('books');
        return table.del({ id: 2 }, { state: 'WY' });
    }),

    $test('del throws no id')
    .description('Verify MobileTableService.del throws when no Id is passed')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            callback(null, { status: 200, responseText: '{"title":"test"}' });
        });

        var table = client.getTable('books');
        return table.del({ Value: 100 }).then(function (results) {
            $assert.fail('Should have failed.');
        }, function (error) {
            $assert.contains(error.message, 'id');
        });
    }),

    $test('del throws id of 0')
    .description('Verify MobileTableService.del throws on an id of 0')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            callback(null, { status: 200, responseText: '{"title":"test"}' });
        });

        var table = client.getTable('books');
        return table.del({ id: 0 }).then(function (results) {
            $assert.fail('Should have failed.');
        }, function (error) {
            $assert.contains(error.message, 'id');
        });
    }),

    $test('Test receiving invalid json')
    .description('Verify error is handled correctly when we receive invalid json')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.microsoft.com", "123456abcdefg");
        var table = client.getTable('books');
        return table.insert({ 'State': 'CA' }).then(function (results) {
            $assert.fail("Should have failed");
        },
        function (error) {
            $assert.isNotNull(error.message);
        });
    }),

    $test('table.refresh() with valid ids')
    .description('Verify table.refresh works with all valid ids')
    .check(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            testIdData = validStringIds.concat(validIntIds),
            testCases = [];

        testIdData.forEach(function (testId) {
            testCases.push(function (prevResults) {
                var originalModelObject = { id: testId, price: 100, custom: 5 };

                client = client.withFilter(function (req, next, callback) {
                    $assert.areEqual(req.type, 'GET');
                    if (typeof testId === 'string') {
                        var uriId = testId.replace(/\'/g, '%27');
                        $assert.areEqual(req.url, "http://www.test.com/tables/books?$filter=id eq '" + encodeURIComponent(uriId) + "'");
                    } else {
                        $assert.areEqual(req.url, "http://www.test.com/tables/books?$filter=id eq " + encodeURIComponent(testId));
                    }
                    callback(null, { status: 200, responseText: '{"id":' + JSON.stringify(testId) + ', "title":"test", "price": 200}' });
                });

                return client.getTable('books').refresh(originalModelObject).then(function (results) {
                    var isWinJs = typeof Windows === "object";
                    if (isWinJs) {
                        // For backward compatibility, the WinJS client mutates the original model
                        // object by adding/overwriting properties from the server response
                        $assert.isTrue(results === originalModelObject);
                        $assert.areEqual(results.id, testId);    // We have the original properties
                        $assert.areEqual(results.custom, 5);
                        $assert.areEqual(results.price, 200);    // updated with the return from the server
                        $assert.areEqual(results.title, 'test');
                    } else {
                        // The web client simply returns the server response without mutating your
                        // original model, which is a lot more useful if the server response might
                        // not be the same kind of object
                        $assert.isFalse(results === originalModelObject);
                        $assert.areEqual(results.title, 'test');     // We have the server response properties
                        $assert.areEqual(results.id, testId);
                        $assert.areEqual(results.custom, null);        // but no original properties carried over
                        $assert.areEqual(results.price, 200);
                        $assert.areEqual(originalModelObject.id, testId); // And the original is unchanged
                        $assert.areEqual(originalModelObject.price, 100);
                        $assert.areEqual(originalModelObject.title, undefined);
                    }
                });
            });
        });
        return $chain.apply(null, testCases);
    }),

    $test('table.refresh() with invalid ids')
    .description('Verify table.refresh does not hit server with nonstring invalid ids')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            testIdData = invalidIntIds.concat(nonStringNonIntIds).
            testCases = [];

        client = client.withFilter(function (req, next, callback) {
            $assert.fail('Should not have hit server');
        });

        testIdData.forEach(function (testId) {
            testCases.push(function () {
                var originalModelObject = { id: testId, price: 100, custom: 5 };

                return client.getTable('books').refresh(originalModelObject).then(function (results) {
                    $assert.areEqual(results.id, testId);
                    $assert.areEqual(results.price, 100);
                    $assert.areEqual(results.custom, 5);
                    $assert.areEqual(results.title, null);
                }, function (error) {
                    $assert.fail('Should have succeeded');
                });
            });
        });
        return $chain.apply(null, testCases);
    }),

    $test('table.refresh() with invalid string ids')
    .description('Verify table.refresh fails with nonstring invalid ids')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg"),
            testIdData = invalidStringIds,
            testCases = [];

        client = client.withFilter(function (req, next, callback) {
            $assert.fail('Should not have hit server');
        });

        testIdData.forEach(function (testId) {
            testCases.push(function () {
                var originalModelObject = { id: testId, price: 100, custom: 5 };

                return client.getTable('books').refresh(originalModelObject).then(function (results) {
                    $assert.fail('Should have failed');
                }, function (error) {
                    $assert.contains(error.message, 'is not valid');
                });
            });
        });
        return $chain.apply(null, testCases);
    }),

    $test('Refresh')
    .description('Verify MobileTableService.refresh')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'GET');
            $assert.areEqual(req.url, 'http://www.test.com/tables/books?$filter=id eq 2');
            callback(null, { status: 200, responseText: '{"title":"test", "price": 200}' });
        });

        var table = client.getTable('books');
        var originalModelObject = { id: 2, price: 100 };

        return table.refresh(originalModelObject).then(function (results) {
            var isWinJs = typeof Windows === "object";
            if (isWinJs) {
                // For backward compatibility, the WinJS client mutates the original model
                // object by adding/overwriting properties from the server response
                $assert.isTrue(results === originalModelObject);
                $assert.areEqual(results.id, 2);         // We have the original properties
                $assert.areEqual(results.price, 200);    // updated with the return from the server
                $assert.areEqual(results.title, 'test');
            } else {
                // The web client simply returns the server response without mutating your
                // original model, which is a lot more useful if the server response might
                // not be the same kind of object
                $assert.isFalse(results === originalModelObject);
                $assert.areEqual(results.title, 'test');     // We have the server response properties
                $assert.areEqual(results.id, undefined);     // but no original properties carried over
                $assert.areEqual(results.price, 200);
                $assert.areEqual(originalModelObject.id, 2); // And the original is unchanged
                $assert.areEqual(originalModelObject.price, 100);
                $assert.areEqual(originalModelObject.title, undefined);
            }
        });
    }),
    
    $test('Refresh with params')
    .description('Verify MobileTableService.refresh')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'GET');
            $assert.areEqual(req.url, 'http://www.test.com/tables/books?$filter=id eq 2&name=bob');
            callback(null, { status: 200, responseText: '{"title":"test", "price": 200}' });
        });

        var table = client.getTable('books');
        var originalModelObject = { id: 2, price: 100 };

        return table.refresh(originalModelObject, {"name":"bob"}).then(function (results) {
            var isWinJs = typeof Windows === "object";
            if (isWinJs) {
                // For backward compatibility, the WinJS client mutates the original model
                // object by adding/overwriting properties from the server response
                $assert.isTrue(results === originalModelObject);
                $assert.areEqual(results.id, 2);         // We have the original properties
                $assert.areEqual(results.price, 200);    // updated with the return from the server
                $assert.areEqual(results.title, 'test');
            } else {
                // The web client simply returns the server response without mutating your
                // original model, which is a lot more useful if the server response might
                // not be the same kind of object
                $assert.isFalse(results === originalModelObject);
                $assert.areEqual(results.title, 'test');     // We have the server response properties
                $assert.areEqual(results.id, undefined);     // but no original properties carried over
                $assert.areEqual(results.price, 200);
                $assert.areEqual(originalModelObject.id, 2); // And the original is unchanged
                $assert.areEqual(originalModelObject.price, 100);
                $assert.areEqual(originalModelObject.title, undefined);
            }
        });
    }),

    $test('Refresh - multiple results')
    .description('Verify MobileTableService.refresh')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'GET');
            $assert.areEqual(req.url, 'http://www.test.com/tables/books?$filter=id eq 2');
            callback(null, { status: 200, responseText: '[{"title":"test", "price": 200}, {"title":"test2", "price": 300}]' });
        });

        var table = client.getTable('books');
        var originalModelObject = { id: 2, price: 100 };

        return table.refresh(originalModelObject).then(function (results) {
            var isWinJs = typeof Windows === "object";
            if (isWinJs) {
                // For backward compatibility, the WinJS client mutates the original model
                // object by adding/overwriting properties from the server response
                $assert.isTrue(results === originalModelObject);
                $assert.areEqual(results.id, 2);         // We have the original properties
                $assert.areEqual(results.price, 200);    // updated with the return from the server
                $assert.areEqual(results.title, 'test');
            } else {
                // The web client simply returns the server response without mutating your
                // original model, which is a lot more useful if the server response might
                // not be the same kind of object
                $assert.isFalse(results === originalModelObject);
                $assert.areEqual(results.title, 'test');     // We have the server response properties
                $assert.areEqual(results.id, undefined);     // but no original properties carried over
                $assert.areEqual(results.price, 200);
                $assert.areEqual(originalModelObject.id, 2); // And the original is unchanged
                $assert.areEqual(originalModelObject.price, 100);
                $assert.areEqual(originalModelObject.title, undefined);
            }
        });
    }),

    $test('Refresh - no results')
    .description('Verify MobileTableService.refresh')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'GET');
            $assert.areEqual(req.url, 'http://www.test.com/tables/books?$filter=id eq 2');
            callback(null, { status: 200, responseText: '[]' });
        });

        var table = client.getTable('books');
        var originalModelObject = { id: 2, price: 100 };

        return table.refresh(originalModelObject).then(function (results) {
            $assert.fail("Should have failed");
        }, function(error) {
            $assert.isNotNull(error.message);
        });
    }),

    $test('Refresh - no id just returns object')
    .description('Verify MobileTableService.refresh succeeds with no id')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.fail("Should have not made a server call");
        });

        var table = client.getTable('books');
        var originalModelObject = { price: 100 };

        return table.refresh(originalModelObject).then(function (results) {
            $assert.areEqual(results.price, 100);
        }, function (error) {
            $assert.fail("Should have succeeded");
        });
    }),

    $test('Refresh - id of 0 just returns object')
    .description('Verify MobileTableService.refresh succeeds with id of 0')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.fail("Should have not made a server call");
        });

        var table = client.getTable('books');
        var originalModelObject = { id: 0.0, price: 100 };

        return table.refresh(originalModelObject).then(function (results) {
            $assert.areEqual(results.price, 100);
        }, function (error) {
            $assert.fail("Should have succeeded");
        });
    }),

    $test('Refresh - empty string id just returns object')
    .description('Verify MobileTableService.refresh succeeds with id of 0')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.fail("Should have not made a server call");
        });

        var table = client.getTable('books');
        var originalModelObject = { id: '', price: 100 };

        return table.refresh(originalModelObject).then(function (results) {
            $assert.areEqual(results.price, 100);
        }, function (error) {
            $assert.fail("Should have succeeded");
        });
    })

);
