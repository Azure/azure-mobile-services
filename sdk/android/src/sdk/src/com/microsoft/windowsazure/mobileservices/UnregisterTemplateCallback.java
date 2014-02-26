package com.microsoft.windowsazure.mobileservices;

/**
 * Callback to invoke after unregister the template
 */
public interface UnregisterTemplateCallback {
	/**
	 * Method to execute when the unregister is finished
	 * 
	 * @param exception
	 *            An exception representing the error, in case there was one
	 */
	public void onUnregister(Exception exception);
}