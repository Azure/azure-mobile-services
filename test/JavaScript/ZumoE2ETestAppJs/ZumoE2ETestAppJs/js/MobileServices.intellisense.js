// ----------------------------------------------------------------------------
//! Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

// Declare JSHint globals
/*global WindowsAzure:false, intellisense:false */

intellisense.annotate(WindowsAzure.MobileServiceClient.prototype, {
    withFilter: function () {
        /// <signature>
        /// <summary>
        /// Create a new MobileServiceClient with a filter inserted into the http 
        /// pipeline to process all of its HTTP requests and responses.
        /// </summary>
        /// <param name="serviceFilter" type="Function">
        /// The filter to use on the service.  The signature of a serviceFilter is
        /// function(request, next, callback) where next := function(request, callback)
        /// and callback := function(error, response).
        /// </param>
        /// <returns type="WindowsAzure.MobileServiceClient">
        /// A new MobileServiceClient whose HTTP requests and responses will be
        /// filtered as desired.
        /// </returns>
        /// </signature>
    },
    login: function () {
        /// <signature>
        /// <summary>
        /// Logs a user into a mobile service by using the specified identity provider.
        /// </summary>
        /// <param name="provider" type="String" mayBeNull="true">
        /// The name of the identity provider, which instructs Mobile Services which provider to use for authentication. 
        /// The following values are supported: 'facebook', 'twitter', 'google', 'windowsazureactivedirectory' (can also use 'aad')
        /// or 'microsoftaccount'. If no provider is specified, the 'token' parameter
        /// is considered a Microsoft Account authentication token. If a provider is specified, 
        /// the 'token' parameter is considered a provider-specific authentication token.
        /// </param>
        /// <param name="token" type="Object" mayBeNull="true">
        /// Optional JSON representation of an authentication token, which can be supplied when the client has already
        /// obtained a token from the identity provider.
        /// </param>
        /// <param name="useSingleSignOn" type="Boolean" mayBeNull="true">
        /// Only applies to Windows 8 clients.  Will be ignored on other platforms.
        /// Indicates if single sign-on should be used. Single sign-on requires that the 
        /// application's Package SID be registered with the Windows Azure Mobile Service, 
        /// but it provides a better experience as HTTP cookies are supported so that users 
        /// do not have to login in everytime the application is launched.
        /// </param>
        /// <returns type="WinJS.Promise">
        /// A winJS.Promise object
        /// </returns>
        ///</signature>
    },

    logout: function () {
        /// <signature>
        /// <summary>
        /// Logs a user out of a Mobile Services application.
        /// </summary>
        /// </signature>
    }
});

intellisense.annotate(WindowsAzure, {
    MobileServiceClient: function () {
        ///<signature>
        /// <summary>
        /// Creates a new instance of the MobileServiceClient.
        /// </summary>
        /// <param name="applicationUrl" type="string" mayBeNull="false">
        /// The URL of the mobile service..
        /// </param>
        /// <param name="applicationKey" type="string" mayBeNull="false">
        /// The application key of the mobile service..
        /// </param>
        ///</signature>		
    },
    MobileServiceTable: function () {
        /// <signature>
        /// <summary>
        /// Represents a table in a mobile service to support insert, update, delete, and query operations.
        /// </summary>
        /// </signature>
    }
});

WindowsAzure.MobileServiceClient = (function () {
    var _client = WindowsAzure.MobileServiceClient;
    var wrapper = function () {
        var instance = new _client();
        intellisense.annotate(instance, {
            /// <field name="applicationKey" type="string">The application key</field>
            applicationKey: String,
            /// <field name="currentUser" type="UserObject">The current user</field>
            currentUser: undefined,
            /// <field name="applicationUrl" type="string">The application Url</field>
            applicationUrl: String,
            getTable: function () {
                /// <signature>
                /// <summary>
                /// Gets a reference to a table and its data operations.
                /// </summary>
                /// <param name="tableName" type="string">The name of the table.</param>
                /// <returns type="WindowsAzure.MobileServiceTable">A reference to the table.</returns>
                /// </signature>
            },
            invokeApi: function () {
                /// <signature>
                /// <summary>
                /// Invokes the specified custom api and returns a response object.
                /// </summary>
                /// <param name="apiName">
                /// The custom api to invoke.
                /// </param>
                /// <param name="options" mayBeNull="true">
                /// Contains additional parameter information, valid values are:
                /// body: The body of the HTTP request.
                /// method: The HTTP method to use in the request, with the default being POST,
                /// parameters: Any additional query string parameters, 
                /// headers: HTTP request headers, specified as an object.
                /// </param>
                /// </signature>
            }
        });

        instance.getTable = (function () {
            var _table = instance.getTable;
            var wrapper2 = function () {
                var instance2 = new _table();
                intellisense.annotate(instance2, {
                    del: function () {
                        /// <signature>
                        /// <summary>
                        /// Deletes an object from a given table.
                        /// </summary>
                        /// <param name="instance" type="Object">The instance to delete from the table.</param>
                        /// <returns type="WinJS.Promise">A WinJS.Promise</returns>
                        /// </signature>
                    },
                    getMobileServiceClient: function () {
                        /// <signature>
                        /// <summary>
                        /// The client associated with this table.
                        /// </summary>
                        /// <returns type="WindowsAzure.MobileServiceClient">A MobileServiceClient</returns>
                        /// </signature>
                    },
                    getTableName: function () {
                        /// <signature>
                        /// <summary>
                        /// The name of the table.
                        /// </summary>
                        /// <returns type="string"></returns>
                        /// </signature>
                    },
                    includeTotalCount: function () {
                        /// <signature>
                        /// <summary>
                        /// Indicate that the query should include the total count for all the records returned
                        /// ignoring any take paging limit clause specified.
                        /// </summary>
                        /// <returns type="Query">A query that can be further composed.</returns>
                        /// </signature>
                    },
                    insert: function () {
                        /// <signature>
                        /// <summary>
                        /// Inserts data from the supplied JSON object into the table.
                        /// </summary>
                        /// <param name="instance" type="Object" mayBeNull="false">
                        /// The instance to insert into the table.  It will be updated upon completion.
                        /// </param>
                        /// <returns type="WinJS.Promise">A WinJS.Promise</returns>
                        /// </signature>
                    },
                    orderBy: function () {
                        /// <signature>
                        /// <summary>
                        /// Sorts a query against the table by the selected columns, in ascending order.
                        /// </summary>
                        /// <param name="col" type="string" mayBeNull="false" parameterArray="true">
                        /// An array of the names of the columns to use for ordering
                        /// </param>
                        /// <returns type="Query">A query that can be further composed.</returns>
                        /// </signature>
                    },
                    orderByDescending: function () {
                        /// <signature>
                        /// <summary>
                        /// Sorts a query against the table by the selected columns, in descending order.
                        /// </summary>
                        /// <param name="col" type="string" mayBeNull="false" parameterArray="true">
                        /// An array of the names of the columns to use for ordering
                        /// </param>
                        /// <returns type="Query">A query that can be further composed.</returns>
                        /// </signature>
                    },
                    read: function () {
                        /// <signature>
                        /// <summary>
                        /// Executes a query against the table.
                        /// </summary>
                        /// <param name="query" type="Query" mayBeNull="true">
                        /// The query to execute. When null or undefined, all rows are returned.
                        /// </param>
                        /// <param name="parameters" type="Object" mayBeNull="true">
                        /// An object of user-defined parameters and values to include in the request URI query string.
                        /// </param>
                        /// <returns type="WinJS.Promise">A WinJS.Promise</returns>
                        /// </signature>
                    },
                    lookup: function () {
                        /// <signature>
                        /// <summary>
                        /// Gets an instance from a given table.
                        /// </summary>
                        /// <param name="id" type="Number" integer="true">
                        /// The id of the instance to get from the table.
                        /// </param>
                        /// <param name="parameters" type="Object" mayBeNull="true">
                        /// An object of user-defined parameters and values to include in the request URI query string.
                        /// </param>
                        /// <returns type="WinJS.Promise">A WinJS.Promise</returns>
                        /// </signature>
                    },
                    refresh: function () {
                        /// <signature>
                        /// <summary>
                        /// Refresh the current instance with the latest values from the table.
                        /// </summary>
                        /// <param name="instance" type="Object">
                        /// The instance to refresh.
                        /// </param>
                        /// <param name="parameters" type="Object" mayBeNull="true">
                        /// An object of user-defined parameters and values to include in the request URI query string.
                        /// </param>
                        /// <returns type="WinJS.Promise">A WinJS.Promise</returns>
                        /// </signature>
                    },
                    select: function () {
                        /// <signature>
                        /// <summary>
                        /// Applies the specific column projection to the query against the table.
                        /// </summary>
                        /// <param name="projection" type="function" mayBeNull="false">
                        /// Function that defines the projection.
                        /// </param>
                        /// <returns type="Query">A query that can be further composed.</returns>
                        /// </signature>
                    },
                    skip: function () {
                        /// <signature>
                        /// <summary>
                        /// Skips the specified number of rows in the query.
                        /// </summary>
                        /// <param name="count" type="Number" mayBeNull="false">
                        /// The number of rows to skip when returning the result.
                        /// </param>
                        /// <returns type="Query">A query that can be further composed.</returns>
                        /// </signature>
                    },
                    take: function () {
                        /// <signature>
                        /// <summary>
                        /// Returns the specified number of rows in the query.
                        /// </summary>
                        /// <param name="count" type="Number" mayBeNull="false">
                        /// The number of rows in the query to return.
                        /// </param>
                        /// <returns type="Query">A query that can be further composed.</returns>
                        /// </signature>
                    },
                    update: function () {
                        /// <signature>
                        /// <summary>						
                        /// Updates an object in a given table.
                        /// </summary>
                        /// <param name="instance" type="Object" mayBeNull="false"> 
                        /// The instance to update in the table, as a JSON object.
                        /// </param>
                        /// <returns type="WinJS.Promise">A WinJS.Promise</returns>
                        /// </signature>
                    },
                    where: function () {
                        /// <signature>
                        /// <summary>
                        /// Applies a row filtering predicate to the query against the table.
                        /// </summary>
                        /// <param name="object" type="Object" mayBeNull="true">
                        /// JSON object that defines the row filter.
                        /// </param>
                        /// <returns type="Query">A query that can be further composed.</returns>
                        /// </signature>
                    }
                });
                return instance2;
            };
            intellisense.redirectDefinition(wrapper2, _table);
            return wrapper2;
        })();

        return instance;
    };

    intellisense.redirectDefinition(wrapper, _client);
    return wrapper;
})();