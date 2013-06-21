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
package com.microsoft.windowsazure.mobileservices.sdk.testapp.test;

import java.io.UnsupportedEncodingException;

import org.apache.http.Header;
import org.apache.http.StatusLine;

import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponse;

public class ServiceFilterResponseMock implements ServiceFilterResponse {
	private Header[] headers;
	private byte[] content;
	private StatusLine status;

	public ServiceFilterResponseMock() {
		 try {
			content = "{}".getBytes(MobileServiceClient.UTF8_ENCODING);
		} catch (UnsupportedEncodingException e) {
			//this should never happen
		}
	}

	@Override
	public Header[] getHeaders() {
		return this.headers;
	}

	@Override
	public String getContent() {
		if (this.content != null) {
			String content = null;
			try {
				content = new String(this.content, MobileServiceClient.UTF8_ENCODING);
			} catch (UnsupportedEncodingException e) {
			}
			return content;
		} else {
			return null;
		}
	}

	@Override
	public StatusLine getStatus() {
		return this.status;
	}

	public void setHeaders(Header[] headers) {
		this.headers = headers;
	}

	public void setContent(String content) {
		if (content != null) {
			this.content = content.getBytes();
		} else {
			this.content = null;
		}
	}
	
	public void setContent(byte[] content) {
		this.content = content;
	}

	public void setStatus(StatusLine status) {
		this.status = status;
	}

	@Override
	public byte[] getRawContent() {
		return content;
	}

}
