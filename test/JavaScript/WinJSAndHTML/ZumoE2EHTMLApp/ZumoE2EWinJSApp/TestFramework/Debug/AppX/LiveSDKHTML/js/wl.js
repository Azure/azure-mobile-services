//! Copyright (c) Microsoft Corporation. All rights reserved.
// WL.JS Version 5.2.3370.0802

(function() {
if (!window.WL) {


var API_DOWNLOAD = "download",
    API_INTERFACE_METHOD = "interface_method", 
    API_JSONP_CALLBACK_NAMESPACE_PREFIX = "WL.Internal.jsonp.",
    API_JSONP_URL_LIMIT = 2000,
    API_PARAM_BODY = "body",
    API_PARAM_CALLBACK = "callback",
    API_PARAM_CODE = "code",
    API_PARAM_ELEMENT = "element",
    API_PARAM_ERROR = "error",
    API_PARAM_ERROR_DESC = "error_description",
    API_PARAM_LOGGING = "logging",
    API_PARAM_TRACING = "tracing",
    API_PARAM_MESSAGE = "message",
    API_PARAM_METHOD = "method",
    API_PARAM_FILEINPUT = "file_input",
    API_PARAM_STREAMINPUT = "stream_input",
    API_PARAM_FILENAME = "file_name",
    API_PARAM_FILEOUTPUT = "file_output",
    API_PARAM_OVERWRITE = "overwrite",
    API_PARAM_PATH = "path",
    API_PARAM_PRETTY = "pretty",
    API_PARAM_RESULT = "result",
    API_PARAM_STATUS = "status",
    API_PARAM_SSLRESOURCE = "return_ssl_resources",
    API_STATUS_SUCCESS = "success",
    API_STATUS_ERROR = "error",
    API_SUPPRESS_REDIRECTS = "suppress_redirects",
    API_SUPPRESS_RESPONSE_CODES = "suppress_response_codes",
    API_X_HTTP_LIVE_LIBRARY = "x_http_live_library";    

/**
 * Application status values indicating whether the app has invoked WL.init(...).
 */
var APP_STATUS_NONE = 0,
    APP_STATUS_INITIALIZED = 1;

/**
 * Auth parameter key values used in multiple occassions: redirect_url parameter, auth cookie sub-key, auth response properties.
 */
var AK_ACCESS_TOKEN = "access_token",
    AK_APPSTATE = "appstate",
    AK_AUTH_TOKEN = "authentication_token",
    AK_CLIENT_ID = "client_id",
    AK_DISPLAY = "display",
    AK_CODE = "code",
    AK_ERROR = "error",
    AK_ERROR_DESC = "error_description",
    AK_EXPIRES = "expires",
    AK_EXPIRES_IN = "expires_in",
    AK_LOCALE = "locale",
    AK_REDIRECT_URI = "redirect_uri",
    AK_RESPONSE_TYPE = "response_type",
    AK_REQUEST_TS = "request_ts",
    AK_SCOPE = "scope",
    AK_SESSION = "session",
    AK_SECURE_COOKIE = "secure_cookie",
    AK_STATE = "state",
    AK_STATUS = "status";
    
var AK_COOKIE_KEYS = [AK_ACCESS_TOKEN, AK_AUTH_TOKEN, AK_SCOPE, AK_EXPIRES_IN, AK_EXPIRES];

/**
 * Auth session status.
 */
var AS_CONNECTED = "connected", // The user is connected and signed in.
    AS_NOTCONNECTED = "notConnected", // The user is not connected.
    AS_UNCHECKED = "unchecked",   // We haven't checked the status yet.
    AS_UNKNOWN = "unknown",   // The user is unknown.
    AS_EXPIRING = "expiring", // The token will expire soon.
    AS_EXPIRED = "expired"; // The token is expired.

var BT_GROUP_UPLOAD = "live-sdk-upload",
    BT_GROUP_DOWNLOAD = "live-sdk-download";

/**
 * Compatible parameter keys(names).
 */
var CK_APPID = "appId",
    CK_CHANNELURL = "channelUrl";

/**
* Cookie names.
*/
var COOKIE_AUTH = "wl_auth",  // This cookie stores the Auth information.
    COOKIE_UPLOAD = "wl_upload";
    
/**
* Display types.
*/
var DISPLAY_APP = "app",
    DISPLAY_POPUP = "popup",
    DISPLAY_PAGE = "page",
    DISPLAY_TOUCH = "touch",
    DISPLAY_NONE = "none";

var DOM_DISPLAY_NONE = "none";

/**
 * Event types.
 */
var EVENT_AUTH_LOGIN = "auth.login",
    EVENT_AUTH_LOGOUT = "auth.logout",
    EVENT_AUTH_SESSIONCHANGE = "auth.sessionChange",
    EVENT_AUTH_STATUSCHANGE = "auth.statusChange",
    EVENT_LOG = "wl.log";

var ERROR_ACCESS_DENIED = "access_denied",
    ERROR_CONNECTION_FAILED = "connection_failed",
    ERROR_COOKIE_ERROR = "invalid_cookie",
    ERROR_INVALID_REQUEST = "invalid_request",
    ERROR_REQ_CANCEL = "request_canceled",
    ERROR_REQUEST_FAILED = "request_failed",
    ERROR_TIMEDOUT = "timed_out",
    ERROR_UNKNOWN_USER = "unknown_user",
    ERROR_USER_CANCELED = "user_canceled",
    ERROR_DESC_ACCESS_DENIED = "METHOD: Failed to get the required user permission to perform this operation.",
    ERROR_DESC_BROWSER_ISSUE = "The request could not be completed due to browser issues.",
    ERROR_DESC_BROWSER_LIMIT = "The request could not be completed due to browser limitations.",
    ERROR_DESC_CANCEL = "METHOD: The operation has been canceled.",
    ERROR_DESC_COOKIE_INVALID = "The 'wl_auth' cookie is not valid.",
    ERROR_DESC_COOKIE_OVERWRITE = "The 'wl_auth' cookie has been modified incorrectly. Ensure that the redirect URI only modifies sub-keys for values received from the OAuth endpoint.",
    ERROR_DESC_COOKIE_MULTIPLEVALUE = "The 'wl_auth' cookie has multiple values. Ensure that the redirect URI specifies a cookie domain and path when setting cookies.",
    ERROR_DESC_DOM_INVALID = "METHOD: The input parameter 'PARAM' does not reference a valid DOM element.",
    ERROR_DESC_EXCEPTION = "METHOD: An exception was received for EVENT. Detail: MESSAGE",
    ERROR_DESC_ENSURE_INIT = "METHOD: The WL object must be initialized with WL.init() prior to invoking this method.",
    ERROR_DESC_FAIL_CONNECT = "A connection to the server could not be established.",
    ERROR_DESC_FAIL_IDENTIFY_USER = "The user could not be identified.",
    ERROR_DESC_LOGIN_CANCEL = "The pending login request has been canceled.",
    ERROR_DESC_LOGOUT_NOTSUPPORTED = "Logging out the user is not supported in current session because the user is logged in with a Microsoft account on this computer. To logout, the user may quit the app or log out from the computer.",
    ERROR_DESC_PARAM_INVALID = "METHOD: The input parameter 'PARAM' is not valid.",
    ERROR_DESC_PARAM_MISSING = "METHOD: The input parameter 'PARAM' must be included.",
    ERROR_DESC_PARAM_TYPE_INVALID = "METHOD: The type of the provided value for the input parameter 'PARAM' is not valid.",
    ERROR_DESC_PENDING_CALL_CONFLICT = "METHOD: There is a pending METHOD request, the current call will be ignored.",
    ERROR_DESC_PENDING_LOGIN_CONFLICT = ERROR_DESC_PENDING_CALL_CONFLICT.replace(/METHOD/g, "WL.login"),
    ERROR_DESC_PENDING_FILEDIALOG_CONFLICT = ERROR_DESC_PENDING_CALL_CONFLICT.replace(/METHOD/g, "WL.fileDialog"),
    ERROR_DESC_PENDING_UPLOAD_IGNORED = ERROR_DESC_PENDING_CALL_CONFLICT.replace(/METHOD/g, "WL.upload"),
    ERROR_DESC_REDIRECTURI_MISSING = "METHOD: The input parameter 'redirect_uri' is required if the value of the 'response_type' parameter is 'code'.",
    ERROR_DESC_REDIRECTURI_INVALID_WWA = "WL.init: The redirect_uri value should be the same as the value of 'Redirect Domain' on API Settings page of your app on https://manage.dev.live.com. It must begin with 'http://' or 'https://'.",
    ERROR_DESC_UNSUPPORTED_API_CALL = "METHOD: The api call is not supported on this platform.",
    ERROR_DESC_UNSUPPORTED_RESPONSE_TYPE_CODE = "WL.init: The response_type value 'code' is not supported on this platform.",
    ERROR_DESC_URL_SSL = "METHOD: The input parameter 'redirect_uri' must use https: to match the scheme of the current page.",
    ERROR_TRACE_AUTH_TIMEOUT = "The auth request is timed out.",
    ERROR_TRACE_AUTH_CLOSE = "The popup is closed without receiving consent.";

/**
 * Flash initialization status.
 */
var FLASH_STATUS_NONE = 0,
    FLASH_STATUS_INITIALIZING = 1,
    FLASH_STATUS_INITIALIZED = 2,
    FLASH_STATUS_ERROR = 3;

/**
* Http method names
*/
var HTTP_METHOD_GET = "GET",
    HTTP_METHOD_POST = "POST",
    HTTP_METHOD_PUT = "PUT",
    HTTP_METHOD_DELETE = "DELETE",
    HTTP_METHOD_COPY = "COPY",
    HTTP_METHOD_MOVE = "MOVE";

/**
 * The maximum time in milliseconds to expire a getLoginStatus() request.
 */
var MAX_GETLOGINSTATUS_TIME = 30000;

var METHOD = "METHOD";

/**
 * Promise class event names
 */
var PROMISE_EVENT_ONSUCCESS = "onSuccess",
    PROMISE_EVENT_ONERROR = "onError",
    PROMISE_EVENT_ONPROGRESS = "onProgress";

/**
 * Used to detect the type of redirect.
 * redirect_type is the name of the parameter.
 * auth is a value of the parameter and is used for authorization redirects.
 * upload is a value of the parameter and is used for WL.upload Form POST redirects.
 */
var REDIRECT_TYPE = "redirect_type",
    REDIRECT_TYPE_AUTH = "auth",  
    REDIRECT_TYPE_UPLOAD = "upload";

/** 
 * Response type values.
 */
var RESPONSE_TYPE_CODE = "code",
    RESPONSE_TYPE_TOKEN = "token";

/**
* Url scheme values.
*/
var SCHEME_HTTPS = "https:",
    SCHEME_HTTP = "http:";

/**
 * Scope deliminators
 */
var SCOPE_SIGNIN = "wl.signin",
    SCOPE_SKYDRIVE = "wl.skydrive",
    SCOPE_SKYDRIVE_UPDATE = "wl.skydrive_update", 
    SCOPE_DELIMINATOR = /\s|,/;

/**
 * Type names
 */
var TYPE_BOOLEAN = "boolean",
    TYPE_DOM = "dom",
    TYPE_FUNCTION = "function",
    TYPE_NUMBER = "number",
    TYPE_STRING = "string",
    TYPE_OBJECT = "object",
    TYPE_STRINGORARRAY = "string_or_array",
    TYPE_UNDEFINED = "undefined";

var UI_PARAM_NAME = "name",
    UI_PARAM_ELEMENT = "element",
    UI_PARAM_BRAND = "brand",
    UI_PARAM_TYPE = "type",
    UI_PARAM_SIGN_IN_TEXT = "sign_in_text",
    UI_PARAM_SIGN_OUT_TEXT = "sign_out_text",
    UI_PARAM_THEME = "theme",
    UI_PARAM_ONLOGGEDIN = "onloggedin",
    UI_PARAM_ONLOGGEDOUT = "onloggedout",
    UI_PARAM_ONERROR = "onerror";

var UI_BRAND_MESSENGER = "messenger",
    UI_BRAND_HOTMAIL = "hotmail",
    UI_BRAND_SKYDRIVE = "skydrive",
    UI_BRAND_WINDOWS = "windows",
    UI_BRAND_WINDOWSLIVE = "windowslive",
    UI_BRAND_NONE = "none";

var UI_SIGNIN = "signin",
    UI_SIGNIN_TYPE_SIGNIN = UI_SIGNIN,
    UI_SIGNIN_TYPE_LOGIN = "login",
    UI_SIGNIN_TYPE_CONNECT = "connect",
    UI_SIGNIN_TYPE_CUSTOM = "custom";

var UI_SIGNIN_THEME_BLUE = "blue",
    UI_SIGNIN_THEME_WHITE = "white",
    UI_SIGNIN_THEME_DARK = "dark",
    UI_SIGNIN_THEME_LIGHT = "light";

// names of parameters used in an upload request's state
var UPLOAD_STATE_ID = "id";

var WL_SDK_ROOT = "sdk_root",
    WL_TRACE = "wl_trace";

var expectedCallback_Optional = {
    name: API_PARAM_CALLBACK,
    type: TYPE_FUNCTION,
    optional: true
};

var expectedCallback_Required = {
    name: API_PARAM_CALLBACK,
    type: TYPE_FUNCTION,
    optional: false
};

window.WL = {

    getSession: function () {
        /// <summary>
        /// A synchronous function that gets the current session object, if it exists.
        /// </summary> 
        /// <returns type="Object" >The current session object.</returns>

        try {
            return wl_app.getSession();
        }
        catch (e) {
            logError(e.message);
        }
    },

    getLoginStatus: function (callback, force) {
        /// <summary>
        /// Returns the status of the current user. If the user is signed in and 
        /// connected to your application, it returns the session object.
        /// This is an asynchronous function that returns the user's status by contacting the Windows Live 
        /// OAuth server. If the user status is already known, the library may return what is cached.
        /// However, you can force the library to retrieve up-to-date status by setting the "force" 
        /// parameter to true. This is an async method that returns a Promise object that allows you to 
        /// attach events to handle succeeded and failed situations.
        /// </summary>
        /// <param name="callback" type="Function">Optional. The callback function that is invoked when the user's login status is retrieved.</param>
        /// <param name="force" type="Boolean">Optional. If set to false (default), the function may return an existing user status, if it exists. 
        /// Otherwise, if set to true, the function contacts the server to determine the user's status.</param>
        /// <returns type="Promise" mayBeNull="false" >The Promise object that allows you to attach events to handle succeeded and failed
        /// situations.</returns>

        try {
            return wl_app.getLoginStatus(
            {
                callback: findArgumentByType(arguments, TYPE_FUNCTION, 2),
                internal: false
            },
            findArgumentByType(arguments, TYPE_BOOLEAN, 2));
        }
        catch (e) {
            return handleAsyncCallingError("WL.getLoginStatus", e);
        }
    },

    logout: function (callback) {
        /// <summary>
        /// Logs the user out of Windows Live and clears any user state that is maintained 
        /// by the JavaScript library, such as cookies. This is an async method that returns a Promise object that 
        /// allows you to attach events to handle succeeded and failed situations.
        /// </summary>
        /// <param name="callback" type="Function">Optional. Specifies a callback function that is invoked when logout is complete.</param>
        /// <returns type="Promise" mayBeNull="false" >The Promise object that allows you to attach events to handle succeeded and failed
        /// situations.</returns>

        try {
            validateParams(callback, expectedCallback_Optional, "WL.logout");
            return wl_app.logout({ callback: callback });
        }
        catch (e) {
            return handleAsyncCallingError("WL.logout", e);
        }
    },

    canLogout: function () {
        /// <summary>
        /// Returns if the app can log the user out.
        /// </summary>
        /// <returns type="boolean" >Whether the app can logout.</returns>

        return wl_app.canLogout();
    },

    api: function (properties, callback) {
        /// <summary>
        /// Makes a call to the Windows Live REST API. This is an async method that returns a Promise object that allows you to 
        /// attach events to handle succeeded and failed situations.
        /// </summary>
        /// <param name="properties" type="Object">Required. A JSON object containing the properties for making the API call:
        /// &#10; path: Required. The path to the REST API object.
        /// &#10; method: The HTTP method. Supported values include "GET" (default), "PUT", "POST", "DELETE", "MOVE", and "COPY".
        /// &#10; body: A JSON object containing all necessary properties for making the REST API request.
        /// </param>
        /// <param name="callback" type="Function">Required. A callback function that is invoked when the REST API call is complete.</param>
        /// <returns type="Promise" mayBeNull="false" >The Promise object that allows you to attach events to handle succeeded and failed
        /// situations.</returns>

        try {
            var args = normalizeApiArguments(arguments);

            // Validate parameters
            validateProperties(args,
                [{ name: API_PARAM_PATH, type: TYPE_STRING, optional: false },
                { name: API_PARAM_METHOD, type: TYPE_STRING, optional: true },
                    expectedCallback_Optional],
                "WL.api");
                
            return wl_app.api(args);
        }
        catch (e) {
            return handleAsyncCallingError("WL.api", e);
        }
    }
};

var allowedEvents = [EVENT_AUTH_LOGIN, EVENT_AUTH_LOGOUT, EVENT_AUTH_SESSIONCHANGE, EVENT_AUTH_STATUSCHANGE, EVENT_LOG];
WL.Event = {

    subscribe: function (event, callback) {
        /// <summary>
        /// Adds a handler to an event.
        /// </summary>
        /// <param name="event" type="String">Required. The name of the event to add a handler to. 
        /// Available events are: "auth.login", "auth.logout", "auth.sessionChange", 
        /// "auth.statusChange", and "wl.log".</param>
        /// <param name="callback" type="Function">Required. The event handler function to be added to the event.</param>

        try {
            // Validate parameters
            validateParams(
                [event, callback],
                [{ name: "event", type: TYPE_STRING, allowedValues: allowedEvents, caseSensitive: true, optional: false },
                    expectedCallback_Required],
                "WL.Event.subscribe");

            wl_event.subscribe(event, callback);
        }
        catch (e) {
            logError(e.message);
        }
    },

    unsubscribe: function (event, callback) {
        /// <summary>
        /// Removes a handler from an event.
        /// </summary>
        /// <param name="event" type="String">Required. The name of the event from which to remove a handler.</param>
        /// <param name="callback" type="Function">Optional. Removes the callback from the event. If this parameter is omitted, all 
        /// callback functions registered to the event are removed.</param>
        
        try {
            // Validate parameters
            validateParams([event, callback],
                [{ name: "event", type: TYPE_STRING, allowedValues: allowedEvents, caseSensitive: true, optional: false },
                expectedCallback_Optional],
                "WL.Event.unsubscribe");
            wl_event.unsubscribe(event, callback);
        }
        catch (e) {
            logError(e.message);
        }
    }
};

WL.Internal = {};

function normalizeArguments(args, methodName) {
    var receivedArgs = cloneArray(args),
        properties = null,
        callback = null;

    for (var i = 0; i < receivedArgs.length; i++) {
        var arg = receivedArgs[i],
            argType = typeof arg;

        if (argType === TYPE_OBJECT && properties === null) {
            properties = cloneObject(arg);
        }
        else if (argType === TYPE_FUNCTION && callback === null) {
            callback = arg;
        }
    }

    properties = properties || {};

    if (callback) {
        properties.callback = callback;
    }

    properties[API_INTERFACE_METHOD] = methodName;
    
    return properties;
}

function normalizeApiArguments(args) {
    var receivedArgs = cloneArray(args),
        path = null,
        method = null;

    if (typeof receivedArgs[0] === TYPE_STRING) {
        // Read path
        path = receivedArgs.shift();

        if (typeof receivedArgs[0] === TYPE_STRING) {
            // Read method
            method = receivedArgs.shift();
        }
    }

    normalizedArgs = normalizeArguments(receivedArgs);

    if (path !== null) {
        normalizedArgs[API_PARAM_PATH] = path;

        if (method != null) {
            normalizedArgs[API_PARAM_METHOD] = method;
        }
    }

    return normalizedArgs;
}

function handleAsyncCallingError(name, err) {
    var error = createExceptionResponse(name, name, err);
    logError(err.message);
    return createCompletePromise(name, false, null, error);
}

var wl_event = {
    subscribe: function (event, callback) {
        trace("Subscribe " + event);

        var handlers = wl_event.getHandlers(event);
        handlers.push(callback);
    },

    unsubscribe: function (event, callback) {
        trace("Unsubscribe " + event);

        var oldHandlers = wl_event.getHandlers(event);
        var newHandlers = [];

        // Constructs a new list with one callback removed.
        // If callback is not available, we remove all.
        if (callback != null) {
            var found = false;
            for (var i = 0; i < oldHandlers.length; i++) {
                if (found || oldHandlers[i] != callback) {
                    newHandlers.push(oldHandlers[i]);
                } else {
                    found = true;
                }
            }
        }

        wl_event._eHandlers[event] = newHandlers;
    },

    getHandlers: function (event) {

        if (!wl_event._eHandlers) {
            wl_event._eHandlers = {};
        }

        var eHandlers = wl_event._eHandlers[event];
        
        if (eHandlers == null) {
            wl_event._eHandlers[event] = eHandlers = [];
        }

        return eHandlers;
    },

    notify: function (event, data) {
        trace("Notify " + event)

        var handlers = wl_event.getHandlers(event);

        for (var i = 0; i < handlers.length; i++) {
            handlers[i](data);
        }
    }
};

/**
 * The wl_app type encapsulates the implementation of all inteface methods.
 */
var wl_app = { _status: APP_STATUS_NONE, _statusRequests: [] };

/**
 * The implementation of WL.init().
 */
wl_app.appInit = function (properties) {

    // If app has already invoked WL.init(), ignore this call.
    if (wl_app._status == APP_STATUS_INITIALIZED)
        return;

    var sdkRoot = WL[WL_SDK_ROOT];
    if (sdkRoot) {
        if (sdkRoot.charAt(sdkRoot.length - 1) !== "/") {
            sdkRoot += "/";
        }

        wl_app[WL_SDK_ROOT] = sdkRoot;
    }

    var logging = properties[API_PARAM_LOGGING];
    if (logging === false) {
        wl_app._logEnabled = logging;
    }

    wl_app._authScope = normalizeScopeValue(properties[AK_SCOPE]);
    wl_app._secureCookie = normalizeBooleanValue(properties[AK_SECURE_COOKIE]);
    wl_app._status = APP_STATUS_INITIALIZED;

    if (wl_app.testInit) {
        wl_app.testInit(properties);
    }
    
    appInitPlatformSpecific(properties);
};

/**
 * This is the very first method invoked after the script is loaded.
 */
wl_app.onloadInit = function () {
    detectBrowsers();
    handlePageLoad();
};

function ensureAppInited(method) {
    if (wl_app._status === APP_STATUS_NONE) {
        throw new Error(ERROR_DESC_ENSURE_INIT.replace("METHOD", method));
    }
}

function getCoreApp() {
    return WL.Internal.tApp || wl_app;
}

wl_app.api = function (properties) {

    ensureAppInited("WL.api");

    var body = properties[API_PARAM_BODY];
    if (body) {
        properties = cloneObject(flattenApiBody(body), properties);
        delete properties[API_PARAM_BODY];
    }

    var method = properties[API_PARAM_METHOD];
    properties[API_PARAM_METHOD] = ((method != null) ? stringTrim(method) : HTTP_METHOD_GET).toUpperCase();

    return new APIRequest(properties).execute();
};

var generateApiRequestId = function () {
    var ticketNumber = wl_app.api.lastId,
        id;
    ticketNumber = (ticketNumber === undefined) ? 1 : ticketNumber + 1;
    id = "WLAPI_REQ_" + ticketNumber + "_" + (new Date().getTime());
    wl_app.api.lastId = ticketNumber;

    return id;
};

var APIRequest = function (properties) {
    var request = this;
    request._properties = properties;
    request._completed = false;
    request._id = generateApiRequestId();
    properties[API_PARAM_PRETTY] = false;
    properties[API_PARAM_SSLRESOURCE] = wl_app._isHttps;
    properties[API_X_HTTP_LIVE_LIBRARY] = wl_app[API_X_HTTP_LIVE_LIBRARY];

    var path = properties[API_PARAM_PATH];
    request._url = getApiServiceUrl() + (path.charAt(0) === "/" ? path.substring(1) : path);
    request._promise = new Promise("WL.api", null, null);
};

APIRequest.prototype = {
    execute: function () {
        executeApiRequest(this);
        return this._promise;
    },

    onCompleted: function (response) {
        if (this._completed) {
            return;
        }

        this._completed = true;
        invokeCallback(this._properties.callback, response, true/*synchronous*/);

        if (response[AK_ERROR]) {
            this._promise[PROMISE_EVENT_ONERROR](response);
        }
        else {
            this._promise[PROMISE_EVENT_ONSUCCESS](response);
        }
    }
};

function processXDRResponse(request, status, responseText, errorDescription) {

    responseText = responseText ? stringTrim(responseText) : "";
    var response = (responseText !== "") ? deserializeJSON(responseText) : null;
    if (response === null) {
        response = {};
        if ((status / 100) !== 2) {
            response[API_PARAM_ERROR] = createErrorObject(status, errorDescription);
        }
    }

    request.onCompleted(response);
}

function createErrorObject(status, errorDescription) {
    var errorObj = {};
    errorObj[API_PARAM_CODE] = ERROR_REQUEST_FAILED;
    errorObj[API_PARAM_MESSAGE] = (errorDescription || ERROR_DESC_FAIL_CONNECT);

    return errorObj;
}

function getAccessTokenForApi() {

    var token = null,
        status = getCoreApp()._session.getStatus();

    if (status.status === AS_EXPIRING || status.status === AS_CONNECTED) {
        token = status.session[AK_ACCESS_TOKEN];
    }

    return token;
}

function flattenApiBody(body) {
    // If the WL.api body parameter is a nested JSON object, we convert it into a flattened dictionary that has one layer
    // and maps each leaf node value on the original JSON tree hierarchy with a key value joining each sub key on the
    // path with a dot character. E.g. { contact { name: "Lin" } } will be converted into: {"contact.name" : "Lin"}
    // If array is used in the structure, the array index value will be part of the key.
    // E.g. { employmentHistory: [ { employer: "Microsoft", period: "2007-2011"} ] } will output the following entries:
    //  {"employmentHistory.0.employer" : "Microsoft", "employmentHistory.0.period" : "2007-2011" }

    var dict = {};
    for (var key in body) {
        var value = body[key],
            type = typeof(value);

        if (value instanceof Array) {
            for (var i = 0; i < value.length; i++) {
                // Note: we shouldn't have immediate nested array cases.
                var elementValue = value[i],
                    elementValueType = typeof (elementValue);
                if (type == TYPE_OBJECT && !(value instanceof Date)) {
                    var elementDict = flattenApiBody(elementValue);
                    for (var elementSubKey in elementDict) {
                        dict[key + "." + i + "." + elementSubKey] = elementDict[elementSubKey];
                    }
                }
                else {
                    dict[key + "." + i] = elementValue;
                }
            }
        }
        else if (type == TYPE_OBJECT && !(value instanceof Date)) {
            var vDic = flattenApiBody(value);
            for (var subKey in vDic) {
                dict[key + "." + subKey] = vDic[subKey];
            }
        }
        else {
            dict[key] = value;
        }
    }

    return dict;
}

function sendAPIRequestViaXHR(request) {

    if (!canDoXHR()) {
        return false;
    }

    var xdrParams = prepareXDRRequest(request),
        xdr = new XMLHttpRequest();

    xdr.open(xdrParams.method, xdrParams.url, true);
    var requestMethod = request._properties[API_PARAM_METHOD];
    if (xdrParams.method != HTTP_METHOD_GET) {
        xdr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
    }

    xdr.onreadystatechange = function () {
        if (xdr.readyState == 4) {
            processXDRResponse(request, xdr.status, xdr.responseText);
        }
    };

    xdr.send(xdrParams.body);

    return true;
}

function prepareXDRRequest(request) {
    var params = cloneObjectExcept(
            request._properties,
            null,
            [API_PARAM_CALLBACK, API_PARAM_PATH, API_PARAM_METHOD]),
        method = request._properties[API_PARAM_METHOD],
        url = appendUrlParameters(request._url, {'ts': (new Date().getTime())}),
        token = getAccessTokenForApi(),
        reqBody,
        xdrMethod;

    params[API_SUPPRESS_REDIRECTS] = "true";
    // Flash(built with Adobe Flex) and Firefox 3.5/3.6 does not return response content when http status code is not 200.
    // We use suppress_response_codes to indicate this so that the server will always return 200.
    params[API_SUPPRESS_RESPONSE_CODES] = "true";

    if (token != null) {
        params[AK_ACCESS_TOKEN] = token;
    }

    if (method === HTTP_METHOD_GET || method === HTTP_METHOD_DELETE) {
        reqBody = null;
        xdrMethod = HTTP_METHOD_GET;
        url += "&" + serializeParameters(params);        
    }
    else {
        reqBody = serializeParameters(params);
        xdrMethod = HTTP_METHOD_POST;
    }

    url += "&method=" + method;

    return {
        url: url,
        method: xdrMethod,
        body: reqBody
    };
}

// Common shared wl.download method code.
// See wl.app.download.wwa.js or wl.app.download.web.js for platform specific
// details.

wl_app.download = function (properties) {
    validateDownloadProperties(properties);

    ensureAppInited("WL.download");

    return new DownloadOperation(properties).execute();    
};

function buildFilePathUrlString(path, extra_params) {
    var params = extra_params || {},
        baseUrl = getApiServiceUrl();

    if (!isPathFullUrl(path)) {
        path = baseUrl + (path.charAt(0) === "/" ? path.substring(1) : path);
    }

    var token = getAccessTokenForApi();
    if (token) {
        params[AK_ACCESS_TOKEN] = token;
    }

    params[API_X_HTTP_LIVE_LIBRARY] = wl_app[API_X_HTTP_LIVE_LIBRARY];

    return appendUrlParameters(path, params);
}

function handleDownloadErrorResponse(errorMessage, op) {
    op.downloadComplete(false, createErrorResponse(ERROR_REQUEST_FAILED, "WL.download: " + errorMessage));
}

var DOWNLOAD_OPSTATE_NOTSTARTED = "notStarted",
    DOWNLOAD_OPSTATE_READY = "ready",
    DOWNLOAD_OPSTATE_DOWNLOADCOMPLETED = "downloadCompleted",
    DOWNLOAD_OPSTATE_DOWNLOADFAILED = "downloadFailed",
    DOWNLOAD_OPSTATE_CANCELED = "canceled",
    DOWNLOAD_OPSTATE_COMPLETED = "completed";

// DownloadOperation type.
function DownloadOperation(properties) {
    this._properties = properties;
    this._status = DOWNLOAD_OPSTATE_NOTSTARTED;
}

DownloadOperation.prototype = {
    execute: function () {
        this._promise = new Promise("WL.download", this, null);
        this._process();
        return this._promise;
    },

    cancel: function () {
        this._status = DOWNLOAD_OPSTATE_CANCELED;
        if (this._cancel) {
            try {
                this._cancel();
            }
            catch (ex) {
            }
        }
        else {
            this._result = createErrorResponse(ERROR_REQ_CANCEL, ERROR_DESC_CANCEL.replace("METHOD", "WL.download"));
            this._process();
        }
    },

    downloadComplete: function (succeeded, result) {
        var op = this;
        op._result = result;
        op._status = succeeded ? DOWNLOAD_OPSTATE_DOWNLOADCOMPLETED : DOWNLOAD_OPSTATE_DOWNLOADFAILED;
        op._process();
    },

    downloadProgress: function (progress) {
        this._promise[PROMISE_EVENT_ONPROGRESS](progress);
    },

    _process: function () {
        switch (this._status) {
            case DOWNLOAD_OPSTATE_NOTSTARTED:
                this._start();
                break;
            case DOWNLOAD_OPSTATE_READY:
                this._download();
                break;
            case DOWNLOAD_OPSTATE_DOWNLOADCOMPLETED:
            case DOWNLOAD_OPSTATE_DOWNLOADFAILED:
            case DOWNLOAD_OPSTATE_CANCELED:
                this._complete();
                break;
        }
    },

    _start: function () {
        var op = this;
        
        wl_app.getLoginStatus({
            internal: true,
            callback: function () {
                    op._status = DOWNLOAD_OPSTATE_READY;
                    op._process()
                }
        });
    },

    _download: function () {
        var op = this;

        // executeDownload must
        // be defined in both
        // wl.app.download.web.js and wl.app.download.wwa.js
        executeDownload(op);
    },

    _complete: function () {
        var op = this,
            result = op._result,
            promiseEvent = (op._status === DOWNLOAD_OPSTATE_DOWNLOADCOMPLETED) ?
                           PROMISE_EVENT_ONSUCCESS :
                           PROMISE_EVENT_ONERROR;

        op._status = DOWNLOAD_OPSTATE_COMPLETED;

        var callback = op._properties[API_PARAM_CALLBACK];
        if (callback) {
            callback(result);
        }

        op._promise[promiseEvent](result);        
    }
};

/**
 * The implementation of WL.login() method.
 */
wl_app.login = function (properties, internal) {

    ensureAppInited("WL.login");
    
    normalizeLoginScope(properties);

    if (!handlePendingLogin(internal)) {
        return createCompletePromise("WL.login",
                                     false/*succeeded*/, 
                                     null,
                                     createErrorResponse(ERROR_REQUEST_FAILED, ERROR_DESC_PENDING_LOGIN_CONFLICT));
    }

    var response = wl_app._session.tryGetResponse(properties.normalizedScope);
    if (response != null) {
        return createCompletePromise("WL.login", true/*succeeded*/, properties.callback, response);
    }

    wl_app._pendingLogin = createLoginRequest(properties, onAuthRequestCompleted);
    return wl_app._pendingLogin.execute();
}

function onAuthRequestCompleted(requestProperties, response) {
    wl_app._pendingLogin = null;

    var error = response[AK_ERROR];
    if (error) {
        log("WL.login: " + response[AK_ERROR_DESC]);
    }
    else {
        invokeCallback(requestProperties.callback, response, true/*synchronous*/);
    }
}

function normalizeScopeValue(scopeValue) {

    var scope = scopeValue || "";
    if (scope instanceof Array) {
        scope = scope.join(" ");
    }

    return stringTrim(scope);
}

/**
 * The implementation of WL.getSession() method.
 */
wl_app.getSession = function () {
    ensureAppInited("WL.getSession");
    return wl_app._session.getStatus()[AK_SESSION];
};

/**
 * The implementation of WL.getLoginStatus() method.
 */
wl_app.getLoginStatus = function (properties, force) {

    ensureAppInited("WL.getLoginStatus");

    properties = properties || {};

    if (!force) {
        var response = wl_app._session.tryGetResponse();
        if (response) {
            return createCompletePromise("WL.getLoginStatus", true/*succeeded*/, properties.callback, response);
        }
    }

    trace("wl_app:getLoginStatus");

    var pendingQueue = wl_app._statusRequests,
        request = null;

    if (!wl_app._pendingStatusRequest) {
        request = createLoginStatusRequest(properties, onGetLoginStatusCompleted);
        wl_app._pendingStatusRequest = request;
    }

    pendingQueue.push(properties);

    if (request != null) {
        request.execute(); 
    }

    return wl_app._pendingStatusRequest._promise;
}

function onGetLoginStatusCompleted(requestProperties, response) {
    var pendingQueue = wl_app._statusRequests;
    wl_app._pendingStatusRequest = null;

    trace("wl_app:onGetLoginStatusCompleted");

    var error = response[AK_ERROR],
        hasAppRequest = false;

    while (pendingQueue.length > 0) {
        var reqProperties = pendingQueue.shift(),
            responseForCallback = cloneObject(response);
        if (!error || reqProperties.internal) {
            invokeCallback(reqProperties.callback, responseForCallback, true/*synchronous*/);
        }

        if (!reqProperties.internal) {
            hasAppRequest = true;
        }
    }

    if (error) {
        if (hasAppRequest && error !== ERROR_TIMEDOUT) {
            log("WL.getLoginStatus: " + response[AK_ERROR_DESC]);
        }
        else {
            trace("wl_app-onGetLoginStatusCompleted: " + response[AK_ERROR_DESC]);            
        }
    }
}


/**
 * The implementation of WL.logout() method.
 */
wl_app.logout = function (properties) {

    ensureAppInited("WL.logout");

    var promise = new Promise("WL.logout", null, null);

    var f = function () {
        var resp = authSession.getNormalStatus();
        invokeCallback(properties.callback, resp, false/*synchronous*/);
        promise[PROMISE_EVENT_ONSUCCESS](resp);
    };

    var authSession = wl_app._session;
    if (authSession.isSignedIn()) {
        if (wl_app.canLogout()) {
            authSession.updateStatus(AS_UNKNOWN);
            logoutWindowsLive(f);
        }
        else {
            throw new Error(ERROR_DESC_LOGOUT_NOTSUPPORTED);
        }
    } else {
        delayInvoke(f);
    }

    return promise;
};

function handleUIParameterError(properties, err) {
    logError(err.message);
    var onerror = properties[UI_PARAM_ONERROR];
    if (onerror) {
        delayInvoke(function () {
            error = createExceptionResponse("WL.ui", "WL.ui", err),
            onerror(error);
        });
    }
}

var SignInControl = function (properties) {

    var control = this;

    control._properties = properties;

    var signInControlInit = createDelegate(control, control.init);

    checkDocumentReady(signInControlInit);    
};

SignInControl.prototype = {
    init: function () {
        var control = this,
            properties = control._properties;

        if (control._inited === true) {
            return;
        }

        control._inited = true;

        try {
            control.validate();
            element = properties[UI_PARAM_ELEMENT],
            type = properties[UI_PARAM_TYPE],
            callback = properties[API_PARAM_CALLBACK],
            signinText = properties[UI_PARAM_SIGN_IN_TEXT],
            signoutText = properties[UI_PARAM_SIGN_OUT_TEXT];
            
            normalizeSignInControlScope(properties);
            
            element = (typeof (element) === TYPE_STRING) ? getElementById(properties[UI_PARAM_ELEMENT]) : element;
            control._element = element;

            type = type != null ? type : UI_SIGNIN_TYPE_SIGNIN;
            if (type == UI_SIGNIN_TYPE_SIGNIN) {
                signinText = WLText.signIn;
                signoutText = WLText.signOut;
            }
            else if (type == UI_SIGNIN_TYPE_LOGIN) {
                signinText = WLText.login;
                signoutText = WLText.logout;
            }
            else if (type == UI_SIGNIN_TYPE_CONNECT) {
                signinText = WLText.connect;
                signoutText = WLText.signOut;
            }

            control[UI_PARAM_SIGN_IN_TEXT] = signinText;
            control[UI_PARAM_SIGN_OUT_TEXT] = signoutText;

            setInnerHtml(element, buildSignInControlHtml(properties));

            var isSignedIn = wl_app._session.isSignedIn(),
                buttonText = isSignedIn ? signoutText : signinText;
            control.updateUI(buttonText, isSignedIn);

            attachSignInControlMouseEvents(control, element.childNodes[0]);

            wl_event.subscribe(EVENT_AUTH_LOGIN, createDelegate(control, control.onLoggedIn));
            wl_event.subscribe(EVENT_AUTH_LOGOUT, createDelegate(control, control.onLoggedOut));

            wl_app.getLoginStatus({ internal: true });

            // The callback should only be invoked once for rendering complete.
            // To avoid conflict with the login callback parameter, remove it here.
            delete properties[API_PARAM_CALLBACK];

            invokeCallback(callback, properties, false/*synchronous*/);
        }
        catch (e) {
            handleUIParameterError(properties, e);
        }
    },

    validate: function () {
        var properties = this._properties;
        validateProperties(
            properties,
            [{
                name: UI_PARAM_ELEMENT,
                type: TYPE_DOM,
                optional: false
            },
             {
                 name: UI_PARAM_TYPE,
                 allowedValues: [UI_SIGNIN_TYPE_SIGNIN, UI_SIGNIN_TYPE_LOGIN, UI_SIGNIN_TYPE_CONNECT, UI_SIGNIN_TYPE_CUSTOM],
                 type: TYPE_STRING,
                 optional: true
             },
             { name: AK_SCOPE, type: TYPE_STRINGORARRAY, optional: true },
             { name: AK_STATE, type: TYPE_STRING, optional: true },
             { name: UI_PARAM_ONLOGGEDIN, type: TYPE_FUNCTION, optional: true },
             { name: UI_PARAM_ONLOGGEDOUT, type: TYPE_FUNCTION, optional: true },
             { name: UI_PARAM_ONERROR, type: TYPE_FUNCTION, optional: true }
            ],
            "WL.ui-signin");

        validateSignInControlPlatformSpecificParameters(properties);

        // Validate custom sign-in control text values
        var type = properties[UI_PARAM_TYPE];
        if (type == UI_SIGNIN_TYPE_CUSTOM) {
            validateProperties(
                properties,
                [{
                    name: UI_PARAM_SIGN_IN_TEXT,
                    type: TYPE_STRING,
                    optional: false
                },
                 {
                     name: UI_PARAM_SIGN_OUT_TEXT,
                     type: TYPE_STRING,
                     optional: false
                 }
                ],
                "WL.ui-signin");
        }
    },

    fireEvent: function (eventName, args) {
        var callback = this._properties[eventName];
        if (callback) {
            callback(args);
        }
    },

    onClick: function () {
        if (this._element.childNodes.length == 0) {
            // The button has been cleared.
            detachSignInControlMouseEvents(this);
            return;
        }

        if (wl_app._session.isSignedIn()) {
            if (wl_app.canLogout()) {
                wl_app.logout({});
            }
        }
        else {
            wl_app.login(this._properties, true/*internal*/).then(
                function (result) { },
                function (result) {
                    this.fireEvent(UI_PARAM_ONERROR, result);
                });
        }

        return false;
    },

    onLoggedIn: function (e) {
        this.updateUI(this[UI_PARAM_SIGN_OUT_TEXT], true/*isSignedIn*/);
        this.fireEvent(UI_PARAM_ONLOGGEDIN, e);
    },

    onLoggedOut: function (e) {
        this.updateUI(this[UI_PARAM_SIGN_IN_TEXT], false/*isSignedIn*/);
        this.fireEvent(UI_PARAM_ONLOGGEDOUT, e);
    }
};

function normalizeSignInControlScope(properties) {
    if (wl_app._authScope && wl_app._authScope !== "") {
        // We use the scope values passed in from WL.init.
        // If it isn't available, the scope value from SignInControl will be used for backward compatibility.
        properties[AK_SCOPE] = wl_app._authScope;
    }

    if (!properties[AK_SCOPE]) {
        // If no scope is available, we use wl.signin as default for auth UI flow.
        properties[AK_SCOPE] = SCOPE_SIGNIN;
    }
}

function getSDKRootPath() {
    return wl_app[WL_SDK_ROOT];
}

function getImagePath() {
    return getSDKRootPath() + "images";
}

function createSignInControlEventHandler(name, control, callback) {
    control._handlers = control._handlers || {};
    var handler = createDelegate(control, callback);
    control._handlers[name] = handler;
    return handler;
}

function getSignInControlEventHandler(name, control) {
    return control._handlers[name];
}

// wl.app.upload.js
// Common WL.upload methods.

wl_app.upload = function (properties) {
    var method = properties[API_INTERFACE_METHOD];
    ensureAppInited(method);

    validateProperties(
        properties,
        [{ name: API_PARAM_PATH, type: TYPE_STRING, optional: false },
         expectedCallback_Optional],
        method);

    validateUploadOverwriteProperty(properties);

    normalizeUploadFileName(properties);

    return new UploadOperation(properties).execute();
}

function normalizeUploadFileName(properties) {
    var file = properties[API_PARAM_FILEINPUT],
     fileName = properties[API_PARAM_FILENAME];
    if (file) {
        properties[API_PARAM_FILENAME] = fileName || file.name;
    }
}

function buildUploadToFolderUrlString(location, file_name, overwrite) {
    var hasFileName = typeof(file_name) !== TYPE_UNDEFINED;
    var hasTrailingSlash = location.charAt(location.length-1) === "/";
    if (hasFileName && !hasTrailingSlash) {
        location += "/";
    }

    var path = location,
        params = {};

    // the file_name for Form multipart uploads are undefined, and should NOT be included.
    if (hasFileName) {
        path += encodeURIComponent(file_name);
    }

    // We only apply "overwrite" parameter when upload to a folder.    
    if (overwrite === "rename") {
        // the API Service's rename value is choosenewname
        params[API_PARAM_OVERWRITE] = "choosenewname";
    } else {  // if overwrite is true or false
        params[API_PARAM_OVERWRITE] = overwrite;
    }
    
    return buildUploadFileUrlString(path, params);
}

function isFilePath(path) {
    return /^(file|\/file)/.test(path.toLowerCase());
}

function buildUploadFileUrlString(path, params) {
    params = params || {};
    params[API_SUPPRESS_RESPONSE_CODES] = "true";

    return buildFilePathUrlString(path, params);
}

function validateUploadOverwriteProperty(properties) {
    // Overwrite is an optional parameter, that can be a boolean or a string with values:
    // "true", "false", or "rename".
    // "rename" will rename the file with a suffix if the file already exists. (e.g.,
    // uploading foo.txt when foo.txt already exists will be renamed to foo(1).txt).
    if (API_PARAM_OVERWRITE in properties) {
        var interfaceMethod = properties[API_INTERFACE_METHOD],
            overwrite = properties[API_PARAM_OVERWRITE],
            type = typeof(overwrite),
            isBoolean = type === TYPE_BOOLEAN,
            isString = type === TYPE_STRING;
        if (!(isBoolean || isString)) {
            throw createParamTypeError(API_PARAM_OVERWRITE, interfaceMethod);
        }

        if (isString) {
            // the overwrite parameter can be "true", "false", or "rename".
            var hasValidValue = (/^(true|false|rename)$/i).test(overwrite);
            if (!hasValidValue) {
                throw createInvalidParamValue(API_PARAM_OVERWRITE, interfaceMethod);
            }
        }
    } else {
        // if it does not have overwrite, the default value for it is false.
        // This is to be consistent with other platforms.
        properties[API_PARAM_OVERWRITE] = false;
    }
}

var UPLOAD_OPSTATE_NOTSTARTED = 0,
    UPLOAD_OPSTATE_AUTHREADY = 1,
    UPLOAD_OPSTATE_UPLOADREADY = 2,
    UPLOAD_OPSTATE_UPLOADCOMPLETED = 3,
    UPLOAD_OPSTATE_UPLOADFAILED = 4,
    UPLOAD_OPSTATE_CANCELED = 5,
    UPLOAD_OPSTATE_COMPLETED = 6;

function UploadOperation(props) {
    this._props = props;
    this._status = UPLOAD_OPSTATE_NOTSTARTED;
}

UploadOperation.prototype = {
    execute: function () {
        var self = this;
        self._strategy = self._getStrategy(self._props);
        self._promise = new Promise(self._props[API_INTERFACE_METHOD], self, null);
        self._process();
        return self._promise;
    },

    cancel: function () {
        var self = this;
        self._status = UPLOAD_OPSTATE_CANCELED;

        if (self._cancel) {
            try {
                self._cancel();
            }
            catch (ex) {
            }
        }
        else {
            var errorDescription = ERROR_DESC_CANCEL.replace(METHOD, self._props[API_INTERFACE_METHOD]);
            self._result = createErrorResponse(ERROR_REQ_CANCEL, errorDescription);
            self._process();
        }
    },

    uploadProgress: function (progress) {
        this._promise[PROMISE_EVENT_ONPROGRESS](progress);
    },

    uploadComplete: function (succeeded, result) {
        var self = this;
        self._result = result;
        self._status = succeeded ? UPLOAD_OPSTATE_UPLOADCOMPLETED : UPLOAD_OPSTATE_UPLOADFAILED;
        self._process();
    },

    onErr: function (errorMessage) {
        var errorDescription = this._props[API_INTERFACE_METHOD] + ":" + errorMessage,
            errorResponse = createErrorResponse(ERROR_REQUEST_FAILED, errorDescription);
        this.uploadComplete(false, errorResponse);
    },

    onResp: function (responseText) {
        responseText = responseText ? stringTrim(responseText) : "";
        var response = (responseText !== "") ? deserializeJSON(responseText) : null;
        response = response || {};

        this.uploadComplete((response.error == null), response);
    },

    setFileName: function (fileName) {
        this._props[API_PARAM_FILENAME] = fileName;
    },

    _process: function () {
        switch (this._status) {
            case UPLOAD_OPSTATE_NOTSTARTED:
                this._start();
                break;
            case UPLOAD_OPSTATE_AUTHREADY:
                this._getUploadPath();
                break;
            case UPLOAD_OPSTATE_UPLOADREADY:
                this._upload();
                break;
            case UPLOAD_OPSTATE_UPLOADCOMPLETED:
            case UPLOAD_OPSTATE_UPLOADFAILED:
            case UPLOAD_OPSTATE_CANCELED:
                this._complete();
                break;
        }
    },

    _start: function () {
        var op = this;
        getCoreApp().getLoginStatus({
            internal: true,
            callback: function () {
                op._status = UPLOAD_OPSTATE_AUTHREADY;
                op._process()
            }
        });
    },

    _getUploadPath: function () {
        var op = this,
            props = op._props,
            path = props[API_PARAM_PATH];

        if (isPathFullUrl(path)) {
            op._uploadPath = buildUploadFileUrlString(path);
            op._status = UPLOAD_OPSTATE_UPLOADREADY;
            op._process();
            return;
        }

        getCoreApp().api({ path: path }).then(
            function (response) {
                var location = response.upload_location;

                if (location) {
                    var file_name = props[API_PARAM_FILENAME],
                        overwrite = props[API_PARAM_OVERWRITE];
                    if (isFilePath(path)) {
                        op._uploadPath = buildUploadFileUrlString(location);
                    } 
                    else {
                        op._uploadPath = buildUploadToFolderUrlString(location, file_name, overwrite);
                    }

                    op._status = UPLOAD_OPSTATE_UPLOADREADY;
                }
                else {
                    op._result = createErrorResponse(ERROR_REQUEST_FAILED, "WL.upload: Failed to get upload_location of the resource."); // TODO: move to wl.constant.js
                    op._status = UPLOAD_OPSTATE_UPLOADFAILED;
                }

                op._process();
            },
            function (error) {
                op._result = error;
                op._status = UPLOAD_OPSTATE_UPLOADFAILED;
                op._process();
            }
        );
    },

    _upload: function () {
        this._strategy.upload(this._uploadPath);
    },

    _complete: function () {
        var op = this,
            result = op._result,
            promiseEvent = (op._status === UPLOAD_OPSTATE_UPLOADCOMPLETED) ?
                            PROMISE_EVENT_ONSUCCESS : PROMISE_EVENT_ONERROR;

        op._status = UPLOAD_OPSTATE_COMPLETED;

        var callback = op._props[API_PARAM_CALLBACK];
        if (callback) {
            callback(result);
        }

        op._promise[promiseEvent](result);
    }
};

function stringTrim(value) {
    return value.replace(/^\s+|\s+$/g, '');
}

function stringsAreEqualIgnoreCase(str1, str2) {
    if (str1 && str2) {
        return str1.toLowerCase() === str2.toLowerCase();
    }

    return str1 === str2;
}

function stringIsNullOrEmpty(s) {
    return s == null || s === "";
}

/**
 * Create a cloned object.(one level clone)
 */
function cloneObject(obj, target) {
    var clonedObj = target || {};
    if (obj != null) {
        for (var key in obj)
            clonedObj[key] = obj[key];
    }
    return clonedObj;
}

/**
 * Create a cloned object and remove some properties.
 */
function cloneObjectExcept(obj, target, exceptionlist) {
    var clonedObject = cloneObject(obj, target);
    for (var i = 0; i < exceptionlist.length; i++) {
        delete clonedObject[exceptionlist[i]];
    }

    return clonedObject;
}

/**
 * Checks if an array contains a given object.
 */
function arrayContains(arr, obj) {
    var i;
    for (i = 0; i < arr.length; i++) {
        if (arr[i] === obj) {
            return true;
        }
    }

    return false;
}

/**
 * Merge two arrays into one and avoid duplicate.
 */
function arrayMerge(arr1, arr2) {
    var arr = [], arrMap = {};

    var addToArray = function (elements) {
        for (var i = 0; i < elements.length; i++) {
            arrElm = elements[i];
            if (arrElm != "" && !arrMap[arrElm]) {
                arrMap[arrElm] = true;
                arr.push(arrElm);
            }
        }
    };

    addToArray(arr1);
    addToArray(arr2);

    return arr;
}

/**
 * Create a cloned array.
 */
function cloneArray(array) {
    return Array.prototype.slice.call(array);
}

/**
 * Create a delegate for a instance method.
 */
function createDelegate(instance, method) {
    return function () {
        if (typeof (method) === TYPE_FUNCTION) {
            return method.apply(instance, arguments);
        }
    }
}

/**
* Log message into console
*/
function writeLog(text, type) {
    text = "[WL]" + text;
    var c = window.console;
    if (c && c.log) {
        switch (type) {
            case "warning":
                c.warn(text);
                break;
            case "error":
                c.error(text);
                break;
            default:
                c.log(text);
        }
    }

    var o = window.opera;
    if (o) {
        o.postError(text);
    }

    var d = window.debugService;
    if (d) {
        d.trace(text);
    }
}

function isPathFullUrl(path) {
    return path.indexOf("https://") === 0 || path.indexOf("http://") === 0;
}

function trace(text) {
    if (wl_app._traceEnabled) {
        writeLog(text);
    }
}

function log(text, type) {
    if (wl_app._logEnabled || wl_app._traceEnabled) {
        writeLog(text, type);
    }

    wl_event.notify(EVENT_LOG, text);
}

if (window.WL && WL.Internal) {
    WL.Internal.trace = trace;
    WL.Internal.log = log;
}

function logError(text) {
    log(text, "error");
}

function createHiddenIframe(url, id) {
    var iframe = createHiddenElement("iframe");
    iframe.id = id;
    iframe.src = url;
    document.body.appendChild(iframe);

    return iframe;
}

function createHiddenElement(tagName) {
    var element = document.createElement(tagName);
    element.style.position = "absolute";
    element.style.top = "-1000px";
    element.style.width = "300px";
    element.style.height = "300px";
    element.style.visibility = "hidden";

    return element;
}

function createUniqueElementId() {

    var id = null;

    while (id == null) {
        id = "wl_" + Math.floor(Math.random() * 1024 * 1024);

        if (getElementById(id) != null) {
            id = null;
        }
    }

    return id;
}

function getElementById(id) {
    return document.getElementById(id);
}

function setElementText(element, text) {
    if (element) {
        if (element.innerText) {
            element.innerText = text;
        }
        else {
            // Firefox does not have innerText property. So, we do it differently.
            var textNode = document.createTextNode(text);
            element.innerHTML = '';
            element.appendChild(textNode);
        }
    }
}

function Uri(url) {
    cloneObject(parseUri(url), this);
}

Uri.prototype = {
    toString: function () {
        var uri = this;
        s = (uri.scheme != "" ? uri.scheme + "//" : "") + uri.host + (uri.port != "" ? ":" + uri.port : "") + uri.path;
        return s;
    },

    resolve: function () {
        var uri = this;

        if (uri.scheme == "") {
            var port = window.location.port,
                host = window.location.host;

            uri.scheme = window.location.protocol;
            uri.host = host.split(":")[0];
            uri.port = port != null ? port : "";

            if (uri.path.charAt(0) != "/") {
                uri.path = resolveRelativePath(uri.host, window.location.href, uri.path);
            }
        }
    }
};

function parseUri(url) {
    // Assume url never be null or empty.

    var scheme = (url.indexOf(SCHEME_HTTPS) == 0) ? SCHEME_HTTPS : (url.indexOf(SCHEME_HTTP) == 0) ? SCHEME_HTTP : "",
        host = "",
        port = "",
        path;

    if (scheme != "") {
        var urlPart = url.substring(scheme.length + 2),
            hostEnd = urlPart.indexOf("/"),
            hostPortStr = (hostEnd > 0) ? urlPart.substring(0, hostEnd) : urlPart,
            hostport = hostPortStr.split(":"),
            host = hostport[0],
            port = (hostport.length > 1) ? hostport[1] : "",
            path = (hostEnd > 0) ? urlPart.substring(hostEnd) : "";
    }
    else {
        path = url;
    }

    return { scheme: scheme, host: host, port: port, path: path };
}

function getDomainName(url) {
    return parseUri(url.toLowerCase()).host;
}

function resolveRelativePath(hostname, href, url) {
    var trimRight = function (url, char) {
        charIdx = href.indexOf(char);
        url = (charIdx > 0) ? url.substring(0, charIdx) : url;
        return url;
    };

    href = trimRight(trimRight(href, "?"), "#");

    var hostIndex = href.indexOf(hostname),
        path = href.substring(hostIndex + hostname.length),
        pathIdx = path.indexOf("/"),
        folderIndex = path.lastIndexOf('/');
    path = (folderIndex >= 0) ? path.substring(pathIdx, folderIndex) : path;

    return path + "/" + url;
}

function trimUrlQuery(url) {
    var queryStart = url.indexOf("?");
    if (queryStart > 0) {
        url = url.substring(0, queryStart);
    }

    queryStart = url.indexOf("#");
    if (queryStart > 0) {
        url = url.substring(0, queryStart);
    }

    return url;
}

function invokeCallback(callback, resp, synchronous, state) {
    if (callback != null) {

        if (state) {
            resp[AK_STATE] = state;
        }

        if (synchronous) {
            callback(resp);
        }
        else {
            delayInvoke(
                function () {
                    callback(resp); 
                }
            );
        }
    }
}

function deserializeJSON(text) {
    if (window.JSON) {
        return JSON.parse(text);
    }
    else {
        return eval("(" + text + ")");
    }
}

function getCurrentSeconds() {
    // Get current timestamp in seconds
    return Math.floor(new Date().getTime() / 1000);
}

function foreach(elementList, processElement) {
    var count = elementList.length;
    for (var i = 0; i < count; i++) {
        processElement(elementList[i]);
    }
}

function createAuthError(error, description) {
    var errorObj = {};
    errorObj[AK_ERROR] = error;
    errorObj[AK_ERROR_DESC] = description;
    return errorObj;
}

function createErrorResponse(code, message) {
    var errorObj = {}, errorResponse = {};

    errorObj[API_PARAM_CODE] = code,
    errorObj[API_PARAM_MESSAGE] = message;
    errorResponse[API_PARAM_ERROR] = errorObj;

    return errorResponse;
}

function createExceptionResponse(opName, event, err) {
    return createErrorResponse(
        ERROR_REQUEST_FAILED,
        ERROR_DESC_EXCEPTION.replace("METHOD", opName).replace("EVENT", event).replace("MESSAGE", err.message)
        );
}

function trimVersionBuildNumber(version) {
    var versionArr = version.split(".");
    return versionArr[0] + "." + versionArr[1];
}

function delayInvoke(callback, delay) {

    if (window.wlUnitTests) {
        wlUnitTests.delayInvoke(callback);
    }
    else {
        window.setTimeout(callback, delay || 1);
    }
}

function detectBrowsers() {
    var browser = getBrowserInfo(navigator.userAgent, document.documentMode),
        libraryValue = wl_app[API_X_HTTP_LIVE_LIBRARY];
    wl_app._browser = browser;
    wl_app[API_X_HTTP_LIVE_LIBRARY] = libraryValue.replace("DEVICE", browser.device);
}

function getBrowserInfo(ua, documentMode) {
    ua = ua.toLowerCase();
    var device = "other",
        browser = {
        "firefox": /firefox/.test(ua),
        "firefox1.5": /firefox\/1\.5/.test(ua),
        "firefox2": /firefox\/2/.test(ua),
        "firefox3": /firefox\/3/.test(ua),
        "firefox4": /firefox\/4/.test(ua),
        "ie": /msie/.test(ua) && !/opera/.test(ua),
        "ie6": false,
        "ie7": false,
        "ie8": false,
        "ie9": false,
        "ie10": false,
        "opera": /opera/.test(ua),
        "webkit": /webkit/.test(ua),
        "chrome": /chrome/.test(ua),
        "mobile": /mobile/.test(ua) || /phone/.test(ua)
    };

    if (browser["ie"]) {
        // detect the rendering engine IE is using.
        // if documentMode is defined, we rely on its value to determine the rendering engine.
        var engine = 0;

        if (documentMode) {
            engine = documentMode;
        }
        else {
            // if we're in a browser that doesn't support documentMode (IE6, IE7) we need to do some more sniffing.
            if (/msie 7/.test(ua)) {
                engine = 7;
            }
        }

        // clamp the engine on 6,9
        engine = Math.min(10, Math.max(engine, 6));
        device = "ie" + engine;

        browser[device] = true;
    }
    else {
        if (browser.firefox) {
            device = "firefox";
        }
        else if (browser.chrome) {
            device = "chrome";
        }
        else if (browser.webkit) {
            device = "webkit";
        }
        else if (browser.opera) {
            device = "opera";
        }
    }

    if (browser.mobile) {
        device += "mobile";
    }

    browser.device = device;
    return browser;
}

/**
 * Deserializes name/value pair parameters from a string into a dictionary object.
 */
function deserializeParameters(value, existingDict) {
    var dict = (existingDict != null) ? existingDict : {};

    if (value != null) {
        var properties = value.split('&');
        for (var i = 0; i < properties.length; i++) {
            var property = properties[i].split('=');
            if (property.length == 2) {
                dict[decodeURIComponent(property[0])] = decodeURIComponent(property[1]);
            }
        }
    }

    return dict;
}

/**
 * Serializes a dictionary object into a string value in a format n1=v1&n2=v2&... 
 */
function serializeParameters(dict) {
    var serialized = "";
    if (dict != null) {
        for (var key in dict) {
            var separator = (serialized.length == 0) ? "" : "&";
            var value = dict[key];
            serialized += separator + encodeURIComponent(key) + "=" + encodeURIComponent(stringifyParamValue(value));
        }
    }

    return serialized;
}

/**
 * Serializes a value into string.
 */
function stringifyParamValue(v) {

    if (v instanceof Date) {
        var padding = function (n, c) {
            switch (c) {
                case 2:
                    return n < 10 ? '0' + n : n;
                case 3:
                    return (n < 10 ? '00' : (n < 100 ? '0' : '')) + n;
            }
        };

        return v.getUTCFullYear() + '-' +
            padding(v.getUTCMonth() + 1, 2) + '-' +
            padding(v.getUTCDate(), 2) + 'T' +
            padding(v.getUTCHours(), 2) + ':' +
            padding(v.getUTCMinutes(), 2) + ':' +
            padding(v.getUTCSeconds(), 2) + '.' +
            padding(v.getUTCMilliseconds(), 3) + 'Z';
    }

    return "" + v;
}

/**
 * Read Url parameters.
 */
function readUrlParameters(url) {

    var queryStart = url.indexOf('?') + 1,
        hashStart = url.indexOf('#') + 1,
        dict = {};

    if (queryStart > 0) {
        var queryEnd = (hashStart > queryStart) ? (hashStart - 1) : url.length;
        dict = deserializeParameters(url.substring(queryStart, queryEnd), dict);
    }
    
    if (hashStart > 0) {
        dict = deserializeParameters(url.substring(hashStart), dict);
    }

    return dict;
}

/**
 * Appends an object of parameters to the base url.
 * The function checks the base url for existing params
 * and appropriately appends a ? or an & to the base url
 * before appending the parameters
 */
function appendUrlParameters(path, params) {
    return path + ((path.indexOf("?") < 0) ? "?" : "&") + serializeParameters(params);
}

/**
 * Normalize a parameter value into boolean type.
 */
function normalizeBooleanValue(value) {
    switch (typeof (value)) {
        case TYPE_BOOLEAN:
            return value;
        case TYPE_NUMBER:
            return !!value;
        case TYPE_STRING:
            return value.toLowerCase() === "true";
        default:
            return false;
    }
}

function validateParams(params, expectedParams, method) {
    if (params instanceof Array) {
        for (var i = 0; i < params.length; i++) {
            validateParam(params[i], expectedParams[i], method); 
        }
    } 
    else {
        validateParam(params, expectedParams, method);
    }
}

function validateParam(param, expectedParam, method) {
    validateParamType(param, expectedParam, method);

    if (expectedParam.type === "properties") {
        validateProperties(param, expectedParam.properties, method);
    }
}

function validateParamType(param, expected, method) {
    var paramName = expected.name,
        paramType = typeof (param),
        expectedType = expected.type;

    if (paramType === "undefined" || param == null) {
        if (expected.optional) {
            return;
        }
        else {
            throw createMissingParamError(paramName, method);
        }
    }

    switch (expectedType) {
        case "string":
        case "url":
            {
                if (paramType !== TYPE_STRING) {
                    throw createParamTypeError(paramName, method);                    
                }
                if (!expected.optional && stringTrim(param) === "") {
                    throw createMissingParamError(paramName, method);
                }
            }
            break;
        case "properties":
            {
                if (paramType != TYPE_OBJECT) {
                    throw createParamTypeError(paramName, method);
                }
            }
            break;
        case "function":
            {
                if (paramType != TYPE_FUNCTION) {
                    throw createParamTypeError(paramType, method);                    
                }
            }
            break;
        case "dom":
            {
                if (paramType == TYPE_STRING) {
                    if (getElementById(param) == null) {
                        throw new Error(ERROR_DESC_DOM_INVALID.replace("METHOD", method).replace("PARAM", paramName));
                    }
                }
                else if (paramType != TYPE_OBJECT) {
                    throw createParamTypeError(paramName, method);
                }
            }
            break;
        case "string_or_array":
            {
                if (paramType !== TYPE_STRING && !(param instanceof Array)) {
                    throw createParamTypeError(paramType, method);
                }
            }
            break;
        default:
            if (paramType !== expectedType) {
                throw createParamTypeError(paramName, method);
            }
            break;
    }
    
    if (expected.allowedValues != null) {
         validateAllowedValue(param, expected.allowedValues, expected.caseSensitive, paramName, method);
    }
}

function validateProperties(param, expectedProperties, method) {
    var properties = param || {};
    for (var i = 0; i < expectedProperties.length; i++) {
        var expectedProperty = expectedProperties[i],
            actualProperty = properties[expectedProperty.name] || properties[expectedProperty.altName];
        validateParamType(actualProperty, expectedProperty, method);
    }
}

function validateAllowedValue(value, allowedValues, caseSensitive, paramName, method) {
    var isString = typeof (allowedValues[0]) === TYPE_STRING;
    for (var i = 0; i < allowedValues.length; i++) {
        if (isString && !caseSensitive) {
            if (value.toLowerCase() === allowedValues[i].toLowerCase()) {
                return;
            }
        }
        else if (value === allowedValues[i]) {
            return;
        }
    }

    throw createInvalidParamValue(paramName, method);    
}

function createParamTypeError(paramName, method) {
    return new Error(ERROR_DESC_PARAM_TYPE_INVALID.replace("METHOD", method).replace("PARAM", paramName));
}

function createMissingParamError(paramName, method) {
    return new Error(ERROR_DESC_PARAM_MISSING.replace("METHOD", method).replace("PARAM", paramName));
}

function createInvalidParamValue(paramName, method, optionalMessage) {
    var message = ERROR_DESC_PARAM_INVALID.replace("METHOD", method).replace("PARAM", paramName);
    if (typeof(optionalMessage) !== TYPE_UNDEFINED) {
        message += " " + optionalMessage;
    }

    return new Error(message);
}

function findArgumentByType(args, type, maxToRead) {
    if (args) {
        for (var i = 0; i < maxToRead && i < args.length; i++) {
            if (type === typeof args[i]) {
                return args[i];
            }
        }
    }

    return undefined;
}

var Promise = function (opName, op, uplinkPromise) {
    this._name = opName;
    this._op = op; 
    this._uplinkPromise = uplinkPromise;
    this._isCompleted = false;
    this._listeners = [];
};

Promise.prototype = {
    then: function (onSuccess, onError, onProgress) {
        var chainedPromise = new Promise(null, null, this),
            listener = {};
        listener[PROMISE_EVENT_ONSUCCESS] = onSuccess;
        listener[PROMISE_EVENT_ONERROR] = onError;
        listener[PROMISE_EVENT_ONPROGRESS] = onProgress;
        listener.chainedPromise = chainedPromise;

        this._listeners.push(listener);

        return chainedPromise;
    },

    cancel: function () {
        if (this._isCompleted) return;

        if (this._uplinkPromise && !this._uplinkPromise._isCompleted) {
            // If there is incomplete uplink promise, we cancel that one and let the flow propagate to this one.
            this._uplinkPromise.cancel();
        }
        else {
            // We need to cancel the current one, if we can.
            var opCancel = (this._op) ? this._op.cancel : null;
            if (typeof (opCancel) === TYPE_FUNCTION) {
                this._op.cancel();
            }
            else {
                this.onError(
                    createErrorResponse(ERROR_REQ_CANCEL, ERROR_DESC_CANCEL.replace("METHOD", this._getName()))
                );
            }
        }
    },

    _getName: function () {
        
        if (this._name) {
            return this._name;
        }

        if (this._op && typeof (this._op._getName) === TYPE_FUNCTION) {
            return this._op._getName();
        }

        if (this._uplinkPromise) {
            return this._uplinkPromise._getName();
        }

        return "";
    },

    _onEvent: function (args, name) {
        if (this._isCompleted) return;
        this._isCompleted = (name !== PROMISE_EVENT_ONPROGRESS);

        this._notify(args, name);
    },

    _notify: function (args, event) {
        var currentPromise = this;
        foreach(this._listeners, function (listener) {
            var callback = listener[event],
                chainedPromise = listener.chainedPromise,
                isPromiseCompleted = (event !== PROMISE_EVENT_ONPROGRESS);

            if (callback) {                
                try {
                    var chainedPromiseOrigin = callback.apply(listener, args);
                    if (isPromiseCompleted && chainedPromiseOrigin && chainedPromiseOrigin.then) {
                        // We need to link and fulfill the chained promise with the one returned from callback
                        // if this is onSuccess or onError. 
                        // Also, set the new promise as the _op of the chained promise in case cancel is invoked.
                        chainedPromise._op = chainedPromiseOrigin;
                        chainedPromiseOrigin.then(
                            function (result) {
                                chainedPromise[PROMISE_EVENT_ONSUCCESS](result);
                            },
                            function (error) {
                                chainedPromise[PROMISE_EVENT_ONERROR](error);
                            },
                            function (progress) {
                                chainedPromise[PROMISE_EVENT_ONPROGRESS](progress);
                            }
                        );
                    }
                }
                catch (err) {
                    if (isPromiseCompleted) {
                        // The the callback throws an error, that should be forwarded to the chained promise.
                        chainedPromise.onError(
                            createExceptionResponse(currentPromise._getName(), event, err)
                        );
                    }
                }
            }
            else {
                if (isPromiseCompleted) {
                    // If no onSuccess/onError is handled, we forward event to the chained promise.
                    chainedPromise[event].apply(chainedPromise, args);
                }
            }
        });
    }
};

Promise.prototype[PROMISE_EVENT_ONSUCCESS] = function () {
    this._onEvent(arguments, PROMISE_EVENT_ONSUCCESS);
};

Promise.prototype[PROMISE_EVENT_ONERROR] = function () {
    this._onEvent(arguments, PROMISE_EVENT_ONERROR);
};

Promise.prototype[PROMISE_EVENT_ONPROGRESS] = function () {
    this._onEvent(arguments, PROMISE_EVENT_ONPROGRESS);
};

function createCompletePromise(opName, succeeded, callback, result) {
    var promise = new Promise(opName, null, null),
        completeEvent = succeeded ? PROMISE_EVENT_ONSUCCESS : PROMISE_EVENT_ONERROR;

    if (typeof (callback) === TYPE_FUNCTION) {
        promise.then(function (rs) {
            callback(rs);
        });
    }

    delayInvoke(
        function () {
            promise[completeEvent](result);
        }
    );

    return promise;
}


var AK_COOKIE_KEYS = [AK_ACCESS_TOKEN, AK_AUTH_TOKEN, AK_SCOPE, AK_EXPIRES_IN, AK_EXPIRES];

var APISERVICE_URI = "apiservice_uri",
    AUTH_SERVER = "auth_server";

var IDS_NON_CONNECTED_ACCOUNT_ERROR_CODE = -2147023579;

var SDK_ROOT_PATH = "///LiveSDKHTML/",
    SDK_RESOURCE_PATH = "ms-resource:///WLText/";

var WLText = null;

WL.init = function (properties) {
    /// <summary>
    /// Initializes the JavaScript library. An application must call this function before making other function calls to 
    /// the library except for subscribing/unsubscribing to events.
    /// </summary>
    /// <param name="properties" type="Object">
    /// Required. A JSON object that includes the following properties:
    /// &#10; scope:  Optional. The scope values used to check and determine the user's login and consent status.
    /// &#10; redirect_uri:  Optional. Specifying redirect_uri value will enable the library to return the user's authentication 
    /// token in the user session object. For further detail about authentication token, please check
    /// http://msdn.microsoft.com/en-us/library/live/hh826544.aspx#get_token
    /// The redirect_uri value must match the redirect domain of the app registered at https://manage.dev.live.com/ site.
    /// &#10; logging: Optional. If set to true (default), the library logs error information to the JavaScript console and
    /// notifies the application through "wl.log" event.
    /// </param>

    try {

        var clonedProperties = cloneObject(properties);

        // Validate parameters
        validateParams(
            clonedProperties,
            {
                name: "properties",
                type: "properties",
                optional: false,
                properties: [
                    { name: AK_SCOPE, type: TYPE_STRINGORARRAY, optional: true },
                    { name: AK_REDIRECT_URI, type: "url", optional: true },                        
                    { name: API_PARAM_LOGGING, type: TYPE_BOOLEAN, optional: true }
                ]
            },
            "WL.init");

        wl_app.appInit(clonedProperties);
    }
    catch (e) {
        logError(e.message);
    }
};

WL.login = function (properties, callback) {
    /// <summary>
    /// Signs the user in or expands the user's permission set.
    /// </summary>
    /// <param name="properties" type="Object">
    /// Required. A JSON object with the following properties:
    /// &#10; scope: Required. The scopes for the user to authorize. It can be an array of scope string values or 
    /// a string value of multiple scopes delimited by a space character.
    /// </param>
    /// <param name="callback" type="Function" >Optional. A function that is invoked when login is completed.</param>
    /// <returns type="Promise" mayBeNull="false" >The Promise object that allows you to attach events to handle succeeded and failed
    /// situations.</returns>

    try {
        var args = normalizeArguments(arguments);

        // Validate parameters
        validateProperties(
            args,
            [
                { name: AK_SCOPE, type: TYPE_STRINGORARRAY, optional: true },
                expectedCallback_Optional
            ],
            "WL.login");

        return wl_app.login(args);
    }
    catch (e) {
        return handleAsyncCallingError("WL.login", e);
    }
};

WL.backgroundDownload = function (properties, callback) {
    /// <summary>
    /// Makes a call to download a file from SkyDrive. This is an async method that returns a Promise object that 
    /// allows you to attach events to handle succeeded, failed and progressed situations.
    /// </summary>
    /// <param name="properties" type="Object">Required. A JSON object containing the properties for downloading a file:
    /// &#10; path: Required. The path to the file to download.
    /// &#10; file_output: Optional. The file output object where to write the downloaded file data.
    /// </param>
    /// <param name="callback" type="Function">Optional. A callback function that is invoked when the download call is complete.</param>
    /// <returns type="Promise" mayBeNull="false" >The Promise object that allows you to attach events to handle succeeded, failed, 
    /// and progressed situations.</returns>

    try {
        var method = "WL.backgroundDownload",
            args = normalizeArguments(arguments, method);
        return wl_app.download(args);
    }
    catch (e) {
        return handleAsyncCallingError(method, e);
    }
};

WL.backgroundUpload = function (properties, callback) {
    /// <summary>
    /// Makes a call to upload a file to SkyDrive. This is an async method that returns a Promise object that 
    /// allows you to attach events to handle succeeded, failed and progressed situations.
    /// </summary>
    /// <param name="properties" type="Object">Required. A JSON object containing the properties for uploading a file:
    /// &#10; path: Required. The path to the file to download.
    /// &#10; file_name: the name of the file.
    /// &#10; file_input: The file input object where to read the upload file data.
    /// &#10; input_stream: The input stream from which to read the upload data.
    /// &#10; overwrite: Indicates if the uploading action should overwrite a file that already exists. This only applies to when uploading to a folder.
    /// Suported values include "true", "false", "rename", true, false.
    /// </param>
    /// <param name="callback" type="Function">Optional. A callback function that is invoked when the upload call is complete.</param>
    /// <returns type="Promise" mayBeNull="false" >The Promise object that allows you to attach events to handle succeeded, failed, 
    /// and progressed situations.</returns>

    try {
        var method = "WL.backgroundUpload",
            args = normalizeArguments(arguments, method);
        return wl_app.upload(args);
    }
    catch (e) {
        return handleAsyncCallingError(method, e);
    }
};

WL.ui = function (properties, callback) {
    /// <summary>
    /// Creates a user interface control on the current page.
    /// </summary>
    /// <param name="properties" type="Object">Required. A JSON object containing properties for creating the user interface element.
    /// &#10; name: Required. Specifies the name of the UI element to create. For the sign-in control, it is "signin".
    /// &#10; element: Required. The DOM element to attach to the UI element.
    /// &#10; brand: Optional. Defines the brand, or type of icon, used with the signin control. It can be one of the following
    /// values: "hotmail", "messenger", "windows"(default), "skydrive", or "none".
    /// &#10; theme: Optional. The options are "dark" (default) and "light".
    /// &#10; type: Optional. Defines the type of the sign-in control. It can be one of the following values: "signin" (default), "login", "connect", 
    /// or "custom". 
    /// &#10; sign_in_text: If the type value is "custom", defines the signin text displayed in the sign-in control.
    /// &#10; sign_out_text: If the type value is "custom", defines the sign out text displayed in the sign-in control.
    /// &#10; onloggedin: Optional. A callback function that will be invoked when the user is logged in.
    /// &#10; onloggedout: Optional. A callback function that will be invoked when the user is logged out.
    /// &#10; onerror: Optional. A callback funtion that will be invoked when there is error during logging in.
    /// </param>
    /// <param name="callback" type="Function">Optional. A callback function that is invoked when the UI element is rendered.</param>

    try {
        var method = "WL.ui",
            args = normalizeArguments(arguments, method);

        // Validate parameters
        validateProperties(
                args,
                [{ name: UI_PARAM_NAME, type: TYPE_STRING, allowedValues: [UI_SIGNIN], optional: false },
                expectedCallback_Optional],
                method);

        wl_app.ui(args);

    }
    catch (e) {
        handleUIParameterError(args, e);
    }
};

/**
 * The WWA version of appInitPlatformSpecific() method.
 */
function appInitPlatformSpecific(properties) {
    if (properties[AK_RESPONSE_TYPE] === AK_CODE) {
        throw new Error(ERROR_DESC_UNSUPPORTED_RESPONSE_TYPE_CODE);
    }

    // let's run as ssl so that api service will return us ssl resources.
    wl_app._isHttps = true;

    wl_app._authScope = ensureDefaultAuthScope(wl_app._authScope);

    if (properties[API_PARAM_TRACING]) {
        wl_app._traceEnabled = true;
    }
        
    if (!wl_app[WL_SDK_ROOT]) {
        wl_app[WL_SDK_ROOT] = SDK_ROOT_PATH;
    }

    wl_app._domain = readRedirectDomain(properties);

    initLocale();

    var psid = Windows.Security.Authentication.Web.WebAuthenticationBroker.getCurrentApplicationCallbackUri().absoluteUri;

    properties[AK_CLIENT_ID] = psid;

    cleanPendingBackgroundTransferOperations();

    wl_app._session = new AuthSession(psid);
    wl_app.getLoginStatus({ internal: true });
}

function readRedirectDomain(properties) {
    var redirectUrl = properties[AK_REDIRECT_URI],
        appDomain = null;

    if (redirectUrl != null) {
        
        if (redirectUrl.indexOf(SCHEME_HTTP) === 0 || redirectUrl.indexOf(SCHEME_HTTPS) === 0) {
            appDomain = getDomainName(redirectUrl);
        }
        else {
            throw new Error(ERROR_DESC_REDIRECTURI_INVALID_WWA);
        }        
    }

    return appDomain;
}

function initLocale() {
    try {
        var ns = Windows.ApplicationModel.Resources.Core,
            rm = ns.ResourceManager,
            locale = rm.current.defaultContext.languages[0],
            resources = ns.ResourceManager.current.mainResourceMap;

        wl_app._locale = ((locale || "en").toLowerCase());

        WLText = {
            signIn: resources.getValue(SDK_RESOURCE_PATH + "signIn").valueAsString,
            signOut: resources.getValue(SDK_RESOURCE_PATH + "signOut").valueAsString,
            login: resources.getValue(SDK_RESOURCE_PATH + "login").valueAsString,
            logout: resources.getValue(SDK_RESOURCE_PATH + "logout").valueAsString,
            connect: resources.getValue(SDK_RESOURCE_PATH + "connect").valueAsString
        };
                
    } catch (error) {
        log(error.message);
    }
}

/**
 * The WWA version of handlePageLoad() method.
 */
function handlePageLoad() {

    // set default log/trace settings.
    wl_app._logEnabled = true;
    wl_app._traceEnabled = false;

    checkDocumentReady(function () {
        // Invoke wlAsyncInit, if it is defined.
        var appInit = window.wlAsyncInit;
        if (appInit && (typeof (appInit) === TYPE_FUNCTION)) {
            appInit.call();
        }
    });
}

function normalizeRedirectUrl(url) {
    return url;
}

function ensureDefaultAuthScope(scope) {
    scope = scope || SCOPE_SIGNIN;

    if (scope.indexOf(SCOPE_SIGNIN) < 0) {
        scope = scope + " " + SCOPE_SIGNIN;
    }

    return stringTrim(scope);
}

/**
 * Reattach pending upload/download background transfer operations in order to clean up unnecessary memory usage.
 */
function cleanPendingBackgroundTransferOperations() {
    var ns = Windows.Networking.BackgroundTransfer;
    var attachPendingBackgroundOperations = function (operation) {
        try{
            // Since can't link to app handlers, we just let the pending operations to end silently.
            var promise = operation.attachAsync().then(
                function (resp) { },
                function (error) { },
                function (progress) { }
            );
        }
        catch (error) {
        }
    };

    ns.BackgroundDownloader.getCurrentDownloadsAsync(BT_GROUP_DOWNLOAD).then(
        function (downloads) {
            if (downloads && downloads.size > 0) {
                for (i = 0; i < downloads.size; i++) {
                    attachPendingBackgroundOperations(downloads[i]);
                }
            }
        }
    );

    ns.BackgroundUploader.getCurrentUploadsAsync(BT_GROUP_UPLOAD).then(
        function (uploads) {
            if (uploads && uploads.size > 0) {
                for (i = 0; i < uploads.size; i++) {
                    attachPendingBackgroundOperations(uploads[i]);
                }
            }
        }
    );
}

/**
 * The WWA version of handlePendingLogin() method.
 */
function handlePendingLogin(internal) {
    var pendingRequest = wl_app._pendingLogin;
    if (pendingRequest != null) {
        if (!internal) {
            log(ERROR_DESC_PENDING_LOGIN_CONFLICT);
        }

        return false;
    }

    return true;
}

/**
 * Normalize login scope.
 */
function normalizeLoginScope(properties) {

    var loginScope = properties[AK_SCOPE] || [],
        currentAppScope = wl_app._authScope.split(SCOPE_DELIMINATOR);
    
    if (typeof(loginScope) === TYPE_STRING) {
        loginScope = loginScope.split(SCOPE_DELIMINATOR);
    }

    properties.normalizedScope = arrayMerge(currentAppScope, loginScope).join(" ");
}

/**
 * The WWA version of createLoginRequest() method.
 */
function createLoginRequest(properties, onAuthRequestCompleted) {
    return new AuthRequest(DISPLAY_PAGE, properties.normalizedScope, properties, onAuthRequestCompleted);
}

/**
 * The WWA version of createLoginStatusRequest() method.
 */
function createLoginStatusRequest(properties, onGetLoginStatusCompleted) {
    return new AuthRequest(DISPLAY_NONE, wl_app._authScope, properties, onGetLoginStatusCompleted);
}

wl_app.canLogout = function () {
    return Windows.Security.Authentication.OnlineId.OnlineIdAuthenticator().canSignOut;
};

/**
 * The WWA version of logoutWindowsLive() method.
 */
function logoutWindowsLive(callback) {

    try {
        var ns = Windows.Security.Authentication.OnlineId,
                clientAuth = new ns.OnlineIdAuthenticator();

        var authOperation = clientAuth.signOutUserAsync().then(callback, callback);
    }
    catch (error) {
        log(error);
        delayInvoke(callback);
    }
}

wl_app.ui = function (properties) {

    ensureAppInited("WL.ui");

    if (properties.name === UI_SIGNIN) {
        new SignInControl(properties);
    }
}

function validateSignInControlPlatformSpecificParameters(properties) {
    validateProperties(
        properties,
        [{
            name: UI_PARAM_THEME,
            allowedValues: [UI_SIGNIN_THEME_DARK, UI_SIGNIN_THEME_LIGHT],
            type: TYPE_STRING,
            optional: true
        },
        {
            name: UI_PARAM_BRAND,
            allowedValues: [UI_BRAND_MESSENGER, UI_BRAND_HOTMAIL, UI_BRAND_SKYDRIVE, UI_BRAND_WINDOWS, UI_BRAND_NONE],
            type: TYPE_STRING,
            optional: true
        }],
        "WL.ui-signin");

    properties[UI_PARAM_THEME] = properties[UI_PARAM_THEME] || UI_SIGNIN_THEME_DARK;
    properties[UI_PARAM_BRAND] = properties[UI_PARAM_BRAND] || UI_BRAND_WINDOWS;
}

function buildSignInControlHtml(properties) {
    var brand = (properties[UI_PARAM_BRAND]),
        theme = (properties[UI_PARAM_THEME]),
        imgHtml = buildSignInControlIconHtml(brand, theme),
        textHtml = buildSignInControlTextHtml(brand),
		buttonHtml = "<button style=\"text-align: center;\">" + imgHtml + textHtml + "</button>";

    return buttonHtml;
}

function buildSignInControlIconHtml(brand, theme) {
    var html = "";
    if (brand !== UI_BRAND_NONE) {
        var imgName = brand + (theme === UI_SIGNIN_THEME_DARK ? "_white.png" : "_black.png");
        html = "<img alt=\"\" src=\"" + getImagePath() +"/SignInControl/"+ imgName + "\" style=\"vertical-align: middle;\">";
    }
    return html;
}

function buildSignInControlTextHtml(brand) {
    var html = (brand !== UI_BRAND_NONE) ? "<span style=\"margin: 0px 4px; text-align: center; vertical-align: middle;\"></span>" : "";
    return html;
}

SignInControl.prototype.updateUI = function (text, signedIn) {
    if (this._element.childNodes.length == 0) {
        // The button has been cleared.
        detachSignInControlMouseEvents(this);
        return;
    }

    var button = this._element.childNodes[0];
    button.disabled = (signedIn && !wl_app.canLogout());

    if (text != this._text) {
        var textContainer = (this._properties[UI_PARAM_BRAND] === UI_BRAND_NONE) ? button : button.childNodes[1];

        setElementText(textContainer, text);
        this._text = text;
    }
};

SignInControl.prototype.onMouseDown = function (e) {
    this.mousedown = true;
    OnSignControlMouseEvent(this);
};

SignInControl.prototype.onMouseUp = function (e) {
    this.mousedown = false;
    OnSignControlMouseEvent(this);
};

SignInControl.prototype.onMouseEnter = function (e) {
    this.mousein = true;
    OnSignControlMouseEvent(this);
};

SignInControl.prototype.onMouseLeave = function (e) {
    this.mousein = false;
    OnSignControlMouseEvent(this);
};

function OnSignControlMouseEvent(signInControl) {
    var properties = signInControl._properties,
        brand = properties[UI_PARAM_BRAND];

    if (brand === UI_BRAND_NONE){
        return;
    }

    if (signInControl._element.childNodes.length == 0) {
        // The button has been cleared. 
        detachSignInControlMouseEvents(signInControl);
        return;
    }

    var imgEl = signInControl._element.childNodes[0].childNodes[0],
        theme = (properties[UI_PARAM_THEME]),
        useWhite = (theme === UI_SIGNIN_THEME_DARK),
        invert = signInControl.mousedown && signInControl.mousein;
    useWhite = invert ? !useWhite : useWhite;
    

    var imgName = brand + (useWhite ? "_white.png" : "_black.png");
    imgEl.src = getImagePath() + "/SignInControl/" + imgName;
}

function attachSignInControlMouseEvents(control, button) {
    control._button = button;
    button.addEventListener("click", createSignInControlEventHandler("click", control, control.onClick), false);
    button.addEventListener("mouseenter", createSignInControlEventHandler("mouseenter", control, control.onMouseEnter), false);
    button.addEventListener("mouseleave", createSignInControlEventHandler("mouseleave", control, control.onMouseLeave), false);
    document.addEventListener("mousedown", createSignInControlEventHandler("mousedown", control, control.onMouseDown), false);
    document.addEventListener("mouseup", createSignInControlEventHandler("mouseup", control, control.onMouseUp), false);
}

function detachSignInControlMouseEvents(control) {
    var button = control._button;
    if (button) {
        button.removeEventListener("click", getSignInControlEventHandler("click", control), false);
        button.removeEventListener("mouseenter", getSignInControlEventHandler("mouseenter", control), false);
        button.removeEventListener("mouseleave", getSignInControlEventHandler("mouseleave", control), false);
        document.removeEventListener("mousedown", getSignInControlEventHandler("mousedown", control), false);
        document.removeEventListener("mouseup", getSignInControlEventHandler("mouseup", control), false);
        delete control._button;
    }
}


var AuthRequest = function (display, scope, properties, callback) {
    var request = this;
    request._display = display;
    request._completed = false;
    request._requestFired = false;
    request._properties = properties;
    request._callback = callback;
    request._scope = scope;
    var opName = (display === DISPLAY_NONE) ? "WL.login" : "WL.getLoginStatus";
    request._promise = new Promise(opName, null, null);
};

AuthRequest.prototype = {
    execute: function () {
        return this._sendRequest(this._scope);
    },

    _sendRequest: function (scope) {    
        var request = this,
            onCompleteDelegate = createDelegate(request, request._onResponse),
            ns = Windows.Security.Authentication.OnlineId,
            promptOption = (request._display === DISPLAY_NONE) ? ns.CredentialPromptType.doNotPrompt : ns.CredentialPromptType.promptIfNeeded;

        request._currentScope = scope;
        authenticateUser(promptOption, scope, onCompleteDelegate);

        return request._promise;
    },

    _onResponse: function (response) {
        var request = this;
        if (request._display === DISPLAY_NONE &&
            response[AK_ERROR] === ERROR_ACCESS_DENIED &&
            request._currentScope !== SCOPE_SIGNIN) {
            // If we receive access_denied when making silent auth check, it is possible the user has revoked consent from consent.live.com.
            // So, let's try the default scope (wl.signin) to get access_token.
            request._sendRequest(SCOPE_SIGNIN);
        }
        else {
            request._onComplete(response);
        }
    },
    
    _onComplete: function(response) {
        var request = this;
        if (!request._completed) {
            request._completed = true;

            if (request._display === DISPLAY_NONE &&
                request._scope === SCOPE_SIGNIN &&
                (response[AK_ERROR] === ERROR_ACCESS_DENIED || response[AK_ERROR] === ERROR_UNKNOWN_USER)) {
                // If we receive access_denied or unknow_user for default scope, it means the user may have revoked consent scopes
                // and we should just logout.
                var status = (response[AK_ERROR] === ERROR_ACCESS_DENIED) ? AS_NOTCONNECTED : AS_UNKNOWN;
                wl_app._session.updateStatus(status);
            }
            else {
                // Let the app session absorb the response.
                wl_app._session.onAuthResponse(response);
            }

            var hasError = false,
                appResp = wl_app._session.getStatus();

            if (this._display !== DISPLAY_NONE) {
                // For login, error means we can't get access token, regardless of any reason.
                if (response[AK_ACCESS_TOKEN] == null) {
                    hasError = true;
                    appResp = response;
                }
                else {
                    // IDCRL API map cached tokens to scope values that have been requested. This will cause a problem that the 
                    // the token we retrieved may not cover all scopes that the user has consented. This can happen if the app implements
                    // incremental consent. In order to get the token that satisfy an app's need, we require the app to always pass in
                    // required scopes via WL.init or WL.login, and we remember scope values from WL.init and from WL.login that has been
                    // invoked successfully.
                    // Because we always include WL.init scopes in WL.login execution, if the login is successful, we update the value
                    // of wl_app._authscope so that recent consented scopes are remembered for later REST api calls.
                    wl_app._authScope = this._scope;
                }
            }
            else {
                // For getLoginStatus, unknown_user or access_denied are considered proper responses from the auth server.
                if (response[AK_ERROR]) {
                    switch (response[AK_ERROR]) {
                        case ERROR_UNKNOWN_USER:
                        case ERROR_ACCESS_DENIED:
                            break;
                        default:
                            hasError = true;
                            appResp = response;
                            break;
                    }
                }
            }

            this._callback(this._properties, appResp);

            if (hasError) {
                this._promise[PROMISE_EVENT_ONERROR](appResp);
            }
            else {
                this._promise[PROMISE_EVENT_ONSUCCESS](appResp);
            }
        }
    }
};

function authenticateUser(uiOption, scope, callback) {    
    var authErrorText = "The authentication process failed with error: ",
        configAppGuide = " To configure your app correctly, please follow the instructions on http://go.microsoft.com/fwlink/?LinkId=220871.",
        UserNotFoundErrorCode = -2147023579,
        ConsentNotGrantedErrorCode = -2138701812,
        Invalid_Client = -2138701821,
        Invalid_Auth_Target = -2138701823;

    try {
        var authMethod = wl_app._authMethod ? wl_app._authMethod : invokeClientAuth;

        // Get access token
        authMethod(uiOption, scope, "DELEGATION").then(
            function (delegation_result) {
                var accessToken = delegation_result.tickets[0].value,
                    resp = null;
                if (accessToken && accessToken !== "") {
                    if (wl_app._domain) {
                        // If app redirect domain is available, get the authentication token
                        authMethod(Windows.Security.Authentication.OnlineId.CredentialPromptType.doNotPrompt, wl_app._domain, "JWT").then(
                            function (authToken_result) {
                                var authToken = authToken_result.tickets[0].value;
                                resp = {};
                                resp[AK_ACCESS_TOKEN] = accessToken;
                                resp[AK_AUTH_TOKEN] = authToken;
                                invokeCallback(callback, resp, false/*synchronous*/);
                            },
                            function (result) {
                                var errorCode = ERROR_REQUEST_FAILED,
                                    errorDesc = authErrorText + result.message;
                                if (result.name === "WinRTError" && result.number === Invalid_Auth_Target) {
                                    errorCode = ERROR_INVALID_REQUEST;
                                    errorDesc += configAppGuide;
                                }

                                resp = createAuthError(errorCode, errorDesc);
                                invokeCallback(callback, resp, false/*synchronous*/);
                            });
                    }
                    else {
                        resp = {};
                        resp[AK_ACCESS_TOKEN] = accessToken;
                    }
                }
                else {
                    resp = createAuthError(ERROR_REQUEST_FAILED, authErrorText + "Unable to get access token.");
                }

                if (resp) {
                    invokeCallback(callback, resp, false/*synchronous*/);
                }
            }, 
            function (result) {
                var error = ERROR_REQUEST_FAILED,
                    errorDesc = authErrorText + result.message;
                switch (result.name)
                {
                    case "Canceled":
                        {
                            error = ERROR_ACCESS_DENIED;
                            break;
                        }
                    case "WinRTError":
                        {
                            switch (result.number) {
                                case UserNotFoundErrorCode:
                                    error = ERROR_UNKNOWN_USER;
                                    break;
                                case ConsentNotGrantedErrorCode:
                                    error = ERROR_ACCESS_DENIED;
                                    break;
                                case Invalid_Client:
                                    error = ERROR_INVALID_REQUEST;
                                    errorDesc += configAppGuide;
                                    break;
                                default:
                                    error = ERROR_REQUEST_FAILED;
                                    break;
                            }
                            break;
                        }
                }

                var resp = createAuthError(error, errorDesc);
                invokeCallback(callback, resp, false/*synchronous*/);
            }
        );
    }
    catch (error) {
        var resp = createAuthError(ERROR_REQUEST_FAILED, authErrorText + error.message);
        invokeCallback(callback, resp, false/*synchronous*/);
    }
}

function invokeClientAuth(uiOption, scope, policy) {
    var ns = Windows.Security.Authentication.OnlineId,
        clientAuth = new ns.OnlineIdAuthenticator(),
        request = new ns.OnlineIdServiceTicketRequest(scope, policy);

    return clientAuth.authenticateUserAsync([request], uiOption);
}

var AuthSession = function (client_id) {
    var _this = this;
    _this._state = {};
    _this._state[AK_CLIENT_ID] = client_id;
    _this._state[AK_STATUS] = AS_UNKNOWN;
    _this._state[AK_ACCESS_TOKEN] = null;

    delayInvoke(function () {
        _this.init();
    });
};

AuthSession.prototype = {
    init: function () {
        // check auth..
        wl_app.getLoginStatus({ internal: true }, true);
    },

    isSignedIn: function () {
        return this._state[AK_STATUS] === AS_CONNECTED;
    },

    getStatus: function () {
        var session = null,
            status = this._state[AK_STATUS];

        if (status === AS_CONNECTED) {
            session = {};
            session[AK_ACCESS_TOKEN] = this._state[AK_ACCESS_TOKEN];
            session[AK_AUTH_TOKEN] = this._state[AK_AUTH_TOKEN];
        }

        return { status: status, session: session };
    },

    getNormalStatus: function () {
        return this.getStatus();
    },

    updateStatus: function (status) {
        var oldStatus = this._state[AK_STATUS],
            oldToken = this._state[AK_ACCESS_TOKEN];
        if (oldStatus != status) {
            this._state[AK_STATUS] = status;
            this._stateDirty = true;
            this.onStatusChanged(oldStatus, status);
            
            if (oldToken != this._state[AK_ACCESS_TOKEN]) {
                wl_event.notify(EVENT_AUTH_SESSIONCHANGE, this.getNormalStatus());
            }
        }
    },

    tryGetResponse: function (scope) {
        // Always return null, because we always rely on IDCRL to give us auth response.
        return null;
    },

    onAuthResponse: function (response) {

        var sessionChanged = false,
            state = this._state,
            oldStatus = state[AK_STATUS],
            newAccessToken = response[AK_ACCESS_TOKEN],
            newAuthToken = response[AK_AUTH_TOKEN];

        if ((newAccessToken && state[AK_ACCESS_TOKEN] != newAccessToken) ||
            (newAuthToken && state[AK_AUTH_TOKEN] != newAuthToken)) {

            state[AK_ACCESS_TOKEN] = newAccessToken;
            state[AK_AUTH_TOKEN] = newAuthToken;
            sessionChanged = true;
        }

        var newStatus = (state[AK_ACCESS_TOKEN]) ? AS_CONNECTED : AS_UNKNOWN;

        // Process state change
        if (oldStatus != newStatus) {
            this._statusChecked = true;
            state[AK_STATUS] = newStatus;
            this.onStatusChanged(oldStatus, newStatus);
        }

        if (sessionChanged) {
            wl_event.notify(EVENT_AUTH_SESSIONCHANGE, this.getNormalStatus());
        }
    },

    onStatusChanged: function (oldStatus, newStatus) {

        trace("AuthSession: Auth status changed: " + oldStatus + "=>" + newStatus);

        if (oldStatus != newStatus) {
            var wasSignedin = (oldStatus == AS_CONNECTED),
                isSignedIn = (newStatus == AS_CONNECTED);

            if (!isSignedIn) {
                // Clear the session data, if the user is signed out.
                if (this._state[AK_ACCESS_TOKEN]) {
                    delete this._state[AK_ACCESS_TOKEN];
                }
            }

            if (oldStatus != newStatus) {
                wl_event.notify(EVENT_AUTH_STATUSCHANGE, this.getStatus());
            }

            if (isSignedIn != wasSignedin) {
                if (isSignedIn) {
                    wl_event.notify(EVENT_AUTH_LOGIN, this.getStatus());
                }
                else {
                    wl_event.notify(EVENT_AUTH_LOGOUT, this.getStatus());
                }
            }
        }
    }
}

/**
 * The WWA version of executeApiRequest() method.
 */
function executeApiRequest(request) {

    var f = function () {
        sendAPIRequestViaXHR(request);
    };

    wl_app.getLoginStatus({ internal: true, callback: f });
}

/**
 * The WWA version of canDoXHR() method.
 */
function canDoXHR() {
    return true;
}

/**
 * The WWA version of getAuthServerName() method.
 */
function getAuthServerName() {
    return wl_app._settings[wl_app._env][AUTH_SERVER];
}

/**
 * The WWA version of getApiServiceUrl() method.
 */
function getApiServiceUrl() {
    return wl_app._settings[wl_app._env][APISERVICE_URI];
}



// wl.app.download.wwa.js contains the implementation details
// for the Windows 8 WL.download method.

function validateDownloadProperties(properties) {
    var iMethod = properties[API_INTERFACE_METHOD];
    validateProperties(
        properties,
        [{ name: API_PARAM_PATH, type: TYPE_STRING, optional: false },
         { name: API_PARAM_FILEOUTPUT, type: TYPE_OBJECT, optional: true },
         expectedCallback_Optional],
        iMethod);

    if (properties[API_PARAM_FILEOUTPUT] && !(properties[API_PARAM_FILEOUTPUT] instanceof Windows.Storage.StorageFile)) {
        throw new Error(iMethod + ": unsupported file_output object type");
    }
}

function executeDownload(op) {
    var props = op._properties,
        path = props[API_PARAM_PATH],
        file_output = props[API_PARAM_FILEOUTPUT];
    startDownloadViaBackgroundTransfer(path, file_output, op);
}

function startDownloadViaBackgroundTransfer(path, file_output, op) {
    
    var downloadPathString = isPathFullUrl(path) ? path : buildFilePathUrlString(path),
        downloadPathUri = new Windows.Foundation.Uri(downloadPathString);

    try {
        var downloader = new Windows.Networking.BackgroundTransfer.BackgroundDownloader();
        downloader.group = BT_GROUP_DOWNLOAD;
        var downloadOp = downloader.createDownload(downloadPathUri, file_output);
        var promise = downloadOp.startAsync().then(
            function (response) {
                var appResp = {
                    content_type: response.getResponseInformation().headers.lookup("Content-Type"),
                    stream: response.getResultStreamAt(0)
                };
                op.downloadComplete(true, appResp);
            },
            function (error) {
                handleDownloadErrorResponse(error.message, op);
            },
            function (response) {
                var progress = response.progress;
                var downloadProgress = {
                    bytesTransferred: progress.bytesReceived,
                    totalBytes: progress.totalBytesToReceive,
                    progressPercentage: (progress.totalBytesToReceive == 0) ? 0 : (progress.bytesReceived / progress.totalBytesToReceive) * 100
                };

                op.downloadProgress(downloadProgress);
            });
        op._cancel = createDelegate(promise, promise.cancel);
    }
    catch (error) {
        handleDownloadErrorResponse(error.message, op);
    }
}

// wl.app.upload.wwa.js

UploadOperation.prototype._getStrategy = function (props) {
    var interfaceMethod = props[API_INTERFACE_METHOD],
        file = props[API_PARAM_FILEINPUT],
        stream = props[API_PARAM_STREAMINPUT];
        
    if (file) {
        if (file instanceof Windows.Storage.StorageFile) {
            return new BackgroundTransferUploadStrategy(this, file);
        } else {
            throw new Error(interfaceMethod + ": unsupported file_input object type");
        }
    }
    else if (stream) {
        // Use BackgroundUploader
        return new BackgroundTransferUploadStrategy(this, stream);
    }
    else {
        throw new Error(interfaceMethod + ": file_input or stream_input must be specified.");
    }
}

/**
 * Logic for performing a Background Transfer upload.
 */
function BackgroundTransferUploadStrategy(operation, uploadSource) {
    this.upload = function (requestUrl) {
        var uploadPathUri = new Windows.Foundation.Uri(requestUrl),
            uploader = new Windows.Networking.BackgroundTransfer.BackgroundUploader(),
            onUploadSuccess = function (uploadOp) {
                var text = "";
                var resultStream = uploadOp.getResultStreamAt(0);
                if (resultStream) {
                    var reader = new Windows.Storage.Streams.DataReader(resultStream);
                    var onRead = function (result) {
                        if (result) {
                            text = reader.readString(result);
                        }

                        operation.onResp(text);
                        reader.close();
                    };

                    reader.loadAsync(100000).then(onRead, onRead);
                }
                else {
                    operation.onResp(text);
                }
            },
            onUploadError = function (error) {
                operation.onErr(error.message);
            },
            onUploadProgress = function (response) {
                var progress = response.progress;
                var uploadProgress = {
                    bytesTransferred: progress.bytesSent,
                    totalBytes: progress.totalBytesToSend,
                    progressPercentage: (progress.totalBytesToSend == 0) ?
                                        0 :
                                        (progress.bytesSent / progress.totalBytesToSend) * 100
                };

                operation.uploadProgress(uploadProgress);
            },
            tryCatchExecution = function (method) {
                try {
                    method();
                } catch (error) {
                    operation.onErr(error.message);
                }
            };

        uploader.group = BT_GROUP_UPLOAD;
        uploader.method = HTTP_METHOD_PUT;

        tryCatchExecution(function () {
            var promise;
            if (uploadSource instanceof Windows.Storage.StorageFile) {
                promise = uploader.createUpload(uploadPathUri,
                                                uploadSource).startAsync().then(
                                                    onUploadSuccess,
                                                    onUploadError,
                                                    onUploadProgress);
            }
            else {
                promise = uploader.createUploadFromStreamAsync(uploadPathUri, uploadSource).then(
                    function (uploadOp) {
                        tryCatchExecution(function () {
                            promise = uploadOp.startAsync().then(
                                                        onUploadSuccess,
                                                        onUploadError,
                                                        onUploadProgress);

                            operation._cancel = createDelegate(promise, promise.cancel);
                        });
                    },
                    onUploadError);
            }

            operation._cancel = createDelegate(promise, promise.cancel);
        });
    }
};

/**
 * The WWA version of checkDocumentReady() method.
 */
function checkDocumentReady(onDocReady) {

    var readyState = document.readyState;
    if (readyState === "complete" || document.body !== null) {
        onDocReady();
    }
    else {
        window.addEventListener("DOMContentLoaded", onDocReady, false);
    }
}

/**
 * The WWA version of setInnerHtml() method.
 */
function setInnerHtml(element, content) {
    MSApp.execUnsafeLocalFunction(function () {
        element.innerHTML = content;
    });
}


wl_app[API_X_HTTP_LIVE_LIBRARY] = "Windows/HTML8_" + trimVersionBuildNumber("5.2.3370.0802");

if (!wl_app._settings) {
	wl_app._settings = {};
}

var settings = wl_app._settings;

var prodSettings = {},
    authServer = "login.live.com";
prodSettings[APISERVICE_URI] = "https://apis.live.net/v5.0/";
prodSettings[AUTH_SERVER] = authServer;
settings["PROD"] = prodSettings;


function getLiveIdServiceName() {
    
    switch (wl_app._env) {
        case "PROD":
        case "BETA":
            return "jwt.oauth.live.net";
        default:
            return "jwt.oauth.live-int.net";
    }
}

wl_app._env = "PROD";


    wl_app.onloadInit();
}
})();
