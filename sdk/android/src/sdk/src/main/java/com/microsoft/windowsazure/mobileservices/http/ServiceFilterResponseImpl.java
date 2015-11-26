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

/**
 * ServiceFilterResponseImpl.java
 */
package com.microsoft.windowsazure.mobileservices.http;

import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.squareup.okhttp.Headers;
import com.squareup.okhttp.Response;
import com.squareup.okhttp.ResponseBody;
import com.squareup.okhttp.internal.http.StatusLine;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.UnsupportedEncodingException;
import java.util.zip.GZIPInputStream;

/**
 * ServiceFilterResponse implementation
 */
public class ServiceFilterResponseImpl implements ServiceFilterResponse {
    /**
     * The original response
     */
    private Response mResponse;

    /**
     * The response content
     */
    private byte[] mResponseContent;

    /**
     * Constructor
     *
     * @param response The request's response
     * @throws java.io.IOException
     * @throws IllegalStateException
     */
    public ServiceFilterResponseImpl(Response response) throws IllegalStateException, IOException {
        mResponse = response;
        mResponseContent = null;

        try {
            // Get the response's content
            ResponseBody entity = mResponse.body();

            if (entity != null) {
                InputStream instream = getUngzippedContent(response);

                ByteArrayOutputStream out = new ByteArrayOutputStream();
                byte[] buffer = new byte[1024];
                int length;

                while ((length = instream.read(buffer)) != -1) {
                    out.write(buffer, 0, length);
                }

                instream.close();

                mResponseContent = out.toByteArray();
            } else {
                mResponseContent = null;
            }
        }finally {
            if (response != null && response.body() != null) {
                response.body().close();
            }
        }
    }

    @Override
    public Headers getHeaders() {
        return mResponse.headers();
    }

    @Override
    public String getContent() {
        if (mResponseContent != null) {
            String responseContent = null;
            try {
                responseContent = new String(mResponseContent, MobileServiceClient.UTF8_ENCODING);
            } catch (UnsupportedEncodingException e) {
            }
            return responseContent;
        } else {
            return null;
        }
    }

    @Override
    public byte[] getRawContent() {
        return mResponseContent;
    }

    @Override
    public StatusLine getStatus() {
        return StatusLine.get(mResponse);
    }

    public static InputStream getUngzippedContent(Response response)  throws IOException {

        InputStream responseStream = response.body().byteStream();

        if (responseStream == null)
            return responseStream;

        String contentEncoding = response.headers().get("content-encoding");

        if (contentEncoding == null) {
            return responseStream;
        }

        if (contentEncoding.contains("gzip")) responseStream
                = new GZIPInputStream(responseStream);

        return responseStream;
    }
}