package com.microsoft.windowsazure.notifications;

import com.google.android.gms.gcm.GoogleCloudMessaging;

import android.content.Context;
import android.content.SharedPreferences;
import android.content.SharedPreferences.Editor;
import android.os.AsyncTask;
import android.preference.PreferenceManager;
import android.util.Log;

public class NotificationsManager {
	
	private static final String NOTIFICATIONS_HANDLER_CLASS = "WAMS_NotificationsHandlerClass";
	private static final String GOOGLE_CLOUD_MESSAGING_REGISTRATION_ID = "WAMS_GoogleCloudMessagingRegistrationId";

	private static NotificationsHandler mHandler;
		
	public static <T extends NotificationsHandler> void handleNotifications(final Context context, final String gcmAppId, final Class<T> notificationHandlerClass) {
		
		new AsyncTask<Void, Void, Void>() {
			@Override
			protected Void doInBackground(Void... params) {
				try {
					setHandler(notificationHandlerClass, context);
					
					GoogleCloudMessaging gcm = GoogleCloudMessaging.getInstance(context);

					String registrationId = gcm.register(gcmAppId);
					
					setRegistrationId(registrationId, context);					

					NotificationsHandler handler = getHandler(context);
					
					if (handler != null && registrationId != null) {
						getHandler(context).onRegistered(context, registrationId);
					}
				} catch (Exception e) {
					Log.e("NotificationsManager", e.toString());
				}
				
				return null;
			}
		}.execute();
	}
	
	public static void stopHandlingNotifications(final Context context) {
		
		new AsyncTask<Void, Void, Void>() {
			@Override
			protected Void doInBackground(Void... params) {
				try {
					GoogleCloudMessaging gcm = GoogleCloudMessaging.getInstance(context);
					gcm.unregister();
					
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

	static NotificationsHandler getHandler(Context context) {
		if (mHandler == null) {
			SharedPreferences prefereneces = PreferenceManager.getDefaultSharedPreferences(context.getApplicationContext());

			String className = prefereneces.getString(NOTIFICATIONS_HANDLER_CLASS, null);
			if (className != null) {
				try {
					Class<?> notificationHandlerClass = Class.forName(className);
					mHandler = (NotificationsHandler) notificationHandlerClass.newInstance();
				} catch (Exception e) {
					return null;
				}
			}
		}
		
		return mHandler;
	}
	
	private static String getRegistrationId(Context context) {
		SharedPreferences prefereneces = PreferenceManager.getDefaultSharedPreferences(context.getApplicationContext());

		String registrationId = prefereneces.getString(GOOGLE_CLOUD_MESSAGING_REGISTRATION_ID, null);
		return registrationId;
	}
	
	private static <T extends NotificationsHandler> void setHandler(Class<T> notificationHandlerClass, Context context) {
		SharedPreferences prefereneces = PreferenceManager.getDefaultSharedPreferences(context.getApplicationContext());

		Editor editor = prefereneces.edit();
		editor.putString(NOTIFICATIONS_HANDLER_CLASS, notificationHandlerClass.getName());
		editor.commit();
	}
	
	private static void setRegistrationId(String registrationId, Context context) {
		SharedPreferences prefereneces = PreferenceManager.getDefaultSharedPreferences(context.getApplicationContext());

		Editor editor = prefereneces.edit();
		editor.putString(GOOGLE_CLOUD_MESSAGING_REGISTRATION_ID, registrationId);
		editor.commit();
	}
}
