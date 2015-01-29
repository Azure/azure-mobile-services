package com.microsoft.windowsazure.mobileservices.table.sync.pull;

import com.google.gson.JsonArray;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceSystemColumns;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceSystemProperty;
import com.microsoft.windowsazure.mobileservices.table.query.Query;
import com.microsoft.windowsazure.mobileservices.table.query.QueryOrder;

import java.util.EnumSet;

/**
 * Created by marianosanchez on 11/3/14.
 */
public class PullStrategy {

    int defaultTop = 50;

    Query query;
    PullCursor cursor;
    MobileServiceJsonTable table;
    public PullStrategy(Query query, MobileServiceJsonTable table) {

        this.query = query;
        this.table = table;
    }

    public PullCursor getCursor() {
        return cursor;
    }

    public void initialize() {

        table.setSystemProperties(EnumSet.noneOf(MobileServiceSystemProperty.class));
        table.setSystemProperties(EnumSet.of(MobileServiceSystemProperty.Deleted, MobileServiceSystemProperty.UpdatedAt));

        if (this.query.getTop() == 0) {
            this.query.top(defaultTop);
        } else {
            this.query.top(Math.min(this.query.getTop(), defaultTop));
        }


        if (query.getOrderBy().size() == 0) {
            this.query.orderBy(MobileServiceSystemColumns.Id, QueryOrder.Ascending);
        }

        if (this.query.getSkip() < 0) {
            this.query.skip(0);
        }

        cursor = new PullCursor(query.getTop(), query.getSkip());
    }

    public void onResultsProcessed(JsonArray elements) {
        return;
    }

    public boolean moveToNextPage(int lastElementCount) {

        if (cursor.getComplete())
            return false;

        if (lastElementCount < this.query.getTop())
            return false;

        this.query.skip(this.cursor.getPosition());
        this.reduceTop();

        return true;
    }

    protected void reduceTop() {
        this.query.top(Math.min(this.query.getTop(), cursor.getRemaining()));
    }

    public Query getLastQuery() {
        return this.query;
    }
}
