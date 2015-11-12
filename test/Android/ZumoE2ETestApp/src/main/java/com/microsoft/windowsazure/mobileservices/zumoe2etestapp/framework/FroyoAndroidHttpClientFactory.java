/*package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework;

import com.microsoft.windowsazure.mobileservices.http.OkHttpClientFactoryImpl;
import com.squareup.okhttp.OkHttpClient;


public class FroyoAndroidHttpClientFactory extends OkHttpClientFactoryImpl {

    @Override
    public OkHttpClient createOkHttpClient() {
        OkHttpClient client = super.createOkHttpClient();

        FroyoSupport.fixAndroidHttpClientForCertificateValidation(client);

        return client;
    }
}
*/