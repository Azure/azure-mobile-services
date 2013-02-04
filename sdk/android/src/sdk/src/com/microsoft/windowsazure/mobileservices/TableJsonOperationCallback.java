/**
 * TableOperationCallback.java
 */

package com.microsoft.windowsazure.mobileservices;

import com.google.gson.JsonObject;

/**
 * Callback used after a TableOperation is executed using JSON
 * 
 */
public interface TableJsonOperationCallback {
	/**
	 * Method to call if the operation finishes successfully
	 * 
	 * @param jsonEntity
	 *            The obtained jsonEntity
	 * @param exception
	 *            An exception representing the error, in case there was one
	 * @param response
	 *            Response object
	 */
	public void onCompleted(JsonObject jsonEntity, Exception exception,
			ServiceFilterResponse response);
}