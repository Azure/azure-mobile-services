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
	 * @param exception
	 *            An exception representing the error, in case there was one
	 * @param response
	 *            Response object
	 */
	public void onCompleted(List<E> result, int count, Exception exception,
			ServiceFilterResponse response);
}
