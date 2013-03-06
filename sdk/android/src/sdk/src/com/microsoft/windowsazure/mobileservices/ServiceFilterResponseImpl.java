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
 * ServiceFilterResponse implementation
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
		if (entity != null) {
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
		} else {
			mResponseContent = null;
		}
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
