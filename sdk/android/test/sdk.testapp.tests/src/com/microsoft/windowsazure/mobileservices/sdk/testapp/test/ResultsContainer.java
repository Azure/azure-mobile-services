package com.microsoft.windowsazure.mobileservices.sdk.testapp.test;

import java.util.List;

import com.microsoft.windowsazure.mobileservices.MobileServiceUser;

/**
 * 
 * Class used in tests to temporally store results obtained in AsyncTasks to
 * have them available in the asserts' section
 * 
 */
public class ResultsContainer {
	private PersonTestObject person;

	private ComplexPersonTestObject complexPerson;

	private String requestUrl;

	private List<PersonTestObject> peopleResult;

	private int count;

	private String errorMessage;

	private MobileServiceUser user;

	private Boolean operationSucceded;

	private String responseContent;

	private String requestContent;

	private DateTestObject dateTestObject;

	public PersonTestObject getPerson() {
		return person;
	}

	public void setPerson(PersonTestObject person) {
		this.person = person;
	}

	public String getRequestUrl() {
		return requestUrl;
	}

	public void setRequestUrl(String requestUrl) {
		this.requestUrl = requestUrl;
	}

	public List<PersonTestObject> getPeopleResult() {
		return peopleResult;
	}

	public void setPeopleResult(List<PersonTestObject> peopleResult) {
		this.peopleResult = peopleResult;
	}

	public int getCount() {
		return count;
	}

	public void setCount(int count) {
		this.count = count;
	}

	public String getErrorMessage() {
		return errorMessage;
	}

	public void setErrorMessage(String errorMessage) {
		this.errorMessage = errorMessage;
	}

	public MobileServiceUser getUser() {
		return user;
	}

	public void setUser(MobileServiceUser user) {
		this.user = user;
	}

	public Boolean getOperationSucceded() {
		return operationSucceded;
	}

	public void setOperationSucceded(Boolean operationSucceded) {
		this.operationSucceded = operationSucceded;
	}

	public String getResponseValue() {
		return responseContent;
	}

	public void setResponseValue(String responseValue) {
		this.responseContent = responseValue;
	}

	public String getRequestContent() {
		return requestContent;
	}

	public void setRequestContent(String requestContent) {
		this.requestContent = requestContent;
	}

	public ComplexPersonTestObject getComplexPerson() {
		return complexPerson;
	}

	public void setComplexPerson(ComplexPersonTestObject complexPerson) {
		this.complexPerson = complexPerson;
	}

	public DateTestObject getDateTestObject() {
		return dateTestObject;
	}

	public void setDateTestObject(DateTestObject dateTestObject) {
		this.dateTestObject = dateTestObject;
	}

}
