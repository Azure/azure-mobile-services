/*
 * NextServiceFilterCallback.java
 */

package com.microsoft.windowsazure.mobileservices;

/**
 * Callback used to chain service filters
 */
public interface NextServiceFilterCallback {
	/**
	 * Method called to execute the next ServiceFilter in the pipeline
	 * 
	 * @param request
	 *            The ServiceFilterRequest to process
	 * @param responseCallback
	 *            The ServiceFilterResponseCallback to invoke when the response
	 *            is obtained
	 */
	public void onNext(ServiceFilterRequest request,
			ServiceFilterResponseCallback responseCallback);
}
