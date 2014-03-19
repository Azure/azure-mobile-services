package com.microsoft.windowsazure.mobileservices;
/**
 * Callback to invoke after completed the internal registration
 */
interface RegisterInternalCallback {
	/**
	 * Method to execute when the registration is finished
	 * 
	 * @param registration
	 *            The current registration
	 *            
	 * @param exception
	 *            An exception representing the error, in case there was one
	 */
	public void onRegister(Registration registration, Exception exception);
}
