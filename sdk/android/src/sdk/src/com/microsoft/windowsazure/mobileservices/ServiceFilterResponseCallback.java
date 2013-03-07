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
 * ServiceFilterResponseCallback.java
 */

package com.microsoft.windowsazure.mobileservices;

/**
 * Callback to invoke after processing the ServiceFilters and executing the
 * request
 */
public interface ServiceFilterResponseCallback {
	/**
	 * Method to execute when the response is ready to be processed
	 * 
	 * @param response
	 *            The response to process
	 * @param exception
	 *            An exception representing the error, in case there was one
	 */
	public void onResponse(ServiceFilterResponse response, Exception exception);
}
