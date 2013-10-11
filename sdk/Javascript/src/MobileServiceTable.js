// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="Generated\MobileServices.DevIntellisense.js" />

var _ = require('Extensions');
var Validate = require('Validate');
var Platform = require('Platform');
var Query = require('Query').Query;

// Name of the reserved Mobile Services ID member.
var idPropertyName = "id";

// The route separator used to denote the table in a uri like
// .../{app}/collections/{coll}.
var tableRouteSeperatorName = "tables";
var idNames = ["ID", "Id", "id", "iD"];

function MobileServiceTable(tableName, client) {
    /// <summary>
    /// Initializes a new instance of the MobileServiceTable class.
    /// </summary>
    /// <param name="tableName" type="String">
    /// Name of the table.
    /// </param>
    /// <param name="client" type="MobileServiceClient" mayBeNull="false">
    /// The MobileServiceClient used to make requests.
    /// </param>

    this.getTableName = function () {
        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <returns type="String">The name of the table.</returns>
        return tableName;
    };

    this.getMobileServiceClient = function () {
        /// <summary>
        /// Gets the MobileServiceClient associated with this table.
        /// </summary>
        /// <returns type="MobileServiceClient">
        /// The MobileServiceClient associated with this table.
        /// </returns>
        return client;
    };
}

// Export the MobileServiceTable class
exports.MobileServiceTable = MobileServiceTable;

// We have an internal _read method using callbacks since it's used by both
// table.read(query) and query.read().
MobileServiceTable.prototype._read = function (query, parameters, callback) {
    /// <summary>
    /// Query a table.
    /// </summary>
    /// <param name="query" type="Object" mayBeNull="true">
    /// The query to execute.  It can be null or undefined to get the entire
    /// collection.
    /// </param>
    /// <param name="parameters" type="Object" mayBeNull="true">
    /// An object of user-defined parameters and values to include in the request URI query string.
    /// </param>
    /// <param name="callback" type="Function">
    /// The callback to invoke when the query is complete.
    /// </param>

    // Account for absent optional arguments
    if (_.isNull(callback))
    {
        if (_.isNull(parameters) && (typeof query === 'function')) {
            callback = query;
            query = null;
            parameters = null;
        } else if (typeof parameters === 'function') {
            callback = parameters;
            parameters = null;
            if (!_.isNull(query) && _.isObject(query)) {
                // This 'query' argument could be either the query or the user-defined 
                // parameters object since both are optional.  A query is either (a) a simple string 
                // or (b) an Object with an toOData member. A user-defined parameters object is just 
                // an Object.  We need to detect which of these has been passed in here.
                if (!_.isString(query) && _.isNull(query.toOData)) {
                    parameters = query;
                    query = null;
                }
            }
        }
    }

    // Validate the arguments
    if (query && _.isString(query)) {
        Validate.notNullOrEmpty(query, 'query');
    }
    if (!_.isNull(parameters)) {
        Validate.isValidParametersObject(parameters, 'parameters');
    }
    Validate.notNull(callback, 'callback');

    // Get the query string
    var tableName = this.getTableName();
    var queryString = null;
    var projection = null;
    if (_.isString(query)) {
        queryString = query;
    } else if (_.isObject(query) && !_.isNull(query.toOData)) {
        if (query.getComponents) {
            var components = query.getComponents();
            projection = components.projection;
            if (components.table) {
                // If the query has a table name, make sure it's compatible with
                // the table executing the query
                
                if (tableName !== components.table) {
                    var message = _.format(Platform.getResourceString("MobileServiceTable_ReadMismatchedQueryTables"), tableName, components.table);
                    callback(_.createError(message), null);
                    return;
                }

                // The oDataQuery will include the table name; we need to remove
                // because the url fragment already includes the table name.
                var oDataQuery = query.toOData();
                queryString = oDataQuery.replace(new RegExp('^/' + components.table), '');
            }
        }
    }

    // Add any user-defined query string parameters
    if (!_.isNull(parameters)) {
        var userDefinedQueryString = _.url.getQueryString(parameters);
        if (!_.isNullOrEmpty(queryString)) {
            queryString += '&' + userDefinedQueryString;
        }
        else {
            queryString = userDefinedQueryString;
        }
    }
    
    // Construct the URL
    var urlFragment = _.url.combinePathSegments(tableRouteSeperatorName, tableName);
    if (!_.isNull(queryString)) {
        urlFragment = _.url.combinePathAndQuery(urlFragment, queryString);
    }

    // Make the request
    this.getMobileServiceClient()._request(
        'GET',
        urlFragment,
        null,
        function (error, response) {
            var values = null;
            if (_.isNull(error)) {
                // Parse the response
                values = _.fromJson(response.responseText);

                // If the values include the total count, we'll attach that
                // directly to the array
                if (values &&
                    !Array.isArray(values) &&
                    typeof values.count !== 'undefined' &&
                    typeof values.results !== 'undefined') {
                    // Create a new total count property on the values array
                    values.results.totalCount = values.count;
                    values = values.results;
                }

                // If we have a projection function, apply it to each item
                // in the collection
                if (projection !== null) {
                    var i = 0;
                    for (i = 0; i < values.length; i++) {
                        values[i] = projection.call(values[i]);
                    }
                }
            }
            callback(error, values);
        });
};

MobileServiceTable.prototype.read = Platform.async(MobileServiceTable.prototype._read);

MobileServiceTable.prototype.insert = Platform.async(
    function (instance, parameters, callback) {
        /// <summary>
        /// Insert a new object into a table.
        /// </summary>
        /// <param name="instance" type="Object">
        /// The instance to insert into the table.
        /// </param>
        /// <param name="parameters" type="Object" mayBeNull="true">
        /// An object of user-defined parameters and values to include in the request URI query string.
        /// </param>
        /// <param name="callback" type="Function">
        /// The callback to invoke when the insert is complete.
        /// </param>

        // Account for absent optional arguments
        if (_.isNull(callback) && (typeof parameters === 'function')) {
            callback = parameters;
            parameters = null;
        }

        // Validate the arguments
        Validate.notNull(instance, 'instance');
        if (!_.isNull(parameters)) {
            Validate.isValidParametersObject(parameters);
        }
        Validate.notNull(callback, 'callback');

        // Integer Ids can not have any Id set
        for (var i in idNames) {
            var id = instance[idNames[i]];

            if (!_.isNullOrZero(id)) {
                if (_.isString(id)) {
                    // String Id's are allowed iif using id
                    if (idNames[i] !== idPropertyName) {
                        throw _.format(
                            Platform.getResourceString("MobileServiceTable_InsertIdAlreadySet"),
                            idPropertyName);
                    } else {
                        Validate.isValidId(id, idPropertyName);
                    }
                } else {
                    throw _.format(
                        Platform.getResourceString("MobileServiceTable_InsertIdAlreadySet"),
                        idPropertyName);
                }
            }
        }

        // Construct the URL
        var urlFragment = _.url.combinePathSegments(tableRouteSeperatorName, this.getTableName());
        if (!_.isNull(parameters)) {
            var queryString = _.url.getQueryString(parameters);
            urlFragment = _.url.combinePathAndQuery(urlFragment, queryString);
        }

        // Make the request
        this.getMobileServiceClient()._request(
            'POST',
            urlFragment,
            instance,
            function (error, response) {
                if (!_.isNull(error)) {
                    callback(error, null);
                } else {
                    var result = _.fromJson(response.responseText);

                    result = Platform.allowPlatformToMutateOriginal(instance, result);
                    callback(null, result);
                }
            });
    });

MobileServiceTable.prototype.update = Platform.async(
    function (instance, parameters, callback) {
        /// <summary>
        /// Update an object in a given table.
        /// </summary>
        /// <param name="instance" type="Object">
        /// The instance to update in the table.
        /// </param>
        /// <param name="parameters" type="Object" mayBeNull="true">
        /// An object of user-defined parameters and values to include in the request URI query string.
        /// </param>
        /// <param name="callback" type="Function">
        /// The callback to invoke when the update is complete.
        /// </param>

        // Account for absent optional arguments
        if (_.isNull(callback) && (typeof parameters === 'function')) {
            callback = parameters;
            parameters = null;
        }

        // Validate the arguments
        Validate.notNull(instance, 'instance');
        Validate.isValidId(instance[idPropertyName], 'instance.' + idPropertyName);
        if (!_.isNull(parameters)) {
            Validate.isValidParametersObject(parameters, 'parameters');
        }
        Validate.notNull(callback, 'callback');
    
        // Construct the URL
        var urlFragment =  _.url.combinePathSegments(
                tableRouteSeperatorName,
                this.getTableName(),
                encodeURIComponent(instance[idPropertyName].toString()));
        if (!_.isNull(parameters)) {
            var queryString = _.url.getQueryString(parameters);
            urlFragment = _.url.combinePathAndQuery(urlFragment, queryString);
        }

        // Make the request
        this.getMobileServiceClient()._request(
            'PATCH',
            urlFragment,
            instance,
            function (error, response) {
                if (!_.isNull(error)) {
                    callback(error, null);
                } else {
                    var result = _.fromJson(response.responseText);                    
                    result = Platform.allowPlatformToMutateOriginal(instance, result);
                    callback(null, result);
                }
            });
    });

MobileServiceTable.prototype.refresh = Platform.async(
    function (instance, parameters, callback) {
        /// <summary>
        ///  Refresh the current instance with the latest values from the
        ///  table.
        /// </summary>
        /// <param name="instance" type="Object">
        /// The instance to refresh.
        /// </param>
        /// <param name="parameters" type="Object" mayBeNull="true">
        /// An object of user-defined parameters and values to include in the request URI query string.
        /// </param>
        /// <param name="callback" type="Function">
        /// The callback to invoke when the refresh is complete.
        /// </param>

        // Account for absent optional arguments
        if (_.isNull(callback) && (typeof parameters === 'function')) {
            callback = parameters;
            parameters = null;
        }

        // Validate the arguments
        Validate.notNull(instance, 'instance');
        if (!_.isValidId(instance[idPropertyName], idPropertyName))
        {
            if (typeof instance[idPropertyName] === 'string' && instance[idPropertyName] !== '') {
                throw _.format(Platform.getResourceString("Validate_InvalidId"), idPropertyName);
            } else {
                callback(null, instance);
            }
            return;
        }

        if (!_.isNull(parameters)) {
            Validate.isValidParametersObject(parameters, 'parameters');
        }
        Validate.notNull(callback, 'callback');

        // Construct the URL
        var urlFragment = _.url.combinePathSegments(
                tableRouteSeperatorName,
                this.getTableName());

        if (typeof instance[idPropertyName] === 'string') {
            var id = encodeURIComponent(instance[idPropertyName]).replace(/\'/g, '%27%27');
            urlFragment = _.url.combinePathAndQuery(urlFragment, "?$filter=id eq '" + id + "'");
        } else {
            urlFragment = _.url.combinePathAndQuery(urlFragment, "?$filter=id eq " + encodeURIComponent(instance[idPropertyName].toString()));
        }

        if (!_.isNull(parameters)) {
            var queryString = _.url.getQueryString(parameters);
            urlFragment = _.url.combinePathAndQuery(urlFragment, queryString);
        }

        // Make the request
        this.getMobileServiceClient()._request(
            'GET',
            urlFragment,
            instance,
            function (error, response) {
                if (!_.isNull(error)) {
                    callback(error, null);
                } else {
                    var result = _.fromJson(response.responseText);
                    if (Array.isArray(result)) {
                        result = result[0]; //get first object from array
                    }

                    if (!result) {
                        var message =_.format(
                            Platform.getResourceString("MobileServiceTable_NotSingleObject"),
                            idPropertyName);
                        callback(_.createError(message), null);
                    }

                    result = Platform.allowPlatformToMutateOriginal(instance, result);
                    callback(null, result);
                }
            });
    });

MobileServiceTable.prototype.lookup = Platform.async(
    function (id, parameters, callback) {
        /// <summary>
        /// Gets an instance from a given table.
        /// </summary>
        /// <param name="id" type="Number" integer="true">
        /// The id of the instance to get from the table.
        /// </param>
        /// <param name="parameters" type="Object" mayBeNull="true">
        /// An object of user-defined parameters and values to include in the request URI query string.
        /// </param>
        /// <param name="callback" type="Function">
        /// The callback to invoke when the lookup is complete.
        /// </param>

        // Account for absent optional arguments
        if (_.isNull(callback) && (typeof parameters === 'function')) {
            callback = parameters;
            parameters = null;
        }

        // Validate the arguments
        Validate.isValidId(id, idPropertyName);
        if (!_.isNull(parameters)) {
            Validate.isValidParametersObject(parameters);
        }
        Validate.notNull(callback, 'callback');

        // Construct the URL
        var urlFragment = _.url.combinePathSegments(
                tableRouteSeperatorName,
                this.getTableName(),
                encodeURIComponent(id.toString()));
        if (!_.isNull(parameters)) {
            var queryString = _.url.getQueryString(parameters);
            urlFragment = _.url.combinePathAndQuery(urlFragment, queryString);
        }

        // Make the request
        this.getMobileServiceClient()._request(
            'GET',
            urlFragment,
            null,
            function (error, response) {
                if (!_.isNull(error)) {
                    callback(error, null);
                } else {
                    var result = _.fromJson(response.responseText);
                    callback(null, result);
                }
            });
    });

MobileServiceTable.prototype.del = Platform.async(
    function (instance, parameters, callback) {
        /// <summary>
        /// Delete an object from a given table.
        /// </summary>
        /// <param name="instance" type="Object">
        /// The instance to delete from the table.
        /// </param>
        /// <param name="parameters" type="Object" mayBeNull="true">
        /// An object of user-defined parameters and values to include in the request URI query string.
        /// </param>
        /// <param name="callback" type="Function">
        /// The callback to invoke when the delete is complete.
        /// </param>

        // Account for absent optional arguments
        if (_.isNull(callback) && (typeof parameters === 'function')) {
            callback = parameters;
            parameters = null;
        }

        // Validate the arguments
        Validate.notNull(instance, 'instance');
        Validate.isValidId(instance[idPropertyName], 'instance.' + idPropertyName);
        if (!_.isNull(parameters)) {
            Validate.isValidParametersObject(parameters);
        }
        Validate.notNull(callback, 'callback');

        // Contruct the URL
        var urlFragment =  _.url.combinePathSegments(
                tableRouteSeperatorName,
                this.getTableName(),
                encodeURIComponent(instance[idPropertyName].toString()));
        if (!_.isNull(parameters)) {
            var queryString = _.url.getQueryString(parameters);
            urlFragment = _.url.combinePathAndQuery(urlFragment, queryString);
        }

        // Make the request
        this.getMobileServiceClient()._request(
            'DELETE',
            urlFragment,
            null,
            function (error, response) {
                callback(error);
            });
    });

// Copy select Query operators to MobileServiceTable so queries can be created
// compactly.  We'll just add them to the MobileServiceTable prototype and then
// forward on directly to a new Query instance.
var queryOperators =
    ['where', 'select', 'orderBy', 'orderByDescending', 'skip', 'take', 'includeTotalCount'];
var copyOperator = function (operator) {
    MobileServiceTable.prototype[operator] = function () {
        /// <summary>
        /// Creates a new query.
        /// </summary>

        // Create a query associated with this table
        var table = this;
        var query = new Query(table.getTableName());

        // Add a .read() method on the query which will execute the query.
        // This method is defined here per query instance because it's
        // implicitly tied to the table.
        query.read = Platform.async(
            function (parameters, callback) {
                /// <summary>
                /// Execute the query.
                /// </summary>                
                table._read(query, parameters, callback);
            });

        // Invoke the query operator on the newly created query
        return query[operator].apply(query, arguments);
    };
};
var i = 0;
for (; i < queryOperators.length; i++) {
    // Avoid unintended closure capture
    copyOperator(queryOperators[i]);
}
