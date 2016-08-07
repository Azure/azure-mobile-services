// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="Generated\MobileServices.DevIntellisense.js" />

var Validate = require('Validate');
var Platform = require('Platform');

function MobileServiceMemoryStore() {
    /// <summary>
    /// Initializes a new instance of the MobileServiceMemoryStore class.
    /// </summary>

    var idProperty = "id";
    var tables = {};

    this.insert = Platform.async(function (tableName, instance, callback) {
        /// <summary>
        /// Insert a new object into the given sync table.
        /// </summary>
        /// <param name="tableName" type="string">
        /// Name of the sync table in which the item is to be inserted
        /// </param>
        /// <param name="instance" type="Object">
        /// The object to insert into the table.
        /// </param>
        /// <param name="callback" type="Function">
        /// The callback to invoke when the insert operation is complete, either successfully or unsuccessfully
        /// </param>

        // Platform.async will invoke this method by passing a callback object as the last argument
        // Make sure that the callback object passed is same as the callback argument to avoid not fulfilling the promise ever.
        Validate.length(arguments, 3, 'arguments');

        Validate.isString(tableName, 'tableName');
        Validate.notNullOrEmpty(tableName, 'tableName');

        Validate.isObject(instance, 'instance');
        Validate.notNull(instance, 'instance');

        Validate.notNull(callback, 'callback');

        // To keep things simple, just verify that the table has an "id" property
        Validate.notNull(instance[idProperty]);

        var table = tables[tableName] = tables[tableName] || {};

        var instanceId = instance[idProperty];
        Validate.notNull(instanceId);

        if (table[instanceId] !== undefined) {
            throw "Record with specified ID already exists in the table";
        }

        // Make a deep copy of the object before inserting it. 
        // We don't want future changes to the object to directly update our table data.
        var newObject = JSON.parse(JSON.stringify(instance));
        table[instanceId] = newObject;

        // notify completion
        callback();
    });

    this.lookup = Platform.async(function (tableName, id, callback) {
        /// <summary>
        /// Gets an item from the given sync table.
        /// </summary>
        /// <param name="tableName" type="string">
        /// Name of the sync table to be used for performing the item lookup
        /// </param>
        /// <param name="id" type="string">
        /// The id of the object to get from the table.
        /// </param>
        /// <param name="callback" type="Function">
        /// The callback to invoke when the insert operation is complete, either successfully or unsuccessfully
        /// </param>

        // Platform.async will invoke this method by passing a callback object as the last argument
        // Make sure that the callback object passed is same as the callback argument to avoid not fulfilling the promise ever.
        Validate.length(arguments, 3, 'arguments');

        Validate.isString(tableName, 'tableName');
        Validate.notNullOrEmpty(tableName, 'tableName');

        Validate.isString(id, 'id');
        Validate.notNullOrEmpty(id, 'id');

        Validate.notNull(callback, 'callback');

        var table = tables[tableName];

        if (table === undefined) {
            throw "Undefined table";
        }

        // notify completion
        callback(null, table[id]);
    });
}

exports.MobileServiceMemoryStore = MobileServiceMemoryStore;
