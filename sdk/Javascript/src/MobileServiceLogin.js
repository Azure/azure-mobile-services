// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="Generated\MobileServices.DevIntellisense.js" />

var _ = require('Extensions');
var Validate = require('Validate');
var Platform = require('Platform');

var loginUrl = "login";
var loginDone = "done";

function MobileServiceLogin(client, ignoreFilters) {
    /// <summary>
    /// Initializes a new instance of the MobileServiceLogin class.
    /// </summary>
    /// <param name="client" type="MobileServiceClient" mayBeNull="false">
    /// Reference to the MobileServiceClient associated with this login.
    /// </param>
    /// <param name="ignoreFilters" type="Boolean" mayBeNull="true">
    /// Optional parameter to indicate if the client filters should be ignored
    /// and requests should be sent directly. Is true by default. This should
    /// only be set to false for testing purposes when filters are needed to intercept
    /// and validate requests and responses.
    /// </param>

    // Account for absent optional arguments
    if (_.isNull(ignoreFilters)) {
        ignoreFilters = true;
    }

    // Validate arguments
    Validate.notNull(client);
    Validate.isObject(client, 'client');

    // Create read/write fields
    this._loginState = { inProcess: false, cancelCallback: null };
    this.ignoreFilters = ignoreFilters;

    // Create get accessors for read-only fields
    this.getMobileServiceClient = function () {
        /// <summary>
        /// Gets the MobileServiceClient associated with this table.
        /// <summary>
        /// <returns type="MobileServiceClient">
        /// The MobileServiceClient associated with this table.
        /// </returns>
        return client;
    };

    this.getLoginInProcess = function () {
        /// <summary>
        /// Indicates if a login is currently in process or not.
        /// <summary>
        /// <returns type="Boolean">
        /// True if a login is in process and false otherwise.
        /// </returns>
        return this._loginState.inProcess;
    };
}

// Export the MobileServiceLogin class
exports.MobileServiceLogin = MobileServiceLogin;

// Define the MobileServiceLogin in a namespace (note: this has global effects
// unless the platform we're using chooses to ignore it because exports are
// good enough).
Platform.addToMobileServicesClientNamespace({ MobileServiceLogin: MobileServiceLogin });

MobileServiceLogin.prototype.login = function (provider, token, useSingleSignOn, callback) {
    /// <summary>
    /// Log a user into a Mobile Services application given a provider name and optional token object
    /// Microsoft Account authentication token.
    /// </summary>
    /// <param name="provider" type="String" mayBeNull="true">
    /// Optional name of the authentication provider to use; one of 'facebook', 'twitter', 'google',
    /// 'windowsazureactivedirectory' (can also use 'aad'), or 'microsoftaccount'.
    /// </param>
    /// <param name="token" type="Object"  mayBeNull="true">
    /// Optional provider specific object with existing OAuth token to log in with or
    /// a JWT Mobile Services authentication token if the provider is null.
    /// </param>
    /// <param name="useSingleSignOn" type="Boolean" mayBeNull="true">
    /// Only applies to Windows 8 clients.  Will be ignored on other platforms.
    /// Indicates if single sign-on should be used. Single sign-on requires that the 
    /// application's Package SID be registered with the Microsoft Azure Mobile Service, 
    /// but it provides a better experience as HTTP cookies are supported so that users 
    /// do not have to login in everytime the application is launched.
    /// </param>
    /// <param name="callback" type="Function"  mayBeNull="true">
    /// Optional callback accepting (error, user) parameters.
    /// </param>

    // Account for absent optional arguments
    if (_.isNull(callback)) {
        if (!_.isNull(useSingleSignOn) && (typeof useSingleSignOn === 'function')) {
            callback = useSingleSignOn;
            useSingleSignOn = null;
        }
        else if (!_.isNull(token) && (typeof token === 'function')) {
            callback = token;
            useSingleSignOn = null;
            token = null;
        }
    }
    if (_.isNull(useSingleSignOn)) {
        if (_.isBool(token)) {
            useSingleSignOn = token;
            token = null;
        }
        else {
            useSingleSignOn = false;
        }
    }
    
    // Determine if the provider is actually a Mobile Services authentication token
    if (_.isNull(token) && _.isString(provider) && provider.split('.').length === 3) {
        token = provider;
        provider = null;
    }

    // Validate parameters; there must be either a provider, a token or both
    if (_.isNull(provider)) {
        Validate.notNull(token);
        Validate.isString(token);
    }
    if (_.isNull(token)) {
        Validate.notNull(provider);
        Validate.isString(provider);
        provider = provider.toLowerCase();
    }

    if (!_.isNull(provider)) {
        if (provider.toLowerCase() === 'windowsazureactivedirectory') {
            // The mobile service REST API uses '/login/aad' for Microsoft Azure Active Directory
            provider = 'aad';
        }
        this.loginWithProvider(provider, token, useSingleSignOn, callback);
    }
    else {
        this.loginWithMobileServiceToken(token, callback);
    }
};

MobileServiceLogin.prototype.loginWithMobileServiceToken = function(authenticationToken, callback) {
    /// <summary>
    /// Log a user into a Mobile Services application given an Mobile Service authentication token.
    /// </summary>
    /// <param name="authenticationToken" type="String">
    /// OAuth access token that authenticates the user.
    /// </param>
    /// <param name="callback" type="Function">
    /// Optional callback accepting (error, user) parameters.
    /// </param>

    var self = this;
    var client = self.getMobileServiceClient();

    Validate.isString(authenticationToken, 'authenticationToken');
    Validate.notNullOrEmpty(authenticationToken, 'authenticationToken');

    client._request(
        'POST',
        loginUrl,
        { authenticationToken: authenticationToken },
        self.ignoreFilters,
        function(error, response) { 
            onLoginResponse(error, response, client, callback);
        });
};

MobileServiceLogin.prototype.loginWithProvider = function(provider, token, useSingleSignOn, callback) {
    /// <summary>
    /// Log a user into a Mobile Services application given a provider name and optional token object.
    /// </summary>
    /// <param name="provider" type="String">
    /// Name of the authentication provider to use; one of 'facebook', 'twitter', 'google',
    /// 'windowsazureactivedirectory' (can also use 'aad'), or 'microsoftaccount'.
    /// </param>
    /// <param name="token" type="Object" mayBeNull="true">
    /// Optional, provider specific object with existing OAuth token to log in with.
    /// </param>
    /// <param name="useSingleSignOn" type="Boolean" mayBeNull="true">
    /// Optional, indicates if single sign-on should be used.  Single sign-on requires that the
    /// application's Package SID be registered with the Microsoft Azure Mobile Service, but it
    /// provides a better experience as HTTP cookies are supported so that users do not have to
    /// login in everytime the application is launched. Is false be default.
    /// </param>
    /// <param name="callback" type="Function" mayBeNull="true">
    /// The callback to execute when the login completes: callback(error, user).
    /// </param>

    // Account for absent optional arguments
    if (_.isNull(callback))
    {
        if (_.isNull(useSingleSignOn) && (typeof token === 'function')) {
            callback = token;
            token = null;
            useSingleSignOn = false;
        } else if (typeof useSingleSignOn === 'function') {
            callback = useSingleSignOn;
            useSingleSignOn = false;
            if (!_.isNull(token) && _.isBool(token)) {
                useSingleSignOn = token;
                token = null;
            }
        }
    }

    // Validate arguments
    Validate.isString(provider, 'provider');
    if (!_.isNull(token)) {
        Validate.isObject(token, 'token');
    }

    // Throw if a login is already in process and is not cancellable
    if (this._loginState.inProcess) {
        var didCancel = this._loginState.cancelCallback && this._loginState.cancelCallback();
        if (!didCancel) {
            throw Platform.getResourceString("MobileServiceLogin_LoginErrorResponse");
        }
    }

    provider = provider.toLowerCase();
    
    // Either login with the token or the platform specific login control.
    if (!_.isNull(token)) {
        loginWithProviderAndToken(this, provider, token, callback);
    }
    else {
        loginWithLoginControl(this, provider, useSingleSignOn, callback);
    }
};

function onLoginComplete(error, token, client, callback) {
    /// <summary>
    /// Handles the completion of the login and calls the user's callback with
    /// either a user or an error.
    /// </summary>
    /// <param name="error" type="string" mayBeNull="true">
    /// Optional error that may have occurred during login. Will be null if the
    /// login succeeded and their is a token.
    /// </param>
    /// <param name="token" type="string" mayBeNull="true">
    /// Optional token that represents the logged-in user. Will be null if the
    /// login failed and their is an error.
    /// </param>
    /// <param name="client" type="MobileServiceClient">
    /// The Mobile Service client associated with the login.
    /// </param>
    /// <param name="callback" type="Function" mayBeNull="true">
    /// The callback to execute when the login completes: callback(error, user).
    /// </param>
    var user = null;

    if (_.isNull(error)) {

        // Validate the token
        if (_.isNull(token) ||
            !_.isObject(token) ||
            !_.isObject(token.user) ||
            !_.isString(token.authenticationToken)) {
            error = Platform.getResourceString("MobileServiceLogin_InvalidResponseFormat");
        }
        else {
            // Set the current user on the client and return it in the callback
            client.currentUser = token.user;
            client.currentUser.mobileServiceAuthenticationToken = token.authenticationToken;
            user = client.currentUser;
        }
    }

    if (!_.isNull(callback)) {
        callback(error, user);
    }
}

function onLoginResponse(error, response, client, callback) {
    /// <summary>
    /// Handles the completion of the login HTTP call and calls the user's callback with
    /// either a user or an error.
    /// </summary>
    /// <param name="error" type="string" mayBeNull="true">
    /// Optional error that may have occurred during login. Will be null if the
    /// login succeeded and their is a token.
    /// </param>
    /// <param name="response" type="string" mayBeNull="true">
    /// Optional HTTP login response from the Mobile Service. Will be null if the
    /// login failed and their is an error.
    /// </param>
    /// <param name="client" type="MobileServiceClient">
    /// The Mobile Service client associated with the login.
    /// </param>
    /// <param name="callback" type="Function" mayBeNull="true">
    /// The callback to execute when the login completes: callback(error, user).
    /// </param>

    var mobileServiceToken = null;
    if (_.isNull(error)) {
        try {
            mobileServiceToken = _.fromJson(response.responseText);
        }
        catch (e) {
            error = e;
        }
    }

    onLoginComplete(error, mobileServiceToken, client, callback);
}

function loginWithProviderAndToken(login, provider, token, callback) {
    /// <summary>
    /// Log a user into a Mobile Services application given a provider name and token object.
    /// </summary>
    /// <param name="login" type="MobileServiceLogin">
    /// The login instance that holds the context used with the login process.
    /// </param>
    /// <param name="provider" type="String">
    /// Name of the authentication provider to use; one of 'facebook', 'twitter', 'google', or 
    /// 'microsoftaccount'. The provider should already have been validated.
    /// </param>
    /// <param name="token" type="Object">
    /// Provider specific object with existing OAuth token to log in with.
    /// </param>
    /// <param name="callback" type="Function" mayBeNull="true">
    /// The callback to execute when the login completes: callback(error, user).
    /// </param>

    var client = login.getMobileServiceClient();

    // This design has always been problematic, because the operation can take arbitrarily
    // long and there is no way for the UI to cancel it. We should probably remove this
    // one-at-a-time restriction.
    login._loginState = { inProcess: true, cancelCallback: null };

    // Invoke the POST endpoint to exchange provider-specific token for a 
    // Microsoft Azure Mobile Services token
    client._request(
        'POST',
        loginUrl + '/' + provider,
        token,
        login.ignoreFilters,
        function (error, response) {
            login._loginState = { inProcess: false, cancelCallback: null };
            onLoginResponse(error, response, client, callback);
        });
}

function loginWithLoginControl(login, provider, useSingleSignOn, callback) {
    /// <summary>
    /// Log a user into a Mobile Services application using a platform specific
    /// login control that will present the user with the given provider's login web page.
    /// </summary>
    /// <param name="login" type="MobileServiceLogin">
    /// The login instance that holds the context used with the login process.
    /// </param>
    /// <param name="provider" type="String">
    /// Name of the authentication provider to use; one of 'facebook', 'twitter', 'google', or 'microsoftaccount'.
    /// </param>
    /// <param name="useSingleSignOn" type="Boolean">
    /// Optional, indicates if single sign-on should be used.  Single sign-on requires that the
    /// application's Package SID be registered with the Microsoft Azure Mobile Service, but it
    /// provides a better experience as HTTP cookies are supported so that users do not have to
    /// login in everytime the application is launched. Is false be default.
    /// </param>
    /// <param name="callback" type="Function"  mayBeNull="true">
    /// The callback to execute when the login completes: callback(error, user).
    /// </param>

    var client = login.getMobileServiceClient();
    var startUri = _.url.combinePathSegments(
        client.applicationUrl,
        loginUrl,
        provider);
    var endUri = null;

    // If not single sign-on, then we need to construct a non-null end uri.
    if (!useSingleSignOn) {
        endUri = _.url.combinePathSegments(
            client.applicationUrl,
            loginUrl,
            loginDone);
    }
    
    login._loginState = { inProcess: true, cancelCallback: null }; // cancelCallback gets set below

    // Call the platform to launch the login control, capturing any
    // 'cancel' callback that it returns
    var platformResult = Platform.login(
        startUri,
        endUri,
        function (error, mobileServiceToken) {
            login._loginState = { inProcess: false, cancelCallback: null };
            onLoginComplete(error, mobileServiceToken, client, callback);
        });
    
    if (login._loginState.inProcess && platformResult && platformResult.cancelCallback) {
        login._loginState.cancelCallback = platformResult.cancelCallback;
    }
}
