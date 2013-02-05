package com.microsoft.windowsazure.mobileservices;

import java.lang.reflect.Type;
import java.security.InvalidParameterException;

import com.google.gson.JsonElement;
import com.google.gson.JsonNull;
import com.google.gson.JsonPrimitive;
import com.google.gson.JsonSerializationContext;
import com.google.gson.JsonSerializer;

class LongSerializer implements JsonSerializer<Long> {

	@Override
	public JsonElement serialize(Long element, Type type,
			JsonSerializationContext ctx) {
		Long maxAllowedValue = 0x0020000000000000L;
		Long minAllowedValue = Long.valueOf(0xFFE0000000000000L);
		if (element != null) {
			if (element > maxAllowedValue || element < minAllowedValue) {
				throw new InvalidParameterException("Long value must be between " + minAllowedValue + " and " + maxAllowedValue);
			} else {
				return new JsonPrimitive(element);
			}
		} else {
			return JsonNull.INSTANCE;
		}

	}

}
