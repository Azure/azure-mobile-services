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
package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests;

import static com.microsoft.windowsazure.mobileservices.MobileServiceQueryOperations.day;
import static com.microsoft.windowsazure.mobileservices.MobileServiceQueryOperations.endsWith;
import static com.microsoft.windowsazure.mobileservices.MobileServiceQueryOperations.field;
import static com.microsoft.windowsazure.mobileservices.MobileServiceQueryOperations.floor;
import static com.microsoft.windowsazure.mobileservices.MobileServiceQueryOperations.month;
import static com.microsoft.windowsazure.mobileservices.MobileServiceQueryOperations.query;
import static com.microsoft.windowsazure.mobileservices.MobileServiceQueryOperations.startsWith;
import static com.microsoft.windowsazure.mobileservices.MobileServiceQueryOperations.subStringOf;
import static com.microsoft.windowsazure.mobileservices.MobileServiceQueryOperations.toLower;
import static com.microsoft.windowsazure.mobileservices.MobileServiceQueryOperations.toUpper;
import static com.microsoft.windowsazure.mobileservices.MobileServiceQueryOperations.val;
import static com.microsoft.windowsazure.mobileservices.MobileServiceQueryOperations.year;

import java.util.ArrayList;
import java.util.Calendar;
import java.util.Collections;
import java.util.List;
import java.util.Locale;

import android.util.Pair;

import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.MobileServiceQuery;
import com.microsoft.windowsazure.mobileservices.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.QueryOrder;
import com.microsoft.windowsazure.mobileservices.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponseCallback;
import com.microsoft.windowsazure.mobileservices.TableJsonOperationCallback;
import com.microsoft.windowsazure.mobileservices.TableOperationCallback;
import com.microsoft.windowsazure.mobileservices.TableQueryCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.ExpectedValueException;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestCase;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestExecutionCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestGroup;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestResult;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestStatus;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.Util;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.AllMovies;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.FilterResult;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.ListFilter;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.Movie;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.MovieComparator;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.SimpleMovieFilter;

public class QueryTests extends TestGroup {

	protected static final String MOVIES_TABLE_NAME = "droidMovies";

	@SuppressWarnings("unchecked")
	public QueryTests() {
		super("Query tests");

		addPopulateTest();

		// numeric functions
		this.addTest(createQueryTest("GreaterThan and LessThan - Movies from the 90s", field("year").gt(1989).and().field("year").lt(2000),
				new SimpleMovieFilter() {

					@Override
					protected boolean criteria(Movie movie) {
						return movie.getYear() > 1989 && movie.getYear() < 2000;
					}
				}));

		this.addTest(createQueryTest("GreaterEqual and LessEqual - Movies from the 90s", field("year").ge(1990).and().field("year").le(1999),
				new SimpleMovieFilter() {

					@Override
					protected boolean criteria(Movie movie) {
						return movie.getYear() >= 1990 && movie.getYear() <= 1999;
					}
				}));

		this.addTest(createQueryTest("Compound statement - OR of ANDs - Movies from the 30s and 50s",
				field("year").ge(1930).and().field("year").lt(1940).or(field("year").ge(1950).and().field("year").lt(1960)), new SimpleMovieFilter() {

					@Override
					protected boolean criteria(Movie movie) {
						return (movie.getYear() >= 1930 && movie.getYear() < 1940) || (movie.getYear() >= 1950 && movie.getYear() < 1960);
					}
				}));

		this.addTest(createQueryTest("Division, equal and different - Movies from the year 2000 with rating other than R", query(field("year").div(1000)).eq(2)
				.and().field("mpaarating").ne("R"), new SimpleMovieFilter() {

			@Override
			protected boolean criteria(Movie movie) {
				return (movie.getYear() / 1000d == 2) && movie.getMPAARating() != "R";
			}
		}));

		this.addTest(createQueryTest("Addition, subtraction, relational, AND - Movies from the 1980s which last less than 2 hours",
				field("year").sub(1900).ge(80).and().field("year").add(10).lt(2000).and().field("duration").lt(120), new SimpleMovieFilter() {

					@Override
					protected boolean criteria(Movie movie) {
						return (movie.getYear() - 1900 >= 80) && (movie.getYear() + 10 < 2000) && (movie.getDuration() < 120);
					}
				}));

		// string functions

		this.addTest(createQueryTest("StartsWith - Movies which starts with 'The'", startsWith("title", "The"), new SimpleMovieFilter() {

			@Override
			protected boolean criteria(Movie movie) {
				return movie.getTitle().startsWith("The");
			}

			@Override
			public FilterResult<Movie> filter(List<Movie> list) {
				return applyTopSkip(super.filter(list), 100, 0);
			}
		}, 100));

		this.addTest(createQueryTest("StartsWith, case insensitive - Movies which start with 'the'", startsWith(toLower(field("title")), val("the")),
				new SimpleMovieFilter() {

					@Override
					protected boolean criteria(Movie movie) {
						return movie.getTitle().toLowerCase(Locale.getDefault()).startsWith("the");
					}

					@Override
					public FilterResult<Movie> filter(List<Movie> list) {
						return applyTopSkip(super.filter(list), 100, 0);
					}
				}, 100));

		this.addTest(createQueryTest("EndsWith, case insensitive - Movies which end with 'r'", endsWith(toLower(field("title")), val("r")),
				new SimpleMovieFilter() {

					@Override
					protected boolean criteria(Movie movie) {
						return movie.getTitle().toLowerCase(Locale.getDefault()).endsWith("r");
					}
				}));

		this.addTest(createQueryTest("Contains - Movies which contain the word 'one', case insensitive", subStringOf(val("ONE"), toUpper(field("title"))),
				new SimpleMovieFilter() {

					@Override
					protected boolean criteria(Movie movie) {
						return movie.getTitle().toUpperCase(Locale.getDefault()).contains("ONE");
					}
				}));

		this.addTest(createQueryTest("String equals - Movies since 1980 with rating PG-13", field("year").ge(1980).and().field("mpaarating").eq("PG-13"),
				new SimpleMovieFilter() {

					@Override
					protected boolean criteria(Movie movie) {
						return movie.getYear() >= 1980 && movie.getMPAARating() == "PG-13";
					}

					@Override
					public FilterResult<Movie> filter(List<Movie> list) {
						return applyTopSkip(super.filter(list), 100, 0);
					}
				}, 100));

		this.addTest(createQueryTest("String field, comparison to null - Movies since 1980 without a MPAA rating",
				field("year").ge(1980).and().field("mpaarating").eq((String) null), new SimpleMovieFilter() {

					@Override
					protected boolean criteria(Movie movie) {
						return movie.getYear() >= 1980 && movie.getMPAARating() == null;
					}

					@Override
					public FilterResult<Movie> filter(List<Movie> list) {
						return applyTopSkip(super.filter(list), 100, 0);
					}
				}, 100));

		this.addTest(createQueryTest("String field, comparison (not equal) to null - Movies before 1970 with a MPAA rating", field("year").lt(1970).and()
				.field("mpaarating").ne((String) null), new SimpleMovieFilter() {

			@Override
			protected boolean criteria(Movie movie) {
				return movie.getYear() < 1970 && movie.getMPAARating() != null;
			}

			@Override
			public FilterResult<Movie> filter(List<Movie> list) {
				return applyTopSkip(super.filter(list), 100, 0);
			}
		}, 100));

		// Numeric functions
		this.addTest(createQueryTest("Floor - Movies which last more than 3 hours", floor(field("duration").div(60)).ge(3), new SimpleMovieFilter() {

			@Override
			protected boolean criteria(Movie movie) {
				return Math.floor(movie.getDuration() / 60d) >= 3;
			}
		}));

		this.addTest(createQueryTest("Ceiling - Best picture winners which last at most 2 hours",
				field("bestPictureWinner").eq(true).and().ceiling(field("duration").div(60)).eq(2), new SimpleMovieFilter() {

					@Override
					protected boolean criteria(Movie movie) {
						return movie.isBestPictureWinner() && Math.ceil(movie.getDuration() / 60d) == 2;
					}
				}));

		this.addTest(createQueryTest("Round - Best picture winners which last more than 2.5 hours",
				field("bestPictureWinner").eq(true).and().round(field("duration").div(60)).gt(2), new SimpleMovieFilter() {

					@Override
					protected boolean criteria(Movie movie) {
						return movie.isBestPictureWinner() && Math.round(movie.getDuration() / 60d) > 2;
					}
				}));

		this.addTest(createQueryTest("Date: Greater than, less than - Movies with release date in the 70s",
				field("releaseDate").gt(Util.getUTCDate(1969, 12, 31)).and().field("releaseDate").lt(Util.getUTCDate(1971, 1, 1)), new SimpleMovieFilter() {

					@Override
					protected boolean criteria(Movie movie) {
						return movie.getReleaseDate().compareTo(Util.getUTCDate(1969, 12, 31)) > 0
								&& movie.getReleaseDate().compareTo(Util.getUTCDate(1971, 1, 1)) < 0;
					}
				}));

		this.addTest(createQueryTest("Date: Greater than, less than - Movies with release date in the 80s", field("releaseDate")
				.ge(Util.getUTCDate(1980, 1, 1)).and().field("releaseDate").le(Util.getUTCDate(1989, 1, 1, 23, 59, 59)), new SimpleMovieFilter() {

			@Override
			protected boolean criteria(Movie movie) {
				return movie.getReleaseDate().compareTo(Util.getUTCDate(1980, 1, 1)) >= 0
						&& movie.getReleaseDate().compareTo(Util.getUTCDate(1989, 1, 1, 23, 59, 59)) <= 0;
			}
		}));

		this.addTest(createQueryTest("Date: Date: Equal - Movies released on 1994-10-14 (Shawshank Redemption / Pulp Fiction)",
				field("releaseDate").eq(Util.getUTCDate(1994, 10, 14)), new SimpleMovieFilter() {

					@Override
					protected boolean criteria(Movie movie) {
						return movie.getReleaseDate().compareTo(Util.getUTCDate(1994, 10, 14)) == 0;
					}
				}));

		// Date functions
		this.addTest(createQueryTest("Date (month): Movies released in November", month("releaseDate").eq(11), new SimpleMovieFilter() {

			@Override
			protected boolean criteria(Movie movie) {
				return Util.getUTCCalendar(movie.getReleaseDate()).get(Calendar.MONTH) == Calendar.NOVEMBER;
			}
		}));

		this.addTest(createQueryTest("Date (day): Movies released in the first day of the month", day("releaseDate").eq(1), new SimpleMovieFilter() {

			@Override
			protected boolean criteria(Movie movie) {
				return Util.getUTCCalendar(movie.getReleaseDate()).get(Calendar.DAY_OF_MONTH) == 1;
			}
		}));

		this.addTest(createQueryTest("Date (year): Movies whose year is different than its release year", year("releaseDate").ne().field("year"),
				new SimpleMovieFilter() {

					@Override
					protected boolean criteria(Movie movie) {
						return Util.getUTCCalendar(movie.getReleaseDate()).get(Calendar.YEAR) != movie.getYear();
					}

					@Override
					public FilterResult<Movie> filter(List<Movie> list) {
						return applyTopSkip(super.filter(list), 100, 0);
					}
				}, 100));

		// boolean fields
		this.addTest(createQueryTest("Bool: equal to true - Best picture winners before 1950",
				field("year").lt(1950).and().field("bestPictureWinner").eq(true), new SimpleMovieFilter() {

					@Override
					protected boolean criteria(Movie movie) {
						return movie.getYear() < 1950 && movie.isBestPictureWinner();
					}
				}));

		this.addTest(createQueryTest("Bool: equal to false - Best picture winners after 2000",
				field("year").ge(2000).and().not(field("bestPictureWinner").eq(false)), new SimpleMovieFilter() {

					@Override
					protected boolean criteria(Movie movie) {
						return movie.getYear() >= 2000 && !(movie.isBestPictureWinner() == false);
					}
				}));

		this.addTest(createQueryTest("Bool: not equal to false - Best picture winners after 2000",
				field("bestPictureWinner").ne(false).and().field("year").ge(2000), new SimpleMovieFilter() {

					@Override
					protected boolean criteria(Movie movie) {
						return movie.isBestPictureWinner() != false && movie.getYear() >= 2000;
					}
				}));

		// top and skip

		this.addTest(createQueryTest("Get all using large $top - 500", null, new SimpleMovieFilter() {

			@Override
			protected boolean criteria(Movie movie) {
				return true;
			}

			@Override
			public FilterResult<Movie> filter(List<Movie> list) {
				return applyTopSkip(super.filter(list), 500, 0);
			}
		}, 500));

		this.addTest(createQueryTest("Skip all using large skip - 500", null, new SimpleMovieFilter() {

			@Override
			protected boolean criteria(Movie movie) {
				return true;
			}

			@Override
			public FilterResult<Movie> filter(List<Movie> list) {
				return applyTopSkip(super.filter(list), Integer.MAX_VALUE, 500);
			}
		}, null, 500, null, null, false, null));

		this.addTest(createQueryTest("Get first ($top) - 10", null, new SimpleMovieFilter() {

			@Override
			protected boolean criteria(Movie movie) {
				return true;
			}

			@Override
			public FilterResult<Movie> filter(List<Movie> list) {
				return applyTopSkip(super.filter(list), 10, 0);
			}
		}, 10));

		final int movieCountMinus10 = QueryTestData.getAllMovies().size() - 10;
		this.addTest(createQueryTest("Get last ($skip) - 10", null, new SimpleMovieFilter() {

			@Override
			protected boolean criteria(Movie movie) {
				return true;
			}

			@Override
			public FilterResult<Movie> filter(List<Movie> list) {
				return applyTopSkip(super.filter(list), Integer.MAX_VALUE, movieCountMinus10);
			}
		}, null, movieCountMinus10, null, null, false, null));

		this.addTest(createQueryTest("Skip, take, includeTotalCount - movies 21-30, ordered by title", null, new SimpleMovieFilter() {

			@Override
			protected boolean criteria(Movie movie) {
				return true;
			}

			@Override
			public FilterResult<Movie> filter(List<Movie> list) {
				return applyTopSkip(super.filter(list), 10, 20);
			}

			@Override
			protected List<Movie> applyOrder(List<Movie> movies) {
				Collections.sort(movies, new MovieComparator("getTitle"));
				return movies;
			}

		}, 10, 20, createOrder(new Pair<String, QueryOrder>("title", QueryOrder.Ascending)), null, false, null));

		this.addTest(createQueryTest("Skip, take, filter includeTotalCount - movies 11-20 which won a best picture award, ordered by year",
				field("bestPictureWinner").eq(true), new SimpleMovieFilter() {

					@Override
					protected boolean criteria(Movie movie) {
						return movie.isBestPictureWinner();
					}

					@Override
					public FilterResult<Movie> filter(List<Movie> list) {
						return applyTopSkip(super.filter(list), 10, 10);
					}

					@Override
					protected List<Movie> applyOrder(List<Movie> movies) {
						Collections.sort(movies, new MovieComparator(new Pair<String, QueryOrder>("getYear", QueryOrder.Descending)));
						return movies;
					}

				}, 10, 10, createOrder(new Pair<String, QueryOrder>("year", QueryOrder.Descending)), null, true, null));

		this.addTest(createQueryTest("Select one field - Only title of movies from 2008", field("year").eq(2008), new SimpleMovieFilter() {

			@Override
			protected FilterResult<Movie> getElements(List<Movie> list) {
				FilterResult<Movie> res = super.getElements(list);
				FilterResult<Movie> newRes = new FilterResult<Movie>();
				newRes.totalCount = res.totalCount;
				newRes.elements = new ArrayList<Movie>();

				for (Movie movie : res.elements) {
					// only add title field
					Movie newMovie = new Movie();
					newMovie.setTitle(movie.getTitle());
					newRes.elements.add(newMovie);
				}

				return newRes;
			}

			@Override
			protected boolean criteria(Movie movie) {
				return movie.getYear() == 2008;
			}

		}, null, null, null, project("title"), true, null));

		this.addTest(createQueryTest("Select multiple fields - List of movies from the 2000's", field("year").ge(2000), new SimpleMovieFilter() {

			@Override
			protected FilterResult<Movie> getElements(List<Movie> list) {
				FilterResult<Movie> res = super.getElements(list);
				FilterResult<Movie> newRes = new FilterResult<Movie>();
				newRes.totalCount = res.totalCount;
				newRes.elements = new ArrayList<Movie>();

				for (Movie movie : res.elements) {
					// only add title, bestPictureWinner, releaseDate
					// and duration fields
					Movie newMovie = new Movie();
					newMovie.setTitle(movie.getTitle());
					newMovie.setBestPictureWinner(movie.isBestPictureWinner());
					newMovie.setReleaseDate(movie.getReleaseDate());
					newMovie.setDuration(movie.getDuration());
					newRes.elements.add(newMovie);
				}

				return newRes;
			}

			@Override
			public FilterResult<Movie> filter(List<Movie> list) {
				return applyTopSkip(super.filter(list), 5, 0);
			}

			@Override
			protected boolean criteria(Movie movie) {
				return movie.getYear() >= 2000;
			}

			@Override
			protected List<Movie> applyOrder(List<Movie> movies) {
				Collections.sort(movies, new MovieComparator(new Pair<String, QueryOrder>("getReleaseDate", QueryOrder.Descending),
						new Pair<String, QueryOrder>("getTitle", QueryOrder.Ascending)));
				return movies;
			}

		}, 5, null,
				createOrder(new Pair<String, QueryOrder>("releaseDate", QueryOrder.Descending), new Pair<String, QueryOrder>("title", QueryOrder.Ascending)),
				project("title", "bestPictureWinner", "duration", "releaseDate"), true, null));

		this.addTest(createQueryTest("(Neg) Very large top value", field("year").gt(2000), null, 1001, null, null, null, false, MobileServiceException.class));

		// invalid lookup
		for (int i = -1; i <= 0; i++) {
			final int id = i;
			TestCase test = new TestCase() {

				@Override
				protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

					final TestCase testCase = this;

					log("lookup id " + id);
					client.getTable(MOVIES_TABLE_NAME).lookUp(id, new TableJsonOperationCallback() {

						@Override
						public void onCompleted(JsonObject jsonEntity, Exception exception, ServiceFilterResponse response) {
							TestResult result = new TestResult();
							result.setTestCase(testCase);
							result.setStatus(TestStatus.Passed);

							if (exception != null) {
								createResultFromException(result, exception);
							} else {
								result.setStatus(TestStatus.Failed);
							}

							if (callback != null)
								callback.onTestComplete(testCase, result);
						}
					});
				}
			};

			test.setExpectedExceptionClass(MobileServiceException.class);
			test.setName("(Neg) Invalid Lookup - ID: " + id);
			this.addTest(test);
		}

	}

	private static String[] project(String... fields) {
		return fields;
	}

	private List<Pair<String, QueryOrder>> createOrder(Pair<String, QueryOrder>... entries) {
		List<Pair<String, QueryOrder>> ret = new ArrayList<Pair<String, QueryOrder>>();

		for (Pair<String, QueryOrder> Pair : entries) {
			ret.add(Pair);
		}

		return ret;
	}

	private void addPopulateTest() {
		TestCase populateTable = new TestCase() {

			@Override
			protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

				AllMovies allMovies = new AllMovies();
				allMovies.setMovies(QueryTestData.getAllMovies());
				final TestCase test = this;

				MobileServiceClient otherClient = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						String json = request.getContent();

						// remove IDs from the array
						json = json.replace("\"id\":0,", "");
						try {
							request.setContent(json);
						} catch (Exception e) {
							// do nothing
						}

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				log("insert movies");
				otherClient.getTable(MOVIES_TABLE_NAME, AllMovies.class).insert(allMovies, new TableOperationCallback<AllMovies>() {

					@Override
					public void onCompleted(AllMovies entity, Exception exception, ServiceFilterResponse response) {
						TestResult result = null;

						if (exception == null) {
							result = new TestResult();
							result.setTestCase(test);
							result.setStatus(TestStatus.Passed);

						} else {
							result = createResultFromException(exception);
						}

						if (callback != null)
							callback.onTestComplete(test, result);
					}

				});
			}
		};

		populateTable.setName("Populate table, if necessary");

		this.addTest(populateTable);

	}

	private TestCase createQueryTest(String name, final MobileServiceQuery<?> filter, final ListFilter<Movie> expectedResultFilter) {

		return createQueryTest(name, filter, expectedResultFilter, null, null, null, null, false, null);
	}

	private TestCase createQueryTest(String name, final MobileServiceQuery<?> filter, final ListFilter<Movie> expectedResultFilter, final int top) {

		return createQueryTest(name, filter, expectedResultFilter, top, null, null, null, false, null);
	}

	private TestCase createQueryTest(String name, final MobileServiceQuery<?> filter, final ListFilter<Movie> expectedResultFilter, final Integer top,
			final Integer skip, final List<Pair<String, QueryOrder>> orderBy, final String[] projection, final boolean includeInlineCount,
			final Class<?> expectedExceptionClass) {

		final TestCase test = new TestCase() {

			@Override
			protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

				MobileServiceQuery<TableQueryCallback<Movie>> query;

				if (filter != null) {
					log("add filter");
					query = client.getTable(MOVIES_TABLE_NAME, Movie.class).where(filter);
				} else {
					query = client.getTable(MOVIES_TABLE_NAME, Movie.class).where();
				}

				if (top != null) {
					log("add top");
					query = query.top(top);
				}

				if (skip != null) {
					log("add skip");
					query = query.skip(skip);
				}

				if (orderBy != null) {
					log("add orderby");
					for (Pair<String, QueryOrder> order : orderBy) {
						query = query.orderBy(order.first, order.second);
					}
				}

				if (projection != null) {
					log("add projection");
					query = query.select(projection);
				}

				if (includeInlineCount) {
					log("add inlinecount");
					query.includeInlineCount();
				}

				final TestCase testCase = this;
				query.execute(new TableQueryCallback<Movie>() {

					@Override
					public void onCompleted(List<Movie> movies, int count, Exception exception, ServiceFilterResponse response) {
						TestResult result = new TestResult();
						result.setStatus(TestStatus.Passed);
						result.setTestCase(testCase);

						if (exception == null) {
							FilterResult<Movie> expectedData = expectedResultFilter.filter(QueryTestData.getAllMovies());

							log("verify result");
							if (Util.compareLists(expectedData.elements, movies)) {

								if (includeInlineCount) {
									log("verify inline count");
									if (expectedData.totalCount != count) {
										createResultFromException(result, new ExpectedValueException(expectedData.totalCount, count));
									}
								}
							} else {
								createResultFromException(result,
										new ExpectedValueException(Util.listToString(expectedData.elements), Util.listToString(movies)));
							}
						} else {
							createResultFromException(result, exception);
						}

						if (callback != null)
							callback.onTestComplete(testCase, result);
					}

				});
			}
		};

		test.setExpectedExceptionClass(expectedExceptionClass);
		test.setName(name);

		return test;
	}
}
