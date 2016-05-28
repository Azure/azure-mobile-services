package com.microsoft.windowsazure.mobileservices.table;

import java.util.Date;

/**
 * Created by marianosanchez on 3/13/15.
 */
public class DateTimeOffset extends Date {

    public DateTimeOffset(Date date) {
        this.setTime(date.getTime());
    }
}
