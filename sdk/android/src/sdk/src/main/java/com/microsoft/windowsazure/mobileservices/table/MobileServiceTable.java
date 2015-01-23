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
 * MobileServiceTable.java
 */
package com.microsoft.windowsazure.mobileservices.table;

import android.util.Pair;

import com.google.common.util.concurrent.FutureCallback;
import com.google.common.util.concurrent.Futures;
import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.SettableFuture;
import com.google.gson.Gson;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.MobileServiceFeatures;
import com.microsoft.windowsazure.mobileservices.MobileServiceList;
import com.microsoft.windowsazure.mobileservices.table.query.ExecutableQuery;
import com.microsoft.windowsazure.mobileservices.table.query.Query;
import com.microsoft.windowsazure.mobileservices.table.query.QueryOrder;
import com.microsoft.windowsazure.mobileservices.table.serialization.JsonEntityParser;

import java.lang.reflect.Field;
import java.util.EnumSet;
import java.util.List;

/**
 * Represents a Mobile Service Table
 */
public final class MobileServiceTable<E> extends MobileServiceTableBase {

    private MobileServiceJsonTable mInternalTable;

    private Class<E> mClazz;

    /**
     * Constructor for MobileServiceTable
     *
     * @param name   The name of the represented table
     * @param client The MobileServiceClient used to invoke table operations
     * @param clazz  The class used for data serialization
     */
    public MobileServiceTable(String name, MobileServiceClient client, Class<E> clazz) {
        super(name, client);
        mFeatures.add(MobileServiceFeatures.TypedTable);

        mInternalTable = new MobileServiceJsonTable(name, client);
        mInternalTable.mFeatures = EnumSet.of(MobileServiceFeatures.TypedTable);
        mClazz = clazz;

        mSystemProperties = getSystemProperties(clazz);
        mInternalTable.setSystemProperties(mSystemProperties);
    }

    public EnumSet<MobileServiceSystemProperty> getSystemProperties() {
        return mInternalTable.getSystemProperties();
    }

    public void setSystemProperties(EnumSet<MobileServiceSystemProperty> systemProperties) {
        this.mSystemProperties = systemProperties;
        this.mInternalTable.setSystemProperties(systemProperties);
    }

    /**
     * Executes a query to retrieve all the table rows
     *
     * @throws com.microsoft.windowsazure.mobileservices.MobileServiceException
     */
    public ListenableFuture<MobileServiceList<E>> execute() throws MobileServiceException {
        final SettableFuture<MobileServiceList<E>> future = SettableFuture.create();
        ListenableFuture<JsonElement> internalFuture = mInternalTable.execute();
        Futures.addCallback(internalFuture, new FutureCallback<JsonElement>() {
            @Override
            public void onFailure(Throwable exc) {
                future.setException(exc);
            }

            @Override
            public void onSuccess(JsonElement result) {
                try {
                    if (result.isJsonObject()) {
                        JsonObject jsonObject = result.getAsJsonObject();

                        int count = jsonObject.get("count").getAsInt();
                        JsonElement elements = jsonObject.get("results");

                        List<E> list = parseResults(elements);
                        future.set(new MobileServiceList<E>(list, count));
                    } else {
                        List<E> list = parseResults(result);
                        future.set(new MobileServiceList<E>(list, -1));
                    }
                } catch (Exception e) {
                    future.setException(e);
                }
            }
        });

        return future;
    }

    /**
     * Executes a query to retrieve all the table rows
     *
     * @param callback Callback to invoke when the operation is completed
     * @throws com.microsoft.windowsazure.mobileservices.MobileServiceException
     * @deprecated use {@link execute()} instead
     */
    public void execute(final TableQueryCallback<E> callback) throws MobileServiceException {

        ListenableFuture<MobileServiceList<E>> executeFuture = execute();

        Futures.addCallback(executeFuture, new FutureCallback<MobileServiceList<E>>() {
            @Override
            public void onFailure(Throwable exception) {
                if (exception instanceof Exception) {
                    callback.onCompleted(null, 0, (Exception) exception, MobileServiceException.getServiceResponse(exception));
                } else {
                    callback.onCompleted(null, 0, new Exception(exception), MobileServiceException.getServiceResponse(exception));
                }
            }

            @Override
            public void onSuccess(MobileServiceList<E> result) {
                callback.onCompleted(result, result.getTotalCount(), null, null);
            }
        });
    }

    /**
     * Executes a query to retrieve all the table rows
     *
     * @param query The Query instance to execute
     */
    public ListenableFuture<MobileServiceList<E>> execute(Query query) {
        final SettableFuture<MobileServiceList<E>> future = SettableFuture.create();
        ListenableFuture<JsonElement> internalFuture = mInternalTable.execute(query);
        Futures.addCallback(internalFuture, new FutureCallback<JsonElement>() {
            @Override
            public void onFailure(Throwable exc) {
                future.setException(exc);
            }

            @Override
            public void onSuccess(JsonElement result) {
                processQueryResults(result, future);
            }
        });

        return future;
    }

    /**
     * Executes a Next Link to retrieve all the table rows
     *
     * @param nextLink The next link with the page information
     */
    public ListenableFuture<MobileServiceList<E>> execute(String nextLink) {
        final SettableFuture<MobileServiceList<E>> future = SettableFuture.create();
        ListenableFuture<JsonElement> internalFuture = mInternalTable.execute(nextLink);
        Futures.addCallback(internalFuture, new FutureCallback<JsonElement>() {
            @Override
            public void onFailure(Throwable exc) {
                future.setException(exc);
            }

            @Override
            public void onSuccess(JsonElement result) {
                processQueryResults(result, future);
            }
        });

        return future;
    }

    /**
     * Process the Results of the Query
     *
     * @param result The Json element with the information
     * @param future The future with the callbacks
     */
    private void processQueryResults(JsonElement result, SettableFuture<MobileServiceList<E>> future) {
        try {
            if (result.isJsonObject()) {
                JsonObject jsonObject = result.getAsJsonObject();

                int count = 0;
                String nextLink = null;

                if (jsonObject.has("count")) {
                    count = jsonObject.get("count").getAsInt();
                }

                if (jsonObject.has("nextLink")) {
                    nextLink = jsonObject.get("nextLink").getAsString();
                }

                JsonElement elements = jsonObject.get("results");

                List<E> list = parseResults(elements);

                MobileServiceList<E> futureResult;

                if (nextLink != null){
                    futureResult = new MobileServiceList<E>(list, count, nextLink);
                } else {
                    futureResult = new MobileServiceList<E>(list, count);
                }

                future.set(futureResult);
            } else {
                List<E> list = parseResults(result);
                future.set(new MobileServiceList<E>(list, list.size()));
            }
        } catch (Exception e) {
            future.setException(e);
        }
    }

    /**
     * Executes a query to retrieve all the table rows
     *
     * @param query    The MobileServiceQuery instance to execute
     * @param callback Callback to invoke when the operation is completed
     * @deprecated use {@link execute( com.microsoft.windowsazure.mobileservices.table.query.Query query)} instead
     */
    public void execute(Query query, final TableQueryCallback<E> callback) {
        ListenableFuture<MobileServiceList<E>> executeFuture = execute(query);

        Futures.addCallback(executeFuture, new FutureCallback<MobileServiceList<E>>() {
            @Override
            public void onFailure(Throwable exception) {
                if (exception instanceof Exception) {
                    callback.onCompleted(null, 0, (Exception) exception, MobileServiceException.getServiceResponse(exception));
                } else {
                    callback.onCompleted(null, 0, new Exception(exception), MobileServiceException.getServiceResponse(exception));
                }
            }

            @Override
            public void onSuccess(MobileServiceList<E> result) {
                callback.onCompleted(result, result.size(), null, null);
            }
        });
    }

    /**
     * Starts a filter to query the table
     *
     * @return The ExecutableQuery<E> representing the filter
     */
    public ExecutableQuery<E> where() {
        ExecutableQuery<E> query = new ExecutableQuery<E>();
        query.setTable(this);
        return query;
    }

    /**
     * Starts a filter to query the table with an existing filter
     *
     * @param query The existing filter
     * @return The ExecutableQuery<E> representing the filter
     */
    public ExecutableQuery<E> where(Query query) {
        if (query == null) {
            throw new IllegalArgumentException("Query must not be null");
        }

        ExecutableQuery<E> baseQuery = new ExecutableQuery<E>(query);
        baseQuery.setTable(this);

        return baseQuery;
    }

    /**
     * Adds a new user-defined parameter to the query
     *
     * @param parameter The parameter name
     * @param value     The parameter value
     * @return ExecutableQuery
     */
    public ExecutableQuery<E> parameter(String parameter, String value) {
        return this.where().parameter(parameter, value);
    }

    /**
     * Creates a query with the specified order
     *
     * @param field Field name
     * @param order Sorting order
     * @return ExecutableQuery
     */
    public ExecutableQuery<E> orderBy(String field, QueryOrder order) {
        return this.where().orderBy(field, order);
    }

    /**
     * Sets the number of records to return
     *
     * @param top Number of records to return
     * @return ExecutableQuery
     */
    public ExecutableQuery<E> top(int top) {
        return this.where().top(top);
    }

    /**
     * Sets the number of records to skip over a given number of elements in a
     * sequence and then return the remainder.
     *
     * @param skip
     * @return ExecutableQuery
     */
    public ExecutableQuery<E> skip(int skip) {
        return this.where().skip(skip);
    }

    /**
     * Specifies the fields to retrieve
     *
     * @param fields Names of the fields to retrieve
     * @return ExecutableQuery
     */
    public ExecutableQuery<E> select(String... fields) {
        return this.where().select(fields);
    }

    /**
     * Include a property with the number of records returned.
     *
     * @return ExecutableQuery
     */
    public ExecutableQuery<E> includeInlineCount() {
        return this.where().includeInlineCount();
    }

    /**
     * Include a the soft deleted items on  records returned.
     *
     * @return ExecutableQuery
     */
    public ExecutableQuery<E> includeDeleted() {
        return this.where().includeDeleted();
    }

    /**
     * Looks up a row in the table.
     *
     * @param id The id of the row
     */
    public ListenableFuture<E> lookUp(Object id) {
        return lookUp(id, (List<Pair<String, String>>) null);
    }

    /**
     * Looks up a row in the table.
     *
     * @param id       The id of the row
     * @param callback Callback to invoke after the operation is completed
     * @deprecated use {@link lookUp(Object id)} instead
     */
    public void lookUp(Object id, final TableOperationCallback<E> callback) {
        ListenableFuture<E> lookUpFuture = lookUp(id);

        Futures.addCallback(lookUpFuture, new FutureCallback<E>() {
            @Override
            public void onFailure(Throwable exception) {
                if (exception instanceof Exception) {
                    callback.onCompleted(null, (Exception) exception, MobileServiceException.getServiceResponse(exception));
                } else {
                    callback.onCompleted(null, new Exception(exception), MobileServiceException.getServiceResponse(exception));
                }
            }

            @Override
            public void onSuccess(E result) {
                callback.onCompleted(result, null, null);
            }
        });
    }

    /**
     * Looks up a row in the table.
     *
     * @param id         The id of the row
     * @param parameters A list of user-defined parameters and values to include in the
     *                   request URI query string
     */
    public ListenableFuture<E> lookUp(Object id, List<Pair<String, String>> parameters) {
        final SettableFuture<E> future = SettableFuture.create();

        ListenableFuture<JsonObject> internalFuture = mInternalTable.lookUp(id, parameters);
        Futures.addCallback(internalFuture, new FutureCallback<JsonElement>() {
            @Override
            public void onFailure(Throwable exc) {
                future.setException(transformToTypedException(exc));
            }

            @Override
            public void onSuccess(JsonElement result) {
                try {
                    future.set(parseResults(result).get(0));
                } catch (Exception e) {
                    future.setException(e);
                }
            }
        });

        return future;
    }

    /**
     * Looks up a row in the table.
     *
     * @param id         The id of the row
     * @param parameters A list of user-defined parameters and values to include in the
     *                   request URI query string
     * @param callback   Callback to invoke after the operation is completed
     * @deprecated use {@link lookUp(Object id, java.util.List< android.util.Pair<String, String>>
     * parameters)} instead
     */
    public void lookUp(Object id, List<Pair<String, String>> parameters, final TableOperationCallback<E> callback) {
        ListenableFuture<E> lookUpFuture = lookUp(id, parameters);

        Futures.addCallback(lookUpFuture, new FutureCallback<E>() {
            @Override
            public void onFailure(Throwable exception) {
                if (exception instanceof Exception) {
                    callback.onCompleted(null, (Exception) exception, MobileServiceException.getServiceResponse(exception));
                } else {
                    callback.onCompleted(null, new Exception(exception), MobileServiceException.getServiceResponse(exception));
                }
            }

            @Override
            public void onSuccess(E result) {
                callback.onCompleted(result, null, null);
            }
        });
    }

    /**
     * Inserts an entity into a Mobile Service Table
     *
     * @param element The entity to insert
     */
    public ListenableFuture<E> insert(final E element) {
        return this.insert(element, (List<Pair<String, String>>) null);
    }

    /**
     * Inserts an entity into a Mobile Service Table
     *
     * @param element  The entity to insert
     * @param callback Callback to invoke when the operation is completed
     * @deprecated use {@link insert(final E element)} instead
     */
    public void insert(final E element, final TableOperationCallback<E> callback) {
        this.insert(element, null, callback);
    }

    /**
     * Inserts an entity into a Mobile Service Table
     *
     * @param element    The entity to insert
     * @param parameters A list of user-defined parameters and values to include in the
     *                   request URI query string
     * @param callback   Callback to invoke when the operation is completed
     */
    public ListenableFuture<E> insert(final E element, List<Pair<String, String>> parameters) {
        final SettableFuture<E> future = SettableFuture.create();
        JsonObject json = null;
        try {
            json = mClient.getGsonBuilder().create().toJsonTree(element).getAsJsonObject();
        } catch (IllegalArgumentException e) {
            future.setException(e);
            return future;
        }

        Class<?> idClazz = getIdPropertyClass(element.getClass());

        if (idClazz != null && !isIntegerClass(idClazz)) {
            json = removeSystemProperties(json);
        }

        ListenableFuture<JsonObject> internalFuture = mInternalTable.insert(json, parameters);
        Futures.addCallback(internalFuture, new FutureCallback<JsonElement>() {
            @Override
            public void onFailure(Throwable exc) {
                future.setException(transformToTypedException(exc));
            }

            @Override
            public void onSuccess(JsonElement result) {
                E entity = null;
                try {
                    entity = parseResults(result).get(0);
                    if (entity != null && element != null) {
                        copyFields(entity, element);
                        entity = element;
                    }
                    future.set(entity);
                } catch (Exception e) {
                    future.setException(e);
                }
            }
        });

        return future;
    }

    /**
     * Inserts an entity into a Mobile Service Table
     *
     * @param element    The entity to insert
     * @param parameters A list of user-defined parameters and values to include in the
     *                   request URI query string
     * @param callback   Callback to invoke when the operation is completed
     * @deprecated use {@link insert(final E element, java.util.List< android.util.Pair<String, String>>
     * parameters)} instead
     */
    public void insert(final E element, List<Pair<String, String>> parameters, final TableOperationCallback<E> callback) {

        ListenableFuture<E> insertFuture = insert(element, parameters);

        Futures.addCallback(insertFuture, new FutureCallback<E>() {
            @Override
            public void onFailure(Throwable exception) {
                if (exception instanceof Exception) {
                    callback.onCompleted(null, (Exception) exception, MobileServiceException.getServiceResponse(exception));
                } else {
                    callback.onCompleted(null, new Exception(exception), MobileServiceException.getServiceResponse(exception));
                }
            }

            @Override
            public void onSuccess(E result) {
                callback.onCompleted(result, null, null);
            }
        });
    }

    /**
     * Updates an entity from a Mobile Service Table
     *
     * @param element The entity to update
     */
    public ListenableFuture<E> update(final E element) {
        return this.update(element, (List<Pair<String, String>>) null);
    }

    /**
     * Updates an entity from a Mobile Service Table
     *
     * @param element  The entity to update
     * @param callback Callback to invoke when the operation is completed
     * @deprecated use {@link update(final E element)} instead
     */
    public void update(final E element, final TableOperationCallback<E> callback) {
        this.update(element, null, callback);
    }

    /**
     * Updates an entity from a Mobile Service Table
     *
     * @param element    The entity to update
     * @param parameters A list of user-defined parameters and values to include in the
     *                   request URI query string
     * @param callback   Callback to invoke when the operation is completed
     */
    public ListenableFuture<E> update(final E element, final List<Pair<String, String>> parameters) {
        final SettableFuture<E> future = SettableFuture.create();

        JsonObject json = null;

        try {
            json = mClient.getGsonBuilder().create().toJsonTree(element).getAsJsonObject();
        } catch (IllegalArgumentException e) {
            future.setException(e);
            return future;
        }

        ListenableFuture<JsonObject> internalFuture = mInternalTable.update(json, parameters);
        Futures.addCallback(internalFuture, new FutureCallback<JsonElement>() {
            @Override
            public void onFailure(Throwable exc) {
                future.setException(transformToTypedException(exc));
            }

            @Override
            public void onSuccess(JsonElement result) {
                E entity = null;
                try {
                    entity = parseResults(result).get(0);
                    if (entity != null && element != null) {
                        copyFields(entity, element);
                        entity = element;
                    }
                    future.set(entity);
                } catch (Exception e) {
                    future.setException(e);
                }
            }
        });

        return future;
    }

    /**
     * Updates an entity from a Mobile Service Table
     *
     * @param element    The entity to update
     * @param parameters A list of user-defined parameters and values to include in the
     *                   request URI query string
     * @param callback   Callback to invoke when the operation is completed
     * @deprecated use {@link update(final E element, final java.util.List< android.util.Pair<String,
     * String>> parameters)} instead
     */
    public void update(final E element, final List<Pair<String, String>> parameters, final TableOperationCallback<E> callback) {
        ListenableFuture<E> updateFuture = update(element, parameters);

        Futures.addCallback(updateFuture, new FutureCallback<E>() {
            @Override
            public void onFailure(Throwable exception) {
                if (exception instanceof Exception) {
                    callback.onCompleted(null, (Exception) exception, MobileServiceException.getServiceResponse(exception));
                } else {
                    callback.onCompleted(null, new Exception(exception), MobileServiceException.getServiceResponse(exception));
                }
            }

            @Override
            public void onSuccess(E result) {
                callback.onCompleted(result, null, null);
            }
        });
    }


    /**
     * Undelete an entity from a Mobile Service Table
     *
     * @param element The entity to Undelete
     */
    public ListenableFuture<E> undelete(final E element) {
        return this.undelete(element, (List<Pair<String, String>>) null);
    }

    /**
     * Undelete an entity from a Mobile Service Table
     *
     * @param element  The entity to update
     * @param callback Callback to invoke when the operation is completed
     * @deprecated use {@link undelete(final E element)} instead
     */
    public void undelete(final E element, final TableOperationCallback<E> callback) {
        this.undelete(element, null, callback);
    }

    /**
     * Undelete an entity from a Mobile Service Table
     *
     * @param element    The entity to Undelete
     * @param parameters A list of user-defined parameters and values to include in the
     *                   request URI query string
     */
    public ListenableFuture<E> undelete(final E element, final List<Pair<String, String>> parameters) {
        final SettableFuture<E> future = SettableFuture.create();

        JsonObject json = null;

        try {
            json = mClient.getGsonBuilder().create().toJsonTree(element).getAsJsonObject();
        } catch (IllegalArgumentException e) {
            future.setException(e);
            return future;
        }

        ListenableFuture<JsonObject> internalFuture = mInternalTable.undelete(json, parameters);
        Futures.addCallback(internalFuture, new FutureCallback<JsonElement>() {
            @Override
            public void onFailure(Throwable exc) {
                future.setException(transformToTypedException(exc));
            }

            @Override
            public void onSuccess(JsonElement result) {
                E entity = null;
                try {
                    entity = parseResults(result).get(0);
                    if (entity != null && element != null) {
                        copyFields(entity, element);
                        entity = element;
                    }
                    future.set(entity);
                } catch (Exception e) {
                    future.setException(e);
                }
            }
        });

        return future;
    }

    /**
     * Undelete an entity from a Mobile Service Table
     *
     * @param element    The entity to Undelete
     * @param parameters A list of user-defined parameters and values to include in the
     *                   request URI query string
     * @param callback   Callback to invoke when the operation is completed
     * @deprecated use {@link undelete(final E element, final java.util.List< android.util.Pair<String,
     * String>> parameters)} instead
     */
    public void undelete(final E element, final List<Pair<String, String>> parameters, final TableOperationCallback<E> callback) {
        ListenableFuture<E> undeleteFuture = undelete(element, parameters);

        Futures.addCallback(undeleteFuture, new FutureCallback<E>() {
            @Override
            public void onFailure(Throwable exception) {
                if (exception instanceof Exception) {
                    callback.onCompleted(null, (Exception) exception, MobileServiceException.getServiceResponse(exception));
                } else {
                    callback.onCompleted(null, new Exception(exception), MobileServiceException.getServiceResponse(exception));
                }
            }

            @Override
            public void onSuccess(E result) {
                callback.onCompleted(result, null, null);
            }
        });
    }


    /**
     * Parses the JSON object to a typed list
     *
     * @param results JSON results
     * @return List of entities
     */
    private List<E> parseResults(JsonElement results) {
        Gson gson = mClient.getGsonBuilder().create();
        return JsonEntityParser.parseResults(results, gson, mClazz);
    }

    /**
     * Copy object field values from source to target object
     *
     * @param source The object to copy the values from
     * @param target The destination object
     * @throws IllegalAccessException
     * @throws IllegalArgumentException
     */
    private void copyFields(Object source, Object target) throws IllegalArgumentException, IllegalAccessException {
        if (source != null && target != null) {
            for (Field field : source.getClass().getDeclaredFields()) {
                field.setAccessible(true);
                field.set(target, field.get(source));
            }
        }
    }

    private Throwable transformToTypedException(Throwable exc) {

        if (exc instanceof MobileServicePreconditionFailedExceptionJson) {
            MobileServicePreconditionFailedExceptionJson ex = (MobileServicePreconditionFailedExceptionJson) exc;

            E entity = parseResultsForTypedException(ex);

            return new MobileServicePreconditionFailedException(ex, entity);

        } else if (exc instanceof MobileServiceConflictExceptionJson) {
            MobileServiceConflictExceptionJson ex = (MobileServiceConflictExceptionJson) exc;

            E entity = parseResultsForTypedException(ex);

            return new MobileServiceConflictException(ex, entity);
        }

        return exc;
    }

    private E parseResultsForTypedException(MobileServiceExceptionBase ex) {
        E entity = null;

        try {
            entity = parseResults(ex.getValue()).get(0);
        } catch (Exception e) {
        }

        return entity;
    }
}