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

/**
 * MobileServicePushStatus.java
 */
package com.microsoft.windowsazure.mobileservices.table.sync.push;

import android.annotation.SuppressLint;

import java.util.HashMap;
import java.util.Map;

/**
 * Enumeration for push sync status.
 */
@SuppressLint("UseSparseArrays")
public enum MobileServicePushStatus {

    /**
     * All table operations in the push action were completed, possibly with
     * errors.
     */
    Complete(0),

    /**
     * Push was aborted due to network error.
     */
    CancelledByNetworkError(1),

    /**
     * Push was aborted due to authentication error.
     */
    CancelledByAuthenticationError(2),

    /**
     * Push was aborted due to error from local store.
     */
    CancelledByLocalStoreError(3),

    /**
     * Push failed due to an internal error.
     */
    InternalError(Integer.MAX_VALUE);
    private static final Map<Integer, MobileServicePushStatus> mValuesMap;
    static {
        mValuesMap = new HashMap<Integer, MobileServicePushStatus>(4);
        mValuesMap.put(0, MobileServicePushStatus.Complete);
        mValuesMap.put(1, MobileServicePushStatus.CancelledByNetworkError);
        mValuesMap.put(2, MobileServicePushStatus.CancelledByAuthenticationError);
        mValuesMap.put(3, MobileServicePushStatus.CancelledByLocalStoreError);
        mValuesMap.put(Integer.MAX_VALUE, MobileServicePushStatus.InternalError);
    }
    private final int mValue;

    private MobileServicePushStatus(int value) {
        this.mValue = value;
    }

    /**
     * Return the MobileServicePushStatus with the provided int value
     *
     * @param value the int value
     * @return the matching MobileServicePushStatus
     */
    public static MobileServicePushStatus parse(int value) {
        return mValuesMap.get(value);
    }

    /**
     * Return the int value associated to the enum
     */
    public int getValue() {
        return this.mValue;
    }
}
