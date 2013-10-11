// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="..\..\js\MobileServices.Internals.js" />
/// <reference path="..\..\generated\Tests.js" />

$testGroup('ToDo')
    .functional()
    .tests(
        $test('ToDoBasics')
        .description('Run through some basic TODO scenarios')
        .checkAsync(function () {
            var table = $getClient().getTable('test_table');
            var context = { };
            return $chain(
                function () {
                    $log('Insert a few records');
                    return table.insert({ col1: "ABC", col2: 0 });
                },
                function (first) {
                    $log('Check we can lookup inserted record');
                    return table.lookup(first.id);
                },
                function (first) {
                    $assert.areEqual("ABC", first.col1);
                    context.first = first;
                    context.newItems = 'id ge ' + first.id;
                    return table.insert({ col1: "DEF", col2: 1 }, { testMode: true });
                },
                function () {
                    $log('Verify the exception message for looking up a instance that does not exist.');
                    return table.lookup(9999).then(
                    function () {
                        $assert.fail('Should have errored');
                    },
                    function (err) {
                        $assert.contains(err.message, 'An item with id \'9999\' does not exist.');
                    });
                },
                function () {
                    return table.insert({ col1: "GHI", col2: 0 });
                },
                function () {
                    return table.where(context.newItems).read({ testMode: true });
                },
                function (results) {
                    $assert.areEqual(3, results.length);

                    $log('Query and sort ascending');
                    return table.where(context.newItems).orderBy('col1').read();
                },
                function (items) {
                    $assert.areEqual(3, items.length);
                    $assert.areEqual('ABC', items[0].col1);
                    $assert.areEqual('DEF', items[1].col1);
                    $assert.areEqual('GHI', items[2].col1);

                    $log('Query and sort descending');
                    return table.where(context.newItems).orderByDescending('col1').read();
                },
                function (items) {
                    $assert.areEqual(3, items.length);
                    $assert.areEqual('ABC', items[2].col1);
                    $assert.areEqual('DEF', items[1].col1);
                    $assert.areEqual('GHI', items[0].col1);

                    $log('Query for completed');
                    return table.where(context.newItems).where('col2 gt 0').read();
                },
                function (items) {
                    $assert.areEqual(1, items.length);
                    $assert.areEqual('DEF', items[0].col1);

                    $log('Verify the exception message for inserting a new element that already has an ID');
                    return table.insert({ id: 30, col1: 'Will fail' }).then(
                        function () {
                            $assert.fail('Should have errored');
                        },
                        function (err) {
                            $assert.contains(err.message, 'id');
                        });
                },
                function () {
                    $log('Verify that usinga non-existant collection also throws an error');
                    return $getClient().getTable('notreal').read().then(
                        function () {
                            $assert.fail('Should have errored');
                        },
                        function (err) {
                            $assert.contains(err.message, 'notreal');
                            $assert.contains(err.request.responseText.toString(), 'notreal');
                        });
                });
        }),

        $test('Usage')
        .description('Simple TODO usage scenario')
        .checkAsync(function () {
            var table = $getClient().getTable('test_table');
            var context = {};
            return $chain(
                function () {
                    $log('Insert a few records');
                    return table.insert({ col1 : "Get Milk", col2 : 0 });
                },
                function (first) {
                    context.first = first;
                    context.newItems = 'id ge ' + first.id;
                    return table.insert({ col1: "Pick up dry cleaning", col2: 0 });
                },
                function () {
                    $log('Run a simple query and verify we get both items');
                    return table.where(context.newItems).read();
                },
                function (items) {
                    $assert.areEqual(2, items.length);
                    return table.insert({ col1 : "Submit TPS report", col2 : 0 });
                },
                function () {
                    $log('Check off the first item');
                    context.first.col2 = 1;
                    return table.update(context.first);
                },
                function () {
                    $log('Query for incomplete items');
                    return table.where(context.newItems).where('col2 eq 0').read();
                },
                function (remaining) {
                    $assert.areEqual(2, remaining.length);

                    $log('Delete the first item');
                    return table.del(context.first);
                },
                function () {
                    return table.where(context.newItems).read();
                },
                function (items) {
                    $assert.areEqual(2, items.length);
                });
        }),

        $test('TotalCountBasics')
        .description('Verify the standard includeTotalCount behavior')
        .tag('TotalCount')
        .checkAsync(function () {
            var table = $getClient().getTable('test_table');
            var context = { };
            return $chain(
                function () {
                    return table.insert({ col1: "ABC", col2: 0 });
                },
                function (first) {
                    context.first = first;
                    context.newItems = 'id ge ' + first.id;
                    return table.insert({ col1: "DEF", col2: 1 });
                },
                function () {
                    return table.insert({ col1: "GHI", col2: 0 });
                },
                function () {
                    return table.where(context.newItems).read();
                },
                function (results) {
                    $assert.areEqual(3, results.length);
                    $assert.isTrue(typeof results.totalCount === "undefined");

                    $log('Use inline count manually');
                    return table.where(context.newItems).includeTotalCount().read();
                },
                function (items) {
                    // Make sure the item has the totalCount
                    $assert.areEqual(3, items.length);
                    $assert.areEqual(3, items.totalCount);
                });
        }),


        $test('TotalCountWithTooManyElements')
        .description('Verify the includeTotalCount when the count is more than the returned items')
        .tag('TotalCount')
        .checkAsync(function () {
            var table = $getClient().getTable('test_table');
            var context = {};
            var actions = [];
            actions.push(function () {
                return table.insert({ col1: "Test1", col2: 0 });
            });
            actions.push(function (first) {
                context.first = first;
                context.newItems = 'id ge ' + first.id;
                return table.insert({ col1: "Test2", col2: 1 });
            });
            var insertRow = function(row) {
                actions.push(function () {
                    return table.insert({ col1: "Test" + row, col2: 0 });
                });
            };
            var i = 3;
            var totalCount = 65;
            for (; i <= totalCount; i++) {
                insertRow(i);
            }
            actions.push(function() {
                $log('Use inline count manually');                
                return table.includeTotalCount().where(context.newItems).read();
            });
            actions.push(function(items) {
                // Make sure the item has the correct totalCount
                $assert.areEqual(50, items.length);
                $assert.areEqual(totalCount, items.totalCount);
            });

            return $chain.apply(null, actions);
        })
    );
