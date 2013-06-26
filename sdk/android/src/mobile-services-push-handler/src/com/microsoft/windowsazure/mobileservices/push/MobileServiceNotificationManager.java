package com.microsoft.windowsazure.mobileservices.push;

import com.google.android.gcm.GCMRegistrar;

import android.content.Context;
import android.content.SharedPreferences;
import android.content.SharedPreferences.Editor;
import android.preference.PreferenceManager;

public class MobileServiceNotificationManager {
	
	private static final String NOTIFICATION_HANDLER_CLASS = "WAMS_MobileServiceNotificationHandlerClass";

	public static <T extends MobileServiceNotificationHandler> void handleNotifications(Context context, String gcmAppId, Class<T> notificationHandlerClass) {
		setHandler(notificationHandlerClass, context);
		
		String registrationId = GCMRegistrar.getRegistrationId(context);

		if (registrationId == null || registrationId.trim().equals("")) {
			com.google.android.gcm.GCMRegistrar.register(context, gcmAppId);
		}
	}

	private static MobileServiceNotificationHandler mHandler;

	public static MobileServiceNotificationHandler getHandler(Context context) {
		if (mHandler == null) {
			SharedPreferences prefereneces = PreferenceManager.getDefaultSharedPreferences(context.getApplicationContext());

			String className = prefereneces.getString(NOTIFICATION_HANDLER_CLASS, null);
			if (className != null) {
				try {
					Class<?> notificationHandlerClass = Class.forName(className);
					mHandler = (MobileServiceNotificationHandler) notificationHandlerClass.newInstance();
				} catch (Exception e) {
					return null;
				}
			}
		}
		return mHandler;
	}
	
	private static <T extends MobileServiceNotificationHandler> void setHandler(Class<T> notificationHandlerClass, Context context) {
		SharedPreferences prefereneces = PreferenceManager.getDefaultSharedPreferences(context.getApplicationContext());

		Editor editor = prefereneces.edit();
		editor.putString(NOTIFICATION_HANDLER_CLASS, notificationHandlerClass.getName());
		editor.commit();
	}
}
