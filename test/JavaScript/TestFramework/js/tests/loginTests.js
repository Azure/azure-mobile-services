// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="../../../ZumoE2ETestAppJs/ZumoE2ETestAppJs/js/MobileServices.js" />
/// <reference path="/LiveSDKHTML/js/wl.js" />
/// <reference path="../testFramework.js" />

function defineLoginTestsNamespace() {
    var tests = [];
    var i;
    var TABLE_PERMISSION_PUBLIC = 1;
    var TABLE_PERMISSION_APPLICATION = 2;
    var TABLE_PERMISSION_USER = 3;
    var TABLE_PERMISSION_ADMIN = 4;
    var TABLE_NAME_PUBLIC = 'public';
    var TABLE_NAME_APPLICATION = 'application';
    var TABLE_NAME_AUTHENTICATED = 'authenticated';
    var TABLE_NAME_ADMIN = 'admin';

    var tables = [
        { name: TABLE_NAME_PUBLIC, permission: TABLE_PERMISSION_PUBLIC },
        { name: TABLE_NAME_APPLICATION, permission: TABLE_PERMISSION_APPLICATION },
        { name: TABLE_NAME_AUTHENTICATED, permission: TABLE_PERMISSION_USER },
        { name: TABLE_NAME_ADMIN, permission: TABLE_PERMISSION_ADMIN }];

    var supportRecycledToken = {
        facebook: true,
        google: false, // Known bug - Drop login via Google token until Google client flow is reintroduced
        twitter: false,
        microsoftaccount: false
    };

    tests.push(createLogoutTest());

    var index, table;
    for (index = 0; index < tables.length; index++) {
        table = tables[index];
        tests.push(createCRUDTest(table.name, null, table.permission, false));
    }

    var indexOfTestsWithAuthentication = tests.length;

    var lastUserIdentityObject = null;

    var providers = ['facebook', 'google', 'twitter', 'microsoftaccount'];
    for (i = 0; i < providers.length; i++) {
        var provider = providers[i];
        tests.push(createLogoutTest());
        tests.push(createLoginTest(provider));

        for (index = 0; index < tables.length; index++) {
            table = tables[index];
            if (table.permission !== TABLE_PERMISSION_PUBLIC) {
                tests.push(createCRUDTest(table.name, provider, table.permission, true));
            }
        }

        if (supportRecycledToken[provider]) {
            tests.push(createLogoutTest());
            tests.push(createClientSideLoginTest(provider));
            tests.push(createCRUDTest(TABLE_NAME_AUTHENTICATED, provider, TABLE_PERMISSION_USER, true));
        }
    }

    if (!testPlatform.IsHTMLApplication) {
        //In Browser, default is single signon and LIVE SDK is not supported
        tests.push(createLogoutTest());
        tests.push(createLiveSDKLoginTest());
        tests.push(createCRUDTest(TABLE_NAME_AUTHENTICATED, 'microsoftaccount', TABLE_PERMISSION_USER, true));

        providers.forEach(function (provider) {
            if (provider === 'microsoftaccount') {
                // Known issue - SSO for MS account does not work in application which also uses the Live SDK
            } else {
                tests.push(createLogoutTest());
                tests.push(createLoginTest(provider, true));
                tests.push(createCRUDTest(TABLE_NAME_AUTHENTICATED, provider, TABLE_PERMISSION_USER, true));
            }
        });
    }

    for (var i = indexOfTestsWithAuthentication; i < tests.length; i++) {
        tests[i].canRunUnattended = false;
    }

    function createLiveSDKLoginTest() {
        var liveSDKInitialized = false;
        return new zumo.Test('Login via token with the Live SDK', function (test, done) {
            /// <param name="test" type="zumo.Test">The test associated with this execution.</param>
            var client = zumo.getClient();
            if (!liveSDKInitialized) {
                WL.init({ redirect_uri: client.applicationUrl });
                liveSDKInitialized = true;
                test.addLog('Initialized the WL object');
            }
            WL.login({ scope: 'wl.basic' }).then(function (wlLoginResult) {
                test.addLog('Logged in via Live SDK: ', wlLoginResult);
                WL.api({ path: 'me', method: 'GET' }).then(function (wlMeResult) {
                    test.addLog('My information: ', wlMeResult);
                    var token = { authenticationToken: wlLoginResult.session.authentication_token };
                    client.login('microsoftaccount', token).done(function (user) {
                        test.addLog('Logged in as ', user);
                        done(true);
                    }, function (err) {
                        test.addLog('Error logging into the mobile service: ', err);
                        done(false);
                    });
                }, function (err) {
                    test.addLog('Error calling WL.api: ', err);
                    done(false);
                });
            }, function (err) {
                test.addLog('Error logging in via Live SDK: ', err);
                done(false);
            });
        });
    }

    function createClientSideLoginTest(provider) {
        /// <param name="provider" type="String" mayBeNull="true">The name of the authentication provider for
        ///            the client. Currently only 'facebook' and 'google' are supported for this test.</param>
        return new zumo.Test('Login via token for ' + provider, function (test, done) {
            /// <param name="test" type="zumo.Test">The test associated with this execution.</param>
            var client = zumo.getClient();
            var lastIdentity = lastUserIdentityObject;
            if (!lastIdentity) {
                test.addLog('Last identity object is null. Cannot run this test.');
                done(false);
            } else {
                var token = {};
                if (provider === 'facebook' || provider === 'google') {
                    token.access_token = lastIdentity[provider].accessToken;
                    client.login(provider, token).done(function (user) {
                        test.addLog('Logged in as ', user);
                        done(true);
                    }, function (err) {
                        test.addLog('Error on login: ', err);
                        done(false);
                    });
                } else {
                    test.addLog('Client-side login for ' + provider + ' is not implemented or not supported.');
                    done(false);
                }
            }
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
            var currentUser = client.currentUser;
            var item = { name: 'hello' };
            var insertedItem;

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
                            var isInternetExplorer10 = testPlatform.IsHTMLApplication && window.ActiveXObject && window.navigator.userAgent.toLowerCase().match(/msie ([\d.]+)/)[1] == "10.0";
                            // IE 10 has a bug in which it doesn't set the status code correctly - https://connect.microsoft.com/IE/feedback/details/785990
                            // so we cannot validate the status code if this is the case.
                            if (isInternetExplorer10) {
                                result = true;
                            } else {
                                if (xhr.status == 401) {
                                    test.addLog('Got expected response code (401) for ', operation);
                                    result = true;
                                } else {
                                    zumo.util.traceResponse(test, xhr);
                                    test.addLog('Error, incorrect response.');
                                }
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
                    //table.del({ id: insertedItem.id || 1 }).done(function () { deleteCallback(); }, function (err) { deleteCallback(err); });
                    table.del({ id: insertedItem.id }).done(function () { deleteCallback(); }, function (err) { deleteCallback(err); });
                }
            }

            // called by insertCallback
            function updateCallback(error) {
                if (validateCRUDResult('update', error)) {
                    item.id = insertedItem.id || 1;
                    table.where({ id: item.id }).read().done(function (items) {
                        test.addLog('Read items: ', items);
                        if (items.length !== 1) {
                            test.addLog('Error, query should have returned exactly one item');
                            done(false);
                        } else {
                            if (items[0].Identities) {
                                lastUserIdentityObject = JSON.parse(items[0].Identities);
                                test.addLog('Identities object: ', lastUserIdentityObject);
                            }
                            readCallback();
                        }
                    }, function (err) {
                        readCallback(err);
                    });
                }
            }

            // called by the callback for insert.
            function insertCallback(error) {
                if (validateCRUDResult('insert', error)) {
                    if (tablePermission === TABLE_PERMISSION_PUBLIC) {
                        // No need for app key anymore
                        client = new WindowsAzure.MobileServiceClient(client.applicationUrl);
                        table = client.getTable(tableName);
                    }
                    item.id = insertedItem.id || 1;
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
                insertedItem = newItem;
                test.addLog('Inserted item: ', newItem);
                if (tablePermission === TABLE_PERMISSION_USER) {
                    var currentUser = client.currentUser.userId;
                    if (currentUser === newItem.userId) {
                        test.addLog('User id correctly added by the server script');
                    } else {
                        test.addLog('Error, user id not set by the server script');
                        done(false);
                        return;
                    }
                }
                insertCallback();
            }, function (err) {
                insertedItem = item;
                insertedItem.id = item.id || 1;
                insertCallback(err);
            });
        });
    }

    function createLoginTest(provider, useSingleSignOn) {
        /// <param name="provider" type="String">The authentication provider to use.</param>
        /// <param name="useSingleSignOn" type="Boolean">Whether to use the single sign-on parameter for login.</param>
        /// <return type="zumo.Test" />
        return new zumo.Test('Login with ' + provider + (useSingleSignOn ? ' (using single sign-on)' : ''), function (test, done) {
            /// <param name="test" type="zumo.Test">The test associated with this execution.</param>
            var client = zumo.getClient();
            var successFunction = function (user) {
                test.addLog('Logged in: ', user);
                done(true);
            };
            var errorFunction = function (err) {
                test.addLog('Error during login: ', err);
                done(false);
            };
            if (useSingleSignOn) {
                client.login(provider, true).done(successFunction, errorFunction);
            } else {
                client.login(provider).done(successFunction, errorFunction);
            }
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
