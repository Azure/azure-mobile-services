package com.microsoft.windowsazure.mobileservices.sdk.testapp.test;

import org.apache.http.Header;
import org.apache.http.StatusLine;

import com.microsoft.windowsazure.mobileservices.ServiceFilterResponse;

public class ServiceFilterResponseMock implements ServiceFilterResponse {
	private Header[] headers;
	private String content = "{}";
	private StatusLine status;

	public ServiceFilterResponseMock() {
	}

	@Override
	public Header[] getHeaders() {
		return this.headers;
	}

	@Override
	public String getContent() {
		return this.content;
	}

	@Override
	public StatusLine getStatus() {
		return this.status;
	}

	public void setHeaders(Header[] headers) {
		this.headers = headers;
	}

	public void setContent(String content) {
		this.content = content;
	}

	public void setStatus(StatusLine status) {
		this.status = status;
	}

}
