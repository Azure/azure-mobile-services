package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types;

import java.util.List;

public interface ListFilter<E> {
	public FilterResult<E> filter(List<E> list);
}
