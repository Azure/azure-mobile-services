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
package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests;

import com.google.android.gms.auth.GoogleAuthUtil;
import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.UserAuthenticationCallback;
import com.microsoft.windowsazure.mobileservices.authentication.MobileServiceAuthenticationProvider;
import com.microsoft.windowsazure.mobileservices.authentication.MobileServiceUser;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.MainActivity;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestCase;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestExecutionCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestGroup;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestResult;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestStatus;

public class ClientSDKLoginTests extends TestGroup {

    public static MainActivity mainActivity;

    public ClientSDKLoginTests() {
        super("ClientSDKLogin tests");
        this.addTest(createLoginWithGoogleSDKTest());
    }

    @SuppressWarnings("deprecation")
    public static TestCase createLoginWithGoogleSDKTest() {
        TestCase test = new TestCase("Login With GoogleSDK Test") {
            @Override
            protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {
                String GOOGLE_SCOPE_TAKE2 = "audience:server:client_id:";
                String CLIENT_ID_WEB_APPS = mainActivity.getGoogleWebAppClientId();
                String GOOGLE_ID_TOKEN_SCOPE = GOOGLE_SCOPE_TAKE2 + CLIENT_ID_WEB_APPS;
                final TestCase testCase = this;
                final TestResult result = new TestResult();
                String idToken = null;
                try {
                    idToken = GoogleAuthUtil.getToken(mainActivity, mainActivity.getGoogleUserId(), GOOGLE_ID_TOKEN_SCOPE);
                    log("Acquired id_token from google");
                } catch (Exception exception) {
                    result.setTestCase(testCase);
                    result.setException(exception);
                    result.setStatus(TestStatus.Failed);
                    callback.onTestComplete(testCase, result);
                    return;
                }
                JsonObject loginBody = new JsonObject();
                loginBody.addProperty("id_token", idToken);
                client.login(MobileServiceAuthenticationProvider.Google, loginBody, new UserAuthenticationCallback() {
                    @Override
                    public void onCompleted(MobileServiceUser user, Exception error,
                                            ServiceFilterResponse response) {
                        try {
                            if (error != null) {
                                log("Login into mobile service with google token failed. Error: " + error);
                                result.setStatus(TestStatus.Failed);
                            } else {
                                log("Logged in to the mobile service as " + user.getUserId());
                                result.setStatus(TestStatus.Passed);
                                client.logout();
                            }
                        } catch (Exception exception) {
                            result.setException(exception);
                            result.setStatus(TestStatus.Failed);
                        } finally {
                            result.setTestCase(testCase);
                            callback.onTestComplete(testCase, result);
                        }
                    }
                });

            }
        };
        return test;
    }
}
