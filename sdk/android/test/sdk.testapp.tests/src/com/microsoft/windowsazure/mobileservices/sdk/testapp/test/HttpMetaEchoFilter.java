package com.microsoft.windowsazure.mobileservices.sdk.testapp.test;

import org.apache.http.Header;
import android.net.Uri;
import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponseCallback;

public class HttpMetaEchoFilter implements ServiceFilter {

	@Override
	public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback, ServiceFilterResponseCallback responseCallback) {

		JsonObject jResponse = new JsonObject();
		
		jResponse.addProperty("method", request.getMethod());
		
		Header[] headers = request.getHeaders();
		if (headers != null && headers.length > 0) {
			JsonObject jHeaders = new JsonObject();
			
			for (Header header : headers) {
				jHeaders.addProperty(header.getName(), header.getValue());
			}
			
			jResponse.add("headers", jHeaders);
		}
		
		Uri uri = Uri.parse(request.getUrl());
		String query = uri.getQuery();
		
		if (query != null && query.trim() != "") {
			JsonObject jParameters = new JsonObject();
			
			for (String parameter : query.split("&")) {
				jParameters.addProperty(parameter.split("=")[0], parameter.split("=")[1]);
			}
			jResponse.add("parameters", jParameters);
		}		
		
		ServiceFilterResponseMock response = new ServiceFilterResponseMock();
		response.setContent(jResponse.toString());
		response.setStatus(new StatusLineMock(200));

		responseCallback.onResponse(response, null);	
	}

}
