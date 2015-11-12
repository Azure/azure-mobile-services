package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework;

import android.net.http.AndroidHttpClient;

import com.microsoft.windowsazure.mobileservices.http.AndroidHttpClientFactoryImpl;
import com.squareup.okhttp.apache.OkApacheClient;

import org.apache.http.client.HttpClient;

/**
 * AndroidHttpClientFactory with Froyo support
 */
public class FroyoAndroidHttpClientFactory extends AndroidHttpClientFactoryImpl {

    @Override
    public OkApacheClient createAndroidHttpClient() {
        OkApacheClient client = super.createAndroidHttpClient();

        FroyoSupport.fixAndroidHttpClientForCertificateValidation(client);

        return client;
    }
}
