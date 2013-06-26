package com.microsoft.windowsazure.mobileservices.push;

import android.content.Context;
import android.content.Intent;
import android.util.Log;

import com.google.android.gcm.GCMBaseIntentService;

public class MobileServiceGCMIntentService extends GCMBaseIntentService {

	@Override
	protected void onError(Context context, String errorId) {
		Log.d("MobileServiceGCMIntentService", "Error: " + errorId);
	}

	@Override
	protected void onMessage(Context context, Intent intent) {
		MobileServiceNotificationHandler handler = MobileServiceNotificationManager.getHandler(context);
		
		if (handler != null) {
			handler.onMessage(context, intent.getExtras());
		}
	}

	@Override
	protected void onRegistered(Context context, String gcmRegistrationId) {
		MobileServiceNotificationHandler handler = MobileServiceNotificationManager.getHandler(context);
		
		if (handler != null) {
			handler.onRegistered(context, gcmRegistrationId);
		}
	}

	@Override
	protected void onUnregistered(Context context, String gcmRegistrationId) {
		MobileServiceNotificationHandler handler = MobileServiceNotificationManager.getHandler(context);
		
		if (handler != null) {
			handler.onUnregistered(context, gcmRegistrationId);
		}
	}
}
