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

import java.net.URI;
import java.util.ArrayList;
import java.util.List;
import java.util.Set;
import java.util.concurrent.CountDownLatch;

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
	public Registration register(String pnsHandle, String... tags) throws Exception {
		if (isNullOrWhiteSpace(pnsHandle)) {
			throw new IllegalArgumentException("pnsHandle");
		}

		Registration registration = mPnsSpecificRegistrationFactory.createNativeRegistration();
		registration.setPNSHandle(pnsHandle);
		registration.setName(Registration.DEFAULT_REGISTRATION_NAME);
		registration.addTags(tags);

		return registerInternal(registration);
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
	 * @return The created registration
	 * @throws Exception
	 */
	public TemplateRegistration registerTemplate(String pnsHandle, String templateName, String template, String... tags) throws Exception {
		if (isNullOrWhiteSpace(pnsHandle)) {
			throw new IllegalArgumentException("pnsHandle");
		}

		if (isNullOrWhiteSpace(templateName)) {
			throw new IllegalArgumentException("templateName");
		}

		if (isNullOrWhiteSpace(template)) {
			throw new IllegalArgumentException("template");
		}

		TemplateRegistration registration = mPnsSpecificRegistrationFactory.createTemplateRegistration();
		registration.setPNSHandle(pnsHandle);
		registration.setName(templateName);
		registration.setBodyTemplate(template);
		registration.addTags(tags);

		return (TemplateRegistration) registerInternal(registration);
	}

	/**
	 * Unregisters the client for native notifications
	 * 
	 * @throws Exception
	 */
	public void unregister() throws Exception {
		unregisterInternal(Registration.DEFAULT_REGISTRATION_NAME);
	}

	/**
	 * Unregisters the client for template notifications of a specific template
	 * 
	 * @param templateName
	 *            The template name
	 * @throws Exception
	 */
	public void unregisterTemplate(String templateName) throws Exception {
		if (isNullOrWhiteSpace(templateName)) {
			throw new IllegalArgumentException("templateName");
		}

		unregisterInternal(templateName);
	}

	/**
	 * Unregisters the client for all notifications
	 * 
	 * @param pnsHandle
	 *            PNS specific identifier
	 * @throws Exception
	 */
	public void unregisterAll(String pnsHandle) throws Exception {
		refreshRegistrationInformation(pnsHandle);

		Set<String> keys = mSharedPreferences.getAll().keySet();

		for (String key : keys) {
			if (key.startsWith(STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY)) {
				String registrationName = key.substring((STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY).length());
				String registrationId = mSharedPreferences.getString(key, "");

				deleteRegistrationInternal(registrationName, registrationId);
			}
		}
	}

	private void refreshRegistrationInformation(String pnsHandle) throws Exception {
		if (isNullOrWhiteSpace(pnsHandle)) {
			throw new IllegalArgumentException("pnsHandle");
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

		// get existing registrations
		final CountDownLatch latch = new CountDownLatch(1);
		final ResultContainer<String> resultContainer = new ResultContainer<String>();

		String filter = "platform='" + mPnsSpecificRegistrationFactory.getPlatform() + "'";
		String resource = "/registrations/?" + filter;

		List<Pair<String, String>> requestHeaders = new ArrayList<Pair<String, String>>();
		requestHeaders.add(new Pair<String, String>(HTTP.CONTENT_TYPE, MobileServiceConnection.JSON_CONTENTTYPE));

		mClient.invokeApiInternal(resource, null, "GET", requestHeaders, null, MobileServiceClient.PNS_API_URL, new ServiceFilterResponseCallback() {

			@Override
			public void onResponse(ServiceFilterResponse response, Exception exception) {
				if (exception != null) {
					resultContainer.setException(exception);
				} else {

					resultContainer.setItem(response.getContent());
				}

				latch.countDown();
			}
		});

		latch.await();

		Exception ex = resultContainer.getException();
		if (ex != null) {
			throw ex;
		}

		String response = resultContainer.getItem();

		JsonArray registrations = new JsonParser().parse(response).getAsJsonArray();

		for (JsonElement registrationJson : registrations) {
			Registration registration = null;
			if (registrationJson.getAsJsonObject().has("templateName")) {
				registration = mPnsSpecificRegistrationFactory.parseTemplateRegistration(registrationJson.getAsJsonObject());
			} else {
				registration = mPnsSpecificRegistrationFactory.parseNativeRegistration(registrationJson.getAsJsonObject());
			}

			storeRegistrationId(registration.getName(), registration.getRegistrationId(), registration.getPNSHandle());
		}

		mIsRefreshNeeded = false;
	}

	/**
	 * Creates a new registration in the server. If it exists, updates its
	 * information
	 * 
	 * @param registration
	 *            The registration to create
	 * @return The created registration
	 * @throws Exception
	 */
	private Registration registerInternal(Registration registration) throws Exception {

		if (mIsRefreshNeeded) {
			String pNSHandle = mSharedPreferences.getString(STORAGE_PREFIX + PNS_HANDLE_KEY, "");

			if (isNullOrWhiteSpace(pNSHandle)) {
				pNSHandle = registration.getPNSHandle();
			}

			refreshRegistrationInformation(pNSHandle);
		}

		String registrationId = retrieveRegistrationId(registration.getName());
		if (isNullOrWhiteSpace(registrationId)) {
			registrationId = createRegistrationId();
		}

		registration.setRegistrationId(registrationId);

		try {
			return upsertRegistrationInternal(registration);
		} catch (RegistrationGoneException e) {
			// if we get an RegistrationGoneException (410) from service, we
			// will recreate registration id and will try to do upsert one more
			// time.
		}

		registrationId = createRegistrationId();
		registration.setRegistrationId(registrationId);
		return upsertRegistrationInternal(registration);
	}

	/**
	 * Deletes a registration and removes it from local storage
	 * 
	 * @param registrationName
	 *            The registration name
	 * @throws Exception
	 */
	private void unregisterInternal(String registrationName) throws Exception {
		String registrationId = retrieveRegistrationId(registrationName);

		if (!isNullOrWhiteSpace(registrationId)) {
			deleteRegistrationInternal(registrationName, registrationId);
		}
	}

	private String createRegistrationId() throws Exception {
		final CountDownLatch latch = new CountDownLatch(1);
		final ResultContainer<String> location = new ResultContainer<String>();

		String resource = "/registrationids/";
		mClient.invokeApiInternal(resource, null, "POST", null, null, MobileServiceClient.PNS_API_URL, new ServiceFilterResponseCallback() {

			@Override
			public void onResponse(ServiceFilterResponse response, Exception exception) {
				if (exception != null) {
					location.setException(exception);
				} else {
					for (Header header : response.getHeaders()) {
						if (header.getName().equalsIgnoreCase(NEW_REGISTRATION_LOCATION_HEADER)) {
							location.setItem(header.getValue());
						}
					}
				}

				latch.countDown();
			}
		});

		latch.await();

		Exception ex = location.getException();
		if (ex != null) {
			throw ex;
		}

		URI regIdUri = new URI(location.getItem());
		String[] pathFragments = regIdUri.getPath().split("/");
		String result = pathFragments[pathFragments.length - 1];

		return result;
	}

	/**
	 * Updates a registration
	 * 
	 * @param registration
	 *            The registration to update
	 * @return The updated registration
	 * @throws Exception
	 */
	private Registration upsertRegistrationInternal(Registration registration) throws Exception {
		final CountDownLatch latch = new CountDownLatch(1);
		final ResultContainer<String> responseContainer = new ResultContainer<String>();

		GsonBuilder gsonBuilder = new GsonBuilder();
		gsonBuilder = gsonBuilder.excludeFieldsWithoutExposeAnnotation();

		Gson gson = gsonBuilder.create();

		String resource = registration.getURI();
		JsonElement json = gson.toJsonTree(registration);
		String body = json.toString();
		byte[] content = body.getBytes(MobileServiceClient.UTF8_ENCODING);

		List<Pair<String, String>> requestHeaders = new ArrayList<Pair<String, String>>();
		requestHeaders.add(new Pair<String, String>(HTTP.CONTENT_TYPE, MobileServiceConnection.JSON_CONTENTTYPE));

		mClient.invokeApiInternal(resource, content, "PUT", requestHeaders, null, MobileServiceClient.PNS_API_URL, new ServiceFilterResponseCallback() {

			@Override
			public void onResponse(ServiceFilterResponse response, Exception exception) {
				if (exception != null) {
					if (response.getStatus().getStatusCode() == 410) {
						exception = new RegistrationGoneException(exception);
					}

					responseContainer.setException(exception);
				}

				latch.countDown();
			}
		});

		latch.await();

		Exception ex = responseContainer.getException();
		if (ex != null) {
			throw ex;
		}

		return registration;
	}

	/**
	 * Deletes a registration and removes it from local storage
	 * 
	 * @param regInfo
	 *            The reginfo JSON object
	 * @throws Exception
	 */
	private void deleteRegistrationInternal(String registrationName, String registrationId) throws Exception {
		final CountDownLatch latch = new CountDownLatch(1);
		final ResultContainer<String> responseContainer = new ResultContainer<String>();

		String resource = "/registrations/" + registrationId;

		mClient.invokeApiInternal(resource, null, "DELETE", null, null, MobileServiceClient.PNS_API_URL, new ServiceFilterResponseCallback() {

			@Override
			public void onResponse(ServiceFilterResponse response, Exception exception) {
				if (exception != null) {
					responseContainer.setException(exception);
				}

				latch.countDown();
			}
		});

		latch.await();

		removeRegistrationId(registrationName);

		Exception ex = responseContainer.getException();
		if (ex != null) {
			throw ex;
		}
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
	private void removeRegistrationId(String registrationName) throws Exception {
		Editor editor = mSharedPreferences.edit();

		editor.remove(STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY + registrationName);

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
