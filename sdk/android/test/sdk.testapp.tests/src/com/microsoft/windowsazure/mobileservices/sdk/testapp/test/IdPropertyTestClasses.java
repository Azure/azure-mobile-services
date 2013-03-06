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