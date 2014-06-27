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
/**
 * MobileServiceException.java
 */

package com.microsoft.windowsazure.mobileservices;

import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;

public class MobileServiceException extends Exception {

	/**
	 * UID used for serialization
	 */
	private static final long serialVersionUID = 5267990724102948298L;
	private ServiceFilterResponse mResponse;

	public MobileServiceException(Throwable throwable, ServiceFilterResponse response) {
		this("There was an error executing the request", throwable, response);
	}

	public MobileServiceException(String detail, Throwable throwable, ServiceFilterResponse response) {
		this(detail, throwable);
		mResponse = response;
	}

	public MobileServiceException(String detail, ServiceFilterResponse response) {
		this(detail);
		mResponse = response;
	}

	/**
	 * Creates a new MobileServiceException with a detail message and a cause
	 * 
	 * @param detail
	 *            The detail message
	 * @param throwable
	 *            The exception cause
	 */
	public MobileServiceException(String detail, Throwable throwable) {
		super(detail, throwable);
	}

	/**
	 * Creates a new MobileServiceException with a detail message
	 * 
	 * @param detail
	 *            The detail message
	 */
	public MobileServiceException(String detail) {
		super(detail);
	}

	public ServiceFilterResponse getResponse() {
		return mResponse;
	}

	public static ServiceFilterResponse getServiceResponse(Throwable throwable) {

		if (!(throwable instanceof MobileServiceException))
			return null;

		MobileServiceException exception = (MobileServiceException) throwable;

		return exception.getResponse();
	}
}
