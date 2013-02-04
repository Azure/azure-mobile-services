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
	 * @param exception
	 *            An exception representing the error, in case there was one
	 * @param response
	 *            Response object
	 */
	public void onCompleted(JsonElement result, int count, Exception exception, ServiceFilterResponse response);

}
