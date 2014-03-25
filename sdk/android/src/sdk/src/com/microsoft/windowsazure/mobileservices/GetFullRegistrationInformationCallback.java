package com.microsoft.windowsazure.mobileservices;

import java.util.ArrayList;

/**
 * Callback to invoke after Refreshing the Registration Information
 */
interface GetFullRegistrationInformationCallback {
	/**
	 * Method to execute when the response is ready to be processed
	 * 
	 * @param registrations
	 *            List of current registrations
	 *            
	 * @param exception
	 *            An exception representing the error, in case there was one
	 */
	public void onCompleted(ArrayList<Registration> registrations, Exception exception);
}

