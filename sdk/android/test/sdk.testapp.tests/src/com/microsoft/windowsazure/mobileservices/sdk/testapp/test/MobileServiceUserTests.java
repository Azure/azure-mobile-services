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
package com.microsoft.windowsazure.mobileservices.sdk.testapp.test;

import android.test.InstrumentationTestCase;

import com.microsoft.windowsazure.mobileservices.MobileServiceUser;

public class MobileServiceUserTests extends InstrumentationTestCase {

	public void testNewMobileServiceUser() {

		MobileServiceUser user = new MobileServiceUser(null);
		assertNull(user.getUserId());

		user = new MobileServiceUser("myUserId");
		assertEquals("myUserId", user.getUserId());

		user.setAuthenticationToken(null);
		assertNull(user.getAuthenticationToken());

		user.setAuthenticationToken("myAuthToken");
		assertEquals("myAuthToken", user.getAuthenticationToken());
	}
}
