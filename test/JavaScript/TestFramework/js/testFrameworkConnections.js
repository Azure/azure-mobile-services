// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="platformSpecificFunctions.js" />
/// <reference path="testFramework.js" />

if (!testPlatform.IsHTMLApplication) { // Call UpdateTestListHeight() if WinJS application is running
    function updateTestListHeight() {
        var tableScroll = document.getElementById('table-scroll');
        var tableHead = document.getElementById('tblTestsHead');
        var tableHeight = document.getElementById('testGroupsTableCell').getBoundingClientRect().height;
        var padding = 30;
        var headerHeight = tableHead.getBoundingClientRect().height;
        var bodyHeight = tableHeight - headerHeight - padding;
        tableScroll.style.height = bodyHeight + "px";
    }
    updateTestListHeight();
}

function setDefaultButtonEventHandler() {
    var buttons = document.getElementsByTagName('button');
    for (var i = 0; i < buttons.length; i++) {
        var btn = buttons[i];
        btn.onclick = function (evt) {
            var name = evt.target.innerText;
            testPlatform.alert('Operation ' + name + ' not implemented');
        }
    }
}

setDefaultButtonEventHandler();

function saveLastUsedAppInfo() {
    var lastAppUrl = document.getElementById('txtAppUrl').value;
    var lastAppKey = document.getElementById('txtAppKey').value;
    var lastUploadUrl = document.getElementById('txtSendLogsUrl').value;

    testPlatform.saveAppInfo(lastAppUrl, lastAppKey, lastUploadUrl);

}

function getTestDisplayColor(test) {
    if (test.status === zumo.TSFailed) {
        return 'Red';
    } else if (test.status == zumo.TSPassed) {
        return 'Lime';
    } else if (test.status == zumo.TSRunning) {
        return 'Gray';
    } else {
        return 'White';
    }
}

document.getElementById('btnRunTests').onclick = function (evt) {
    if (zumo.currentGroup < 0) {
        testPlatform.alert('Please select a test group to run');
        return;
    }

    var currentGroup = zumo.testGroups[zumo.currentGroup];
    var appUrl = document.getElementById('txtAppUrl').value;
    var appKey = document.getElementById('txtAppKey').value;
    var uploadUrl = document.getElementById('txtSendLogsUrl').value.trim();

    if (zumo.initializeClient(appUrl, appKey)) {

        saveLastUsedAppInfo();

        var groupDone = function (testsPassed, testsFailed) {
            var logs = 'Test group finished';
            logs = logs + '\n=-=-=-=-=-=-=-=-=-=-=-=-=-=-=\n';
            logs = logs + 'Tests passed: ' + testsPassed + '\n';
            logs = logs + 'Tests failed: ' + testsFailed;
            if (currentGroup.name.indexOf(zumo.AllTestsGroupName) === 0 && uploadUrl !== '') {
                // For all tests, upload logs automatically if URL is set
                var testLogs = currentGroup.getLogs();
                uploadLogs(uploadUrl, testLogs, true);
                if (testsFailed == 0) {
                    if (currentGroup.name == zumo.AllTestsGroupName) {
                        btnRunAllTests.textContent = "Passed";
                    }
                    else {
                        btnRunAllUnattendedTests.textContent = "Passed";
                    }

                }
            }

            if (showAlerts) {
                testPlatform.alert(logs);
            }

        }
        var updateTest = function (test, index) {
            var tblTests = document.getElementById('tblTestsBody');
            var tr = tblTests.childNodes[index];
            var td = tr.firstChild;
            td.style.color = getTestDisplayColor(test);
            td.innerText = "" + (index + 1) + ". " + test.displayText();
        }
        var testFinished = updateTest;
        var testStarted = updateTest;
        currentGroup.runTests(testStarted, testFinished, groupDone);
    }
}

document.getElementById('btnResetTests').onclick = function (evt) {
    if (zumo.currentGroup < 0) {
        testPlatform.alert('Please select a test group to reset its tests');
        return;
    }

    var currentGroup = zumo.testGroups[zumo.currentGroup];
    var tests = currentGroup.tests;
    var tblTests = document.getElementById('tblTestsBody');
    tests.forEach(function (test, index) {
        test.reset();
        var tr = tblTests.childNodes[index];
        var td = tr.firstChild;
        td.style.color = getTestDisplayColor(test);
        td.innerText = "" + (index + 1) + ". " + test.displayText();
    });
}

document.getElementById('btnSendLogs').onclick = function (evt) {
    if (zumo.currentGroup < 0) {
        testPlatform.alert('Please select a test group to upload the logs for');
        return;
    }

    var uploadUrl = document.getElementById('txtSendLogsUrl').value;
    if (uploadUrl.trim() == '') {
        testPlatform.alert('Please enter a valid upload url');
        return;
    }

    var currentGroup = zumo.testGroups[zumo.currentGroup];
    var logs = currentGroup.getLogs();
    uploadLogs(uploadUrl, logs, false, function () {
        saveLastUsedAppInfo();
    });
}

function uploadLogs(url, logs, allTests, done) {
    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function () {
        if (xhr.readyState === 4) {
            if (showAlerts) {
                testPlatform.alert(xhr.responseText);
            }
            if (done) {
                done();
            }
            document.getElementById('btnSendLogs').textContent = xhr.responseText;
            if (!testPlatform.IsHTMLApplication) {
                Windows.Storage.KnownFolders.picturesLibrary.createFileAsync("done.txt", Windows.Storage.CreationCollisionOption.replaceExisting).then(function (file) {
                    Windows.Storage.FileIO.writeTextAsync(file, xhr.responseText);
                });
            }
           
        }
    }

    var platform = testPlatform.IsHTMLApplication ? 'htmljs' : 'winstorejs';
    var uploadUrl = url + '?platform=' + platform;
    if (allTests) {
        uploadUrl = uploadUrl + '&allTests=true';
    }

    var runtimeVersion = zumo.util.globalTestParams[zumo.constants.SERVER_VERSION_KEY];
    var clientVersion = zumo.util.globalTestParams[zumo.constants.CLIENT_VERSION_KEY];
    if (runtimeVersion) {
        uploadUrl = uploadUrl + '&runtimeVersion=' + runtimeVersion;
    }

    if (clientVersion) {
        uploadUrl = uploadUrl + '&clientVersion=' + clientVersion;
    }

    xhr.open('POST', uploadUrl, true);
    xhr.setRequestHeader('content-type', 'text/plain');
    xhr.send(logs);
}

var testGroups = zumo.testGroups;

var btnRunAllTests = document.getElementById('btnRunAllTests');
var btnRunAllUnattendedTests = document.getElementById('btnRunAllUnattendedTests');
var showAlerts = true;

if (btnRunAllTests) btnRunAllTests.onclick = handlerForAllTestsButtons(false);

if (btnRunAllUnattendedTests) btnRunAllUnattendedTests.onclick = handlerForAllTestsButtons(true);

function handlerForAllTestsButtons(unattendedOnly) {
    return function (evt) {
        showAlerts = false;
        for (var i = 0; i < testGroups.length; i++) {
            var groupName = testGroups[i].name;
            if (!unattendedOnly && groupName === zumo.AllTestsGroupName) {
                testGroupSelected(i);
                break;
            } else if (unattendedOnly && groupName == zumo.AllTestsUnattendedGroupName) {
                testGroupSelected(i);
                break;
            }
        }
    }
}

function highlightSelectedGroup(groupIndex) {
    var testsGroupBody = document.getElementById('tblTestsGroupBody');
    for (var i = 0; i < testsGroupBody.children.length; i++) {
        var tr = testsGroupBody.children[i];
        var td = tr.firstElementChild;
        td.style.fontWeight = i == groupIndex ? 'bold' : 'normal';
    }
}

function testGroupSelected(index) {
    highlightSelectedGroup(index);
    var group = testGroups[index];
    zumo.currentGroup = index;
    document.getElementById('testsTitle').innerText = 'Tests for group: ' + group.name;
    var tblTests = document.getElementById('tblTestsBody');
    for (var i = tblTests.childElementCount - 1; i >= 0; i--) {
        tblTests.removeChild(tblTests.children[i]);
    }

    function viewTestLogs(groupIndex, testIndex) {
        var test = zumo.testGroups[groupIndex].tests[testIndex];
        var logs = test.getLogs();
        testPlatform.alert(logs);
    }

    group.tests.forEach(function (test, index) {
        var tr = document.createElement('tr');
        var td = document.createElement('td');
        td.innerText = "" + (index + 1) + ". " + test.displayText();
        tr.appendChild(td);
        td.style.color = getTestDisplayColor(test);
        td.ondblclick = function () {
            viewTestLogs(zumo.currentGroup, index);
        }
        tblTests.appendChild(tr);
    });

    if (group.name === zumo.AllTestsGroupName || group.name === zumo.AllTestsUnattendedGroupName) {
        document.getElementById('btnRunTests').click();
    }
}

function addAttribute(element, name, value) {
    var attr = document.createAttribute(name);
    attr.value = value.toString();
    element.attributes.setNamedItem(attr);
}

function addTestGroups() {
    var tblTestsGroup = document.getElementById('tblTestsGroupBody');

    for (var index = 0; index < testGroups.length; index++) {
        var item = testGroups[index];
        var name = "" + (index + 1) + ". " + item.name + " tests";
        var tr = document.createElement('tr');
        var td = document.createElement('td');
        tr.appendChild(td);
        var a = document.createElement('a');
        td.appendChild(a);
        addAttribute(a, 'href', '#');
        addAttribute(a, 'class', 'testGroupItem');

        var attachEvent = function (a, index) {
            if (a.attachEvent) {

                a.attachEvent('onclick', function () {
                    testGroupSelected(index);
                });
                a.innerText = toStaticHTML(name);
            }
            else {
                a.addEventListener('click', function () {
                    testGroupSelected(index);
                }, false);
                a.textContent = name;
            }
        }

        attachEvent(a, index);

        tblTestsGroup.appendChild(tr);
    }

}

addTestGroups();
