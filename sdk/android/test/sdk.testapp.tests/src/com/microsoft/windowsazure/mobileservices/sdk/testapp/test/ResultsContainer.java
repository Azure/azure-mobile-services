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

import java.util.ArrayList;
import java.util.List;

import com.google.gson.JsonElement;
import com.microsoft.windowsazure.mobileservices.MobileServiceUser;

/**
 * 
 * Class used in tests to temporally store results obtained in AsyncTasks to
 * have them available in the asserts' section
 * 
 */
public class ResultsContainer {	
	private PersonTestObject person;
	
	private PersonTestObjectWithoutId personWithoutId;
	
	private PersonTestObjectWithStringId personWithStringId;

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
	
	private IdPropertyWithGsonAnnotation idPropertyWithGsonAnnotation;
	
	private IdPropertyWithDifferentIdPropertyCasing idPropertyWithDifferentIdPropertyCasing;
	
	private Exception exception;
	
	private JsonElement jsonResult;
	
	private byte[] rawResponseContent;

	private Object customResult;
	
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

	public void setPeopleResult(PersonTestObject[] peopleResult) {
		this.peopleResult = new ArrayList<PersonTestObject>();
		for (PersonTestObject person : peopleResult) {
			this.peopleResult.add(person);
		}
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

	public PersonTestObjectWithoutId getPersonWithoutId() {
		return personWithoutId;
	}

	public void setPersonWithoutId(PersonTestObjectWithoutId personWithNoId) {
		this.personWithoutId = personWithNoId;
	}

	public PersonTestObjectWithStringId getPersonWithStringId() {
		return personWithStringId;
	}

	public void setPersonWithStringId(PersonTestObjectWithStringId personWithStringId) {
		this.personWithStringId = personWithStringId;
	}

	public IdPropertyWithGsonAnnotation getIdPropertyWithGsonAnnotation() {
		return idPropertyWithGsonAnnotation;
	}

	public void setIdPropertyWithGsonAnnotation(
			IdPropertyWithGsonAnnotation idPropertyWithGsonAnnotation) {
		this.idPropertyWithGsonAnnotation = idPropertyWithGsonAnnotation;
	}

	public IdPropertyWithDifferentIdPropertyCasing getIdPropertyWithDifferentIdPropertyCasing() {
		return idPropertyWithDifferentIdPropertyCasing;
	}

	public void setIdPropertyWithDifferentIdPropertyCasing(
			IdPropertyWithDifferentIdPropertyCasing idPropertyWithDifferentIdPropertyCasing) {
		this.idPropertyWithDifferentIdPropertyCasing = idPropertyWithDifferentIdPropertyCasing;
	}


	public Exception getException() {
		return exception;
	}

	
	public void setException(Exception exception) {
		this.exception = exception;
	}

	public JsonElement getJsonResult() {
		return jsonResult;
	}

	public void setJsonResult(JsonElement jsonResult) {
		this.jsonResult = jsonResult;
	}

	public byte[] getRawResponseContent() {
		return rawResponseContent;
	}

	public void setRawResponseContent(byte[] rawResponseContent) {
		this.rawResponseContent = rawResponseContent;
	}

	public Object getCustomResult() {
		return customResult;
	}

	public void setCustomResult(Object customResult) {
		this.customResult = customResult;
	}

}
