package com.microsoft.windowsazure.mobileservices;

/**
 * Callback to invoke after Upsert the Registration
 */
interface UpsertRegistrationInternalCallback {
	/**
	 * Method to execute when the upsert is finished
	 * 
	 * @param registration
	 *            The current registration
	 *            
	 * @param exception
	 *            An exception representing the error, in case there was one
	 */
	public void onUpsert(Registration registration, Exception exception);
}
