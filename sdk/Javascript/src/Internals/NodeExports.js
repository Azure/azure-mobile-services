// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

// The server-side node runtime wants to provide the same query syntax and we
// want to reuse as much code as possible.  This will bundle up the entire
// library and add a single node.js export that translates queries into OData.

global.Query = require('Query').Query;
