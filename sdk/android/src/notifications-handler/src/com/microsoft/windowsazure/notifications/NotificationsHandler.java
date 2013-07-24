package com.microsoft.windowsazure.notifications;

import android.content.Context;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.widget.Toast;

public class NotificationsHandler {
	
	/**
	 * Method called after the device is registered for notifications
	 * @param context Application context
	 * @param gcmRegistrationId Google Cloud Messaging registration id
	 */
	public void onRegistered(Context context, String gcmRegistrationId) {
	}

	/**
	 * Method called after the device is unregistered for notifications
	 * @param context Application context
	 * @param gcmRegistrationId Google Cloud Messaging registration id
	 */
	public void onUnregistered(Context context, String gcmRegistrationId) {	
	}

	/**
	 * Method called after a notification is received. 
	 * By default, it shows a toast with the value asociated to the "message" key in the bundle
	 * @param context Application Context
	 * @param bundle Bundle with notification data
	 */
	public void onReceive(final Context context, final Bundle bundle) {
		Handler h = new Handler(Looper.getMainLooper());
		h.post(new Runnable() {

			@Override
			public void run() {
				Toast.makeText(context, bundle.getString("message"), Toast.LENGTH_SHORT).show();
			}
		});
	}
}
