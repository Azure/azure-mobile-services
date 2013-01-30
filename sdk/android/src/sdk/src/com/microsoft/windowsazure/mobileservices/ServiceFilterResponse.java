/**
 * ServiceFilterResponse.java
 */

package com.microsoft.windowsazure.mobileservices;

import org.apache.http.Header;
import org.apache.http.StatusLine;

/***
 * 
 * Represents an HTTP response that can be manipulated by ServiceFilters
 * 
 */
public interface ServiceFilterResponse {
	/***
	 * Gets the response's headers.
	 * 
	 * @return The response's headers
	 */
	public Header[] getHeaders();

	/***
	 * Gets the response's content.
	 * 
	 * @return String with the response's content
	 * @throws Exception
	 */
	public String getContent();

	/**
	 * Gets the response's status.
	 * 
	 * @return Response's status
	 */
	public StatusLine getStatus();
}
