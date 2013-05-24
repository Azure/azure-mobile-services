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

        //[To do]: write file code is pending to implement, I think it is not very imporatent to write this code for HTML application
        // Need to confirm with Carlos


    }

    return {
        alert: alertFunction,
        saveAppInfo: saveAppInfo,
        IsHTMLApplication: true,
    };
}


var testPlatform = createPlatformSpecificFunctions();
