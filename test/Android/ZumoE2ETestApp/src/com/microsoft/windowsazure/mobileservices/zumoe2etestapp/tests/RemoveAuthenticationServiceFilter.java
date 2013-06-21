package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests;

import com.microsoft.windowsazure.mobileservices.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponseCallback;

public class RemoveAuthenticationServiceFilter implements ServiceFilter {

	@Override
	public void handleRequest(ServiceFilterRequest request,
			NextServiceFilterCallback nextServiceFilterCallback,
			ServiceFilterResponseCallback responseCallback) {
		
		request.removeHeader("X-ZUMO-AUTH");
		request.removeHeader("X-ZUMO-APPLICATION");
		
		nextServiceFilterCallback.onNext(request, responseCallback);
	}

}
