/*
 * MobileServiceTable.java
 */

package com.microsoft.windowsazure.mobileservices;

import java.io.UnsupportedEncodingException;
import java.lang.reflect.Type;
import java.net.URLEncoder;
import java.security.InvalidParameterException;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Locale;
import java.util.Map;

import org.apache.http.client.methods.HttpDelete;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.client.methods.HttpPatch;
import org.apache.http.client.methods.HttpPost;

import android.util.Pair;

import com.google.gson.Gson;
import com.google.gson.JsonArray;
import com.google.gson.JsonDeserializer;
import com.google.gson.JsonElement;
import com.google.gson.JsonNull;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.google.gson.JsonSerializer;

//

/**
 * Represents a Mobile Service Table
 */
public final class MobileServiceTable {

	/**
	 * Tables URI part
	 */
	private static final String TABLES_URL = "tables/";

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
	 * Registers a JsonSerializer for the specified type
	 * 
	 * @param type
	 *            The type to use in the registration
	 * @param serializer
	 *            The serializer to use in the registration
	 */
	public <T> void registerSerializer(Type type, JsonSerializer<T> serializer) {
		mClient.getGsonBuilder().registerTypeAdapter(type, serializer);
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
		mClient.getGsonBuilder().registerTypeAdapter(type, deserializer);
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

		lookUp(id, new TableJsonOperationCallback() {

			@Override
			public void onCompleted(JsonObject jsonEntity, Exception exception,
					ServiceFilterResponse response) {
				if (callback != null) {
					if (exception == null) {
						E entity = null;
						Exception ex = null;
						try {
							entity = parseResults(jsonEntity, clazz).get(0);
						} catch (Exception e) {
							ex = e;
						}

						callback.onCompleted(entity, ex, response);
					} else {
						callback.onCompleted(null, exception, response);
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
				callback.onCompleted(null, e, null);
			}
			return;
		}

		executeGetRecords(url, new TableJsonQueryCallback() {

			@Override
			public void onCompleted(JsonElement results, int count,
					Exception exception, ServiceFilterResponse response) {
				if (callback != null) {
					if (exception == null) {
						if (results.isJsonArray()) { // empty result
							callback.onCompleted(
									null,
									new MobileServiceException(
											"A record with the specified Id cannot be found"),
									response);
						} else { // Lookup result
							callback.onCompleted(results.getAsJsonObject(),
									exception, response);
						}
					} else {
						callback.onCompleted(null, exception, response);
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
		final JsonObject json = mClient.getGsonBuilder().create()
				.toJsonTree(element).getAsJsonObject();

		insert(json, new TableJsonOperationCallback() {

			@SuppressWarnings("unchecked")
			@Override
			public void onCompleted(JsonObject jsonEntity, Exception exception,
					ServiceFilterResponse response) {
				if (callback != null) {
					if (exception == null) {
						E entity = null;
						Exception ex = null;
						try {
							entity = (E) parseResults(jsonEntity,
									element.getClass()).get(0);
						} catch (Exception e) {
							ex = e;
						}

						callback.onCompleted(entity, ex, response);
					} else {
						callback.onCompleted(null, exception, response);
					}
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

		for (int i = 0; i < 4; i++) {
			String idProperty = idPropertyNames[i];
			if (json.has(idProperty)) {
				JsonElement idElement = json.get(idProperty);
				if (!idElement.isJsonNull() && idElement.getAsInt() != 0) {
					throw new InvalidParameterException(
							"The entity to insert should not have "
									+ idProperty + " property defined");
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

		try {
			removeIdFromJson(element);
		} catch (InvalidParameterException e) {
			if (callback != null) {
				callback.onCompleted(null, e, null);
			}
			return;
		}

		String content = element.toString();

		ServiceFilterRequest post;
		try {
			post = new ServiceFilterRequestImpl(new HttpPost(mClient
					.getAppUrl().toString()
					+ TABLES_URL
					+ URLEncoder.encode(mTableName,
							MobileServiceClient.UTF8_ENCODING)));
		} catch (UnsupportedEncodingException e) {
			if (callback != null) {
				callback.onCompleted(null, e, null);
			}
			return;
		}

		try {
			post.setContent(content);
		} catch (Exception e) {
			if (callback != null) {
				callback.onCompleted(null, e, null);
			}
			return;
		}

		executeTableOperation(post, new TableJsonOperationCallback() {

			@Override
			public void onCompleted(JsonObject jsonEntity, Exception exception,
					ServiceFilterResponse response) {
				if (callback != null) {
					if (exception == null) {
						JsonObject patchedJson = patchOriginalEntityWithResponseEntity(
								element, jsonEntity);

						callback.onCompleted(patchedJson, exception, response);
					} else {
						callback.onCompleted(jsonEntity, exception, response);
					}
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
		final JsonObject json = mClient.getGsonBuilder().create()
				.toJsonTree(element).getAsJsonObject();

		update(json, new TableJsonOperationCallback() {

			@SuppressWarnings("unchecked")
			@Override
			public void onCompleted(JsonObject jsonEntity, Exception exception,
					ServiceFilterResponse response) {
				if (callback != null) {
					if (exception == null) {
						E entity = null;
						Exception ex = null;
						try {
							entity = (E) parseResults(jsonEntity,
									element.getClass()).get(0);
						} catch (Exception e) {
							ex = e;
						}

						callback.onCompleted(entity, ex, response);
					} else {
						callback.onCompleted(null, exception, response);
					}
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

		ServiceFilterRequest patch;
		try {
			patch = new ServiceFilterRequestImpl(new HttpPatch(mClient
					.getAppUrl().toString()
					+ TABLES_URL
					+ URLEncoder.encode(mTableName,
							MobileServiceClient.UTF8_ENCODING)
					+ "/"
					+ Integer.valueOf(getObjectId(element)).toString()));
		} catch (UnsupportedEncodingException e) {
			if (callback != null) {
				callback.onCompleted(null, e, null);
			}
			return;
		}

		try {
			patch.setContent(content);
		} catch (Exception e) {
			if (callback != null) {
				callback.onCompleted(null, e, null);
			}
			return;
		}

		executeTableOperation(patch, new TableJsonOperationCallback() {

			@Override
			public void onCompleted(JsonObject jsonEntity, Exception exception,
					ServiceFilterResponse response) {
				if (callback != null) {
					if (exception == null) {
						JsonObject patchedJson = patchOriginalEntityWithResponseEntity(
								element, jsonEntity);
						callback.onCompleted(patchedJson, exception, response);
					} else {
						callback.onCompleted(jsonEntity, exception, response);
					}
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
		int id = -1;
		try {
			id = getObjectId(element);
		} catch (Exception e) {
			callback.onCompleted(e, null);
			return;
		}
		
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
		ServiceFilterRequest delete;
		try {
			delete = new ServiceFilterRequestImpl(new HttpDelete(mClient
					.getAppUrl().toString()
					+ TABLES_URL
					+ URLEncoder.encode(mTableName,
							MobileServiceClient.UTF8_ENCODING)
					+ "/"
					+ Integer.valueOf(id).toString()));
		} catch (UnsupportedEncodingException e) {
			if (callback != null) {
				callback.onCompleted(e, null);
			}
			return;
		}

		// Create AsyncTask to execute the request
		new RequestAsyncTask(delete, mClient.createConnection()) {
			@Override
			protected void onPostExecute(ServiceFilterResponse result) {
				if (callback != null) {
					callback.onCompleted(mTaskException, result);
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

						callback.onCompleted(newEntityJson, null, result);

					} else {
						callback.onCompleted(null, mTaskException, result);
						;
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
		Gson gson = mClient.getGsonBuilder().create();
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
		if (element == null) {
			throw new InvalidParameterException("Element cannot be null"); 
		} else if (element instanceof Integer) {
			return ((Integer) element).intValue();
		}
			
		JsonObject jsonElement;
		if (element instanceof JsonObject) {
			jsonElement = (JsonObject)element;
		} else {
			jsonElement = mClient.getGsonBuilder().create()
					.toJsonTree(element).getAsJsonObject();
		}
		
		JsonElement idProperty = jsonElement.get("id");
		if (idProperty instanceof JsonNull) {
			throw new InvalidParameterException("Element must contain id property");
		}

		return idProperty.getAsInt();
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
			} else {
				String rowSetModifiers = query.getRowSetModifiers();
				if (rowSetModifiers.length() > 0) {
					url += "?" + query.getRowSetModifiers().substring(1);
				}
			}

		} catch (UnsupportedEncodingException e) {
			if (callback != null) {
				callback.onCompleted(null, 0, e, null);
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
		Gson gson = mClient.getGsonBuilder().create();
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

						int count = 0;

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
							callback.onCompleted(
									null,
									0,
									new MobileServiceException(
											"Error while retrieving data from response.",
											e), response);
							return;
						}

						callback.onCompleted(results, count, null, response);

					} else {
						callback.onCompleted(null, 0, mTaskException, response);
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
				public void onCompleted(JsonElement result, int count,
						Exception exception, ServiceFilterResponse response) {
					if (callback != null) {
						if (exception == null) {
							Exception ex = null;
							List<E> elements = null;
							try {
								elements = parseResults(result, clazz);
							} catch (Exception e) {
								ex = e;
							}

							callback.onCompleted(elements, count, ex, response);
						} else {
							callback.onCompleted(null, count, exception,
									response);
						}
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

		/**
		 * Specifies a date value
		 * 
		 * @param number
		 *            The date value to use
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery val(Date date) {
			this.querySteps.add(MobileServiceQueryOperations.val(date));
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

		/**
		 * Logical not.
		 * 
		 * @param booleanValue
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery not(boolean booleanValue) {
			this.querySteps.add(MobileServiceQueryOperations
					.not(MobileServiceQueryOperations.val(booleanValue)));
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
		 * Greater than or equal comparison operator.
		 * 
		 * @param numberValue
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery ge(Number numberValue) {
			this.querySteps.add(MobileServiceQueryOperations
					.ge(MobileServiceQueryOperations.val(numberValue)));
			return this;
		}

		/**
		 * Greater than or equal comparison operator.
		 * 
		 * @param dateValue
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery ge(Date dateValue) {
			this.querySteps.add(MobileServiceQueryOperations
					.ge(MobileServiceQueryOperations.val(dateValue)));
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
		 * Less than or equal comparison operator.
		 * 
		 * @param numberValue
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery le(Number numberValue) {
			this.querySteps.add(MobileServiceQueryOperations
					.le(MobileServiceQueryOperations.val(numberValue)));
			return this;
		}

		/**
		 * Less than or equal comparison operator.
		 * 
		 * @param dateValue
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery le(Date dateValue) {
			this.querySteps.add(MobileServiceQueryOperations
					.le(MobileServiceQueryOperations.val(dateValue)));
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
		 * Greater than comparison operator.
		 * 
		 * @param numberValue
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery gt(Number numberValue) {
			this.querySteps.add(MobileServiceQueryOperations
					.gt(MobileServiceQueryOperations.val(numberValue)));
			return this;
		}

		/**
		 * Greater than comparison operator.
		 * 
		 * @param dateValue
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery gt(Date dateValue) {
			this.querySteps.add(MobileServiceQueryOperations
					.gt(MobileServiceQueryOperations.val(dateValue)));
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
		 * Less than comparison operator.
		 * 
		 * @param numberValue
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery lt(Number numberValue) {
			this.querySteps.add(MobileServiceQueryOperations
					.lt(MobileServiceQueryOperations.val(numberValue)));
			return this;
		}

		/**
		 * Less than comparison operator.
		 * 
		 * @param dateValue
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery lt(Date dateValue) {
			this.querySteps.add(MobileServiceQueryOperations
					.lt(MobileServiceQueryOperations.val(dateValue)));
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
		 * Equal comparison operator.
		 * 
		 * @param numberValue
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery eq(Number numberValue) {
			this.querySteps.add(MobileServiceQueryOperations
					.eq(MobileServiceQueryOperations.val(numberValue)));
			return this;
		}

		/**
		 * Equal comparison operator.
		 * 
		 * @param booleanValue
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery eq(boolean booleanValue) {
			this.querySteps.add(MobileServiceQueryOperations
					.eq(MobileServiceQueryOperations.val(booleanValue)));
			return this;
		}

		/**
		 * Equal comparison operator.
		 * 
		 * @param stringValue
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery eq(String stringValue) {
			this.querySteps.add(MobileServiceQueryOperations
					.eq(MobileServiceQueryOperations.val(stringValue)));
			return this;
		}

		/**
		 * Equal comparison operator.
		 * 
		 * @param dateValue
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery eq(Date dateValue) {
			this.querySteps.add(MobileServiceQueryOperations
					.eq(MobileServiceQueryOperations.val(dateValue)));
			return this;
		}

		/**
		 * Not equal comparison operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery ne() {
			this.querySteps.add(MobileServiceQueryOperations.ne());
			return this;
		}

		/**
		 * Not equal comparison operator.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery ne(MobileServiceQuery otherQuery) {
			this.querySteps.add(MobileServiceQueryOperations.ne(otherQuery));
			return this;
		}

		/**
		 * Not equal comparison operator.
		 * 
		 * @param numberValue
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery ne(Number numberValue) {
			this.querySteps.add(MobileServiceQueryOperations
					.ne(MobileServiceQueryOperations.val(numberValue)));
			return this;
		}

		/**
		 * Not equal comparison operator.
		 * 
		 * @param booleanValue
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery ne(boolean booleanValue) {
			this.querySteps.add(MobileServiceQueryOperations
					.ne(MobileServiceQueryOperations.val(booleanValue)));
			return this;
		}

		/**
		 * Not equal comparison operator.
		 * 
		 * @param stringValue
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery ne(String stringValue) {
			this.querySteps.add(MobileServiceQueryOperations
					.ne(MobileServiceQueryOperations.val(stringValue)));
			return this;
		}

		/**
		 * Not equal comparison operator.
		 * 
		 * @param dateValue
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery ne(Date dateValue) {
			this.querySteps.add(MobileServiceQueryOperations
					.ne(MobileServiceQueryOperations.val(dateValue)));
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
		 * Add operator.
		 * 
		 * @param val
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery add(Number val) {
			this.querySteps.add(MobileServiceQueryOperations.add(val));
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
		 * Subtract operator.
		 * 
		 * @param val
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery sub(Number val) {
			this.querySteps.add(MobileServiceQueryOperations.sub(val));
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
		 * Multiply operator.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery mul(Number val) {
			this.querySteps.add(MobileServiceQueryOperations.mul(val));
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
		 * Divide operator.
		 * 
		 * @param val
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery div(Number val) {
			this.querySteps.add(MobileServiceQueryOperations.div(val));
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

		/**
		 * Reminder (or modulo) operator.
		 * 
		 * @param val
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery mod(Number val) {
			this.querySteps.add(MobileServiceQueryOperations.mod(val));
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
		 * The year component value of the parameter value.
		 * 
		 * @param field
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery year(String field) {
			this.querySteps.add(MobileServiceQueryOperations.year(field));
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
		 * The month component value of the parameter value.
		 * 
		 * @param field
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery month(String field) {
			this.querySteps.add(MobileServiceQueryOperations.month(field));
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
		 * The day component value of the parameter value.
		 * 
		 * @param field
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery day(String field) {
			this.querySteps.add(MobileServiceQueryOperations.day(field));
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
		 * The hour component value of the parameter value.
		 * 
		 * @param field
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery hour(String field) {
			this.querySteps.add(MobileServiceQueryOperations.hour(field));
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
		 * The minute component value of the parameter value.
		 * 
		 * @param field
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery minute(String field) {
			this.querySteps.add(MobileServiceQueryOperations.minute(field));
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

		/**
		 * The second component value of the parameter value.
		 * 
		 * @param field
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery second(String field) {
			this.querySteps.add(MobileServiceQueryOperations.second(field));
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
		 * String value with the contents of the parameter value converted to
		 * lower case.
		 * 
		 * @param exp
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery toLower(MobileServiceQuery exp) {
			this.querySteps.add(MobileServiceQueryOperations.toLower(exp));
			return this;
		}

		/**
		 * String value with the contents of the parameter value converted to
		 * lower case.
		 * 
		 * @param field
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery toLower(String field) {
			this.querySteps.add(MobileServiceQueryOperations.toLower(field));
			return this;
		}

		/**
		 * String value with the contents of the parameter value converted to
		 * upper case.
		 * 
		 * @param exp
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery toUpper(MobileServiceQuery exp) {
			this.querySteps.add(MobileServiceQueryOperations.toUpper(exp));
			return this;
		}

		/**
		 * String value with the contents of the parameter value converted to
		 * upper case.
		 * 
		 * @param field
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery toUpper(String field) {
			this.querySteps.add(MobileServiceQueryOperations.toUpper(field));
			return this;
		}

		/**
		 * The number of characters in the specified parameter value.
		 * 
		 * @param exp
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery length(MobileServiceQuery exp) {
			this.querySteps.add(MobileServiceQueryOperations.length(exp));
			return this;
		}

		/**
		 * The number of characters in the specified parameter value.
		 * 
		 * @param field
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery length(String field) {
			this.querySteps.add(MobileServiceQueryOperations.length(field));
			return this;
		}

		/**
		 * String value with the contents of the parameter value with all
		 * leading and trailing white-space characters removed.
		 * 
		 * @param exp
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery trim(MobileServiceQuery exp) {
			this.querySteps.add(MobileServiceQueryOperations.trim(exp));
			return this;
		}

		/**
		 * String value with the contents of the parameter value with all
		 * leading and trailing white-space characters removed.
		 * 
		 * @param field
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery trim(String field) {
			this.querySteps.add(MobileServiceQueryOperations.trim(field));
			return this;
		}

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
		 * Whether the beginning of the first parameter values matches the
		 * second parameter value.
		 * 
		 * @param field
		 *            The field to evaluate
		 * @param start
		 *            Start value
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery startsWith(String field, String start) {
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
		 * Whether the end of the first parameter value matches the second
		 * parameter value.
		 * 
		 * @param field
		 *            The field to evaluate
		 * @param end
		 *            End value
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery endsWith(String field, String end) {
			this.querySteps.add(MobileServiceQueryOperations.endsWith(field,
					end));
			return this;
		}

		/**
		 * Whether the first parameter string value occurs in the second
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
		 * Whether the string parameter occurs in the field
		 * 
		 * @param str2
		 *            String to search
		 * @param field
		 *            Field to search in
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery subStringOf(String str, String field) {
			this.querySteps.add(MobileServiceQueryOperations.subStringOf(str,
					field));
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
		 * Index of the first occurrence of the second parameter value in the
		 * first parameter value or -1 otherwise.
		 * 
		 * @param field
		 *            Field to search in
		 * @param str
		 *            Value to search for
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery indexOf(String field, String needle) {
			this.querySteps.add(MobileServiceQueryOperations.indexOf(field,
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
		public MobileServiceQuery subString(MobileServiceQuery str,
				MobileServiceQuery pos) {
			this.querySteps.add(MobileServiceQueryOperations
					.subString(str, pos));
			return this;
		}

		/**
		 * String value starting at the character index specified by the second
		 * parameter value in the first parameter string value.
		 * 
		 * @param field
		 *            Field to scan
		 * @param pos
		 *            Starting position
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery subString(String field, int pos) {
			this.querySteps.add(MobileServiceQueryOperations.subString(field,
					pos));
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
		public MobileServiceQuery subString(MobileServiceQuery str,
				MobileServiceQuery pos, MobileServiceQuery length) {
			this.querySteps.add(MobileServiceQueryOperations.subString(str,
					pos, length));
			return this;
		}

		/**
		 * String value starting at the character index specified by the second
		 * parameter value in the first parameter string value.
		 * 
		 * @param field
		 *            Field to scan
		 * @param pos
		 *            Starting position
		 * @param length
		 *            Length
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery subString(String field, int pos, int length) {
			this.querySteps.add(MobileServiceQueryOperations.subString(field,
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

		/**
		 * Finds the second string parameter in the first parameter string value
		 * and replaces it with the third parameter value.
		 * 
		 * @param field
		 *            Field to scan
		 * @param find
		 *            Search value
		 * @param replace
		 *            Replace value
		 * @return MobileServiceQuery
		 */
		public MobileServiceQuery replace(String field, String find,
				String replace) {
			this.querySteps.add(MobileServiceQueryOperations.replace(field,
					find, replace));
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

		public static MobileServiceQuery query(MobileServiceQuery subQuery) {
			MobileServiceQuery query = table.new MobileServiceQuery();

			query.addInternalValue(subQuery);

			return query;
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

			if (number == null) {
				query.setQueryText("null");
			} else {
				query.setQueryText(number.toString());
			}

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

		/**
		 * Creates a MobileServiceQuery representing a date value
		 * 
		 * @param date
		 *            the date to represent
		 * @return the MobileServiceQuery
		 */
		public static MobileServiceQuery val(Date date) {

			MobileServiceQuery query = table.new MobileServiceQuery();

			if (date == null) {
				query.setQueryText("null");
			} else {
				query.setQueryText("'"
						+ sanitize(DateSerializer.serialize(date)) + "'");
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

		/**
		 * Logical not.
		 * 
		 * @param booleanValue
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery not(boolean booleanValue) {
			return not(MobileServiceQueryOperations.val(booleanValue));
		}

		/****** Comparison Operators ******/

		/**
		 * Greater than or equal comparison operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery ge() {
			MobileServiceQuery nullQuery = null;
			return ge(nullQuery);
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
		 * Greater than or equal comparison operator.
		 * 
		 * @param numberValue
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery ge(Number numberValue) {
			return ge(MobileServiceQueryOperations.val(numberValue));
		}

		/**
		 * Greater than or equal comparison operator.
		 * 
		 * @param dateValue
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery ge(Date dateValue) {
			return ge(MobileServiceQueryOperations.val(dateValue));
		}

		/**
		 * Less than or equal comparison operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery le() {
			MobileServiceQuery nullQuery = null;
			return le(nullQuery);
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
		 * Less than or equal comparison operator.
		 * 
		 * @param numberValue
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery le(Number numberValue) {
			return le(MobileServiceQueryOperations.val(numberValue));
		}

		/**
		 * Less than or equal comparison operator.
		 * 
		 * @param dateValue
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery le(Date dateValue) {
			return le(MobileServiceQueryOperations.val(dateValue));
		}

		/**
		 * Greater than comparison operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery gt() {
			MobileServiceQuery nullQuery = null;
			return gt(nullQuery);
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
		 * Greater than comparison operator.
		 * 
		 * @param numberValue
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery gt(Number numberValue) {
			return gt(MobileServiceQueryOperations.val(numberValue));
		}

		/**
		 * Greater than comparison operator.
		 * 
		 * @param dateValue
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery gt(Date dateValue) {
			return gt(MobileServiceQueryOperations.val(dateValue));
		}

		/**
		 * Less than comparison operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery lt() {
			MobileServiceQuery nullQuery = null;
			return lt(nullQuery);
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
		 * Less than comparison operator.
		 * 
		 * @param numberValue
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery lt(Number numberValue) {
			return lt(MobileServiceQueryOperations.val(numberValue));
		}

		/**
		 * Less than comparison operator.
		 * 
		 * @param dateValue
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery lt(Date dateValue) {
			return lt(MobileServiceQueryOperations.val(dateValue));
		}

		/**
		 * Equal comparison operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery eq() {
			MobileServiceQuery nullQuery = null;
			return eq(nullQuery);
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
		 * Equal comparison operator.
		 * 
		 * @param numberValue
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery eq(Number numberValue) {
			return eq(MobileServiceQueryOperations.val(numberValue));
		}

		/**
		 * Equal comparison operator.
		 * 
		 * @param booleanValue
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery eq(boolean booleanValue) {
			return eq(MobileServiceQueryOperations.val(booleanValue));
		}

		/**
		 * Equal comparison operator.
		 * 
		 * @param stringValue
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery eq(String stringValue) {
			return eq(MobileServiceQueryOperations.val(stringValue));
		}

		/**
		 * Equal comparison operator.
		 * 
		 * @param dateValue
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery eq(Date dateValue) {
			return eq(MobileServiceQueryOperations.val(dateValue));
		}

		/**
		 * Not equal comparison operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery ne() {
			MobileServiceQuery nullQuery = null;
			return ne(nullQuery);
		}

		/**
		 * Not equal comparison operator.
		 * 
		 * @param otherQuery
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery ne(MobileServiceQuery otherQuery) {
			return simpleOperator(otherQuery, "ne");
		}

		/**
		 * Not equal comparison operator.
		 * 
		 * @param numberValue
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery ne(Number numberValue) {
			return ne(MobileServiceQueryOperations.val(numberValue));
		}

		/**
		 * Not equal comparison operator.
		 * 
		 * @param booleanValue
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery ne(boolean booleanValue) {
			return ne(MobileServiceQueryOperations.val(booleanValue));
		}

		/**
		 * Not equal comparison operator.
		 * 
		 * @param stringValue
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery ne(String stringValue) {
			return ne(MobileServiceQueryOperations.val(stringValue));
		}

		/**
		 * Not equal comparison operator.
		 * 
		 * @param dateValue
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery ne(Date dateValue) {
			return ne(MobileServiceQueryOperations.val(dateValue));
		}

		/****** Arithmetic Operators ******/

		/**
		 * Add operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery add() {
			MobileServiceQuery nullQuery = null;
			return add(nullQuery);
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
		 * Add operator.
		 * 
		 * @param val
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery add(Number val) {
			return add(val(val));
		}

		/**
		 * Subtract operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery sub() {
			MobileServiceQuery nullQuery = null;
			return sub(nullQuery);
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
		 * Subtract operator.
		 * 
		 * @param val
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery sub(Number val) {
			return sub(val(val));
		}

		/**
		 * Multiply operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery mul() {
			MobileServiceQuery nullQuery = null;
			return mul(nullQuery);
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
		 * Multiply operator.
		 * 
		 * @param val
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery mul(Number val) {
			return mul(val(val));
		}

		/**
		 * Divide operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery div() {
			MobileServiceQuery nullQuery = null;
			return div(nullQuery);
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
		 * Divide operator.
		 * 
		 * @param val
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery div(Number val) {
			return div(val(val));
		}

		/**
		 * Reminder (or modulo) operator.
		 * 
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery mod() {
			MobileServiceQuery nullQuery = null;
			return mod(nullQuery);
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

		/**
		 * Reminder (or modulo) operator.
		 * 
		 * @param val
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery mod(Number val) {
			return mod(val(val));
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
		 * The year component value of the parameter value.
		 * 
		 * @param field
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery year(String field) {
			return function("year", field(field));
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
		 * The month component value of the parameter value.
		 * 
		 * @param field
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery month(String field) {
			return function("month", field(field));
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
		 * The day component value of the parameter value.
		 * 
		 * @param field
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery day(String field) {
			return function("day", field(field));
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
		 * The hour component value of the parameter value.
		 * 
		 * @param field
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery hour(String field) {
			return function("hour", field(field));
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
		 * The minute component value of the parameter value.
		 * 
		 * @param field
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery minute(String field) {
			return function("minute", field(field));
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

		/**
		 * The second component value of the parameter value.
		 * 
		 * @param field
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery second(String field) {
			return function("second", field(field));
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
			return function("tolower", exp);
		}

		/**
		 * String value with the contents of the parameter value converted to
		 * lower case.
		 * 
		 * @param field
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery toLower(String field) {
			return toLower(field(field));
		}

		/**
		 * String value with the contents of the parameter value converted to
		 * upper case
		 * 
		 * @param exp
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery toUpper(MobileServiceQuery exp) {
			return function("toupper", exp);
		}

		/**
		 * String value with the contents of the parameter value converted to
		 * upper case
		 * 
		 * @param field
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery toUpper(String field) {
			return toUpper(field(field));
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
		 * The number of characters in the specified parameter value.
		 * 
		 * @param field
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery length(String field) {
			return length(field(field));
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
		 * String value with the contents of the parameter value with all
		 * leading and trailing white-space characters removed.
		 * 
		 * @param exp
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery trim(String field) {
			return trim(field(field));
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
		 * Whether the beginning of the first parameter values matches the
		 * second parameter value.
		 * 
		 * @param field
		 *            The field to evaluate.
		 * @param start
		 *            Start value.
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery startsWith(String field, String start) {
			return startsWith(field(field), val(start));
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
		 * Whether the end of the first parameter value matches the second
		 * parameter value.
		 * 
		 * @param field
		 *            The field to evaluate.
		 * @param end
		 *            End value.
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery endsWith(String field, String end) {
			return endsWith(field(field), val(end));
		}

		/**
		 * Whether the first parameter string value occurs in the second
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
		 * Whether the string parameter occurs in the field
		 * 
		 * @param str
		 *            String to search
		 * @param field
		 *            Field to search in
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery subStringOf(String str, String field) {
			return subStringOf(val(str), field(field));
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
		 * Index of the first occurrence of the second parameter value in the
		 * first parameter value or -1 otherwise.
		 * 
		 * @param field
		 *            Field to seach in
		 * @param str
		 *            Value to search for
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery indexOf(String field, String str) {
			return indexOf(field(field), val(str));
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
		public static MobileServiceQuery subString(MobileServiceQuery str,
				MobileServiceQuery pos) {
			return function("substring", str, pos);
		}

		/**
		 * String value starting at the character index specified by the second
		 * parameter value in the first parameter string value.
		 * 
		 * @param field
		 *            Field to scan
		 * @param pos
		 *            Starting position
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery subString(String field, int pos) {
			return subString(field(field), val(pos));
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
		public static MobileServiceQuery subString(MobileServiceQuery str,
				MobileServiceQuery pos, MobileServiceQuery length) {
			return function("substring", str, pos, length);
		}

		/**
		 * String value starting at the character index specified by the second
		 * parameter value in the first parameter string value.
		 * 
		 * @param field
		 *            Field to scan
		 * @param pos
		 *            Starting position
		 * @param length
		 *            Length
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery subString(String field, int pos,
				int length) {
			return subString(field(field), val(pos), val(length));
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

		/**
		 * Finds the second string parameter in the first parameter string value
		 * and replaces it with the third parameter value.
		 * 
		 * @param field
		 *            Field to scan
		 * @param find
		 *            Search value
		 * @param replace
		 *            Replace value
		 * @return MobileServiceQuery
		 */
		public static MobileServiceQuery replace(String field, String find,
				String replace) {
			return replace(field(field), val(find), val(replace));
		}
	}

}
