package com.microsoft.windowsazure.mobileservices;
/**
 * Callback to invoke after set the registration Id
 */
interface SetRegistrationIdCallback {
	/**
	 * Method to execute when the set is finished
	 * 
	 * @param registrationId
	 * 			  A registration id
	 * 
	 * @param exception
	 *            An exception representing the error, in case there was one
	 */
	public void onSet(String registrationId, Exception exception);
}
