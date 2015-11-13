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

/*
package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.log;


import android.annotation.SuppressLint;
import android.os.AsyncTask;
import android.os.Build;

import com.google.common.util.concurrent.FutureCallback;
import com.google.common.util.concurrent.Futures;
import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.SettableFuture;
import com.google.gson.JsonArray;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.google.gson.JsonPrimitive;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestCase;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestExecutionCallback;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.io.UnsupportedEncodingException;
import java.net.HttpURLConnection;
import java.net.URI;
import java.net.URISyntaxException;
import java.net.URL;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.TimeZone;
import java.util.UUID;
import java.util.concurrent.ExecutionException;

public class DaylightLogger {

    private String mDaylightUrl;
    private String mDayLightProject;
    private String mClientId;
    private String mClientSecret;
    private String mRuntime;
    private String mRunId;
    private String mPlatform = "Android";
    private String mRevision = "20140729-002505";
    private String mSDKVersion = "sdk v1.2.4";

    public DaylightLogger(String daylightUrl, String dayLightProject, String clientId, String clientSecret, String runtime, String runId) {
        mDaylightUrl = daylightUrl;
        mDayLightProject = dayLightProject;
        mClientId = clientId;
        mClientSecret = clientSecret;
        mRuntime = runtime;
        mRunId = runId;
    }

    private static void uploadBlob(List<TestCase> tests, String blobAccessToken) {
        String urlBlob = "https://daylight.blob.core.windows.net/attachments";

        for (TestCase test : tests) {
            String blobName = test.getFileName();
            String body = test.getLog();

            URI requestUrl = null;

            try {
                requestUrl = new URI(urlBlob + "/" + blobName + "?" + blobAccessToken);
            } catch (URISyntaxException e) {
            }

            test.setFileName(blobName);

            HttpPut request = new HttpPut(requestUrl);
            request.addHeader("x-ms-blob-type", "BlockBlob");

            try {
                request.setEntity(new StringEntity(body, "UTF-8"));
            } catch (UnsupportedEncodingException uee) {
            }

            final SettableFuture<Void> externalFuture = SettableFuture.create();

            ListenableFuture<HttpURLConnection> internalFuture = execute(request);

            Futures.addCallback(internalFuture, new FutureCallback<HttpURLConnection>() {
                @Override
                public void onFailure(Throwable throwable) {
                    externalFuture.setException(throwable);
                }

                @Override
                public void onSuccess(HttpURLConnection connection) {
                    try {
                        connection.getInputStream();
                        externalFuture.set(null);
                    } catch (Throwable throwable) {
                        externalFuture.setException(throwable);
                    }
                }
            });

            try {
                externalFuture.get();
            } catch (InterruptedException | ExecutionException e) {
                e.printStackTrace();
            }
        }
    }

    private static ListenableFuture<HttpURLConnection> execute(final HttpEntityEnclosingRequestBase request) {
        final SettableFuture<HttpURLConnection> result = SettableFuture.create();

        AsyncTask<Void, Void, Void> task = new AsyncTask<Void, Void, Void>() {
            @Override
            protected Void doInBackground(Void... params) {

                try {
                    result.set(createHttpURLConnection(request));
                } catch (Throwable t) {
                    result.setException(t);
                }

                return null;
            }
        };

        execute(task);

        return result;
    }

    private static HttpURLConnection createHttpURLConnection(HttpEntityEnclosingRequestBase request) throws IOException {
        URL url = request.getURI().toURL();

        HttpURLConnection connection = (HttpURLConnection) url.openConnection();
        connection.setConnectTimeout(15 * 1000);
        connection.setRequestMethod(request.getMethod());

        Header[] headers = request.getAllHeaders();

        if (headers != null) {

            for (Header header : headers) {
                connection.setRequestProperty(header.getName(), header.getValue());
            }
        }

        HttpEntity entity = request.getEntity();

        if (entity != null) {
            InputStream in = entity.getContent();
            OutputStream out = connection.getOutputStream();
            byte[] buffer = new byte[1024];
            int length;

            while ((length = in.read(buffer)) != -1) {
                out.write(buffer, 0, length);
            }

            in.close();
            out.close();
        }

        return connection;
    }

    private static String getContent(HttpURLConnection connection) throws IOException {
        String content = null;
        byte[] rawContent = getRawContent(connection);

        if (rawContent != null) {
            try {
                content = new String(rawContent, "UTF-8");
            } catch (UnsupportedEncodingException e) {
            }
        }

        return content;
    }

    private static byte[] getRawContent(HttpURLConnection connection) throws IOException {
        byte[] rawContent = null;
        InputStream instream = connection.getInputStream();
        ByteArrayOutputStream out = new ByteArrayOutputStream();
        byte[] buffer = new byte[1024];
        int length;

        while ((length = instream.read(buffer)) != -1) {
            out.write(buffer, 0, length);
        }

        instream.close();
        rawContent = out.toByteArray();

        return rawContent;
    }

    @SuppressLint("NewApi")
    private static void execute(AsyncTask<Void, Void, Void> task) {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.HONEYCOMB) {
            task.executeOnExecutor(AsyncTask.THREAD_POOL_EXECUTOR);
        } else {
            task.execute();
        }
    }

    private static String dateToString(Date date) {
        SimpleDateFormat dateFormat = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss.SSS'Z'", Locale.getDefault());
        dateFormat.setTimeZone(TimeZone.getTimeZone("UTC"));

        return dateFormat.format(date);
    }

    private static Long getFileTime(Date date) {
        return (date.getTime() + 11644473600000L) * 10000L;
    }

    public void reportResultsToDaylight(final int failedTestCount, final Date startTime, final Date endTime, final List<TestCase> tests,
                                        final Map<String, String> sourceMap) throws Throwable {
        try {
            final int testCount = tests.size();
            final JsonObject testRun = initializeRun(testCount, startTime);

            String authAccessToken = requestAuthAccessToken().get();

            String runId = requestRunId(authAccessToken, testRun).get();

            String blobAccessToken = requestBlobAccessToken(authAccessToken).get();

            try {
                List<TestCase> mastertests = createMasterTestLog(testCount, failedTestCount, runId);

                // Post result for master test run
                JsonArray masterRunResult = createMasterRunResult(failedTestCount, startTime, endTime, mRunId, mastertests.get(0).getFileName());
                postResult(authAccessToken, masterRunResult).get();

                // Upload master test log
                uploadBlob(mastertests, blobAccessToken);
            } catch (Throwable t) {

            }

            // post test results
            JsonArray result = parseRunResult(runId, tests, sourceMap);
            postResult(authAccessToken, result).get();

            // Upload test logs
            uploadBlob(tests, blobAccessToken);
        } catch (ExecutionException e) {
            throw e.getCause();
        }
    }

    private JsonObject initializeRun(int testCount, Date startTime) {
        JsonObject versionSpec = new JsonObject();
        versionSpec.addProperty("project_name", mDayLightProject);
        versionSpec.addProperty("branch_name", mRuntime);
        versionSpec.addProperty("revision", mRevision);

        JsonObject testRun = new JsonObject();
        testRun.addProperty("name", mPlatform + "|" + mSDKVersion + "|" + mRuntime + "|" + dateToString(startTime));

        testRun.addProperty("start_time", getFileTime(startTime));
        testRun.add("version_spec", versionSpec);
        testRun.addProperty("tags", mPlatform);
        testRun.addProperty("test_count", testCount);

        return testRun;
    }

    private ListenableFuture<String> requestAuthAccessToken() {
        String url = mDaylightUrl + "/oauth2/token";

        HttpPost request = new HttpPost(url);
        request.addHeader("Content-Type", "application/x-www-form-urlencoded");

        try {
            request.setEntity(new StringEntity("grant_type=client_credentials&client_id=" + mClientId + "&client_secret=" + mClientSecret, "UTF-8"));
        } catch (UnsupportedEncodingException uee) {
        }

        final SettableFuture<String> result = SettableFuture.create();

        ListenableFuture<HttpURLConnection> internalFuture = execute(request);

        Futures.addCallback(internalFuture, new FutureCallback<HttpURLConnection>() {
            @Override
            public void onFailure(Throwable throwable) {
                result.setException(throwable);
            }

            @Override
            public void onSuccess(HttpURLConnection connection) {
                try {
                    int statusCode = connection.getResponseCode();

                    if (statusCode == 200) {
                        String content = getContent(connection);
                        JsonObject json = new JsonParser().parse(content).getAsJsonObject();
                        String authAccessToken = json.get("access_token").getAsString();
                        result.set(authAccessToken);
                    } else {
                        result.setException(new Exception("Invalid response status code " + statusCode));
                    }
                } catch (Throwable t) {
                    result.setException(t);
                }
            }
        });

        return result;
    }

    private ListenableFuture<String> requestRunId(String authAccessToken, JsonObject testRun) {
        String jsonStr = testRun.toString();
        String requestUrl = mDaylightUrl + "/api/zumo2/runs?access_token=" + authAccessToken;

        HttpPost request = new HttpPost(requestUrl);
        request.addHeader("Accept", "application/json");

        try {
            request.setEntity(new StringEntity(jsonStr, "UTF-8"));
        } catch (UnsupportedEncodingException uee) {
        }

        final SettableFuture<String> result = SettableFuture.create();

        ListenableFuture<HttpURLConnection> internalFuture = execute(request);

        Futures.addCallback(internalFuture, new FutureCallback<HttpURLConnection>() {
            @Override
            public void onFailure(Throwable throwable) {
                result.setException(throwable);
            }

            @Override
            public void onSuccess(HttpURLConnection connection) {
                try {
                    int statusCode = connection.getResponseCode();

                    if (statusCode == 201) {
                        String content = getContent(connection);
                        JsonObject json = new JsonParser().parse(content).getAsJsonObject();
                        String runId = json.get("run_id").getAsString();
                        result.set(runId);
                    } else {
                        result.setException(new Exception("Invalid response status code " + statusCode));
                    }
                } catch (Throwable t) {
                    result.setException(t);
                }
            }
        });

        return result;
    }

    private ListenableFuture<String> requestBlobAccessToken(String authAccessToken) {
        String body = "grant_type=urn%3Adaylight%3Aoauth2%3Ashared-access-signature&permissions=rwdl&scope=attachments";
        String requestUrl = mDaylightUrl + "/api/zumo2/storageaccounts/token?access_token=" + authAccessToken;

        HttpPost request = new HttpPost(requestUrl);
        request.addHeader("Content-Type", "application/x-www-form-urlencoded");

        try {
            request.setEntity(new StringEntity(body, "UTF-8"));
        } catch (UnsupportedEncodingException uee) {
        }

        final SettableFuture<String> result = SettableFuture.create();

        ListenableFuture<HttpURLConnection> internalFuture = execute(request);

        Futures.addCallback(internalFuture, new FutureCallback<HttpURLConnection>() {
            @Override
            public void onFailure(Throwable throwable) {
                result.setException(throwable);
            }

            @Override
            public void onSuccess(HttpURLConnection connection) {
                try {
                    int statusCode = connection.getResponseCode();

                    if (statusCode == 201) {
                        String content = getContent(connection);
                        JsonObject json = new JsonParser().parse(content).getAsJsonObject();
                        String blobAccessToken = json.get("access_token").getAsString();
                        result.set(blobAccessToken);
                    } else {
                        result.setException(new Exception("Invalid response status code " + statusCode));
                    }
                } catch (Throwable t) {
                    result.setException(t);
                }
            }
        });

        return result;
    }

    private List<TestCase> createMasterTestLog(int testCount, int failedTestCount, String runId) {
        TestCase test = new TestCase() {
            @Override
            protected void executeTest(MobileServiceClient client, TestExecutionCallback callback) {

            }
        };

        String blobName = UUID.randomUUID().toString();
        test.setFileName(blobName);

        test.log("TestRun:" + mDaylightUrl + "/" + mDayLightProject + "/runs/" + runId);
        test.log("Passed:" + (testCount - failedTestCount));
        test.log("Failed:" + failedTestCount);
        test.log("TestCount:" + testCount);

        List<TestCase> result = new ArrayList<TestCase>();
        result.add(test);

        return result;
    }

    private JsonArray createMasterRunResult(int failedTestCount, Date startTime, Date endTime, String runId, String fileName) {
        JsonObject test = new JsonObject();
        test.addProperty("adapter", "zumotestsconverter");
        test.addProperty("name", mPlatform + " " + dateToString(startTime));
        test.addProperty("full_name", mPlatform + " " + dateToString(startTime));
        test.addProperty("source", "Android");
        test.addProperty("run_id", runId);
        test.addProperty("outcome", failedTestCount == 0 ? "Passed" : "Failed");
        test.addProperty("start_time", getFileTime(startTime));
        test.addProperty("end_time", getFileTime(endTime));

        JsonArray tags = new JsonArray();
        tags.add(new JsonPrimitive(mPlatform));
        test.add("tags", tags);

        JsonObject attach = new JsonObject();
        attach.addProperty("logs.txt", fileName);
        test.add("attachments", attach);

        JsonArray result = new JsonArray();
        result.add(test);

        return result;
    }

    private ListenableFuture<Void> postResult(String authAccessToken, JsonArray testResultArray) {
        String body = testResultArray.toString();
        String requestUrl = mDaylightUrl + "/api/zumo2/results?access_token=" + authAccessToken;

        HttpPost request = new HttpPost(requestUrl);

        try {
            request.setEntity(new StringEntity(body, "UTF-8"));
        } catch (UnsupportedEncodingException uee) {
        }

        final SettableFuture<Void> result = SettableFuture.create();

        ListenableFuture<HttpURLConnection> internalFuture = execute(request);

        Futures.addCallback(internalFuture, new FutureCallback<HttpURLConnection>() {
            @Override
            public void onFailure(Throwable throwable) {
                result.setException(throwable);
            }

            @Override
            public void onSuccess(HttpURLConnection connection) {
                try {
                    int statusCode = connection.getResponseCode();

                    if (statusCode == 200) {
                        result.set(null);
                    } else {
                        result.setException(new Exception("Invalid response status code " + statusCode));
                    }
                } catch (Throwable t) {
                    result.setException(t);
                }
            }
        });

        return result;
    }

    private JsonArray parseRunResult(String runId, List<TestCase> tests, Map<String, String> sourceMap) {
        JsonArray result = new JsonArray();

        for (TestCase testCase : tests) {
            String blobName = UUID.randomUUID().toString();
            testCase.setFileName(blobName);

            JsonObject test = new JsonObject();
            test.addProperty("adapter", "zumotestsconverter");
            test.addProperty("name", testCase.getName());
            test.addProperty("full_name", testCase.getName());
            test.addProperty("source", sourceMap.get(testCase.getName()).replace(' ', '_'));
            test.addProperty("run_id", runId);
            test.addProperty("outcome", testCase.getStatus().name());
            test.addProperty("start_time", getFileTime(testCase.getStartTime()));
            test.addProperty("end_time", getFileTime(testCase.getEndTime()));

            JsonArray tags = new JsonArray();
            tags.add(new JsonPrimitive(mPlatform));
            test.add("tags", tags);

            JsonObject attach = new JsonObject();
            attach.addProperty("logs.txt", testCase.getFileName());
            test.add("attachments", attach);

            result.add(test);
        }

        return result;
    }
}

 */