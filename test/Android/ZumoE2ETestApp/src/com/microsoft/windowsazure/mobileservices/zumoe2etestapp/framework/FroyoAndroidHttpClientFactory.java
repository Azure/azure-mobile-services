package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework;

import android.net.http.AndroidHttpClient;

import com.microsoft.windowsazure.mobileservices.AndroidHttpClientFactoryImpl;

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
