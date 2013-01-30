/**
 * TableJsonQueryCallback.java
 */

package com.microsoft.windowsazure.mobileservices;

import com.google.gson.JsonElement;

/**
 * Callback used after a query is executed using JSON
 * 
 */
public interface TableJsonQueryCallback {
	/**
	 * Method to call if the operation finishes successfully
	 * 
	 * @param result
	 *            JSON result
	 * @param count
	 *            Number of results
	 */
	public void onSuccess(JsonElement result, int count);

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
