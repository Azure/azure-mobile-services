// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="..\..\js\MobileServices.Internals.js" />
/// <reference path="..\..\generated\Tests.js" />

// Declare JSHint globals
/*global MobileServiceClient:false, Platform:false */

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
            "id with 255 characters " + new Array(257 - 24).join('A'),
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

function emptyTable(table) {
    return table.read().then(function (results) {
        var deletes = [];
        results.forEach(function (result) {
            deletes.push(function (prevResult) {
                return table.del(result);
            });
        });
        return $chain.apply(null, deletes);
    });
}
$testGroup('Mobile Service Table Tests')
    .functional()
    .tests(
        $test('table operations')
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
                    validStringIds.forEach(function (id) {
                        singleIdTests.push(function () {
                            return $chain(
                                function () {
                                    return table.insert({ id: 'apple', string: 'Hey' });
                                },
                                function (item) {
                                    $assert.areEqual('apple', item.id);

                                    return table.read();
                                },
                                function (items) {
                                    $assert.areEqual(1, items.length);
                                    $assert.areEqual('apple', items[0].id);
                                    $assert.areEqual('Hey', items[0].string);

                                    return table.where({ id: 'apple' }).read();
                                },
                                function (items) {
                                    $assert.areEqual(1, items.length);
                                    $assert.areEqual('apple', items[0].id);
                                    $assert.areEqual('Hey', items[0].string);

                                    return table.select('id', 'string').read();
                                }, function (items) {
                                    $assert.areEqual(1, items.length);
                                    $assert.areEqual('apple', items[0].id);
                                    $assert.areEqual('Hey', items[0].string);

                                    return table.lookup('apple');
                                }, function (item) {
                                    $assert.areEqual('apple', item.id);
                                    $assert.areEqual('Hey', item.string);

                                    item.string = "What?";
                                    return table.update(item);
                                }, function (item) {
                                    $assert.areEqual('apple', item.id);
                                    $assert.areEqual('What?', item.string);

                                    item = { id: 'apple', string: 'hey' };

                                    return table.refresh(item);
                                }, function (item) {
                                    $assert.areEqual('apple', item.id);
                                    $assert.areEqual('What?', item.string);

                                    return table.read();
                                }, function (items) {
                                    $assert.areEqual(1, items.length);
                                    $assert.areEqual('apple', items[0].id);
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
                    validStringIds.forEach(function (testId) {
                        inserts.push(function (result) {
                            return table.insert({ id: testId, string: 'Hey' });
                        });
                    });
                    return $chain.apply(null, inserts);
                },
                function () {
                    $log('verify no results for ids we didn\'t use');
                    var testIds = emptyStringIds.concat(invalidStringIds).concat(null),
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
                    validStringIds.forEach(function (testId) {
                        inserts.push(function (result) {
                            return table.insert({ id: testId, string: 'Hey' });
                        });
                    });
                    return $chain.apply(null, inserts);
                },
                function () {
                    $log('Delete our data');
                    var deletes = [];
                    validStringIds.forEach(function (testId) {
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
                    validStringIds.forEach(function (testId) {
                        lookups.push(function () {
                            return table.lookup(testId).then(function (item) {
                                $assert.fail('should have failed');
                            }, function (error) {
                                $assert.contains(error.message, 'does not exist.');
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
                    validStringIds.forEach(function (testId) {
                        inserts.push(function (result) {
                            return table.insert({ id: testId, string: 'Hey' });
                        });
                    });
                    return $chain.apply(null, inserts);
                },
                function () {
                    var lookups = [];
                    validStringIds.forEach(function (testId) {
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
                    validStringIds.forEach(function (testId) {
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
                    validStringIds.forEach(function (testId) {
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
                    validStringIds.forEach(function (testId) {
                        inserts.push(function (result) {
                            return table.insert({ id: testId, string: 'Hey' });
                        });
                    });
                    return $chain.apply(null, inserts);
                },
                function () {
                    $log('Lookup our data');
                    var lookups = [];
                    validStringIds.forEach(function (testId) {
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
                    validStringIds.forEach(function (testId) {
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
                    validStringIds.forEach(function (testId) {
                        inserts.push(function (result) {
                            return table.insert({ id: testId, string: 'Hey' });
                        });
                    });
                    return $chain.apply(null, inserts);
                },
                function () {
                    $log('Delete our data');
                    var deletes = [];
                    validStringIds.forEach(function (testId) {
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
                    validStringIds.forEach(function (testId) {
                        updates.push(function () {
                            return table.update({ id: testId, string: 'Alright!' }).then(function (item) {
                                $assert.fail('Should have failed.');
                            }, function (error) {
                                $assert.contains(error.message, 'does not exist');
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
                    validStringIds.forEach(function (testId) {
                        inserts.push(function (result) {
                            return table.insert({ id: testId, string: 'Hey' });
                        });
                    });
                    return $chain.apply(null, inserts);
                },
                function () {
                    $log('Verify our data');
                    var lookups = [];
                    validStringIds.forEach(function (testId) {
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
                    validStringIds.forEach(function (testId) {
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
                    validStringIds.forEach(function (testId) {
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
        })
    );