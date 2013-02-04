/*
 * MobileServiceClient.java
 */
package com.microsoft.windowsazure.mobileservices;

import java.lang.reflect.Type;
import java.net.MalformedURLException;
import java.net.URL;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;
import java.util.TimeZone;

import com.google.gson.GsonBuilder;
import com.google.gson.JsonDeserializationContext;
import com.google.gson.JsonDeserializer;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParseException;
import com.google.gson.JsonPrimitive;
import com.google.gson.JsonSerializationContext;
import com.google.gson.JsonSerializer;

import android.annotation.SuppressLint;
import android.content.Context;

/**
 * Entry-point for Windows Azure Mobile Services interactions
 */
public class MobileServiceClient {
	/**
	 * LoginManager used for login methods
	 */
	private LoginManager mloginManager;

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

		// Register custom date deserializer
		gsonBuilder.registerTypeAdapter(Date.class,
				new JsonDeserializer<Date>() {
					@SuppressLint("SimpleDateFormat")
					@Override
					public Date deserialize(JsonElement element, Type type,
							JsonDeserializationContext ctx)
							throws JsonParseException {
						String strVal = element.getAsString();

						// Change Z to +00:00 to adapt the string to a format
						// that can
						// be parsed in Java
						String s = strVal.replace("Z", "+00:00");
						try {
							// Remove the ":" character to adapt the string to a
							// format
							// that can be parsed in Java
							s = s.substring(0, 26) + s.substring(27);
						} catch (IndexOutOfBoundsException e) {
							throw new JsonParseException("Invalid length");
						}

						try {
							// Parse the well-formatted date string
							SimpleDateFormat dateFormat = new SimpleDateFormat(
									"yyyy-MM-dd'T'HH:mm:ss'.'SSSZ");
							dateFormat.setTimeZone(TimeZone.getDefault());
							Date date = dateFormat.parse(s);

							return date;
						} catch (ParseException e) {
							throw new JsonParseException(e);
						}
					}
				});

		// Register custom date serializer
		gsonBuilder.registerTypeAdapter(Date.class, new JsonSerializer<Date>() {
			@Override
			public JsonElement serialize(Date date, Type type,
					JsonSerializationContext ctx) {
				SimpleDateFormat dateFormat = new SimpleDateFormat(
						"yyyy-MM-dd'T'HH:mm:ss'.'SSS'Z'", Locale.getDefault());
				dateFormat.setTimeZone(TimeZone.getTimeZone("UTC"));

				String formatted = dateFormat.format(date);

				JsonElement element = new JsonPrimitive(formatted);
				return element;
			}
		});

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
				client.getCurrentUser(), client.getGsonBuilder(), client.getContext());
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
	public void login(MobileServiceAuthenticationProvider provider, UserAuthenticationCallback callback) {
		mLoginInProgress = true;

		final UserAuthenticationCallback externalCallback = callback;

		mloginManager.authenticate(provider, mContext,
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
		mLoginInProgress = true;

		final UserAuthenticationCallback externalCallback = callback;

		mloginManager.authenticate(provider, oAuthToken,
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
	 * Creates a MobileServiceTable
	 * 
	 * @param name
	 *            Table name
	 * @return MobileServiceTable with the given name
	 */
	public MobileServiceTable getTable(String name) {
		if (name == null || name.toString().trim().length() == 0) {
			throw new IllegalArgumentException("Invalid Table Name");
		}

		return new MobileServiceTable(name, this);
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
	private void initialize(URL appUrl, String appKey,
			MobileServiceUser currentUser, GsonBuilder gsonBuiler, Context context) {
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
		mloginManager = new LoginManager(this);
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
	 * Sets the GsonBuilder used to in JSON Serialization/Deserialization
	 * 
	 * @param mGsonBuilder
	 *            The GsonBuilder to set
	 */
	public void setGsonBuilder(GsonBuilder gsonBuilder) {
		mGsonBuilder = gsonBuilder;
	}

	public Context getContext() {
		return mContext;
	}

	public void setContext(Context mContext) {
		this.mContext = mContext;
	}
}
