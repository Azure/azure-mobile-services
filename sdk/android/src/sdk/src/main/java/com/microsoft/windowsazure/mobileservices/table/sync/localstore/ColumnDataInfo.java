package com.microsoft.windowsazure.mobileservices.table.sync.localstore;

/**
 * Created by marianosanchez on 1/14/15.
 */
public class ColumnDataInfo {

    ColumnDataType mColumnDataType;

    String mOriginalName;


    public String getOriginalName() {
        return mOriginalName;
    }

    public ColumnDataType getColumnDataType() {
        return mColumnDataType;
    }

    public ColumnDataInfo(ColumnDataType columnDataType, String originalName) {
        mColumnDataType = columnDataType;
        mOriginalName = originalName;
    }
}
