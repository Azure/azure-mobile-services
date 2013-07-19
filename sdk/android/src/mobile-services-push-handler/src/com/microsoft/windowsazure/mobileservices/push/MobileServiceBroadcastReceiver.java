package com.microsoft.windowsazure.mobileservices.push;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;

public class MobileServiceBroadcastReceiver extends BroadcastReceiver {

	@Override
	public void onReceive(Context context, Intent intent) {
		MobileServiceNotificationHandler handler = MobileServiceNotificationManager.getHandler(context);
		
		if (handler != null) {
			handler.onReceive(context, intent.getExtras());
		}
	}
}
