/*
 * DeleteOperationCallback.java
 */
package com.microsoft.windowsazure.mobileservices;

/**
 * Callback to invoke after executing a delete operation
 * 
 */
public interface TableDeleteCallback {
	/**
	 * Method to call if the operation finishes successfully
	 */
	public void onSuccess();

	/**
	 * Method to call if the operation fails
	 * 
	 * @param exception
	 *            The exception representing the error
	 * @param response
	 *            Response object
	 */
	public void onError(Exception exception, ServiceFilterResponse response);
}
