package com.microsoft.windowsazure.mobileservices;

import java.net.URI;

import org.apache.http.client.methods.HttpEntityEnclosingRequestBase;

class HttpPatch extends HttpEntityEnclosingRequestBase {

	public static final String METHOD_NAME = "PATCH";

	public HttpPatch(String url) {
		super();
		this.setURI(URI.create(url));
	}

	@Override
	public String getMethod() {
		return METHOD_NAME;
	}

}
