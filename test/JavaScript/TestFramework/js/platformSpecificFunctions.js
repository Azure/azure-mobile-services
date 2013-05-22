
function createPlatformSpecificFunctions() {

    var alertFunction;
    
    if (!IsHTMLApplicationRunning) { // Ture if user run WinJS application
        if (typeof alert === 'undefined') {
            alertFunction = function (text) {
                var dialog = new Windows.UI.Popups.MessageDialog(text);
                dialog.showAsync();
            }
        }
    } else {
        //Custom method that can use alert method to display text validation message
        alertFunction = function (text) {
            window.alert(text);
        }
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
        if (!IsHTMLApplicationRunning) {
            WinJS.Application.local.writeText('savedAppInfo.txt', JSON.stringify(state));
        } else {
            // [To do]: I think Save last AppURL and AppKey is not very imporatent to implement in HTML. Need to confirm with carlos
        }

    }

return {
    alert: alertFunction,
    saveAppInfo: saveAppInfo,
};
}


var testPlatform = createPlatformSpecificFunctions();
