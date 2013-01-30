/*
 * MobileServiceClient.java
 */
package com.microsoft.windowsazure.mobileservices;

import java.net.MalformedURLException;
import java.net.URL;

import com.google.gson.JsonObject;

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
	 * UTF-8 encoding
	 */
	public static final String UTF8_ENCODING = "UTF-8";

	/**
	 * Constructor for the MobileServiceClient
	 * 
	 * @param appUrl
	 *            Mobile Service URL
	 * @param appKey
	 *            Mobile Service application key
	 * @throws MalformedURLException
	 */
	public MobileServiceClient(String appUrl, String appKey)
			throws MalformedURLException {
		initialize(new URL(appUrl), appKey, null);
	}

	/**
	 * Constructor for the MobileServiceClient
	 * 
	 * @param client
	 *            An existing MobileServiceClient
	 */
	public MobileServiceClient(MobileServiceClient client) {
		initialize(client.getAppUrl(), client.getAppKey(),
				client.getCurrentUser());
	}

	/**
	 * Constructor for the MobileServiceClient
	 * 
	 * @param appUrl
	 *            Mobile Service URL
	 * @param appKey
	 *            Mobile Service application key
	 */
	public MobileServiceClient(URL appUrl, String appKey) {
		initialize(appUrl, appKey, null);
	}

	/**
	 * Invokes an interactive authentication process using the specified
	 * Authentication Provider
	 * 
	 * @param provider
	 *            The provider used for the authentication process
	 * @param context
	 *            The context used to create the authentication dialog
	 * @param callback
	 *            Callback to invoke when the authentication process finishes
	 */
	public void login(MobileServiceAuthenticationProvider provider,
			Context context, UserAuthenticationCallback callback) {
		mLoginInProgress = true;

		final UserAuthenticationCallback externalCallback = callback;

		mloginManager.authenticate(provider, context,
				new UserAuthenticationCallback() {

					@Override
					public void onSuccess(MobileServiceUser user) {
						mCurrentUser = user;
						mLoginInProgress = false;
						if (externalCallback != null) {
							externalCallback.onSuccess(user);
						}
					}

					@Override
					public void onError(Exception exception,
							ServiceFilterResponse response) {
						mCurrentUser = null;
						mLoginInProgress = false;

						if (externalCallback != null) {
							externalCallback.onError(exception, response);
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
					public void onSuccess(MobileServiceUser user) {
						mCurrentUser = user;
						mLoginInProgress = false;

						if (externalCallback != null) {
							externalCallback.onSuccess(user);
						}
					}

					@Override
					public void onError(Exception exception,
							ServiceFilterResponse response) {
						mCurrentUser = null;
						mLoginInProgress = false;
						if (externalCallback != null) {
							externalCallback.onError(exception, response);
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
	 */
	private void initialize(URL appUrl, String appKey,
			MobileServiceUser currentUser) {
		if (appUrl == null || appUrl.toString().trim().length() == 0) {
			throw new IllegalArgumentException("Invalid Application URL");
		}

		if (appKey == null || appKey.toString().trim().length() == 0) {
			throw new IllegalArgumentException("Invalid Application Key");
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
	}
}
