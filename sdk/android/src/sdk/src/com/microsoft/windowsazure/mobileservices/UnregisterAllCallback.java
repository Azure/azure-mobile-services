package com.microsoft.windowsazure.mobileservices;

/**
 * Callback to invoke after unregister all registrations
 */
public interface UnregisterAllCallback {
	/**
	 * Method to execute when the unregister is finished
	 * 
	 * @param exception
	 *            An exception representing the error, in case there was one
	 */
	public void onUnregister(Exception exception);
}
