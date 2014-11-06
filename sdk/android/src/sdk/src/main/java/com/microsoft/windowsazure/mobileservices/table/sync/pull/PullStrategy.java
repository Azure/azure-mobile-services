package com.microsoft.windowsazure.mobileservices.table.sync.pull;

import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceSystemColumns;
import com.microsoft.windowsazure.mobileservices.table.query.Query;

import java.util.ArrayList;
import java.util.Date;
import java.util.List;

/**
 * Created by marianosanchez on 11/3/14.
 */
public class PullStrategy {

    int defaultTop = 50;

    Query query;
    boolean supportSkip;
    boolean supportTop;
    PullCursor cursor;

    public PullStrategy(Query query, PullCursor cursor) {
        this.query = query;
        this.supportSkip = true; //query.getSkip() > 0;
        this.supportTop = true; //query.getTop() > 0;
        this.cursor = cursor;
    }

    public void initialize() {
        if (this.supportTop) {
            //if (this.supportSkip) // mongo requires skip if top is given but table storage doesn't support skip
            //{
            //    this.Query.Skip = this.Query.Skip.GetValueOrDefault();
            //}

            // always download in batches of 50 or less for efficiency

            if (this.query.getTop() == 0) {
                this.query.top(defaultTop);
            } else {
                this.query.top(Math.min(this.query.getTop(), defaultTop));
            }
        }
    }

    public void onResultsProcessed(JsonArray elements) {
        return;
    }

    public boolean moveToNextPage(int lastElementCount) {

        if (!this.supportSkip)
            return false;

        if (cursor.getComplete())
            return false;

        if (lastElementCount < this.query.getTop())
            return false;

        // then we continue downloading the changes using skip and top
        this.query.skip(this.cursor.getPosition());
        this.reduceTop();

        return true;
    }

    void reduceTop() {
        if (!this.supportTop)
            return;

        // only read as many as we want
        this.query.top(Math.min(this.query.getTop(), cursor.getRemaining()));
    }

    public void pullComplete() {
        return;
    }
}