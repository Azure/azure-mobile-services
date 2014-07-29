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
 * QuerySQLWriter.java
 */
package com.microsoft.windowsazure.mobileservices.table.query;

import java.util.Locale;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;

import android.util.Pair;

public class QuerySQLWriter {

	/**
	 * Returns the SQL string representation of the query's select clause
	 */
	public static String getSelectClause(Query query) {
		String result = "*";

		if (query != null && query.getProjection() != null && query.getProjection().size() > 0) {
			StringBuilder sb = new StringBuilder();

			int index = 0;
			for (String projection : query.getProjection()) {
				sb.append("\"");
				sb.append(projection.trim().toLowerCase(Locale.getDefault()));
				sb.append("\" ");

				if (index < query.getProjection().size() - 1) {
					sb.append(", ");
				}

				index++;
			}

			result = sb.toString();
		}

		return result;
	}

	/**
	 * Returns the SQL string representation of the query's where clause
	 */
	public static String getWhereClause(Query query) throws MobileServiceException {
		QueryNodeSQLWriter sqlWriter = new QueryNodeSQLWriter();

		if (query != null && query.getQueryNode() != null) {
			query.getQueryNode().accept(sqlWriter);
		}

		return sqlWriter.getBuilder().toString();
	}

	/**
	 * Returns the SQL string representation of the query's order by clause
	 */
	public static String getOrderByClause(Query query) {
		String result = null;

		if (query != null && query.getOrderBy() != null && query.getOrderBy().size() > 0) {
			StringBuilder sb = new StringBuilder();

			int index = 0;
			for (Pair<String, QueryOrder> order : query.getOrderBy()) {
				sb.append("\"");
				sb.append(order.first.trim().toLowerCase(Locale.getDefault()));
				sb.append("\" ");

				String direction = order.second == QueryOrder.Ascending ? "ASC" : "DESC";
				sb.append(direction);

				if (index < query.getOrderBy().size() - 1) {
					sb.append(", ");
				}

				index++;
			}

			result = sb.toString();
		}

		return result;
	}

	/**
	 * Returns the SQL string representation of the query's limit clause
	 */
	public static String getLimitClause(Query query) {
		String result = null;

		int limit = query != null ? query.getTop() : 0;
		int offset = query != null ? query.getSkip() : 0;

		if (limit > 0 || offset > 0) {
			result = String.valueOf(offset) + "," + String.valueOf(limit);
		}

		return result;
	}
}