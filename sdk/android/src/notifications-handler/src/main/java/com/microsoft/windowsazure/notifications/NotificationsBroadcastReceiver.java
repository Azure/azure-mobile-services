package com.microsoft.windowsazure.notifications;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;

public class NotificationsBroadcastReceiver extends BroadcastReceiver {

    @Override
    public void onMessageReceived(RemoteMessage remoteMessage) {

        Context context = getApplicationContext();
        NotificationsHandler handler = NotificationsManager.getHandler(context);

        if (handler != null) {

            Bundle bundle = new Bundle();
            for (Map.Entry<String, String> entry : remoteMessage.getData().entrySet()) {
                bundle.putString(entry.getKey(), entry.getValue());
            }

            handler.onReceive(context, bundle);
        }
    }
}
