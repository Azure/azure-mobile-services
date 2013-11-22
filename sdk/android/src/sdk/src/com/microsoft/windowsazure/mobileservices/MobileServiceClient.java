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
/*
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
import java.util.List;

import org.apache.http.client.methods.HttpDelete;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.client.methods.HttpPut;
import org.apache.http.protocol.HTTP;

import android.accounts.Account;
import android.accounts.AccountManager;
import android.accounts.AccountManagerCallback;
import android.accounts.AccountManagerFuture;
import android.app.Activity;
import android.content.Context;
import android.net.Uri;
import android.os.Bundle;
import android.util.Pair;

import com.google.gson.GsonBuilder;
import com.google.gson.JsonDeserializer;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.google.gson.JsonSerializer;
import com.google.gson.annotations.SerializedName;

/**
 * Entry-point for Windows Azure Mobile Services interactions
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
	 * Creates a GsonBuilder with custom serializers to use with Windows Azure
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
	 * @throws MalformedURLException
	 * 
	 */
	public MobileServiceClient(String appUrl, String appKey, Context context)
			throws MalformedURLException {
		this(new URL(appUrl), appKey, context);
	}

	/**
	 * Constructor for the MobileServiceClient
	 * 
	 * @param client
	 *            An existing MobileServiceClient
	 */
	public MobileServiceClient(MobileServiceClient client) {
		initialize(client.getAppUrl(), client.getAppKey(),
				client.getCurrentUser(), client.getGsonBuilder(),
				client.getContext(), client.getAndroidHttpClientFactory());
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
	public void login(MobileServiceAuthenticationProvider provider,
			UserAuthenticationCallback callback) {
		login(provider.toString(), callback);
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
	public void login(String provider, UserAuthenticationCallback callback) {
		mLoginInProgress = true;

		final UserAuthenticationCallback externalCallback = callback;

		mLoginManager.authenticate(provider, mContext,
				new UserAuthenticationCallback() {

					@Override
					public void onCompleted(MobileServiceUser user,
							Exception exception, ServiceFilterResponse response) {
						mCurrentUser = user;
						mLoginInProgress = false;

						if (externalCallback != null) {
							externalCallback.onCompleted(user, exception,
									response);
						}
					}
				});
	}

	/**
	 * Invokes Windows Azure Mobile Service authentication using a
	 * provider-specific oAuth token
	 *
	 * @param provider
	 *            The provider used for the authentication process
	 * @param oAuthToken
	 *            A Json object representing the oAuth token used for
	 *            authentication
	 * @param callback
	 *            Callback to invoke when the authentication process finishes
	 */
	public void login(MobileServiceAuthenticationProvider provider,
			JsonObject oAuthToken, UserAuthenticationCallback callback) {
		login(provider.toString(), oAuthToken, callback);
	}

	/**
	 * Invokes Windows Azure Mobile Service authentication using a
	 * provider-specific oAuth token
	 *
	 * @param provider
	 *            The provider used for the authentication process
	 * @param oAuthToken
	 *            A Json object representing the oAuth token used for
	 *            authentication
	 * @param callback
	 *            Callback to invoke when the authentication process finishes
	 */
	public void login(String provider,
			JsonObject oAuthToken, UserAuthenticationCallback callback) {
		if (oAuthToken == null) {
			throw new IllegalArgumentException("oAuthToken cannot be null");
		}

		login(provider, oAuthToken.toString(), callback);
	}

	/**
	 * Invokes Windows Azure Mobile Service authentication using a
	 * provider-specific oAuth token
	 * 
	 * @param provider
	 *            The provider used for the authentication process
	 * @param oAuthToken
	 *            The oAuth token used for authentication
	 * @param callback
	 *            Callback to invoke when the authentication process finishes
	 */
	public void login(MobileServiceAuthenticationProvider provider,
			String oAuthToken, UserAuthenticationCallback callback) {
		login(provider.toString(), oAuthToken, callback);
	}

	/**
	 * Invokes Windows Azure Mobile Service authentication using a
	 * provider-specific oAuth token
	 * 
	 * @param provider
	 *            The provider used for the authentication process
	 * @param oAuthToken
	 *            The oAuth token used for authentication
	 * @param callback
	 *            Callback to invoke when the authentication process finishes
	 */
	public void login(String provider, String oAuthToken,
			UserAuthenticationCallback callback) {
		if (oAuthToken == null) {
			throw new IllegalArgumentException("oAuthToken cannot be null");
		}

		mLoginInProgress = true;

		final UserAuthenticationCallback externalCallback = callback;

		mLoginManager.authenticate(provider, oAuthToken,
				new UserAuthenticationCallback() {

					@Override
					public void onCompleted(MobileServiceUser user,
							Exception exception, ServiceFilterResponse response) {
						mCurrentUser = user;
						mLoginInProgress = false;

						if (externalCallback != null) {
							externalCallback.onCompleted(user, exception,
									response);
						}
					}
				});
	}
	
	/**
	 * Invokes Windows Azure Mobile Service authentication using a
	 * the Google account registered in the device
	 * 
	 * @param activity
	 *            The activity that triggered the authentication
	 * @param callback
	 *            Callback to invoke when the authentication process finishes
	 */
	public void loginWithGoogleAccount(Activity activity, final UserAuthenticationCallback callback) {
		loginWithGoogleAccount(activity, GOOGLE_USER_INFO_SCOPE, callback);
	}
	
	/**
	 * Invokes Windows Azure Mobile Service authentication using a
	 * the Google account registered in the device
	 * 
	 * @param activity
	 *            The activity that triggered the authentication
	 * @param scopes
	 *            The scopes used as authentication token type for login
	 * @param callback
	 *            Callback to invoke when the authentication process finishes
	 */
	public void loginWithGoogleAccount(Activity activity, String scopes, final UserAuthenticationCallback callback) {
		AccountManager acMgr = AccountManager.get(activity.getApplicationContext());
		Account[] accounts = acMgr.getAccountsByType(GOOGLE_ACCOUNT_TYPE);
		
		Account account;
		if (accounts.length == 0) {
			account = null;
		} else {
			account = accounts[0];
		}
		
		loginWithGoogleAccount(activity, account, scopes, callback);
	}
	
	/**
	 * Invokes Windows Azure Mobile Service authentication using a
	 * the Google account registered in the device
	 * 
	 * @param activity
	 *            The activity that triggered the authentication
	 * @param account
	 *            The account used for the login operation
	 * @param callback
	 *            Callback to invoke when the authentication process finishes
	 */
	public void loginWithGoogleAccount(Activity activity, Account account, final UserAuthenticationCallback callback) {
		loginWithGoogleAccount(activity, account, GOOGLE_USER_INFO_SCOPE, callback);
	}
	

	/**
	 * Invokes Windows Azure Mobile Service authentication using a
	 * the Google account registered in the device
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
		try {
			if (account == null) {
				callback.onCompleted(null, new IllegalArgumentException("account"), null);
			}
					
			final MobileServiceClient client = this;
			
			AccountManagerCallback<Bundle> authCallback = new AccountManagerCallback<Bundle>() {
				
				@Override
				public void run(AccountManagerFuture<Bundle> futureBundle) {
					try {
						if (futureBundle.isCancelled()) {
							callback.onCompleted(null, new MobileServiceException("User cancelled"), null);
						} else {
							Bundle bundle = futureBundle.getResult();
							
							String token = (String)(bundle.get(AccountManager.KEY_AUTHTOKEN));
							
							JsonObject json = new JsonObject();
							json.addProperty("access_token", token);
							
							client.login(MobileServiceAuthenticationProvider.Google, json, callback);	
						}
					} catch (Exception e) {
						callback.onCompleted(null, e, null);
					}
				}
			};
			
			AccountManager acMgr = AccountManager.get(activity.getApplicationContext());
			acMgr.getAuthToken(account, scopes, null, activity, authCallback, null);
			
		} catch (Exception e) {
			callback.onCompleted(null, e, null);
		}
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
	 * Creates a MobileServiceTable
	 * 
	 * @param clazz
	 *            The class used for table name and data serialization
	 * 
	 * @return MobileServiceTable with the given name
	 */
	public <E> MobileServiceTable<E> getTable(Class<E> clazz) {
		validateClass(clazz);
		
		return new MobileServiceTable<E>(clazz.getSimpleName(), this, clazz);
	}
	
	/**
	 * Invokes a custom API using POST HTTP method
	 * @param apiName The API name
	 * @param clazz The API result class
	 * @param callback The callback to invoke after the API execution
	 */
	public <E> void invokeApi(
			String apiName, 
			Class<E> clazz,
			ApiOperationCallback<E> callback) {
		invokeApi(apiName, null, HttpPost.METHOD_NAME, null, clazz, callback);
	}
	
	/**
	 * Invokes a custom API using POST HTTP method
	 * @param apiName The API name
	 * @param body The object to send as the request body
	 * @param clazz The API result class
	 * @param callback The callback to invoke after the API execution
	 */
	public <E> void invokeApi(
			String apiName, 
			Object body,
			Class<E> clazz,
			ApiOperationCallback<E> callback) {
		invokeApi(apiName, body, HttpPost.METHOD_NAME, null, clazz, callback);
	}

	/**
	 * Invokes a custom API
	 * @param apiName The API name
	 * @param httpMethod The HTTP Method used to invoke the API
	 * @param parameters The query string parameters sent in the request
	 * @param clazz The API result class
	 * @param callback The callback to invoke after the API execution
	 */
	public <E> void invokeApi(
			String apiName, 
			String httpMethod, 
			List<Pair<String, String>> parameters,
			Class<E> clazz,
			ApiOperationCallback<E> callback) {
		invokeApi(apiName, null, httpMethod, parameters, clazz, callback);
	}

	/**
	 * Invokes a custom API
	 * @param apiName The API name
	 * @param body The object to send as the request body
	 * @param httpMethod The HTTP Method used to invoke the API
	 * @param parameters The query string parameters sent in the request
	 * @param clazz The API result class
	 * @param callback The callback to invoke after the API execution
	 */
	public <E> void invokeApi(
			String apiName, 
			Object body, 
			String httpMethod, 
			List<Pair<String, String>> parameters,
			final Class<E> clazz,
			final ApiOperationCallback<E> callback) {
		if (clazz == null) {
			if (callback != null) {
				callback.onCompleted(null, new IllegalArgumentException("clazz cannot be null"), null);
			}
			return;
		}
				
		JsonElement json = null;
		if (body != null) {
			json = getGsonBuilder().create().toJsonTree(body);
		}
		
		invokeApi(apiName, json, httpMethod, parameters, new ApiJsonOperationCallback() {
			
			@SuppressWarnings("unchecked")
			@Override
			public void onCompleted(JsonElement jsonElement, Exception exception,
					ServiceFilterResponse response) {
				if (callback != null) {
					if (exception == null) {
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
							callback.onCompleted(array, null, response);
						} else {
							callback.onCompleted((E)entities.get(0), exception, response);
						}
					} else {
						callback.onCompleted(null, exception, response);
					}
				}
			}
		});
	}


	/**
	 * Invokes a custom API using POST HTTP method
	 * @param apiName The API name
	 * @param callback The callback to invoke after the API execution
	 */
	public void invokeApi(String apiName, ApiJsonOperationCallback callback) {
		invokeApi(apiName, null, callback);
	}
	
	/**
	 * Invokes a custom API using POST HTTP method
	 * @param apiName The API name
	 * @param body The json element to send as the request body
	 * @param callback The callback to invoke after the API execution
	 */
	public void invokeApi(String apiName, JsonElement body, ApiJsonOperationCallback callback) {
		invokeApi(apiName, body, HttpPost.METHOD_NAME, null, callback);
	}
	
	/**
	 * Invokes a custom API
	 * @param apiName The API name
	 * @param httpMethod The HTTP Method used to invoke the API
	 * @param parameters The query string parameters sent in the request
	 * @param callback The callback to invoke after the API execution
	 */
	public void invokeApi(
			String apiName, 
			String httpMethod, 
			List<Pair<String, String>> parameters, 
			ApiJsonOperationCallback callback) {
		invokeApi(apiName, null, httpMethod, parameters, callback);
	}
	
	/**
	 * Invokes a custom API
	 * @param apiName The API name
	 * @param body The json element to send as the request body
	 * @param httpMethod The HTTP Method used to invoke the API
	 * @param parameters The query string parameters sent in the request
	 * @param callback The callback to invoke after the API execution
	 */
	public void invokeApi(
			String apiName, 
			JsonElement body, 
			String httpMethod, 
			List<Pair<String, String>> parameters,
			final ApiJsonOperationCallback callback) {
		
		byte[] content = null;
		if (body != null) {
			try {
				content = body.toString().getBytes(UTF8_ENCODING);
			} catch (UnsupportedEncodingException e) {
				if (callback != null) {
					callback.onCompleted(null, e, null);
				}
				return;
			}
		}
		
		List<Pair<String, String>> requestHeaders = new ArrayList<Pair<String,String>>();
		if (body != null) {
			requestHeaders.add(new Pair<String, String>(HTTP.CONTENT_TYPE, MobileServiceConnection.JSON_CONTENTTYPE));			
		}
		
		invokeApi(apiName, content, httpMethod, requestHeaders, parameters, new ServiceFilterResponseCallback() {
			
			@Override
			public void onResponse(ServiceFilterResponse response, Exception exception) {
				if (callback != null) {
					if (exception == null) {
						String content = response.getContent();
						JsonElement json = new JsonParser().parse(content);
						
						callback.onCompleted(json, null, response);
					} else {
						callback.onCompleted(null, exception, response);
					}
				}
			}
		});
	}
	
	/**
	 * 
	 * @param apiName The API name
	 * @param content The byte array to send as the request body
	 * @param httpMethod The HTTP Method used to invoke the API
	 * @param requestHeaders The extra headers to send in the request
	 * @param parameters The query string parameters sent in the request
	 * @param callback The callback to invoke after the API execution
	 */
	public void invokeApi(
			String apiName, 
			byte[] content, 
			String httpMethod, 
			List<Pair<String, String>> requestHeaders, 
			List<Pair<String, String>> parameters, 
			final ServiceFilterResponseCallback callback) {
		
		if (apiName == null || apiName.trim().equals("")) {
			if (callback != null) {
				callback.onResponse(null, new IllegalArgumentException("apiName cannot be null"));
			}
			return;
		}
		
		if (httpMethod == null || httpMethod.trim().equals("")) {
			if (callback != null) {
				callback.onResponse(null, new IllegalArgumentException("httpMethod cannot be null"));
			}
			return;
		}
		
		Uri.Builder uriBuilder = Uri.parse(getAppUrl().toString()).buildUpon();
		uriBuilder.path(CUSTOM_API_URL + apiName);
		
		if (parameters != null && parameters.size() > 0) {
			for (Pair<String, String> parameter : parameters) {
				uriBuilder.appendQueryParameter(parameter.first, parameter.second);
			}
		}
		
		ServiceFilterRequest request;
		String url = uriBuilder.build().toString();
		
		if (httpMethod.equalsIgnoreCase(HttpGet.METHOD_NAME)) {
			request = new ServiceFilterRequestImpl(new HttpGet(url), getAndroidHttpClientFactory());
		} else if (httpMethod.equalsIgnoreCase(HttpPost.METHOD_NAME)) {
			request = new ServiceFilterRequestImpl(new HttpPost(url), getAndroidHttpClientFactory());
		} else if (httpMethod.equalsIgnoreCase(HttpPut.METHOD_NAME)) {
			request = new ServiceFilterRequestImpl(new HttpPut(url), getAndroidHttpClientFactory());
		} else if (httpMethod.equalsIgnoreCase(HttpPatch.METHOD_NAME)) {
			request = new ServiceFilterRequestImpl(new HttpPatch(url), getAndroidHttpClientFactory());
		} else if (httpMethod.equalsIgnoreCase(HttpDelete.METHOD_NAME)) {
			request = new ServiceFilterRequestImpl(new HttpDelete(url), getAndroidHttpClientFactory());
		} else {
			if (callback != null) {
				callback.onResponse(null, new IllegalArgumentException("httpMethod not supported"));
			}
			return;
		}
		
		if (requestHeaders != null && requestHeaders.size() > 0) {
			for (Pair<String, String> header: requestHeaders) {
				request.addHeader(header.first, header.second);
			}
		}
		
		if (content != null) {
			try {
				request.setContent(content);
			} catch (Exception e) {
				if (callback != null) {
					callback.onResponse(null, e);
				}
				return;
			}
		}
		
		MobileServiceConnection conn = createConnection();
		
		// Create AsyncTask to execute the request and parse the results
		new RequestAsyncTask(request, conn) {
			@Override
			protected void onPostExecute(ServiceFilterResponse response) {
				if (callback != null) {
					callback.onResponse(response, mTaskException);
				}
			}
		}.execute();	
	}

	/**
	 * Validates the class has an id property defined
	 * @param clazz
	 */
	private <E> void validateClass(Class<E> clazz) {
		if(clazz.isInterface() || Modifier.isAbstract(clazz.getModifiers()))
		{
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

			// Composed service filter
			newClient.mServiceFilter = new ServiceFilter() {
				// Create a filter that after executing the new ServiceFilter
				// executes the existing filter
				ServiceFilter externalServiceFilter = newServiceFilter;
				ServiceFilter internalServiceFilter = oldServiceFilter;

				@Override
				public void handleRequest(
						ServiceFilterRequest request,
						final NextServiceFilterCallback nextServiceFilterCallback,
						ServiceFilterResponseCallback responseCallback) {

					// Executes new ServiceFilter
					externalServiceFilter.handleRequest(request,
							new NextServiceFilterCallback() {

								@Override
								public void onNext(
										ServiceFilterRequest request,
										ServiceFilterResponseCallback responseCallback) {
									// Execute existing ServiceFilter
									internalServiceFilter.handleRequest(
											request, nextServiceFilterCallback,
											responseCallback);
								}
							}, responseCallback);

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
				public void handleRequest(ServiceFilterRequest request,
						NextServiceFilterCallback nextServiceFilterCallback,
						ServiceFilterResponseCallback responseCallback) {
					nextServiceFilterCallback.onNext(request, responseCallback);
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
	MobileServiceConnection createConnection() {
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
	private void initialize(URL appUrl, String appKey,
			MobileServiceUser currentUser, GsonBuilder gsonBuiler,
			Context context, AndroidHttpClientFactory androidHttpClientFactory) {
		if (appUrl == null || appUrl.toString().trim().length() == 0) {
			throw new IllegalArgumentException("Invalid Application URL");
		}

		if (appKey == null || appKey.toString().trim().length() == 0) {
			throw new IllegalArgumentException("Invalid Application Key");
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
	public <T> void registerDeserializer(Type type,
			JsonDeserializer<T> deserializer) {
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
}
