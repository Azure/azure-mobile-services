// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="../../../ZumoE2ETestAppJs/ZumoE2ETestAppJs/js/MobileServices.js" />
/// <reference path="../testFramework.js" />

function defineRoundTripTestsNamespace() {
    var tests = [];
    var tableName = 'w8jsRoundTripTable';
    var stringIdTableName = 'stringIdRoundTripTable';

    tests.push(new zumo.Test('Setup dynamic schema', function (test, done) {
        var table = zumo.getClient().getTable(tableName);
        var item = {
            string1: "test",
            date1: new Date(),
            bool1: true,
            number: -1,
        };
        table.insert(item).done(function () {
            test.addLog('Successfully set up the dynamic schema: ' + JSON.stringify(item));
            done(true);
        }, function (err) {
            test.addLog('Error setting up dynamic schema: ' + JSON.stringify(err));
            done(false);
        });
    }));

    tests.push(createRoundTripTest('String: empty', 'string1', ''));
    tests.push(createRoundTripTest('String: null', 'string1', null));
    tests.push(createRoundTripTest('String: multiple characters', 'string1',
        ' !\"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~'));
    tests.push(createRoundTripTest('String: large (1000 characters)', 'string1', Array(1001).join('*')));
    tests.push(createRoundTripTest('String: large (64k+ characters)', 'string1', Array(65538).join('*')));

    tests.push(createRoundTripTest('String: non-ASCII characters - Latin', 'string1', 'ãéìôü ÇñÑ'));
    tests.push(createRoundTripTest('String: non-ASCII characters - Arabic', 'string1', 'الكتاب على الطاولة'));
    tests.push(createRoundTripTest('String: non-ASCII characters - Chinese', 'string1', '这本书在桌子上'));
    tests.push(createRoundTripTest('String: non-ASCII characters - Chinese 2', 'string1', '⒈①Ⅻㄨㄩ 啊阿鼾齄 丂丄狚狛 狜狝﨨﨩 ˊˋ˙–〇 㐀㐁䶴䶵'));
    tests.push(createRoundTripTest('String: non-ASCII characters - Japanese', 'string1', '本は机の上に'));
    tests.push(createRoundTripTest('String: non-ASCII characters - Hebrew', 'string1', 'הספר הוא על השולחן'));

    tests.push(createRoundTripTest('Date: now', 'date1', new Date()));
    tests.push(createRoundTripTest('Date: null', 'date1', null));
    tests.push(createRoundTripTest('Date: unix 0', 'date1', new Date(Date.UTC(1970, 1 - 1, 1, 0, 0, 0, 0))));
    tests.push(createRoundTripTest('Date: before unix 0', 'date1', new Date(Date.UTC(1969, 12 - 1, 31, 23, 59, 59, 999))));
    tests.push(createRoundTripTest('Date: after unix 0', 'date1', new Date(Date.UTC(1970, 1 - 1, 1, 0, 0, 0, 1))));
    tests.push(createRoundTripTest('Date: distant past', 'date1', new Date(1, 2 - 1, 3, 4, 5, 6, 7)));
    tests.push(createRoundTripTest('Date: distant future', 'date1', new Date(9876, 5 - 1, 4, 3, 2, 1)));

    tests.push(createRoundTripTest('Bool: true', 'bool1', true));
    tests.push(createRoundTripTest('Bool: false', 'bool1', false));
    tests.push(createRoundTripTest('Bool: null', 'bool1', null));

    tests.push(createRoundTripTest('Number: zero', 'number', 0));
    tests.push(createRoundTripTest('Number: positive', 'number', 12345678));
    tests.push(createRoundTripTest('Number: negative', 'number', -1232435));
    tests.push(createRoundTripTest('Number: large long', 'number', 1152921504606846975));
    tests.push(createRoundTripTest('Number: large long (negative)', 'number', -1152921504606846976));
    tests.push(createRoundTripTest('Number: floating point', 'number', 12345.678));
    tests.push(createRoundTripTest('Number: POSITIVE_INFINITY', 'number', Number.POSITIVE_INFINITY));
    tests.push(createRoundTripTest('Number: NEGATIVE_INFINITY', 'number', Number.NEGATIVE_INFINITY));
    tests.push(createRoundTripTest('Number: MAX_VALUE', 'number', Number.MAX_VALUE));
    tests.push(createRoundTripTest('Number: MIN_VALUE', 'number', Number.MIN_VALUE));
    tests.push(createRoundTripTest('Number: positive NaN', 'number', Number.NaN));
    tests.push(createRoundTripTest('Number: large floating point', 'number', 1.234e+123));

    tests.push(createRoundTripTest('Complex type (object): simple value', 'complexType',
        { Name: 'John Doe', Age: 33, Friends: ['Jane Roe', 'John Smith'] }));
    tests.push(createRoundTripTest('Complex type (object): null', 'complexType', null));
    tests.push(createRoundTripTest('Complex type (object): empty object', 'complexType', {}));
    tests.push(createRoundTripTest('Complex type (array): simple value', 'complexType',
        [{ Name: 'Scooby Doo', Age: 10 }, { Name: 'Shaggy', Age: 19 }]));
    tests.push(createRoundTripTest('Complex type (array): empty array', 'complexType', []));
    tests.push(createRoundTripTest('Complex type (array): array with null elements', 'complexType',
        [{ Name: 'Scooby Doo', Age: 10 }, null, { Name: 'Shaggy', Age: 19 }]));

    tests.push(createRoundTripTest('Invalid id: zero', 'id', 0));
    tests.push(createRoundTripTest('Invalid id: empty', 'id', ''));
    tests.push(createRoundTripTest('Invalid id: null', 'id', null));
    tests.push(createNegativeRoundTripTest('(Neg) Insert item with \'id\' property (value = 1)', { id: 1, string1: 'hello' }));
    tests.push(createNegativeRoundTripTest('(Neg) Insert item with \'Id\' property (value = 1)', { Id: 1, string1: 'hello' }));
    tests.push(createNegativeRoundTripTest('(Neg) Insert item with \'ID\' property (value = 1)', { ID: 1, string1: 'hello' }));

    for (var i = 0; i < tests.length; i++) {
        tests[i].addRequiredFeature(zumo.runtimeFeatureNames.INT_ID_TABLES);
    }

    var firstStringIdTestIndex = tests.length;

    tests.push(new zumo.Test('Setup string id dynamic schema', function (test, done) {
        var table = zumo.getClient().getTable(stringIdTableName);
        var item = {
            name: 'a string',
            number: 123.45,
            bool: true,
            date1: new Date(),
            complex: [ 'a complex object which will be converted to string at the database' ]
        };
        table.insert(item).done(function () {
            test.addLog('Successfully set up the dynamic schema: ' + JSON.stringify(item));
            done(true);
        }, function (err) {
            test.addLog('Error setting up dynamic schema: ' + JSON.stringify(err));
            done(false);
        });
    }));

    var itemName = 'ãéìôü ÇñÑ - الكتاب على الطاولة - 这本书在桌子上 - ⒈①Ⅻㄨㄩ 啊阿鼾齄 丂丄狚狛 狜狝﨨﨩 ˊˋ˙–〇 㐀㐁䶴䶵 - 本は机の上に - הספר הוא על השולחן';
    var itemNumber = Number.MAX_VALUE;
    var itemBool = true;
    var itemComplex = ['abc', 'def', 'ghi'];

    var differentIds = { ascii: 'id-', latin: 'ãéìôü ÇñÑ', arabic: 'الكتاب على الطاولة', chinese: '这本书在桌子上', hebrew: 'הספר הוא על השולחן' };
    tests.push(createStringIdRoundTripTest('String id - no id on insert, multiple properties', { name: itemName, number: itemNumber, bool: itemBool, date1: zumo.util.randomDate(), complex: itemComplex }));
    for (var t in differentIds) {
        tests.push(createStringIdRoundTripTest('String id - ' + t + ' id on insert, multiple properties', { id: differentIds[t], name: t, number: itemNumber, bool: itemBool, date1: zumo.util.randomDate(), complex: itemComplex }));
    }

    var invalidIds = ['.', '..', 'control\u0010characters', 'large id' + Array(260).join('*')];
    invalidIds.forEach(function (id) {
        tests.push(createNegativeRoundTripTest('(Neg) String id - insert with invalid id: ' + (id.length > 30 ? (id.substring(0, 30) + '...') : id), { id: id, name: 'hello' }));
    });

    for (var i = firstStringIdTestIndex; i < tests.length; i++) {
        tests[i].addRequiredFeature(zumo.runtimeFeatureNames.STRING_ID_TABLES);
    }

    var currentIEVersion = zumo.getIEBrowserVersion(); // get IE8 version ...
    function dateReviver(key, value) {
        var re = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})\.(\d{3})Z$/;
        if (typeof value === 'string') {
            if (currentIEVersion === 8.0) {
                // UTC date(yyyy-mm-ddTtt:hh:mm:milZ) format isn't supported in IE8
                re = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})Z$/;
                if (value === '--T::Z') { // '--T::Z this format is not supported by IE8 so we use defult date
                    value = '1970-01-01T00:00:00Z'; //Default Date and time
                    item[propertyName] = value;
                }
            }

            var a = re.exec(value);
            if (a) {
                if (currentIEVersion === 8.0) {
                    var d = new Date(Date.UTC(+a[1], +a[2] - 1, +a[3], +a[4], +a[5], +a[6]));
                    return d;
                }

                var d = new Date(Date.UTC(+a[1], +a[2] - 1, +a[3], +a[4], +a[5], +a[6], +a[7]));
                return d;

            }
        }
        return value;
    }

    function createStringIdRoundTripTest(testName, objectToInsert) {
        return new zumo.Test(testName, function (test, done) {
            var table = zumo.getClient().getTable(stringIdTableName);
            var currentIEVersion = zumo.getIEBrowserVersion();
            var strItem = JSON.stringify(objectToInsert);
            var originalItem = JSON.parse(strItem, dateReviver);
            var hasId = !!objectToInsert.id;
            if (hasId) {
                // force id to be unique
                originalItem.id = originalItem.id + '-' + (new Date().toISOString());
            }
            var originalId = originalItem.id;
            table.insert(originalItem).done(function (itemInserted) {
                test.addLog('Inserted item: ', JSON.stringify(itemInserted));
                var id = itemInserted.id;

                if (hasId) {
                    if (originalId !== id) {
                        test.addLog('Error, id passed to insert is not the same (' + objectToInsert.id + ') as the id returned by the server (' + id + ')');
                        done(false);
                        return;
                    }
                } else {
                    if (!id) {
                        test.addLog('Error, inserted object does not have an \'id\' property');
                        done(false);
                        return;
                    } else {
                        if (typeof id !== 'string') {
                            test.addLog('Error, id should be a string');
                            done(false);
                            return;
                        }
                    }
                }

                table.lookup(id).done(function (retrieved) {
                    test.addLog('Retrieved the item from the service: ' + JSON.stringify(retrieved));
                    var errors = [];
                    if (zumo.util.compare(originalItem, retrieved, errors)) {
                        test.addLog('Object round tripped successfully.');
                        test.addLog('Now trying to insert an item with an existing id (should fail)');
                        var newItem = { id: id, name: 'something' };
                        table.insert(newItem).done(function (inserted2) {
                            test.addLog('Error, item was inserted but should not have been: ', inserted2);
                            done(false);
                        }, function (err) {
                            test.addLog('Ok, got expected error');
                            done(true);
                        });
                    } else {
                        for (var index = 0; index < errors.length; index++) {
                            var error = errors[index];
                            test.addLog(error);
                        }
                        test.addLog('Round-tripped item is different!');
                        done(false);
                    }
                }, function (err) {
                    test.addLog('Error retrieving data: ' + JSON.stringify(err));
                    done(false);
                });
            }, function (err) {
                test.addLog('Error inserting data: ' + JSON.stringify(err));
                done(false);
            });
        });
    }

    function createNegativeRoundTripTest(testName, objectToInsert) {
        return new zumo.Test(testName, function (test, done) {
            var table = zumo.getClient().getTable(tableName);
            table.insert(objectToInsert).done(function () {
                test.addLog('Error, insertion of object should have failed, but succeeded: ', JSON.stringify(objectToInsert));
                done(false);
            }, function (err) {
                test.addLog('Received expected error: ' + JSON.stringify(err));
                done(true);
            });
        });
    }

    function createRoundTripTest(testName, propertyName, value) {
        return new zumo.Test(testName, function (test, done) {
            var item = {};
            item[propertyName] = value;
            var table = zumo.getClient().getTable(tableName);
            var currentIEVersion = zumo.getIEBrowserVersion(); // get IE8 version ...
            var strItem = JSON.stringify(item);
            var originalItem = JSON.parse(strItem, dateReviver);
            table.insert(item).done(function (itemInserted) {
                test.addLog('Inserted item: ', JSON.stringify(itemInserted));
                var id = itemInserted.id;
                if (id <= 0) {
                    test.addLog('Error, insert did not succeed: id=' + id);
                    done(false);
                } else {
                    table.lookup(id).done(function (retrieved) {
                        test.addLog('Retrieved the item from the service: ' + JSON.stringify(retrieved));
                        var errors = [];
                        if (propertyName === 'id') {
                            originalItem.id = id;
                        }
                        if (zumo.util.compare(originalItem, retrieved, errors)) {
                            test.addLog('Object round tripped successfully.');
                            done(true);
                        } else {
                            for (var index = 0; index < errors.length; index++) {
                                var error = errors[index];
                                test.addLog(error);
                            }
                            test.addLog('Round-tripped item is different!');
                            done(false);
                        }
                    }, function (err) {
                        test.addLog('Error retrieving data: ' + JSON.stringify(err));
                        done(false);
                    });
                }
            }, function (err) {
                test.addLog('Error inserting data: ' + JSON.stringify(err));
                done(false);
            });
        });
    }

    return {
        name: 'Round trip',
        tests: tests
    };
}

zumo.tests.roundTrip = defineRoundTripTestsNamespace();
