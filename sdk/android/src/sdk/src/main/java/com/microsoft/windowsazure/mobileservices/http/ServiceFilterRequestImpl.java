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
 * ServiceFilterRequestImpl.java
 */
package com.microsoft.windowsazure.mobileservices.http;

import android.util.Pair;

import com.microsoft.windowsazure.mobileservices.MobileServiceClient;

import com.squareup.okhttp.Headers;
import com.squareup.okhttp.MediaType;
import com.squareup.okhttp.OkHttpClient;
import com.squareup.okhttp.Request;
import com.squareup.okhttp.RequestBody;
import com.squareup.okhttp.Response;

import java.io.UnsupportedEncodingException;
import java.net.URISyntaxException;
import java.util.List;

/**
 * ServiceFilterRequest implementation
 */
public class ServiceFilterRequestImpl implements ServiceFilterRequest {

    /**
     * The request to execute
     */
    private Request mRequest;

    /**
     * The request content
     */
    private byte[] mContent;

    private OkHttpClientFactory mOkHttpClientFactory;

    private static final MediaType JSON
            = MediaType.parse(MobileServiceConnection.JSON_CONTENTTYPE);

    /**
     * @param request
     *            The request to use
     */

    /**
     * Constructor
     *
     * @param request The request to use
     * @param factory The AndroidHttpClientFactory instance used to create
     *                AndroidHttpClient objects
     */
    private ServiceFilterRequestImpl(Request request, OkHttpClientFactory factory) {
        mRequest = request;
        mOkHttpClientFactory = factory;
    }

    private ServiceFilterRequestImpl(Request request, OkHttpClientFactory factory, byte[] content) {
        mRequest = request;
        mOkHttpClientFactory = factory;
        mContent = content;
    }

    public static ServiceFilterRequestImpl post(OkHttpClientFactory factory, String url, byte[] content) {

        if (content == null) {
            content = "".getBytes();
        }

        RequestBody requestBody = RequestBody.create(JSON, content);

        Request request = getBaseRequestBuilder(url)
                .post(requestBody).build();

        return new ServiceFilterRequestImpl(request, factory, content);
    }

    public static ServiceFilterRequestImpl put(OkHttpClientFactory factory, String url, byte[] content) {

        if (content == null) {
            content = "".getBytes();
        }

        RequestBody requestBody = RequestBody.create(JSON, content);

        Request request = getBaseRequestBuilder(url)
                .put(requestBody).build();

        return new ServiceFilterRequestImpl(request, factory, content);
    }

    public static ServiceFilterRequestImpl patch(OkHttpClientFactory factory, String url, byte[] content) {

        if (content == null) {
            content = "".getBytes();
        }

        RequestBody requestBody = RequestBody.create(JSON, content);

        Request request = getBaseRequestBuilder(url)
                .patch(requestBody).build();

        return new ServiceFilterRequestImpl(request, factory, content);
    }

    public static ServiceFilterRequestImpl get(OkHttpClientFactory factory, String url) {

        Request request = getBaseRequestBuilder(url).get().build();

        return new ServiceFilterRequestImpl(request, factory);
    }

    public static ServiceFilterRequestImpl delete(OkHttpClientFactory factory, String url) {
        return delete(factory, url, (byte[]) null);
    }

    public static ServiceFilterRequestImpl delete(OkHttpClientFactory factory, String url, byte[] content) {

        Request.Builder requestBuilder = getBaseRequestBuilder(url);

        if (content != null) {
            RequestBody requestBody = RequestBody.create(JSON, content);
            requestBuilder = requestBuilder.delete(requestBody);
        } else {
            requestBuilder = requestBuilder.delete();
        }

        return new ServiceFilterRequestImpl(requestBuilder.build(), factory, content);
    }

    private static Request.Builder getBaseRequestBuilder(String url) {

        Request.Builder builder = new Request.Builder()
                .url(url);

        return builder;
    }

    @Override
    public ServiceFilterResponse execute() throws Exception {
        // Execute request
        OkHttpClient client = mOkHttpClientFactory.createOkHttpClient();

        final Response response = client.newCall(mRequest).execute();

        ServiceFilterResponse serviceFilterResponse = new ServiceFilterResponseImpl(response);
        return serviceFilterResponse;
    }
    
    @Override
    public Headers getHeaders() {
        return mRequest.headers();
    }

    @Override
    public void addHeader(String name, String val) {
        mRequest = mRequest.newBuilder().addHeader(name, val).build();
    }

    @Override
    public void removeHeader(String name) {
        mRequest = mRequest.newBuilder().removeHeader(name).build();
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
    public byte[] getRawContent() {
        return mContent;
    }

    @Override
    public String getUrl() {
        return mRequest.httpUrl().toString();
    }

    @Override
    public void setUrl(String url) throws URISyntaxException {
        mRequest = mRequest.newBuilder().url(url).build();

    }

    @Override
    public String getMethod() {
        return mRequest.method();
    }
}