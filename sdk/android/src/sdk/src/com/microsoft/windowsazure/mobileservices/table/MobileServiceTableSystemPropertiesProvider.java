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
 * MobileServiceTableSystemPropertiesProvider.java
 */
package com.microsoft.windowsazure.mobileservices.table;

import java.util.EnumSet;
import java.util.List;

import android.util.Pair;

/**
 * Interface used to decouple the table implementation from the query writers.
 */
public interface MobileServiceTableSystemPropertiesProvider {
	/**
	 * Returns the set of enabled System Properties
	 */
	public EnumSet<MobileServiceSystemProperty> getSystemProperties();

	/**
	 * Sets the set of enabled system properties
	 */
	public void setSystemProperties(EnumSet<MobileServiceSystemProperty> systemProperties);

	/**
	 * Adds the tables requested system properties to the parameters collection.
	 * 
	 * @param systemProperties
	 *            The system properties to add.
	 * @param parameters
	 *            The parameters collection.
	 * @return The parameters collection with any requested system properties
	 *         included.
	 */
	List<Pair<String, String>> addSystemProperties(EnumSet<MobileServiceSystemProperty> systemProperties, List<Pair<String, String>> existingProperties);
}
