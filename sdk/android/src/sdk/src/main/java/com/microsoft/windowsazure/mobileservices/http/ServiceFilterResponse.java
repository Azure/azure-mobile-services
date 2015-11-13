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
 * ServiceFilterResponse.java
 */
package com.microsoft.windowsazure.mobileservices.http;

import com.squareup.okhttp.Headers;

/**
 * Represents an HTTP response that can be manipulated by ServiceFilters
 */
public interface ServiceFilterResponse {
    /**
     * Gets the response's headers.
     *
     * @return The response's headers
     */
    public Headers getHeaders();

    /**
     * Gets the response's content.
     *
     * @return String with the response's content
     * @throws Exception
     */
    public String getContent();

    /**
     * Gets the response's content.
     *
     * @return byte array with the response's content
     * @throws Exception
     */
    public byte[] getRawContent();

    /**
     * Gets the response's status.
     *
     * @return Response's status
     */
    public int getStatus();
}