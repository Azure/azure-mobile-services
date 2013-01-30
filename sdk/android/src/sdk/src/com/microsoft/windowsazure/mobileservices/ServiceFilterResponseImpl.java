/*
 * ServiceFilterResponseImpl.java
 */

package com.microsoft.windowsazure.mobileservices;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;

import org.apache.http.Header;
import org.apache.http.HttpEntity;
import org.apache.http.HttpResponse;
import org.apache.http.StatusLine;

/**
 * 
 * ServiceFilterResponse implementation
 * 
 */
public class ServiceFilterResponseImpl implements ServiceFilterResponse {
	/**
	 * The original response
	 */
	private HttpResponse mResponse;

	/**
	 * The response content
	 */
	private String mResponseContent;

	/**
	 * Constructor
	 * 
	 * @param response
	 *            The request's response
	 * @throws IOException
	 * @throws IllegalStateException
	 */
	public ServiceFilterResponseImpl(HttpResponse response)
			throws IllegalStateException, IOException {
		mResponse = response;
		mResponseContent = null;

		// Get the response's content
		HttpEntity entity = mResponse.getEntity();
		InputStream instream = entity.getContent();
		BufferedReader reader = new BufferedReader(new InputStreamReader(
				instream));

		StringBuilder sb = new StringBuilder();
		String content = reader.readLine();
		while (content != null) {
			sb.append(content);
			sb.append('\n');
			content = reader.readLine();
		}

		mResponseContent = sb.toString();
	}

	@Override
	public Header[] getHeaders() {
		return mResponse.getAllHeaders();
	}

	@Override
	public String getContent() {
		return mResponseContent;
	}

	@Override
	public StatusLine getStatus() {
		return mResponse.getStatusLine();
	}
}
