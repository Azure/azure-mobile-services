/*
 * ServiceFilterRequestImpl.java
 */

package com.microsoft.windowsazure.mobileservices;

import java.io.UnsupportedEncodingException;
import java.net.URI;
import java.net.URISyntaxException;

import org.apache.http.Header;
import org.apache.http.HttpResponse;
import org.apache.http.client.methods.HttpEntityEnclosingRequestBase;
import org.apache.http.client.methods.HttpRequestBase;
import org.apache.http.entity.StringEntity;
import org.apache.http.impl.client.DefaultHttpClient;

/**
 * 
 * ServiceFilterRequest implementation
 * 
 */
class ServiceFilterRequestImpl implements ServiceFilterRequest {
	
	private static DefaultHttpClient mHttpClient;
	
	/**
	 * The request to execute
	 */
	private HttpRequestBase mRequest;

	/**
	 * The request content
	 */
	private String mContent;

	static {
		mHttpClient = new DefaultHttpClient();
	}
	
	/**
	 * Constructor
	 * 
	 * @param request
	 *            The request to use
	 */
	public ServiceFilterRequestImpl(HttpRequestBase request) {
		mRequest = request;
	}

	@Override
	public ServiceFilterResponse execute() throws Exception {
		// Execute request
		final HttpResponse response = mHttpClient.execute(mRequest);

		return new ServiceFilterResponseImpl(response);
	}

	@Override
	public Header[] getHeaders() {
		return mRequest.getAllHeaders();
	}

	@Override
	public void addHeader(String name, String val) {
		mRequest.addHeader(name, val);
	}

	@Override
	public void removeHeader(String name) {
		mRequest.removeHeaders(name);
	}

	@Override
	public void setContent(String content) throws UnsupportedEncodingException {
		((HttpEntityEnclosingRequestBase) mRequest).setEntity(new StringEntity(
				content, "utf-8"));
		mContent = content;
	}

	@Override
	public String getContent() {
		return mContent;
	}

	@Override
	public String getUrl() {
		return mRequest.getURI().toString();
	}

	@Override
	public void setUrl(String url) throws URISyntaxException {
		mRequest.setURI(new URI(url));

	}

	@Override
	public String getMethod() {
		return mRequest.getMethod();
	}
}
