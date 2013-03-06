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
 * MobileServiceConnection.java
 */

package com.microsoft.windowsazure.mobileservices;

import org.apache.http.protocol.HTTP;

import android.os.Build;

/**
 * Class for handling communication with Windows Azure Mobile Services REST APIs
 */
class MobileServiceConnection {

	/**
	 * The MobileServiceClient used for communication with the Mobile Service
	 */
	private MobileServiceClient mClient;

	/**
	 * Request header to indicate the Mobile Service application key
	 */
	private static final String X_ZUMO_APPLICATION_HEADER = "X-ZUMO-APPLICATION";

	/**
	 * Request header to indicate the Mobile Service Installation ID
	 */
	private static final String X_ZUMO_INSTALLATION_ID_HEADER = "X-ZUMO-INSTALLATION-ID";

	/**
	 * Request header to indicate the Mobile Service user authentication token
	 */
	private static final String X_ZUMO_AUTH_HEADER = "X-ZUMO-AUTH";

	/**
	 * Header value to represent JSON content-type
	 */
	private static final String JSON_CONTENTTYPE = "application/json";

	/**
	 * Current SDK version
	 */
	private static final String SDK_VERSION = "1.0";

	/**
	 * Constructor for the MobileServiceConnection
	 * 
	 * @param client
	 *            The client used for communication with the Mobile Service
	 */
	public MobileServiceConnection(MobileServiceClient client) {
		mClient = client;
	}

	/**
	 * Execute a request-response operation with a Mobile Service
	 * 
	 * @param request
	 *            The request to execute
	 * @param responseCallback
	 *            Callback to invoke after the request is executed
	 */
	public void start(final ServiceFilterRequest request,
			ServiceFilterResponseCallback responseCallback) {
		if (request == null) {
			throw new IllegalArgumentException("Request can not be null");
		}

		ServiceFilter filter = mClient.getServiceFilter();
		// Set the request's headers
		configureHeadersOnRequest(request);
		filter.handleRequest(request, new NextServiceFilterCallback() {

			@Override
			public void onNext(ServiceFilterRequest request,
					ServiceFilterResponseCallback responseCallback) {

				ServiceFilterResponse response = null;

				try {
					response = request.execute();
					int statusCode = response.getStatus().getStatusCode();

					// If the response has error throw exception
					if (statusCode < 200 || statusCode >= 300) {
						String responseContent = response.getContent();
						if (responseContent != null
								&& responseContent.trim() != "") {
							throw new MobileServiceException(responseContent);
						} else {
							throw new MobileServiceException(String.format(
									"{'code': %d}", statusCode));
						}
					}

				} catch (Exception e) {
					// Something went wrong, call onResponse with exception
					// method
					if (responseCallback != null) {
						responseCallback.onResponse(response,
								new MobileServiceException(
										"Error while processing request.", e));
						return;
					}
				}

				// Call onResponse method
				if (responseCallback != null) {
					responseCallback.onResponse(response, null);
				}
			}
		}, responseCallback);
	}

	/**
	 * Configures the HttpRequestBase to execute a request with a Mobile Service
	 * 
	 * @param request
	 *            The request to configure
	 */
	private void configureHeadersOnRequest(ServiceFilterRequest request) {
		// Add the authentication header if the user is logged in
		MobileServiceUser user = mClient.getCurrentUser();
		if (user != null && user.getAuthenticationToken() != "") {
			request.addHeader(X_ZUMO_AUTH_HEADER, user.getAuthenticationToken());
		}

		// Set the User Agent header
		request.addHeader(HTTP.USER_AGENT, this.getUserAgent());

		// Set the special Application key header
		request.addHeader(X_ZUMO_APPLICATION_HEADER, mClient.getAppKey());

		// Set the special Installation ID header
		request.addHeader(
				X_ZUMO_INSTALLATION_ID_HEADER,
				MobileServiceApplication.getInstallationId(mClient.getContext()));

		// Set the content type header
		request.addHeader(HTTP.CONTENT_TYPE, JSON_CONTENTTYPE);

		request.addHeader("Accept", JSON_CONTENTTYPE);
	}

	/**
	 * Generates the User-Agent
	 */
	private String getUserAgent() {
		String userAgent = String.format(
				"ZUMO/%s (lang=%s; os=%s; os_version=%s; arch=%s)",
				SDK_VERSION, "Java", "Android", Build.VERSION.RELEASE,
				Build.CPU_ABI);

		return userAgent;
	}
}
