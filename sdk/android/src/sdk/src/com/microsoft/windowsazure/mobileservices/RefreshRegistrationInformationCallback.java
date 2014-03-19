package com.microsoft.windowsazure.mobileservices;

/**
 * Callback to invoke after Refreshing the Registration Information
 */
interface RefreshRegistrationInformationCallback {
	/**
	 * Method to execute when the response is ready to be processed
	 * 
	 * @param exception
	 *            An exception representing the error, in case there was one
	 */
	public void onRefresh(Exception exception);
}

