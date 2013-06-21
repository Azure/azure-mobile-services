/*
Copyright (c) Microsoft Open Technologies, Inc.
All Rights Reserved
Apache 2.0 License
 
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
 
     http://www.apache.org/licenses/LICENSE-2.0
 
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 
See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.
 */
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

	public void waitForPushMessage(long milliseconds, final GCMMessageCallback callback) {
		if (!pushMessages.isEmpty()) {
			Intent message = pushMessages.get(0);
			pushMessages.remove(0);
			callback.pushMessageReceived(message);
		} else {
			TimerTask task = new TimerTask() {

				@Override
				public void run() {
					if (!pushMessages.isEmpty()) {
						Intent message = pushMessages.get(0);
						pushMessages.remove(0);
						callback.pushMessageReceived(message);
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
