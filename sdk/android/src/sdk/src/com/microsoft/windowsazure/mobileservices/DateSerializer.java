package com.microsoft.windowsazure.mobileservices;

import java.lang.reflect.Type;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;
import java.util.TimeZone;

import android.annotation.SuppressLint;
import com.google.gson.JsonDeserializationContext;
import com.google.gson.JsonDeserializer;
import com.google.gson.JsonElement;
import com.google.gson.JsonParseException;
import com.google.gson.JsonPrimitive;
import com.google.gson.JsonSerializationContext;
import com.google.gson.JsonSerializer;

@SuppressLint("SimpleDateFormat")
class DateSerializer implements JsonSerializer<Date>, JsonDeserializer<Date> {

	@Override
	public Date deserialize(JsonElement element, Type type,
			JsonDeserializationContext ctx) throws JsonParseException {
		String strVal = element.getAsString();

		try {
			return deserialize(strVal);
		} catch (ParseException e) {
			throw new JsonParseException(e);
		}

	}

	@Override
	public JsonElement serialize(Date date, Type type,
			JsonSerializationContext ctx) {
		JsonElement element = new JsonPrimitive(serialize(date));
		return element;
	}

	public static Date deserialize(String strVal) throws ParseException {
		// Change Z to +00:00 to adapt the string to a format
		// that can be parsed in Java
		String s = strVal.replace("Z", "+00:00");
		try {
			// Remove the ":" character to adapt the string to a
			// format
			// that can be parsed in Java
			s = s.substring(0, 26) + s.substring(27);
		} catch (IndexOutOfBoundsException e) {
			throw new JsonParseException("Invalid length");
		}

		// Parse the well-formatted date string
		SimpleDateFormat dateFormat = new SimpleDateFormat(
				"yyyy-MM-dd'T'HH:mm:ss'.'SSSZ");
		dateFormat.setTimeZone(TimeZone.getDefault());
		Date date = dateFormat.parse(s);

		return date;
	}

	public static String serialize(Date date) {
		SimpleDateFormat dateFormat = new SimpleDateFormat(
				"yyyy-MM-dd'T'HH:mm:ss'.'SSS'Z'", Locale.getDefault());
		dateFormat.setTimeZone(TimeZone.getTimeZone("UTC"));

		String formatted = dateFormat.format(date);

		return formatted;
	}

}
