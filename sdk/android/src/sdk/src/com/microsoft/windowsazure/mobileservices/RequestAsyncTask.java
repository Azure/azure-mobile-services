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
/*
 w * RequestAsyncTask.java
 */

package com.microsoft.windowsazure.mobileservices;

import android.os.AsyncTask;

/**
 * Default implementation for performing requests using AsyncTask
 */
abstract class RequestAsyncTask extends
		AsyncTask<Void, Void, ServiceFilterResponse> {
	/**
	 * Error message
	 */
	protected Exception mTaskException = null;

	/**
	 * Task response
	 */
	private ServiceFilterResponse mTaskResponse = null;

	/**
	 * Connection to use for the request
	 */
	private MobileServiceConnection mConnection;

	/**
	 * Request to execute
	 */
	private ServiceFilterRequest mRequest;

	/**
	 * Default constructor
	 */
	public RequestAsyncTask() {

	}

	/**
	 * Constructor that specifies request and connection
	 * 
	 * @param request
	 *            Request to use
	 * @param connection
	 *            Connection to use
	 */
	public RequestAsyncTask(ServiceFilterRequest request,
			MobileServiceConnection connection) {
		mRequest = request;
		mConnection = connection;
	}

	@Override
	protected ServiceFilterResponse doInBackground(Void... params) {
		// Call start method that executes the request
		mConnection.start(mRequest, new ServiceFilterResponseCallback() {

			@Override
			public void onResponse(ServiceFilterResponse response,
					Exception exception) {
				mTaskResponse = response;
				mTaskException = exception;
			}
		});

		return mTaskResponse;
	}
}
