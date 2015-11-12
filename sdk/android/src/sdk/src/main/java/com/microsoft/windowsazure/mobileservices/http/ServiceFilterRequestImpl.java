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

import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.squareup.okhttp.apache.OkApacheClient;

import org.apache.http.Header;
import org.apache.http.HttpResponse;
import org.apache.http.client.methods.HttpEntityEnclosingRequestBase;
import org.apache.http.client.methods.HttpRequestBase;
import org.apache.http.entity.ByteArrayEntity;
import org.apache.http.entity.StringEntity;

import java.io.UnsupportedEncodingException;
import java.net.URI;
import java.net.URISyntaxException;

/**
 * ServiceFilterRequest implementation
 */
public class ServiceFilterRequestImpl implements ServiceFilterRequest {

    /**
     * The request to execute
     */
    private HttpRequestBase mRequest;

    /**
     * The request content
     */
    private byte[] mContent;

    private AndroidHttpClientFactory mAndroidHttpClientFactory;

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
    public ServiceFilterRequestImpl(HttpRequestBase request, AndroidHttpClientFactory factory) {
        mRequest = request;
        mAndroidHttpClientFactory = factory;
    }

    @Override
    public ServiceFilterResponse execute() throws Exception {
        // Execute request
        OkApacheClient client = mAndroidHttpClientFactory.createAndroidHttpClient();

        final HttpResponse response = client.execute(mRequest);
        ServiceFilterResponse serviceFilterResponse = new ServiceFilterResponseImpl(response);
        return serviceFilterResponse;
    }
    
    @Override
    public Header[] getHeaders() {
        return mRequest.getAllHeaders();
    }

    @Override
    public void addHeader(String name, String val) {
        mRequest.addHeader(name, val);
    }

    @Override
    public void removeHeader(String name) {
        mRequest.removeHeaders(name);
    }

    @Override
    public void setContent(byte[] content) throws Exception {
        ((HttpEntityEnclosingRequestBase) mRequest).setEntity(new ByteArrayEntity(content));
        mContent = content;
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
    public void setContent(String content) throws UnsupportedEncodingException {
        ((HttpEntityEnclosingRequestBase) mRequest).setEntity(new StringEntity(content, MobileServiceClient.UTF8_ENCODING));
        mContent = content.getBytes(MobileServiceClient.UTF8_ENCODING);
    }

    @Override
    public byte[] getRawContent() {
        return mContent;
    }

    @Override
    public String getUrl() {
        return mRequest.getURI().toString();
    }

    @Override
    public void setUrl(String url) throws URISyntaxException {
        mRequest.setURI(new URI(url));

    }

    @Override
    public String getMethod() {
        return mRequest.getMethod();
    }
}