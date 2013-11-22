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

package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework;

import java.io.IOException;
import java.io.InputStream;
import java.net.Socket;
import java.security.KeyManagementException;
import java.security.KeyStore;
import java.security.KeyStoreException;
import java.security.NoSuchAlgorithmException;
import java.security.UnrecoverableKeyException;
import java.security.cert.CertificateException;
import java.security.cert.X509Certificate;
import java.util.ArrayList;
import java.util.Arrays;

import javax.net.ssl.SSLContext;
import javax.net.ssl.TrustManager;
import javax.net.ssl.TrustManagerFactory;
import javax.net.ssl.X509TrustManager;

import org.apache.http.conn.scheme.Scheme;
import org.apache.http.conn.scheme.SchemeRegistry;
import org.apache.http.conn.ssl.SSLSocketFactory;

import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.MainActivity;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.R;

import android.net.http.AndroidHttpClient;

/**
 * Method for Froyo Support for MobileServices
 */
public class FroyoSupport {
	/**
	 * Fixes an AndroidHttpClient instance to accept MobileServices SSL certificate
	 * @param client AndroidHttpClient to fix
	 */
	public static void fixAndroidHttpClientForCertificateValidation(AndroidHttpClient client) {
		
		final SchemeRegistry schemeRegistry = new SchemeRegistry();
		schemeRegistry.register(new Scheme("https",
				createAdditionalCertsSSLSocketFactory(), 443));
		client.getConnectionManager().getSchemeRegistry().unregister("https");
		
		client.getConnectionManager().getSchemeRegistry().register(new Scheme("https",
				createAdditionalCertsSSLSocketFactory(), 443));
	}

	private static SSLSocketFactory createAdditionalCertsSSLSocketFactory() {
		try {
			final KeyStore ks = KeyStore.getInstance("BKS");

			final InputStream in = MainActivity.getInstance().getResources().openRawResource(R.raw.mobileservicestore);
			try {
				ks.load(in, "mobileservices".toCharArray());
			} finally {
				in.close();
			}

			return new AdditionalKeyStoresSSLSocketFactory(ks);

		} catch (Exception e) {
			throw new RuntimeException(e);
		}
	}

	private static class AdditionalKeyStoresSSLSocketFactory extends SSLSocketFactory {
		protected SSLContext sslContext = SSLContext.getInstance("TLS");

		public AdditionalKeyStoresSSLSocketFactory(KeyStore keyStore)
				throws NoSuchAlgorithmException, KeyManagementException,
				KeyStoreException, UnrecoverableKeyException {
			super(null, null, null, null, null, null);
			sslContext.init(null,
					new TrustManager[] { new AdditionalKeyStoresTrustManager(
							keyStore) }, null);
		}

		@Override
		public Socket createSocket(Socket socket, String host, int port,
				boolean autoClose) throws IOException {
			return sslContext.getSocketFactory().createSocket(socket, host,
					port, autoClose);
		}

		@Override
		public Socket createSocket() throws IOException {
			return sslContext.getSocketFactory().createSocket();
		}

		class AdditionalKeyStoresTrustManager implements X509TrustManager {

			protected ArrayList<X509TrustManager> x509TrustManagers = new ArrayList<X509TrustManager>();

			protected AdditionalKeyStoresTrustManager(
					KeyStore... additionalkeyStores) {
				final ArrayList<TrustManagerFactory> factories = new ArrayList<TrustManagerFactory>();

				try {
					final TrustManagerFactory original = TrustManagerFactory
							.getInstance(TrustManagerFactory
									.getDefaultAlgorithm());
					original.init((KeyStore) null);
					factories.add(original);
					for (KeyStore keyStore : additionalkeyStores) {

						final TrustManagerFactory additionalCerts = TrustManagerFactory
								.getInstance(TrustManagerFactory
										.getDefaultAlgorithm());
						additionalCerts.init(keyStore);
						factories.add(additionalCerts);
					}

				} catch (Exception e) {
					throw new RuntimeException(e);
				}

				for (TrustManagerFactory tmf : factories)
					for (TrustManager tm : tmf.getTrustManagers())
						if (tm instanceof X509TrustManager)
							x509TrustManagers.add((X509TrustManager) tm);

				if (x509TrustManagers.size() == 0)
					throw new RuntimeException(
							"Couldn't find any X509TrustManagers");
			}

			public void checkClientTrusted(X509Certificate[] chain,
					String authType) throws CertificateException {
				final X509TrustManager defaultX509TrustManager = x509TrustManagers
						.get(0);
				defaultX509TrustManager.checkClientTrusted(chain, authType);
			}

			public void checkServerTrusted(X509Certificate[] chain,
					String authType) throws CertificateException {
				for (X509TrustManager tm : x509TrustManagers) {
					try {
						tm.checkServerTrusted(chain, authType);
						return;
					} catch (CertificateException e) {
					}
				}
				throw new CertificateException();
			}

			public X509Certificate[] getAcceptedIssuers() {
				final ArrayList<X509Certificate> list = new ArrayList<X509Certificate>();
				for (X509TrustManager tm : x509TrustManagers) {
					list.addAll(Arrays.asList(tm.getAcceptedIssuers()));
				}
				return list.toArray(new X509Certificate[list.size()]);
			}
		}
	}
}
