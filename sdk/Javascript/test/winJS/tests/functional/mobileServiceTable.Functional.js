// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
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
        .tag('dotNet_not_supported') 
        .checkAsync(function () {

            var client = $getClient(),
                table = client.getTable('StringIdJavascriptTable'),
                savedItem,
                savedVersion,
                savedUpdatedAt;

            table.systemProperties = WindowsAzure.MobileServiceTable.SystemProperties.All;  //All

            return $chain(
                function () {
                    return emptyTable(table);
                },
                function () {
                    return table.insert({ id: 'an id', string: 'a value' });
                },
                function (item) {
                    $assert.isNotNull(item.__createdAt);
                    $assert.isNotNull(item.__updatedAt);
                    $assert.isNotNull(item.__version);

                    return table.read();
                },
                function (items) {
                    $assert.areEqual(1, items.length);
                    var item = items[0];
                    $assert.isNotNull(item.__createdAt);
                    $assert.isNotNull(item.__updatedAt);
                    $assert.isNotNull(item.__version);
                    savedItem = item;

                    return table.where(function (value) { return this.__version == value; }, item.__version).read();
                },
                function (items) {
                    $assert.areEqual(1, items.length);
                    var item = items[0];
                    $assert.isNotNull(item.__createdAt);
                    $assert.isNotNull(item.__updatedAt);
                    $assert.isNotNull(item.__version);

                    return table.where(function (value) { return this.__createdAt == value; }, savedItem.__createdAt).read();
                },
                function (items) {
                    $assert.areEqual(1, items.length);
                    var item = items[0];
                    $assert.isNotNull(item.__createdAt);
                    $assert.isNotNull(item.__updatedAt);
                    $assert.isNotNull(item.__version);

                    return table.where(function (value) { return this.__updatedAt == value; }, savedItem.__updatedAt).read();
                },
                function (items) {
                    $assert.areEqual(1, items.length);
                    var item = items[0];
                    $assert.isNotNull(item.__createdAt);
                    $assert.isNotNull(item.__updatedAt);
                    $assert.isNotNull(item.__version);

                    return table.lookup(savedItem.id);
                },
                function (item) {
                    $assert.areEqual(item.id, savedItem.id);
                    $assert.areEqual(item.__updatedAt.valueOf(), savedItem.__updatedAt.valueOf());
                    $assert.areEqual(item.__createdAt.valueOf(), savedItem.__createdAt.valueOf());
                    $assert.areEqual(item.__version, savedItem.__version);

                    savedItem.string = 'Hello';
                    savedVersion = savedItem.__version; //WinJS Mutates
                    savedUpdatedAt = savedItem.__updatedAt.valueOf();

                    return table.update(savedItem);
                },
                function (item) {
                    $assert.areEqual(item.id, savedItem.id);
                    $assert.areEqual(item.__createdAt.valueOf(), savedItem.__createdAt.valueOf());
                    $assert.areNotEqual(item.__version, savedVersion);
                    $assert.areNotEqual(item.__updatedAt.valueOf(), savedUpdatedAt);
                    savedItem = item;

                    return table.read();
                },
                function (items) {
                    var item = items[0];
                    $assert.areEqual(item.id, savedItem.id);
                    $assert.areEqual(item.__updatedAt.valueOf(), savedItem.__updatedAt.valueOf());
                    $assert.areEqual(item.__createdAt.valueOf(), savedItem.__createdAt.valueOf());
                    $assert.areEqual(item.__version, savedItem.__version);

                    return table.del(savedItem);
                },
                function (error) {
                    $assert.isNull(error);
                });
        }),

        $test('AsyncTableOperationsWithSystemPropertiesSetExplicitly')
        .tag('SystemProperties')
        .tag('dotNet_not_supported') // .NET apps always return all system properties on insert operation
        .checkAsync(function () {

            var client = $getClient(),
                table = client.getTable('StringIdJavascriptTable'),
                props = WindowsAzure.MobileServiceTable.SystemProperties;

            table.systemProperties = props.Version | props.CreatedAt | props.UpdatedAt;

            return $chain(
                function () {
                    return emptyTable(table);
                },
                function () {
                    return table.insert({ string: 'a value'  });
                },
                function (item) {
                    $assert.isNotNull(item.__createdAt);
                    $assert.isNotNull(item.__updatedAt);
                    $assert.isNotNull(item.__version);

                    table.systemProperties = props.Version | props.CreatedAt;
                    return table.insert({ string: 'a value' });
                },
                function (item) {
                    $assert.isNotNull(item.__createdAt);
                    $assert.isNotNull(item.__version);
                    $assert.isNull(item.__updatedAt);

                    table.systemProperties = props.Version | props.UpdatedAt;
                    return table.insert({ string: 'a value' });
                },
                function (item) {
                    $assert.isNull(item.__createdAt);
                    $assert.isNotNull(item.__updatedAt);
                    $assert.isNotNull(item.__version);

                    table.systemProperties = props.UpdatedAt | props.CreatedAt;
                    return table.insert({ string: 'a value' });
                },
                function (item) {
                    $assert.isNotNull(item.__createdAt);
                    $assert.isNotNull(item.__updatedAt);
                    $assert.isNull(item.__version);

                    table.systemProperties = props.UpdatedAt;
                    return table.insert({ string: 'a value' });
                }, function (item) {
                    $assert.isNull(item.__createdAt);
                    $assert.isNotNull(item.__updatedAt);
                    $assert.isNull(item.__version);

                    return emptyTable(table);
                });
        }),

        $test('AsyncTableOperationsWithAllSystemPropertiesUsingCustomSystemParameters')
        .tag('SystemProperties')
        .tag('dotNet_not_supported') // .NET apps don't support filtering system properties
        .checkAsync(function () {
            var client = $getClient(),
                table = client.getTable('StringIdJavascriptTable'),
                savedItem;

            return $chain(
                function () {
                    return emptyTable(table);
                },
                function () {
                    var commands = [];
                    testData.testValidSystemPropertyQueryStrings.forEach(function (systemProperties) {
                        var systemPropertiesKeyValue = systemProperties.split('='),
                            userParams = {},
                            savedVersion;

                        userParams[systemPropertiesKeyValue[0]] = systemPropertiesKeyValue[1];

                        var lowerCaseSysProperties = systemProperties.toLowerCase(),
                            shouldHaveCreatedAt = lowerCaseSysProperties.indexOf('created') !== -1,
                            shouldHaveUpdatedAt = lowerCaseSysProperties.indexOf('updated') !== -1,
                            shouldHaveVersion = lowerCaseSysProperties.indexOf('version') !== -1;

                        if (lowerCaseSysProperties.indexOf('*') !== -1) {
                            shouldHaveCreatedAt = shouldHaveUpdatedAt = shouldHaveVersion = true;
                        }

                        commands.push(function () {
                            return $chain(
                                function () {
                                    return table.insert({ id: 'an id', string: 'a value' }, userParams);
                                },
                                function (item) {
                                    $assert.areEqual(shouldHaveCreatedAt, item.__createdAt !== undefined);
                                    $assert.areEqual(shouldHaveUpdatedAt, item.__updatedAt !== undefined);
                                    $assert.areEqual(shouldHaveVersion, item.__version !== undefined);
                                    savedItem = item;

                                    return table.read('', userParams);
                                },
                                function (items) {
                                    $assert.areEqual(1, items.length);

                                    var item = items[0];
                                    $assert.areEqual(shouldHaveCreatedAt, item.__createdAt !== undefined);
                                    $assert.areEqual(shouldHaveUpdatedAt, item.__updatedAt !== undefined);
                                    $assert.areEqual(shouldHaveVersion, item.__version !== undefined);

                                    return table.where({ __version: savedItem.__version }).read(userParams);
                                },
                                function (items) {
                                    if (shouldHaveVersion) {
                                        $assert.areEqual(1, items.length);
                                        var item = items[0];
                                        $assert.areEqual(shouldHaveCreatedAt, item.__createdAt !== undefined);
                                        $assert.areEqual(shouldHaveUpdatedAt, item.__updatedAt !== undefined);
                                        $assert.areEqual(shouldHaveVersion, item.__version !== undefined);
                                    } else {
                                        $assert.areEqual(0, items.length);
                                    }

                                    return table.where({ __createdAt: savedItem.__createdAt }).read(userParams);
                                },
                               function (items) {
                                   if (shouldHaveCreatedAt) {
                                       $assert.areEqual(1, items.length);
                                       var item = items[0];
                                       $assert.areEqual(shouldHaveCreatedAt, item.__createdAt !== undefined);
                                       $assert.areEqual(shouldHaveUpdatedAt, item.__updatedAt !== undefined);
                                       $assert.areEqual(shouldHaveVersion, item.__version !== undefined);
                                   } else {
                                       $assert.areEqual(0, items.length);
                                   }

                                   return table.where({ __updatedAt: savedItem.__updatedAt }).read(userParams);
                               },
                               function (items) {
                                   if (shouldHaveUpdatedAt) {
                                       $assert.areEqual(1, items.length);
                                       var item = items[0];
                                       $assert.areEqual(shouldHaveCreatedAt, item.__createdAt !== undefined);
                                       $assert.areEqual(shouldHaveUpdatedAt, item.__updatedAt !== undefined);
                                       $assert.areEqual(shouldHaveVersion, item.__version !== undefined);
                                   } else {
                                       $assert.areEqual(0, items.length);
                                   }

                                   return table.lookup(savedItem.id, userParams);
                               },
                               function (item) {
                                   $assert.areEqual(shouldHaveCreatedAt, item.__createdAt !== undefined);
                                   $assert.areEqual(shouldHaveUpdatedAt, item.__updatedAt !== undefined);
                                   $assert.areEqual(shouldHaveVersion, item.__version !== undefined);

                                   savedItem.string = 'Hello!';
                                   savedVersion = item.__version;
                                   return table.update(savedItem, userParams);
                               },
                               function (item) {
                                   $assert.areEqual(shouldHaveCreatedAt, item.__createdAt !== undefined);
                                   $assert.areEqual(shouldHaveUpdatedAt, item.__updatedAt !== undefined);
                                   $assert.areEqual(shouldHaveVersion, item.__version !== undefined);
                                   if (shouldHaveVersion) {
                                       $assert.areNotEqual(item.__version, savedVersion);
                                   }

                                   return table.del(item);
                               });
                        });
                    });
                    return $chain.apply(null, commands);
                });
        }),

        $test('AsyncTableOperationsWithInvalidSystemPropertiesQuerystring')
        .tag('SystemProperties')
        .tag('dotNet_not_supported') // .NET runtime apps always get items whatever you put into __systemproperties
        .checkAsync(function () {
            var client = $getClient(),
                table = client.getTable('StringIdJavascriptTable'),
                commands = [];

            testData.testInvalidSystemPropertyQueryStrings.forEach(function (systemProperties) {
                var systemPropertiesKeyValue = systemProperties.split('='),
                    userParams = {};

                userParams[systemPropertiesKeyValue[0]] = systemPropertiesKeyValue[1];

                commands.push(
                    function () {
                        $log('querystring: ' + systemProperties);
                        return table.insert({ id: 'an id', string: 'a value' }, userParams).then(
                            function (item) {
                                $assert.fail('Should have failed');
                            }, function (error) {
                                //$assert.contains(error.message, "is not a supported system property.");
                            });
                    },
                    function () {
                        return table.read('', userParams).then(function (items) {
                            $assert.fail('Should have failed');
                        }, function (error) {
                            //$assert.contains(error.message, "is not a supported system property.");
                        });
                    }, function () {
                        return table.where({ __version: 'AAA' }).read(userParams).then(function (items) {
                            $assert.fail('Should have failed');
                        }, function (error) {
                            //$assert.contains(error.message, "is not a supported system property.");
                        });
                    }, function () {
                        return table.lookup('an id', userParams).then(function (items) {
                            $assert.fail('Should have failed');
                        }, function (error) {
                            //$assert.contains(error.message, "is not a supported system property.");
                        });
                    }, function () {
                        return table.update({ id: 'an id', string: 'new value' }, userParams).then(function (items) {
                            $assert.fail('Should have failed');
                        }, function (error) {
                            //$assert.contains(error.message, "is not a supported system property.");
                        });
                    });
            });

            return $chain.apply(null, commands);
        }),

        $test('AsyncTableOperationsWithInvalidSystemParameterQueryString')
        .description('test table ops with invalid querystrings')
        .tag('SystemProperties')
        .tag('dotNet_not_supported') // .NET apps don't support filtering system properties
        .checkAsync(function () {
            var client = $getClient(),
                table = client.getTable('StringIdJavascriptTable'),
                savedItem,
                systemPropertiesKeyValue = testData.testInvalidSystemParameterQueryString.split('='),
                userParams = {},
                savedVersion;

            userParams[systemPropertiesKeyValue[0]] = systemPropertiesKeyValue[1];

            return $chain(
                function () {
                    return emptyTable(table);
                },
                function () {
                    return table.insert({ id: 'an id', string: 'a value' }, userParams).then(
                        function (item) {
                            $assert.fail('Should have failed');
                        }, function (error) {
                            //$assert.contains(error.message, 'Custom query parameter names must start with a letter.');
                        });
                },
                function () {
                    return table.read('', userParams).then(
                        function (item) {
                            $assert.fail('Should have failed');
                        }, function (error) {
                            //$assert.contains(error.message, 'Custom query parameter names must start with a letter.');
                        });
                },
                function () {
                    return table.where({ __version: 'AAA'}).read(userParams).then(
                        function (item) {
                            $assert.fail('Should have failed');
                        }, function (error) {
                            //$assert.contains(error.message, 'Custom query parameter names must start with a letter.');
                        });
                },
                function () {
                    return table.lookup('an id', userParams).then(
                        function (item) {
                            $assert.fail('Should have failed');
                        }, function (error) {
                            //$assert.contains(error.message, 'Custom query parameter names must start with a letter.');
                        });
                },
                function () {
                    return table.update({ id: 'an id', string: 'new value'}, userParams).then(
                        function (item) {
                            $assert.fail('Should have failed');
                        }, function (error) {
                            //$assert.contains(error.message, 'Custom query parameter names must start with a letter.');
                        });
                });
        }),

        $test('AsyncFilterSelectOrderingOperationsNotImpactedBySystemProperties')
        .description('test table sorting with various system properties')
        .tag('SystemProperties')
        .tag('dotNet_not_supported') // .NET apps don't support filtering system properties
        .checkAsync(function () {
            var client = $getClient(),
                table = client.getTable('StringIdJavascriptTable'),
                savedItems = [];

            table.systemProperties = WindowsAzure.MobileServiceTable.SystemProperties.All;

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
                    testData.testSystemProperties.forEach(function (systemProperties) {
                        table.systemProperties = systemProperties;
                        commands.push(function () {
                            return $chain(
                                function () {
                                    $log('testing properties: ' + systemProperties);
                                    return table.orderBy('__createdAt').read();
                                },
                                function (items) {
                                    for (var i = 0; i < items.length - 1; i++) {
                                        $assert.isTrue(items[i].id <  items[i + 1].id);
                                    }
                                    return table.orderBy('__updatedAt').read();
                                },
                                function (items) {
                                    for (var i = 0; i < items.length - 1; i++) {
                                        $assert.isTrue(items[i].id < items[i + 1].id);
                                    }
                                    return table.orderBy('__version').read();
                                },
                                function (items) {
                                    for (var i = 0; i < items.length - 1; i++) {
                                        $assert.isTrue(items[i].id < items[i + 1].id);
                                    }

                                    return table.where(function (value) { return this.__createdAt >= value; }, savedItems[3].__createdAt).read();
                                },
                                function (items) {
                                    $assert.areEqual(2, items.length);

                                    return table.where(function (value) { return this.__updatedAt >= value; }, savedItems[3].__updatedAt).read();
                                },
                                function (items) {
                                    $assert.areEqual(2, items.length);

                                    return table.where({ __version: savedItems[3].__version }).read();
                                },
                                function (items) {
                                    $assert.areEqual(1, items.length);

                                    return table.select('id', '__createdAt').read();
                                },
                                function (items) {
                                    for (var i = 0; i < items.length; i++) {
                                        $assert.isNotNull(items[i].__createdAt);
                                    }

                                    return table.select('id', '__updatedAt').read();
                                },
                                function (items) {
                                    for (var i = 0; i < items.length; i++) {
                                        $assert.isNotNull(items[i].__updatedAt);
                                    }

                                    return table.select('id', '__version').read();
                                },
                                function (items) {
                                    for (var i = 0; i < items.length; i++) {
                                        $assert.isNotNull(items[i].__version);
                                    }

                                });
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

            table.systemProperties = WindowsAzure.MobileServiceTable.SystemProperties.All;

            return $chain(function () {
                return emptyTable(table);
            }, function () {
                return table.insert({ id: 'an id', string: 'a value'});
            }, function (item) {
                savedVersion = item.__version;

                item.string = 'Hello!';
                return table.update(item);
            }, function (item) {
                $assert.areNotEqual(item.__version, savedVersion);

                item.string = 'But Wait!';
                correctVersion = item.__version;
                item.__version = savedVersion;

                return table.update(item).then(function (item) {
                    $assert.fail('Should have failed');
                }, function (error) {
                    $assert.areEqual(412, error.request.status);
                    $assert.contains(error.message, "Precondition Failed");
                    $assert.areEqual(error.serverInstance.__version, correctVersion);
                    $assert.areEqual(error.serverInstance.string, 'Hello!');

                    item.__version = correctVersion;
                    return table.update(item);
                });
            }, function (item) {
                $assert.areNotEqual(item.__version, correctVersion);
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

            table.systemProperties = WindowsAzure.MobileServiceTable.SystemProperties.All;

            return $chain(function () {
                return emptyTable(table);
            }, function () {
                return table.insert({ id: 'an id', String: 'a value' });
            }, function (item) {
                savedVersion = item.__version;

                item.String = 'Hello!';
                return table.update(item);
            }, function (item) {
                $assert.areNotEqual(item.__version, savedVersion);

                item.String = 'But Wait!';
                correctVersion = item.__version;
                item.__version = savedVersion;

                return table.del(item).then(function (item) {
                    $assert.fail('Should have failed');
                }, function (error) {
                    $assert.areEqual(412, error.request.status);
                    $assert.contains(error.message, "Precondition Failed");
                    $assert.areEqual(error.serverInstance.__version, correctVersion);
                    $assert.areEqual(error.serverInstance.String, 'Hello!');

                    item.__version = correctVersion;
                    return table.del(item);
                });
            });
        })
    );