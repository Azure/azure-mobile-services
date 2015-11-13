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
 * ServiceFilterRequest.java
 */
package com.microsoft.windowsazure.mobileservices.http;

import com.squareup.okhttp.Headers;

import java.net.URISyntaxException;

/**
 * Represents an HTTP request that can be manipulated by ServiceFilters
 */
public interface ServiceFilterRequest {
    /**
     * Returns request's Headers
     */
    public Headers getHeaders();

    /**
     * Adds a new Header to the request
     *
     * @param name Header Name
     * @param val  Header Value
     */
    public void addHeader(String name, String val);

    /**
     * Remove Header from the request
     *
     * @param name Header Name
     */
    public void removeHeader(String name);

    /**
     * Gets the request's content
     */
    public String getContent();

    /**
     * Gets the request's content
     */
    public byte[] getRawContent();

    /**
     * Gets request's URL
     */
    public String getUrl();

    /**
     * Sets the request's URL
     *
     * @param url
     * @throws java.net.URISyntaxException
     */
    public void setUrl(String url) throws URISyntaxException;

    /**
     * Gets the request's method
     */
    public String getMethod();

    /**
     * Executes the request
     *
     * @return ServiceFilterResponse with the request's response
     * @throws Exception
     */
    public ServiceFilterResponse execute() throws Exception;
}