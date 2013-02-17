package com.microsoft.windowsazure.mobileservices.zumoe2etestapp;

import android.content.Context;
import android.content.Intent;

import com.google.android.gcm.GCMBaseIntentService;

import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.push.GCMMessageManager;

public class GCMIntentService extends GCMBaseIntentService {

	@Override
	protected void onError(Context context, String errorId) {
		GCMMessageManager.instance.newRegistrationMessage(true, errorId);

	}

	@Override
	protected void onMessage(Context context, Intent intent) {
		// TODO Auto-generated method stub

	}

	@Override
	protected void onRegistered(Context context, String registrationId) {
		GCMMessageManager.instance.newRegistrationMessage(false, registrationId);

	}

	@Override
	protected void onUnregistered(Context context, String registrationId) {

	}

}
