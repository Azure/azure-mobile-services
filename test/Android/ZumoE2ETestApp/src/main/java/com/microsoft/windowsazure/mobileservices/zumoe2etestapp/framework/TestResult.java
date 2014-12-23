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

public class TestResult {
    private TestStatus mStatus;

    private Exception mException;

    private TestCase mTestCase;

    public TestStatus getStatus() {
        return mStatus;
    }

    public void setStatus(TestStatus status) {
        this.mStatus = status;
    }

    public Exception getException() {
        return mException;
    }

    public void setException(Exception e) {
        this.mException = e;
    }

    public TestCase getTestCase() {
        return mTestCase;
    }

    public void setTestCase(TestCase testCase) {
        mTestCase = testCase;
    }
}