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
 * PnsSpecificRegistrationFactory.java
 */

package com.microsoft.windowsazure.mobileservices;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import com.google.gson.JsonObject;

/**
 * Represents a factory which creates Registrations according the PNS supported
 * on device, and also provides some PNS specific utility methods
 */
final class PnsSpecificRegistrationFactory {

	/**
	 * Creates a new instance of PnsSpecificRegistrationFactory
	 */
	PnsSpecificRegistrationFactory() {
		
	}

	/**
	 * Creates native registration according the PNS supported on device
	 */
	public Registration createNativeRegistration() {
		Registration registration = new GcmNativeRegistration();
		
		registration.setName(Registration.DEFAULT_REGISTRATION_NAME);
		
		return registration;
		
	}

	/**
	 * Creates template registration according the PNS supported on device
	 */
	public TemplateRegistration createTemplateRegistration() {
		return new GcmTemplateRegistration();
	}

	/**
	 * Parses a native registration according the PNS supported on device
	 * 
	 * @param registrationJson
	 *            The Json representation of the registration
	 */
	public Registration parseNativeRegistration(JsonObject registrationJson) {
		GsonBuilder builder = new GsonBuilder();
		builder = builder.excludeFieldsWithoutExposeAnnotation();
		Gson gson = builder.create();
		Class<? extends Registration> clazz = GcmNativeRegistration.class;
		Registration registration = gson.fromJson(registrationJson, clazz);

		registration.setName(Registration.DEFAULT_REGISTRATION_NAME);
		
		return registration;
	}

	/**
	 * Parses a template registration according the PNS supported on device
	 * 
	 * @param registrationJson
	 *            The Json representation of the registration
	 */
	public TemplateRegistration parseTemplateRegistration(JsonObject registrationJson) {
		GsonBuilder builder = new GsonBuilder();
		builder = builder.excludeFieldsWithoutExposeAnnotation();
		Gson gson = builder.create();
		Class<? extends TemplateRegistration> clazz = GcmTemplateRegistration.class;
		return gson.fromJson(registrationJson, clazz);
	}

	/**
	 * Returns PNS handle field name according the PNS supported on device
	 */
	public String getPlatform() {
		return GcmNativeRegistration.GCM_PLATFORM;
	}
}
