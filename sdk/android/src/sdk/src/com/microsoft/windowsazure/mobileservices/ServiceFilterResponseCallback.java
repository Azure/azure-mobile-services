/*
 * ServiceFilterResponseCallback.java
 */

package com.microsoft.windowsazure.mobileservices;

/**
 * Callback to invoke after processing the ServiceFilters and executing the
 * request
 * 
 */
public interface ServiceFilterResponseCallback {
	/**
	 * Method to execute when the response is ready to be processed
	 * 
	 * @param response
	 *            The response to process
	 * @param exception
	 *            An exception representing the error, in case there was one
	 */
	public void onResponse(ServiceFilterResponse response, Exception exception);
}
