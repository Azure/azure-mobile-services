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

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.ConcurrentLinkedQueue;

import com.microsoft.windowsazure.mobileservices.MobileServiceClient;

public abstract class TestGroup {
	List<TestCase> mTestCases;
	String mName;
	TestStatus mStatus;
	ConcurrentLinkedQueue<TestCase> mTestRunQueue;
	boolean mNewTestRun;
	
	public static final String AllTestsGroupName = "All tests";
	public static final String AllUnattendedTestsGroupName = AllTestsGroupName + " (unattended)";
	
	public static final String ClientVersionKey = "client-version";
	public static final String ServerVersionKey = "server-version";

	public TestGroup(String name) {
		mName = name;
		mStatus = TestStatus.NotRun;
		mTestCases = new ArrayList<TestCase>();
		mTestRunQueue = new ConcurrentLinkedQueue<TestCase>();
		mNewTestRun = false;
	}

	public TestStatus getStatus() {
		return mStatus;
	}

	public List<TestCase> getTestCases() {
		return mTestCases;
	}

	protected void addTest(TestCase testCase) {
		mTestCases.add(testCase);
	}

	
	public void runTests(MobileServiceClient client, TestExecutionCallback callback) {
		List<TestCase> testsToRun = new ArrayList<TestCase>();
		
		for (int i = 0; i < mTestCases.size(); i++) {
			if (mTestCases.get(i).isEnabled()) {
				testsToRun.add(mTestCases.get(i));
			}
		}

		if (testsToRun.size() > 0) {
			runTests(testsToRun, client, callback);
		}
	}
	
	
	public void runTests(List<TestCase> testsToRun, final MobileServiceClient client, final TestExecutionCallback callback) {
		try {
			onPreExecute(client);
		} catch (Exception e) {
			mStatus = TestStatus.Failed;
			if (callback != null)
				callback.onTestGroupComplete(this, null);
			return;
		}
		
		final TestRunStatus testRunStatus = new TestRunStatus();

		mNewTestRun = true;
		
		int oldQueueSize = mTestRunQueue.size();
		mTestRunQueue.clear();
		mTestRunQueue.addAll(testsToRun);
		cleanTestsState();
		testRunStatus.results.clear();
		mStatus = TestStatus.NotRun;
		
		if (oldQueueSize == 0) {
			executeNextTest(client, callback, testRunStatus);
		}
	}


	private void cleanTestsState() {
		for (TestCase test : mTestRunQueue) {
			test.setStatus(TestStatus.NotRun);
			test.clearLog();
		}
	}
	
	private void executeNextTest(final MobileServiceClient client, final TestExecutionCallback callback, final TestRunStatus testRunStatus) {
		mNewTestRun = false;
		final TestGroup group = this;

		try {
			TestCase nextTest = mTestRunQueue.poll();
			if (nextTest != null) {
				nextTest.run(client, new TestExecutionCallback() {
					@Override
					public void onTestStart(TestCase test) {
						if (!mNewTestRun && callback != null)
							callback.onTestStart(test);
					}

					@Override
					public void onTestGroupComplete(TestGroup group, List<TestResult> results) {
						if (!mNewTestRun && callback != null)
							callback.onTestGroupComplete(group, results);
					}

					@Override
					public void onTestComplete(TestCase test, TestResult result) {
						if (mNewTestRun) {
							cleanTestsState();
							testRunStatus.results.clear();
							mStatus = TestStatus.NotRun;
						} else {
							if (test.getExpectedExceptionClass() != null) {
								if (result.getException() != null && result.getException().getClass() == test.getExpectedExceptionClass()) {
									result.setStatus(TestStatus.Passed);
								} else {
									result.setStatus(TestStatus.Failed);
								}
							}
		
							test.setStatus(result.getStatus());
							testRunStatus.results.add(result);
		
							if (callback != null)
								callback.onTestComplete(test, result);
						}
						
						executeNextTest(client, callback, testRunStatus);
					}
				});
				
				
			} else {
				// end run
				
				try {
					onPostExecute(client);
				} catch (Exception e) {
					mStatus = TestStatus.Failed;
				}
				
				// if at least one test failed, the test group
				// failed
				if (mStatus != TestStatus.Failed) {
					mStatus = TestStatus.Passed;
					for (TestResult r : testRunStatus.results) {
						if (r.getStatus() == TestStatus.Failed) {
							mStatus = TestStatus.Failed;
							break;
						}
					}
				}

				if (callback != null)
					callback.onTestGroupComplete(group, testRunStatus.results);
			}
			
			
		} catch (Exception e) {
			if (callback != null)
				callback.onTestGroupComplete(this, testRunStatus.results);
		}
	}

	public String getName() {
		return mName;
	}

	protected void setName(String name) {
		mName = name;
	}

	@Override
	public String toString() {
		return getName();
	}

	public void onPreExecute(MobileServiceClient client) {

	}

	public void onPostExecute(MobileServiceClient client) {

	}

	private class TestRunStatus {
		public List<TestResult> results;

		public TestRunStatus() {
			results = new ArrayList<TestResult>();
		}
	}
}