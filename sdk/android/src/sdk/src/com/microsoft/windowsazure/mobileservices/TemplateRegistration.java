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
 * TemplateRegistration.java
 */

package com.microsoft.windowsazure.mobileservices;

import com.google.gson.annotations.Expose;
import com.google.gson.annotations.SerializedName;

/**
 * Represents a template registration
 */
public abstract class TemplateRegistration extends Registration {

	/**
	 * The template body
	 */
	@Expose
	@SerializedName("templateBody")
	protected String mTemplateBody;

	/**
	 * The registration name
	 */
	@Expose
	@SerializedName("templateName")
	protected String mName;

	/**
	 * Creates a new template registration
	 */
	TemplateRegistration() {
		super();
	}

	/**
	 * Gets the template body
	 */
	public String getTemplateBody() {
		return mTemplateBody;
	}

	/**
	 * Sets the template body
	 */
	void setTemplateBody(String templateBody) {
		mTemplateBody = templateBody;
	}

	/**
	 * Gets the template name
	 */
	public String getTemplateName() {
		return getName();
	}
}
