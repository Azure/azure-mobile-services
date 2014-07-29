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

/**
 * MobileServiceFeatures.java
 */
package com.microsoft.windowsazure.mobileservices;

import java.util.ArrayList;
import java.util.Collections;
import java.util.EnumSet;
import java.util.Iterator;

/**
 * The list of mobile services features exposed in the HTTP headers of
 * requests for telemetry purposes.
 */
public enum MobileServiceFeatures {
	TypedTable("TT"), UntypedTable("TU"),
	TypedApiCall("AT"), JsonApiCall("AJ"), GenericApiCall("AG"),
	AdditionalQueryParameters("QS"), Offline("OL");

	private String value;
	private final static MobileServiceFeatures[] AllFeatures;

	static {
		AllFeatures = MobileServiceFeatures.class.getEnumConstants();
	}

	MobileServiceFeatures(String value) {
		this.value = value;
	}

	public String getValue() {
		return value;
	}

	public static String featuresToString(EnumSet<MobileServiceFeatures> features) {
		ArrayList<String> usedFeatures = new ArrayList<String>();
		for (MobileServiceFeatures feature : AllFeatures) {
			if (features.contains(feature)) {
				usedFeatures.add(feature.getValue());
			}
		}
		if (usedFeatures.isEmpty()) {
			return null;
		}

		Collections.sort(usedFeatures);

		StringBuilder sb = new StringBuilder();
		Iterator<String> iter = usedFeatures.iterator();
		while (iter.hasNext()) {
			sb.append(iter.next());
			if (!iter.hasNext()) {
				break;
			}
			sb.append(",");
		}
		return sb.toString();
	}
}
