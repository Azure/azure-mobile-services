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

import java.net.MalformedURLException;
import java.util.ArrayList;
import java.util.EnumSet;
import java.util.Hashtable;
import java.util.List;
import java.util.concurrent.ExecutionException;

import org.apache.http.Header;
import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.SettableFuture;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.MobileServiceFeatures;
import com.microsoft.windowsazure.mobileservices.http.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;

import android.test.InstrumentationTestCase;
import android.util.Pair;

public class MobileServiceFeaturesTest extends InstrumentationTestCase {
	String appUrl;
	String appKey;

	protected void setUp() throws Exception {
		appUrl = "http://myapp.com/";
		appKey = "qwerty";
		super.setUp();
	}

	protected void tearDown() throws Exception {
		super.tearDown();
	}

	public void testFeaturesToStringConversion() {
		Hashtable<EnumSet<MobileServiceFeatures>, String> cases;
		cases = new Hashtable<EnumSet<MobileServiceFeatures>, String>();
		for (MobileServiceFeatures feature : MobileServiceFeatures.class.getEnumConstants()) {
			cases.put(EnumSet.of(feature), feature.getValue());
		}
		cases.put(EnumSet.of(MobileServiceFeatures.TypedTable, MobileServiceFeatures.AdditionalQueryParameters), "QS,TT");
		cases.put(EnumSet.of(MobileServiceFeatures.UntypedTable, MobileServiceFeatures.AdditionalQueryParameters), "QS,TU");
		cases.put(EnumSet.of(MobileServiceFeatures.TypedTable, MobileServiceFeatures.Offline), "OL,TT");
		cases.put(EnumSet.of(MobileServiceFeatures.UntypedTable, MobileServiceFeatures.Offline), "OL,TU");
		cases.put(EnumSet.of(MobileServiceFeatures.TypedApiCall, MobileServiceFeatures.AdditionalQueryParameters), "AT,QS");
		cases.put(EnumSet.of(MobileServiceFeatures.JsonApiCall, MobileServiceFeatures.AdditionalQueryParameters), "AJ,QS");
		
		for (EnumSet<MobileServiceFeatures> features : cases.keySet()) {
			String expected = cases.get(features);
			String actual = MobileServiceFeatures.featuresToString(features);
			assertEquals(expected, actual);
		}
	}
}
