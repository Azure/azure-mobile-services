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
	 */
	public void onResponse(ServiceFilterResponse response);

	/**
	 * Method to execute when there is an error
	 * 
	 * @param exception
	 *            The exception representing the error
	 * @param response
	 *            The response that caused the error
	 */
	public void onError(Exception exception, ServiceFilterResponse response);
}
