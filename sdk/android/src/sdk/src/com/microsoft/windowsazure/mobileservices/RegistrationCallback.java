package com.microsoft.windowsazure.mobileservices;

/**
 * Callback to invoke after register the template
 */
public interface RegistrationCallback {
	/**
	 * Method to execute when the register is finished
	 * 
	 * @param registration
	 *            The current registration
	 *            
	 * @param exception
	 *            An exception representing the error, in case there was one
	 */
	public void onRegister(Registration registration, Exception exception);
}