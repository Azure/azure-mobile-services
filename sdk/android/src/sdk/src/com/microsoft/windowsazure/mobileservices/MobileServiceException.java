/**
 * MobileServiceException.java
 */

package com.microsoft.windowsazure.mobileservices;

public class MobileServiceException extends Exception {

	/**
	 * UID used for serialization
	 */
	private static final long serialVersionUID = 5267990724102948298L;

	/**
	 * Creates a new MobileServiceException with a detail message and a cause
	 * 
	 * @param detail
	 *            The detail message
	 * @param throwable
	 *            The exception cause
	 */
	public MobileServiceException(String detail, Throwable throwable) {
		super(detail, throwable);
	}

	/**
	 * Creates a new MobileServiceException with a detail message
	 * 
	 * @param detail
	 *            The detail message
	 */
	public MobileServiceException(String detail) {
		super(detail);
	}
}
