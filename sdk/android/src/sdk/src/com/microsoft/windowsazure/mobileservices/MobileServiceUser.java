/*
 * MobileServiceUser.java
 */

package com.microsoft.windowsazure.mobileservices;

/**
 * Mobile Service authenticated user
 */
public class MobileServiceUser {

	/**
	 * The User Id
	 */
	private String mUserId;

	/**
	 * Mobile Service authentication token for the user
	 */
	private String mAuthenticationToken;

	/**
	 * Creates a user specifying the User Id
	 * 
	 * @param userId
	 *            The User Id
	 */
	public MobileServiceUser(String userId) {
		mUserId = userId;
	}

	/**
	 * Returns the current User Id
	 */
	public String getUserId() {
		return mUserId;
	}

	/**
	 * Sets the user's id
	 * 
	 * @param userId
	 *            The user's id
	 */
	public void setUserId(String userId) {
		mUserId = userId;
	}

	/**
	 * Returns the authentication token for the user
	 */
	public String getAuthenticationToken() {
		return mAuthenticationToken;
	}

	/**
	 * Sets the authentication token for the user
	 * 
	 * @param authenticationToken
	 *            Authentication token
	 */
	public void setAuthenticationToken(String authenticationToken) {
		mAuthenticationToken = authenticationToken;
	}
}
