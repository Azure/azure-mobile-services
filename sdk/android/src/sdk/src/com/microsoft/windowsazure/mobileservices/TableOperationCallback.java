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
	 * @param exception
	 *            An exception representing the error, in case there was one
	 * @param response
	 *            Response object
	 */
	public void onCompleted(E entity, Exception exception,
			ServiceFilterResponse response);
}
