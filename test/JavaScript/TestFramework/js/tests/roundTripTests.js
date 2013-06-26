// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="../../../ZumoE2ETestAppJs/ZumoE2ETestAppJs/js/MobileServices.js" />
/// <reference path="../testFramework.js" />

function defineRoundTripTestsNamespace() {
    var tests = [];
    var tableName = 'w8jsRoundTripTable';

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
    tests.push(createRoundTripTest('Date: now (UTC)', 'date1', new Date(Date.UTC)));
    tests.push(createRoundTripTest('Date: null', 'date1', null));
    tests.push(createRoundTripTest('Date: unix 0', 'date1', new Date(1970, 1, 1, 0, 0, 0, 0)));
    tests.push(createRoundTripTest('Date: before unix 0', 'date1', new Date(1969, 12, 31, 23, 59, 59, 999)));
    tests.push(createRoundTripTest('Date: after unix 0', 'date1', new Date(1970, 1, 1, 0, 0, 0, 1)));
    tests.push(createRoundTripTest('Date: distant past', 'date1', new Date(1, 2, 3, 4, 5, 6, 7)));
    tests.push(createRoundTripTest('Date: distant future', 'date1', new Date(9876, 5, 4, 3, 2, 1)));

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

    tests.push(createNegativeRoundTripTest('(Neg) Insert item with \'id\' property (value = 0)', { id: 0, string1: 'hello' }));
    tests.push(createNegativeRoundTripTest('(Neg) Insert item with \'id\' property (value = 1)', { id: 1, string1: 'hello' }));
    tests.push(createNegativeRoundTripTest('(Neg) Insert item with \'Id\' property (value = 1)', { Id: 1, string1: 'hello' }));
    tests.push(createNegativeRoundTripTest('(Neg) Insert item with \'ID\' property (value = 1)', { ID: 1, string1: 'hello' }));

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
            var dateReviver = function (key, value) {
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
            var originalItem = JSON.parse(JSON.stringify(item), dateReviver);
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
