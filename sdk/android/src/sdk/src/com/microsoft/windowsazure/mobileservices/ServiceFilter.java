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
 * ServiceFilter.java
 */

package com.microsoft.windowsazure.mobileservices;

/**
 * The service filter can be used to manipulate requests and responses in the
 * HTTP pipeline used by the MobileServiceClient. ServiceFilters can be
 * associated with a MobileServiceClient via the WithFilter method.
 */
public interface ServiceFilter {
	/**
	 * Method to handle the requests
	 * 
	 * @param request
	 *            Request to execute
	 * @param nextServiceFilterCallback
	 *            The next filter to execute
	 * @param responseCallback
	 *            The callback to invoke once the request is executed
	 */
	public void handleRequest(ServiceFilterRequest request,
			NextServiceFilterCallback nextServiceFilterCallback,
			ServiceFilterResponseCallback responseCallback);
}
