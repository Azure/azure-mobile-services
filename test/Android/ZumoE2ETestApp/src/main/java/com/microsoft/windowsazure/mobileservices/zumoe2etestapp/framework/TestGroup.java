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

import android.annotation.TargetApi;
import android.os.Build;

import com.microsoft.windowsazure.mobileservices.MobileServiceClient;

import java.util.ArrayList;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.concurrent.ConcurrentLinkedQueue;
import java.util.concurrent.CountDownLatch;

public abstract class TestGroup {
    public static final String AllTestsGroupName = "All tests";
    public static final String AllUnattendedTestsGroupName = AllTestsGroupName + " (unattended)";
    public static final String ClientVersionKey = "client-version";
    public static final String ServerVersionKey = "server-version";
    List<TestCase> mTestCases;
    Map<String, String> mSourceMap;
    String mName;
    TestStatus mStatus;
    ConcurrentLinkedQueue<TestCase> mTestRunQueue;
    boolean mNewTestRun;
    private int mFailedTestCount;
    private Date mStartTime;
    private Date mEndTime;

    public TestGroup(String name) {
        mName = name;
        mStatus = TestStatus.NotRun;
        mTestCases = new ArrayList<TestCase>();
        mSourceMap = new HashMap<String, String>();
        mTestRunQueue = new ConcurrentLinkedQueue<TestCase>();
        mNewTestRun = false;
    }

    public TestStatus getStatus() {
        return mStatus;
    }

    public List<TestCase> getTestCases() {
        return mTestCases;
    }

    public Map<String, String> getSourceMap() {
        return mSourceMap;
    }

    protected void addTest(TestCase testCase) {
        addTest(testCase, this.getClass().getName());
    }

    protected void addTest(TestCase testCase, String source) {
        mTestCases.add(testCase);
        mSourceMap.put(testCase.getName(), source);
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

    @TargetApi(Build.VERSION_CODES.HONEYCOMB)
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
            for (final TestCase test : mTestRunQueue) {

                final CountDownLatch latch = new CountDownLatch(1);

                Thread thread = new Thread() {
                    public void run() {
                        executeNextTest(test, client, callback, testRunStatus, latch);
                    }
                };

                thread.run();

                try {
                    latch.await();
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }

                if (test.getStatus() == TestStatus.Failed) {
                    mFailedTestCount++;
                }
            }

            // End Run
            final CountDownLatch latch = new CountDownLatch(1);

            Thread thread = new Thread() {
                public void run() {
                    executeNextTest(null, client, callback, testRunStatus, latch);
                }
            };

            thread.run();

            try {
                latch.await();
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
        }
    }

    private void cleanTestsState() {
        for (TestCase test : mTestRunQueue) {
            test.setStatus(TestStatus.NotRun);
            test.clearLog();
        }
    }

    @TargetApi(Build.VERSION_CODES.HONEYCOMB)
    private void executeNextTest(final TestCase nextTest, final MobileServiceClient client, final TestExecutionCallback callback,
                                 final TestRunStatus testRunStatus, final CountDownLatch latch) {
        mNewTestRun = false;
        final TestGroup group = this;

        try {
            // TestCase nextTest = mTestRunQueue.poll();
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

                        latch.countDown();
                        // executeNextTest(client, callback, testRunStatus);
                    }
                });
            } else {
                // end run

                try {
                    group.onPostExecute(client);
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

                latch.countDown();
            }
        } catch (Exception e) {
            if (callback != null)
                callback.onTestGroupComplete(group, testRunStatus.results);

            latch.countDown();
        }
    }

    public String getName() {
        return mName;
    }

    protected void setName(String name) {
        mName = name;
    }

    public int getFailedTestCount() {
        return mFailedTestCount;
    }

    public Date getStartTime() {
        return mStartTime;
    }

    public Date getEndTime() {
        return mEndTime;
    }

    @Override
    public String toString() {
        return getName();
    }

    public void onPreExecute(MobileServiceClient client) {
        mFailedTestCount = 0;
        mStartTime = new Date();
    }

    public void onPostExecute(MobileServiceClient client) {
        mEndTime = new Date();
    }

    private class TestRunStatus {
        public List<TestResult> results;

        public TestRunStatus() {
            results = new ArrayList<TestResult>();
        }
    }
}