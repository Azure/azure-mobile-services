package com.microsoft.windowsazure.mobileservices;

/**
 * Callback to invoke after creating the registration id
 */
interface CreateRegistrationIdCallback {
	/**
	 * Method to execute when the response is ready to be processed
	 * 
	 * @param registrationId
	 *            An registration Id
	 *            
	 * @param exception
	 *            An exception representing the error, in case there was one
	 */
	public void onCreate(String registrationId, Exception exception);
}
