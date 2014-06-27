package com.microsoft.windowsazure.mobileservices.sdk.testapp.test;

public interface CustomFunctionOneParameter<T, U> {

	U apply(T t) throws Exception;
}
