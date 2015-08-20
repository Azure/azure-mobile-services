// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="Generated\MobileServices.DevIntellisense.js" />

var Validate = require('Validate');
var Platform = require('Platform');

function MobileServiceSyncTable(tableName, client) {
    /// <summary>
    /// Creates an instance of the MobileServiceSyncTable class.
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

    this.insert = function (instance) {
        /// <summary>
        /// Insert a new object into the given sync table.
        /// </summary>
        /// <param name="instance" type="Object">
        /// The object to insert into the table.
        /// </param>

        return client.getSyncContext().insert(tableName, instance);
    };

    this.lookup = function (id) {
        /// <summary>
        /// Gets an item from the given sync table.
        /// </summary>
        /// <param name="id" type="string">
        /// The id of the object to get from the table.
        /// </param>

        return client.getSyncContext().lookup(tableName, id);
    };

}

// Export the MobileServiceSyncTable class
exports.MobileServiceSyncTable = MobileServiceSyncTable;
