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
package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

public class AggregateException extends Exception {
	/**
	 * 
	 */
	private static final long serialVersionUID = -3507943118377161658L;

	private List<Exception> mAggregatedExceptions = Collections.synchronizedList(new ArrayList<Exception>());

	public AggregateException() {
		super();
	}

	public AggregateException(String detailMessage) {
		super(detailMessage);
	}

	public AggregateException(Throwable throwable) {
		super(throwable);
	}

	public AggregateException(String detailMessage, Throwable throwable) {
		super(detailMessage, throwable);
	}

	public void addException(Exception exception) {
		mAggregatedExceptions.add(exception);
	}

	public List<Exception> getAggregatedExceptions() {
		return mAggregatedExceptions;
	}
}
