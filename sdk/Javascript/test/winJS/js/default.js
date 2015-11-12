// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

(function () {
    "use strict";

    // Handle WebUIApplication.activate instead of WinJS.Application.activated
    // because it gives us access to initialization arguments which are passed
    // in by the test launcher (if invoked via the build, etc.).
    Windows.UI.WebUI.WebUIApplication.addEventListener(
        'activated',
        function (eventObject) {
            // Get initialization arguments and set them on the harness
            if (eventObject.detail && eventObject.detail[0]) {
                var args = eventObject.detail[0].arguments;
                if (args) {
                    var harnessArgs = JSON.parse(args);
                    $harness.settings.testResultsServerUrl = harnessArgs.TestServerUri;
                    $harness.settings.tagExpression = '';
                    $harness.settings.breakOnStart = harnessArgs.BreakOnStart;
                    $harness.settings.custom.MobileServiceRuntimeUrl = harnessArgs.RuntimeUri;
                    $harness.settings.custom.platform = harnessArgs.platform;
                }
            }

            // Start executing the unit tests
            WinJS.UI.processAll();

            // Setup the test run
            $showTestSetupUI();
        });
    
    WinJS.Application.start();
})();
