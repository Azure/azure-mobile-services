/*
 * UserAuthenticationCallback.java
 */

package com.microsoft.windowsazure.mobileservices;

/**
 * Callback for the user authentication process
 */
public interface UserAuthenticationCallback {
	/**
	 * Method to call if the authentication process finishes successfully
	 * 
	 * @param user
	 *            The logged user
	 */
	public void onSuccess(MobileServiceUser user);

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
