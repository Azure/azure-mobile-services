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
 * RequestAsyncTask.java
 */
package com.microsoft.windowsazure.mobileservices.http;

import android.annotation.SuppressLint;
import android.os.AsyncTask;
import android.os.Build;

import com.microsoft.windowsazure.mobileservices.MobileServiceException;

/**
 * Default implementation for performing requests using AsyncTask
 */
public abstract class RequestAsyncTask extends AsyncTask<Void, Void, ServiceFilterResponse> {
    /**
     * Error message
     */
    protected MobileServiceException mTaskException = null;

    /**
     * Connection to use for the request
     */
    private MobileServiceConnection mConnection;

    /**
     * Request to execute
     */
    private ServiceFilterRequest mRequest;

    /**
     * Constructor that specifies request and connection
     *
     * @param request    Request to use
     * @param connection Connection to use
     */
    public RequestAsyncTask(ServiceFilterRequest request, MobileServiceConnection connection) {
        mRequest = request;
        mConnection = connection;
    }

    @SuppressLint("NewApi")
    public void executeTask() {
        // If it's running with Honeycomb or greater, it must execute each
        // request in a different thread
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.HONEYCOMB) {
            this.executeOnExecutor(AsyncTask.THREAD_POOL_EXECUTOR);
        } else {
            this.execute();
        }
    }

    @Override
    protected ServiceFilterResponse doInBackground(Void... params) {
        // Call start method that executes the request
        ServiceFilterResponse response = null;
        try {
            response = mConnection.start(mRequest).get();
            return response;
        } catch (Exception e) {
            if (e.getCause() instanceof MobileServiceException) {
                mTaskException = (MobileServiceException) e.getCause();
            } else {
                mTaskException = new MobileServiceException(e);
            }
        }

        return response;
    }
}