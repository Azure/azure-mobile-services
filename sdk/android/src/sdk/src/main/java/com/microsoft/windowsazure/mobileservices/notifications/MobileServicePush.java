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

import android.content.Context;
import android.content.SharedPreferences;
import android.content.SharedPreferences.Editor;
import android.net.Uri;
import android.preference.PreferenceManager;
import android.util.Pair;

import com.google.common.util.concurrent.FutureCallback;
import com.google.common.util.concurrent.Futures;
import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.SettableFuture;
import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.google.gson.JsonPrimitive;
import com.microsoft.windowsazure.mobileservices.MobileServiceApplication;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.MobileServiceFeatures;
import com.microsoft.windowsazure.mobileservices.http.MobileServiceConnection;
import com.microsoft.windowsazure.mobileservices.http.MobileServiceHttpClient;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;

import org.apache.http.Header;
import org.apache.http.protocol.HTTP;

import java.io.UnsupportedEncodingException;
import java.net.URI;
import java.net.URISyntaxException;
import java.util.ArrayList;
import java.util.EnumSet;
import java.util.List;
import java.util.Set;
import java.util.concurrent.CopyOnWriteArrayList;
import java.util.concurrent.ExecutionException;

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
     * Push registration path
     */
    private static final String PNS_API_URL = "push";

    /**
     * The class used to make HTTP clients associated with this instance
     */
    private MobileServiceHttpClient mHttpClient;

    /**
     * SharedPreferences reference used to access local storage
     */
    private SharedPreferences mSharedPreferences;

    /**
     * Factory which creates Registr
     *
     * ations according the PNS supported on device
     */
    private PnsSpecificRegistrationFactory mPnsSpecificRegistrationFactory;

    /**
     * Flag to signal the need of refreshing local storage
     */
    private boolean mIsRefreshNeeded = false;

    /**
     * Creates a new NotificationHub client
     *
     * @param notificationHubPath Notification Hub path
     * @param connectionString    Notification Hub connection string
     * @param context             Android context used to access SharedPreferences
     */
    public MobileServicePush(MobileServiceClient client, Context context) {
        mPnsSpecificRegistrationFactory = new PnsSpecificRegistrationFactory();

        mHttpClient = new MobileServiceHttpClient(client);

        if (context == null) {
            throw new IllegalArgumentException("context");
        }
    }

    private static boolean isNullOrWhiteSpace(String str) {
        return str == null || str.trim().equals("");
    }

    /**
     * Registers the client for native notifications with the specified tags
     *
     * @param pnsHandle PNS specific identifier
     * @param tags      Tags to use in the registration
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

        ListenableFuture<Void> registerInternalFuture = createOrUpdateInstallation(pnsHandle);

        Futures.addCallback(registerInternalFuture, new FutureCallback<Void>() {
            @Override
            public void onFailure(Throwable exception) {
                resultFuture.setException(exception);
            }

            @Override
            public void onSuccess(Void v) {
                resultFuture.set(registration);
            }
        });

        return resultFuture;
    }

    /**
     * Registers the client for native notifications with the specified tags
     *
     * @param pnsHandle PNS specific identifier
     * @param callback  The callback to invoke after the Push execution
     * @param tags      Tags to use in the registration
     * @deprecated use {@link register(String pnsHandle, String[] tags)} instead
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
     * @param pnsHandle    PNS specific identifier
     * @param templateName The template name
     * @param template     The template body
     * @param tags         The tags to use in the registration
     * @return Future with TemplateRegistration Information
     */
    public ListenableFuture<TemplateRegistration> registerTemplate(String pnsHandle, String templateName, String body, String[] tags) {

        final SettableFuture<TemplateRegistration> resultFuture = SettableFuture.create();

        if (isNullOrWhiteSpace(pnsHandle)) {
            resultFuture.setException(new IllegalArgumentException("pnsHandle"));
            return resultFuture;
        }

        if (isNullOrWhiteSpace(templateName)) {
            resultFuture.setException(new IllegalArgumentException("templateName"));
            return resultFuture;
        }

        if (isNullOrWhiteSpace(body)) {
            resultFuture.setException(new IllegalArgumentException("body"));
            return resultFuture;
        }

        JsonObject templateDetailObject = new JsonObject();
        templateDetailObject.addProperty("body", body);

        if (tags != null) {
            templateDetailObject.add("tags", new Gson().toJsonTree(tags));
        }

        JsonObject templateObject = new JsonObject();
        templateObject.add(templateName, templateDetailObject);

        final TemplateRegistration registration = mPnsSpecificRegistrationFactory.createTemplateRegistration();
        registration.setPNSHandle(pnsHandle);
        registration.setName(templateName);
        registration.setTemplateBody(body);
        registration.addTags(tags);

        ListenableFuture<Void> registerInternalFuture = createOrUpdateInstallation(pnsHandle, tags, templateObject);

        Futures.addCallback(registerInternalFuture, new FutureCallback<Void>() {
            @Override
            public void onFailure(Throwable exception) {
                resultFuture.setException(exception);
            }

            @Override
            public void onSuccess(Void v) {
                resultFuture.set(registration);
            }
        });

        return resultFuture;
    }

    /**
     * Registers the client for template notifications with the specified tags
     *
     * @param pnsHandle    PNS specific identifier
     * @param templateName The template name
     * @param template     The template body
     * @param tags         The tags to use in the registration
     * @param callback     The operation callback
     * @deprecated use {@link registerTemplate(String pnsHandle, String
     * templateName, String template, String[] tags)} instead
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
        return deleteInstallation();
    }

    /**
     * Unregisters the client for native notifications
     *
     * @param callback The operation callback
     * @deprecated use {@link unregister()} instead
     */
    public void unregister(final UnregisterCallback callback) {
        ListenableFuture<Void> deleteInstallationFuture = deleteInstallation();

        Futures.addCallback(deleteInstallationFuture, new FutureCallback<Void>() {
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
     * @param templateName The template name
     * @return Future with TemplateRegistration Information
     */
    public ListenableFuture<Void> unregisterTemplate(String templateName) {
        if (isNullOrWhiteSpace(templateName)) {
            throw new IllegalArgumentException("templateName");
        }

        return deleteInstallation();
    }

    /**
     * Unregisters the client for template notifications of a specific template
     *
     * @param templateName The template name
     * @param callback     The operation callback
     * @deprecated use {@link unregisterTemplate(String templateName)} instead
     */
    public void unregisterTemplate(String templateName, final UnregisterCallback callback) {
        ListenableFuture<Void> deleteInstallationFuture = deleteInstallation();

        Futures.addCallback(deleteInstallationFuture, new FutureCallback<Void>() {
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
     * @param pnsHandle PNS specific identifier
     * @return Future with TemplateRegistration Information
     */
    public ListenableFuture<Void> unregisterAll(String pnsHandle) {
        return deleteInstallation();
    }

    /**
     * Unregisters the client for all notifications
     *
     * @param pnsHandle PNS specific identifier
     * @param callback  The operation callback
     * @deprecated use {@link unregisterAll(String pnsHandle)} instead
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

    public ListenableFuture<JsonElement> deleteRegistrationsForChannel(String registrationId) {

        final SettableFuture<JsonElement> resultFuture = SettableFuture.create();

        ArrayList<Pair<String, String>> parameters = new ArrayList<Pair<String, String>>();

        parameters.add(new Pair<String, String>("channelUri", registrationId));

        ListenableFuture<JsonElement> serviceFilterFuture = mHttpClient.getClient().invokeApi("deleteRegistrationsForChannel", "DELETE", parameters);

        Futures.addCallback(serviceFilterFuture, new FutureCallback<JsonElement>() {
            @Override
            public void onFailure(Throwable exception) {
                resultFuture.setException(exception);
            }

            @Override
            public void onSuccess(JsonElement response) {
                resultFuture.set(null);
            }
        });

        return resultFuture;
    }

    private ListenableFuture<Void> deleteInstallation()
    {
        final SettableFuture<Void> resultFuture = SettableFuture.create();

        String installationId = MobileServiceApplication.getInstallationId(mHttpClient.getClient().getContext());

        String path = PNS_API_URL + "/installations/" + Uri.encode(installationId);

        ListenableFuture<ServiceFilterResponse> serviceFilterFuture = mHttpClient.request(path, null, "DELETE", null, null);

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

    public ListenableFuture<Void> createOrUpdateInstallation(String registrationId) {
        return createOrUpdateInstallation(registrationId, null, null);
    }

    public ListenableFuture<Void> createOrUpdateInstallation(String registrationId, String[] tags) {
        return createOrUpdateInstallation(registrationId, tags, null);
    }

    private ListenableFuture<Void> createOrUpdateInstallation(String registrationId, String[] tags, JsonElement templates)
    {
        JsonObject installation = new JsonObject();
        installation.addProperty("pushChannel", registrationId);
        installation.addProperty("platform", "gcm");

        if (tags != null){
            installation.add("tags", new Gson().toJsonTree(tags));
        }

        if (templates != null) {
            installation.add("templates", templates);
        }

        final SettableFuture<Void> resultFuture = SettableFuture.create();

        String installationId = MobileServiceApplication.getInstallationId(mHttpClient.getClient().getContext());

        String path = PNS_API_URL+ "/installations/" + Uri.encode(installationId);

        List<Pair<String, String>> headers = new ArrayList<Pair<String, String>>();

        Pair<String, String> header = new Pair<String, String>("Content-Type", "application/json");

        headers.add(header);

        ListenableFuture<ServiceFilterResponse> serviceFilterFuture = mHttpClient.request(path, installation.toString(), "PUT", headers, null, EnumSet.noneOf(MobileServiceFeatures.class));

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

}