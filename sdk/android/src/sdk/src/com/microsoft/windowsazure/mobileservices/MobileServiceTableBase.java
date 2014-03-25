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
package com.microsoft.windowsazure.mobileservices;

import java.lang.reflect.Field;
import java.util.ArrayList;
import java.util.EnumSet;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Map.Entry;
import java.util.TreeMap;

import org.apache.http.client.methods.HttpDelete;

import android.net.Uri;
import android.util.Pair;

import com.google.gson.JsonElement;
import com.google.gson.JsonNull;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.google.gson.JsonPrimitive;
import com.google.gson.annotations.SerializedName;

abstract class MobileServiceTableBase<E> {

	/**
	 * Tables URI part
	 */
	public static final String TABLES_URL = "tables/";
	
	/**
	 * The string prefix used to indicate system properties
	 */
	protected static final String SystemPropertyPrefix = "__";
	
	/**
	 * The name of the _system query string parameter
	 */
    protected static final String SystemPropertiesQueryParameterName = "__systemproperties";
    
    /**
	 * The system property names with the correct prefix.
	 */
    protected static final TreeMap<String,MobileServiceSystemProperty> SystemPropertyNameToEnum;
    static { 
    	SystemPropertyNameToEnum = new TreeMap<String,MobileServiceSystemProperty>(String.CASE_INSENSITIVE_ORDER);
    	SystemPropertyNameToEnum.put(getSystemPropertyString(MobileServiceSystemProperty.CreatedAt),MobileServiceSystemProperty.CreatedAt);
    	SystemPropertyNameToEnum.put(getSystemPropertyString(MobileServiceSystemProperty.UpdatedAt),MobileServiceSystemProperty.UpdatedAt);
    	SystemPropertyNameToEnum.put(getSystemPropertyString(MobileServiceSystemProperty.Version),MobileServiceSystemProperty.Version);
    }
    
    /**
	 * The version system property as a string with the prefix.
	 */
    protected static final String VersionSystemPropertyName = getSystemPropertyString(MobileServiceSystemProperty.Version);

    protected static final List<String> IdProperties;
    static { 
    	IdProperties = new ArrayList<String>();
    	IdProperties.add("id");
    	IdProperties.add("iD");
    	IdProperties.add("Id");
    	IdProperties.add("ID");
    }
    
	/**
	 * The MobileServiceClient used to invoke table operations
	 */
	protected MobileServiceClient mClient;

	/**
	 * The name of the represented table
	 */
	protected String mTableName;
	
	/**
	 * The Mobile Service system properties to be included with items.
	 */ 
	protected EnumSet<MobileServiceSystemProperty> mSystemProperties = EnumSet.noneOf(MobileServiceSystemProperty.class);

	protected void initialize(String name, MobileServiceClient client) {
		if (name == null || name.toString().trim().length() == 0) {
			throw new IllegalArgumentException("Invalid Table Name");
		}

		if (client == null) {
			throw new IllegalArgumentException("Invalid Mobile Service Client");
		}

		mClient = client;
		mTableName = name;
	}
	
	public abstract void execute(MobileServiceQuery<?> query, E callback);
	
	/**
	 * Executes a query to retrieve all the table rows
	 * 
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public void execute(E callback) {
		this.where().execute(callback);
	}

	/**
	 * Returns the name of the represented table
	 */
	public String getTableName() {
		return mTableName;
	}
	
	public EnumSet<MobileServiceSystemProperty> getSystemProperties() {
		return mSystemProperties;
	}

	public void setSystemProperties(EnumSet<MobileServiceSystemProperty> systemProperties) {
		this.mSystemProperties = systemProperties;
	}

	/**
	 * Returns the client used for table operations
	 */
	protected MobileServiceClient getClient() {
		return mClient;
	}

	/**
	 * Adds a new user-defined parameter to the query
	 * 
	 * @param parameter
	 *            The parameter name
	 * @param value
	 *            The parameter value
	 * @return MobileServiceQuery
	 */
	public MobileServiceQuery<E> parameter(String parameter, String value) {
		return this.where().parameter(parameter, value);
	}

	/**
	 * Creates a query with the specified order
	 * 
	 * @param field
	 *            Field name
	 * @param order
	 *            Sorting order
	 * @return MobileServiceQuery
	 */
	public MobileServiceQuery<E> orderBy(String field, QueryOrder order) {
		return this.where().orderBy(field, order);
	}

	/**
	 * Sets the number of records to return
	 * 
	 * @param top
	 *            Number of records to return
	 * @return MobileServiceQuery
	 */
	public MobileServiceQuery<E> top(int top) {
		return this.where().top(top);
	}

	/**
	 * Sets the number of records to skip over a given number of elements in a
	 * sequence and then return the remainder.
	 * 
	 * @param skip
	 * @return MobileServiceQuery
	 */
	public MobileServiceQuery<E> skip(int skip) {
		return this.where().skip(skip);
	}

	/**
	 * Specifies the fields to retrieve
	 * 
	 * @param fields
	 *            Names of the fields to retrieve
	 * @return MobileServiceQuery
	 */
	public MobileServiceQuery<E> select(String... fields) {
		return this.where().select(fields);
	}

	/**
	 * Include a property with the number of records returned.
	 * 
	 * @return MobileServiceQuery
	 */
	public MobileServiceQuery<E> includeInlineCount() {
		return this.where().includeInlineCount();
	}

	/**
	 * Starts a filter to query the table
	 * 
	 * @return The MobileServiceQuery<E> representing the filter
	 */
	public MobileServiceQuery<E> where() {
		MobileServiceQuery<E> query = new MobileServiceQuery<E>();
		query.setTable(this);
		return query;
	}

	/**
	 * Starts a filter to query the table with an existing filter
	 * 
	 * @param query
	 *            The existing filter
	 * @return The MobileServiceQuery<E> representing the filter
	 */
	public MobileServiceQuery<E> where(MobileServiceQuery<?> query) {
		if (query == null) {
			throw new IllegalArgumentException("Query must not be null");
		}

		MobileServiceQuery<E> baseQuery = new MobileServiceQuery<E>(query);
		baseQuery.setTable(this);
		return baseQuery;
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
		this.delete(element, null, callback);
	}
	
	/**
	 * Deletes an entity from a Mobile Service Table using a given id
	 * 
	 * @param id
	 *            The id of the entity to delete
	 * @param parameters
	 * 			  A list of user-defined parameters and values to include in the request URI query string            
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public void delete(Object elementOrId, List<Pair<String, String>> parameters, final TableDeleteCallback callback) {
		try {	
			validateId(elementOrId);
		} catch (Exception e) {
			if (callback != null) {
				callback.onCompleted(e, null);
			}
				
			return;
		}
				
		// Create delete request
		ServiceFilterRequest delete;	

		Uri.Builder uriBuilder = Uri.parse(mClient.getAppUrl().toString()).buildUpon();
		uriBuilder.path(TABLES_URL);
		uriBuilder.appendPath(mTableName);
		uriBuilder.appendPath(getObjectId(elementOrId).toString());
		
		parameters = addSystemProperties(mSystemProperties, parameters);

		if (parameters != null && parameters.size() > 0) {
			for (Pair<String, String> parameter : parameters) {
				uriBuilder.appendQueryParameter(parameter.first, parameter.second);
			}
		}
		
		delete = new ServiceFilterRequestImpl(new HttpDelete(uriBuilder.build().toString()), mClient.getAndroidHttpClientFactory());			

		// Create AsyncTask to execute the request
		new RequestAsyncTask(delete, mClient.createConnection()) {
			@Override
			protected void onPostExecute(ServiceFilterResponse result) {
				if (callback != null) {
					callback.onCompleted(mTaskException, result);
				}
			}
		}.executeTask();
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
	protected JsonObject patchOriginalEntityWithResponseEntity(
			JsonObject originalEntity, JsonObject newEntity) {
		// Patch the object to return with the new values
		JsonObject patchedEntityJson = (JsonObject) new JsonParser()
				.parse(originalEntity.toString());

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
	protected Object getObjectId(Object element) {
		if (element == null || (element instanceof JsonNull)) {
			throw new IllegalArgumentException("Element cannot be null");
		} else if (element instanceof Integer) {
			return ((Integer) element).intValue();
		} else if (element instanceof String) {
			return element;
		} else {
			JsonObject jsonObject;
			
			if (element instanceof JsonObject) {
				jsonObject = (JsonObject)element;
			} else {
				jsonObject = mClient.getGsonBuilder().create().toJsonTree(element).getAsJsonObject();
			}
			
			updateIdProperty(jsonObject);
	
			JsonElement idProperty = jsonObject.get("id");
			
			if (idProperty == null || (idProperty instanceof JsonNull)) {
				throw new IllegalArgumentException("Element must contain id property");
			}
	
			if (idProperty.isJsonPrimitive()) {
				if (idProperty.getAsJsonPrimitive().isNumber()) {
					return idProperty.getAsJsonPrimitive().getAsLong();
				} else if(idProperty.getAsJsonPrimitive().isString()) {
					return idProperty.getAsJsonPrimitive().getAsString();
				} else {
					throw new IllegalArgumentException("Invalid id type");
				}
			} else {
				throw new IllegalArgumentException("Invalid id type");
			}
		}
	}
	
	/**
	 * Updates the JsonObject to have an id property
	 * @param json
	 *            the element to evaluate
	 */
	protected void updateIdProperty(final JsonObject json) throws IllegalArgumentException {
		for (Map.Entry<String,JsonElement> entry : json.entrySet()){
			String key = entry.getKey();
			
			if (key.equalsIgnoreCase("id")) {
				JsonElement element = entry.getValue();
				
				if (isValidTypeId(element)) {
					if (!key.equals("id")) {
						//force the id name to 'id', no matter the casing 
						json.remove(key);
						// Create a new id property using the given property name
						
						JsonPrimitive value = entry.getValue().getAsJsonPrimitive();
						if (value.isNumber()) {
							json.addProperty("id", value.getAsLong());
						} else {
							json.addProperty("id", value.getAsString());
						}
					}
					
					return;
				} else {
					throw new IllegalArgumentException("The id must be numeric or string");
				}
			}
		}
	}
	
	/**
	 * Validates if the id property is numeric or string.
	 * @param element
	 * @return
	 */
	protected boolean isValidTypeId(JsonElement element) {
		return isStringType(element) || isNumericType(element);
	}

	/**
	 * Validates the id value from an Object on Lookup/Update/Delete action
	 * 
	 * @param elementOrId The Object to validate
	 */
	protected void validateId(final Object elementOrId) {
		if (elementOrId == null || (elementOrId instanceof JsonNull)) {
			throw new IllegalArgumentException("Element or id cannot be null.");
		} else if (isStringType(elementOrId)) {				
			String id = getStringValue(elementOrId);
			
			if (!isValidStringId(id) || isDefaultStringId(id)) {
				throw new IllegalArgumentException("The string id is invalid.");
			}
		} else if (isNumericType(elementOrId)) {				
			long id = getNumericValue(elementOrId);
			
			if (!isValidNumericId(id) || isDefaultNumericId(id)) {
				throw new IllegalArgumentException("The numeric id is invalid.");
			}
		} else if (elementOrId instanceof JsonObject) {
			validateId((JsonObject)elementOrId);
		} else {
			validateId(mClient.getGsonBuilder().create().toJsonTree(elementOrId).getAsJsonObject());
		}
	}
	
	/**
	 * Validates the id property from a JsonObject on Lookup/Update/Delete action
	 * 
	 * @param element The JsonObject to validate
	 */
	protected Object validateId(final JsonObject element) {
		if (element == null) {
			throw new IllegalArgumentException("The entity cannot be null.");			
		} else {
			updateIdProperty(element);
			
			if (element.has("id")) {
				JsonElement idElement = element.get("id");
				
				if(isStringType(idElement)) {
					String id = getStringValue(idElement);
					
					if (!isValidStringId(id) || isDefaultStringId(id)) {
						throw new IllegalArgumentException("The entity has an invalid string value on id property.");
					}
					
					return id;
				} else if (isNumericType(idElement)) {
					long id = getNumericValue(idElement);
					
					if (!isValidNumericId(id) || isDefaultNumericId(id)) {
						throw new IllegalArgumentException("The entity has an invalid numeric value on id property.");
					}
					
					return id;
				} else {
					throw new IllegalArgumentException("The entity must have a valid numeric or string id property.");
				}
			} else {
				throw new IllegalArgumentException("You must specify an id property with a valid numeric or string value.");
			}
		}
	}
	
	/**
	 * Validates if the object represents a string value.
	 * @param o
	 * @return
	 */
	protected boolean isStringType(Object o) {
		boolean result = (o instanceof String);
		
		if (o instanceof JsonElement) {
			JsonElement json = (JsonElement)o;
			
			if (json.isJsonPrimitive()) {
				JsonPrimitive primitive = json.getAsJsonPrimitive();
				result = primitive.isString();				
			}
		}
		
		return result;
	}
	
	/**
	 * Returns the string value represented by the object.
	 * @param o
	 * @return
	 */
	protected String getStringValue(Object o) {		
		String result;
		
		if (o instanceof String) {
			result = (String)o;		
		} else if (o instanceof JsonElement) {
			JsonElement json = (JsonElement)o;
			
			if (json.isJsonPrimitive()) {
				JsonPrimitive primitive = json.getAsJsonPrimitive();
				
				if (primitive.isString()) {
					result = primitive.getAsString();
				} else {
					throw new IllegalArgumentException("Object does not represent a string value.");
				}
			} else {
				throw new IllegalArgumentException("Object does not represent a string value.");
			}				
		} else {
			throw new IllegalArgumentException("Object does not represent a string value.");
		}
		
		return result;
	}

	/**
	 * Validates if the string id is valid.
	 * @param id
	 * @return
	 */
	protected boolean isValidStringId(String id) {
		boolean result = isDefaultStringId(id);
		
		if (!result && id != null) {
			result = id.length() <= 255;
			result &= !containsControlCharacter(id);
			result &= !containsSpecialCharacter(id);
			result &= !id.equals(".");
			result &= !id.equals("..");
		}
		
		return result;
	}

	/**
	 * Validates if the string id is a default value.
	 * @param id
	 * @return
	 */
	protected boolean isDefaultStringId(String id) {
		return (id == null) || (id.equals(""));
	}
	
	/**
	 * Validates if a given string contains a control character.
	 * @param s
	 * @return
	 */
	protected boolean containsControlCharacter(String s) {
		boolean result = false;
		
		final int length = s.length();
		
		for (int offset = 0; offset < length; ) {
		   final int codepoint = s.codePointAt(offset);

		   if (Character.isISOControl(codepoint)) {
			   result = true;
			   break;
		   }

		   offset += Character.charCount(codepoint);
		}
		
		return result;
	}
	
	/**
	 * Validates if a given string contains any of the following special characters: "(U+0022),  +(U+002B), /(U+002F), ?(U+003F), \(U+005C), `(U+0060)
	 * @param s
	 * @return
	 */
	protected boolean containsSpecialCharacter(String s) {
		boolean result = false;
		
		final int length = s.length();
		
		final int cpQuotationMark = 0x0022;
		final int cpPlusSign = 0x002B;
		final int cpSolidus = 0x002F;
		final int cpQuestionMark = 0x003F;
		final int cpReverseSolidus = 0x005C;
		final int cpGraveAccent = 0x0060;
		
		for (int offset = 0; offset < length; ) {
		   final int codepoint = s.codePointAt(offset);

		   if (codepoint == cpQuotationMark 
				   || codepoint == cpPlusSign
				   || codepoint == cpSolidus
				   || codepoint == cpQuestionMark
				   || codepoint == cpReverseSolidus
				   || codepoint == cpGraveAccent) {
			   result = true;
			   break;
		   }

		   offset += Character.charCount(codepoint);
		}
		
		return result;
	}
	
	/**
	 * Validates if the object represents a numeric value.
	 * @param o
	 * @return
	 */
	protected boolean isNumericType(Object o) {
		boolean result = (o instanceof Integer) || (o instanceof Long);
		
		if (o instanceof JsonElement) {
			JsonElement json = (JsonElement)o;
			
			if (json.isJsonPrimitive()) {
				JsonPrimitive primitive = json.getAsJsonPrimitive();
				result = primitive.isNumber();				
			}
		}
		
		return result;
	}
	
	/**
	 * Returns the numeric value represented by the object.
	 * @param o
	 * @return
	 */
	protected long getNumericValue(Object o) {		
		long result;
		
		if (o instanceof Integer) {
			result = (Integer)o;		
		} else if (o instanceof Long) {
			result = (Long)o;		
		} else if (o instanceof JsonElement) {
			JsonElement json = (JsonElement)o;
			
			if (json.isJsonPrimitive()) {
				JsonPrimitive primitive = json.getAsJsonPrimitive();
				
				if (primitive.isNumber()) {
					result = primitive.getAsLong();
				} else {
					throw new IllegalArgumentException("Object does not represent a string value.");
				}
			} else {
				throw new IllegalArgumentException("Object does not represent a string value.");
			}				
		} else {
			throw new IllegalArgumentException("Object does not represent a string value.");
		}
		
		return result;
	}
	
	/**
	 * Validates if the numeric id is valid.
	 * @param id
	 * @return
	 */
	protected boolean isValidNumericId(long id) {
		return isDefaultNumericId(id) || id > 0;
	}

	/**
	 * Validates if the numeric id is a default value.
	 * @param id
	 * @return
	 */
	protected boolean isDefaultNumericId(long id) {
		return (id == 0);
	}

	/**
	 * Adds the tables requested system properties to the parameters collection.
	 * @param	systemProperties	The system properties to add.
	 * @param	parameters			The parameters collection.
	 * @return						The parameters collection with any requested system properties included.
	 */ 
	protected List<Pair<String, String>> addSystemProperties(EnumSet<MobileServiceSystemProperty> systemProperties, List<Pair<String, String>> parameters) {
		boolean containsSystemProperties = false;
		
		List<Pair<String,String>> result = new  ArrayList<Pair<String,String>>(parameters != null ? parameters.size() : 0);
		
		// Make sure we have a case-insensitive parameters list
        if (parameters != null) {
        	for (Pair<String,String> parameter : parameters) {
        		result.add(parameter);
        		containsSystemProperties = containsSystemProperties || parameter.first.equalsIgnoreCase(SystemPropertiesQueryParameterName);
        	}
        }

        // If there is already a user parameter for the system properties, just use it
        if (!containsSystemProperties) {
            String systemPropertiesString = getSystemPropertiesString(systemProperties);
            
            if (systemPropertiesString != null) {
                result.add(new Pair<String,String>(SystemPropertiesQueryParameterName,systemPropertiesString));
            }
        }

        return result;
    }
	
    /**
	 * Removes all system properties (name start with '__') from the instance if the instance is determined to have a string id and 
	 * therefore be for table that supports system properties.
	 * @param	instance	The instance to remove the system properties from.
	 * @param	version		Set to the value of the version system property before it is removed.
	 * @return				The instance with the system properties removed.
	 */
    protected static JsonObject removeSystemProperties(JsonObject instance)
    {
        boolean haveCloned = false;
        
        for (Entry<String,JsonElement> property : instance.entrySet()) {
        	if (SystemPropertyNameToEnum.containsKey(property.getKey())) {
        		// We don't want to alter the original JsonObject passed in by the caller
                // so if we find a system property to remove, we have to clone first
                if (!haveCloned) {
                    instance = (JsonObject) new JsonParser().parse(instance.toString());
                    haveCloned = true;
                }

                instance.remove(property.getKey());
        	}
        }

        return instance;
    }
    
    /**
	 * Gets the version system property.
	 * @param	instance	The instance to remove the system properties from.
	 * @return				The value of the version system property or null if none present.
	 */
    protected static String getVersionSystemProperty(JsonObject instance)
    {
        String version = null;
        
        for (Entry<String,JsonElement> property : instance.entrySet()) {
            if (property.getKey().equalsIgnoreCase(VersionSystemPropertyName) && !property.getValue().isJsonNull()) {
                version = property.getValue().getAsString();
            }
        }

        return version;
    }
    
    /**
	 * Gets a valid etag from a string value. Etags are surrounded by double quotes and any internal quotes 
	 * must be escaped with a '\'.
	 * @param	value	The value to create the etag from.
	 * @return			The etag.
	 */
    protected static String getEtagFromValue(String value) {
        // If the value has double quotes, they will need to be escaped.
        for (int i = 0; i < value.length(); i++) {
            if (value.charAt(i) == '"') {
            	if (i == 0) {
            		value = String.format("%s%s", "\\", value);
            	} else if (value.charAt(i - 1) != '\\') {
            		value = String.format("%s%s%s", value.substring(0, i), "\\", value.substring(i));
            	}
            }
        }

        // All etags are quoted;
        return String.format("\"%s\"", value);
    }

    /**
	 * Gets a value from an etag. Etags are surrounded by double quotes and any internal quotes 
	 * must be escaped with a '\'.
	 * @param	etag	The etag to get the value from.
	 * @return			The value.
	 */
    protected static String getValueFromEtag(String etag) {
        int length = etag.length();
        
        if (length > 1 && etag.charAt(0) == '\"' && etag.charAt(length - 1) == '\"') {
            etag = etag.substring(1, length - 1);
        }

        return etag.replace("\\\"", "\"");
    }
	
	/**
	 * Gets the system properties header value from the MobileServiceSystemProperties.
	 * @param	properties	The system properties to set in the system properties header.
	 * @return				The system properties header value. Returns null if properties is null or empty.
	 */
    private static String getSystemPropertiesString(EnumSet<MobileServiceSystemProperty> properties) {
        if (properties == null || properties.isEmpty()) {
            return null;
        }
        
        if (properties.containsAll(EnumSet.allOf(MobileServiceSystemProperty.class))) {
            return "*";
        }
        
        StringBuilder sb = new StringBuilder();
        
        int i = 0;

        for (MobileServiceSystemProperty systemProperty : properties) {
            sb.append(getSystemPropertyString(systemProperty));
            
            i++;
            
            if (i < properties.size()) {
            	sb.append(",");
            }
        }

        return sb.toString();
    }

	/**
	 * Gets the system property header value from the MobileServiceSystemProperty.
	 * @param	systemProperty	The system property to set in the system properties header.
	 * @return					The system property header value.
	 */
    private static String getSystemPropertyString(MobileServiceSystemProperty systemProperty) {
    	String property = systemProperty.toString().trim();
    	char firstLetterAsLower = property.toLowerCase(Locale.getDefault()).charAt(0);
		return SystemPropertyPrefix + firstLetterAsLower + property.substring(1);
    }
    
    /**
	 * Returns the system properties defined or annotated in the entity class
	 * @param	clazz	Target entity class
	 * @return 			List of entities
	 */
	protected static <F> EnumSet<MobileServiceSystemProperty> getSystemProperties(Class<F> clazz) {
		EnumSet<MobileServiceSystemProperty> result = EnumSet.noneOf(MobileServiceSystemProperty.class);

		Class<?> idClazz = getIdPropertyClass(clazz);
		
		if (idClazz != null && !isIntegerClass(idClazz)) {
			// Search for system properties annotations, regardless case
			for (Field field : clazz.getDeclaredFields()) {
				SerializedName serializedName = field.getAnnotation(SerializedName.class);
				
				if(serializedName != null) {
					if (SystemPropertyNameToEnum.containsKey(serializedName.value())) {
						result.add(SystemPropertyNameToEnum.get(serializedName.value()));
					}
				} else {
					if (SystemPropertyNameToEnum.containsKey(field.getName())) {
						result.add(SystemPropertyNameToEnum.get(field.getName()));
					}
				}
			}
		}

		// Otherwise, return empty
		return result;
	}
	
    /**
	 * Returns the id property class defined or annotated in the entity class
	 * @param	clazz	Target entity class
	 * @return 			Property class
	 */
	protected static <F> Class<?> getIdPropertyClass(Class<F> clazz) {
		// Search for id properties annotations, regardless case
		for (Field field : clazz.getDeclaredFields()) {
			SerializedName serializedName = field.getAnnotation(SerializedName.class);
			
			if(serializedName != null) {
				if (IdProperties.contains(serializedName.value())) {
					return field.getType();
				}
			} else {
				if (IdProperties.contains(field.getName())) {
					return field.getType();
				}
			}
		}
		
		return null;
	}
	
    /**
	 * Returns the id property class defined or annotated in the entity class
	 * @param	clazz	Target entity class
	 * @return 			Property class
	 */
	protected static <F> boolean isIntegerClass(Class<F> clazz) {		
		return clazz.equals(Integer.class) || clazz.equals(Long.class) || clazz.equals(int.class) || clazz.equals(long.class);
	}
	

}
