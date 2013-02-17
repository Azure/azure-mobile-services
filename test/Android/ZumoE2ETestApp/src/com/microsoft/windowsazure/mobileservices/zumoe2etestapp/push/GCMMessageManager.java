package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.push;

import java.util.ArrayList;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;

import android.content.Intent;

public class GCMMessageManager {
	static {
		instance = new GCMMessageManager();
	}
	
	public final static GCMMessageManager instance;

	private List<GCMRegistrationMessage> registrationMessages;
	private List<Intent> pushMessages;
	
	private GCMMessageManager() {
		pushMessages = new ArrayList<Intent>();
		registrationMessages = new ArrayList<GCMRegistrationMessage>();
	}

	public synchronized void newRegistrationMessage(boolean isError, String value) {
		registrationMessages.add(new GCMRegistrationMessage(value, isError));
	}
	
	public synchronized void newPushMessage(Intent intent) {
		pushMessages.add(intent);
	}

	public synchronized void waitForRegistrationMessage(long milliseconds, final GCMMessageCallback callback) {
		if (!registrationMessages.isEmpty()) {
			GCMRegistrationMessage message = registrationMessages.get(0);
			registrationMessages.remove(0);
			callback.registrationMessageReceived(message.isError, message.value);
		} else {
			TimerTask task = new TimerTask() {

				@Override
				public void run() {
					if (!registrationMessages.isEmpty()) {
						GCMRegistrationMessage message = registrationMessages.get(0);
						registrationMessages.remove(0);
						callback.registrationMessageReceived(message.isError, message.value);
					} else {
						callback.timeoutElapsed();
					}
					
				}
				
			};
			
			Timer timer = new Timer();
			timer.schedule(task, milliseconds);
		}
	}
}
