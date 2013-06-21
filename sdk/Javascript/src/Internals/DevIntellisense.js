// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

// The following declarations will let Visual Studio provide cross-module
// Intellisense by defining free variables provided by the module system as
// globals that it can find.  Visual Studio's JavaScript Intellisense engine
// will actually evaluate code on a background thread, so when your module
// calls require('foo'), VS actually runs the real require code (since we make
// it visible here) and will provide Intellisense for all of foo's exports.
global.exports = {};
global.require = require;
