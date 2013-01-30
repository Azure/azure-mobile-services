/*
 * TableOperationCallback.java
 */

package com.microsoft.windowsazure.mobileservices;

/**
 * 
 * Callback used after a TableOperation is executed
 * 
 * @param <E>
 *            The table's entity
 */
public interface TableOperationCallback<E> {
	/**
	 * Method to call if the operation finishes successfully
	 * 
	 * @param entity
	 *            The obtained entity
	 */
	public void onSuccess(E entity);

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
