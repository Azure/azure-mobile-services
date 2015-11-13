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
package com.microsoft.windowsazure.mobileservices.zumoe2etestapp;

import android.annotation.SuppressLint;
import android.annotation.TargetApi;
import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.res.Configuration;
import android.os.AsyncTask;
import android.os.Build;
import android.os.Bundle;
import android.os.Environment;
import android.preference.PreferenceManager;
import android.text.ClipboardManager;
import android.text.TextUtils;
import android.util.Log;
import android.util.Pair;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.webkit.WebView;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.ListView;
import android.widget.Spinner;

import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.CompositeTestGroup;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestCase;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestExecutionCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestGroup;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestResult;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestStatus;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.Util;
//import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.log.DaylightLogger;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.ClientSDKLoginTests;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.CustomApiTests;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.EnhancedPushTests;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.LoginTests;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.MiscTests;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.OfflineTests;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.PushTests;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.QueryTests;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.RoundTripTests;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.SystemPropertiesTests;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.UpdateDeleteTests;
import com.squareup.okhttp.OkHttpClient;
import com.squareup.okhttp.Request;
import com.squareup.okhttp.Response;
import com.squareup.okhttp.internal.http.StatusLine;

import java.io.BufferedWriter;
import java.io.ByteArrayOutputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.OutputStreamWriter;
import java.net.MalformedURLException;
import java.util.ArrayList;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

//import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.EnhancedPushTests;
//import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.LoginTests;
//import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.MiscTests;
//import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.PushTests;
//import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.OfflineTests;
//import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.SystemPropertiesTests;

@TargetApi(Build.VERSION_CODES.HONEYCOMB)
@SuppressWarnings("deprecation")
public class MainActivity extends Activity {

    private static Activity mInstance;
    private StringBuilder mLog;
    private SharedPreferences mPrefManager;
    private Map<String, String> mAutomationPreferences;
    private ListView mTestCaseList;
    private Spinner mTestGroupSpinner;

    public static Activity getInstance() {
        return mInstance;
    }

    @Override
    public void onConfigurationChanged(Configuration newConfig) {
        // don't restart the activity. Just process the configuration change
        super.onConfigurationChanged(newConfig);
    }

    @SuppressLint("WorldReadableFiles")
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        setContentView(R.layout.activity_main);

        mInstance = this;

        mPrefManager = PreferenceManager.getDefaultSharedPreferences(this);

        Bundle extras = getIntent().getExtras();
        if (extras != null) {
            mAutomationPreferences = new HashMap<String, String>();
            mAutomationPreferences.put("pref_run_unattended", extras.getString("pref_run_unattended", ""));
            mAutomationPreferences.put("pref_mobile_service_url", extras.getString("pref_mobile_service_url", ""));
            mAutomationPreferences.put("pref_mobile_service_key", extras.getString("pref_mobile_service_key", ""));
            mAutomationPreferences.put("pref_google_userid", extras.getString("pref_google_userid", ""));
            mAutomationPreferences.put("pref_google_webapp_clientid", extras.getString("pref_google_webapp_clientid", ""));
            mAutomationPreferences.put("pref_master_run_id", extras.getString("pref_master_run_id", ""));
            mAutomationPreferences.put("pref_runtime_version", extras.getString("pref_runtime_version", ""));
            mAutomationPreferences.put("pref_daylight_client_id", extras.getString("pref_client_id", ""));
            mAutomationPreferences.put("pref_daylight_client_secret", extras.getString("pref_client_secret", ""));
            mAutomationPreferences.put("pref_daylight_url", extras.getString("pref_daylight_url", ""));
            mAutomationPreferences.put("pref_daylight_project", extras.getString("pref_daylight_project", ""));
        }

        mTestCaseList = (ListView) findViewById(R.id.testCaseList);
        TestCaseAdapter testCaseAdapter = new TestCaseAdapter(this, R.layout.row_list_test_case);
        mTestCaseList.setAdapter(testCaseAdapter);

        mTestGroupSpinner = (Spinner) findViewById(R.id.testGroupSpinner);

        ArrayAdapter<TestGroup> testGroupAdapter = new ArrayAdapter<TestGroup>(this, android.R.layout.simple_spinner_item);
        mTestGroupSpinner.setAdapter(testGroupAdapter);
        mTestGroupSpinner.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {

            @Override
            public void onItemSelected(AdapterView<?> parent, View view, int pos, long id) {
                selectTestGroup(pos);
            }

            @Override
            public void onNothingSelected(AdapterView<?> arg0) {
                // do nothing
            }
        });

        ClientSDKLoginTests.mainActivity = this;

        PushTests.mainActivity = this;
        EnhancedPushTests.mainActivity = this;

        refreshTestGroupsAndLog();
    }

    private void selectTestGroup(int pos) {
        TestGroup tg = (TestGroup) mTestGroupSpinner.getItemAtPosition(pos);
        List<TestCase> testCases = tg.getTestCases();

        fillTestList(testCases);
    }

    @SuppressWarnings("unchecked")
    private void refreshTestGroupsAndLog() {
        mLog = new StringBuilder();

        Thread thread = new Thread() {

            @Override
            public void run() {

                final boolean isNetBackend = IsNetBackend();

                runOnUiThread(new Runnable() {

                    @Override
                    public void run() {

                        ArrayAdapter<TestGroup> adapter = (ArrayAdapter<TestGroup>) mTestGroupSpinner.getAdapter();
                        adapter.clear();
                        adapter.add(new RoundTripTests());
                        adapter.add(new QueryTests());
                        adapter.add(new UpdateDeleteTests());
                        //adapter.add(new ClientSDKLoginTests());
                        adapter.add(new LoginTests(isNetBackend));
                        adapter.add(new MiscTests());
                        // adapter.add(new PushTests());
                        adapter.add(new CustomApiTests());
                        adapter.add(new SystemPropertiesTests(isNetBackend));
                        adapter.add(new EnhancedPushTests(isNetBackend));
                        adapter.add(new OfflineTests());

                        ArrayList<Pair<TestCase, String>> allTests = new ArrayList<Pair<TestCase, String>>();
                        ArrayList<Pair<TestCase, String>> allUnattendedTests = new ArrayList<Pair<TestCase, String>>();
                        for (int i = 0; i < adapter.getCount(); i++) {
                            TestGroup group = adapter.getItem(i);
                            allTests.add(new Pair<TestCase, String>(Util.createSeparatorTest("Start of group: " + group.getName()), "Separator"));
                            allUnattendedTests.add(new Pair<TestCase, String>(Util.createSeparatorTest("Start of group: " + group.getName()), "Separator"));

                            List<TestCase> testsForGroup = group.getTestCases();
                            for (TestCase test : testsForGroup) {
                                allTests.add(new Pair<TestCase, String>(test, group.getName()));
                                if (test.canRunUnattended()) {
                                    allUnattendedTests.add(new Pair<TestCase, String>(test, group.getName()));
                                }
                            }
                            allTests.add(new Pair<TestCase, String>(Util.createSeparatorTest("----" + group.getName() + "----"), "Separator"));
                            allUnattendedTests.add(new Pair<TestCase, String>(Util.createSeparatorTest("----" + group.getName() + "----"), "Separator"));
                        }

                        int unattendedTestsIndex = adapter.getCount();

                        adapter.add(new CompositeTestGroup(TestGroup.AllUnattendedTestsGroupName, allUnattendedTests));
                        adapter.add(new CompositeTestGroup(TestGroup.AllTestsGroupName, allTests));

                        if (shouldRunUnattended()) {
                            mTestGroupSpinner.setSelection(unattendedTestsIndex);
                            selectTestGroup(unattendedTestsIndex);
                            changeCheckAllTests(true);
                            runTests();
                        } else {
                            mTestGroupSpinner.setSelection(0);
                            selectTestGroup(0);
                        }
                    }
                });
            }
        };

        thread.start();
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.activity_main, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
            case R.id.menu_settings:
                startActivity(new Intent(this, ZumoPreferenceActivity.class));
                return true;

            case R.id.menu_run_tests:
                if (getMobileServiceURL().trim() == "") {
                    startActivity(new Intent(this, ZumoPreferenceActivity.class));
                } else {
                    runTests();
                }
                return true;

            case R.id.menu_check_all:
                changeCheckAllTests(true);
                return true;

            case R.id.menu_uncheck_all:
                changeCheckAllTests(false);
                return true;

            case R.id.menu_reset:
                refreshTestGroupsAndLog();
                return true;

            case R.id.menu_view_log:
                AlertDialog.Builder logDialogBuilder = new AlertDialog.Builder(this);
                logDialogBuilder.setTitle("Log");

                final WebView webView = new WebView(this);

                String logContent = TextUtils.htmlEncode(mLog.toString()).replace("\n", "<br />");
                String logHtml = "<html><body><pre>" + logContent + "</pre></body></html>";
                webView.loadData(logHtml, "text/html", "utf-8");

                logDialogBuilder.setPositiveButton("Copy", new DialogInterface.OnClickListener() {

                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        ClipboardManager clipboardManager = (ClipboardManager) getSystemService(CLIPBOARD_SERVICE);
                        clipboardManager.setText(mLog.toString());
                    }
                });

                logDialogBuilder.setView(webView);

                logDialogBuilder.create().show();
                return true;

            default:
                return super.onOptionsItemSelected(item);
        }
    }

    private void changeCheckAllTests(boolean check) {
        TestGroup tg = (TestGroup) mTestGroupSpinner.getSelectedItem();
        List<TestCase> testCases = tg.getTestCases();

        for (TestCase testCase : testCases) {
            testCase.setEnabled(check);
        }

        fillTestList(testCases);
    }

    private void fillTestList(List<TestCase> testCases) {
        TestCaseAdapter testCaseAdapter = (TestCaseAdapter) mTestCaseList.getAdapter();

        testCaseAdapter.clear();
        for (TestCase testCase : testCases) {
            testCaseAdapter.add(testCase);
        }
    }

    private void runTests() {

        MobileServiceClient client = null;

        try {
            client = createMobileServiceClient();
        } catch (MalformedURLException e) {
            createAndShowDialog(e, "Error");
        }

        // getMobileServiceRuntimeFeatures(client);

        final TestGroup group = (TestGroup) mTestGroupSpinner.getSelectedItem();
        logWithTimestamp(new Date(), "Tests for group \'" + group.getName() + "\'");

        logSeparator();

        final MobileServiceClient currentClient = client;

        if (Build.VERSION.SDK_INT == Build.VERSION_CODES.ICE_CREAM_SANDWICH_MR1 || Build.VERSION.SDK_INT == Build.VERSION_CODES.ICE_CREAM_SANDWICH) {
            // For android versions 4.0.x
            // Run a first Void AsyncTask on UI thread to enable the possibility
            // of running others on sub threads
            new AsyncTask<Void, Void, Void>() {
                @Override
                protected Void doInBackground(Void... params) {
                    return null;
                }
            }.execute();

        }

        Thread thread = new Thread() {

            @Override
            public void run() {
                group.runTests(currentClient, new TestExecutionCallback() {

                    @Override
                    public void onTestStart(TestCase test) {
                        final TestCaseAdapter adapter = (TestCaseAdapter) mTestCaseList.getAdapter();

                        runOnUiThread(new Runnable() {

                            @Override
                            public void run() {
                                adapter.notifyDataSetChanged();
                            }

                        });

                        log("TEST START", test.getName());
                    }

                    @Override
                    public void onTestGroupComplete(TestGroup group, List<TestResult> results) {
                        log("TEST GROUP COMPLETED", group.getName() + " - " + group.getStatus().toString());
                        logSeparator();

                        if (group.getName().startsWith(TestGroup.AllTestsGroupName)) {

                            List<TestCase> tests = new ArrayList<TestCase>();

                            for (TestResult result : results) {
                                tests.add(result.getTestCase());
                            }

                            /*DaylightLogger logger = new DaylightLogger(getDaylightURL(), getDaylightProject(), getDaylightClientId(),
                                    getDaylightClientSecret(), getDaylightRuntime(), getDaylightRunId());
                            try {
                                logger.reportResultsToDaylight(group.getFailedTestCount(), group.getStartTime(), group.getEndTime(), tests,
                                        group.getSourceMap());
                            } catch (Throwable e) {
                                log(e.getMessage());
                            }*/
                        }

                        if (shouldRunUnattended()) {
                            // String logContent = mLog.toString();
                            // postLogs(logContent, true);

                            boolean passed = true;
                            for (TestResult result : results) {
                                if (result.getStatus() != TestStatus.Passed) {
                                    passed = false;
                                    break;
                                }
                            }

                            try {
                                String sdCard = Environment.getExternalStorageDirectory().getPath();
                                FileOutputStream fos = new FileOutputStream(sdCard + "/done_android_e2e.txt");
                                OutputStreamWriter osw = new OutputStreamWriter(fos);
                                BufferedWriter bw = new BufferedWriter(osw);
                                bw.write("Completed successfully.\n");
                                bw.write(passed ? "PASSED" : "FAILED");
                                bw.write("\n");
                                bw.close();
                                osw.close();
                                fos.close();
                            } catch (IOException e) {
                                e.printStackTrace();
                            }
                        }
                    }

                    @Override
                    public void onTestComplete(TestCase test, TestResult result) {
                        Throwable e = result.getException();
                        if (e != null) {
                            StringBuilder sb = new StringBuilder();
                            while (e != null) {
                                sb.append(e.getClass().getSimpleName() + ": ");
                                sb.append(e.getMessage());
                                sb.append("\n");
                                sb.append(Log.getStackTraceString(e));
                                sb.append("\n\n");
                                e = e.getCause();
                            }

                            test.log("Exception: " + sb.toString());
                        }

                        final TestCaseAdapter adapter = (TestCaseAdapter) mTestCaseList.getAdapter();

                        runOnUiThread(new Runnable() {

                            @Override
                            public void run() {
                                adapter.notifyDataSetChanged();

                            }

                        });
                        logWithTimestamp(test.getStartTime(), "Logs for test " + test.getName() + " (" + result.getStatus().toString() + ")");
                        String testLogs = test.getLog();
                        if (testLogs.length() > 0) {
                            if (testLogs.endsWith("\n")) {
                                testLogs = testLogs.substring(0, testLogs.length() - 1);
                            }
                            log(testLogs);
                        }

                        logWithTimestamp(test.getEndTime(), "Test " + result.getStatus().toString());
                        logWithTimestamp(test.getEndTime(), "-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-");
                        logSeparator();
                    }
                });
            }
        };

        thread.start();
    }

    // private static List<Pair<String, String>> mobileServiceRuntimeFeatures;

    // private void getMobileServiceRuntimeFeatures(MobileServiceClient client)
    // {
    // mobileServiceRuntimeFeatures = new ArrayList<Pair<String, String>>();
    //
    // Pair<String, String> runtimeFeature1 = new Pair<String, String>("1",
    // "1");
    //
    // mobileServiceRuntimeFeatures.add(runtimeFeature1);
    // }
    //
    // public boolean mobileServiceRuntimeHasFeature(String featureKey, String
    // featureValue) {
    //
    // for (Pair<String, String> runtimeFeature : mobileServiceRuntimeFeatures)
    // {
    // if (runtimeFeature.first.equals(featureKey) &&
    // runtimeFeature.second.equals(featureValue)) {
    // return true;
    // }
    // }
    //
    // return false;
    // }

    private void logSeparator() {
        mLog.append("\n");
        mLog.append("----\n");
        mLog.append("\n");
    }

    private void log(String content) {
        log("Info", content);
    }

    private void logWithTimestamp(Date time, String content) {
        // log("Info", "[" + Util.dateToString(time, Util.LogTimeFormat) + "] "
        // + content);
    }

    private void log(String title, String content) {
        String message = title + " - " + content;
        Log.d("ZUMO-E2ETESTAPP", message);

        mLog.append(content);
        mLog.append('\n');
    }

    private String getMobileServiceURL() {
        return this.getPreference(Constants.PREFERENCE_MOBILE_SERVICE_URL);
    }

    private String getDaylightURL() {
        return this.getPreference(Constants.PREFERENCE_DAYLIGHT_URL);
    }

    private String getDaylightProject() {
        return this.getPreference(Constants.PREFERENCE_DAYLIGHT_PROJECT);
    }

    private String getDaylightClientId() {
        return this.getPreference(Constants.PREFERENCE_DAYLIGHT_CLIENT_ID);
    }

    private String getDaylightClientSecret() {
        return this.getPreference(Constants.PREFERENCE_DAYLIGHT_CLIENT_SECRET);
    }

    private String getDaylightRuntime() {
        return this.getPreference(Constants.PREFERENCE_RUNTIME_VERSION);
    }

    private String getDaylightRunId() {
        return this.getPreference(Constants.PREFERENCE_MASTER_RUN_ID);
    }

    public String getGoogleUserId() {
        return this.getPreference(Constants.PREFERENCE_GOOGLE_USERID);
    }

    public String getGCMSenderId() {
        return this.getPreference(Constants.PREFERENCE_GCM_SENDER_ID);
    }

    public String getGoogleWebAppClientId() {
        return this.getPreference(Constants.PREFERENCE_GOOGLE_WEBAPP_CLIENTID);
    }

    private boolean shouldRunUnattended() {
        if (mAutomationPreferences != null) {
            if (mAutomationPreferences.containsKey("pref_run_unattended")) {
                String runUnattended = mAutomationPreferences.get("pref_run_unattended");
                return runUnattended.equals("true");
            }
        }

        return false;
    }

    private String getPreference(String key) {
        if (mAutomationPreferences != null && mAutomationPreferences.containsKey(key)) {
            return mAutomationPreferences.get(key);
        } else {
            return mPrefManager.getString(key, "");
        }
    }

    private MobileServiceClient createMobileServiceClient() throws MalformedURLException {
        String url = getMobileServiceURL();

        MobileServiceClient client = new MobileServiceClient(url, this);

        return client;
    }

    /**
     * Creates a dialog and shows it
     *
     * @param exception The exception to show in the dialog
     * @param title     The dialog title
     */
    private void createAndShowDialog(Exception exception, String title) {
        createAndShowDialog(exception.toString(), title);
    }

    /**
     * Creates a dialog and shows it
     *
     * @param message The dialog message
     * @param title   The dialog title
     */
    private void createAndShowDialog(String message, String title) {
        AlertDialog.Builder builder = new AlertDialog.Builder(this);

        builder.setMessage(message);
        builder.setTitle(title);
        builder.create().show();
    }

    private boolean IsNetBackend() {

        try {

            OkHttpClient httpclient = new OkHttpClient();

            Request request = new Request.Builder()
                    .url(getMobileServiceURL() + "api/runtimeinfo")
                    .addHeader("ZUMO-API-VERSION", "2.0.0")
                    .build();

            Response response = httpclient.newCall(request).execute();

            String runtimeType;

            if (response.code() == 200) {
                ByteArrayOutputStream out = new ByteArrayOutputStream();
                String responseString = response.body().string();

                JsonObject jsonResult = new JsonParser().parse(responseString).getAsJsonObject();

                runtimeType = jsonResult.get("runtime").getAsJsonObject().get("type").getAsString();

                out.close();
            } else {
                response.body().close();
                throw new IOException(String.valueOf(response.code()));
            }

            if (runtimeType.equals(".NET")) {
                return true;
            }

            return false;
        }
        catch(Exception ex) {
            return false;
        }
    }
}