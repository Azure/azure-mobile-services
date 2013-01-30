package com.microsoft.windowsazure.mobileservices.sdk.testapp.test;

public class Address {
	private String streetAddress;
	private int zipCode;
	private String country;

	public Address(String streetAddress, int zipCode, String country) {
		this.streetAddress = streetAddress;
		this.zipCode = zipCode;
		this.country = country;
	}

	public String getStreetAddress() {
		return streetAddress;
	}

	public void setStreetAddress(String streetAddress) {
		this.streetAddress = streetAddress;
	}

	public int getZipCode() {
		return zipCode;
	}

	public void setZipCode(int zipCode) {
		this.zipCode = zipCode;
	}

	public String getCountry() {
		return country;
	}

	public void setCountry(String country) {
		this.country = country;
	}
}
