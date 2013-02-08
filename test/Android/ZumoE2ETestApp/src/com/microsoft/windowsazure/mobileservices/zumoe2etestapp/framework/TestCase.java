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
package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework;

import com.microsoft.windowsazure.mobileservices.MobileServiceClient;

public abstract class TestCase {
	private String mName;

	private String mDescription;

	private Class<?> mExpectedExceptionClass;

	private boolean mEnabled;

	private TestStatus mStatus;

	private StringBuilder mTestLog;

	public TestCase() {
		mEnabled = false;
		mStatus = TestStatus.NotRun;
		mTestLog = new StringBuilder();
	}

	public void log(String log) {
		mTestLog.append(log);
		mTestLog.append("\n");
	}

	public String getLog() {
		return mTestLog.toString();
	}

	public TestStatus getStatus() {
		return mStatus;
	}

	public void setStatus(TestStatus status) {
		mStatus = status;
	}

	public boolean isEnabled() {
		return mEnabled;
	}

	public void setEnabled(boolean enabled) {
		mEnabled = enabled;
	}

	public void run(MobileServiceClient client, TestExecutionCallback callback) {
		try {
			if (callback != null)
				callback.onTestStart(this);
		} catch (Exception e) {
			// do nothing
		}
		mStatus = TestStatus.Running;
		try {
			executeTest(client, callback);
		} catch (Exception e) {
			TestResult result;
			if (e.getClass() != this.getExpectedExceptionClass()) {
				result = createResultFromException(e);
				mStatus = result.getStatus();
			} else {
				result = new TestResult();
				result.setException(e);
				result.setStatus(TestStatus.Passed);
				result.setTestCase(this);
				mStatus = result.getStatus();
			}

			if (callback != null)
				callback.onTestComplete(this, result);
		}
	}

	protected abstract void executeTest(MobileServiceClient client, TestExecutionCallback callback);

	protected TestResult createResultFromException(Exception e) {
		return createResultFromException(new TestResult(), e);
	}

	protected TestResult createResultFromException(TestResult result, Exception e) {
		result.setException(e);
		result.setTestCase(this);

		result.setStatus(TestStatus.Failed);

		return result;
	}

	public String getName() {
		return mName;
	}

	public void setName(String name) {
		mName = name;
	}

	public String getDescription() {
		return mDescription;
	}

	public void setDescription(String description) {
		mDescription = description;
	}

	public void setExpectedExceptionClass(Class<?> expectedExceptionClass) {
		mExpectedExceptionClass = expectedExceptionClass;
	}

	public Class<?> getExpectedExceptionClass() {
		return mExpectedExceptionClass;
	}
}
