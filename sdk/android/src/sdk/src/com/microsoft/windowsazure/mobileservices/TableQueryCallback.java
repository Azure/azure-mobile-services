/*
 * QueryResultCallback.java
 */

package com.microsoft.windowsazure.mobileservices;

import java.util.List;

/**
 * Callback used after a query is executed
 */
public interface TableQueryCallback<E> {

	/**
	 * Method to call if the operation finishes successfully
	 * 
	 * @param result
	 *            List of entities
	 * @param count
	 *            Number of results
	 */
	public void onSuccess(List<E> result, int count);

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
