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

import java.lang.reflect.Field;
import java.lang.reflect.Modifier;
import java.lang.reflect.Type;
import java.net.MalformedURLException;
import java.net.URL;
import java.util.Date;

import android.content.Context;

import com.google.gson.GsonBuilder;
import com.google.gson.JsonDeserializer;
import com.google.gson.JsonObject;
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
	 * UTF-8 encoding
	 */
	public static final String UTF8_ENCODING = "UTF-8";

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
				client.getContext());
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

		initialize(appUrl, appKey, null, gsonBuilder, context);
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
			Context context) {
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
}
