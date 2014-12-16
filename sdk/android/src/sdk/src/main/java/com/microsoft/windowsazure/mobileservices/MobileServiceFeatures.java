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
	/**
	 * Feature header value for requests going through typed tables.
	 */
	TypedTable("TT"),

	/**
	 * Feature header value for requests going through untyped (JSON) tables.
	 */
	UntypedTable("TU"),

	/**
	 * Feature header value for API calls using typed (generic) overloads.
	 */
	TypedApiCall("AT"),

	/**
	 * Feature header value for API calls using JSON overloads.
	 */
	JsonApiCall("AJ"),

	/**
	 * Feature header value for API calls using the generic (HTTP) overload.
	 */
	GenericApiCall("AG"),

	/**
	 * Feature header value for table / API requests which include additional query string parameters.
	 */
	AdditionalQueryParameters("QS"),

	/**
	 * Feature header value for offline initiated requests (push / pull).
	 */
	Offline("OL"),

	/**
	 * Feature header value for conditional updates.
	 */
	OpportunisticConcurrency("OC");

	private String value;
	private final static MobileServiceFeatures[] AllFeatures;

	static {
		AllFeatures = MobileServiceFeatures.class.getEnumConstants();
	}

	/**
	 * Constructor
	 *
	 * @param value the code associated with the feature which will
	 * be sent to the server in the features header
	 */
	MobileServiceFeatures(String value) {
		this.value = value;
	}

	/**
	 * Gets the code will be sent to the server for this feature
	 * in the features header
	 *
	 * @return the code associated with this feature.
	 */
	public String getValue() {
		return value;
	}

	/**
	 * Returns a comma-separated list of feature codes which can be sent to
	 * the service in the features header.
	 *
	 * @param features a set of features
	 * @return a comma-separated list of the feature codes from the given set
	 */
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
