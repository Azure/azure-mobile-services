package com.microsoft.windowsazure.mobileservices;

/**
 * Callback to invoke after Delete the Registration
 */
interface DeleteRegistrationInternalCallback {
	/**
	 * Method to execute when the delete registration is finished
	 * 
	 * @param registration
	 *            The current registration
	 *            
	 * @param exception
	 *            An exception representing the error, in case there was one
	 */
	public void onDelete(String registrationId, Exception exception);
}
