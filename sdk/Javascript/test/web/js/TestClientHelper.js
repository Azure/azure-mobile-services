// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

(function (global) {

    // This file provides global.$getClient, which the test code uses when trying to make requests to the runtime.
    // It also allows you to configure the URL of the runtime that it will talk to.

    // Note: Implementing this without jQuery (or similar DOM library) to ensure
    // that tests don't inadvertently start depending on it

    global.$getClient = function () {
        return new global.WindowsAzure.MobileServiceClient(clientUriElem.value);
    };

    var configureButtonElem = document.getElementById("edit-test-config"),
        configureFormElem = document.getElementById("test-config"),
        clientUriElem = document.getElementById("runtime-uri"),
        defaultClientUri = "http://localhost:11926/";

    // Try to recover client URI from querystring
    clientUriElem.value = parseQueryString()[clientUriElem.name] || defaultClientUri;

    configureButtonElem.onclick = function () {
        configureFormElem.style.display = "block";
        configureButtonElem.style.display = "none";
    };

    function parseQueryString() {
        var href = global.window.location.href,
                   queryStartPos = href.indexOf("?"),
                   query = queryStartPos >= 0 ? href.substring(queryStartPos + 1) : null,
                   tokens = query ? query.split("&") : [],
                   result = {},
                   tokenIndex,
                   pair;

        for (tokenIndex = 0; tokenIndex < tokens.length; tokenIndex++) {
            pair = tokens[tokenIndex].split("=");
            if (pair.length === 2) {
                result[decodeURIComponent(pair[0])] = decodeURIComponent(pair[1]);
            }
        }

        return result;
    }

})(this);