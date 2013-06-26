package com.microsoft.windowsazure.mobileservices.push;

import android.content.Context;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.widget.Toast;

public class MobileServiceNotificationHandler {
	public void onRegistered(Context context, String gcmRegistrationId) {
	}

	public void onUnregistered(Context context, String gcmRegistrationId) {	
	}

	public void onMessage(final Context context, final Bundle bundle) {
		Handler h = new Handler(Looper.getMainLooper());
		h.post(new Runnable() {

			@Override
			public void run() {
				Toast.makeText(context, bundle.getString("message"), Toast.LENGTH_SHORT).show();
			}
		});
	}
}
