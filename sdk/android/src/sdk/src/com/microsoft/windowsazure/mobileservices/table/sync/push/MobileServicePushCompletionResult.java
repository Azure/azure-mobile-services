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

/**
 * MobileServicePushCompletionResult.java
 */
package com.microsoft.windowsazure.mobileservices.table.sync.push;

import java.util.List;

import com.microsoft.windowsazure.mobileservices.table.sync.operations.TableOperationError;

/**
 * Gives you errors and status of the push completion.
 */
public class MobileServicePushCompletionResult {
	/**
	 * The state in which push finished.
	 */
	private MobileServicePushStatus mStatus;

	/**
	 * Errors caused by executing operation against remote table.
	 */
	private List<TableOperationError> mOperationErrors;

	/**
	 * Internal error caught during push.
	 */
	private Throwable mInternalError;

	/**
	 * Constructor for MobileServicePushCompletionResult
	 */
	public MobileServicePushCompletionResult() {

	}

	/**
	 * Gets the push status
	 */
	public MobileServicePushStatus getStatus() {
		return this.mStatus;
	}

	/**
	 * Sets the push status
	 */
	public void setStatus(MobileServicePushStatus status) {
		this.mStatus = status;
	}

	/**
	 * Gets the list of table operation errors
	 */
	public List<TableOperationError> getOperationErrors() {
		return this.mOperationErrors;
	}

	/**
	 * Sets the list of table operation errors
	 */
	public void setOperationErrors(List<TableOperationError> operationErrors) {
		this.mOperationErrors = operationErrors;
	}

	/**
	 * Gets the internal error
	 */
	public Throwable getInternalError() {
		return this.mInternalError;
	}

	/**
	 * Sets the internal error
	 */
	public void setInternalError(Throwable internalError) {
		this.mInternalError = internalError;
	}
}