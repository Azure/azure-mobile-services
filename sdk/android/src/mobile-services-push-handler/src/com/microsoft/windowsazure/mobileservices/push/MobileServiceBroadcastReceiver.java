package com.microsoft.windowsazure.mobileservices.push;

import android.content.Context;

import com.google.android.gcm.GCMBroadcastReceiver;

public class MobileServiceBroadcastReceiver extends GCMBroadcastReceiver {

	@Override
	protected String getGCMIntentServiceClassName(Context context) {
		return MobileServiceGCMIntentService.class.getName();
	}
}
