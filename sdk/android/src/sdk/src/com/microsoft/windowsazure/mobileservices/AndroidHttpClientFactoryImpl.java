package com.microsoft.windowsazure.mobileservices;

import android.net.http.AndroidHttpClient;

public class AndroidHttpClientFactoryImpl implements AndroidHttpClientFactory {

	@Override
	public AndroidHttpClient createAndroidHttpClient() {
		return AndroidHttpClient.newInstance(MobileServiceConnection.getUserAgent());
	}

}
