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
 * RegistrationGoneException.java
 */

package com.microsoft.windowsazure.mobileservices;

/**
 * Represents an exception when registration is gone
 */
public class RegistrationGoneException extends Exception {

	private static final long serialVersionUID = -156200383034074631L;

	/**
	 * Creates a RegistrationGoneException
	 */
	RegistrationGoneException() {
		super("Registration is gone");
	}

	/**
	 * Creates a RegistrationGoneException
	 * 
	 * @param cause
	 *            The Exception that caused the current instance
	 */
	RegistrationGoneException(Exception cause) {
		super("Registration is gone", cause);
	}

}
