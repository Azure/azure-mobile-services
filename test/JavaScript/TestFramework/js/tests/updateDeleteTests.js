// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="../../../ZumoE2ETestAppJs/ZumoE2ETestAppJs/js/MobileServices.js" />
/// <reference path="../testFramework.js" />

function defineUpdateDeleteTestsNamespace() {
    var tests = [];
    var i;
    var tableName = 'w8jsRoundTripTable';
    var stringIdTableName = 'stringIdRoundTripTable';

    tests.push(createDeleteTest('Delete item', function (test, done, table, id) {
        table.del({ id: id }).done(function () {
            test.addLog('Success callback called. Now trying to retrieve the data, it should return a 404');
            table.lookup(id).done(function (item) {
                test.addLog('Error, item has not been deleted: ' + JSON.stringify(item));
                done(false);
            }, function (err) {
                test.addLog('Error callback called as expected: ' + JSON.stringify(err));
                var request = err.request;
                if (request && request.status == 404) {
                    test.addLog('Got expected status code from server');
                    done(true);
                } else {
                    test.addLog('Error, status code not the expected');
                    done(false);
                }
            });
        }, function (err) {
            test.addLog('Error, this should be a positive test, but error callback was called: ' + JSON.stringify(err));
            done(false);
        });
    }));

    tests.push(createDeleteTest('(Neg) Delete item by passing a non-object', function (test, done, table, id) {
        table.del(id).done(function () {
            test.addLog('Error, success function was called but expected error.');
            done(false);
        }, function (err) {
            test.addLog('Expected error callback called: ' + JSON.stringify(err));
            done(true);
        });
    }));

    tests.push(createDeleteTest('(Neg) Delete item by passing an object with no \'id\' property', function (test, done, table, id) {
        table.del({ string1: 'test' }).done(function () {
            test.addLog('Error, success function was called but expected error.');
            done(false);
        }, function (err) {
            test.addLog('Expected error callback called: ' + JSON.stringify(err));
            done(true);
        });
    }));

    tests.push(createDeleteTest('(Neg) Delete item by passing an object with invalid \'id\' property', function (test, done, table, id) {
        table.del({ string1: 'test', id: 1234567890 }).done(function () {
            test.addLog('Error, success function was called but expected error.');
            done(false);
        }, function (err) {
            test.addLog('Expected error callback called: ' + JSON.stringify(err));
            var request = err.request;
            if (request && request.status == 404) {
                test.addLog('Got expected status code from server');
                done(true);
            } else {
                test.addLog('Error, status code not the expected');
                done(false);
            }
        });
    }));

    var ids = ['with space', '1234567', 'non-english ãéìôü ÇñÑالكتاب على الطاولة这本书在桌子上הספר הוא על השולחן'];
    ids.forEach(function (id) {
        tests.push(new zumo.Test('[string id] Delete, id = ' + id, function (test, done) {
            var client = zumo.getClient();
            var table = client.getTable(stringIdTableName);

            function afterInsert(place) {
                // insert will either have failed (item already exists) or succeeded (item didn't exist);
                // in either case we'll be able to delete it.
                return function (obj) {
                    test.addLog(place + ': ', obj);
                    table.del({ id: id }).done(function () {
                        test.addLog('Delete succeeded');
                        done(true);
                    }, function (err) {
                        test.addLog('Error calling delete: ', err);
                        done(false);
                    });
                }
            }

            table.insert({ id: id, name: 'test' }).done(afterInsert('Inserted'), afterInsert('Insert error (likely expected)'));
        }, zumo.runtimeFeatureNames.STRING_ID_TABLES));
    });

    tests.push(createUpdateTest('Update item', function (test, done, table, insertedItem) {
        var newNumber = 999;
        table.update({ id: insertedItem.id, number: newNumber }).done(function (item) {
            if (item.number === newNumber) {
                test.addLog('Updated number seems correct. Will retrieve it again to guarantee');
                table.lookup(insertedItem.id).done(function (newItem) {
                    var errors = [];
                    if (zumo.util.compare(item, newItem, errors)) {
                        test.addLog('Retrieved item is correct.');
                        done(true);
                    } else {
                        var error;
                        for (error in errors) {
                            test.addLog(error);
                        }
                        test.addLog('Retrieved item is not the expected one: ' + JSON.stringify(newItem));
                        done(false);
                    }
                }, function (err) {
                    test.addLog('Error retrieving the item: ' + JSON.stringify(err));
                    zumo.util.traceResponse(test, err.request);
                    done(false);
                });
            } else {
                test.addLog('Error, update did not succeed, item = ' + JSON.stringify(item));
                done(false);
            }
        }, function (err) {
            test.addLog('Error, this should be a positive test, but error callback was called: ' + JSON.stringify(err));
            zumo.util.traceResponse(test, err.request);
            done(false);
        });
    }));

    tests.push(createUpdateTest('(Neg) Update non-object', function (test, done, table, insertedItem) {
        table.update(insertedItem.id).done(function () {
            test.addLog('Error, success function was called but expected error.');
            done(false);
        }, function (err) {
            test.addLog('Expected error callback called: ' + JSON.stringify(err));
            zumo.util.traceResponse(test, err.request);
            done(true);
        });
    }));

    tests.push(createUpdateTest('(Neg) Update object without an id', function (test, done, table, insertedItem) {
        var newItem = JSON.parse(JSON.stringify(insertedItem));
        delete newItem.id;
        table.update(newItem).done(function () {
            test.addLog('Error, success function was called but expected error.');
            done(false);
        }, function (err) {
            test.addLog('Expected error callback called: ' + JSON.stringify(err));
            zumo.util.traceResponse(test, err.request);
            done(true);
        });
    }));

    tests.push(createUpdateTest('(Neg) Update object with inexistent id', function (test, done, table, insertedItem) {
        table.update({ id: 123456789, number: 123 }).done(function () {
            test.addLog('Error, success function was called but expected error.');
            done(false);
        }, function (err) {
            test.addLog('Expected error callback called: ' + JSON.stringify(err));
            var request = err.request;
            if (request && request.status == 404) {
                test.addLog('Got expected status code from server');
                done(true);
            } else {
                test.addLog('Error, status code not the expected');
                zumo.util.traceResponse(test, request);
                done(false);
            }
        });
    }));

    ids.forEach(function (id) {
        tests.push(new zumo.Test('[string id] Update, id = ' + id, function (test, done) {
            var client = zumo.getClient();
            var table = client.getTable(stringIdTableName);

            function afterDelete(callbackName, testResult) {
                return function (obj) {
                    test.addLog('In delete ' + callbackName + ' callback, arg = ', obj);
                    done(testResult);
                }
            }

            function afterInsert(place) {
                // insert will either have failed (item already exists) or succeeded (item didn't exist);
                // in either case we'll be able to delete it.
                return function (obj) {
                    test.addLog(place + ': ', obj);
                    var toUpdate = { id: id, name: 'test2' };
                    table.update({ id: id, name: 'test2' }).done(function (updated) {
                        test.addLog('Updated: ', updated);
                        table.lookup(id).done(function (item) {
                            var errors = [];
                            var testResult = true;
                            if (zumo.util.compare(toUpdate, item, errors)) {
                                test.addLog('Item was updated successfully');
                            } else {
                                testResult = false;
                                test.addLog('Error during update:');
                                errors.forEach(function (error) {
                                    test.addLog(error);
                                });
                            }
                            table.del({ id: id }).done(afterDelete('success', testResult), afterDelete('error', testResult));
                        }, function (err) {
                            test.addLog('Error calling lookup: ', err);
                            done(false);
                        });
                    }, function (err) {
                        test.addLog('Error calling update: ', err);
                        done(false);
                    });
                }
            }

            table.insert({ id: id, name: 'test' }).done(afterInsert('Inserted'), afterInsert('Insert error (likely expected)'));
        }, zumo.runtimeFeatureNames.STRING_ID_TABLES));
    });

    function createDeleteTest(testName, actionAfterInsert) {
        // actionAfterInsert: function(test, done, table, insertedItemId)
        return new zumo.Test(testName, function (test, done) {
            var client = zumo.getClient();
            var table = client.getTable(tableName);
            table.insert({ string1: 'test' }).done(function (inserted) {
                var id = inserted.id;
                test.addLog('Inserted item to be deleted, with id = ' + id);
                actionAfterInsert(test, done, table, id);
            }, function (insertError) {
                test.addLog('Error inserting item: ' + JSON.stringify(insertError));
                zumo.util.traceResponse(test, insertError.request);
                done(false);
            });
        }, zumo.runtimeFeatureNames.INT_ID_TABLES);
    }

    function createUpdateTest(testName, actionAfterInsert) {
        // actionAfterInsert: function(test, done, table, insertedItem)
        return new zumo.Test(testName, function (test, done) {
            var client = zumo.getClient();
            var table = client.getTable(tableName);
            table.insert({ string1: 'test', number: 123 }).done(function (inserted) {
                var id = inserted.id;
                test.addLog('Inserted item to be updated, with id = ' + id);
                actionAfterInsert(test, done, table, inserted);
            }, function (insertError) {
                test.addLog('Error inserting item: ' + JSON.stringify(insertError));
                zumo.util.traceResponse(test, insertError.request);
                done(false);
            });
        }, zumo.runtimeFeatureNames.INT_ID_TABLES);
    }

    return {
        name: 'Update / Delete',
        tests: tests
    };
}

zumo.tests.updateDelete = defineUpdateDeleteTestsNamespace();
