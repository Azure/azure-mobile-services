/*
 * RequestAsyncTask.java
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
			public void onResponse(ServiceFilterResponse response) {
				mTaskResponse = response;
			}

			@Override
			public void onError(Exception exception,
					ServiceFilterResponse response) {
				mTaskResponse = response;
				mTaskException = exception;
			}
		});

		return mTaskResponse;
	}
}
