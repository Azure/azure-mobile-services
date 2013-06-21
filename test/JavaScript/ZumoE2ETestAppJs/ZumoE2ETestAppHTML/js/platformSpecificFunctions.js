// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

function createPlatformSpecificFunctions() {

    var alertFunction;

    alertFunction = function (text) {
        window.alert(text);
    }

    var saveAppInfo = function (lastAppUrl, lastAppKey, lastUploadLogUrl) {
        /// <param name="lastAppUrl" type="String">The last value used in the application URL text box</param>
        /// <param name="lastAppKey" type="String">The last value used in the application key text box</param>
        /// <param name="lastUploadLogUrl" type="String">The last value used in the upload logs URL text box</param>
        var state = {
            lastAppUrl: lastAppUrl,
            lastAppKey: lastAppKey,
            lastUploadUrl: lastUploadLogUrl
        };
    }

    return {
        alert: alertFunction,
        saveAppInfo: saveAppInfo,
        IsHTMLApplication: true,
    };
}

var testPlatform = createPlatformSpecificFunctions();
