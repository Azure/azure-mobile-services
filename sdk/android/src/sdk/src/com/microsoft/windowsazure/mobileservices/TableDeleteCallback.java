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
	 * 
	 * @param exception
	 *            An exception representing the error, in case there was one
	 * @param response
	 *            Response object
	 */
	public void onCompleted(Exception exception, ServiceFilterResponse response);;
}
