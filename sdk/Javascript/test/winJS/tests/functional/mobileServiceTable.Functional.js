// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.2.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.2.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.2.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.2.0\js\ui.js" />
/// <reference path="..\..\js\MobileServices.Internals.js" />
/// <reference path="..\..\generated\Tests.js" />

// Declare JSHint globals
/*global MobileServiceClient:false, Platform:false */

var testData = require("constants");

function emptyTable(table) {
    return table.read().then(function (results) {
        var deletes = [];
        results.forEach(function (result) {
            deletes.push(function () {
                return table.del(result);
            });
        });
        return $chain.apply(this, deletes);
    });
}

$testGroup('Mobile Service Table Tests')
    .functional()
    .tests(
        $test('AsyncTableOperationsWithValidStringIdAgainstStringIdTable')
        .tag('stringId')
        .description('Verify the overall flow of a string Id table')
        .checkAsync(function () {
            var client = $getClient();
            var table = client.getTable('StringIdJavascriptTable');

            return $chain(
                function () {
                    return emptyTable(table);
                },
                function() {
                    var singleIdTests = [];
                    testData.validStringIds.forEach(function (testId) {
                        singleIdTests.push(function () {
                            return $chain(
                                function () {
                                    $log('testing id: ' + testId);
                                    return table.insert({ id: testId, string: 'Hey' });
                                },
                                function (item) {
                                    $assert.areEqual(testId, item.id);

                                    return table.read();
                                },
                                function (items) {
                                    $assert.areEqual(1, items.length);
                                    $assert.areEqual(testId, items[0].id);
                                    $assert.areEqual('Hey', items[0].string);

                                    return table.where({ id: testId }).read();
                                },
                                function (items) {
                                    $assert.areEqual(1, items.length);
                                    $assert.areEqual(testId, items[0].id);
                                    $assert.areEqual('Hey', items[0].string);

                                    return table.select('id', 'string').read();
                                }, function (items) {
                                    $assert.areEqual(1, items.length);
                                    $assert.areEqual(testId, items[0].id);
                                    $assert.areEqual('Hey', items[0].string);

                                    return table.lookup(testId);
                                }, function (item) {
                                    $assert.areEqual(testId, item.id);
                                    $assert.areEqual('Hey', item.string);

                                    item.string = "What?";
                                    return table.update(item);
                                }, function (item) {
                                    $assert.areEqual(testId, item.id);
                                    $assert.areEqual('What?', item.string);

                                    item = { id: testId, string: 'hey' };

                                    return table.refresh(item);
                                }, function (item) {
                                    $assert.areEqual(testId, item.id);
                                    $assert.areEqual('What?', item.string);

                                    return table.read();
                                }, function (items) {
                                    $assert.areEqual(1, items.length);
                                    $assert.areEqual(testId, items[0].id);
                                    $assert.areEqual('What?', items[0].string);

                                    return table.del(items[0]);
                                });
                        });
                    });
                    return $chain.apply(null, singleIdTests);
                });
        }),

        $test('OrderingReadAsyncWithValidStringIdAgainstStringIdTable')
        .checkAsync(function () {
            var client = $getClient();
            var table = client.getTable('StringIdJavascriptTable');

            return $chain(
                function () {
                    $log('Clean up our data');
                    return emptyTable(table);
                },
                function () {
                    $log('Initialize our data');
                    var inserts = [];
                    ["a", "b", "C", "_A", "_B", "_C", "1", "2", "3"].forEach(function (testId) {
                        inserts.push(function (result) {
                            return table.insert({ id: testId, string: 'Hey' });
                        });
                    });
                    return $chain.apply(null, inserts);
                },
                function (result) {
                    $log('sort the data');
                    return table.orderBy('id').read();
                },
                function (items) {
                    $assert.areEqual(9, items.length);
                    $assert.areEqual("_A", items[0].id);
                    $assert.areEqual("_B", items[1].id);
                    $assert.areEqual("_C", items[2].id);
                    $assert.areEqual("1", items[3].id);
                    $assert.areEqual("2", items[4].id);
                    $assert.areEqual("3", items[5].id);
                    $assert.areEqual("a", items[6].id);
                    $assert.areEqual("b", items[7].id);
                    $assert.areEqual("C", items[8].id);

                    $log('sort the data descending');
                    return table.orderByDescending('id').read();
                },
                function (items) {
                    $assert.areEqual(9, items.length);
                    $assert.areEqual("_A", items[8].id);
                    $assert.areEqual("_B", items[7].id);
                    $assert.areEqual("_C", items[6].id);
                    $assert.areEqual("1", items[5].id);
                    $assert.areEqual("2", items[4].id);
                    $assert.areEqual("3", items[3].id);
                    $assert.areEqual("a", items[2].id);
                    $assert.areEqual("b", items[1].id);
                    $assert.areEqual("C", items[0].id);

                    return emptyTable(table);
                });
        }),

        $test('FilterReadAsyncWithEmptyStringIdAgainstStringIdTable')
        .checkAsync(function () {
            var client = $getClient();
            var table = client.getTable('StringIdJavascriptTable');
            
            return $chain(
                function () {
                    return emptyTable(table);
                },
                function () {
                    $log('Initialize our data');
                    var inserts = [];
                    testData.validStringIds.forEach(function (testId) {
                        inserts.push(function (result) {
                            return table.insert({ id: testId, string: 'Hey' });
                        });
                    });
                    return $chain.apply(null, inserts);
                },
                function () {
                    $log('verify no results for ids we didn\'t use');
                    var testIds = testData.emptyStringIds.concat(testData.invalidStringIds).concat(null),
                        filters = [];
                    testIds.forEach(function (testId) {
                        filters.push(function () {
                            return table.where({ id: testId }).read().then(function (items) {
                                $assert.areEqual(items.length, 0);
                            });
                        });
                    });
                    return $chain.apply(null, filters);
                },
                function () {
                    return emptyTable(table);
                });
        }),

        $test('LookupAsyncWithNosuchItemAgainstStringIdTable')
        .checkAsync(function () {
            var client = $getClient();
            var table = client.getTable('StringIdJavascriptTable');

            return $chain(
                function () {
                    return emptyTable(table);
                },
                function () {
                    $log('Initialize our data');
                    var inserts = [];
                    testData.validStringIds.forEach(function (testId) {
                        inserts.push(function (result) {
                            return table.insert({ id: testId, string: 'Hey' });
                        });
                    });
                    return $chain.apply(null, inserts);
                },
                function () {
                    $log('Delete our data');
                    var deletes = [];
                    testData.validStringIds.forEach(function (testId) {
                        deletes.push(function () {
                            return table.del({ id: testId }).then(function (result) {
                                // Do nothing
                            }, function (error) {
                                $assert.fail('Should have suceeded');
                            });
                        });
                    });
                    return $chain.apply(null, deletes);
                },
                function () {
                    var lookups = [];
                    testData.validStringIds.forEach(function (testId) {
                        lookups.push(function () {
                            return table.lookup(testId).then(function (item) {
                                $assert.fail('should have failed');
                            }, function (error) {
                                $assert.contains(error.message, $isDotNet() ? 'Not Found' : 'does not exist');
                            });
                        });
                    });
                    return $chain.apply(null, lookups);
                });
        }),

        $test('RefreshAsyncWithNoSuchItemAgainstStringIdTable')
        .checkAsync(function () {
            var client = $getClient();
            var table = client.getTable('StringIdJavascriptTable');
            
            return $chain(
                function () {
                    return emptyTable(table);
                },
                function () {
                    $log('Initialize our data');
                    var inserts = [];
                    testData.validStringIds.forEach(function (testId) {
                        inserts.push(function (result) {
                            return table.insert({ id: testId, string: 'Hey' });
                        });
                    });
                    return $chain.apply(null, inserts);
                },
                function () {
                    var lookups = [];
                    testData.validStringIds.forEach(function (testId) {
                        lookups.push(function () {
                            return table.lookup(testId).then(function (item) {
                                $assert.areEqual(item.id, testId);
                            });
                        });
                    });
                    return $chain.apply(null, lookups);
                },
                function () {
                    $log('Delete our data');
                    var deletes = [];
                    testData.validStringIds.forEach(function (testId) {
                        deletes.push(function () {
                            return table.del({ id: testId }).then(function (result) {
                                // Do Nothing
                            }, function (error) {
                                $assert.fail('Should have suceeded');
                            });
                        });
                    });
                    return $chain.apply(null, deletes);
                },
                function () {
                    $log('Refresh our data');
                    var refreshes = [];
                    testData.validStringIds.forEach(function (testId) {
                        refreshes.push(function () {
                            return table.refresh({id: testId, string: 'Hey'}).then(function (result) {
                                $assert.fail('Should have failed');
                            }, function (error) {
                                $assert.contains(error.message, 'Could not get object from response');
                            });
                        });
                    });
                    return $chain.apply(null, refreshes);
                });
        }),

        $test('InsertAsyncWithExistingItemAgainstStringIdTable')
        .checkAsync(function () {
            var client = $getClient(),
                table = client.getTable('StringIdJavascriptTable');
            
            return $chain(
                function () {
                    return emptyTable(table);
                },
                function () {
                    $log('Initialize our data');
                    var inserts = [];
                    testData.validStringIds.forEach(function (testId) {
                        inserts.push(function (result) {
                            return table.insert({ id: testId, string: 'Hey' });
                        });
                    });
                    return $chain.apply(null, inserts);
                },
                function () {
                    $log('Lookup our data');
                    var lookups = [];
                    testData.validStringIds.forEach(function (testId) {
                        lookups.push(function () {
                            return table.lookup(testId).then(function (item) {
                                $assert.areEqual(item.id, testId);
                            });
                        });
                    });
                    return $chain.apply(null, lookups);
                },
                function () {
                    $log('Insert duplicates into our data');
                    var inserts = [];
                    testData.validStringIds.forEach(function (testId) {
                        inserts.push(function (result) {
                            return table.insert({ id: testId, string: 'I should really not do this' }).then(function (item) {
                                $assert.fail('Should have failed');
                            }, function (error) {

                            });
                        });
                    });
                    return $chain.apply(null, inserts);
                },
                function () {
                    return emptyTable(table);
                });
        }),

        $test('UpdateAsyncWithNosuchItemAgainstStringIdTable')
        .checkAsync(function () {
            var client = $getClient();
            var table = client.getTable('StringIdJavascriptTable');

            return $chain(
                function () {
                    return emptyTable(table);
                },
                function () {
                    $log('Initialize our data');
                    var inserts = [];
                    testData.validStringIds.forEach(function (testId) {
                        inserts.push(function (result) {
                            return table.insert({ id: testId, string: 'Hey' });
                        });
                    });
                    return $chain.apply(null, inserts);
                },
                function () {
                    $log('Delete our data');
                    var deletes = [];
                    testData.validStringIds.forEach(function (testId) {
                        deletes.push(function () {
                            return table.del({ id: testId }).then(function (result) {
                                // Do Nothing
                            }, function (error) {
                                $assert.fail('Should have suceeded');
                            });
                        });
                    });
                    return $chain.apply(null, deletes);
                },
                function () {
                    $log('Update records the don\'t exist');
                    var updates = [];
                    testData.validStringIds.forEach(function (testId) {
                        updates.push(function () {
                            return table.update({ id: testId, string: 'Alright!' }).then(function (item) {
                                $assert.fail('Should have failed.');
                            }, function (error) {
                                $assert.contains(error.message, $isDotNet() ? 'Not Found' : 'does not exist');
                            });
                        });
                    });
                    return $chain.apply(null, updates);
                });
        }),

        $test('DeleteAsyncWithNosuchItemAgainstStringIdTable')
        .checkAsync(function () {
            var client = $getClient();
            var table = client.getTable('StringIdJavascriptTable');

            return $chain(
                function () {
                    return emptyTable(table);
                },
                function () {
                    $log('Initialize our data');
                    var inserts = [];
                    testData.validStringIds.forEach(function (testId) {
                        inserts.push(function (result) {
                            return table.insert({ id: testId, string: 'Hey' });
                        });
                    });
                    return $chain.apply(null, inserts);
                },
                function () {
                    $log('Verify our data');
                    var lookups = [];
                    testData.validStringIds.forEach(function (testId) {
                        lookups.push(function () {
                            return table.lookup(testId).then(function (item) {
                                $assert.areEqual(item.id, testId);
                            });
                        });
                    });
                    return $chain.apply(null, lookups);
                },
                function () {
                    $log('Delete our data');
                    var deletes = [];
                    testData.validStringIds.forEach(function (testId) {
                        deletes.push(function () {
                            return table.del({ id: testId }).then(function (result) {
                                // Do Nothing
                            }, function (error) {
                                $assert.fail('Should have suceeded');
                            });
                        });
                    });
                    return $chain.apply(null, deletes);
                },
                function () {
                    $log('Delete data that doesn\'t exist');
                    var deletes = [];
                    testData.validStringIds.forEach(function (testId) {
                        deletes.push(function () {
                            return table.del({ id: testId }).then(function (result) {
                                $assert.fail('Should have failed');
                            }, function (error) {
                                $assert.areEqual(error.request.status, 404);
                            });
                        });
                    });
                    return $chain.apply(null, deletes);
                });
        }),

        $test('AsyncTableOperationsWithIntegerAsStringIdAgainstIntIdTable')
        .tag('dotNet_not_supported')  //.NET apps don't support integer ids.
        .checkAsync(function () {
            var client = $getClient(),
                table = client.getTable('IntegerIdJavascriptTable'),
                testId;

            return $chain(
                function () {
                    return emptyTable(table);
                },
                function () {
                    $log('Insert record');
                    return table.insert({ string: 'Hey' }).then(function (item) {
                        testId = item.id;
                    });
                },
                function (item) {
                    $log('read table');
                    return table.read();
                },
                function (items) {
                    $assert.areEqual(1, items.length);
                    $assert.areEqual(testId, items[0].id);
                    $assert.areEqual("Hey", items[0].string);

                    $log('perform select');
                    return table.select(function () { this.xid = this.id; this.xstring = this.string; return this; }).read();
                },
                function (items) {
                    $assert.areEqual(1, items.length);
                    $assert.areEqual(testId, items[0].xid);
                    $assert.areEqual('Hey', items[0].xstring);

                    $log('perform lookup');
                    return table.lookup(items[0].xid);
                },
                function (item) {
                    $assert.areEqual(testId, item.id);
                    $assert.areEqual('Hey', item.string);

                    $log('perform update');
                    item.string = 'What?';
                    return table.update(item);
                },
                function (item) {
                    $assert.areEqual(testId, item.id);
                    $assert.areEqual('What?', item.string);

                    $log('perform refresh');
                    return table.refresh({ id: item.id, string: 'Hey' });
                },
                function (item) {
                    $assert.areEqual(testId, item.id);
                    $assert.areEqual('What?', item.string);

                    $log('perform read again');
                    return table.read();
                },
                function (items) {
                    $assert.areEqual(1, items.length);
                    $assert.areEqual(testId, items[0].id);
                    $assert.areEqual('What?', items[0].string);
                });
        }),

        $test('ReadAsyncWithValidIntIdAgainstIntIdTable')
        .tag('dotNet_not_supported') //.NET apps don't support integer ids.
        .checkAsync(function () {
            var client = $getClient(),
                table = client.getTable('IntegerIdJavascriptTable'), 
                testIds = [];

            return $chain(
                function () {
                    return emptyTable(table);
                },
                function () {
                    return table.insert({ string: 'Hey' });
                },
                function (item) {
                    return table.read();
                },
                function (items) {
                    $assert.areEqual(1, items.length);
                    $assert.isTrue(items[0].id > 0);
                    $assert.areEqual('Hey', items[0].string);
                });
        }),

        // System Properties Tests

        $test('AsyncTableOperationsWithAllSystemProperties')
        .tag('SystemProperties')
        .checkAsync(function () {

            var client = $getClient(),
                table = client.getTable('StringIdJavascriptTable'),
                savedItem,
                savedVersion,
                savedUpdatedAt;

            return $chain(
                function () {
                    return emptyTable(table);
                },
                function () {
                    return table.insert({ id: 'an id', string: 'a value' });
                },
                function (item) {
                    $assert.isNotNull(item.createdAt);
                    $assert.isNotNull(item.updatedAt);
                    $assert.isNotNull(item.version);

                    return table.read();
                },
                function (items) {
                    $assert.areEqual(1, items.length);
                    var item = items[0];
                    $assert.isNotNull(item.createdAt);
                    $assert.isNotNull(item.updatedAt);
                    $assert.isNotNull(item.version);
                    savedItem = item;

                    return table.where(function (value) { return this.version == value; }, item.version).read();
                },
                function (items) {
                    $assert.areEqual(1, items.length);
                    var item = items[0];
                    $assert.isNotNull(item.createdAt);
                    $assert.isNotNull(item.updatedAt);
                    $assert.isNotNull(item.version);

                    return table.where(function (value) { return this.createdAt == value; }, savedItem.createdAt).read();
                },
                function (items) {
                    $assert.areEqual(1, items.length);
                    var item = items[0];
                    $assert.isNotNull(item.createdAt);
                    $assert.isNotNull(item.updatedAt);
                    $assert.isNotNull(item.version);

                    return table.where(function (value) { return this.updatedAt == value; }, savedItem.updatedAt).read();
                },
                function (items) {
                    $assert.areEqual(1, items.length);
                    var item = items[0];
                    $assert.isNotNull(item.createdAt);
                    $assert.isNotNull(item.updatedAt);
                    $assert.isNotNull(item.version);

                    return table.lookup(savedItem.id);
                },
                function (item) {
                    $assert.areEqual(item.id, savedItem.id);
                    $assert.areEqual(item.updatedAt.valueOf(), savedItem.updatedAt.valueOf());
                    $assert.areEqual(item.createdAt.valueOf(), savedItem.createdAt.valueOf());
                    $assert.areEqual(item.version, savedItem.version);

                    savedItem.string = 'Hello';
                    savedVersion = savedItem.version; //WinJS Mutates
                    savedUpdatedAt = savedItem.updatedAt.valueOf();

                    return table.update(savedItem);
                },
                function (item) {
                    $assert.areEqual(item.id, savedItem.id);
                    $assert.areEqual(item.createdAt.valueOf(), savedItem.createdAt.valueOf());
                    $assert.areNotEqual(item.version, savedVersion);
                    $assert.areNotEqual(item.updatedAt.valueOf(), savedUpdatedAt);
                    savedItem = item;

                    return table.read();
                },
                function (items) {
                    var item = items[0];
                    $assert.areEqual(item.id, savedItem.id);
                    $assert.areEqual(item.updatedAt.valueOf(), savedItem.updatedAt.valueOf());
                    $assert.areEqual(item.createdAt.valueOf(), savedItem.createdAt.valueOf());
                    $assert.areEqual(item.version, savedItem.version);

                    return table.del(savedItem);
                },
                function (error) {
                    $assert.isNull(error);
                });
        }),

        $test('AsyncTableOperationsWithSystemProperties')
        .tag('SystemProperties')
        .checkAsync(function () {

            var client = $getClient(),
                table = client.getTable('StringIdJavascriptTable');

            return $chain(
                function () {
                    return emptyTable(table);
                },
                function () {
                    return table.insert({ string: 'a value'  });
                },
                function (item) {
                    $assert.isNotNull(item.createdAt);
                    $assert.isNotNull(item.updatedAt);
                    $assert.isNotNull(item.version);

                    return emptyTable(table);
                });
        }),

        $test('AsyncFilterSelectOrderingOperationsNotImpactedBySystemProperties')
        .description('test table sorting with various system properties')
        .tag('SystemProperties')
        .checkAsync(function () {
            var client = $getClient(),
                table = client.getTable('StringIdJavascriptTable'),
                savedItems = [];

            return $chain(
                function () {
                    return emptyTable(table);
                },
                function () {
                    return table.insert({ id: '1', string: 'value' });
                },
                function (item) {
                    savedItems.push(item);
                    return table.insert({ id: '2', string: 'value' });
                },
                function (item) {
                    savedItems.push(item);
                    return table.insert({ id: '3', string: 'value' });
                },
                function (item) {
                    savedItems.push(item);
                    return table.insert({ id: '4', string: 'value' });
                },
                function (item) {
                    savedItems.push(item);
                    return table.insert({ id: '5', string: 'value' });
                },
                function (item) {
                    savedItems.push(item);

                    var commands = [];
                    commands.push(function () {
                        return $chain(
                            function () {
                                $log('testing properties: ' + systemProperties);
                                return table.orderBy('createdAt').read();
                            },
                            function (items) {
                                for (var i = 0; i < items.length - 1; i++) {
                                    $assert.isTrue(items[i].id <  items[i + 1].id);
                                }
                                return table.orderBy('updatedAt').read();
                            },
                            function (items) {
                                for (var i = 0; i < items.length - 1; i++) {
                                    $assert.isTrue(items[i].id < items[i + 1].id);
                                }
                                return table.orderBy('version').read();
                            },
                            function (items) {
                                for (var i = 0; i < items.length - 1; i++) {
                                    $assert.isTrue(items[i].id < items[i + 1].id);
                                }

                                return table.where(function (value) { return this.createdAt >= value; }, savedItems[3].createdAt).read();
                            },
                            function (items) {
                                $assert.areEqual(2, items.length);

                                return table.where(function (value) { return this.updatedAt >= value; }, savedItems[3].updatedAt).read();
                            },
                            function (items) {
                                $assert.areEqual(2, items.length);

                                return table.where({ version: savedItems[3].version }).read();
                            },
                            function (items) {
                                $assert.areEqual(1, items.length);

                                return table.select('id', 'createdAt').read();
                            },
                            function (items) {
                                for (var i = 0; i < items.length; i++) {
                                    $assert.isNotNull(items[i].createdAt);
                                }

                                return table.select('id', 'updatedAt').read();
                            },
                            function (items) {
                                for (var i = 0; i < items.length; i++) {
                                    $assert.isNotNull(items[i].updatedAt);
                                }

                                return table.select('id', 'version').read();
                            },
                            function (items) {
                                for (var i = 0; i < items.length; i++) {
                                    $assert.isNotNull(items[i].version);
                                }

                            });
                    });
                    return $chain.apply(null, commands);
                });
        }),

        $test('UpdateAsyncWithWithMergeConflict')
        .description('test update with conflict')
        .tag('SystemProperties')
        .checkAsync(function () {
            var client = $getClient(),
                table = client.getTable('StringIdJavascriptTable'),
                savedVersion,
                correctVersion;

            return $chain(function () {
                return emptyTable(table);
            }, function () {
                return table.insert({ id: 'an id', string: 'a value'});
            }, function (item) {
                savedVersion = item.version;

                item.string = 'Hello!';
                return table.update(item);
            }, function (item) {
                $assert.areNotEqual(item.version, savedVersion);

                item.string = 'But Wait!';
                correctVersion = item.version;
                item.version = savedVersion;

                return table.update(item).then(function (item) {
                    $assert.fail('Should have failed');
                }, function (error) {
                    $assert.areEqual(412, error.request.status);
                    $assert.contains(error.message, "Precondition Failed");
                    $assert.areEqual(error.serverInstance.version, correctVersion);
                    $assert.areEqual(error.serverInstance.string, 'Hello!');

                    item.version = correctVersion;
                    return table.update(item);
                });
            }, function (item) {
                $assert.areNotEqual(item.version, correctVersion);
            });
        }),

        $test('DeleteAsyncWithWithMergeConflict')
        .description('test delete with conflict')
        .tag('SystemProperties')
        .checkAsync(function () {
            var client = $getClient(),
                table = client.getTable('StringIdJavascriptTable'),
                savedVersion,
                correctVersion;

            return $chain(function () {
                return emptyTable(table);
            }, function () {
                return table.insert({ id: 'an id', String: 'a value' });
            }, function (item) {
                savedVersion = item.version;

                item.String = 'Hello!';
                return table.update(item);
            }, function (item) {
                $assert.areNotEqual(item.version, savedVersion);

                item.String = 'But Wait!';
                correctVersion = item.version;
                item.version = savedVersion;

                return table.del(item).then(function (item) {
                    $assert.fail('Should have failed');
                }, function (error) {
                    $assert.areEqual(412, error.request.status);
                    $assert.contains(error.message, "Precondition Failed");
                    $assert.areEqual(error.serverInstance.version, correctVersion);
                    $assert.areEqual(error.serverInstance.String, 'Hello!');

                    item.version = correctVersion;
                    return table.del(item);
                });
            });
        })
    );