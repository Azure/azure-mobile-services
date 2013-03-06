/*
Copyright (c) Microsoft Open Technologies, Inc.
All Rights Reserved
Apache 2.0 License
 
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
 
     http://www.apache.org/licenses/LICENSE-2.0
 
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 
See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.
 */
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
