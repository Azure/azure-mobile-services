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
		// TODO Auto-generated method stub
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
		// TODO Auto-generated method stub

	}

	@Override
	public void setContent(String content) throws InvalidClassException {
		// TODO Auto-generated method stub

	}

	@Override
	public String getContent() {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public String getUrl() {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public void setUrl(String url) throws URISyntaxException {
		// TODO Auto-generated method stub

	}

	@Override
	public String getMethod() {
		// TODO Auto-generated method stub
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

}
