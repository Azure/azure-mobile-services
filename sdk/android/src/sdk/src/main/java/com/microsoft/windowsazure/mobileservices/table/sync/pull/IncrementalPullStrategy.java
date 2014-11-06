package com.microsoft.windowsazure.mobileservices.table.sync.pull;

import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceSystemColumns;
import com.microsoft.windowsazure.mobileservices.table.query.Query;
import com.microsoft.windowsazure.mobileservices.table.query.QueryNode;
import com.microsoft.windowsazure.mobileservices.table.query.QueryOperations;
import com.microsoft.windowsazure.mobileservices.table.query.QueryOrder;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.ColumnDataType;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.MobileServiceLocalStore;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.MobileServiceLocalStoreException;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.HashMap;
import java.util.Map;
import java.util.TimeZone;

/**
 * Created by marianosanchez on 11/3/14.
 */
public class IncrementalPullStrategy extends PullStrategy {

    private static final String INCREMENTAL_PULL_STRATEGY_TABLE = "__incrementalPullData";

    private MobileServiceLocalStore mStore;
    private Date maxUpdatedAt;
    private String lastElementId;
    private Date deltaToken;
    private String queryKey;
    private QueryNode originalFilter;

    public IncrementalPullStrategy(Query query, String queryKey, PullCursor cursor, MobileServiceLocalStore localStore) {
        super(query, cursor);
        this.mStore = localStore;
        this.queryKey = queryKey;

        originalFilter = query.getQueryNode();

    }

    public void initialize() {

        JsonElement results = null;

        try {
            results = mStore.read(
                    QueryOperations.tableName(INCREMENTAL_PULL_STRATEGY_TABLE)
                            .field("id")
                            .eq(query.getTableName() + "_" + queryKey));


            JsonArray resultsArray = results.getAsJsonArray();

            if (resultsArray.size() > 0) {
                JsonElement result = resultsArray.get(0);

                String stringMaxUpdatedDate = result.getAsJsonObject()
                        .get("maxupdateddate").getAsString();

                deltaToken = maxUpdatedAt = getDateFromString(stringMaxUpdatedDate);

                setMaxUpdatedAtToQuery(maxUpdatedAt, null);
            }

        } catch (MobileServiceLocalStoreException e) {

        }

        super.initialize();
    }

    public void onResultsProcessed(JsonArray elements) {

        if (elements.size() <= 0) {
            return;
        }

        JsonObject lastElement = elements.get(elements.size() - 1).getAsJsonObject();

        String lastElementUpdatedAt = lastElement.get(MobileServiceSystemColumns.UpdatedAt).getAsString();

        maxUpdatedAt = getDateFromString(lastElementUpdatedAt);

        lastElementId =  lastElement.get(MobileServiceSystemColumns.Id).getAsString();

        saveMaxUpdatedDate(lastElementUpdatedAt);
    }

    public boolean moveToNextPage(int lastElementCount) {

        if (maxUpdatedAt.after(deltaToken)) {

            deltaToken = maxUpdatedAt;

            // reset the cursor because deltatoken has changed
            this.cursor.reset();

            if (this.supportSkip)
            {
                this.query.skip(0);
            }

            this.reduceTop();

            setMaxUpdatedAtToQuery(maxUpdatedAt, lastElementId);

            return true;
        }

        return super.moveToNextPage(lastElementCount);
    }


    public void pullComplete() {
        return;
    }

    private void saveMaxUpdatedDate(String lastElementUpdatedAt) {

        JsonObject updatedElement = new JsonObject();

        updatedElement.addProperty("id", query.getTableName() + "_" + queryKey);
        updatedElement.addProperty("maxUpdatedDate", lastElementUpdatedAt);

        try {
            mStore.upsert(INCREMENTAL_PULL_STRATEGY_TABLE, updatedElement);
        } catch (MobileServiceLocalStoreException e) {
            e.printStackTrace();
        }
    }

    private void setMaxUpdatedAtToQuery(Date maxUpdatedAt, String lastItemId) {

        if (query.getOrderBy().size() > 0) {
            this.query.getOrderBy().clear();
        }

        this.query.orderBy(MobileServiceSystemColumns.UpdatedAt, QueryOrder.Ascending);
        this.query.orderBy(MobileServiceSystemColumns.Id, QueryOrder.Ascending);

        this.query.setQueryNode(originalFilter);

        this.query.field(MobileServiceSystemColumns.UpdatedAt)
                .gt(this.maxUpdatedAt);


        if (lastItemId != null) {

            Query maxUpdatedAndIdFilter = QueryOperations.field(MobileServiceSystemColumns.UpdatedAt)
                    .ge(this.maxUpdatedAt)
                    .and()
                    .field(MobileServiceSystemColumns.Id)
                    .gt(lastItemId);

            this.query.or(maxUpdatedAndIdFilter);
        }
    }

    public static void initializeStore(MobileServiceLocalStore store) throws MobileServiceLocalStoreException {

        Map<String, ColumnDataType> columns = new HashMap<String, ColumnDataType>();
        columns.put("id", ColumnDataType.String);
        columns.put("maxupdateddate", ColumnDataType.String);

        store.defineTable(INCREMENTAL_PULL_STRATEGY_TABLE, columns);
    }

    private Date getDateFromString(String stringValue) {

        if (stringValue == null) {
            return null;
        }

        SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'");
        sdf.setTimeZone(TimeZone.getTimeZone("UTC"));

        try {
           return sdf.parse(stringValue);
        } catch (ParseException e) {
            return null;
        }
    }
}