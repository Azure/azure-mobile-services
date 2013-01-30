/*
 * HttpPatch.java
 */

package com.microsoft.windowsazure.mobileservices;

import java.net.URI;

import org.apache.http.annotation.NotThreadSafe;
import org.apache.http.client.methods.HttpEntityEnclosingRequestBase;

/**
 * HTTP PATCH method. This implementation is used to create HTTP PATCH requests
 * because of the lack of out-of-the-box support for PATCH operations in Android
 */
@NotThreadSafe
class HttpPatch extends HttpEntityEnclosingRequestBase {

	/**
	 * The request method
	 */
	public final static String METHOD_NAME = "PATCH";

	/**
	 * Creates a new HttpPatch
	 */
	public HttpPatch() {
		super();
	}

	/**
	 * Creates a new HttpPatch with an URI
	 * 
	 * @param uri
	 */
	public HttpPatch(final URI uri) {
		super();
		setURI(uri);
	}

	/**
	 * Creates a new HttpPatch with an uri
	 * 
	 * @param uri
	 */
	public HttpPatch(final String uri) {
		super();
		setURI(URI.create(uri));
	}

	/**
	 * Returns the HTTP PATCH request method.
	 */
	@Override
	public String getMethod() {
		return METHOD_NAME;
	}
}