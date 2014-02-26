package com.microsoft.windowsazure.mobileservices;

/**
 * Callback to invoke after register the template
 */
public interface TemplateRegistrationCallback {
	/**
	 * Method to execute when the register is finished
	 * 
	 * @param templateRegistration
	 *            The current template registration
	 *            
	 * @param exception
	 *            An exception representing the error, in case there was one
	 */
	public void onRegister(TemplateRegistration templateRegistration, Exception exception);
}