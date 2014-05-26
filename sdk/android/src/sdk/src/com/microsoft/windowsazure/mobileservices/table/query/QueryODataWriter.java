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
package com.microsoft.windowsazure.mobileservices.table.query;

import java.io.UnsupportedEncodingException;
import java.net.URLEncoder;
import java.util.List;

import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceTableBase;

import android.util.Pair;

public class QueryODataWriter {

	/**
	 * Returns the OData string representation of the query
	 * 
	 * @throws MobileServiceException
	 */
	public static String getRowFilter(Query query) throws MobileServiceException {
		QueryNodeODataWriter oDataWriter = new QueryNodeODataWriter();

		if (query.getQueryNode() != null) {
			query.getQueryNode().accept(oDataWriter);
		}

		return oDataWriter.getBuilder().toString();
	}

	/**
	 * Returns the OData string representation of the rowset's modifiers
	 * 
	 * @throws UnsupportedEncodingException
	 */
	public static String getRowSetModifiers(Query query, MobileServiceTableBase<?> table) throws UnsupportedEncodingException {
		StringBuilder sb = new StringBuilder();

		if (query.hasInlineCount()) {
			sb.append("&$inlinecount=allpages");
		}

		if (query.getTop() > 0) {
			sb.append("&$top=");
			sb.append(query.getTop());
		}

		if (query.getSkip() > 0) {
			sb.append("&$skip=");
			sb.append(query.getSkip());
		}

		if (query.getOrderBy().size() > 0) {
			sb.append("&$orderby=");

			boolean first = true;
			for (Pair<String, QueryOrder> order : query.getOrderBy()) {
				if (first) {
					first = false;
				} else {
					sb.append(URLEncoder.encode(",", MobileServiceClient.UTF8_ENCODING));
				}

				sb.append(URLEncoder.encode(order.first, MobileServiceClient.UTF8_ENCODING));
				sb.append(URLEncoder.encode(" ", MobileServiceClient.UTF8_ENCODING));
				sb.append(order.second == QueryOrder.Ascending ? "asc" : "desc");

			}
		}
		List<Pair<String, String>> parameters = table.addSystemProperties(table.getSystemProperties(), query.getUserDefinedParameters());
		for (Pair<String, String> parameter : parameters) {
			if (parameter.first != null) {
				sb.append("&");

				String key = parameter.first;
				String value = parameter.second;
				if (value == null)
					value = "null";

				sb.append(URLEncoder.encode(key, MobileServiceClient.UTF8_ENCODING));
				sb.append("=");
				sb.append(URLEncoder.encode(value, MobileServiceClient.UTF8_ENCODING));
			}
		}

		if (query.getProjection() != null && query.getProjection().size() > 0) {
			sb.append("&$select=");

			boolean first = true;
			for (String field : query.getProjection()) {
				if (first) {
					first = false;
				} else {
					sb.append(URLEncoder.encode(",", MobileServiceClient.UTF8_ENCODING));
				}

				sb.append(URLEncoder.encode(field, MobileServiceClient.UTF8_ENCODING));
			}
		}

		return sb.toString();
	}
}
