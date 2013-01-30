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
	 */
	public void onSuccess(JsonObject jsonEntity);

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