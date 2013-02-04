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
	 * Name of the key in the config setting that stores the
	 * installation ID
	 */
	private static final String INSTALLATION_ID_KEY= "applicationInstallationId";

	
	/**
	 * The ID used to identify this installation of the
	 * application to provide telemetry data.  It will either be retrieved
	 * from local settings or generated fresh.
     */
	private static String mInstallationId = null;

	/**
	 * Gets the ID used to identify this installation of the
	 * application to provide telemetry data.  It will either be retrieved
	 * from local settings or generated fresh.
	 */
    public static String getInstallationId(Context context)
    {
		SharedPreferences prefereneces = PreferenceManager.getDefaultSharedPreferences(context);
	
		if (mInstallationId == null)
		{	
			String val = prefereneces.getString(INSTALLATION_ID_KEY, null);
			mInstallationId = val;

			// Generate a new AppInstallationId if we failed to find one
            if (mInstallationId == null)
            {
                mInstallationId = UUID.randomUUID().toString();
    			
				Editor preferencesEditor = prefereneces.edit();
				preferencesEditor.putString(INSTALLATION_ID_KEY, mInstallationId);
				preferencesEditor.commit();
            }
        }
		
		return mInstallationId;

    }
}
