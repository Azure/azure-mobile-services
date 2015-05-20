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

import com.google.common.util.concurrent.FutureCallback;
import com.google.common.util.concurrent.Futures;
import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.SettableFuture;
import com.microsoft.windowsazure.mobileservices.http.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;

public class LogServiceFilter implements ServiceFilter {

    @Override
    public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

        final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

        String content = request.getContent();
        if (content == null)
            content = "NULL";

        String url = request.getUrl();
        if (url == null)
            url = "";

        Log.d("REQUEST URL", url);
        Log.d("REQUEST CONTENT", content);

        ListenableFuture<ServiceFilterResponse> nextServiceFilterCallbackFuture = nextServiceFilterCallback.onNext(request);

        Futures.addCallback(nextServiceFilterCallbackFuture, new FutureCallback<ServiceFilterResponse>() {

            @Override
            public void onFailure(Throwable exception) {
                resultFuture.setException(exception);

            }

            @Override
            public void onSuccess(ServiceFilterResponse response) {
                if (response != null) {
                    String content = response.getContent();
                    if (content != null) {
                        Log.d("RESPONSE CONTENT", content);
                    }
                }

                resultFuture.set(response);
            }
        });

        return resultFuture;
    }
}
