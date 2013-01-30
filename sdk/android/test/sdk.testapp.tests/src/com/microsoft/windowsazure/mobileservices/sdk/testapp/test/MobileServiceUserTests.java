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
