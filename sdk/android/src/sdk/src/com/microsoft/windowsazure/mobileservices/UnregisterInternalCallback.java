package com.microsoft.windowsazure.mobileservices;

/**
 * Callback to invoke after unregister the registration
 */
interface UnregisterInternalCallback {
	/**
	 * Method to execute when the unregister is finished
	 * 
	 * @param registrationId
	 *            The registration Id used for unregister
	 *            
	 * @param exception
	 *            An exception representing the error, in case there was one
	 */
	public void onUnregister(String registrationId, Exception exception);
}