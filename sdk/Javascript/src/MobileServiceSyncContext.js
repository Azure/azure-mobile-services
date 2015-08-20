// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="Generated\MobileServices.DevIntellisense.js" />

var Validate = require('Validate');
var Platform = require('Platform');

function MobileServiceSyncContext(client) {
    /// <summary>
    /// Creates an instance of the MobileServiceSyncContext class
    /// </summary>
    /// <param name="client" type="MobileServiceClient" mayBeNull="false">
    /// The MobileServiceClient used to make requests.
    /// </param>

    Validate.isObject(client, 'client');
    Validate.notNull(client, 'client');

    var _store;

    this.initialize = function (store) {
        /// <summary>
        /// Initializes the sync context with an instance of the store to be used
        /// </summary>

        Validate.isObject(store);
        Validate.notNull(store);

        _store = store;
    };

    this.insert = function (tableName, instance) {
        /// <summary>
        /// Insert a new object into the given sync table.
        /// </summary>
        /// <param name="tableName" type="string">
        /// Name of the sync table in which the item is to be inserted
        /// </param>
        /// <param name="instance" type="Object">
        /// The object to insert into the table.
        /// </param>

        // TODO(shrirs): Check if the record already exists

        return _store.upsert(tableName, instance).then(function () {
            // TODO(shrirs): Add operation to the operations table
        }, function(error) {
            throw error;
        });
    };

    this.lookup = function (tableName, id) {
        /// <summary>
        /// Gets an item from the given sync table.
        /// </summary>
        /// <param name="tableName" type="string">
        /// Name of the sync table to be used for performing the item lookup
        /// </param>
        /// <param name="id" type="string">
        /// The id of the object to get from the table.
        /// </param>

        return _store.lookup(tableName, id);
    };
}

exports.MobileServiceSyncContext = MobileServiceSyncContext;
