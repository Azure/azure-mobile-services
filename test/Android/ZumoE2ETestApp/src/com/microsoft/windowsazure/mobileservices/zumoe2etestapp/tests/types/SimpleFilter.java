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

public abstract class SimpleFilter<E> implements ListFilter<E> {

	@Override
	public FilterResult<E> filter(List<E> list) {
		return getElements(list);
	}

	abstract protected boolean criteria(E element);

	protected FilterResult<E> getElements(List<E> list) {
		List<E> newList = new ArrayList<E>();
		FilterResult<E> result = new FilterResult<E>();

		for (E element : list) {
			if (criteria(element)) {
				newList.add(element);
			}
		}
		result.totalCount = newList.size();

		result.elements = applyOrder(newList);
		return result;
	}

	protected List<E> applyOrder(List<E> list) {
		return list;
	}

	protected FilterResult<E> applyTopSkip(FilterResult<E> result, int top, int skip) {
		if (result.elements.size() <= skip) {
			result.elements = new ArrayList<E>();
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