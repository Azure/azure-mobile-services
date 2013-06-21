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

import java.io.InvalidClassException;
import java.net.URISyntaxException;

import org.apache.http.Header;
import org.apache.http.message.BasicHeader;

import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponse;

public class ServiceFilterRequestMock implements ServiceFilterRequest {
	private ServiceFilterResponse responseToUse;
	private Boolean hasErrorOnExecute;

	public ServiceFilterRequestMock(ServiceFilterResponse response) {
		this.responseToUse = response;
		this.hasErrorOnExecute = false;
	}

	@Override
	public Header[] getHeaders() {
		return null;
	}

	@Override
	public void addHeader(String name, String val) {
		Header[] currentHeaders = this.responseToUse.getHeaders();
		int oldSize = currentHeaders == null ? 0 : currentHeaders.length;
		Header[] newHeaders = new Header[oldSize + 1];
		if (oldSize > 0) {
			System.arraycopy(currentHeaders, 0, newHeaders, 0, oldSize);
		}

		newHeaders[oldSize] = new BasicHeader(name, val);

		((ServiceFilterResponseMock) this.responseToUse).setHeaders(newHeaders);
	}

	@Override
	public void removeHeader(String name) {
	}

	@Override
	public void setContent(String content) throws InvalidClassException {
	}

	@Override
	public String getContent() {
		return null;
	}

	@Override
	public String getUrl() {
		return null;
	}

	@Override
	public void setUrl(String url) throws URISyntaxException {
	}

	@Override
	public String getMethod() {
		return null;
	}

	@Override
	public ServiceFilterResponse execute() throws Exception {
		if (this.hasErrorOnExecute) {
			throw new MobileServiceException("Error while processing request");
		}

		return this.responseToUse;
	}

	public Boolean getHasErrorOnExecute() {
		return hasErrorOnExecute;
	}

	public void setHasErrorOnExecute(Boolean hasErrorOnExecute) {
		this.hasErrorOnExecute = hasErrorOnExecute;
	}

	@Override
	public void setContent(byte[] content) throws Exception {
	}

	@Override
	public byte[] getRawContent() {
		return null;
	}

}
