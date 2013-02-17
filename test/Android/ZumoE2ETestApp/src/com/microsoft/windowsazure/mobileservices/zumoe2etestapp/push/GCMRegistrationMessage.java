package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.push;

public class GCMRegistrationMessage {
	public final boolean isError;
	public final String value;
	public GCMRegistrationMessage(String value, boolean isError) {
		this.value = value;
		this.isError = isError;
	}
}
