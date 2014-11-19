package com.microsoft.windowsazure.mobileservices.table.sync.pull;

import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceSystemColumns;
import com.microsoft.windowsazure.mobileservices.table.query.Query;
import com.microsoft.windowsazure.mobileservices.table.query.QueryOperations;
import com.microsoft.windowsazure.mobileservices.table.query.QueryOrder;

import java.util.ArrayList;
import java.util.Date;
import java.util.List;

/**
 * Created by marianosanchez on 11/3/14.
 */
public class PullStrategy {

    int defaultTop = 50;

    Query query;
    PullCursor cursor;

    public PullStrategy(Query query, PullCursor cursor) {

        this.query = query;
        this.cursor = cursor;
    }

    public void initialize() {

        if (this.query.getTop() == 0) {
            this.query.top(defaultTop);
        } else {
            this.query.top(Math.min(this.query.getTop(), defaultTop));
        }


        if (query.getOrderBy().size() == 0) {
            this.query.orderBy(MobileServiceSystemColumns.Id, QueryOrder.Ascending);
        }
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