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

import android.app.Activity;
import android.content.Context;
import android.graphics.Color;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.CheckBox;

import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestCase;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestStatus;

/**
 * Adapter to bind a ToDoItem List to a view
 */
public class TestCaseAdapter extends ArrayAdapter<TestCase> {

    /**
     * Adapter context
     */
    Context mContext;

    /**
     * Adapter View layout
     */
    int mLayoutResourceId;

    public TestCaseAdapter(Context context, int layoutResourceId) {
        super(context, layoutResourceId);

        mContext = context;
        mLayoutResourceId = layoutResourceId;
    }

    /**
     * Returns the view for a specific item on the list
     */
    @Override
    public View getView(final int position, View convertView, ViewGroup parent) {
        View row = convertView;

        final TestCase testCase = getItem(position);

        if (row == null) {
            LayoutInflater inflater = ((Activity) mContext).getLayoutInflater();
            row = inflater.inflate(mLayoutResourceId, parent, false);
        }

        final CheckBox checkBox = (CheckBox) row.findViewById(R.id.checkTestCase);

        String text = String.format("%s - %s", testCase.getName(), testCase.getStatus().toString());

        if (testCase.getStatus() == TestStatus.Failed) {
            checkBox.setTextColor(Color.RED);
        } else if (testCase.getStatus() == TestStatus.Passed) {
            checkBox.setTextColor(Color.GREEN);
        } else if (testCase.getStatus() == TestStatus.MissingFeatures) {
            checkBox.setTextColor(Color.YELLOW);
        } else {
            checkBox.setTextColor(Color.BLACK);
        }

        checkBox.setText(text);
        checkBox.setChecked(testCase.isEnabled());

        checkBox.setOnClickListener(new View.OnClickListener() {

            @Override
            public void onClick(View v) {
                testCase.setEnabled(checkBox.isChecked());
            }
        });

        return row;
    }

}
