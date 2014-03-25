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
 * MobileServicePush.java
 */

package com.microsoft.windowsazure.mobileservices;

import java.io.UnsupportedEncodingException;
import java.net.URI;
import java.net.URISyntaxException;
import java.util.ArrayList;
import java.util.List;
import java.util.Set;
import java.util.concurrent.CopyOnWriteArrayList;

import org.apache.http.Header;
import org.apache.http.protocol.HTTP;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonParser;
import android.content.Context;
import android.content.SharedPreferences;
import android.content.SharedPreferences.Editor;
import android.preference.PreferenceManager;
import android.util.Pair;

/**
 * The notification hub client
 */
public class MobileServicePush {

	/**
	 * Prefix for Storage keys
	 */
	private static final String STORAGE_PREFIX = "__NH_";

	/**
	 * Prefix for registration information keys in local storage
	 */
	private static final String REGISTRATION_NAME_STORAGE_KEY = "REG_NAME_";

	/**
	 * Storage Version key
	 */
	private static final String STORAGE_VERSION_KEY = "STORAGE_VERSION";

	/**
	 * Storage Version
	 */
	private static final String STORAGE_VERSION = "1.0.0";

	/**
	 * PNS Handle Key
	 */
	private static final String PNS_HANDLE_KEY = "PNS_HANDLE";

	/**
	 * New registration location header name
	 */
	private static final String NEW_REGISTRATION_LOCATION_HEADER = "Location";

	/**
	 * The MobileServiceClient associated with this instance
	 */
	private MobileServiceClient mClient;

	/**
	 * SharedPreferences reference used to access local storage
	 */
	private SharedPreferences mSharedPreferences;

	/**
	 * Factory which creates Registrations according the PNS supported on device
	 */
	private PnsSpecificRegistrationFactory mPnsSpecificRegistrationFactory;

	/**
	 * Flag to signal the need of refreshing local storage
	 */
	private boolean mIsRefreshNeeded = false;

	/**
	 * Creates a new NotificationHub client
	 * 
	 * @param notificationHubPath
	 *            Notification Hub path
	 * @param connectionString
	 *            Notification Hub connection string
	 * @param context
	 *            Android context used to access SharedPreferences
	 */
	public MobileServicePush(MobileServiceClient client, Context context) {
		mPnsSpecificRegistrationFactory = new PnsSpecificRegistrationFactory();

		mClient = client;

		if (context == null) {
			throw new IllegalArgumentException("context");
		}

		mSharedPreferences = PreferenceManager.getDefaultSharedPreferences(context.getApplicationContext());

		verifyStorageVersion();
	}

	/**
	 * Registers the client for native notifications with the specified tags
	 * 
	 * @param pnsHandle
	 *            PNS specific identifier
	 * @param callback
	 *            The callback to invoke after the Push execution
	 * @param tags
	 *            Tags to use in the registration
	 * @return The created registration
	 * @throws Exception
	 */
	public void register(String pnsHandle, String[] tags, final RegistrationCallback callback) {
		if (isNullOrWhiteSpace(pnsHandle)) {
			callback.onRegister(null, new IllegalArgumentException("pnsHandle"));
			return;
		}

		Registration registration = mPnsSpecificRegistrationFactory.createNativeRegistration();
		registration.setPNSHandle(pnsHandle);
		registration.addTags(tags);

		registerInternal(registration, new RegisterInternalCallback() {

			@Override
			public void onRegister(Registration registration, Exception exception) {
				callback.onRegister(registration, exception);

				return;

			}

		});
	}

	/**
	 * Registers the client for template notifications with the specified tags
	 * 
	 * @param pnsHandle
	 *            PNS specific identifier
	 * @param templateName
	 *            The template name
	 * @param template
	 *            The template body
	 * @param tags
	 *            The tags to use in the registration
	 * @param callback
	 *            The operation callback
	 */
	public void registerTemplate(String pnsHandle, String templateName, String template, String[] tags, final TemplateRegistrationCallback callback) {
		if (isNullOrWhiteSpace(pnsHandle)) {
			callback.onRegister(null, new IllegalArgumentException("pnsHandle"));
			return;
		}

		if (isNullOrWhiteSpace(templateName)) {
			callback.onRegister(null, new IllegalArgumentException("templateName"));
			return;
		}

		if (isNullOrWhiteSpace(template)) {
			callback.onRegister(null, new IllegalArgumentException("template"));
			return;
		}

		TemplateRegistration registration = mPnsSpecificRegistrationFactory.createTemplateRegistration();
		registration.setPNSHandle(pnsHandle);
		registration.setName(templateName);
		registration.setTemplateBody(template);
		registration.addTags(tags);

		registerInternal(registration, new RegisterInternalCallback() {

			@Override
			public void onRegister(Registration registration, Exception exception) {
				callback.onRegister((TemplateRegistration) registration, exception);

				return;
			}
		});
	}

	/**
	 * Unregisters the client for native notifications
	 * 
	 * @param callback
	 *            The operation callback
	 */
	public void unregister(final UnregisterCallback callback) {
		unregisterInternal(Registration.DEFAULT_REGISTRATION_NAME, new UnregisterInternalCallback() {

			@Override
			public void onUnregister(String registrationId, Exception exception) {
				callback.onUnregister(exception);

				return;
			}

		});
	}

	/**
	 * Unregisters the client for template notifications of a specific template
	 * 
	 * @param templateName
	 *            The template name
	 * @param callback
	 *            The operation callback
	 * 
	 */
	public void unregisterTemplate(String templateName, final UnregisterCallback callback) {
		if (isNullOrWhiteSpace(templateName)) {
			callback.onUnregister(new IllegalArgumentException("templateName"));
			return;
		}

		unregisterInternal(templateName, new UnregisterInternalCallback() {

			@Override
			public void onUnregister(String registrationId, Exception exception) {
				callback.onUnregister(exception);

				return;
			}
		});
	}

	/**
	 * Unregisters the client for all notifications
	 * 
	 * @param pnsHandle
	 *            PNS specific identifier
	 * @param callback
	 *            The operation callback
	 */
	public void unregisterAll(String pnsHandle, final UnregisterCallback callback) {
		getFullRegistrationInformation(pnsHandle, new GetFullRegistrationInformationCallback() {

			@Override
			public void onCompleted(ArrayList<Registration> registrations, Exception exception) {

				if (exception != null) {
					callback.onUnregister(exception);
					return;
				}

				final SyncState state = new SyncState();

				state.size = registrations.size();

				final CopyOnWriteArrayList<String> concurrentArray = new CopyOnWriteArrayList<String>();

				final Object syncObject = new Object();

				if (state.size == 0) {

					removeAllRegistrationsId();
					
					mIsRefreshNeeded = false;
					
					callback.onUnregister(null);
					return;
				}

				for (Registration registration : registrations) {
					deleteRegistrationInternal(registration.getName(), registration.getRegistrationId(), new DeleteRegistrationInternalCallback() {

						@Override
						public void onDelete(String registrationId, Exception exception) {

							concurrentArray.add(registrationId);

							if (exception != null) {
								synchronized (syncObject) {
									if (!state.alreadyReturn) {
										callback.onUnregister(exception);
										state.alreadyReturn = true;
										return;
									}
								}
							}

							if (concurrentArray.size() == state.size && !state.alreadyReturn) {
								removeAllRegistrationsId();
								
								mIsRefreshNeeded = false;
								
								callback.onUnregister(null);

								return;
							}
						}
					});
				}
			}
		});
	}

	private class SyncState {
		public boolean alreadyReturn;
		public int size;
	}

	private void refreshRegistrationInformation(String pnsHandle, final RefreshRegistrationInformationCallback callback) {
		if (isNullOrWhiteSpace(pnsHandle)) {
			callback.onRefresh(new IllegalArgumentException("pnsHandle"));
			return;
		}

		// delete old registration information
		Editor editor = mSharedPreferences.edit();
		Set<String> keys = mSharedPreferences.getAll().keySet();
		for (String key : keys) {
			if (key.startsWith(STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY)) {
				editor.remove(key);
			}
		}

		editor.commit();

		getFullRegistrationInformation(pnsHandle, new GetFullRegistrationInformationCallback() {

			@Override
			public void onCompleted(ArrayList<Registration> registrations, Exception exception) {
				if (exception != null) {
					callback.onRefresh(exception);

					return;
				} else {
					for (Registration registration : registrations) {

						try {
							storeRegistrationId(registration.getName(), registration.getRegistrationId(), registration.getPNSHandle());
						} catch (Exception e) {
							callback.onRefresh(exception);

							return;
						}
					}

					mIsRefreshNeeded = false;

					callback.onRefresh(null);

					return;
				}
			}
		});
	}

	private void getFullRegistrationInformation(String pnsHandle, final GetFullRegistrationInformationCallback callback) {
		if (isNullOrWhiteSpace(pnsHandle)) {
			callback.onCompleted(null, new IllegalArgumentException("pnsHandle"));
			return;
		}

		// get existing registrations
		String resource = "/registrations/";

		List<Pair<String, String>> requestHeaders = new ArrayList<Pair<String, String>>();
		List<Pair<String, String>> parameters = new ArrayList<Pair<String, String>>();
		parameters.add(new Pair<String, String>("platform", mPnsSpecificRegistrationFactory.getPlatform()));
		parameters.add(new Pair<String, String>("deviceId", pnsHandle));
		requestHeaders.add(new Pair<String, String>(HTTP.CONTENT_TYPE, MobileServiceConnection.JSON_CONTENTTYPE));

		mClient.invokeApiInternal(resource, null, "GET", requestHeaders, parameters, MobileServiceClient.PNS_API_URL, new ServiceFilterResponseCallback() {

			@Override
			public void onResponse(ServiceFilterResponse response, Exception exception) {
				if (exception != null) {
					callback.onCompleted(null, exception);

					return;
				} else {

					ArrayList<Registration> registrationsList = new ArrayList<Registration>();

					JsonArray registrations = new JsonParser().parse(response.getContent()).getAsJsonArray();

					for (JsonElement registrationJson : registrations) {
						Registration registration = null;
						if (registrationJson.getAsJsonObject().has("templateName")) {
							registration = mPnsSpecificRegistrationFactory.parseTemplateRegistration(registrationJson.getAsJsonObject());
						} else {
							registration = mPnsSpecificRegistrationFactory.parseNativeRegistration(registrationJson.getAsJsonObject());
						}

						registrationsList.add(registration);
					}

					callback.onCompleted(registrationsList, null);

					return;
				}
			}
		});
	}

	/**
	 * Creates a new registration in the server. If it exists, updates its
	 * information
	 * 
	 * @param registration
	 *            The registration to create
	 * @param callback
	 *            The operation callback
	 */
	private void registerInternal(final Registration registration, final RegisterInternalCallback callback) {

		if (mIsRefreshNeeded) {
			String pNSHandle = mSharedPreferences.getString(STORAGE_PREFIX + PNS_HANDLE_KEY, "");

			if (isNullOrWhiteSpace(pNSHandle)) {
				pNSHandle = registration.getPNSHandle();
			}

			refreshRegistrationInformation(pNSHandle, new RefreshRegistrationInformationCallback() {

				@Override
				public void onRefresh(Exception exception) {
					if (exception != null) {
						callback.onRegister(registration, exception);
					} else {
						createRegistrationId(registration, callback);
					}
				}

			});
		} else {
			createRegistrationId(registration, callback);
		}
	}

	private void createRegistrationId(final Registration registration, final RegisterInternalCallback callback) {
		createRegistrationId(registration, new CreateRegistrationIdCallback() {
			@Override
			public void onCreate(final String registrationId, Exception exception) {
				callback.onRegister(registration, exception);
			}
		});
	}

	private void createRegistrationId(final Registration registration, final CreateRegistrationIdCallback callback) {

		String registrationId = null;

		try {
			registrationId = retrieveRegistrationId(registration.getName());
		} catch (Exception e) {
			callback.onCreate(null, e);
			return;
		}

		if (isNullOrWhiteSpace(registrationId)) {
			createRegistrationId(new CreateRegistrationIdCallback() {
				@Override
				public void onCreate(final String registrationId, Exception exception) {
					if (exception != null) {
						callback.onCreate(null, exception);

						return;
					} else {
						setRegistrationId(registration, registrationId, new SetRegistrationIdCallback() {

							@Override
							public void onSet(String registrationId, Exception exception) {
								callback.onCreate(registrationId, exception);

								return;
							}

						});
					}
				}

			});
		} else {
			setRegistrationId(registration, registrationId, new SetRegistrationIdCallback() {

				@Override
				public void onSet(String registrationId, Exception exception) {
					callback.onCreate(registrationId, exception);

					return;
				}

			});
		}
	}

	private void setRegistrationId(final Registration registration, final String registrationId, final SetRegistrationIdCallback callback) {
		registration.setRegistrationId(registrationId);

		upsertRegistrationInternal(registration, new UpsertRegistrationInternalCallback() {

			@Override
			public void onUpsert(final Registration registration, Exception exception) {

				if (exception == null) {

					try {
						storeRegistrationId(registration.getName(), registration.getRegistrationId(), registration.getPNSHandle());
					} catch (Exception e) {
						callback.onSet(registrationId, e);
						return;
					}

					callback.onSet(registrationId, null);

					return;
				} else if (exception instanceof RegistrationGoneException) {
					// if we get an RegistrationGoneException (410) from
					// service, we
					// will recreate registration id and will try to do
					// upsert one more
					// time.
					// This can occur if the backing NotificationHub is changed
					// or if the registration expires.

					try {
						removeRegistrationId(registration.getName());
					} catch (Exception e) {
						callback.onSet(registrationId, e);
						return;
					}

					createRegistrationId(new CreateRegistrationIdCallback() {

						@Override
						public void onCreate(final String registrationId, Exception exception) {
							if (exception != null) {
								callback.onSet(registrationId, exception);

								return;
							} else {
								registration.setRegistrationId(registrationId);
								upsertRegistrationInternal(registration, new UpsertRegistrationInternalCallback() {

									@Override
									public void onUpsert(Registration registration, Exception exception) {
										if (exception != null) {
											callback.onSet(registrationId, exception);

											return;
										} else {
											try {
												storeRegistrationId(registration.getName(), registration.getRegistrationId(), registration.getPNSHandle());
											} catch (Exception e) {
												callback.onSet(registrationId, e);
												return;
											}

											callback.onSet(registrationId, null);

											return;
										}

									}
								});
							}
						}
					});
				} else {
					callback.onSet(registrationId, exception);

					return;
				}
			}
		});
	}

	/**
	 * Deletes a registration and removes it from local storage
	 * 
	 * @param registrationName
	 *            The registration name
	 * @param callback
	 *            The operation callback
	 */
	private void unregisterInternal(String registrationName, final UnregisterInternalCallback callback) {
		String registrationId = null;
		try {
			registrationId = retrieveRegistrationId(registrationName);
		} catch (Exception e) {
			callback.onUnregister(null, e);
			return;
		}

		if (!isNullOrWhiteSpace(registrationId)) {
			deleteRegistrationInternal(registrationName, registrationId, new DeleteRegistrationInternalCallback() {

				@Override
				public void onDelete(String registrationId, Exception exception) {
					if (exception != null) {
						callback.onUnregister(registrationId, exception);

						return;
					} else {
						callback.onUnregister(registrationId, null);

						return;
					}

				}
			});
		} else {
			callback.onUnregister(null, null);
			return;
		}
	}

	private void createRegistrationId(final CreateRegistrationIdCallback callback) {

		String resource = "/registrationids/";
		mClient.invokeApiInternal(resource, null, "POST", null, null, MobileServiceClient.PNS_API_URL, new ServiceFilterResponseCallback() {

			@Override
			public void onResponse(ServiceFilterResponse response, Exception exception) {
				if (exception != null) {
					callback.onCreate(null, exception);

					return;
				} else {
					for (Header header : response.getHeaders()) {
						if (header.getName().equalsIgnoreCase(NEW_REGISTRATION_LOCATION_HEADER)) {

							URI regIdUri = null;
							try {
								regIdUri = new URI(header.getValue());
							} catch (URISyntaxException e) {
								callback.onCreate(null, e);

								return;
							}

							String[] pathFragments = regIdUri.getPath().split("/");
							String result = pathFragments[pathFragments.length - 1];

							callback.onCreate(result, null);

							return;
						}
					}
				}
			}
		});
	}

	/**
	 * Updates a registration
	 * 
	 * @param registration
	 *            The registration to update
	 * @param callback
	 *            The operation callback
	 */
	private void upsertRegistrationInternal(final Registration registration, final UpsertRegistrationInternalCallback callback) {

		GsonBuilder gsonBuilder = new GsonBuilder();
		gsonBuilder = gsonBuilder.excludeFieldsWithoutExposeAnnotation();

		Gson gson = gsonBuilder.create();

		String resource = registration.getURI();
		JsonElement json = gson.toJsonTree(registration);
		String body = json.toString();
		byte[] content = null;

		try {
			content = body.getBytes(MobileServiceClient.UTF8_ENCODING);
		} catch (UnsupportedEncodingException e) {
			callback.onUpsert(registration, e);
			return;
		}

		List<Pair<String, String>> requestHeaders = new ArrayList<Pair<String, String>>();

		requestHeaders.add(new Pair<String, String>(HTTP.CONTENT_TYPE, MobileServiceConnection.JSON_CONTENTTYPE));

		mClient.invokeApiInternal(resource, content, "PUT", requestHeaders, null, MobileServiceClient.PNS_API_URL, new ServiceFilterResponseCallback() {

			@Override
			public void onResponse(ServiceFilterResponse response, Exception exception) {
				if (exception != null) {
					if (response.getStatus().getStatusCode() == 410) {
						exception = new RegistrationGoneException(exception);
					}

					callback.onUpsert(registration, exception);

					return;
				} else {
					callback.onUpsert(registration, null);

					return;
				}
			}
		});
	}

	/**
	 * Deletes a registration and removes it from local storage
	 * 
	 * @param registrationName
	 *            The registration Name
	 * 
	 * @param registrationId
	 *            The registration Id
	 * 
	 * @param callback
	 *            The operation callback
	 */
	private void deleteRegistrationInternal(final String registrationName, final String registrationId, final DeleteRegistrationInternalCallback callback) {

		String resource = "/registrations/" + registrationId;

		mClient.invokeApiInternal(resource, null, "DELETE", null, null, MobileServiceClient.PNS_API_URL, new ServiceFilterResponseCallback() {

			@Override
			public void onResponse(ServiceFilterResponse response, Exception exception) {
				try {
					removeRegistrationId(registrationName);
				} catch (Exception e) {
					callback.onDelete(registrationId, e);
					return;
				}

				if (exception != null) {
					callback.onDelete(registrationId, exception);
					return;
				}

				callback.onDelete(registrationId, null);
				return;

			}
		});
	}

	/**
	 * Retrieves the registration id associated to the registration name from
	 * local storage
	 * 
	 * @param registrationName
	 *            The registration name to look for in local storage
	 * @return A registration id String
	 * @throws Exception
	 */
	private String retrieveRegistrationId(String registrationName) throws Exception {
		return mSharedPreferences.getString(STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY + registrationName, null);
	}

	/**
	 * Stores the registration name and id association in local storage
	 * 
	 * @param registrationName
	 *            The registration name to store in local storage
	 * @param registrationId
	 *            The registration id to store in local storage
	 * @throws Exception
	 */
	private void storeRegistrationId(String registrationName, String registrationId, String pNSHandle) throws Exception {
		Editor editor = mSharedPreferences.edit();

		editor.putString(STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY + registrationName, registrationId);

		editor.putString(STORAGE_PREFIX + PNS_HANDLE_KEY, pNSHandle);

		// Always overwrite the storage version with the latest value
		editor.putString(STORAGE_PREFIX + STORAGE_VERSION_KEY, STORAGE_VERSION);

		editor.commit();
	}

	/**
	 * Removes the registration name and id association from local storage
	 * 
	 * @param registrationName
	 *            The registration name of the association to remove from local
	 *            storage
	 * @throws Exception
	 */
	private void removeRegistrationId(String registrationName) {
		Editor editor = mSharedPreferences.edit();

		editor.remove(STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY + registrationName);

		editor.commit();
	}

	private void removeAllRegistrationsId() {
		Editor editor = mSharedPreferences.edit();

		Set<String> keys = mSharedPreferences.getAll().keySet();

		for (String key : keys) {
			if (key.startsWith(STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY)) {
				editor.remove(key);
			}
		}

		editor.commit();
	}

	private void verifyStorageVersion() {
		String currentStorageVersion = mSharedPreferences.getString(STORAGE_PREFIX + STORAGE_VERSION_KEY, "");

		Editor editor = mSharedPreferences.edit();

		if (!currentStorageVersion.equals(STORAGE_VERSION)) {
			Set<String> keys = mSharedPreferences.getAll().keySet();

			for (String key : keys) {
				if (key.startsWith(STORAGE_PREFIX)) {
					editor.remove(key);
				}
			}
		}

		editor.commit();

		mIsRefreshNeeded = true;
	}

	private static boolean isNullOrWhiteSpace(String str) {
		return str == null || str.trim().equals("");
	}
}
