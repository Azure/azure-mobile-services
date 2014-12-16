/*
Copyright (c) Microsoft Open Technologies, Inc.
All Rights Reserved
Apache 2.0 License
 
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
 
     http://www.apache.org/licenses/LICENSE-2.0
 
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 
See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.
 */

/**
 * MobileServiceClient.java
 */
package com.microsoft.windowsazure.mobileservices;

import java.io.UnsupportedEncodingException;
import java.lang.reflect.Array;
import java.lang.reflect.Field;
import java.lang.reflect.Modifier;
import java.lang.reflect.Type;
import java.net.MalformedURLException;
import java.net.URL;
import java.util.ArrayList;
import java.util.Date;
import java.util.EnumSet;
import java.util.List;

import org.apache.http.client.methods.HttpPost;
import org.apache.http.protocol.HTTP;

import android.accounts.Account;
import android.accounts.AccountManager;
import android.accounts.AccountManagerCallback;
import android.accounts.AccountManagerFuture;
import android.app.Activity;
import android.content.Context;
import android.os.Bundle;
import android.util.Pair;

import com.google.common.util.concurrent.FutureCallback;
import com.google.common.util.concurrent.Futures;
import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.SettableFuture;
import com.google.gson.GsonBuilder;
import com.google.gson.JsonDeserializer;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.google.gson.JsonSerializer;
import com.google.gson.annotations.SerializedName;
import com.microsoft.windowsazure.mobileservices.authentication.LoginManager;
import com.microsoft.windowsazure.mobileservices.authentication.MobileServiceAuthenticationProvider;
import com.microsoft.windowsazure.mobileservices.authentication.MobileServiceUser;
import com.microsoft.windowsazure.mobileservices.http.AndroidHttpClientFactory;
import com.microsoft.windowsazure.mobileservices.http.AndroidHttpClientFactoryImpl;
import com.microsoft.windowsazure.mobileservices.http.MobileServiceConnection;
import com.microsoft.windowsazure.mobileservices.http.MobileServiceHttpClient;
import com.microsoft.windowsazure.mobileservices.http.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.notifications.MobileServicePush;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceTable;
import com.microsoft.windowsazure.mobileservices.table.serialization.DateSerializer;
import com.microsoft.windowsazure.mobileservices.table.serialization.JsonEntityParser;
import com.microsoft.windowsazure.mobileservices.table.serialization.LongSerializer;
import com.microsoft.windowsazure.mobileservices.table.sync.MobileServiceJsonSyncTable;
import com.microsoft.windowsazure.mobileservices.table.sync.MobileServiceSyncContext;
import com.microsoft.windowsazure.mobileservices.table.sync.MobileServiceSyncTable;

/**
 * Entry-point for Microsoft Azure Mobile Services interactions
 */
public class MobileServiceClient {
	/**
	 * LoginManager used for login methods
	 */
	private LoginManager mLoginManager;

	/**
	 * Mobile Service application key
	 */
	private String mAppKey;

	/**
	 * Mobile Service URL
	 */
	private URL mAppUrl;

	/**
	 * Flag to indicate that a login operation is in progress
	 */
	private boolean mLoginInProgress;

	/**
	 * The current authenticated user
	 */
	private MobileServiceUser mCurrentUser;

	/**
	 * Service filter to execute the request
	 */
	private ServiceFilter mServiceFilter;

	/**
	 * GsonBuilder used to in JSON Serialization/Deserialization
	 */
	private GsonBuilder mGsonBuilder;

	/**
	 * Context where the MobileServiceClient is created
	 */
	private Context mContext;

	/**
	 * AndroidHttpClientFactory used for request execution
	 */
	private AndroidHttpClientFactory mAndroidHttpClientFactory;

	/**
	 * MobileServicePush used for push notifications
	 */
	private MobileServicePush mPush;

	/**
	 * MobileServiceSyncContext used for synchronization between local and
	 * remote databases.
	 */
	private MobileServiceSyncContext mSyncContext;

	/**
	 * UTF-8 encoding
	 */
	public static final String UTF8_ENCODING = "UTF-8";

	/**
	 * Custom API Url
	 */
	private static final String CUSTOM_API_URL = "api/";

	/**
	 * Google account type
	 */
	public static final String GOOGLE_ACCOUNT_TYPE = "com.google";

	/**
	 * Authentication token type required for client login
	 */
	public static final String GOOGLE_USER_INFO_SCOPE = "oauth2:https://www.googleapis.com/auth/userinfo.profile";

	/**
	 * Creates a GsonBuilder with custom serializers to use with Microsoft Azure
	 * Mobile Services
	 * 
	 * @return
	 */
	public static GsonBuilder createMobileServiceGsonBuilder() {
		GsonBuilder gsonBuilder = new GsonBuilder();

		// Register custom date serializer/deserializer
		gsonBuilder.registerTypeAdapter(Date.class, new DateSerializer());
		LongSerializer longSerializer = new LongSerializer();
		gsonBuilder.registerTypeAdapter(Long.class, longSerializer);
		gsonBuilder.registerTypeAdapter(long.class, longSerializer);

		return gsonBuilder;
	}

	/**
	 * Constructor for the MobileServiceClient
	 * 
	 * @param appUrl
	 *            Mobile Service URL
	 * @param appKey
	 *            Mobile Service application key
	 * @param context
	 *            The Context where the MobileServiceClient is created
	 * @throws java.net.MalformedURLException
	 * 
	 */
	public MobileServiceClient(String appUrl, String appKey, Context context) throws MalformedURLException {
		this(new URL(appUrl), appKey, context);
	}

	/**
	 * Constructor for the MobileServiceClient
	 * 
	 * @param client
	 *            An existing MobileServiceClient
	 */
	public MobileServiceClient(MobileServiceClient client) {
		initialize(client.getAppUrl(), client.getAppKey(), client.getCurrentUser(), client.getGsonBuilder(), client.getContext(),
				client.getAndroidHttpClientFactory());
	}

	/**
	 * Constructor for the MobileServiceClient
	 * 
	 * @param appUrl
	 *            Mobile Service URL
	 * @param appKey
	 *            Mobile Service application key
	 * @param context
	 *            The Context where the MobileServiceClient is created
	 */
	public MobileServiceClient(URL appUrl, String appKey, Context context) {
		GsonBuilder gsonBuilder = createMobileServiceGsonBuilder();
		gsonBuilder.serializeNulls(); // by default, add null serialization

		initialize(appUrl, appKey, null, gsonBuilder, context, new AndroidHttpClientFactoryImpl());
	}

	/**
	 * Invokes an interactive authentication process using the specified
	 * Authentication Provider
	 * 
	 * @param provider
	 *            The provider used for the authentication process
	 * @param callback
	 *            Callback to invoke when the authentication process finishes
	 */
	public ListenableFuture<MobileServiceUser> login(MobileServiceAuthenticationProvider provider) {
		return login(provider.toString());
	}

	/**
	 * Invokes an interactive authentication process using the specified
	 * Authentication Provider
	 * 
	 * @deprecated use {@link login( com.microsoft.windowsazure.mobileservices.authentication.MobileServiceAuthenticationProvider
	 *             provider)} instead
	 * 
	 * @param provider
	 *            The provider used for the authentication process
	 * @param callback
	 *            Callback to invoke when the authentication process finishes
	 */
	public void login(MobileServiceAuthenticationProvider provider, UserAuthenticationCallback callback) {
		login(provider.toString(), callback);
	}

	/**
	 * Invokes an interactive authentication process using the specified
	 * Authentication Provider
	 * 
	 * @param provider
	 *            The provider used for the authentication process
	 */
	public ListenableFuture<MobileServiceUser> login(String provider) {
		mLoginInProgress = true;

		final SettableFuture<MobileServiceUser> resultFuture = SettableFuture.create();

		ListenableFuture<MobileServiceUser> future = mLoginManager.authenticate(provider, mContext);

		Futures.addCallback(future, new FutureCallback<MobileServiceUser>() {
			@Override
			public void onFailure(Throwable e) {
				mLoginInProgress = false;

				resultFuture.setException(e);
			}

			@Override
			public void onSuccess(MobileServiceUser user) {
				mCurrentUser = user;
				mLoginInProgress = false;

				resultFuture.set(user);
			}
		});

		return resultFuture;
	}

	/**
	 * 
	 * Invokes an interactive authentication process using the specified
	 * Authentication Provider
	 * 
	 * @deprecated use {@link login(String provider)} instead
	 * 
	 * @param provider
	 *            The provider used for the authentication process
	 * @param callback
	 *            Callback to invoke when the authentication process finishes
	 */
	public void login(String provider, final UserAuthenticationCallback callback) {
		ListenableFuture<MobileServiceUser> loginFuture = login(provider);

		Futures.addCallback(loginFuture, new FutureCallback<MobileServiceUser>() {
			@Override
			public void onFailure(Throwable exception) {
				if (exception instanceof Exception) {
					callback.onCompleted(null, (Exception) exception, MobileServiceException.getServiceResponse(exception));
				} else {
					callback.onCompleted(null, new Exception(exception), MobileServiceException.getServiceResponse(exception));
				}
			}

			@Override
			public void onSuccess(MobileServiceUser user) {
				callback.onCompleted(user, null, null);
			}
		});
	}

	/**
	 * Invokes Microsoft Azure Mobile Service authentication using a
	 * provider-specific oAuth token
	 * 
	 * @param provider
	 *            The provider used for the authentication process
	 * @param oAuthToken
	 *            A Json object representing the oAuth token used for
	 *            authentication
	 */
	public ListenableFuture<MobileServiceUser> login(MobileServiceAuthenticationProvider provider, JsonObject oAuthToken) {
		return login(provider.toString(), oAuthToken);
	}

	/**
	 * Invokes Microsoft Azure Mobile Service authentication using a
	 * provider-specific oAuth token
	 * 
	 * @deprecated use {@link login( com.microsoft.windowsazure.mobileservices.authentication.MobileServiceAuthenticationProvider
	 *             provider, com.google.gson.JsonObject oAuthToken)} instead
	 * 
	 * @param provider
	 *            The provider used for the authentication process
	 * @param oAuthToken
	 *            A Json object representing the oAuth token used for
	 *            authentication
	 * @param callback
	 *            Callback to invoke when the authentication process finishes
	 */
	public void login(MobileServiceAuthenticationProvider provider, JsonObject oAuthToken, UserAuthenticationCallback callback) {
		login(provider.toString(), oAuthToken, callback);
	}

	/**
	 * Invokes Microsoft Azure Mobile Service authentication using a
	 * provider-specific oAuth token
	 * 
	 * @param provider
	 *            The provider used for the authentication process
	 * @param oAuthToken
	 *            A Json object representing the oAuth token used for
	 *            authentication
	 */
	public ListenableFuture<MobileServiceUser> login(String provider, JsonObject oAuthToken) {
		if (oAuthToken == null) {
			throw new IllegalArgumentException("oAuthToken cannot be null");
		}

		return login(provider, oAuthToken.toString());
	}

	/**
	 * Invokes Microsoft Azure Mobile Service authentication using a
	 * provider-specific oAuth token
	 * 
	 * @deprecated use {@link login(String provider, com.google.gson.JsonObject oAuthToken)}
	 *             instead
	 * 
	 * @param provider
	 *            The provider used for the authentication process
	 * @param oAuthToken
	 *            A Json object representing the oAuth token used for
	 *            authentication
	 * @param callback
	 *            Callback to invoke when the authentication process finishes
	 */
	public void login(String provider, JsonObject oAuthToken, UserAuthenticationCallback callback) {
		if (oAuthToken == null) {
			throw new IllegalArgumentException("oAuthToken cannot be null");
		}

		login(provider, oAuthToken.toString(), callback);
	}

	/**
	 * Invokes Microsoft Azure Mobile Service authentication using a
	 * provider-specific oAuth token
	 * 
	 * @param provider
	 *            The provider used for the authentication process
	 * @param oAuthToken
	 *            The oAuth token used for authentication
	 */
	public ListenableFuture<MobileServiceUser> login(MobileServiceAuthenticationProvider provider, String oAuthToken) {
		return login(provider.toString(), oAuthToken);
	}

	/**
	 * Invokes Microsoft Azure Mobile Service authentication using a
	 * provider-specific oAuth token
	 * 
	 * @deprecated use {@link login( com.microsoft.windowsazure.mobileservices.authentication.MobileServiceAuthenticationProvider
	 *             provider, String oAuthToken)} instead
	 * 
	 * @param provider
	 *            The provider used for the authentication process
	 * @param oAuthToken
	 *            The oAuth token used for authentication
	 * @param callback
	 *            Callback to invoke when the authentication process finishes
	 */
	public void login(MobileServiceAuthenticationProvider provider, String oAuthToken, UserAuthenticationCallback callback) {
		login(provider.toString(), oAuthToken, callback);
	}

	/**
	 * Invokes Microsoft Azure Mobile Service authentication using a
	 * provider-specific oAuth token
	 * 
	 * @param provider
	 *            The provider used for the authentication process
	 * @param oAuthToken
	 *            The oAuth token used for authentication
	 */
	public ListenableFuture<MobileServiceUser> login(String provider, String oAuthToken) {
		if (oAuthToken == null) {
			throw new IllegalArgumentException("oAuthToken cannot be null");
		}

		final SettableFuture<MobileServiceUser> resultFuture = SettableFuture.create();

		mLoginInProgress = true;

		ListenableFuture<MobileServiceUser> future = mLoginManager.authenticate(provider, oAuthToken);

		Futures.addCallback(future, new FutureCallback<MobileServiceUser>() {
			@Override
			public void onFailure(Throwable e) {
				mLoginInProgress = false;

				resultFuture.setException(e);
			}

			@Override
			public void onSuccess(MobileServiceUser user) {
				mCurrentUser = user;
				mLoginInProgress = false;

				resultFuture.set(user);
			}
		});

		return resultFuture;
	}

	/**
	 * 
	 * Invokes Microsoft Azure Mobile Service authentication using a
	 * provider-specific oAuth token
	 * 
	 * @deprecated use {@link login(String provider, String oAuthToken)} instead
	 * 
	 * @param provider
	 *            The provider used for the authentication process
	 * @param oAuthToken
	 *            The oAuth token used for authentication
	 * @param callback
	 *            Callback to invoke when the authentication process finishes
	 */
	public void login(String provider, String oAuthToken, final UserAuthenticationCallback callback) {
		ListenableFuture<MobileServiceUser> loginFuture = login(provider, oAuthToken);

		Futures.addCallback(loginFuture, new FutureCallback<MobileServiceUser>() {
			@Override
			public void onFailure(Throwable exception) {
				if (exception instanceof Exception) {
					callback.onCompleted(null, (Exception) exception, MobileServiceException.getServiceResponse(exception));
				} else {
					callback.onCompleted(null, new Exception(exception), MobileServiceException.getServiceResponse(exception));
				}
			}

			@Override
			public void onSuccess(MobileServiceUser user) {
				callback.onCompleted(user, null, null);
			}
		});
	}

	/**
	 * Invokes Microsoft Azure Mobile Service authentication using a the Google
	 * account registered in the device
	 * 
	 * @param activity
	 *            The activity that triggered the authentication
	 */
	public ListenableFuture<MobileServiceUser> loginWithGoogleAccount(Activity activity) {
		return loginWithGoogleAccount(activity, GOOGLE_USER_INFO_SCOPE);
	}

	/**
	 * Invokes Microsoft Azure Mobile Service authentication using a the Google
	 * account registered in the device
	 * 
	 * @deprecated use {@link loginWithGoogleAccount( android.app.Activity activity)} instead
	 * 
	 * @param activity
	 *            The activity that triggered the authentication
	 * 
	 * @param callback
	 *            Callback to invoke when the authentication process finishes
	 */
	public void loginWithGoogleAccount(Activity activity, final UserAuthenticationCallback callback) {
		loginWithGoogleAccount(activity, GOOGLE_USER_INFO_SCOPE, callback);
	}

	/**
	 * Invokes Microsoft Azure Mobile Service authentication using a the Google
	 * account registered in the device
	 * 
	 * @param activity
	 *            The activity that triggered the authentication
	 * @param scopes
	 *            The scopes used as authentication token type for login
	 */
	public ListenableFuture<MobileServiceUser> loginWithGoogleAccount(Activity activity, String scopes) {
		AccountManager acMgr = AccountManager.get(activity.getApplicationContext());
		Account[] accounts = acMgr.getAccountsByType(GOOGLE_ACCOUNT_TYPE);

		Account account;
		if (accounts.length == 0) {
			account = null;
		} else {
			account = accounts[0];
		}

		return loginWithGoogleAccount(activity, account, scopes);
	}

	/**
	 * Invokes Microsoft Azure Mobile Service authentication using a the Google
	 * account registered in the device
	 * 
	 * @deprecated use {@link loginWithGoogleAccount( android.app.Activity activity, String
	 *             scopes)} instead
	 * 
	 * @param activity
	 *            The activity that triggered the authentication
	 * 
	 * @param scopes
	 *            The scopes used as authentication token type for login
	 * @param callback
	 *            Callback to invoke when the authentication process finishes
	 */
	public void loginWithGoogleAccount(Activity activity, String scopes, final UserAuthenticationCallback callback) {
		ListenableFuture<MobileServiceUser> loginFuture = loginWithGoogleAccount(activity, scopes);

		Futures.addCallback(loginFuture, new FutureCallback<MobileServiceUser>() {
			@Override
			public void onFailure(Throwable exception) {
				if (exception instanceof Exception) {
					callback.onCompleted(null, (Exception) exception, MobileServiceException.getServiceResponse(exception));
				} else {
					callback.onCompleted(null, new Exception(exception), MobileServiceException.getServiceResponse(exception));
				}
			}

			@Override
			public void onSuccess(MobileServiceUser user) {
				callback.onCompleted(user, null, null);
			}
		});
	}

	/**
	 * Invokes Microsoft Azure Mobile Service authentication using a the Google
	 * account registered in the device
	 * 
	 * @param activity
	 *            The activity that triggered the authentication
	 * @param account
	 *            The account used for the login operation
	 */
	public ListenableFuture<MobileServiceUser> loginWithGoogleAccount(Activity activity, Account account) {
		return loginWithGoogleAccount(activity, account, GOOGLE_USER_INFO_SCOPE);
	}

	/**
	 * Invokes Microsoft Azure Mobile Service authentication using a the Google
	 * account registered in the device
	 * 
	 * @deprecated use {@link loginWithGoogleAccount( android.app.Activity activity, android.accounts.Account
	 *             account)} instead
	 * 
	 * @param activity
	 *            The activity that triggered the authentication
	 * @param account
	 *            The account used for the login operation
	 * 
	 * @param callback
	 *            Callback to invoke when the authentication process finishes
	 */
	public void loginWithGoogleAccount(Activity activity, Account account, final UserAuthenticationCallback callback) {
		loginWithGoogleAccount(activity, account, GOOGLE_USER_INFO_SCOPE, callback);
	}

	/**
	 * Invokes Microsoft Azure Mobile Service authentication using a the Google
	 * account registered in the device
	 * 
	 * @param activity
	 *            The activity that triggered the authentication
	 * @param account
	 *            The account used for the login operation
	 * @param scopes
	 *            The scopes used as authentication token type for login
	 */
	public ListenableFuture<MobileServiceUser> loginWithGoogleAccount(Activity activity, Account account, String scopes) {
		final SettableFuture<MobileServiceUser> future = SettableFuture.create();

		try {
			if (account == null) {
				throw new IllegalArgumentException("account");
			}

			final MobileServiceClient client = this;

			AccountManagerCallback<Bundle> authCallback = new AccountManagerCallback<Bundle>() {

				@Override
				public void run(AccountManagerFuture<Bundle> futureBundle) {
					try {
						if (futureBundle.isCancelled()) {
							future.setException(new MobileServiceException("User cancelled"));
							// callback.onCompleted(null, new
							// MobileServiceException("User cancelled"), null);
						} else {
							Bundle bundle = futureBundle.getResult();

							String token = (String) (bundle.get(AccountManager.KEY_AUTHTOKEN));

							JsonObject json = new JsonObject();
							json.addProperty("access_token", token);

							ListenableFuture<MobileServiceUser> loginFuture = client.login(MobileServiceAuthenticationProvider.Google, json);

							Futures.addCallback(loginFuture, new FutureCallback<MobileServiceUser>() {
								@Override
								public void onFailure(Throwable e) {
									future.setException(e);
								}

								@Override
								public void onSuccess(MobileServiceUser user) {
									future.set(user);
								}
							});
						}
					} catch (Exception e) {
						future.setException(e);
					}
				}
			};

			AccountManager acMgr = AccountManager.get(activity.getApplicationContext());
			acMgr.getAuthToken(account, scopes, null, activity, authCallback, null);

		} catch (Exception e) {
			future.setException(e);
		}

		return future;
	}

	/**
	 * 
	 * Invokes Microsoft Azure Mobile Service authentication using a the Google
	 * account registered in the device
	 * 
	 * @deprecated use {@link loginWithGoogleAccount( android.app.Activity activity, android.accounts.Account
	 *             account, String scopes)} instead
	 * 
	 * @param activity
	 *            The activity that triggered the authentication
	 * @param account
	 *            The account used for the login operation
	 * @param scopes
	 *            The scopes used as authentication token type for login
	 * @param callback
	 *            Callback to invoke when the authentication process finishes
	 */
	public void loginWithGoogleAccount(Activity activity, Account account, String scopes, final UserAuthenticationCallback callback) {
		ListenableFuture<MobileServiceUser> loginFuture = loginWithGoogleAccount(activity, account, scopes);

		Futures.addCallback(loginFuture, new FutureCallback<MobileServiceUser>() {
			@Override
			public void onFailure(Throwable exception) {
				if (exception instanceof Exception) {
					callback.onCompleted(null, (Exception) exception, MobileServiceException.getServiceResponse(exception));
				} else {
					callback.onCompleted(null, new Exception(exception), MobileServiceException.getServiceResponse(exception));
				}
			}

			@Override
			public void onSuccess(MobileServiceUser user) {
				callback.onCompleted(user, null, null);
			}
		});
	}

	/**
	 * Log the user out of the Mobile Service
	 */
	public void logout() {
		mCurrentUser = null;
	}

	/**
	 * Returns the Mobile Service application key
	 */
	public String getAppKey() {
		return mAppKey;
	}

	/**
	 * Returns The Mobile Service URL
	 */
	public URL getAppUrl() {
		return mAppUrl;
	}

	/**
	 * Indicates if a login operation is in progress
	 */
	public boolean isLoginInProgress() {
		return mLoginInProgress;
	}

	/**
	 * Returns the current authenticated user
	 */
	public MobileServiceUser getCurrentUser() {
		return mCurrentUser;
	}

	/**
	 * Sets a user to authenticate the Mobile Service operations
	 * 
	 * @param user
	 *            The user used to authenticate requests
	 */
	public void setCurrentUser(MobileServiceUser user) {
		mCurrentUser = user;
	}

	/**
	 * Returns a MobileServiceSyncContext instance.
	 * 
	 * @return the MobileServiceSyncContext instance
	 */
	public MobileServiceSyncContext getSyncContext() {
		return this.mSyncContext;
	}

	/**
	 * Creates a MobileServiceJsonTable
	 * 
	 * @param name
	 *            Table name
	 * @return MobileServiceJsonTable with the given name
	 */
	public MobileServiceJsonTable getTable(String name) {
		return new MobileServiceJsonTable(name, this);
	}

	/**
	 * Returns a MobileServiceJsonSyncTable instance, which provides untyped
	 * data operations for a local table.
	 * 
	 * @param name
	 *            Table name
	 * @return The MobileServiceJsonSyncTable instance
	 */
	public MobileServiceJsonSyncTable getSyncTable(String name) {
		return new MobileServiceJsonSyncTable(name, this);
	}

	/**
	 * Creates a MobileServiceTable
	 * 
	 * @param clazz
	 *            The class used for table name and data serialization
	 * 
	 * @return MobileServiceTable with the given name
	 */
	public <E> MobileServiceTable<E> getTable(Class<E> clazz) {
		return this.getTable(clazz.getSimpleName(), clazz);
	}

	/**
	 * Creates a MobileServiceTable
	 * 
	 * @param name
	 *            Table name
	 * @param clazz
	 *            The class used for data serialization
	 * 
	 * @return MobileServiceTable with the given name
	 */
	public <E> MobileServiceTable<E> getTable(String name, Class<E> clazz) {
		validateClass(clazz);
		return new MobileServiceTable<E>(name, this, clazz);
	}

	/**
	 * Returns a MobileServiceSyncTable<E> instance, which provides strongly
	 * typed data operations for a local table.
	 * 
	 * @param clazz
	 *            The class used for table name and data serialization
	 * 
	 * @return The MobileServiceSyncTable instance
	 */
	public <E> MobileServiceSyncTable<E> getSyncTable(Class<E> clazz) {
		return this.getSyncTable(clazz.getSimpleName(), clazz);
	}

	/**
	 * Returns a MobileServiceSyncTable<E> instance, which provides strongly
	 * typed data operations for a local table.
	 * 
	 * @param name
	 *            Table name
	 * @param clazz
	 *            The class used for data serialization
	 * 
	 * @return The MobileServiceSyncTable instance
	 */
	public <E> MobileServiceSyncTable<E> getSyncTable(String name, Class<E> clazz) {
		validateClass(clazz);
		return new MobileServiceSyncTable<E>(name, this, clazz);
	}

	/**
	 * Invokes a custom API using POST HTTP method
	 * 
	 * @param apiName
	 *            The API name
	 * @param clazz
	 *            The API result class
	 */
	public <E> ListenableFuture<E> invokeApi(String apiName, Class<E> clazz) {
		return invokeApi(apiName, null, HttpPost.METHOD_NAME, null, clazz);
	}

	/**
	 * Invokes a custom API using POST HTTP method
	 * 
	 * @deprecated use {@link invokeApi(String apiName, Class<E> clazz)} instead
	 * 
	 * @param apiName
	 *            The API name
	 * @param clazz
	 *            The API result class
	 * @param callback
	 *            The callback to invoke after the API execution
	 */
	public <E> void invokeApi(String apiName, Class<E> clazz, ApiOperationCallback<E> callback) {
		invokeApi(apiName, null, HttpPost.METHOD_NAME, null, clazz, callback);
	}

	/**
	 * Invokes a custom API using POST HTTP method
	 * 
	 * @param apiName
	 *            The API name
	 * @param body
	 *            The object to send as the request body
	 * @param clazz
	 *            The API result class
	 */
	public <E> ListenableFuture<E> invokeApi(String apiName, Object body, Class<E> clazz) {
		return invokeApi(apiName, body, HttpPost.METHOD_NAME, null, clazz);
	}

	/**
	 * Invokes a custom API using POST HTTP method
	 * 
	 * @deprecated use {@link invokeApi(String apiName, Object body, Class<E>
	 *             clazz)} instead
	 * 
	 * @param apiName
	 *            The API name
	 * @param body
	 *            The object to send as the request body
	 * @param clazz
	 *            The API result class
	 * @param callback
	 *            The callback to invoke after the API execution
	 */
	public <E> void invokeApi(String apiName, Object body, Class<E> clazz, ApiOperationCallback<E> callback) {
		invokeApi(apiName, body, HttpPost.METHOD_NAME, null, clazz, callback);
	}

	/**
	 * Invokes a custom API
	 * 
	 * @param apiName
	 *            The API name
	 * @param httpMethod
	 *            The HTTP Method used to invoke the API
	 * @param parameters
	 *            The query string parameters sent in the request
	 * @param clazz
	 *            The API result class
	 */
	public <E> ListenableFuture<E> invokeApi(String apiName, String httpMethod, List<Pair<String, String>> parameters, Class<E> clazz) {
		return invokeApi(apiName, null, httpMethod, parameters, clazz);
	}

	/**
	 * Invokes a custom API
	 * 
	 * @deprecated use {@link invokeApi(String apiName, String httpMethod,
	 *             java.util.List< android.util.Pair<String, String>> parameters, Class<E> clazz)}
	 *             instead
	 * 
	 * @param apiName
	 *            The API name
	 * 
	 * @param httpMethod
	 *            The HTTP Method used to invoke the API
	 * @param parameters
	 *            The query string parameters sent in the request
	 * @param clazz
	 *            The API result class
	 * @param callback
	 *            The callback to invoke after the API execution
	 */
	public <E> void invokeApi(String apiName, String httpMethod, List<Pair<String, String>> parameters, Class<E> clazz, ApiOperationCallback<E> callback) {
		invokeApi(apiName, null, httpMethod, parameters, clazz, callback);
	}

	/**
	 * Invokes a custom API
	 * 
	 * @param apiName
	 *            The API name
	 * @param body
	 *            The object to send as the request body
	 * @param httpMethod
	 *            The HTTP Method used to invoke the API
	 * @param parameters
	 *            The query string parameters sent in the request
	 * @param clazz
	 *            The API result class
	 */
	public <E> ListenableFuture<E> invokeApi(String apiName, Object body, String httpMethod, List<Pair<String, String>> parameters, final Class<E> clazz) {
		if (clazz == null) {
			throw new IllegalArgumentException("clazz cannot be null");
		}

		JsonElement json = null;
		if (body != null) {
			if (body instanceof JsonElement) {
				json = (JsonElement) body;
			} else {
				json = getGsonBuilder().create().toJsonTree(body);
			}
		}

		final SettableFuture<E> future = SettableFuture.create();
		ListenableFuture<JsonElement> internalFuture = this.invokeApiInternal(apiName, json, httpMethod, parameters, EnumSet.of(MobileServiceFeatures.TypedApiCall));

		Futures.addCallback(internalFuture, new FutureCallback<JsonElement>() {
			@Override
			public void onFailure(Throwable e) {
				future.setException(e);
			}

			@Override
			@SuppressWarnings("unchecked")
			public void onSuccess(JsonElement jsonElement) {
				Class<?> concreteClass = clazz;
				if (clazz.isArray()) {
					concreteClass = clazz.getComponentType();
				}

				List<?> entities = JsonEntityParser.parseResults(jsonElement, getGsonBuilder().create(), concreteClass);

				if (clazz.isArray()) {
					E array = (E) Array.newInstance(concreteClass, entities.size());
					for (int i = 0; i < entities.size(); i++) {
						Array.set(array, i, entities.get(i));
					}

					future.set(array);
				} else {
					future.set((E) entities.get(0));
				}
			}
		});

		return future;
	}

	/**
	 * Invokes a custom API
	 * 
	 * @deprecated use {@link invokeApi(String apiName, Object body, String
	 *             httpMethod, java.util.List< android.util.Pair<String, String>> parameters, final
	 *             Class<E> clazz)} instead
	 * 
	 * @param apiName
	 *            The API name
	 * @param body
	 *            The object to send as the request body
	 * @param httpMethod
	 *            The HTTP Method used to invoke the API
	 * @param parameters
	 *            The query string parameters sent in the request
	 * @param clazz
	 *            The API result class
	 * @param callback
	 *            The callback to invoke after the API execution
	 */
	public <E> void invokeApi(String apiName, Object body, String httpMethod, List<Pair<String, String>> parameters, final Class<E> clazz,
			final ApiOperationCallback<E> callback) {

		ListenableFuture<E> invokeApiFuture = invokeApi(apiName, body, httpMethod, parameters, clazz);

		Futures.addCallback(invokeApiFuture, new FutureCallback<E>() {
			@Override
			public void onFailure(Throwable exception) {
				if (exception instanceof Exception) {
					callback.onCompleted(null, (Exception) exception, MobileServiceException.getServiceResponse(exception));
				} else {
					callback.onCompleted(null, new Exception(exception), MobileServiceException.getServiceResponse(exception));
				}
			}

			@Override
			public void onSuccess(E result) {
				callback.onCompleted(result, null, null);
			}
		});
	}

	/**
	 * Invokes a custom API using POST HTTP method
	 * 
	 * @param apiName
	 *            The API name
	 */
	public ListenableFuture<JsonElement> invokeApi(String apiName) {
		return invokeApi(apiName, (JsonElement) null);
	}

	/**
	 * Invokes a custom API using POST HTTP method
	 * 
	 * @deprecated use {@link invokeApi(String apiName)} instead
	 * 
	 * @param apiName
	 *            The API name
	 * 
	 * @param callback
	 *            The callback to invoke after the API execution
	 */
	public void invokeApi(String apiName, ApiJsonOperationCallback callback) {
		invokeApi(apiName, null, callback);
	}

	/**
	 * Invokes a custom API using POST HTTP method
	 * 
	 * @param apiName
	 *            The API name
	 * @param body
	 *            The json element to send as the request body
	 */
	public ListenableFuture<JsonElement> invokeApi(String apiName, JsonElement body) {
		return invokeApi(apiName, body, HttpPost.METHOD_NAME, null);
	}

	/**
	 * Invokes a custom API using POST HTTP method
	 * 
	 * @deprecated use {@link invokeApi(String apiName, com.google.gson.JsonElement body)}
	 *             instead
	 * 
	 * @param apiName
	 *            The API name
	 * @param body
	 *            The json element to send as the request body
	 * @param callback
	 *            The callback to invoke after the API execution
	 */
	public void invokeApi(String apiName, JsonElement body, ApiJsonOperationCallback callback) {
		invokeApi(apiName, body, HttpPost.METHOD_NAME, null, callback);
	}

	/**
	 * Invokes a custom API
	 * 
	 * @param apiName
	 *            The API name
	 * @param httpMethod
	 *            The HTTP Method used to invoke the API
	 * @param parameters
	 *            The query string parameters sent in the request
	 */
	public ListenableFuture<JsonElement> invokeApi(String apiName, String httpMethod, List<Pair<String, String>> parameters) {
		return invokeApi(apiName, null, httpMethod, parameters);
	}

	/**
	 * Invokes a custom API
	 * 
	 * @deprecated use {@link invokeApi(String apiName, String httpMethod,
	 *             java.util.List< android.util.Pair<String, String>> parameters)} instead
	 * 
	 * @param apiName
	 *            The API name
	 * 
	 * @param httpMethod
	 *            The HTTP Method used to invoke the API
	 * @param parameters
	 *            The query string parameters sent in the request
	 * @param callback
	 *            The callback to invoke after the API execution
	 */
	public void invokeApi(String apiName, String httpMethod, List<Pair<String, String>> parameters, ApiJsonOperationCallback callback) {
		invokeApi(apiName, null, httpMethod, parameters, callback);
	}

	/**
	 * Invokes a custom API
	 *
	 * @param apiName
	 *            The API name
	 * @param body
	 *            The json element to send as the request body
	 * @param httpMethod
	 *            The HTTP Method used to invoke the API
	 * @param parameters
	 *            The query string parameters sent in the request
	 */
	public ListenableFuture<JsonElement> invokeApi(String apiName, JsonElement body, String httpMethod, List<Pair<String, String>> parameters) {
		return this.invokeApiInternal(apiName, body, httpMethod, parameters, EnumSet.of(MobileServiceFeatures.JsonApiCall));
	}

	/**
	 * Invokes a custom API
	 * 
	 * @param apiName
	 *            The API name
	 * @param body
	 *            The json element to send as the request body
	 * @param httpMethod
	 *            The HTTP Method used to invoke the API
	 * @param parameters
	 *            The query string parameters sent in the request
	 * @param features
	 *            The features used in the request
	 */
	private ListenableFuture<JsonElement> invokeApiInternal(String apiName, JsonElement body, String httpMethod, List<Pair<String, String>> parameters, EnumSet<MobileServiceFeatures> features) {

		byte[] content = null;
		if (body != null) {
			try {
				content = body.toString().getBytes(UTF8_ENCODING);
			} catch (UnsupportedEncodingException e) {
				throw new IllegalArgumentException(e);
			}
		}

		List<Pair<String, String>> requestHeaders = new ArrayList<Pair<String, String>>();
		if (body != null) {
			requestHeaders.add(new Pair<String, String>(HTTP.CONTENT_TYPE, MobileServiceConnection.JSON_CONTENTTYPE));
		}

		if (parameters != null && !parameters.isEmpty()) {
			features.add(MobileServiceFeatures.AdditionalQueryParameters);
		}

		final SettableFuture<JsonElement> future = SettableFuture.create();
		ListenableFuture<ServiceFilterResponse> internalFuture = invokeApiInternal(apiName, content, httpMethod, requestHeaders, parameters, features);

		Futures.addCallback(internalFuture, new FutureCallback<ServiceFilterResponse>() {
			@Override
			public void onFailure(Throwable e) {
				future.setException(e);
			}

			@Override
			public void onSuccess(ServiceFilterResponse response) {
				String content = response.getContent();
				JsonElement json = new JsonParser().parse(content);
				future.set(json);
			}
		});

		return future;
	}

	/**
	 * Invokes a custom API
	 * 
	 * @deprecated use {@link invokeApi(String apiName, com.google.gson.JsonElement body, String
	 *             httpMethod, java.util.List< android.util.Pair<String, String>> parameters)} instead
	 * 
	 * @param apiName
	 *            The API name
	 * @param body
	 *            The json element to send as the request body
	 * @param httpMethod
	 *            The HTTP Method used to invoke the API
	 * @param parameters
	 *            The query string parameters sent in the request
	 * @param callback
	 *            The callback to invoke after the API execution
	 */
	public void invokeApi(String apiName, JsonElement body, String httpMethod, List<Pair<String, String>> parameters, final ApiJsonOperationCallback callback) {

		ListenableFuture<JsonElement> invokeApiFuture = invokeApi(apiName, body, httpMethod, parameters);

		Futures.addCallback(invokeApiFuture, new FutureCallback<JsonElement>() {
			@Override
			public void onFailure(Throwable exception) {
				if (exception instanceof Exception) {
					callback.onCompleted(null, (Exception) exception, MobileServiceException.getServiceResponse(exception));
				} else {
					callback.onCompleted(null, new Exception(exception), MobileServiceException.getServiceResponse(exception));
				}
			}

			@Override
			public void onSuccess(JsonElement result) {
				callback.onCompleted(result, null, null);
			}
		});
	}

	/**
	 * Invokes a custom API
	 * 
	 * @param apiName
	 *            The API name
	 * @param content
	 *            The byte array to send as the request body
	 * @param httpMethod
	 *            The HTTP Method used to invoke the API
	 * @param requestHeaders
	 *            The extra headers to send in the request
	 * @param parameters
	 *            The query string parameters sent in the request
	 */
	public ListenableFuture<ServiceFilterResponse> invokeApi(String apiName, byte[] content, String httpMethod, List<Pair<String, String>> requestHeaders,
			List<Pair<String, String>> parameters) {
		return invokeApiInternal(apiName, content, httpMethod, requestHeaders, parameters, EnumSet.of(MobileServiceFeatures.GenericApiCall));
	}

	/**
	 * Invokes a custom API
	 * 
	 * @param apiName
	 *            The API name
	 * @param content
	 *            The byte array to send as the request body
	 * @param httpMethod
	 *            The HTTP Method used to invoke the API
	 * @param requestHeaders
	 *            The extra headers to send in the request
	 * @param parameters
	 *            The query string parameters sent in the request
	 * @param callback
	 *            The callback to invoke after the API execution
	 */
	public void invokeApi(String apiName, byte[] content, String httpMethod, List<Pair<String, String>> requestHeaders, List<Pair<String, String>> parameters,
			final ServiceFilterResponseCallback callback) {

		ListenableFuture<ServiceFilterResponse> invokeApiFuture = invokeApi(apiName, content, httpMethod, requestHeaders, parameters);

		Futures.addCallback(invokeApiFuture, new FutureCallback<ServiceFilterResponse>() {
			@Override
			public void onFailure(Throwable exception) {
				if (exception instanceof Exception) {
					callback.onResponse(MobileServiceException.getServiceResponse(exception), (Exception) exception);
				} else {
					callback.onResponse(MobileServiceException.getServiceResponse(exception), new Exception(exception));
				}
			}

			@Override
			public void onSuccess(ServiceFilterResponse result) {
				callback.onResponse(result, null);
			}
		});
	}

	/**
	 * Invokes a custom API
	 * 
	 * @param apiName
	 *            The API name
	 * @param content
	 *            The byte array to send as the request body
	 * @param httpMethod
	 *            The HTTP Method used to invoke the API
	 * @param requestHeaders
	 *            The extra headers to send in the request
	 * @param parameters
	 *            The query string parameters sent in the request
	 * @param features
	 *            The SDK features used in the request
	 */
	private ListenableFuture<ServiceFilterResponse> invokeApiInternal(String apiName, byte[] content, String httpMethod,
			List<Pair<String, String>> requestHeaders, List<Pair<String, String>> parameters, EnumSet<MobileServiceFeatures> features) {
		final SettableFuture<ServiceFilterResponse> future = SettableFuture.create();

		if (apiName == null || apiName.trim().equals("")) {
			future.setException(new IllegalArgumentException("apiName cannot be null"));
			return future;
		}

		MobileServiceHttpClient httpClient = new MobileServiceHttpClient(this);
		return httpClient.request(CUSTOM_API_URL + apiName, content, httpMethod, requestHeaders, parameters, features);
	}

	/**
	 * Validates the class has an id property defined
	 * 
	 * @param clazz
	 */
	private <E> void validateClass(Class<E> clazz) {
		if (clazz.isInterface() || Modifier.isAbstract(clazz.getModifiers())) {
			throw new IllegalArgumentException("The class type used for creating a MobileServiceTable must be a concrete class");
		}

		int idPropertyCount = 0;
		for (Field field : clazz.getDeclaredFields()) {
			SerializedName serializedName = field.getAnnotation(SerializedName.class);
			if (serializedName != null) {
				if (serializedName.value().equalsIgnoreCase("id")) {
					idPropertyCount++;
				}
			} else {
				if (field.getName().equalsIgnoreCase("id")) {
					idPropertyCount++;
				}
			}
		}

		if (idPropertyCount != 1) {
			throw new IllegalArgumentException("The class representing the MobileServiceTable must have a single id property defined");
		}
	}

	/**
	 * Adds a new filter to the MobileServiceClient
	 * 
	 * @param serviceFilter
	 * @return MobileServiceClient with filters updated
	 */
	public MobileServiceClient withFilter(final ServiceFilter serviceFilter) {
		if (serviceFilter == null) {
			throw new IllegalArgumentException("Invalid ServiceFilter");
		}

		// Generate a new instance of the MobileServiceClient
		MobileServiceClient newClient = new MobileServiceClient(this);

		// If there's no filter, set serviceFilter with the new filter.
		// Otherwise create a composed filter
		if (mServiceFilter == null) {
			newClient.mServiceFilter = serviceFilter;
		} else {
			final ServiceFilter oldServiceFilter = mServiceFilter;
			final ServiceFilter newServiceFilter = serviceFilter;

			newClient.mServiceFilter = new ServiceFilter() {
				// Create a filter that after executing the new ServiceFilter
				// executes the existing filter
				ServiceFilter externalServiceFilter = newServiceFilter;
				ServiceFilter internalServiceFilter = oldServiceFilter;

				@Override
				public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request,
						final NextServiceFilterCallback nextServiceFilterCallback) {

					// Executes new ServiceFilter
					return externalServiceFilter.handleRequest(request, new NextServiceFilterCallback() {

						@Override
						public ListenableFuture<ServiceFilterResponse> onNext(ServiceFilterRequest request) {
							// Execute existing ServiceFilter
							return internalServiceFilter.handleRequest(request, nextServiceFilterCallback);
						}
					});

				}
			};
		}

		return newClient;
	}

	/**
	 * Gets the ServiceFilter. If there is no ServiceFilter, it creates and
	 * returns a default filter
	 * 
	 * @return ServiceFilter The service filter to use with the client.
	 */
	public ServiceFilter getServiceFilter() {
		if (mServiceFilter == null) {
			return new ServiceFilter() {

				@Override
				public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {
					return nextServiceFilterCallback.onNext(request);
				}
			};
		} else {
			return mServiceFilter;
		}
	}

	/**
	 * Creates a MobileServiceConnection
	 * 
	 * @return MobileServiceConnection
	 */
	public MobileServiceConnection createConnection() {
		return new MobileServiceConnection(this);
	}

	/**
	 * Initializes the MobileServiceClient
	 * 
	 * @param appUrl
	 *            Mobile Service URL
	 * @param appKey
	 *            Mobile Service application key
	 * @param currentUser
	 *            The Mobile Service user used to authenticate requests
	 * @param gsonBuilder
	 *            the GsonBuilder used to in JSON Serialization/Deserialization
	 * @param context
	 *            The Context where the MobileServiceClient is created
	 */
	private void initialize(URL appUrl, String appKey, MobileServiceUser currentUser, GsonBuilder gsonBuiler, Context context,
			AndroidHttpClientFactory androidHttpClientFactory) {
		if (appUrl == null || appUrl.toString().trim().length() == 0) {
			throw new IllegalArgumentException("Invalid Application URL");
		}

		if (context == null) {
			throw new IllegalArgumentException("Context cannot be null");
		}

		URL normalizedAppURL = appUrl;

		if (normalizedAppURL.getPath() == "") {
			try {
				normalizedAppURL = new URL(appUrl.toString() + "/");
			} catch (MalformedURLException e) {
				// This exception won't happen, since it's just adding a
				// trailing "/" to a valid URL
			}
		}

		mAppUrl = normalizedAppURL;
		mAppKey = appKey;
		mLoginManager = new LoginManager(this);
		mServiceFilter = null;
		mLoginInProgress = false;
		mCurrentUser = currentUser;
		mContext = context;
		mGsonBuilder = gsonBuiler;
		mAndroidHttpClientFactory = androidHttpClientFactory;
		mPush = new MobileServicePush(this, context);
		mSyncContext = new MobileServiceSyncContext(this);
	}

	/**
	 * Gets the GsonBuilder used to in JSON Serialization/Deserialization
	 */
	public GsonBuilder getGsonBuilder() {
		return mGsonBuilder;
	}

	/**
	 * Registers a JsonSerializer for the specified type
	 * 
	 * @param type
	 *            The type to use in the registration
	 * @param serializer
	 *            The serializer to use in the registration
	 */
	public <T> void registerSerializer(Type type, JsonSerializer<T> serializer) {
		mGsonBuilder.registerTypeAdapter(type, serializer);
	}

	/**
	 * Registers a JsonDeserializer for the specified type
	 * 
	 * @param type
	 *            The type to use in the registration
	 * @param deserializer
	 *            The deserializer to use in the registration
	 */
	public <T> void registerDeserializer(Type type, JsonDeserializer<T> deserializer) {
		mGsonBuilder.registerTypeAdapter(type, deserializer);
	}

	/**
	 * Sets the GsonBuilder used to in JSON Serialization/Deserialization
	 * 
	 * @param mGsonBuilder
	 *            The GsonBuilder to set
	 */
	public void setGsonBuilder(GsonBuilder gsonBuilder) {
		mGsonBuilder = gsonBuilder;
	}

	/**
	 * Gets the Context object used to create the MobileServiceClient
	 */
	public Context getContext() {
		return mContext;
	}

	/**
	 * Sets the Context object for the MobileServiceClient
	 */
	public void setContext(Context mContext) {
		this.mContext = mContext;
	}

	/**
	 * Gets the AndroidHttpClientFactory
	 * 
	 * @return
	 */
	public AndroidHttpClientFactory getAndroidHttpClientFactory() {
		return mAndroidHttpClientFactory;
	}

	/**
	 * Sets the AndroidHttpClientFactory
	 */
	public void setAndroidHttpClientFactory(AndroidHttpClientFactory mAndroidHttpClientFactory) {
		this.mAndroidHttpClientFactory = mAndroidHttpClientFactory;
	}

	/**
	 * Gets the MobileServicePush used for push notifications
	 */
	public MobileServicePush getPush() {
		return mPush;
	}
}
