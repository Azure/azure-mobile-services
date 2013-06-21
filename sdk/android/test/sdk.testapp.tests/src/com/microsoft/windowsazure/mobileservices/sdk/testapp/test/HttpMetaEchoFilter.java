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
