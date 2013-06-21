package com.microsoft.windowsazure.mobileservices.sdk.testapp.test;

import com.microsoft.windowsazure.mobileservices.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponseCallback;

public class EchoFilter implements ServiceFilter {

	@Override
	public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback, ServiceFilterResponseCallback responseCallback) {
		ServiceFilterResponseMock response = new ServiceFilterResponseMock();
		response.setContent(request.getRawContent());
		response.setStatus(new StatusLineMock(200));

		responseCallback.onResponse(response, null);	
	}

}
