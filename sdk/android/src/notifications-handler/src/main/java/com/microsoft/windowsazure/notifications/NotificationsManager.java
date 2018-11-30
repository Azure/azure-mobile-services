package com.microsoft.windowsazure.notifications;

import android.content.Context;
import android.content.SharedPreferences;
import android.content.SharedPreferences.Editor;
import android.os.AsyncTask;
import android.preference.PreferenceManager;
import android.util.Log;

import com.google.firebase.iid.FirebaseInstanceId;
import com.google.firebase.iid.InstanceIdResult;
import com.google.firebase.messaging.FirebaseMessaging;

import com.google.android.gms.tasks.OnSuccessListener;

public class NotificationsManager {

    /**
     * Key for handler class name in local storage
     */
    private static final String NOTIFICATIONS_HANDLER_CLASS = "WAMS_NotificationsHandlerClass";

    /**
     * Key for registration id in local storage
     */
    private static final String GOOGLE_CLOUD_MESSAGING_REGISTRATION_ID = "WAMS_GoogleCloudMessagingRegistrationId";

    /**
     * NotificationsHandler instance
     */
    private static NotificationsHandler mHandler;

    /**
     * Handles notifications with the provided NotificationsHandler class
     *
     * @param context                   Application Context
     * @param fcmAppId                  Firebase Cloud Messaging Application ID
     * @param notificationsHandlerClass NotificationHandler class used for handling notifications
     */
    public static <T extends NotificationsHandler> void handleNotifications(final Context context, final String fcmAppId, final Class<T> notificationsHandlerClass) {

        new AsyncTask<Void, Void, Void>() {
            @Override
            protected Void doInBackground(Void... params) {
                try {
                    setHandler(notificationsHandlerClass, context);

                    FirebaseMessaging.getInstance().setAutoInitEnabled(true);

                    FirebaseInstanceId.getInstance().getInstanceId().addOnSuccessListener(new OnSuccessListener<InstanceIdResult>() {
                        @Override
                        public void onSuccess(InstanceIdResult instanceIdResult) {
                            String registrationId = fcmAppId;

                            setRegistrationId(registrationId, context);

                            NotificationsHandler handler = getHandler(context);

                            if (handler != null && registrationId != null) {
                                handler.onRegistered(context, registrationId);
                            }

                        }
                    });


                } catch (Exception e) {
                    Log.e("NotificationsManager", e.toString());
                }

                return null;
            }
        }.execute();
    }

    /**
     * Stops handlind notifications
     *
     * @param context Application Context
     */
    public static void stopHandlingNotifications(final Context context) {

        new AsyncTask<Void, Void, Void>() {
            @Override
            protected Void doInBackground(Void... params) {
                try {
                    FirebaseMessaging.getInstance().setAutoInitEnabled(false);

                    FirebaseInstanceId.getInstance().deleteInstanceId();

                    String registrationId = getRegistrationId(context);

                    setRegistrationId(null, context);

                    NotificationsHandler handler = getHandler(context);

                    if (handler != null && registrationId != null) {
                        handler.onUnregistered(context, registrationId);
                    }
                } catch (Exception e) {
                    Log.e("NotificationsManager", e.toString());
                }

                return null;
            }
        }.execute();
    }

    /**
     * Retrieves the NotificationsHandler from local storage
     *
     * @param context Application Context
     */
    static NotificationsHandler getHandler(Context context) {
        if (mHandler == null) {
            SharedPreferences prefereneces = PreferenceManager.getDefaultSharedPreferences(context.getApplicationContext());

            String className = prefereneces.getString(NOTIFICATIONS_HANDLER_CLASS, null);
            if (className != null) {
                try {
                    Class<?> notificationsHandlerClass = Class.forName(className);
                    mHandler = (NotificationsHandler) notificationsHandlerClass.newInstance();
                } catch (Exception e) {
                    return null;
                }
            }
        }

        return mHandler;
    }

    /**
     * Retrieves the RegistrationId from local storage
     *
     * @param context Application Context
     */
    private static String getRegistrationId(Context context) {
        SharedPreferences prefereneces = PreferenceManager.getDefaultSharedPreferences(context.getApplicationContext());

        String registrationId = prefereneces.getString(GOOGLE_CLOUD_MESSAGING_REGISTRATION_ID, null);
        return registrationId;
    }


    /**
     * Stores the NotificationsHandler class in local storage
     *
     * @param notificationsHandlerClass NotificationsHandler class
     * @param context                   Application Context
     */
    private static <T extends NotificationsHandler> void setHandler(Class<T> notificationsHandlerClass, Context context) {
        SharedPreferences prefereneces = PreferenceManager.getDefaultSharedPreferences(context.getApplicationContext());

        Editor editor = prefereneces.edit();
        editor.putString(NOTIFICATIONS_HANDLER_CLASS, notificationsHandlerClass.getName());
        editor.commit();
    }

    /**
     * Stores the RegistrationId in local storage
     *
     * @param registrationId RegistrationId to store
     * @param context        Application Context
     */
    private static void setRegistrationId(String registrationId, Context context) {
        SharedPreferences prefereneces = PreferenceManager.getDefaultSharedPreferences(context.getApplicationContext());

        Editor editor = prefereneces.edit();
        editor.putString(GOOGLE_CLOUD_MESSAGING_REGISTRATION_ID, registrationId);
        editor.commit();
    }
}
