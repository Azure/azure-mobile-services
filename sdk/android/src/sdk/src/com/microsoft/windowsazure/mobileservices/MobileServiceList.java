package com.microsoft.windowsazure.mobileservices;

import java.util.ArrayList;
import java.util.Collection;

public class MobileServiceList<E> extends ArrayList<E> {

    private static final long serialVersionUID = 5772338570723574845L;
    private int mTotalCount;

    public MobileServiceList(Collection<? extends E> collection, int totalCount) {
        super(collection);
        mTotalCount = totalCount;
    }
    
    public int getTotalCount() {
        return mTotalCount;
    }
}
