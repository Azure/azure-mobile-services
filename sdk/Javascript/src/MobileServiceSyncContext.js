// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="Generated\MobileServices.DevIntellisense.js" />

var Validate = require('./Utilities/Validate');
var Platform = require('Platforms/Platform');
var _ = require('./Utilities/Extensions');

function MobileServiceSyncContext(client) {
    /// <summary>
    /// Creates an instance of the MobileServiceSyncContext class
    /// </summary>
    /// <param name="client" type="MobileServiceClient" mayBeNull="false">
    /// The MobileServiceClient used to make requests.
    /// </param>

    Validate.notNull(client, 'client');

    var _store;

    this.initialize = function (store) {
        /// <summary>
        /// Initializes the sync context with an instance of the store to be used
        /// </summary>

        Validate.notNull(store);

        _store = store;
    };

    // TODO(shrirs): Add tracking operations to the operations table for insert/update/delete
    this.insert = function (tableName, instance) {
        /// <summary>
        /// Insert a new object into the given sync table.
        /// </summary>
        /// <param name="tableName" type="string">
        /// Name of the sync table in which the object is to be inserted
        /// </param>
        /// <param name="instance" type="Object">
        /// The object to be inserted into the table.
        /// </param>
        /// <returns type="Promise">
        /// A promise that is resolved with the inserted object when the operation is completed successfully.
        /// If the operation fails, the promise is rejected
        /// </returns>

        Validate.isString(tableName, 'tableName');
        Validate.notNullOrEmpty(tableName, 'tableName');

        Validate.notNull(instance, 'instance');
        Validate.notNull(instance.id, 'instance.id'); //TODO(shrirs): instance.id is a valid scenario, handle it

        Validate.notNull(_store, '_store');

        return _store.lookup(tableName, instance.id).then(function(result) {
            if (result !== null) {
                throw "An object with the same ID already exists in the table";
            }

            _store.upsert(tableName, instance);
        }).then(function() {
            return instance;
        });
    };

    this.update = function (tableName, instance) {
        /// <summary>
        /// Update an object in the given sync table.
        /// </summary>
        /// <param name="tableName" type="string">
        /// Name of the sync table in which the object is to be updated
        /// </param>
        /// <param name="instance" type="Object">
        /// The object to be updated
        /// </param>
        /// <returns type="Promise">
        /// A promise that is resolved when the operation is completed successfully.
        /// If the operation fails, the promise is rejected
        /// </returns>

        Validate.isString(tableName, 'tableName');
        Validate.notNullOrEmpty(tableName, 'tableName');

        Validate.notNull(instance, 'instance');
        Validate.notNull(instance.id, 'instance.id');

        Validate.notNull(_store, '_store');

        return _store.upsert(tableName, instance);
    };

    this.lookup = function (tableName, id) {
        /// <summary>
        /// Gets an object from the given sync table.
        /// </summary>
        /// <param name="tableName" type="string">
        /// Name of the sync table to be used for performing the object lookup
        /// </param>
        /// <param name="id" type="string">
        /// The id of the object to get from the table.
        /// </param>
        /// <returns type="Promise">
        /// A promise that is resolved with the looked up object when the operation is completed successfully.
        /// If the operation fails, the promise is rejected
        /// </returns>

        Validate.isString(tableName, 'tableName');
        Validate.notNullOrEmpty(tableName, 'tableName');

        Validate.notNull(id, 'id');

        Validate.notNull(_store, '_store');

        return _store.lookup(tableName, id);
    };

    this.del = function (tableName, instance) {
        /// <summary>
        /// Delete an object from the given sync table
        /// </summary>
        /// <param name="tableName" type="string">
        /// Name of the sync table to delete the object from
        /// </param>
        /// <param name="instance" type="Object">
        /// The object to delete from the sync table.
        /// </param>
        /// <returns type="Promise">
        /// A promise that is resolved when the operation is completed successfully.
        /// If the operation fails, the promise is rejected
        /// </returns>

        Validate.isString(tableName, 'tableName');
        Validate.notNullOrEmpty(tableName, 'tableName');

        Validate.notNull(instance);
        Validate.notNull(instance.id);

        return _store.del(tableName, instance);
    };
}

exports.MobileServiceSyncContext = MobileServiceSyncContext;
