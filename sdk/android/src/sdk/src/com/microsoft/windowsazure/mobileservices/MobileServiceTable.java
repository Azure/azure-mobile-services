/*
 * MobileServiceTable.java
 */

package com.microsoft.windowsazure.mobileservices;

import java.io.UnsupportedEncodingException;
import java.lang.reflect.Type;
import java.net.URLEncoder;
import java.security.InvalidParameterException;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.TimeZone;

import org.apache.http.client.methods.HttpDelete;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.client.methods.HttpPost;
//
import android.annotation.SuppressLint;
import android.util.Pair;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import com.google.gson.JsonArray;
import com.google.gson.JsonDeserializationContext;
import com.google.gson.JsonDeserializer;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParseException;
import com.google.gson.JsonParser;
import com.google.gson.JsonPrimitive;
import com.google.gson.JsonSerializationContext;
import com.google.gson.JsonSerializer;

/**
 * Represents a Mobile Service Table
 */
public final class MobileServiceTable {

	/**
	 * Tables URI part
	 */
	private static final String TABLES_URL = "tables/";

	/**
	 * GsonBuilder used to in JSON Serialization/Deserialization
	 */
	private GsonBuilder mGsonBuilder;

	/**
	 * The MobileServiceClient used to invoke table operations
	 */
	private MobileServiceClient mClient;

	/**
	 * The name of the represented table
	 */
	private String mTableName;

	/**
	 * Private constructor used for static query creation
	 */
	private MobileServiceTable() {

	}

	/**
	 * Constructor for MobileServiceTable
	 * 
	 * @param name
	 *            The name of the represented table
	 * @param client
	 *            The MobileServiceClient used to invoke table operations
	 */
	public MobileServiceTable(String name, MobileServiceClient client) {
		if (name == null || name.toString().trim().length() == 0) {
			throw new IllegalArgumentException("Invalid Table Name");
		}

		if (client == null) {
			throw new IllegalArgumentException("Invalid Mobile Service Client");
		}

		mClient = client;
		mTableName = name;
		mGsonBuilder = new GsonBuilder();

		// Register custom date deserializer
		this.registerDeserializer(Date.class, new JsonDeserializer<Date>() {
			@SuppressLint("SimpleDateFormat")
			@Override
			public Date deserialize(JsonElement element, Type type,
					JsonDeserializationContext ctx) throws JsonParseException {
				String strVal = element.getAsString();

				// Change Z to +00:00 to adapt the string to a format that can
				// be parsed in Java
				String s = strVal.replace("Z", "+00:00");
				try {
					// Remove the ":" character to adapt the string to a format
					// that can be parsed in Java
					s = s.substring(0, 26) + s.substring(27);
				} catch (IndexOutOfBoundsException e) {
					throw new JsonParseException("Invalid length");
				}

				try {
					// Parse the well-formatted date string
					SimpleDateFormat dateFormat = new SimpleDateFormat(
							"yyyy-MM-dd'T'HH:mm:ss'.'SSSZ");
					dateFormat.setTimeZone(TimeZone.getDefault());
					Date date = dateFormat.parse(s);

					return date;
				} catch (ParseException e) {
					throw new JsonParseException(e);
				}
			}
		});

		// Register custom date serializer
		this.registerSerializer(Date.class, new JsonSerializer<Date>() {
			@Override
			public JsonElement serialize(Date date, Type type,
					JsonSerializationContext ctx) {
				SimpleDateFormat dateFormat = new SimpleDateFormat(
						"yyyy-MM-dd'T'HH:mm:ss'.'SSS'Z'", Locale.getDefault());
				dateFormat.setTimeZone(TimeZone.getTimeZone("UTC"));

				String formatted = dateFormat.format(date);

				JsonElement element = new JsonPrimitive(formatted);
				return element;
			}
		});
	}

	/**
	 * Crates a query to retrieve all the table rows
	 * 
	 * @return The MobileServiceQuery to retrieve all the table rows
	 */
	public MobileServiceQuery all() {
		return this.where();
	}

	/**
	 * Starts a filter to query the table
	 * 
	 * @return The MobileServiceQuery representing the filter
	 */
	public MobileServiceQuery where() {
		MobileServiceQuery query = new MobileServiceQuery();
		query.setTable(this);
		return query;
	}

	/**
	 * Starts a filter to query the table with an existing filter
	 * 
	 * @param query
	 *            The existing filter
	 * @return The MobileServiceQuery representing the filter
	 */
	public MobileServiceQuery where(MobileServiceQuery query) {
		if (query == null) {
			throw new IllegalArgumentException("Query must not be null");
		}

		MobileServiceQuery baseQuery = new MobileServiceQuery(query);
		baseQuery.setTable(this);
		return baseQuery;
	}

	/**
	 * Returns the name of the represented table
	 */
	public String getTableName() {
		return mTableName;
	}

	/**
	 * Returns the client used for table operations
	 */
	public MobileServiceClient getClient() {
		return mClient;
	}

	/**
	 * Returns the GsonBuilder used to serialize/deserialize objects with this
	 * table
	 */
	public GsonBuilder getGsonBuilder() {
		return mGsonBuilder;
	}

	/**
	 * Registers a JsonSerializer for the specified type
	 * 
	 * @param type
	 *            The type to use in the registration
	 * @param serializer
	 *            The serializer to use in the registration
	 */
	public <T> void registerSerializer(Type type, JsonSerializer<T> serializer) {
		mGsonBuilder.registerTypeAdapter(type, serializer);
	}

	/**
	 * Registers a JsonDeserializer for the specified type
	 * 
	 * @param type
	 *            The type to use in the registration
	 * @param deserializer
	 *            The deserializer to use in the registration
	 */
	public <T> void registerDeserializer(Type type,
			JsonDeserializer<T> deserializer) {
		mGsonBuilder.registerTypeAdapter(type, deserializer);
	}

	/**
	 * Looks up a row in the table. Deserializes the row using the given class.
	 * 
	 * @param id
	 *            The id of the row
	 * @param clazz
	 *            The class used to deserialize the row
	 * @param callback
	 *            Callback to invoke after the operation is completed
	 */
	public <E> void lookUp(Object id, final Class<E> clazz,
			final TableOperationCallback<E> callback) {

		// Create request URL
		String url = null;
		try {
			url = mClient.getAppUrl().toString()
					+ TABLES_URL
					+ URLEncoder.encode(mTableName,
							MobileServiceClient.UTF8_ENCODING)
					+ "/"
					+ URLEncoder.encode(id.toString(),
							MobileServiceClient.UTF8_ENCODING);
		} catch (UnsupportedEncodingException e) {
			if (callback != null) {
				callback.onError(e, null);
			}
			return;
		}

		executeGetRecords(url, new TableJsonQueryCallback() {

			@Override
			public void onError(Exception exception,
					ServiceFilterResponse response) {
				if (callback != null) {
					callback.onError(exception, response);
				}
			}

			@Override
			public void onSuccess(JsonElement results, int count) {
				if (callback != null) {
					List<E> result = parseResults(results, clazz);

					if (result.size() > 0) {
						callback.onSuccess(result.get(0));
					} else {
						callback.onError(
								new MobileServiceException(
										"A record with the specified Id cannot be found"),
								null);
					}
				}
			}
		});
	}

	/**
	 * Looks up a row in the table and retrieves its JSON value.
	 * 
	 * @param id
	 *            The id of the row
	 * @param callback
	 *            Callback to invoke after the operation is completed
	 */
	public void lookUp(Object id, final TableJsonOperationCallback callback) {
		// Create request URL
		String url = mClient.getAppUrl().toString() + TABLES_URL
				+ URLEncoder.encode(mTableName) + "/"
				+ URLEncoder.encode(id.toString());

		executeGetRecords(url, new TableJsonQueryCallback() {

			@Override
			public void onError(Exception exception,
					ServiceFilterResponse response) {
				if (callback != null) {
					callback.onError(exception, response);
				}
			}

			@Override
			public void onSuccess(JsonElement results, int count) {
				if (callback != null) {
					if (results.isJsonArray()) {
						JsonArray elements = results.getAsJsonArray();
						if (elements.size() > 0) {
							callback.onSuccess(elements.get(0)
									.getAsJsonObject());
						} else {
							callback.onSuccess(null);
						}
					} else { // Lookup result
						callback.onSuccess(results.getAsJsonObject());
					}
				}
			}
		});
	}

	/**
	 * Inserts an entity into a Mobile Service Table
	 * 
	 * @param element
	 *            The entity to insert
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 * @throws InvalidParameterException
	 */
	public <E> void insert(final E element,
			final TableOperationCallback<E> callback)
			throws InvalidParameterException {
		final JsonObject json = mGsonBuilder.create().toJsonTree(element)
				.getAsJsonObject();

		removeIdFromJson(json);

		String content = json.toString();

		ServiceFilterRequest post = new ServiceFilterRequestImpl(new HttpPost(
				mClient.getAppUrl().toString() + TABLES_URL + mTableName));
		try {
			post.setContent(content);
		} catch (Exception e) {
			// this should never happen
		}

		executeTableOperation(post, new TableJsonOperationCallback() {
			@Override
			public void onSuccess(JsonObject jsonEntity) {
				if (callback != null) {
					JsonObject patchedJson = patchOriginalEntityWithResponseEntity(
							json, jsonEntity);
					@SuppressWarnings("unchecked")
					E newEntity = (E) parseResults(patchedJson,
							element.getClass()).get(0);

					callback.onSuccess(newEntity);
				}
			}

			@Override
			public void onError(Exception exception,
					ServiceFilterResponse response) {
				if (callback != null) {
					callback.onError(exception, response);
				}
			}
		});
	}

	private void removeIdFromJson(final JsonObject json) {
		// Remove id property if exists
		String[] idPropertyNames = new String[4];
		idPropertyNames[0] = "id";
		idPropertyNames[1] = "Id";
		idPropertyNames[2] = "iD";
		idPropertyNames[3] = "ID";
		
		for(int i = 0; i < 4; i++) {
			String idProperty = idPropertyNames[i];
			if (json.has(idProperty)) {
				JsonElement idElement = json.get(idProperty);
				if (!idElement.isJsonNull() && idElement.getAsInt() != 0) {
					throw new InvalidParameterException(
							"The entity to insert should not have " + idProperty + " property defined");
				}
	
				json.remove(idProperty);
			}
		}
	}

	/**
	 * Inserts a jsonEntity into a Mobile Service Table
	 * 
	 * @param element
	 *            The jsonEntity to insert
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 * @throws InvalidParameterException
	 */
	public void insert(final JsonObject element,
			final TableJsonOperationCallback callback)
			throws InvalidParameterException {

		removeIdFromJson(element);
		
		String content = element.toString();

		ServiceFilterRequest post = new ServiceFilterRequestImpl(new HttpPost(
				mClient.getAppUrl().toString() + TABLES_URL + mTableName));
		try {
			post.setContent(content);
		} catch (Exception e) {
			// this should never happen
		}

		executeTableOperation(post, new TableJsonOperationCallback() {

			@Override
			public void onSuccess(JsonObject jsonEntity) {
				if (callback != null) {
					JsonObject patchedJson = patchOriginalEntityWithResponseEntity(
							element, jsonEntity);
					callback.onSuccess(patchedJson);
				}
			}

			@Override
			public void onError(Exception exception,
					ServiceFilterResponse response) {
				if (callback != null) {
					callback.onError(exception, response);
				}
			}
		});
	}

	/**
	 * Updates an entity from a Mobile Service Table
	 * 
	 * @param element
	 *            The entity to update
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public <E> void update(final E element,
			final TableOperationCallback<E> callback) {
		final JsonObject json = mGsonBuilder.create().toJsonTree(element)
				.getAsJsonObject();
		String content = json.toString();

		ServiceFilterRequest patch = new ServiceFilterRequestImpl(
				new HttpPatch(mClient.getAppUrl().toString() + TABLES_URL
						+ mTableName + "/"
						+ Integer.valueOf(getObjectId(element)).toString()));
		try {
			patch.setContent(content);
		} catch (Exception e) {
			// this will never happen
		}

		executeTableOperation(patch, new TableJsonOperationCallback() {
			@Override
			public void onSuccess(JsonObject jsonEntity) {
				if (callback != null) {
					JsonObject patchedJson = patchOriginalEntityWithResponseEntity(
							json, jsonEntity);
					@SuppressWarnings("unchecked")
					E newEntity = (E) parseResults(patchedJson,
							element.getClass()).get(0);

					callback.onSuccess(newEntity);
				}
			}

			@Override
			public void onError(Exception exception,
					ServiceFilterResponse response) {
				if (callback != null) {
					callback.onError(exception, response);
				}
			}
		});
	}

	/**
	 * Updates an entity from a Mobile Service Table
	 * 
	 * @param element
	 *            The jsonEntity to update
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public void update(final JsonObject element,
			final TableJsonOperationCallback callback) {
		String content = element.toString();
		ServiceFilterRequest patch = new ServiceFilterRequestImpl(
				new HttpPatch(mClient.getAppUrl().toString() + TABLES_URL
						+ mTableName + "/"
						+ Integer.valueOf(getObjectId(element)).toString()));
		try {
			patch.setContent(content);
		} catch (Exception e) {
			// this will never happen
		}

		executeTableOperation(patch, new TableJsonOperationCallback() {

			@Override
			public void onSuccess(JsonObject jsonEntity) {
				if (callback != null) {
					JsonObject patchedJson = patchOriginalEntityWithResponseEntity(
							element, jsonEntity);
					callback.onSuccess(patchedJson);
				}
			}

			@Override
			public void onError(Exception exception,
					ServiceFilterResponse response) {
				if (callback != null) {
					callback.onError(exception, response);
				}
			}
		});
	}

	/**
	 * Deletes an entity from a Mobile Service Table
	 * 
	 * @param element
	 *            The entity to delete
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public void delete(Object element, TableDeleteCallback callback) {
		int id = getObjectId(element);

		this.delete(id, callback);
	}

	/**
	 * Deletes an entity from a Mobile Service Table using a given id
	 * 
	 * @param id
	 *            The id of the entity to delete
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public void delete(int id, final TableDeleteCallback callback) {
		// Create delete request
		HttpDelete delete = new HttpDelete(mClient.getAppUrl().toString()
				+ TABLES_URL + mTableName + "/"
				+ Integer.valueOf(id).toString());

		// Create AsyncTask to execute the request
		new RequestAsyncTask(new ServiceFilterRequestImpl(delete),
				mClient.createConnection()) {
			@Override
			protected void onPostExecute(ServiceFilterResponse result) {
				if (callback != null) {
					if (mTaskException == null && result != null) {
						callback.onSuccess();
					} else {
						callback.onError(mTaskException, result);
					}
				}
			}
		}.execute();
	}

	/**
	 * Executes the query against the table
	 * 
	 * @param request
	 *            Request to execute
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	private void executeTableOperation(ServiceFilterRequest request,
			final TableJsonOperationCallback callback) {
		// Create AsyncTask to execute the operation
		new RequestAsyncTask(request, mClient.createConnection()) {
			@Override
			protected void onPostExecute(ServiceFilterResponse result) {
				if (callback != null) {
					JsonObject newEntityJson = null;
					if (mTaskException == null && result != null) {
						String content = null;
						content = result.getContent();

						newEntityJson = new JsonParser().parse(content)
								.getAsJsonObject();

						callback.onSuccess(newEntityJson);

					} else {
						callback.onError(mTaskException, result);
					}
				}
			}
		}.execute();
	}

	/**
	 * Patches the original entity with the one returned in the response after
	 * executing the operation
	 * 
	 * @param originalEntity
	 *            The original entity
	 * @param newEntity
	 *            The entity obtained after executing the operation
	 * @return
	 */
	private JsonObject patchOriginalEntityWithResponseEntity(
			Object originalEntity, JsonObject newEntity) {
		// Patch the object to return with the new values
		Gson gson = mGsonBuilder.create();
		JsonObject patchedEntityJson;

		if (originalEntity instanceof JsonObject) {
			patchedEntityJson = (JsonObject) originalEntity;
		} else {
			patchedEntityJson = gson.toJsonTree(originalEntity)
					.getAsJsonObject();
		}

		for (Map.Entry<String, JsonElement> entry : newEntity.entrySet()) {
			patchedEntityJson.add(entry.getKey(), entry.getValue());
		}

		return patchedEntityJson;
	}

	/**
	 * Gets the id property from a given element
	 * 
	 * @param element
	 *            The element to use
	 * @return The id of the element
	 */
	private int getObjectId(Object element) {
		int id;

		if (element instanceof JsonObject) {
			id = ((JsonObject) element).get("id").getAsInt();
		} else {
			JsonObject json = mGsonBuilder.create().toJsonTree(element)
					.getAsJsonObject();
			id = json.get("id").getAsInt();
		}

		return id;
	}

	/**
	 * Retrieves a set of rows from the table using a query
	 * 
	 * @param query
	 *            The query used to retrieve the rows
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	private void executeQuery(final MobileServiceQuery query,
			final TableJsonQueryCallback callback) {
		String url = null;
		try {
			String filtersUrl = URLEncoder.encode(query.toString().trim(),
					MobileServiceClient.UTF8_ENCODING);
			url = mClient.getAppUrl().toString()
					+ TABLES_URL
					+ URLEncoder.encode(mTableName,
							MobileServiceClient.UTF8_ENCODING);

			if (filtersUrl.length() > 0) {
				url += "?$filter=" + filtersUrl + query.getRowSetModifiers();
				;
			} else {
				String rowSetModifiers = query.getRowSetModifiers();
				if (rowSetModifiers.length() > 0) {
					url += query.getRowSetModifiers().substring(1);
				}
			}

		} catch (UnsupportedEncodingException e) {
			if (callback != null) {
				callback.onError(e, null);
			}
			return;
		}

		executeGetRecords(url, callback);
	}

	/**
	 * Parses the JSON object to a typed list
	 * 
	 * @param results
	 *            JSON results
	 * @param clazz
	 *            Entity type
	 * @return List of entities
	 */
	private <E> List<E> parseResults(JsonElement results, final Class<E> clazz) {
		Gson gson = mGsonBuilder.create();
		List<E> result = new ArrayList<E>();

		// Parse results
		if (results.isJsonArray()) // Query result
		{
			JsonArray elements = results.getAsJsonArray();

			for (JsonElement element : elements) {
				E typedElement = ((E) gson.fromJson(element, clazz));
				result.add(typedElement);
			}
		} else { // Lookup result
			E typedElement = ((E) gson.fromJson(results, clazz));
			result.add(typedElement);
		}
		return result;
	}

	/**
	 * Retrieves a set of rows from using the specified URL
	 * 
	 * @param query
	 *            The URL used to retrieve the rows
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	private void executeGetRecords(final String url,
			final TableJsonQueryCallback callback) {
		ServiceFilterRequest request = new ServiceFilterRequestImpl(
				new HttpGet(url));

		MobileServiceConnection conn = mClient.createConnection();
		// Create AsyncTask to execute the request and parse the results
		new RequestAsyncTask(request, conn) {
			@Override
			protected void onPostExecute(ServiceFilterResponse response) {
				if (callback != null) {
					if (mTaskException == null && response != null) {
						JsonElement results = null;

						int count = -1;

						try {
							// Parse the results using the given Entity class
							String content = response.getContent();
							JsonElement json = new JsonParser().parse(content);

							if (json.isJsonObject()) {
								JsonObject jsonObject = json.getAsJsonObject();
								// If the response has count property, store its
								// value
								if (jsonObject.has("results")
										&& jsonObject.has("count")) { // inlinecount
									// result
									count = jsonObject.get("count").getAsInt();
									results = jsonObject.get("results");
								} else {
									results = json;
								}
							} else {
								results = json;
							}
						} catch (Exception e) {
							callback.onError(
									new MobileServiceException(
											"Error while retrieving data from response.",
											e), response);
							return;
						}

						callback.onSuccess(results, count);

					} else {
						callback.onError(mTaskException, response);
					}
				}
			}
		}.execute();
	}

	/**
	 * Class that represents a query to a table
	 */
	public class MobileServiceQuery {

		/**
		 * The MobileServiceTable to query
		 */
		private MobileServiceTable table;

		/**
		 * The main text of the query
		 */
		private String queryText = null;

		private boolean hasInlineCount = false;

		private List<Pair<String, QueryOrder>> orderBy = new ArrayList<Pair<String, QueryOrder>>();

		private List<String> projection = null;

		private int top = -1;

		private int skip = -1;

		/**
		 * List of values to print between parentheses
		 */
		private List<MobileServiceQuery> internalValues = new ArrayList<MobileServiceQuery>();

		/**
		 * Next steps in the query
		 */
		private List<MobileServiceQuery> querySteps = new ArrayList<MobileServiceQuery>();

		/**
		 * Returns the main text of the query
		 */
		public String getQueryText() {
			return queryText;
		}

		/**
		 * Sets the main text of the query
		 * 
		 * @param queryText
		 *            The text to set
		 */
		public void setQueryText(String queryText) {
			this.queryText = queryText;
		}

		/**
		 * Returns the MobileServiceTable to query
		 */
		private MobileServiceTable getTable() {
			return table;
		}

		/**
		 * Sets the MobileServiceTable to query
		 * 
		 * @param table
		 *            The MobileServiceTable to query
		 */
		private void setTable(MobileServiceTable table) {
			this.table = table;
		}

		/**
		 * Creates an empty MobileServiceQuery
		 */
		private MobileServiceQuery() {

		}

		/**
		 * Creates MobileServiceQuery with an existing query as its only query
		 * step
		 * 
		 * @param query
		 *            The query step to add
		 */
		private MobileServiceQuery(MobileServiceQuery query) {
			internalValues.add(query);
		}

		/**
		 * Adds an internal value to the query
		 * 
		 * @param query
		 *            The value to add
		 */
		private void addInternalValue(MobileServiceQuery query) {
			internalValues.add(query);
		}

		/**
		 * Returns the string representation of the query
		 */
		@Override
		public String toString() {
			StringBuilder sb = new StringBuilder();

			if (getQueryText() != null) {
				sb.append(getQueryText());
			}

			if (internalValues.size() > 0) {
				sb.append("(");

				boolean first = true;
				for (MobileServiceQuery val : internalValues) {
					if (first) {
						first = false;
					} else {
						sb.append(",");
					}

					sb.append(val.toString());
				}

				sb.append(")");
			}

			for (MobileServiceQuery step : querySteps) {
				// If the string is not empty and it doesn't end with space or
				// if it ends with ")", then add a space
				if ((!sb.toString().endsWith(" ") && sb.toString().length() > 0)
						|| sb.toString().endsWith(")")) {
					sb.append(" ");
				}

				sb.append(step.toString());
			}

			return sb.toString();
		}

		/**
		 * Returns the string representation of the rowset's modifiers
		 * 
		 * @throws UnsupportedEncodingException
		 */
		public String getRowSetModifiers() throws UnsupportedEncodingException {
			StringBuilder sb = new StringBuilder();

			if (this.hasInlineCount) {
				sb.append("&$inlinecount=allpages");
			}

			if (this.top > 0) {
				sb.append("&$top=");
				sb.append(this.top);
			}

			if (this.skip > 0) {
				sb.append("&$skip=");
				sb.append(this.skip);
			}

			if (this.orderBy.size() > 0) {
				sb.append("&$orderby=");

				boolean first = true;
				for (Pair<String, QueryOrder> order : this.orderBy) {
					if (first) {
						first = false;
					} else {
						sb.append(URLEncoder.encode(",",
								MobileServiceClient.UTF8_ENCODING));
					}

					sb.append(URLEncoder.encode(order.first,
							MobileServiceClient.UTF8_ENCODING));
					sb.append(URLEncoder.encode(" ",
							MobileServiceClient.UTF8_ENCODING));
					sb.append(order.second == QueryOrder.Ascending ? "asc"
							: "desc");

				}
			}

			if (this.projection != null && this.projection.size() > 0) {
				sb.append("&$select=");

				boolean first = true;
				for (String field : this.projection) {
					if (first) {
						first = false;
					} else {
						sb.append(URLEncoder.encode(",",
								MobileServiceClient.UTF8_ENCODING));
					}

					sb.append(URLEncoder.encode(field,
							MobileServiceClient.UTF8_ENCODING));
				}
			}

			return sb.toString();
		}

		/**
		 * Executes the query
		 * 
		 * @param clazz
		 *            The class used to deserialize the rows
		 * @param callback
		 *            Callback to invoke when the operation is completed
		 */
		public <E> void execute(final Class<E> clazz,
				final TableQueryCallback<E> callback) {
			getTable().executeQuery(this, new TableJsonQueryCallback() {
				@Override
				public void onSuccess(JsonElement results, int count) {
					if (callback != null) {
						List<E> result = parseResults(results, clazz);
						callback.onSuccess(result, count);
					}
				}

				@Override
				public void onError(Exception exception,
						ServiceFilterResponse response) {
					if (callback != null) {
						callback.onError(exception, response);
					}
				}
			});
		}

		/**
		 * Executes the query and returns the JSON with the results
		 * 
		 * @param clazz
		 *            The class used to deserialize the rows
		 * @param callback
		 *            Callback to invoke when the operation is completed
		 */
		public void execute(final TableJsonQueryCallback callback) {
			getTable().executeQuery(this, callback);
		}

		/**** Row Operations ****/

		/**
		 * Adds a new order by statement
		 * 
		 * @param field
		 *            FieldName
		 * @param order
		 *            Sorting order
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery orderBy(String field, QueryOrder order) {
			this.orderBy.add(new Pair<String, QueryOrder>(field, order));
			return this;
		}

		/**
		 * Sets the number of records to return
		 * 
		 * @param top
		 *            Number of records to return
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery top(int top) {
			if (top > 0) {
				this.top = top;
			}

			return this;
		}

		/**
		 * Sets the number of records to skip over a given number of elements in
		 * a sequence and then return the remainder.
		 * 
		 * @param skip
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery skip(int skip) {
			if (skip > 0) {
				this.skip = skip;
			}

			return this;
		}

		/**
		 * The inlinecount property specifies whether or not to retrieve a
		 * property with the number of records returned.
		 * 
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery includeInlineCount() {
			this.hasInlineCount = true;

			return this;
		}

		/**
		 * Specifies the fields to retrieve
		 * 
		 * @param fields
		 *            Names of the fields to retrieve
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery select(String... fields) {
			this.projection = new ArrayList<String>();
			for (String field : fields) {
				this.projection.add(field);
			}

			return this;
		}

		/**** Query Operations ****/

		/**
		 * Specifies the field to use
		 * 
		 * @param fieldName
		 *            The field to use
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery field(String fieldName) {
			this.querySteps.add(MobileServiceQueryOperations.field(fieldName));
			return this;
		}

		/**
		 * Specifies a numeric value
		 * 
		 * @param number
		 *            The numeric value to use
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery val(Number number) {
			this.querySteps.add(MobileServiceQueryOperations.val(number));
			return this;
		}

		/**
		 * Specifies a boolean value
		 * 
		 * @param number
		 *            The boolean value to use
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery val(boolean val) {
			this.querySteps.add(MobileServiceQueryOperations.val(val));
			return this;
		}

		/**
		 * Specifies a string value
		 * 
		 * @param number
		 *            The string value to use
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery val(String s) {
			this.querySteps.add(MobileServiceQueryOperations.val(s));
			return this;
		}

		/****** Logical Operators ******/

		/**
		 * Conditional and.
		 * 
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery and() {
			this.querySteps.add(MobileServiceQueryOperations.and());
			return this;
		}

		/**
		 * Conditional and.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery and(MobileServiceQuery otherQuery) {
			this.querySteps.add(MobileServiceQueryOperations.and(otherQuery));
			return this;
		}

		/**
		 * Conditional or.
		 * 
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery or() {
			this.querySteps.add(MobileServiceQueryOperations.or());
			return this;
		}

		/**
		 * Conditional or.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery or(MobileServiceQuery otherQuery) {
			this.querySteps.add(MobileServiceQueryOperations.or(otherQuery));
			return this;
		}

		/**
		 * Logical not.
		 * 
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery not() {
			this.querySteps.add(MobileServiceQueryOperations.not());
			return this;
		}

		/**
		 * Logical not.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery not(MobileServiceQuery otherQuery) {
			this.querySteps.add(MobileServiceQueryOperations.not(otherQuery));
			return this;
		}

		/****** Comparison Operators ******/

		/**
		 * Greater than or equal comparison operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery ge() {
			this.querySteps.add(MobileServiceQueryOperations.ge());
			return this;
		}

		/**
		 * Greater than or equal comparison operator.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery ge(MobileServiceQuery otherQuery) {
			this.querySteps.add(MobileServiceQueryOperations.ge(otherQuery));
			return this;
		}

		/**
		 * Less than or equal comparison operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery le() {
			this.querySteps.add(MobileServiceQueryOperations.le());
			return this;
		}

		/**
		 * Less than or equal comparison operator.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery le(MobileServiceQuery otherQuery) {
			this.querySteps.add(MobileServiceQueryOperations.le(otherQuery));
			return this;
		}

		/**
		 * Greater than comparison operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery gt() {
			this.querySteps.add(MobileServiceQueryOperations.gt());
			return this;
		}

		/**
		 * Greater than comparison operator.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery gt(MobileServiceQuery otherQuery) {
			this.querySteps.add(MobileServiceQueryOperations.gt(otherQuery));
			return this;
		}

		/**
		 * Less than comparison operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery lt() {
			this.querySteps.add(MobileServiceQueryOperations.lt());
			return this;
		}

		/**
		 * Less than comparison operator.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery lt(MobileServiceQuery otherQuery) {
			this.querySteps.add(MobileServiceQueryOperations.lt(otherQuery));
			return this;
		}

		/**
		 * Equal comparison operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery eq() {
			this.querySteps.add(MobileServiceQueryOperations.eq());
			return this;
		}

		/**
		 * Equal comparison operator.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery eq(MobileServiceQuery otherQuery) {
			this.querySteps.add(MobileServiceQueryOperations.eq(otherQuery));
			return this;
		}

		/**
		 * Not equal comparison operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery neq() {
			this.querySteps.add(MobileServiceQueryOperations.neq());
			return this;
		}

		/**
		 * Not equal comparison operator.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery neq(MobileServiceQuery otherQuery) {
			this.querySteps.add(MobileServiceQueryOperations.neq(otherQuery));
			return this;
		}

		/****** Arithmetic Operators ******/

		/**
		 * Add operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery add() {
			this.querySteps.add(MobileServiceQueryOperations.add());
			return this;
		}

		/**
		 * Add operator.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery add(MobileServiceQuery otherQuery) {
			this.querySteps.add(MobileServiceQueryOperations.add(otherQuery));
			return this;
		}

		/**
		 * Subtract operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery sub() {
			this.querySteps.add(MobileServiceQueryOperations.sub());
			return this;
		}

		/**
		 * Subtract operator.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery sub(MobileServiceQuery otherQuery) {
			this.querySteps.add(MobileServiceQueryOperations.sub(otherQuery));
			return this;
		}

		/**
		 * Multiply operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery mul() {
			this.querySteps.add(MobileServiceQueryOperations.mul());
			return this;
		}

		/**
		 * Multiply operator.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery mul(MobileServiceQuery otherQuery) {
			this.querySteps.add(MobileServiceQueryOperations.mul(otherQuery));
			return this;
		}

		/**
		 * Divide operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery div() {
			this.querySteps.add(MobileServiceQueryOperations.div());
			return this;
		}

		/**
		 * Divide operator.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery div(MobileServiceQuery otherQuery) {
			this.querySteps.add(MobileServiceQueryOperations.div(otherQuery));
			return this;
		}

		/**
		 * Reminder (or modulo) operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery mod() {
			this.querySteps.add(MobileServiceQueryOperations.mod());
			return this;
		}

		/**
		 * Reminder (or modulo) operator.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery mod(MobileServiceQuery otherQuery) {
			this.querySteps.add(MobileServiceQueryOperations.mod(otherQuery));
			return this;
		}

		/****** Date Operators ******/

		/**
		 * The year component value of the parameter value.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery year(MobileServiceQuery otherQuery) {
			this.querySteps.add(MobileServiceQueryOperations.year(otherQuery));
			return this;
		}

		/**
		 * The month component value of the parameter value.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery month(MobileServiceQuery otherQuery) {
			this.querySteps.add(MobileServiceQueryOperations.month(otherQuery));
			return this;
		}

		/**
		 * The day component value of the parameter value.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery day(MobileServiceQuery otherQuery) {
			this.querySteps.add(MobileServiceQueryOperations.day(otherQuery));
			return this;
		}

		/**
		 * The hour component value of the parameter value.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery hour(MobileServiceQuery otherQuery) {
			this.querySteps.add(MobileServiceQueryOperations.hour(otherQuery));
			return this;
		}

		/**
		 * The minute component value of the parameter value.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery minute(MobileServiceQuery otherQuery) {
			this.querySteps
					.add(MobileServiceQueryOperations.minute(otherQuery));
			return this;
		}

		/**
		 * The second component value of the parameter value.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery second(MobileServiceQuery otherQuery) {
			this.querySteps
					.add(MobileServiceQueryOperations.second(otherQuery));
			return this;
		}

		/****** Math Functions ******/

		/**
		 * The largest integral value less than or equal to the parameter value.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery floor(MobileServiceQuery otherQuery) {
			this.querySteps.add(MobileServiceQueryOperations.floor(otherQuery));
			return this;
		}

		/**
		 * The smallest integral value greater than or equal to the parameter
		 * value.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery ceiling(MobileServiceQuery otherQuery) {
			this.querySteps.add(MobileServiceQueryOperations
					.ceiling(otherQuery));
			return this;
		}

		/**
		 * The nearest integral value to the parameter value.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery round(MobileServiceQuery otherQuery) {
			this.querySteps.add(MobileServiceQueryOperations.round(otherQuery));
			return this;
		}

		/****** String Operators ******/

		/**
		 * Whether the beginning of the first parameter values matches the
		 * second parameter value.
		 * 
		 * @param field
		 *            The field to evaluate
		 * @param start
		 *            Start value
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery startsWith(MobileServiceQuery field,
				MobileServiceQuery start) {
			this.querySteps.add(MobileServiceQueryOperations.startsWith(field,
					start));
			return this;
		}

		/**
		 * Whether the end of the first parameter value matches the second
		 * parameter value.
		 * 
		 * @param field
		 *            The field to evaluate
		 * @param end
		 *            End value
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery endsWith(MobileServiceQuery field,
				MobileServiceQuery end) {
			this.querySteps.add(MobileServiceQueryOperations.endsWith(field,
					end));
			return this;
		}

		/**
		 * Whether the second parameter string value occurs in the first
		 * parameter string value.
		 * 
		 * @param str1
		 *            First string
		 * @param str2
		 *            Second string
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery subStringOf(MobileServiceQuery str1,
				MobileServiceQuery str2) {
			this.querySteps.add(MobileServiceQueryOperations.subStringOf(str1,
					str2));
			return this;
		}

		/**
		 * String value which is the first and second parameter values merged
		 * together with the first parameter value coming first in the result.
		 * 
		 * @param str1
		 *            First string
		 * @param str2
		 *            Second string
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery concat(MobileServiceQuery str1,
				MobileServiceQuery str2) {
			this.querySteps
					.add(MobileServiceQueryOperations.concat(str1, str2));
			return this;
		}

		/**
		 * Index of the first occurrence of the second parameter value in the
		 * first parameter value or -1 otherwise.
		 * 
		 * @param haystack
		 *            String content
		 * @param needle
		 *            Value to search for
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery indexOf(MobileServiceQuery haystack,
				MobileServiceQuery needle) {
			this.querySteps.add(MobileServiceQueryOperations.indexOf(haystack,
					needle));
			return this;
		}

		/**
		 * String value starting at the character index specified by the second
		 * parameter value in the first parameter string value.
		 * 
		 * @param str
		 *            String content
		 * @param pos
		 *            Starting position
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery substring(MobileServiceQuery str,
				MobileServiceQuery pos) {
			this.querySteps.add(MobileServiceQueryOperations
					.substring(str, pos));
			return this;
		}

		/**
		 * String value starting at the character index specified by the second
		 * parameter value in the first parameter string value.
		 * 
		 * @param str
		 *            String content
		 * @param pos
		 *            Starting position
		 * @param length
		 *            Length
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery substring(MobileServiceQuery str,
				MobileServiceQuery pos, MobileServiceQuery length) {
			this.querySteps.add(MobileServiceQueryOperations.substring(str,
					pos, length));
			return this;
		}

		/**
		 * Finds the second string parameter in the first parameter string value
		 * and replaces it with the third parameter value.
		 * 
		 * @param str
		 *            String content
		 * @param find
		 *            Search value
		 * @param replace
		 *            Replace value
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery replace(MobileServiceQuery str,
				MobileServiceQuery find, MobileServiceQuery replace) {
			this.querySteps.add(MobileServiceQueryOperations.replace(str, find,
					replace));
			return this;
		}
	}

	/**
	 * Static class used to create query operations
	 */
	public static class MobileServiceQueryOperations {
		/**
		 * Dummy MobileServiceTable used to create MobileServiceQuery objects
		 */
		private static MobileServiceTable table = new MobileServiceTable();

		/**
		 * Creates a MobileServiceQuery representing a function call
		 * 
		 * @param functionName
		 *            The function name
		 * @param parameters
		 *            The function parameters
		 * @return The MobileServiceQuery representing a function call
		 */
		private static MobileServiceQuery function(String functionName,
				MobileServiceQuery... parameters) {

			MobileServiceQuery query = table.new MobileServiceQuery();

			query.setQueryText(functionName);

			for (MobileServiceQuery p : parameters) {
				query.addInternalValue(p);
			}

			return query;
		}

		/**
		 * Creates a MobileServiceQuery representing an operator
		 * 
		 * @param otherQuery
		 *            The query to operateWith
		 * @param operator
		 *            The operator
		 * @return The MobileServiceQuery representing an operation
		 */
		private static MobileServiceQuery simpleOperator(
				MobileServiceQuery otherQuery, String operator) {
			MobileServiceQuery query = table.new MobileServiceQuery();

			query.setQueryText(operator + " ");

			if (otherQuery != null) {
				query.addInternalValue(otherQuery);
			}

			return query;
		}

		/**
		 * Sanitizes the string to use in a oData query
		 * 
		 * @param s
		 *            The string to sanitize
		 * @return The sanitized string
		 */
		private static String sanitize(String s) {
			if (s != null) {
				return s.replace("'", "''");
			} else {
				return null;
			}
		}

		public static MobileServiceQuery field(String fieldName) {
			if (fieldName == null || fieldName.trim().length() == 0) {
				throw new InvalidParameterException(
						"fieldName cannot be null or empty");
			}

			MobileServiceQuery query = table.new MobileServiceQuery();

			query.setQueryText(fieldName);

			return query;
		}

		/**
		 * Creates a MobileServiceQuery representing a numeric value
		 * 
		 * @param number
		 *            the number to represent
		 * @return the MobileServiceQuery
		 */
		public static MobileServiceQuery val(Number number) {
			MobileServiceQuery query = table.new MobileServiceQuery();

			query.setQueryText(number.toString());

			return query;
		}

		/**
		 * Creates a MobileServiceQuery representing a boolean value
		 * 
		 * @param val
		 *            the boolean to represent
		 * @return the MobileServiceQuery
		 */
		public static MobileServiceQuery val(boolean val) {
			MobileServiceQuery query = table.new MobileServiceQuery();

			query.setQueryText(Boolean.valueOf(val).toString()
					.toLowerCase(Locale.getDefault()));

			return query;
		}

		/**
		 * Creates a MobileServiceQuery representing a string value
		 * 
		 * @param s
		 *            the string to represent
		 * @return the MobileServiceQuery
		 */
		public static MobileServiceQuery val(String s) {

			MobileServiceQuery query = table.new MobileServiceQuery();

			if (s == null) {
				query.setQueryText("null");
			} else {
				query.setQueryText("'" + sanitize(s) + "'");
			}

			return query;
		}

		/****** Logical Operators ******/

		/**
		 * Conditional and.
		 * 
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery and() {
			return and(null);
		}

		/**
		 * Conditional and.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery and(MobileServiceQuery otherQuery) {
			return simpleOperator(otherQuery, "and");
		}

		/**
		 * Conditional or.
		 * 
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery or() {
			return or(null);
		}

		/**
		 * Conditional or.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery or(MobileServiceQuery otherQuery) {
			return simpleOperator(otherQuery, "or");
		}

		/**
		 * Logical not.
		 * 
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery not() {
			return not(null);
		}

		/**
		 * Logical not.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery not(MobileServiceQuery otherQuery) {
			return simpleOperator(otherQuery, "not");
		}

		/****** Comparison Operators ******/

		/**
		 * Greater than or equal comparison operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery ge() {
			return ge(null);
		}

		/**
		 * Greater than or equal comparison operator.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery ge(MobileServiceQuery otherQuery) {
			return simpleOperator(otherQuery, "ge");
		}

		/**
		 * Less than or equal comparison operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery le() {
			return le(null);
		}

		/**
		 * Less than or equal comparison operator.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery le(MobileServiceQuery otherQuery) {
			return simpleOperator(otherQuery, "le");
		}

		/**
		 * Greater than comparison operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery gt() {
			return gt(null);
		}

		/**
		 * Greater than comparison operator.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery gt(MobileServiceQuery otherQuery) {
			return simpleOperator(otherQuery, "gt");
		}

		/**
		 * Less than comparison operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery lt() {
			return lt(null);
		}

		/**
		 * Less than comparison operator.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery lt(MobileServiceQuery otherQuery) {
			return simpleOperator(otherQuery, "lt");
		}

		/**
		 * Equal comparison operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery eq() {
			return eq(null);
		}

		/**
		 * Equal comparison operator.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery eq(MobileServiceQuery otherQuery) {
			return simpleOperator(otherQuery, "eq");
		}

		/**
		 * Not equal comparison operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery neq() {
			return neq(null);
		}

		/**
		 * Not equal comparison operator.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery neq(MobileServiceQuery otherQuery) {
			return simpleOperator(otherQuery, "neq");
		}

		/****** Arithmetic Operators ******/

		/**
		 * Add operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery add() {
			return add(null);
		}

		/**
		 * Add operator.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery add(MobileServiceQuery otherQuery) {
			return simpleOperator(otherQuery, "add");
		}

		/**
		 * Subtract operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery sub() {
			return sub(null);
		}

		/**
		 * Subtract operator.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery sub(MobileServiceQuery otherQuery) {
			return simpleOperator(otherQuery, "sub");
		}

		/**
		 * Multiply operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery mul() {
			return mul(null);
		}

		/**
		 * Multiply operator.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery mul(MobileServiceQuery otherQuery) {
			return simpleOperator(otherQuery, "mul");
		}

		/**
		 * Divide operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery div() {
			return div(null);
		}

		/**
		 * Divide operator.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery div(MobileServiceQuery otherQuery) {
			return simpleOperator(otherQuery, "div");
		}

		/**
		 * Reminder (or modulo) operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery mod() {
			return mod(null);
		}

		/**
		 * Reminder (or modulo) operator.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery mod(MobileServiceQuery otherQuery) {
			return simpleOperator(otherQuery, "mod");
		}

		/****** Date Functions ******/

		/**
		 * The year component value of the parameter value.
		 * 
		 * @param exp
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery year(MobileServiceQuery exp) {
			return function("year", exp);
		}

		/**
		 * The month component value of the parameter value.
		 * 
		 * @param exp
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery month(MobileServiceQuery exp) {
			return function("month", exp);
		}

		/**
		 * The day component value of the parameter value.
		 * 
		 * @param exp
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery day(MobileServiceQuery exp) {
			return function("day", exp);
		}

		/**
		 * The hour component value of the parameter value.
		 * 
		 * @param exp
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery hour(MobileServiceQuery exp) {
			return function("hour", exp);
		}

		/**
		 * The minute component value of the parameter value.
		 * 
		 * @param exp
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery minute(MobileServiceQuery exp) {
			return function("minute", exp);
		}

		/**
		 * The second component value of the parameter value.
		 * 
		 * @param exp
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery second(MobileServiceQuery exp) {
			return function("second", exp);
		}

		/****** Math Functions ******/

		/**
		 * The largest integral value less than or equal to the parameter value.
		 * 
		 * @param exp
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery floor(MobileServiceQuery exp) {
			return function("floor", exp);
		}

		/**
		 * The smallest integral value greater than or equal to the parameter
		 * value.
		 * 
		 * @param exp
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery ceiling(MobileServiceQuery exp) {
			return function("ceiling", exp);
		}

		/**
		 * The nearest integral value to the parameter value.
		 * 
		 * @param exp
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery round(MobileServiceQuery exp) {
			return function("round", exp);
		}

		/****** String Functions ******/

		/**
		 * String value with the contents of the parameter value converted to
		 * lower case.
		 * 
		 * @param exp
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery toLower(MobileServiceQuery exp) {
			return function("toLower", exp);
		}

		/**
		 * String value with the contents of the parameter value converted to
		 * upper case
		 * 
		 * @param exp
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery toUpper(MobileServiceQuery exp) {
			return function("toUpper", exp);
		}

		/**
		 * The number of characters in the specified parameter value.
		 * 
		 * @param exp
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery length(MobileServiceQuery exp) {
			return function("length", exp);
		}

		/**
		 * String value with the contents of the parameter value with all
		 * leading and trailing white-space characters removed.
		 * 
		 * @param exp
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery trim(MobileServiceQuery exp) {
			return function("trim", exp);
		}

		/**
		 * Whether the beginning of the first parameter values matches the
		 * second parameter value.
		 * 
		 * @param field
		 *            The field to evaluate.
		 * @param start
		 *            Start value.
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery startsWith(MobileServiceQuery field,
				MobileServiceQuery start) {
			return function("startswith", field, start);
		}

		/**
		 * Whether the end of the first parameter value matches the second
		 * parameter value.
		 * 
		 * @param field
		 *            The field to evaluate.
		 * @param end
		 *            End value.
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery endsWith(MobileServiceQuery field,
				MobileServiceQuery end) {
			return function("endswith", field, end);
		}

		/**
		 * Whether the second parameter string value occurs in the first
		 * parameter string value.
		 * 
		 * @param str1
		 *            First string
		 * @param str2
		 *            Second string
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery subStringOf(MobileServiceQuery str1,
				MobileServiceQuery str2) {
			return function("substringof", str1, str2);
		}

		/**
		 * String value which is the first and second parameter values merged
		 * together with the first parameter value coming first in the result.
		 * 
		 * @param str1
		 *            First string
		 * @param str2
		 *            Second string
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery concat(MobileServiceQuery str1,
				MobileServiceQuery str2) {
			return function("concat", str1, str2);
		}

		/**
		 * Index of the first occurrence of the second parameter value in the
		 * first parameter value or -1 otherwise.
		 * 
		 * @param haystack
		 *            String content
		 * @param needle
		 *            Value to search for
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery indexOf(MobileServiceQuery haystack,
				MobileServiceQuery needle) {
			return function("indexof", haystack, needle);
		}

		/**
		 * String value starting at the character index specified by the second
		 * parameter value in the first parameter string value.
		 * 
		 * @param str
		 *            String content
		 * @param pos
		 *            Starting position
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery substring(MobileServiceQuery str,
				MobileServiceQuery pos) {
			return function("substring", str, pos);
		}

		/**
		 * String value starting at the character index specified by the second
		 * parameter value in the first parameter string value.
		 * 
		 * @param str
		 *            String content
		 * @param pos
		 *            Starting position
		 * @param length
		 *            Length
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery substring(MobileServiceQuery str,
				MobileServiceQuery pos, MobileServiceQuery length) {
			return function("substring", str, pos, length);
		}

		/**
		 * Finds the second string parameter in the first parameter string value
		 * and replaces it with the third parameter value.
		 * 
		 * @param str
		 *            String content
		 * @param find
		 *            Search value
		 * @param replace
		 *            Replace value
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery replace(MobileServiceQuery str,
				MobileServiceQuery find, MobileServiceQuery replace) {
			return function("replace", str, find, replace);
		}
	}

}
