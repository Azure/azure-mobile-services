// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="..\generated\Tests.js" />

// Provide an implementation of the ITestResporter interface that creates HTML
// markup in a WinJS application.

// Declare JSHint globals
/*global document:false, window: false, Microsoft:false, $harness:false, $run:false */

// Require the TestFramework module so we can pull in various utilities like
// $fmt to make generating the interface markup easier.
require('TestFramework');

function getResultsElement() {
    return document.getElementById('testResults');
}

function write(markup) {
    /// <summary>
    /// Add some HTML content to the end of interface results.
    /// </summary>
    /// <param name="markup" type="String">HTML content to write.</param>

    var element = getResultsElement();
    if (element) {
        element.innerHTML += markup;
        document.getElementById('footer').scrollIntoView();
    }
}

function tag(element, styleOrContent, content) {
    /// <summary>
    /// Simple helper to generate basic markup for a tag with optional style
    /// settings.
    /// </summary>
    /// <param name="element" type="String">Tag to create.</param>
    /// <param name="styleOrContent" type="String">
    /// Either optional style settings or the element content.
    /// </param>
    /// <param name="content" type="String">The content of the element.</param>
    /// <returns type="String">The generated markup.</returns>

    var className = styleOrContent;
    if (content === null || content === undefined) {
        content = styleOrContent;
        className = null;
    }

    var markup = $fmt('<{0}', element);
    if (className) {
        markup += $fmt(' class="{0}"', className);
    }
    markup += $fmt('>{0}</{1}>', content, element);
    return markup;
}

var reporter = new Microsoft.Azure.Zumo.Win8.Test.WinJS.TestReporter();

reporter.onexecutestartrun = function (args) {
    document.getElementById('currentTestNumber').innerText = args.progress;
    document.getElementById('totalTestNumber').innerText = args.count;
    document.getElementById('failureNumber').innerText = args.failures;
    document.getElementById('progress').value = 1;
    document.getElementById('footer').style.display = 'block';
};

reporter.onexecuteendrun = function (args) {
    var shouldClose = false;

    document.getElementById('footer').style.display = 'none';
    if (args.failures > 0) {
        write(tag('h2', 'testRunEndFailure', $fmt('{0}/{1} tests failed!', args.failures, args.count)));
    } else {
        write(tag('h2', 'testRunEndSuccess', $fmt('{0} tests passed!', args.count)));
        shouldClose = true;
    }

    // Close the app after 5 seconds on a successful run or if we're running
    // the tests automatically as part of the build
    //if (shouldClose || $harness.testResultsServerUrl !== undefined) {
    //    window.setTimeout(function () { window.close(); }, 5000);
    //}
};

reporter.onexecuteprogress = function (args) {
    document.getElementById('currentTestNumber').innerText = args.progress;
    document.getElementById('failureNumber').innerText = args.failures;
    document.getElementById('progress').value = args.progress / args.count * 100;
};

reporter.onexecutestartgroup = function (args) {
    write(tag('h2', 'testGroup', $fmt('{0} tests', args.name)));
};

reporter.onexecutestarttest = function (args) {
    write(tag('h3', 'testMethod',
        tag('span', 'testMethodName', args.name) +
        tag('span', 'testMethodDescription', args.description)));
    if (args.excluded) {
        write(tag('h4', 'testMethodIgnore', $fmt('Ignored: {0}', args.excludeReason)));
    }
};

reporter.onexecuteendtest = function (args) {
    var parent = getResultsElement();
    var index = parent.childElementCount - 1;
    while (index >= 0) {
        var element = parent.children[index];
        if (element.nodeName == 'H3') {
            if (args.excluded) {
                element.firstChild.className = 'testMethodNameIgnored';
            } else if (args.passed) {
                element.firstChild.className = 'testMethodNamePassed';
            } else {
                element.firstChild.className = 'testMethodNameFailed';
            }
            break;
        }
        index--;
    }
};

reporter.onexecutelog = function (args) {
    write(tag('h4', 'testMethodLog', args.message));
};

reporter.onexecuteerror = function (args) {
    write(tag('h4', 'testMethodError', $fmt('Error: {0}', args.message)));
};

reporter.onexecutestatus = function (args) {
    var testStatus = document.getElementById('testStatus');
    testStatus.innerText = args.message;
};

// Export the TestReporter for use by the harness
exports.Reporter = reporter;

// Expose the global setup function
global.$showTestSetupUI = function () {
    var setupFlyout = document.getElementById('setupFlyout');
    var txtRuntimeUri = document.getElementById('txtRuntimeUri');
    var txtRuntimeKey = document.getElementById('txtRuntimeKey');
    var txtTags = document.getElementById('txtTags');
    var btnStartTests = document.getElementById('btnStartTests');
    var testResults = document.getElementById('testResults');
    var show = function () {
        setupFlyout.winControl.show(testResults, "bottom");
    };

    // Delay starting the unit tests if we're supposed to break before
    // we start (so a debugger can attach to the process)
    if (!$harness.settings.breakOnStart) {
        setupFlyout.winControl.hide();
        $run();
    } else {
        // Have any click on the background show the flyout so it won't
        // just disappear all the time (modality would be nice)
        window.onclick = show;

        // Fill the UI with the latest values
        txtRuntimeUri.value = $harness.settings.custom.MobileServiceRuntimeUrl || '';
        txtRuntimeKey.value = $harness.settings.custom.MobileServiceRuntimeKey || '';
        txtTags.value = $harness.settings.tagExpression || '';

        show();
        btnStartTests.onclick = function () {
            window.onclick = null;

            $harness.settings.custom.MobileServiceRuntimeUrl = txtRuntimeUri.value;
            $harness.settings.custom.MobileServiceRuntimeKey = txtRuntimeKey.value;
            $harness.settings.tagExpression = txtTags.value;
            setupFlyout.winControl.hide();
            $run();
        };
    }
};