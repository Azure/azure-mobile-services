// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="..\..\js\MobileServices.Internals.js" />
/// <reference path="..\..\generated\Tests.js" />

// Declare JSHint globals
/*global MobileServiceClient:false */

$testGroup('MobileServiceTables.js',
    $test('get')
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

    $test('query via table.read() with no response content')
    .description('Verify MobileServiceTable table operations allow responses without content')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.url, 'http://www.test.com/tables/books');
            callback(null, { status: 200, responseText: "" });
        });

        var table = client.getTable('books');
        return table.read().then(function (results) {
            $assert.areEqual(results, null);
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
    .description('Verify MobileTableService.insert throws on an existing id')
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

     $test('lookup')
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

    $test('Refresh - no id')
    .description('Verify MobileTableService.refresh succeeds with no id')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'GET');
            $assert.areEqual(req.url, 'http://www.test.com/tables/books?$filter=id eq 2');
            callback(null, { status: 200, responseText: '[]' });
        });

        var table = client.getTable('books');
        var originalModelObject = { price: 100 };

        return table.refresh(originalModelObject).then(function (results) {
            $assert.areEqual(results.price, 100);
        }, function (error) {
            $assert.fail("Should have succeeded");
        });
    }),

    $test('Refresh - id of 0')
    .description('Verify MobileTableService.refresh succeeds with id of 0')
    .checkAsync(function () {
        var client = new WindowsAzure.MobileServiceClient("http://www.test.com", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'GET');
            $assert.areEqual(req.url, 'http://www.test.com/tables/books?$filter=id eq 2');
            callback(null, { status: 200, responseText: '[]' });
        });

        var table = client.getTable('books');
        var originalModelObject = { id: 0.0, price: 100 };

        return table.refresh(originalModelObject).then(function (results) {
            $assert.areEqual(results.price, 100);
        }, function (error) {
            $assert.fail("Should have succeeded");
        });
    })

);
