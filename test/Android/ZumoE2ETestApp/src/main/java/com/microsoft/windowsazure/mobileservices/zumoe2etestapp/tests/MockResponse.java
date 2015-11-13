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
package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests;

import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.squareup.okhttp.Headers;
import com.squareup.okhttp.Protocol;
import com.squareup.okhttp.internal.http.StatusLine;

import java.io.UnsupportedEncodingException;

public class MockResponse implements ServiceFilterResponse {

    private StatusLine mStatus;
    private byte[] mContent;

    public MockResponse(String content, int statusCode) {
        mStatus = new StatusLine(Protocol.HTTP_2, statusCode, "");
        mContent = content.getBytes();
    }

    public MockResponse(byte[] content, int statusCode) {
        mStatus = new StatusLine(Protocol.HTTP_2, statusCode, "");
        mContent = content;
    }

    @Override
    public Headers getHeaders() {
        return null;
    }

    @Override
    public String getContent() {
        if (mContent != null) {
            String content = null;
            try {
                content = new String(mContent, MobileServiceClient.UTF8_ENCODING);
            } catch (UnsupportedEncodingException e) {
            }
            return content;
        } else {
            return null;
        }
    }

    @Override
    public StatusLine getStatus() {
        return mStatus;
    }

    @Override
    public byte[] getRawContent() {
        return mContent;
    }

}
