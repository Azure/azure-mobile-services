package com.microsoft.windowsazure.mobileservices.sdk.testapp.test;

public class ComplexPersonTestObject {
	private Integer id;
	private String firstName;
	private String lastName;
	private Address address;

	public ComplexPersonTestObject(String firstName, String lastName,
			Address address) {
		this.firstName = firstName;
		this.lastName = lastName;
		this.address = address;
	}

	public Integer getId() {
		return id;
	}

	public void setId(int id) {
		this.id = id;
	}

	public String getFirstName() {
		return firstName;
	}

	public void setFirstName(String firstName) {
		this.firstName = firstName;
	}

	public String getLastName() {
		return lastName;
	}

	public void setLastName(String lastName) {
		this.lastName = lastName;
	}

	public Address getAddress() {
		return address;
	}

	public void setAddress(Address address) {
		this.address = address;
	}
}
