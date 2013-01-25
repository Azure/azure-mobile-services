/// <reference path="/MobileServicesJavaScriptClient/MobileServices.js" />
/// <reference path="../testFramework.js" />

function defineLoginTestsNamespace() {
    var tests = [];
    var i;
    var TABLE_PERMISSION_PUBLIC = 1;
    var TABLE_PERMISSION_APPLICATION = 2;
    var TABLE_PERMISSION_USER = 3;
    var TABLE_PERMISSION_ADMIN = 4;

    var tables = [
        { name: 'w8Application', permission: TABLE_PERMISSION_APPLICATION },
        { name: 'w8Authenticated', permission: TABLE_PERMISSION_USER },
        { name: 'w8Admin', permission: TABLE_PERMISSION_ADMIN }];

    tests.push(createLogoutTest());
    tables.forEach(function (table) {
        tests.push(createCRUDTest(table.name, null, table.permission, false));
    });

    var providers = ['facebook', 'google', 'twitter', 'microsoftaccount'];
    for (i = 0; i < providers.length; i++) {
        var provider = providers[i];
        tests.push(createLogoutTest());
        tests.push(createLoginTest(provider));
        tables.forEach(function (table) {
            tests.push(createCRUDTest(table.name, provider, table.permission, true));
        });
    }

    function createCRUDTest(tableName, provider, tablePermission, userIsAuthenticated) {
        /// <param name="tableName" type="String">The name of the table to attempt the CRUD operations for.</param>
        /// <param name="provider" type="String" mayBeNull="true">The name of the authentication provider for
        ///            the client.</param>
        /// <param name="tablePermission" type="Number" mayBeNull="false">The permission required to access the
        ///            table. One of the constants defined in the scope.</param>
        /// <param name="userIsAuthenticated" type="Boolean" mayBeNull="false">The name of the table to attempt
        ///            the CRUD operations for.</param>
        /// <return type="zumo.Test"/>
        var testName = 'CRUD, ' + (userIsAuthenticated ? ('auth by ' + provider) : 'unauthenticated');
        testName = testName + ', table with ';
        testName = testName + ['public', 'application', 'user', 'admin'][tablePermission - 1];
        testName = testName + ' permission.';
        return new zumo.Test(testName, function (test, done) {
            /// <param name="test" type="zumo.Test">The test associated with this execution.</param>
            var crudShouldWork = tablePermission === TABLE_PERMISSION_PUBLIC ||
                tablePermission === TABLE_PERMISSION_APPLICATION ||
                (tablePermission === TABLE_PERMISSION_USER && userIsAuthenticated);
            var client = zumo.getClient();
            var table = client.getTable(tableName);
            var item = { text: 'hello' };

            var validateCRUDResult = function (operation, error) {
                var result = false;
                if (crudShouldWork) {
                    if (error) {
                        test.addLog(operation + ' should have succeeded, but got error: ', error);
                    } else {
                        test.addLog(operation + ' succeeded as expected.');
                        result = true;
                    }
                } else {
                    if (error) {
                        var xhr = error.request;
                        if (xhr) {
                            if (xhr.status == 401) {
                                test.addLog('Got expected response code (401) for ', operation);
                                result = true;
                            } else {
                                zumo.util.traceResponse(test, xhr);
                                test.addLog('Error, incorrect response.');
                            }
                        } else {
                            test.addLog('Error, error object does not have a \'request\' (for the XMLHttpRequest object) property.');
                        }
                    } else {
                        test.addLog(operation + ' should not have succeeded, but the success callback was called.');
                    }
                }

                if (!result) {
                    done(false);
                }

                return result;
            }

            // The last of the callbacks, which will call 'done(true);' if validation succeeds.
            // called by readCallback
            function deleteCallback(error) {
                if (validateCRUDResult('delete', error)) {
                    test.addLog('Validation succeeded for all operations');
                    done(true);
                }
            }

            // called by updateCallback
            function readCallback(error) {
                if (validateCRUDResult('read', error)) {
                    table.del({ id: item.id || 1 }).done(function () { deleteCallback(); }, function (err) { deleteCallback(err); });
                }
            }

            // called by insertCallback
            function updateCallback(error) {
                if (validateCRUDResult('update', error)) {
                    item.id = item.id || 1;
                    table.where({ id: item.id }).read().done(function (items) {
                        test.addLog('Read items: ', items);
                        readCallback();
                    }, function (err) {
                        readCallback(err);
                    });
                }
            }

            // called by the callback for insert.
            function insertCallback(error) {
                if (validateCRUDResult('insert', error)) {
                    item.id = item.id || 1;
                    item.text = 'world';
                    table.update(item).done(function (newItem) {
                        test.addLog('Updated item: ', newItem);
                        updateCallback();
                    }, function (err) {
                        updateCallback(err);
                    });
                }
            }

            table.insert(item).done(function (newItem) {
                test.addLog('Inserted item: ', newItem);
                insertCallback();
            }, function (err) {
                insertCallback(err);
            });
        });
    }

    function createLoginTest(provider, token, useSingleSignOn) {
        /// <param name="provider" type="String">The authentication provider to use.</param>
        /// <return type="zumo.Test" />
        return new zumo.Test('Login with ' + provider + (useSingleSignOn ? ' (using single sign-on)' : ''), function (test, done) {
            /// <param name="test" type="zumo.Test">The test associated with this execution.</param>
            if (useSingleSignOn || token) {
                test.addLog('Test using token and for single-sign on have yet to be implemented');
                done(false);
                return;
            }

            var client = zumo.getClient();
            client.login(provider).done(function (user) {
                test.addLog('Logged in: ', user);
                done(true);
            }, function (err) {
                test.addLog('Error during login: ', err);
                done(false);
            });
        });
    }
    function createLogoutTest() {
        return new zumo.Test('Log out', function (test, done) {
            var client = zumo.getClient();
            client.logout();
            test.addLog('Logged out');
            done(true);
        });
    }

    return {
        name: 'Login',
        tests: tests
    };
}

zumo.tests.login = defineLoginTestsNamespace();
