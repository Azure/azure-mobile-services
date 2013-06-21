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