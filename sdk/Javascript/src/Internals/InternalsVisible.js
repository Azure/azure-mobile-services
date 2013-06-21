// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

// Walk all of the registered modules and make all of their exports publicly
// available for unit testing.  This relies on both the global and $__modules__
// free variables exposed via our module system.


// Cache all of the module definition functions before any of the modules have
// been require-d.  This allows us to reset the modules by reevaluating all of
// the definition functions.
var moduleCache = {};
var moduleName = null;
for (moduleName in $__modules__) {
    moduleCache[moduleName] = $__modules__[moduleName];
}

// Expose all of the exports for each module
exposeModules();

function exposeModules() {
    /// <summary>
    /// Expose all of the exports for a module as a global member so they can
    /// be easily accessed for testing.
    /// <summary>
    /// <remarks>
    /// Note that all modules will be require-d to gain access to their
    /// exported members.
    /// </remarks>

    var moduleName = null;
    for (moduleName in $__modules__) {
        // We need to require the module which will force all of its exports to
        // be defined (or do nothing if they've already been require-d).
        require(moduleName);

        // Declare a new global variable with the module's names that will
        // contain all of its exports.
        global[moduleName] = $__modules__[moduleName];
    }
}

global.resetModules = function () {
    /// <summary>
    /// Reset the modules by reevaluating the functions that provide their
    /// exports.
    /// </summary>

    // Reset $__modules__ to contain the original functions
    var moduleName = null;
    for (moduleName in $__modules__) {
        $__modules__[moduleName] = moduleCache[moduleName];
    }

    // Re-require all of the modules which will reevaluate their functions
    exposeModules();
};