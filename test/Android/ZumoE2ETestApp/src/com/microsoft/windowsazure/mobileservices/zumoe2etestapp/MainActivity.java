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

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.net.MalformedURLException;
import java.net.URI;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;

import org.apache.http.client.methods.HttpPost;
import org.apache.http.entity.StringEntity;
import org.apache.http.impl.client.DefaultHttpClient;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.res.Configuration;
import android.os.AsyncTask;
import android.os.Bundle;
import android.os.Environment;
import android.preference.PreferenceManager;
import android.text.ClipboardManager;
import android.text.TextUtils;
import android.util.Log;
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
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.CustomApiTests;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.LoginTests;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.MiscTests;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.PushTests;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.QueryTests;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.RoundTripTests;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.UpdateDeleteTests;

@SuppressWarnings("deprecation")
public class MainActivity extends Activity {

	private StringBuilder mLog;

	private boolean mRunningAllTests;

	private SharedPreferences mPrefManager;

	private JsonObject mAutomationPreferences;

	private ListView mTestCaseList;
	
	private Spinner mTestGroupSpinner;

	private static Activity mInstance;
	
	private static final String automationPreferencesFile = "/zumo/automationPreferences.txt";

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

		try {
			String sdCard = Environment.getExternalStorageDirectory().getPath();
			FileInputStream fis = new FileInputStream(sdCard + automationPreferencesFile);
			InputStreamReader isr = new InputStreamReader(fis);
			BufferedReader br = new BufferedReader(isr);
			StringBuilder sb = new StringBuilder();
			String line = br.readLine();
			while (line != null) {
				sb.append(line);
				sb.append('\n');
				line = br.readLine();
			}

			JsonElement prefs = new JsonParser().parse(sb.toString());
			if (prefs.isJsonObject()) {
				this.mAutomationPreferences = prefs.getAsJsonObject();
			}

			br.close();
			isr.close();
			fis.close();

			// Remove the file so that it doesn't get picked up for manual runs
			File toBeDeleted = new File(sdCard + automationPreferencesFile);
			toBeDeleted.delete();
		} catch (IOException e) {
			e.printStackTrace();
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

		PushTests.mainActivity = this;
		
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
		mRunningAllTests = false;

		ArrayAdapter<TestGroup> adapter = (ArrayAdapter<TestGroup>) mTestGroupSpinner.getAdapter();
		adapter.clear();
		adapter.add(new RoundTripTests());
		adapter.add(new QueryTests());
		adapter.add(new UpdateDeleteTests());
		adapter.add(new LoginTests());
		adapter.add(new MiscTests());
		adapter.add(new PushTests());
		adapter.add(new CustomApiTests());

		ArrayList<TestCase> allTests = new ArrayList<TestCase>();
		ArrayList<TestCase> allUnattendedTests = new ArrayList<TestCase>();
		for (int i = 0; i < adapter.getCount(); i++) {
			TestGroup group = adapter.getItem(i);
			allTests.add(Util.createSeparatorTest("Start of group: " + group.getName()));
			allUnattendedTests.add(Util.createSeparatorTest("Start of group: " + group.getName()));
			
			List<TestCase> testsForGroup = group.getTestCases();
			for (TestCase test : testsForGroup) {
				allTests.add(test);
				if (test.canRunUnattended()) {
					allUnattendedTests.add(test);
				}
			}
			allTests.add(Util.createSeparatorTest("------------------"));
			allUnattendedTests.add(Util.createSeparatorTest("------------------"));
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
			if (getMobileServiceKey().trim() == "" || getMobileServiceURL().trim() == "") {
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
			final boolean isLogForAllGroups = mRunningAllTests;
			String logHtml = "<html><body><pre>" + logContent + "</pre></body></html>";
			webView.loadData(logHtml, "text/html", "utf-8");
			
			logDialogBuilder.setPositiveButton("Copy", new DialogInterface.OnClickListener() {

				@Override
				public void onClick(DialogInterface dialog, int which) {
					ClipboardManager clipboardManager = (ClipboardManager) getSystemService(CLIPBOARD_SERVICE);
					clipboardManager.setText(mLog.toString());
				}
			});
			
			final String postContent = mLog.toString();
			
			logDialogBuilder.setNeutralButton("Post data", new DialogInterface.OnClickListener() {
				
				@Override
				public void onClick(DialogInterface dialog, int which) {
					postLogs(postContent, isLogForAllGroups);
				}
			});

			logDialogBuilder.setView(webView);

			logDialogBuilder.create().show();
			return true;

		default:
			return super.onOptionsItemSelected(item);
		}
	}

	private void postLogs(final String logs, final boolean isLogForAllGroups) {
		new AsyncTask<Void, Void, Void>() {

			@Override
			protected Void doInBackground(Void... params) {
				try {
					String url = getLogPostURL();
					if (url != null && url.trim() != "") {
						url = url + "?platform=android";
						if (isLogForAllGroups) {
							url = url + "&allTests=true";
						}
						String clientVersion = Util.getGlobalTestParameters().get(TestGroup.ClientVersionKey);
						String runtimeVersion = Util.getGlobalTestParameters().get(TestGroup.ServerVersionKey);
						if (clientVersion != null) {
							url = url + "&clientVersion=" + clientVersion;
						}
						if (runtimeVersion != null) {
							url = url + "&runtimeVersion=" + runtimeVersion;
						}
						HttpPost post = new HttpPost();
						post.setEntity(new StringEntity(logs, MobileServiceClient.UTF8_ENCODING));

						post.setURI(new URI(url));

						new DefaultHttpClient().execute(post);
					}
				} catch (Exception e) {
					// Wasn't able to post the data. Do nothing
				}

				return null;
			}
		}.execute();
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
		MobileServiceClient client;

		try {
			client = createMobileServiceClient();
		} catch (MalformedURLException e) {
			createAndShowDialog(e, "Error");
			return;
		}

		TestGroup group = (TestGroup) mTestGroupSpinner.getSelectedItem();
		logWithTimestamp(new Date(), "Tests for group \'" + group.getName() + "\'");
		if (group.getName().startsWith(TestGroup.AllTestsGroupName)) {
			mRunningAllTests = true;
		}
		logSeparator();
		group.runTests(client, new TestExecutionCallback() {

			@Override
			public void onTestStart(TestCase test) {
				TestCaseAdapter adapter = (TestCaseAdapter) mTestCaseList.getAdapter();
				adapter.notifyDataSetChanged();
				// log("TEST START", test.getName());
			}

			@Override
			public void onTestGroupComplete(TestGroup group, List<TestResult> results) {
				log("TEST GROUP COMPLETED", group.getName() + " - " + group.getStatus().toString());
				logSeparator();

				if (shouldRunUnattended()) {
					String logContent = mLog.toString();
					postLogs(logContent, true);

					boolean passed = true;
					for (TestResult result : results) {
						if (result.getStatus() != TestStatus.Passed) {
							passed = false;
							break;
						}
					}

					try {
						String sdCard = Environment.getExternalStorageDirectory().getPath();
						FileOutputStream fos = new FileOutputStream(sdCard + "/zumo/done.txt");
						OutputStreamWriter osw = new OutputStreamWriter(fos);
						BufferedWriter bw = new BufferedWriter(osw);
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
						sb.append(" // ");
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

	private void logSeparator() {
		mLog.append("\n");
		mLog.append("----\n");
		mLog.append("\n");
	}
	
	private void log(String content) {
		log("Info", content);
	}

	private void logWithTimestamp(Date time, String content) {
		log("Info", "[" + Util.dateToString(time, Util.LogTimeFormat) + "] " + content);
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

	private String getMobileServiceKey() {
		return this.getPreference(Constants.PREFERENCE_MOBILE_SERVICE_KEY);
	}
	
	private String getLogPostURL() {
		return this.getPreference(Constants.PREFERENCE_LOG_POST_URL);
	}
	
	public String getGCMSenderId() {
		return this.getPreference(Constants.PREFERENCE_GCM_SENDER_ID);
	}

	private boolean shouldRunUnattended() {
		if (mAutomationPreferences != null) {
			if (mAutomationPreferences.has("pref_run_unattended")) {
				JsonElement runUnattended = mAutomationPreferences.get("pref_run_unattended");
				return runUnattended.getAsBoolean();
			}
		}

		return false;
	}

	private String getPreference(String key) {
		if (mAutomationPreferences != null && mAutomationPreferences.has(key)) {
			return mAutomationPreferences.get(key).getAsString();
		} else {
			return mPrefManager.getString(key, "");
		}
	}

	private MobileServiceClient createMobileServiceClient() throws MalformedURLException {
		String url = getMobileServiceURL();
		String key = getMobileServiceKey();

		MobileServiceClient client = new MobileServiceClient(url, key, this);

		return client;
	}

	/**
	 * Creates a dialog and shows it
	 * 
	 * @param exception
	 *            The exception to show in the dialog
	 * @param title
	 *            The dialog title
	 */
	private void createAndShowDialog(Exception exception, String title) {
		createAndShowDialog(exception.toString(), title);
	}

	/**
	 * Creates a dialog and shows it
	 * 
	 * @param message
	 *            The dialog message
	 * @param title
	 *            The dialog title
	 */
	private void createAndShowDialog(String message, String title) {
		AlertDialog.Builder builder = new AlertDialog.Builder(this);

		builder.setMessage(message);
		builder.setTitle(title);
		builder.create().show();
	}
}