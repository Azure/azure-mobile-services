// ----------------------------------------------------------------------------
// ----------------------------------------------------------------------------
// Copyright (c) Yoshiyuki Taniguchi (iwatesoftware@live.jp)
// ----------------------------------------------------------------------------

var PostMessageExchange = require('PostMessageExchange');

exports.supportsCurrentRuntime = function () {
    /// <summary>
    /// Determines whether or not this login UI is usable in the current runtime.
    /// </summary>
    return true;
};

exports.login = function (startUri, endUri, callback) {
    /// <summary>
    /// Displays the login UI and calls back on completion
    /// </summary>

    // Tell the runtime which form of completion signal we are looking for,
    // and which origin should be allowed to receive the result (note that this
    // is validated against whitelist on the server; we are only supplying this
    // origin to indicate *which* of the whitelisted origins to use).
    // Browsers don't allow postMessage to a file:// URL (except by setting origin to "*", which is unacceptable)
    // so abort the process early with an explanation in that case.
    var completionOrigin = PostMessageExchange.getOriginRoot(window.location.href);
    if (!(completionOrigin && (completionOrigin.indexOf("http:") === 0 || completionOrigin.indexOf("https:") === 0 || completionOrigin.indexOf("app:") === 0))) {
        var error = "Login is only supported from http:// or https:// URLs. Please host your page in a web server.";
        callback(error, null);
        return;
    }
    var loginFrame = document.createElement("div"),
        loginTitle = document.createElement("p"),
        loginCloseBtn = document.createElement("a"),
        loginBrowser = document.createElement("iframe"),
        complete = function(errorValue, oauthValue) {
            // Clean up event handlers, windows, frames, ...
            loginFrame.parentNode.removeChild(loginFrame);
            // Finally, notify the caller
            callback(errorValue, oauthValue);
        };
    loginFrame.style = "position:absolute; width:100%; height:100%; left: 0; top: 0; background: white; display: flex; flex-direction: column;";
    loginTitle.textContent = "Login";
    loginTitle.style = "background:rgb(237,112,23); color: white; padding: 5px 10px";
    loginCloseBtn.innerHTML = "&times";
    loginCloseBtn.style = "text-decoration: none; color: white; float:right;";
    loginCloseBtn.addEventListener("click", function(){
        complete("canceled", null);
    });
    loginBrowser.setAttribute("mozbrowser","");
    loginBrowser.setAttribute("remote","");
    loginBrowser.style = "width:100%; flex: 1; border:none;";
    loginBrowser.src = startUri;
    loginBrowser.addEventListener("mozbrowserlocationchange", function(evt){
        var location = evt.detail;
        if(location.startsWith(endUri)){
            var hash = location.substring(location.indexOf("#")+1),
                encToken = location.substring(location.indexOf("=") + 1),
                decToken = decodeURIComponent(encToken),
                token;
            try{
                token = JSON.parse(decToken);
            } catch(ex){
                 complete("canceled", null);
            }
            complete(null, token);
        }
    });
    document.body.appendChild(loginFrame);
    loginFrame.appendChild(loginTitle);
    loginFrame.appendChild(loginBrowser);
    loginTitle.appendChild(loginCloseBtn);

    // Permit cancellation, e.g., if the app tries to login again while the popup is still open
    return {
        cancelCallback: function () {
            complete("canceled", null);
            return true; // Affirm that it was cancelled
        }
    };
};