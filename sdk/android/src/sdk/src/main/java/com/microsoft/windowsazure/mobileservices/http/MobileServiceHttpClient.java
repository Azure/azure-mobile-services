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
 * MobileServiceHttpClient.java
 */
package com.microsoft.windowsazure.mobileservices.http;

import java.io.UnsupportedEncodingException;
import java.util.ArrayList;
import java.util.EnumSet;
import java.util.List;

import org.apache.http.client.methods.HttpDelete;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.client.methods.HttpPut;

import android.net.Uri;
import android.util.Pair;

import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.SettableFuture;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceFeatures;

/**
 * Utility class which centralizes the HTTP requests sent by the
 * mobile services client.
 */
public class MobileServiceHttpClient {
	
	/**
	 * Request header to indicate the features in this SDK used by the request.
	 */
	public final static String X_ZUMO_FEATURES = "X-ZUMO-FEATURES";

	/**
	 * The client associated with this HTTP caller.
	 */
	MobileServiceClient mClient;

	/**
	 * Constructor
	 *
	 * @param client
	 * 				The client associated with this HTTP caller.
	 */
	public MobileServiceHttpClient(MobileServiceClient client) {
		this.mClient = client;
	}

	/**
	 * Makes a request over HTTP
	 *
	 * @param path
	 *            The path of the request URI
	 * @param content
	 *            The byte array to send as the request body
	 * @param httpMethod
	 *            The HTTP Method used to invoke the API
	 * @param requestHeaders
	 *            The extra headers to send in the request
	 * @param parameters
	 *            The query string parameters sent in the request
	 */
	public ListenableFuture<ServiceFilterResponse> request(String path, byte[] content, String httpMethod,
			List<Pair<String, String>> requestHeaders, List<Pair<String, String>> parameters) {
		return this.request(path, content, httpMethod, requestHeaders, parameters, EnumSet.noneOf(MobileServiceFeatures.class));
	}

	/**
	 * Makes a request over HTTP
	 *
	 * @param path
	 *            The path of the request URI
	 * @param content
	 *            The string to send as the request body
	 * @param httpMethod
	 *            The HTTP Method used to invoke the API
	 * @param requestHeaders
	 *            The extra headers to send in the request
	 * @param parameters
	 *            The query string parameters sent in the request
	 * @param features
	 *            The features used in the request
	 * @throws java.io.UnsupportedEncodingException
	 *            If the content cannot be converted into a byte array.
	 */
	public ListenableFuture<ServiceFilterResponse> request(String path, String content, String httpMethod,
			List<Pair<String, String>> requestHeaders, List<Pair<String, String>> parameters,
			EnumSet<MobileServiceFeatures> features) {
		try {
			byte[] byteContent = null;

            if (content != null) {
                byteContent = content.getBytes(MobileServiceClient.UTF8_ENCODING);
            }

			return this.request(path, byteContent, httpMethod, requestHeaders, parameters, features);
		} catch (UnsupportedEncodingException e) {
			SettableFuture<ServiceFilterResponse> future = SettableFuture.create();
			future.setException(e);
			return future;
		}
	}

	/**
	 * Makes a request over HTTP
	 *
	 * @param path
	 *            The path of the request URI
	 * @param content
	 *            The byte array to send as the request body
	 * @param httpMethod
	 *            The HTTP Method used to invoke the API
	 * @param requestHeaders
	 *            The extra headers to send in the request
	 * @param parameters
	 *            The query string parameters sent in the request
	 * @param features
	 *            The features used in the request
	 */
	public ListenableFuture<ServiceFilterResponse> request(String path, byte[] content, String httpMethod,
			List<Pair<String, String>> requestHeaders, List<Pair<String, String>> parameters,
			EnumSet<MobileServiceFeatures> features) {
		final SettableFuture<ServiceFilterResponse> future = SettableFuture.create();

		if (path == null || path.trim().equals("")) {
			future.setException(new IllegalArgumentException("request path cannot be null"));
			return future;
		}

		if (httpMethod == null || httpMethod.trim().equals("")) {
			future.setException(new IllegalArgumentException("httpMethod cannot be null"));
			return future;
		}

		Uri.Builder uriBuilder = Uri.parse(mClient.getAppUrl().toString()).buildUpon();
		uriBuilder.path(path);

		if (parameters != null && parameters.size() > 0) {
			for (Pair<String, String> parameter : parameters) {
				uriBuilder.appendQueryParameter(parameter.first, parameter.second);
			}
		}

		ServiceFilterRequestImpl request;
		String url = uriBuilder.build().toString();

		if (httpMethod.equalsIgnoreCase(HttpGet.METHOD_NAME)) {
			request = new ServiceFilterRequestImpl(new HttpGet(url), mClient.getAndroidHttpClientFactory());
		} else if (httpMethod.equalsIgnoreCase(HttpPost.METHOD_NAME)) {
			request = new ServiceFilterRequestImpl(new HttpPost(url), mClient.getAndroidHttpClientFactory());
		} else if (httpMethod.equalsIgnoreCase(HttpPut.METHOD_NAME)) {
			request = new ServiceFilterRequestImpl(new HttpPut(url), mClient.getAndroidHttpClientFactory());
		} else if (httpMethod.equalsIgnoreCase(HttpPatch.METHOD_NAME)) {
			request = new ServiceFilterRequestImpl(new HttpPatch(url), mClient.getAndroidHttpClientFactory());
		} else if (httpMethod.equalsIgnoreCase(HttpDelete.METHOD_NAME)) {
			request = new ServiceFilterRequestImpl(new HttpDelete(url), mClient.getAndroidHttpClientFactory());
		} else {
			future.setException(new IllegalArgumentException("httpMethod not supported"));
			return future;
		}

		String featuresHeader = MobileServiceFeatures.featuresToString(features);
		if (featuresHeader != null) {
			if (requestHeaders == null) {
				requestHeaders = new ArrayList<Pair<String, String>>();
			}

			boolean containsFeatures = false;
			for (Pair<String, String> header : requestHeaders) {
				if (header.first.equals(X_ZUMO_FEATURES)) {
					containsFeatures = true;
					break;
				}
			}

			if (!containsFeatures) {
				// Clone header list to prevent changing user's list
				requestHeaders = new ArrayList<Pair<String, String>>(requestHeaders);
				requestHeaders.add(new Pair<String, String>(X_ZUMO_FEATURES, featuresHeader));
			}
		}

		if (requestHeaders != null && requestHeaders.size() > 0) {
			for (Pair<String, String> header : requestHeaders) {
				request.addHeader(header.first, header.second);
			}
		}

		if (content != null) {
			try {
				request.setContent(content);
			} catch (Exception e) {
				future.setException(e);
				return future;
			}
		}

		MobileServiceConnection conn = mClient.createConnection();

		new RequestAsyncTask(request, conn) {
			@Override
			protected void onPostExecute(ServiceFilterResponse response) {
				if (mTaskException != null) {
					future.setException(mTaskException);
				} else {
					future.set(response);
				}
			}
		}.executeTask();

		return future;
	}
}
