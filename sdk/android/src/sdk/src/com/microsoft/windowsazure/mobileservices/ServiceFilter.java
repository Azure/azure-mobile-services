/*
 * ServiceFilter.java
 */

package com.microsoft.windowsazure.mobileservices;

/**
 * The service filter can be used to manipulate requests and responses in the
 * HTTP pipeline used by the MobileServiceClient. ServiceFilters can be
 * associated with a MobileServiceClient via the WithFilter method.
 */
public interface ServiceFilter {
	/**
	 * Method to handle the requests
	 * 
	 * @param request
	 *            Request to execute
	 * @param nextServiceFilterCallback
	 *            The next filter to execute
	 * @param responseCallback
	 *            The callback to invoke once the request is executed
	 */
	public void handleRequest(ServiceFilterRequest request,
			NextServiceFilterCallback nextServiceFilterCallback,
			ServiceFilterResponseCallback responseCallback);
}
