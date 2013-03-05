package com.microsoft.windowsazure.mobileservices.sdk.testapp.test;

import com.google.gson.annotations.SerializedName;

// Class with ID property
class IdPropertyUpperCasedTestObject {
	private int ID;
	private String name;

	public IdPropertyUpperCasedTestObject(String name) {
		this.name = name;
	}

	public int getID() {
		return ID;
	}

	public void setID(int iD) {
		ID = iD;
	}

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}
}

// Class with iD property
class IdPropertyDUpperCasedTestObject {
	private int iD;
	private String name;

	public IdPropertyDUpperCasedTestObject(String name) {
		this.name = name;
	}

	public int getiD() {
		return iD;
	}

	public void setiD(int iD) {
		this.iD = iD;
	}

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}

}

//Class with Id property
class IdPropertyIUpperCasedTestObject {
	private int Id;
	private String name;

	public IdPropertyIUpperCasedTestObject(String name) {
		this.name = name;
	}

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}

	public int getId() {
		return Id;
	}

	public void setId(int id) {
		Id = id;
	}

}

// Class with an invalid Id property but using a valid gson serialized name
class IdPropertyWithGsonAnnotation {
	@SerializedName("id")
	private int ID;

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
		return ID;
	}

	public void setId(int id) {
		ID = id;
	}
}

//Class with Id property
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