package com.microsoft.windowsazure.notifications;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;

public class NotificationsBroadcastReceiver extends BroadcastReceiver {

	@Override
	public void onReceive(Context context, Intent intent) {
		NotificationsHandler handler = NotificationsManager.getHandler(context);
		
		if (handler != null) {
			handler.onReceive(context, intent.getExtras());
		}
	}
}
