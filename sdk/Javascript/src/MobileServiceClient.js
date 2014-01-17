// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="Generated\MobileServices.DevIntellisense.js" />

var _ = require('Extensions');
var Validate = require('Validate');
var Platform = require('Platform');
var MobileServiceTable = require('MobileServiceTable').MobileServiceTable;
var MobileServiceLogin = require('MobileServiceLogin').MobileServiceLogin;

function MobileServiceClient(applicationUrl, applicationKey) {
    /// <summary>
    /// Initializes a new instance of the MobileServiceClient class.
    /// </summary>
    /// <param name="applicationUrl" type="string" mayBeNull="false">
    /// The URL to the Mobile Services application.
    /// </param>
    /// <param name="applicationKey" type="string" mayBeNull="false">
    /// The Mobile Service application's key.
    /// </param>

    Validate.isString(applicationUrl, 'applicationUrl');
    Validate.notNullOrEmpty(applicationUrl, 'applicationUrl');
    Validate.isString(applicationKey, 'applicationKey');

    this.applicationUrl = applicationUrl;
    this.applicationKey = applicationKey || null;

    var sdkInfo = Platform.getSdkInfo();
    var osInfo = Platform.getOperatingSystemInfo();
    var sdkVersion = sdkInfo.fileVersion.split(".").slice(0, 2).join(".");
    this.version = "ZUMO/" + sdkVersion + " (lang=" + sdkInfo.language + "; " +
                                            "os=" + osInfo.name + "; " +
                                            "os_version=" + osInfo.version + "; " +
                                            "arch=" + osInfo.architecture + "; " +
                                            "version=" + sdkInfo.fileVersion + ")";
    this.currentUser = null;
    this._serviceFilter = null;
    this._login = new MobileServiceLogin(this);
    this.getTable = function (tableName) {
        /// <summary>
        /// Gets a reference to a table and its data operations.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>A reference to the table.</returns>

        Validate.isString(tableName, 'tableName');
        Validate.notNullOrEmpty(tableName, 'tableName');
        return new MobileServiceTable(tableName, this);
    };
    
    if (Platform.Push) {
        this.push = new Platform.Push(this);
    }
}

// Export the MobileServiceClient class
exports.MobileServiceClient = MobileServiceClient;

// Define the MobileServiceClient in a namespace (note: this has global effects
// unless the platform we're using chooses to ignore it because exports are
// good enough).
Platform.addToMobileServicesClientNamespace({ MobileServiceClient: MobileServiceClient });

MobileServiceClient.prototype.withFilter = function (serviceFilter) {
    /// <summary>
    /// Create a new MobileServiceClient with a filter used to process all
    /// of its HTTP requests and responses.
    /// </summary>
    /// <param name="serviceFilter" type="Function">
    /// The filter to use on the service.  The signature of a serviceFilter is
    ///    function(request, next, callback)
    ///  where
    ///    next := function(request, callback)
    ///    callback := function(error, response)
    /// </param>
    /// <returns type="MobileServiceClient">
    /// A new MobileServiceClient whose HTTP requests and responses will be
    /// filtered as desired.
    /// </returns>
    /// <remarks>
    /// The Mobile Services HTTP pipeline is a chain of filters composed
    /// together by giving each the next operation which it can invoke
    /// (zero, one, or many times as necessary).  The default continuation
    /// of a brand new MobileServiceClient will just get the HTTP response
    /// for the corresponding request.  Here's an example of a Handle
    /// implementation that will automatically retry a request that times
    /// out.
    ///     function(req, next, callback) {
    ///         next(req, function(err, rsp) {
    ///           if (rsp.statusCode >= 400) {
    ///               next(req, callback);
    ///           } else {
    ///               callback(err, rsp);
    ///           }
    ///         });
    ///     }
    /// Note that because these operations are asynchronous, this sample
    /// filter could end up actually making two HTTP requests before
    /// returning a response to the developer without the developer writing
    /// any special code to handle the situation.
    /// -
    /// Filters are composed just like standard function composition.  If
    /// we had new MobileServiceClient().withFilter(F1).withFilter(F2)
    /// .withFilter(F3), it's conceptually equivalent to saying:
    ///     var response = F3(F2(F1(next(request)));
    /// </remarks>

    Validate.notNull(serviceFilter, 'serviceFilter');

    // Clone the current instance
    var client = new MobileServiceClient(this.applicationUrl, this.applicationKey);
    client.currentUser = this.currentUser;

    // Chain the service filter with any existing filters
    var existingFilter = this._serviceFilter;
    client._serviceFilter = _.isNull(existingFilter) ?
        serviceFilter :
        function (req, next, callback) {
            // compose existingFilter with next so it can be used as the next
            // of the new serviceFilter
            var composed = function (req, callback) {
                existingFilter(req, next, callback);
            };
            serviceFilter(req, composed, callback);
        };

    return client;
};

MobileServiceClient.prototype._request = function (method, uriFragment, content, ignoreFilters, headers, callback) {
    /// <summary>
    /// Perform a web request and include the standard Mobile Services headers.
    /// </summary>
    /// <param name="method" type="string">
    /// The HTTP method used to request the resource.
    /// </param>
    /// <param name="uriFragment" type="String">
    /// URI of the resource to request (relative to the Mobile Services
    /// runtime).
    /// </param>
    /// <param name="content" type="Object">
    /// Optional content to send to the resource.
    /// </param>
    /// <param name="ignoreFilters" type="Boolean" mayBeNull="true">
    /// Optional parameter to indicate if the client filters should be ignored
    /// and the request should be sent directly. Is false by default.
    /// </param>
    /// <param name="headers" type="Object">
    /// Optional request headers
    /// </param>
    /// <param name="callback" type="function(error, response)">
    /// Handler that will be called on the response.
    /// </param>

    // Account for absent optional arguments
    if (_.isNull(callback) && (typeof headers === 'function')) {
        callback = headers;
        headers = null;
    }

    if (_.isNull(callback) && (typeof ignoreFilters === 'function')) {
        callback = ignoreFilters;
        ignoreFilters = false;
    }

    Validate.isString(method, 'method');
    Validate.notNullOrEmpty(method, 'method');
    Validate.isString(uriFragment, 'uriFragment');
    Validate.notNull(uriFragment, 'uriFragment');
    Validate.notNull(callback, 'callback');

    // Create the absolute URI
    var options = { type: method.toUpperCase() };
    options.url = _.url.combinePathSegments(this.applicationUrl, uriFragment);

    // Set MobileServices authentication, application, User-Agent and telemetry headers
    options.headers = {};
    if (!_.isNull(headers)) {
        _.extend(options.headers, headers);
    }
    options.headers["X-ZUMO-INSTALLATION-ID"] = MobileServiceClient._applicationInstallationId;
    if (!_.isNullOrEmpty(this.applicationKey)) {
        options.headers["X-ZUMO-APPLICATION"] = this.applicationKey;
    }
    if (this.currentUser && !_.isNullOrEmpty(this.currentUser.mobileServiceAuthenticationToken)) {
        options.headers["X-ZUMO-AUTH"] = this.currentUser.mobileServiceAuthenticationToken;
    }
    if (!_.isNull(MobileServiceClient._userAgent)) {
        options.headers["User-Agent"] = MobileServiceClient._userAgent;
    }
    if (!_.isNullOrEmpty["X-ZUMO-VERSION"]) {
        options.headers["X-ZUMO-VERSION"] = this.version;
    }

    // Add any content as JSON
    if (!_.isNull(content)) {
        if (!_.isString(content)) {
            options.data = _.toJson(content);
        } else {
            options.data = content;
        }

        if(!_.hasProperty(options.headers, ['Content-Type','content-type','CONTENT-TYPE','Content-type'])) {
            options.headers['Content-Type'] = 'application/json';
        }
    } else {
        // options.data must be set to null if there is no content or the xhr object
        // will set the content-type to "application/text" for non-GET requests.
        options.data = null;
    }

    // Treat any >=400 status codes as errors.  Also treat the status code 0 as
    // an error (which indicates a connection failure).
    var handler = function (error, response) {
        if (!_.isNull(error)) {
            error = _.createError(error);
        } else if (!_.isNull(response) && (response.status >= 400 || response.status === 0)) {
            error = _.createError(null, response);
            response = null;
        }
        callback(error, response);
    };

    // Make the web request
    if (!_.isNull(this._serviceFilter) && !ignoreFilters) {
        this._serviceFilter(options, Platform.webRequest, handler);
    } else {
        Platform.webRequest(options, handler);
    }
};

MobileServiceClient.prototype.login = Platform.async(
    function (provider, token, useSingleSignOn, callback) {
        /// <summary>
        /// Log a user into a Mobile Services application given a provider name and optional 
        /// authentication token.
        /// </summary>
        /// <param name="provider" type="String" mayBeNull="true">
        /// Name of the authentication provider to use; one of 'facebook', 'twitter', 'google', 
        /// 'windowsazureactivedirectory' (can also use 'aad')
        /// or 'microsoftaccount'. If no provider is specified, the 'token' parameter
        /// is considered a Microsoft Account authentication token. If a provider is specified, 
        /// the 'token' parameter is considered a provider-specific authentication token.
        /// </param>
        /// <param name="token" type="Object" mayBeNull="true">
        /// Optional, provider specific object with existing OAuth token to log in with.
        /// </param>
        /// <param name="useSingleSignOn" type="Boolean" mayBeNull="true">
        /// Only applies to Windows 8 clients.  Will be ignored on other platforms.
        /// Indicates if single sign-on should be used. Single sign-on requires that the 
        /// application's Package SID be registered with the Windows Azure Mobile Service, 
        /// but it provides a better experience as HTTP cookies are supported so that users 
        /// do not have to login in everytime the application is launched.
        /// </param>
        /// <param name="callback" type="Function" mayBeNull="true">
        /// Optional callback accepting (error, user) parameters.
        /// </param>
        this._login.login(provider, token, useSingleSignOn, callback);
    });

MobileServiceClient.prototype.logout = function () {
    /// <summary>
    /// Log a user out of a Mobile Services application.
    /// </summary>
    this.currentUser = null;
};

MobileServiceClient.prototype.invokeApi = Platform.async(
    function (apiName, options, callback) {   
        /// <summary>
        /// Invokes the specified custom api and returns a response object.
        /// </summary>
        /// <param name="apiName">
        /// The custom api to invoke.
        /// </param>
        /// <param name="options" mayBeNull="true">
        /// Contains additional parameter information, valid values are:
        /// body: The body of the HTTP request.
        /// method: The HTTP method to use in the request, with the default being POST,
        /// parameters: Any additional query string parameters, 
        /// headers: HTTP request headers, specified as an object.
        /// </param>
        /// <param name="callback" type="Function" mayBeNull="true">
        /// Optional callback accepting (error, results) parameters.
        /// </param>

        Validate.isString(apiName, 'apiName');

        // Account for absent optional arguments
        if (_.isNull(callback)) {
            if (typeof options === 'function') {
                callback = options;
                options = null;
            }
        }
        Validate.notNull(callback, 'callback');

        var parameters, method, body, headers;
        if (!_.isNull(options)) {
            parameters = options.parameters;
            if (!_.isNull(parameters)) {
                Validate.isValidParametersObject(options.parameters);
            }

            method = options.method;
            body = options.body;
            headers = options.headers;
        }
        if (_.isNull(method)) {
            method = "POST";
        }

        // Construct the URL
        var urlFragment = _.url.combinePathSegments("api", apiName);
        if (!_.isNull(parameters)) {
            var queryString = _.url.getQueryString(parameters);
            urlFragment = _.url.combinePathAndQuery(urlFragment, queryString);
        }

        // Make the request
        this._request(
            method,
            urlFragment,
            body,
            null,
            headers,
            function (error, response) {
                if (!_.isNull(error)) {
                    callback(error, null);
                } else {
                    if (typeof response.getResponseHeader === 'undefined') { // (when using IframeTransport, IE9)
                        try {
                            response.result = _.fromJson(response.responseText);
                        } catch(e) {
                            // Do nothing, since we don't know the content-type, failing may be ok
                        }
                    } else if (response.getResponseHeader('Content-Type').toLowerCase().indexOf('json') !== -1) {
                        response.result = _.fromJson(response.responseText);
                    }

                    callback(null, response);
                }
            });

    });

function getApplicationInstallationId() {
    /// <summary>
    /// Gets or creates the static application installation ID.
    /// </summary>
    /// <returns type="string">
    /// The application installation ID.
    /// </returns>

    // Get or create a new installation ID that can be passed along on each
    // request to provide telemetry data
    var applicationInstallationId = null;

    // Check if the config settings exist
    var path = "MobileServices.Installation.config";
    var contents = Platform.readSetting(path);
    if (!_.isNull(contents)) {
        // Parse the contents of the file as JSON and pull out the
        // application's installation ID.
        try {
            var config = _.fromJson(contents);
            applicationInstallationId = config.applicationInstallationId;
        } catch (ex) {
            // Ignore any failures (like invalid JSON, etc.) which will allow
            // us to fall through to and regenerate a valid config below
        }
    }

    // If no installation ID was found, generate a new one and save the config
    // settings.  This is pulled out as a separate function because we'll do it
    // even if we successfully read an existing config but there's no
    // installation ID.
    if (_.isNullOrEmpty(applicationInstallationId)) {
        applicationInstallationId = _.createUniqueInstallationId();

        // TODO: How many other settings should we write out as well?
        var configText = _.toJson({ applicationInstallationId: applicationInstallationId });
        Platform.writeSetting(path, configText);
    }

    return applicationInstallationId;
}


/// <summary>
/// Get or set the static _applicationInstallationId by checking the settings
/// and create the value if necessary.
/// </summary>
MobileServiceClient._applicationInstallationId = getApplicationInstallationId();

/// <summary>
/// Get or set the static _userAgent by calling into the Platform.
/// </summary>
MobileServiceClient._userAgent = Platform.getUserAgent();


