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
package com.microsoft.windowsazure.mobileservices;

import java.lang.reflect.Type;
import java.security.InvalidParameterException;

import com.google.gson.JsonElement;
import com.google.gson.JsonNull;
import com.google.gson.JsonPrimitive;
import com.google.gson.JsonSerializationContext;
import com.google.gson.JsonSerializer;

/**
 * Long Serializer to avoid losing precision when sending data to Mobile
 * Services
 */
class LongSerializer implements JsonSerializer<Long> {

	/**
	 * Serializes a Long instance to a JsonElement, verifying the maximum and
	 * minimum allowed values
	 */
	@Override
	public JsonElement serialize(Long element, Type type,
			JsonSerializationContext ctx) {
		Long maxAllowedValue = 0x0020000000000000L;
		Long minAllowedValue = Long.valueOf(0xFFE0000000000000L);
		if (element != null) {
			if (element > maxAllowedValue || element < minAllowedValue) {
				throw new InvalidParameterException(
						"Long value must be between " + minAllowedValue
								+ " and " + maxAllowedValue);
			} else {
				return new JsonPrimitive(element);
			}
		} else {
			return JsonNull.INSTANCE;
		}

	}

}
