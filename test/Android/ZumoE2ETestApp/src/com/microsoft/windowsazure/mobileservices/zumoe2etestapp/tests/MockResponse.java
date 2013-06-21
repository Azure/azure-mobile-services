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
package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests;

import java.io.UnsupportedEncodingException;

import org.apache.http.Header;
import org.apache.http.ProtocolVersion;
import org.apache.http.StatusLine;

import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponse;

public class MockResponse implements ServiceFilterResponse {

	private int mStatus;
	private byte[] mContent;

	public MockResponse(String content, int status) {
		mStatus = status;
		mContent = content.getBytes();
	}
	
	public MockResponse(byte[] content, int status) {
		mStatus = status;
		mContent = content;
	}

	@Override
	public Header[] getHeaders() {
		return null;
	}

	@Override
	public String getContent() {
		if (mContent != null) {
			String content = null;
			try {
				content = new String(mContent, MobileServiceClient.UTF8_ENCODING);
			} catch (UnsupportedEncodingException e) {
			}
			return content;
		} else {
			return null;
		}
	}

	@Override
	public StatusLine getStatus() {
		return new StatusLine() {

			@Override
			public int getStatusCode() {
				return mStatus;
			}

			@Override
			public String getReasonPhrase() {
				return null;
			}

			@Override
			public ProtocolVersion getProtocolVersion() {
				return new ProtocolVersion("HTTP", 1, 1);
			}
		};
	}

	@Override
	public byte[] getRawContent() {
		return mContent;
	}

}
