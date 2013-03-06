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
package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework;

import android.util.Log;

import com.microsoft.windowsazure.mobileservices.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponseCallback;

public class LogServiceFilter implements ServiceFilter {

	@Override
	public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
			final ServiceFilterResponseCallback responseCallback) {

		String content = request.getContent();
		if (content == null)
			content = "NULL";

		String url = request.getUrl();
		if (url == null)
			url = "";

		Log.d("REQUEST URL", url);
		Log.d("REQUEST CONTENT", content);

		nextServiceFilterCallback.onNext(request, new ServiceFilterResponseCallback() {

			@Override
			public void onResponse(ServiceFilterResponse response, Exception exception) {
				if (exception != null && response != null) {
					String content = response.getContent();
					if (content != null) {
						Log.d("RESPONSE CONTENT", content);
					}
				}
				responseCallback.onResponse(response, exception);
			}
		});
	}

}
