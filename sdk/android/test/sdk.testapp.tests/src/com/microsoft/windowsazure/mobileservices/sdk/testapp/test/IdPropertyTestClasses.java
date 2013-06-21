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

import com.google.gson.annotations.SerializedName;

// Class with an invalid Id property but using a valid gson serialized name
class IdPropertyWithGsonAnnotation {
	@SerializedName("id")
	private int myId;

	private String name;

	public IdPropertyWithGsonAnnotation(String name) {
		this.name = name;
	}

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}

	public int getId() {
		return myId;
	}

	public void setId(int id) {
		myId = id;
	}
}

// Class with a different cased id property
class IdPropertyWithDifferentIdPropertyCasing {
	private int ID;

	private String name;

	public IdPropertyWithDifferentIdPropertyCasing(String name) {
		this.name = name;
	}

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}

	public int getId() {
		return ID;
	}

	public void setId(int id) {
		ID = id;
	}
}

//Class with multiple id properties
class IdPropertyMultipleIdsTestObject {
	private int id;
	private int ID;
	private int iD;
	
	private String name;

	public IdPropertyMultipleIdsTestObject(String name) {
		this.name = name;
	}

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}

	public int getId() {
		return id;
	}

	public void setId(int id) {
		this.id = id;
	}

	public int getID() {
		return ID;
	}

	public void setID(int iD) {
		ID = iD;
	}

	public int getiD() {
		return iD;
	}

	public void setiD(int iD) {
		this.iD = iD;
	}

}