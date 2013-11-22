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

import java.lang.reflect.Field;
import java.util.ArrayList;
import java.util.List;

import com.google.gson.Gson;
import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.annotations.SerializedName;

class JsonEntityParser {
	/**
	 * Parses the JSON object to a typed list
	 * 
	 * @param results
	 *            JSON results
	 * @param gson
	 *            Gson object used for parsing
	 * @param clazz
	 *            Target entity class
	 * @return List of entities
	 */
	public static <E> List<E> parseResults(JsonElement results, Gson gson, Class<E> clazz) {
		List<E> result = new ArrayList<E>();
		String idPropertyName = getIdPropertyName(clazz);

		// Parse results
		if (results.isJsonArray()) // Query result
		{
			JsonArray elements = results.getAsJsonArray();

			for (JsonElement element : elements) {
				changeIdPropertyName(element.getAsJsonObject(), idPropertyName);
				E typedElement = gson.fromJson(element, clazz);
				result.add(typedElement);
			}
		} else { // Lookup result
			if (results.isJsonObject()) {
				changeIdPropertyName(results.getAsJsonObject(), idPropertyName);
			}
			E typedElement = gson.fromJson(results, clazz);
			result.add(typedElement);
		}
		return result;
	}

	/**
	 * Get's the class' id property name
	 * @param clazz
	 * @return Id Property name
	 */
	@SuppressWarnings("rawtypes")
	private static String getIdPropertyName(Class clazz)
	{
		// Search for annotation called id, regardless case
		for (Field field : clazz.getDeclaredFields()) {

			SerializedName serializedName = field.getAnnotation(SerializedName.class);
			if(serializedName != null && serializedName.value().equalsIgnoreCase("id")) {
				return serializedName.value();
			} else if(field.getName().equalsIgnoreCase("id")) {
				return field.getName();
			}
		}

		// Otherwise, return empty
		return "";
	}

	/**
	 * Changes returned JSon object's id property name to match with type's id property name.
	 * @param element
	 * @param propertyName
	 */
	private static void changeIdPropertyName(JsonObject element, String propertyName)
	{		
		// If the property name is id or if there's no id defined, then return without performing changes
		if (propertyName.equals("id") || propertyName.length() == 0) return;
		
		if (element.has("id")) {
			
			JsonElement idElement = element.get("id");
			
			String value = idElement.isJsonNull() ? null : idElement.getAsString();			
			element.remove("id");
			
			// Create a new id property using the given property name
			element.addProperty(propertyName, value);
		}
	}
}
