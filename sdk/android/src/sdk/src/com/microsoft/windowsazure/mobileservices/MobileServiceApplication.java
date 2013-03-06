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
package com.microsoft.windowsazure.mobileservices;

import java.util.UUID;

import android.content.Context;
import android.content.SharedPreferences;
import android.content.SharedPreferences.Editor;
import android.preference.PreferenceManager;

/**
 * Provides context regarding the application that is using the Mobile Service.
 */
public final class MobileServiceApplication {

	/**
	 * Name of the key in the config setting that stores the installation ID
	 */
	private static final String INSTALLATION_ID_KEY = "applicationInstallationId";

	/**
	 * The ID used to identify this installation of the application to provide
	 * telemetry data. It will either be retrieved from local settings or
	 * generated fresh.
	 */
	private static String mInstallationId = null;

	/**
	 * Gets the ID used to identify this installation of the application to
	 * provide telemetry data. It will either be retrieved from local settings
	 * or generated fresh.
	 * 
	 * @param context
	 *            The context used to manage the application preferences
	 * @return The Installation ID
	 */
	public static String getInstallationId(Context context) {
		SharedPreferences prefereneces = PreferenceManager
				.getDefaultSharedPreferences(context.getApplicationContext());

		if (mInstallationId == null) {
			String val = prefereneces.getString(INSTALLATION_ID_KEY, null);
			mInstallationId = val;

			// Generate a new AppInstallationId if we failed to find one
			if (mInstallationId == null) {
				mInstallationId = UUID.randomUUID().toString();

				Editor preferencesEditor = prefereneces.edit();
				preferencesEditor.putString(INSTALLATION_ID_KEY,
						mInstallationId);
				preferencesEditor.commit();
			}
		}

		return mInstallationId;

	}
}
