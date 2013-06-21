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
import java.util.List;

public abstract class SimpleMovieFilter implements ListFilter<Movie> {

	@Override
	public FilterResult<Movie> filter(List<Movie> list) {
		return getElements(list);
	}

	abstract protected boolean criteria(Movie movie);

	protected FilterResult<Movie> getElements(List<Movie> list) {
		List<Movie> newList = new ArrayList<Movie>();
		FilterResult<Movie> result = new FilterResult<Movie>();

		for (Movie movie : list) {
			if (criteria(movie)) {
				newList.add(movie);
			}
		}
		result.totalCount = newList.size();

		result.elements = applyOrder(newList);
		return result;
	}

	protected List<Movie> applyOrder(List<Movie> movies) {
		return movies;
	}

	protected FilterResult<Movie> applyTopSkip(FilterResult<Movie> result, int top, int skip) {
		if (result.elements.size() <= skip) {
			result.elements = new ArrayList<Movie>();
			return result;
		} else {
			result.elements = result.elements.subList(skip, result.elements.size());
		}

		if (result.elements.size() > top) {
			result.elements = result.elements.subList(0, top);
		}

		return result;
	}
}