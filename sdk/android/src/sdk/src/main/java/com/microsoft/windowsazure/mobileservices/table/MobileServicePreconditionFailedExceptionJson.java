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
 * MobileServicePreconditionFailedExceptionJson.java
 */
package com.microsoft.windowsazure.mobileservices.table;

import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;

public class MobileServicePreconditionFailedExceptionJson extends MobileServiceExceptionBase {

    /**
     * UID used for serialization
     */
    private static final long serialVersionUID = 4489352410883725274L;

    /**
     * Initializes a new instance of the
     * MobileServicePreconditionFailedExceptionJson class.
     *
     * @param throwable The inner exception.
     * @param value     The current instance from the server that the precondition
     *                  failed for.
     */
    public MobileServicePreconditionFailedExceptionJson(MobileServiceException msException, JsonObject value) {
        super(msException, value);
    }
}