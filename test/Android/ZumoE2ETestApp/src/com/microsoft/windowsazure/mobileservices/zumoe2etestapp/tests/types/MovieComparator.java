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
package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types;

import java.util.ArrayList;
import java.util.Comparator;
import java.util.List;

import android.util.Pair;

import com.microsoft.windowsazure.mobileservices.QueryOrder;


public class MovieComparator implements Comparator<Movie> {
	protected Pair<String, QueryOrder>[] mFields;

	// default ascending
	@SuppressWarnings("unchecked")
	public MovieComparator(String... fields) {
		List<Pair<String, QueryOrder>> newFields = new ArrayList<Pair<String, QueryOrder>>();

		for (String field : fields) {
			newFields.add(new Pair<String, QueryOrder>(field, QueryOrder.Ascending));
		}

		mFields = new Pair[0];
		mFields = newFields.toArray(mFields);
	}

	public MovieComparator(Pair<String, QueryOrder>... fields) {
		mFields = fields;
	}

	@SuppressWarnings("unchecked")
	@Override
	public int compare(Movie m1, Movie m2) {
		try {
			for (Pair<String, QueryOrder> field : mFields) {

				Object fieldM1 = Movie.class.getMethod(field.first, (Class<?>[]) null).invoke(m1, (Object[]) null);
				;
				Object fieldM2 = Movie.class.getMethod(field.first, (Class<?>[]) null).invoke(m2, (Object[]) null);
				;

				if (fieldM1 instanceof Comparable) {
					int res = ((Comparable<Object>) fieldM1).compareTo((Comparable<Object>) fieldM2);
					if (res != 0) {
						if (field.second == QueryOrder.Ascending)
							return res;
						else
							return res * -1;
					}
				}
			}

			return 0;
		} catch (Exception e) {
			return 0;
		}
	}

}