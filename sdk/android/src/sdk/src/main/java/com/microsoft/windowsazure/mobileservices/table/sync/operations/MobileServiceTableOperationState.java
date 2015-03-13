package com.microsoft.windowsazure.mobileservices.table.sync.operations;

import java.util.HashMap;
import java.util.Map;

/**
 * Created by marianosanchez on 3/10/15.
 */
public enum MobileServiceTableOperationState
{
    /**
     * Pending
     */
    Pending(0),

    /**
     * Attempted
     */
    Attempted(1),

    /**
     * Failed
     */
    Failed(2);
    private static final Map<Integer, MobileServiceTableOperationState> mValuesMap;
    static {
        mValuesMap = new HashMap<Integer, MobileServiceTableOperationState>(3);
        mValuesMap.put(0, MobileServiceTableOperationState.Pending);
        mValuesMap.put(1, MobileServiceTableOperationState.Attempted);
        mValuesMap.put(2, MobileServiceTableOperationState.Failed);
    }
    private final int mValue;

    private MobileServiceTableOperationState(int value) {
        this.mValue = value;
    }

    /**
     * Return the MobileServiceTableOperationState with the provided int value
     *
     * @param value the int value
     * @return the matching MobileServiceTableOperationState
     */
    public static MobileServiceTableOperationState parse(int value) {
        return mValuesMap.get(value);
    }

    /**
     * Return the int value associated to the enum
     */
    public int getValue() {
        return this.mValue;
    }
}