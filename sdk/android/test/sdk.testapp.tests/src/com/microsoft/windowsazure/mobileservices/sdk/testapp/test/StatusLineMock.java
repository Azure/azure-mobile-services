package com.microsoft.windowsazure.mobileservices.sdk.testapp.test;

import org.apache.http.ProtocolVersion;
import org.apache.http.StatusLine;

class StatusLineMock implements StatusLine {
	private int statusCode;

	public StatusLineMock(int statusCode) {
		this.statusCode = statusCode;
	}

	@Override
	public ProtocolVersion getProtocolVersion() {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public String getReasonPhrase() {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public int getStatusCode() {
		return this.statusCode;
	}

}
