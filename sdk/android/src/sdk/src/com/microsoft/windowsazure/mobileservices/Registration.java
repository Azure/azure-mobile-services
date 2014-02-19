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
/*
 * Registration.java
 */

package com.microsoft.windowsazure.mobileservices;

import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Locale;
import java.util.TimeZone;

import org.json.JSONException;
import org.json.JSONObject;
import org.w3c.dom.Element;
import org.w3c.dom.NodeList;

import com.google.gson.annotations.Expose;
import com.google.gson.annotations.SerializedName;

public abstract class Registration {

	/**
	 * Name for default registration
	 */
	static final String DEFAULT_REGISTRATION_NAME = "$Default";

	/**
	 * RegistrationId property for regid json object
	 */
	static final String REGISTRATIONID_JSON_PROPERTY = "registrationid";

	/**
	 * RegistrationName property for regid json object
	 */
	static final String REGISTRATION_NAME_JSON_PROPERTY = "registrationName";

	/**
	 * The Registration Id
	 */
	@Expose
	@SerializedName("registrationId")
	protected String mRegistrationId;

	/**
	 * The expiration time
	 */
	@Expose
	@SerializedName("expirationTime")
	protected String mExpirationTime;

	/**
	 * The registration tags
	 */
	@Expose
	@SerializedName("tags")
	protected List<String> mTags;

	/**
	 * Get the node value
	 * 
	 * @param element
	 *            The element to read
	 * @param node
	 *            The node name to retrieve
	 * @return
	 */
	protected static String getNodeValue(Element element, String node) {
		NodeList nodes = element.getElementsByTagName(node);
		if (nodes.getLength() > 0) {
			return nodes.item(0).getTextContent();
		} else {
			return null;
		}
	}

	/**
	 * Creates a new registration
	 */
	Registration() {
		mTags = new ArrayList<String>();
	}

	/**
	 * Gets the registration ID
	 */
	public String getRegistrationId() {
		return mRegistrationId;
	}

	/**
	 * Sets the registration ID
	 */
	void setRegistrationId(String registrationId) {
		mRegistrationId = registrationId;
	}

	/**
	 * Gets the registration name
	 */
	protected abstract String getName();

	/**
	 * Sets the registration name
	 */
	protected abstract void setName(String name);

	/**
	 * Gets the registration tags
	 */
	public List<String> getTags() {
		return new ArrayList<String>(mTags);
	}

	/**
	 * Gets the registration URI
	 */
	public String getURI() {
		return "/registrations/" + mRegistrationId;
	}

	/**
	 * Parses an UTC date string into a Date object
	 * 
	 * @param dateString
	 *            The date string to parse
	 * @return The Date object
	 * @throws ParseException
	 */
	private static Date UTCDateStringToDate(String dateString) throws ParseException {
		// Change Z to +00:00 to adapt the string to a format
		// that can be parsed in Java
		String s = dateString.replace("Z", "+00:00");
		try {
			// Remove the ":" character to adapt the string to a
			// format that can be parsed in Java
			s = s.substring(0, 26) + s.substring(27);
		} catch (IndexOutOfBoundsException e) {
			throw new ParseException("The 'updated' value has an invalid format", 26);
		}

		// Parse the well-formatted date string
		SimpleDateFormat dateFormat = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss'.'SSSZ", Locale.getDefault());
		dateFormat.setTimeZone(TimeZone.getDefault());
		Date date = dateFormat.parse(s);

		return date;

	}

	/**
	 * Gets the PNS specific identifier
	 */
	public abstract String getPNSHandle();

	/**
	 * Sets the PNS specific identifier
	 */
	protected abstract void setPNSHandle(String pNSHandle);

	/**
	 * Gets the expiration time
	 * 
	 * @throws ParseException
	 */
	public Date getExpirationTime() throws ParseException {
		return UTCDateStringToDate(mExpirationTime);
	}

	/**
	 * Gets the expiration time string
	 */
	String getExpirationTimeString() {
		return mExpirationTime;
	}

	/**
	 * Sets the expiration time string
	 */
	void setExpirationTimeString(String expirationTimeString) {
		mExpirationTime = expirationTimeString;
	}

	/**
	 * Adds the tags in the array to the registration
	 */
	void addTags(String[] tags) {
		if (tags != null) {
			for (String tag : tags) {
				if (!isNullOrWhiteSpace(tag)) {
					mTags.add(tag);
				}
			}
		}
	}

	/**
	 * Gets the registration information JSON object
	 * 
	 * @throws JSONException
	 */
	JSONObject getRegistrationInformation() throws JSONException {
		JSONObject regInfo = new JSONObject();
		regInfo.put(REGISTRATIONID_JSON_PROPERTY, getRegistrationId());
		regInfo.put(REGISTRATION_NAME_JSON_PROPERTY, getName());

		return regInfo;
	}

	private static boolean isNullOrWhiteSpace(String str) {
		return str == null || str.trim().equals("");
	}
}