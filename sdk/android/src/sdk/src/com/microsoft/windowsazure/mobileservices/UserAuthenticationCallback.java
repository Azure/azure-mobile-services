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
	 * @param exception
	 *            An exception representing the error, in case there was one
	 * @param response
	 *            Response object
	 */
	public void onCompleted(MobileServiceUser user, Exception exception, ServiceFilterResponse response);
}
