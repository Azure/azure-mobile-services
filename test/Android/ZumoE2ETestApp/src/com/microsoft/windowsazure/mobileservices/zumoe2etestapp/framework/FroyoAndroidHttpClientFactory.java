package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework;

import com.microsoft.windowsazure.mobileservices.http.AndroidHttpClientFactoryImpl;

import android.net.http.AndroidHttpClient;

/**
 * AndroidHttpClientFactory with Froyo support
 */
public class FroyoAndroidHttpClientFactory extends AndroidHttpClientFactoryImpl {
	
	@Override
	public AndroidHttpClient createAndroidHttpClient() {
		AndroidHttpClient client = super.createAndroidHttpClient();
		
		FroyoSupport.fixAndroidHttpClientForCertificateValidation(client);
		
		return client;
	}
}
