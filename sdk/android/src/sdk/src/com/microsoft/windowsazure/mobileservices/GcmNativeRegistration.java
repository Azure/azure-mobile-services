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
 * GcmNativeRegistration.java
 */

package com.microsoft.windowsazure.mobileservices;

import com.google.gson.annotations.Expose;
import com.google.gson.annotations.SerializedName;

/**
 * Represents GCM native registration
 */
public class GcmNativeRegistration extends Registration {

	/**
	 * Gcm Platform identifier
	 */
	static final String GCM_PLATFORM = "gcm";

	@Expose
	@SerializedName("platform")
	private String mPlatform = GCM_PLATFORM;

	/**
	 * The PNS specific identifier
	 */
	@Expose
	@SerializedName("deviceId")
	protected String mPNSHandle;

	/**
	 * The registration name
	 */
	protected String mName;

	/**
	 * Creates a new native registration
	 */
	GcmNativeRegistration() {
		super();
	}

	@Override
	public String getName() {
		return mName;
	}

	@Override
	protected void setName(String name) {
		this.mName = name;
	}

	@Override
	public String getPNSHandle() {
		return mPNSHandle;
	}

	@Override
	protected void setPNSHandle(String pNSHandle) {
		this.mPNSHandle = pNSHandle;
	}
}
