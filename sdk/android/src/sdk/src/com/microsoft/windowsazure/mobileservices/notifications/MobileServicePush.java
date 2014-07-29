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
 * MobileServicePush.java
 */
package com.microsoft.windowsazure.mobileservices.notifications;

import java.io.UnsupportedEncodingException;
import java.net.URI;
import java.net.URISyntaxException;
import java.util.ArrayList;
import java.util.List;
import java.util.Set;
import java.util.concurrent.CopyOnWriteArrayList;
import java.util.concurrent.ExecutionException;

import org.apache.http.Header;
import org.apache.http.protocol.HTTP;

import com.google.common.util.concurrent.FutureCallback;
import com.google.common.util.concurrent.Futures;
import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.SettableFuture;
import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonParser;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.http.MobileServiceConnection;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;

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
	 * @param tags
	 *            Tags to use in the registration
	 * 
	 * @return Future with Registration Information
	 */
	public ListenableFuture<Registration> register(String pnsHandle, String[] tags) {

		final SettableFuture<Registration> resultFuture = SettableFuture.create();

		if (isNullOrWhiteSpace(pnsHandle)) {
			resultFuture.setException(new IllegalArgumentException("pnsHandle"));
			return resultFuture;
		}

		final Registration registration = mPnsSpecificRegistrationFactory.createNativeRegistration();
		registration.setPNSHandle(pnsHandle);
		registration.addTags(tags);

		ListenableFuture<String> registerInternalFuture = registerInternal(registration);

		Futures.addCallback(registerInternalFuture, new FutureCallback<String>() {
			@Override
			public void onFailure(Throwable exception) {
				resultFuture.setException(exception);
			}

			@Override
			public void onSuccess(String v) {
				resultFuture.set(registration);
			}
		});

		return resultFuture;
	}

	/**
	 * Registers the client for native notifications with the specified tags
	 * 
	 * @deprecated use {@link register(String pnsHandle, String[] tags)} instead
	 * 
	 * @param pnsHandle
	 *            PNS specific identifier
	 * @param callback
	 *            The callback to invoke after the Push execution
	 * @param tags
	 *            Tags to use in the registration
	 */
	public void register(String pnsHandle, String[] tags, final RegistrationCallback callback) {
		ListenableFuture<Registration> registerFuture = register(pnsHandle, tags);

		Futures.addCallback(registerFuture, new FutureCallback<Registration>() {
			@Override
			public void onFailure(Throwable exception) {
				if (exception instanceof Exception) {
					callback.onRegister(null, (Exception) exception);
				}
			}

			@Override
			public void onSuccess(Registration registration) {
				callback.onRegister(registration, null);
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
	 * 
	 * @return Future with TemplateRegistration Information
	 */
	public ListenableFuture<TemplateRegistration> registerTemplate(String pnsHandle, String templateName, String template, String[] tags) {

		final SettableFuture<TemplateRegistration> resultFuture = SettableFuture.create();

		if (isNullOrWhiteSpace(pnsHandle)) {
			resultFuture.setException(new IllegalArgumentException("pnsHandle"));
			return resultFuture;
		}

		if (isNullOrWhiteSpace(templateName)) {
			resultFuture.setException(new IllegalArgumentException("templateName"));
			return resultFuture;
		}

		if (isNullOrWhiteSpace(template)) {
			resultFuture.setException(new IllegalArgumentException("template"));
			return resultFuture;
		}

		final TemplateRegistration registration = mPnsSpecificRegistrationFactory.createTemplateRegistration();
		registration.setPNSHandle(pnsHandle);
		registration.setName(templateName);
		registration.setTemplateBody(template);
		registration.addTags(tags);

		ListenableFuture<String> registerInternalFuture = registerInternal(registration);

		Futures.addCallback(registerInternalFuture, new FutureCallback<String>() {
			@Override
			public void onFailure(Throwable exception) {
				resultFuture.setException(exception);
			}

			@Override
			public void onSuccess(String v) {
				resultFuture.set(registration);
			}
		});

		return resultFuture;
	}

	/**
	 * Registers the client for template notifications with the specified tags
	 * 
	 * @deprecated use {@link registerTemplate(String pnsHandle, String
	 *             templateName, String template, String[] tags)} instead
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
		ListenableFuture<TemplateRegistration> registerFuture = registerTemplate(pnsHandle, templateName, template, tags);

		Futures.addCallback(registerFuture, new FutureCallback<TemplateRegistration>() {
			@Override
			public void onFailure(Throwable exception) {
				if (exception instanceof Exception) {
					callback.onRegister(null, (Exception) exception);
				}
			}

			@Override
			public void onSuccess(TemplateRegistration registration) {
				callback.onRegister(registration, null);
			}
		});
	}

	/**
	 * Unregisters the client for native notifications
	 * 
	 * @return Future with TemplateRegistration Information
	 */
	public ListenableFuture<Void> unregister() {
		return unregisterInternal(Registration.DEFAULT_REGISTRATION_NAME);
	}

	/**
	 * Unregisters the client for native notifications
	 * 
	 * @deprecated use {@link unregister()} instead
	 * 
	 * @param callback
	 *            The operation callback
	 */
	public void unregister(final UnregisterCallback callback) {
		ListenableFuture<Void> unregisterFuture = unregister();

		Futures.addCallback(unregisterFuture, new FutureCallback<Void>() {
			@Override
			public void onFailure(Throwable exception) {
				if (exception instanceof Exception) {
					callback.onUnregister((Exception) exception);
				}
			}

			@Override
			public void onSuccess(Void v) {
				callback.onUnregister(null);
			}
		});
	}

	/**
	 * Unregisters the client for template notifications of a specific template
	 * 
	 * @param templateName
	 *            The template name
	 * 
	 * @return Future with TemplateRegistration Information
	 * 
	 */
	public ListenableFuture<Void> unregisterTemplate(String templateName) {
		if (isNullOrWhiteSpace(templateName)) {
			throw new IllegalArgumentException("templateName");
		}

		return unregisterInternal(templateName);
	}

	/**
	 * Unregisters the client for template notifications of a specific template
	 * 
	 * @deprecated use {@link unregisterTemplate(String templateName)} instead
	 * 
	 * @param templateName
	 *            The template name
	 * @param callback
	 *            The operation callback
	 * 
	 */
	public void unregisterTemplate(String templateName, final UnregisterCallback callback) {
		ListenableFuture<Void> unregisterTemplateFuture = unregisterTemplate(templateName);

		Futures.addCallback(unregisterTemplateFuture, new FutureCallback<Void>() {
			@Override
			public void onFailure(Throwable exception) {
				if (exception instanceof Exception) {
					callback.onUnregister((Exception) exception);
				}
			}

			@Override
			public void onSuccess(Void v) {
				callback.onUnregister(null);
			}
		});
	}

	/**
	 * Unregisters the client for all notifications
	 * 
	 * @param pnsHandle
	 *            PNS specific identifier
	 * 
	 * @return Future with TemplateRegistration Information
	 */
	public ListenableFuture<Void> unregisterAll(String pnsHandle) {

		final SettableFuture<Void> resultFuture = SettableFuture.create();

		ListenableFuture<ArrayList<Registration>> fullRegistrationInformationFuture = getFullRegistrationInformation(pnsHandle);

		Futures.addCallback(fullRegistrationInformationFuture, new FutureCallback<ArrayList<Registration>>() {
			@Override
			public void onFailure(Throwable exception) {
				resultFuture.setException(exception);
			}

			@Override
			public void onSuccess(ArrayList<Registration> registrations) {

				ListenableFuture<Void> unregisterAllInternalFuture = unregisterAllInternal(registrations);

				Futures.addCallback(unregisterAllInternalFuture, new FutureCallback<Void>() {
					@Override
					public void onFailure(Throwable exception) {
						resultFuture.setException(exception);
					}

					@Override
					public void onSuccess(Void v) {
						resultFuture.set(null);
					}
				});
			}
		});

		return resultFuture;
	}

	/**
	 * Unregisters the client for all notifications
	 * 
	 * @deprecated use {@link unregisterAll(String pnsHandle)} instead
	 * 
	 * @param pnsHandle
	 *            PNS specific identifier
	 * @param callback
	 *            The operation callback
	 */
	public void unregisterAll(String pnsHandle, final UnregisterCallback callback) {
		ListenableFuture<Void> unregisterAllFuture = unregisterAll(pnsHandle);

		Futures.addCallback(unregisterAllFuture, new FutureCallback<Void>() {
			@Override
			public void onFailure(Throwable exception) {
				if (exception instanceof Exception) {
					callback.onUnregister((Exception) exception);
				}
			}

			@Override
			public void onSuccess(Void v) {
				callback.onUnregister(null);
			}
		});
	}

	private ListenableFuture<Void> unregisterAllInternal(ArrayList<Registration> registrations) {
		final SettableFuture<Void> resultFuture = SettableFuture.create();

		final SyncState state = new SyncState();

		state.size = registrations.size();

		final CopyOnWriteArrayList<String> concurrentArray = new CopyOnWriteArrayList<String>();

		final Object syncObject = new Object();

		if (state.size == 0) {

			removeAllRegistrationsId();

			mIsRefreshNeeded = false;

			resultFuture.set(null);

			return resultFuture;
		}

		for (final Registration registration : registrations) {

			ListenableFuture<Void> serviceFilterFuture = deleteRegistrationInternal(registration.getName(), registration.getRegistrationId());

			Futures.addCallback(serviceFilterFuture, new FutureCallback<Void>() {
				@Override
				public void onFailure(Throwable exception) {

					if (exception != null) {
						synchronized (syncObject) {
							if (!state.alreadyReturn) {
								state.alreadyReturn = true;

								resultFuture.setException(exception);

								return;
							}
						}
					}
				}

				@Override
				public void onSuccess(Void v) {
					concurrentArray.add(registration.getRegistrationId());

					if (concurrentArray.size() == state.size && !state.alreadyReturn) {
						removeAllRegistrationsId();

						mIsRefreshNeeded = false;

						resultFuture.set(null);

						return;
					}
				}
			});
		}

		return resultFuture;
	}

	private class SyncState {
		public boolean alreadyReturn;
		public int size;
	}

	private ListenableFuture<Void> refreshRegistrationInformation(String pnsHandle) {

		final SettableFuture<Void> resultFuture = SettableFuture.create();

		if (isNullOrWhiteSpace(pnsHandle)) {
			resultFuture.setException(new IllegalArgumentException("pnsHandle"));
			return resultFuture;
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

		ListenableFuture<ArrayList<Registration>> getFullRegistrationInformationFuture = getFullRegistrationInformation(pnsHandle);

		Futures.addCallback(getFullRegistrationInformationFuture, new FutureCallback<ArrayList<Registration>>() {
			@Override
			public void onFailure(Throwable exception) {
				resultFuture.setException(exception);
			}

			@Override
			public void onSuccess(ArrayList<Registration> registrations) {
				for (Registration registration : registrations) {

					try {
						storeRegistrationId(registration.getName(), registration.getRegistrationId(), registration.getPNSHandle());
					} catch (Exception e) {
						resultFuture.setException(e);

						return;
					}
				}

				mIsRefreshNeeded = false;

				resultFuture.set(null);
			}
		});

		return resultFuture;
	}

	private ListenableFuture<ArrayList<Registration>> getFullRegistrationInformation(String pnsHandle) {
		final SettableFuture<ArrayList<Registration>> resultFuture = SettableFuture.create();

		if (isNullOrWhiteSpace(pnsHandle)) {
			resultFuture.setException(new IllegalArgumentException("pnsHandle"));
			return resultFuture;
		}

		// get existing registrations
		String resource = "/registrations/";

		List<Pair<String, String>> requestHeaders = new ArrayList<Pair<String, String>>();
		List<Pair<String, String>> parameters = new ArrayList<Pair<String, String>>();
		parameters.add(new Pair<String, String>("platform", mPnsSpecificRegistrationFactory.getPlatform()));
		parameters.add(new Pair<String, String>("deviceId", pnsHandle));
		requestHeaders.add(new Pair<String, String>(HTTP.CONTENT_TYPE, MobileServiceConnection.JSON_CONTENTTYPE));

		ListenableFuture<ServiceFilterResponse> serviceFilterFuture = mClient.invokeApiInternal(resource, null, "GET", requestHeaders, parameters,
				MobileServiceClient.PNS_API_URL);

		Futures.addCallback(serviceFilterFuture, new FutureCallback<ServiceFilterResponse>() {
			@Override
			public void onFailure(Throwable exception) {
				resultFuture.setException(exception);
			}

			@Override
			public void onSuccess(ServiceFilterResponse response) {
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

				resultFuture.set(registrationsList);
			}
		});

		return resultFuture;
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
	private ListenableFuture<String> registerInternal(final Registration registration) {

		final SettableFuture<String> resultFuture = SettableFuture.create();

		if (mIsRefreshNeeded) {
			String pNSHandle = mSharedPreferences.getString(STORAGE_PREFIX + PNS_HANDLE_KEY, "");

			if (isNullOrWhiteSpace(pNSHandle)) {
				pNSHandle = registration.getPNSHandle();
			}

			ListenableFuture<Void> refreshRegistrationInformationFuture = refreshRegistrationInformation(pNSHandle);

			Futures.addCallback(refreshRegistrationInformationFuture, new FutureCallback<Void>() {

				@Override
				public void onFailure(Throwable exception) {
					resultFuture.setException(exception);
				}

				@Override
				public void onSuccess(Void v) {
					resultFuture.set(registration.getRegistrationId());
				}
			});

			return resultFuture;

		} else {
			return createRegistrationId(registration);
		}
	}

	private ListenableFuture<String> createRegistrationId(final Registration registration) {

		String registrationId = null;

		final SettableFuture<String> resultFuture = SettableFuture.create();

		try {
			registrationId = retrieveRegistrationId(registration.getName());
		} catch (Exception e) {
			resultFuture.setException(e);
			return resultFuture;
		}

		if (isNullOrWhiteSpace(registrationId)) {
			return createRegistrationIdInternal(registration);
		} else {
			ListenableFuture<Void> unregisterInternalFuture = unregisterInternal(registration.getName());

			Futures.addCallback(unregisterInternalFuture, new FutureCallback<Void>() {

				@Override
				public void onFailure(Throwable exception) {
					resultFuture.setException(exception);
				}

				@Override
				public void onSuccess(Void v) {

					ListenableFuture<String> createRegistrationIdInternalFuture = createRegistrationIdInternal(registration);

					Futures.addCallback(createRegistrationIdInternalFuture, new FutureCallback<String>() {

						@Override
						public void onFailure(Throwable exception) {
							resultFuture.setException(exception);
						}

						@Override
						public void onSuccess(String registrationId) {
							resultFuture.set(registrationId);
						}
					});
				}
			});
		}

		return resultFuture;
	}

	private ListenableFuture<String> createRegistrationIdInternal(final Registration registration) {

		final SettableFuture<String> resultFuture = SettableFuture.create();

		ListenableFuture<String> createRegistrationIdFuture = createRegistrationId();

		Futures.addCallback(createRegistrationIdFuture, new FutureCallback<String>() {

			@Override
			public void onFailure(Throwable exception) {
				resultFuture.setException(exception);
			}

			@Override
			public void onSuccess(String registrationId) {
				ListenableFuture<String> setRegistrationIdFuture = setRegistrationId(registration, registrationId);

				Futures.addCallback(setRegistrationIdFuture, new FutureCallback<String>() {

					@Override
					public void onFailure(Throwable exception) {
						resultFuture.setException(exception);
					}

					@Override
					public void onSuccess(String registrationId) {
						resultFuture.set(registrationId);
					}
				});
			}
		});

		return resultFuture;
	}

	private ListenableFuture<String> setRegistrationId(final Registration registration, final String registrationId) {
		registration.setRegistrationId(registrationId);

		final SettableFuture<String> resultFuture = SettableFuture.create();

		ListenableFuture<Void> upsertRegistrationInternalFuture = upsertRegistrationInternal(registration);

		Futures.addCallback(upsertRegistrationInternalFuture, new FutureCallback<Void>() {
			@Override
			public void onFailure(Throwable exception) {

				if (!(exception instanceof MobileServiceException)) {
					resultFuture.setException(exception);
				}

				MobileServiceException mobileServiceException = (MobileServiceException) exception;

				ServiceFilterResponse response = mobileServiceException.getResponse();

				if (response != null && response.getStatus().getStatusCode() == 410) {

					// if we get an RegistrationGoneException (410) from
					// service, we will recreate registration id and will try to
					// do upsert one more time.
					// This can occur if the backing NotificationHub is changed
					// or if the registration expires.

					try {
						removeRegistrationId(registration.getName());
					} catch (Exception e) {
						resultFuture.setException(e);
						return;
					}

					ListenableFuture<String> createRegistrationIdFuture = createRegistrationId();

					Futures.addCallback(createRegistrationIdFuture, new FutureCallback<String>() {
						@Override
						public void onFailure(Throwable exception) {
							resultFuture.setException(exception);
						}

						@Override
						public void onSuccess(final String registrationId) {
							ListenableFuture<Void> upsertRegistrationInternalFuture2 = upsertRegistrationInternal(registration);

							Futures.addCallback(upsertRegistrationInternalFuture2, new FutureCallback<Void>() {
								@Override
								public void onFailure(Throwable exception) {

									if (!(exception instanceof MobileServiceException)) {
										resultFuture.setException(exception);
									}

									MobileServiceException mobileServiceException = (MobileServiceException) exception;

									ServiceFilterResponse response = mobileServiceException.getResponse();

									if (response != null && response.getStatus().getStatusCode() == 410) {

										RegistrationGoneException registrationGoneException = new RegistrationGoneException(mobileServiceException);
										resultFuture.setException(registrationGoneException);
									}

								}

								public void onSuccess(Void v) {
									try {
										storeRegistrationId(registration.getName(), registration.getRegistrationId(), registration.getPNSHandle());
									} catch (Exception exception) {
										resultFuture.setException(exception);
										return;
									}
									resultFuture.set(registrationId);
								}
							});
						}
					});
				} else {
					resultFuture.setException(exception);
				}
			}

			@Override
			public void onSuccess(Void v) {
				try {
					storeRegistrationId(registration.getName(), registration.getRegistrationId(), registration.getPNSHandle());
				} catch (Exception exception) {
					resultFuture.setException(exception);
					return;
				}

				resultFuture.set(registrationId);
			}
		});

		return resultFuture;
	}

	/**
	 * Deletes a registration and removes it from local storage
	 * 
	 * @param registrationName
	 *            The registration name
	 * @param callback
	 *            The operation callback
	 * @throws Exception
	 */
	private ListenableFuture<Void> unregisterInternal(String registrationName) {
		String registrationId = null;

		registrationId = retrieveRegistrationId(registrationName);

		return deleteRegistrationInternal(registrationName, registrationId);
	}

	private ListenableFuture<String> createRegistrationId() {

		String resource = "/registrationids/";

		final SettableFuture<String> resultFuture = SettableFuture.create();
		ListenableFuture<ServiceFilterResponse> serviceFilterFuture = mClient.invokeApiInternal(resource, null, "POST", null, null,
				MobileServiceClient.PNS_API_URL);

		Futures.addCallback(serviceFilterFuture, new FutureCallback<ServiceFilterResponse>() {
			@Override
			public void onFailure(Throwable exception) {
				resultFuture.setException(exception);
			}

			@Override
			public void onSuccess(ServiceFilterResponse response) {
				for (Header header : response.getHeaders()) {
					if (header.getName().equalsIgnoreCase(NEW_REGISTRATION_LOCATION_HEADER)) {

						URI regIdUri = null;
						try {
							regIdUri = new URI(header.getValue());
						} catch (URISyntaxException e) {
							resultFuture.setException(e);

							return;
						}

						String[] pathFragments = regIdUri.getPath().split("/");
						String result = pathFragments[pathFragments.length - 1];

						resultFuture.set(result);
					}
				}
			}
		});

		return resultFuture;
	}

	/**
	 * Updates a registration
	 * 
	 * @param registration
	 *            The registration to update
	 * @param callback
	 *            The operation callback
	 * @throws UnsupportedEncodingException
	 * @throws Exception
	 */
	private ListenableFuture<Void> upsertRegistrationInternal(final Registration registration) {

		final SettableFuture<Void> resultFuture = SettableFuture.create();

		GsonBuilder gsonBuilder = new GsonBuilder();
		gsonBuilder = gsonBuilder.excludeFieldsWithoutExposeAnnotation();

		Gson gson = gsonBuilder.create();

		String resource = registration.getURI();
		JsonElement json = gson.toJsonTree(registration);
		String body = json.toString();

		byte[] content;
		try {
			content = body.getBytes(MobileServiceClient.UTF8_ENCODING);
		} catch (UnsupportedEncodingException e) {
			resultFuture.setException(e);
			return resultFuture;
		}

		List<Pair<String, String>> requestHeaders = new ArrayList<Pair<String, String>>();

		requestHeaders.add(new Pair<String, String>(HTTP.CONTENT_TYPE, MobileServiceConnection.JSON_CONTENTTYPE));

		ListenableFuture<ServiceFilterResponse> serviceFilterFuture = mClient.invokeApiInternal(resource, content, "PUT", requestHeaders, null,
				MobileServiceClient.PNS_API_URL);

		Futures.addCallback(serviceFilterFuture, new FutureCallback<ServiceFilterResponse>() {
			@Override
			public void onFailure(Throwable exception) {
				resultFuture.setException(exception);
			}

			@Override
			public void onSuccess(ServiceFilterResponse response) {
				resultFuture.set(null);
			}
		});

		return resultFuture;
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
	 * @throws ExecutionException
	 * @throws InterruptedException
	 */
	private ListenableFuture<Void> deleteRegistrationInternal(final String registrationName, final String registrationId) {

		final SettableFuture<Void> resultFuture = SettableFuture.create();

		if (isNullOrWhiteSpace(registrationId)) {
			resultFuture.set(null);
			return resultFuture;
		}

		String resource = "/registrations/" + registrationId;

		ListenableFuture<ServiceFilterResponse> serviceFilterFuture = mClient.invokeApiInternal(resource, null, "DELETE", null, null,
				MobileServiceClient.PNS_API_URL);

		Futures.addCallback(serviceFilterFuture, new FutureCallback<ServiceFilterResponse>() {
			@Override
			public void onFailure(Throwable exception) {
				resultFuture.setException(exception);
			}

			@Override
			public void onSuccess(ServiceFilterResponse response) {
				removeRegistrationId(registrationName);

				resultFuture.set(null);
			}
		});

		return resultFuture;
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
	private String retrieveRegistrationId(String registrationName) {
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
	private void storeRegistrationId(String registrationName, String registrationId, String pNSHandle) {
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

		if (!currentStorageVersion.equals(STORAGE_VERSION)) {

			Editor editor = mSharedPreferences.edit();

			Set<String> keys = mSharedPreferences.getAll().keySet();

			for (String key : keys) {
				if (key.startsWith(STORAGE_PREFIX)) {
					editor.remove(key);
				}
			}

			editor.commit();

			mIsRefreshNeeded = true;
		}

	}

	private static boolean isNullOrWhiteSpace(String str) {
		return str == null || str.trim().equals("");
	}
}