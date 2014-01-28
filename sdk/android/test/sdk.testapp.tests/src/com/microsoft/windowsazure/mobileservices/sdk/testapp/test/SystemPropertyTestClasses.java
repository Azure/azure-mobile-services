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
package com.microsoft.windowsazure.mobileservices.sdk.testapp.test;

import java.util.Date;

import com.google.gson.annotations.SerializedName;

class CreatedAtType {
	public String Id;

	@SerializedName("__createdAt")
	public Date CreatedAt;
}

class UpdatedAtType {
	public String Id;

	@SerializedName("__updatedAt")
	public Date UpdatedAt;
}

class VersionType {
	public String Id;

	@SerializedName("__version")
	public String Version;
}

class AllSystemPropertiesType {
	public String Id;

	@SerializedName("__createdAt")
	public Date CreatedAt;

	@SerializedName("__updatedAt")
	public Date UpdatedAt;

	@SerializedName("__version")
	public String Version;
}

class NamedSystemPropertiesType {
	public String Id;

	Date __createdAt;
}

class NotSystemPropertyCreatedAtType {
	public String Id;

	public Date CreatedAt;
}

class NotSystemPropertyUpdatedAtType {
	public String Id;

	public Date _UpdatedAt;
}

class NotSystemPropertyVersionType {
	public String Id;

	public String version;
}

class StringType {
	public int Id;
	public String String;
}

class IntegerIdNotSystemPropertyCreatedAtType {
	public int Id;

	public Date __createdAt;
}

class IntegerIdWithNamedSystemPropertiesType {
	public int Id;

	public Date __createdAt;
}

class LongIdWithNamedSystemPropertiesType {
	public long Id;

	public Date __createdAt;
}

class DoubleNamedSystemPropertiesType {
	public String Id;

	public Date __createdAt;

	public Date CreatedAt;
}

class NamedDifferentCasingSystemPropertiesType {
	public String Id;

	public Date __CreatedAt;
}

class StringCreatedAtType {
	public String Id;

	@SerializedName("__createdAt")
	public String CreatedAt;
}

class StringUpdatedAtType {
	public String Id;

	@SerializedName("__updatedAt")
	public String UpdatedAt;
}