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

import android.util.Pair;

import com.google.common.util.concurrent.ListenableFuture;
import com.google.gson.Gson;
import com.google.gson.JsonElement;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.http.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.table.TableJsonQueryCallback;
import com.microsoft.windowsazure.mobileservices.table.TableQueryCallback;
import com.microsoft.windowsazure.mobileservices.table.query.ExecutableJsonQuery;
import com.microsoft.windowsazure.mobileservices.table.query.ExecutableQuery;
import com.microsoft.windowsazure.mobileservices.table.query.Query;
import com.microsoft.windowsazure.mobileservices.table.query.QueryOrder;
import com.microsoft.windowsazure.mobileservices.table.serialization.JsonEntityParser;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.ExpectedValueException;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestCase;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestExecutionCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestGroup;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestResult;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestStatus;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.Util;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.AllIntIdMovies;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.FilterResult;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.IntIdMovie;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.ListFilter;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.Movie;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.MovieComparator;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.SimpleMovieFilter;

import java.util.ArrayList;
import java.util.Calendar;
import java.util.Collections;
import java.util.List;
import java.util.Locale;

import static com.microsoft.windowsazure.mobileservices.table.query.QueryOperations.day;
import static com.microsoft.windowsazure.mobileservices.table.query.QueryOperations.endsWith;
import static com.microsoft.windowsazure.mobileservices.table.query.QueryOperations.field;
import static com.microsoft.windowsazure.mobileservices.table.query.QueryOperations.floor;
import static com.microsoft.windowsazure.mobileservices.table.query.QueryOperations.month;
import static com.microsoft.windowsazure.mobileservices.table.query.QueryOperations.query;
import static com.microsoft.windowsazure.mobileservices.table.query.QueryOperations.startsWith;
import static com.microsoft.windowsazure.mobileservices.table.query.QueryOperations.subStringOf;
import static com.microsoft.windowsazure.mobileservices.table.query.QueryOperations.toLower;
import static com.microsoft.windowsazure.mobileservices.table.query.QueryOperations.toUpper;
import static com.microsoft.windowsazure.mobileservices.table.query.QueryOperations.val;
import static com.microsoft.windowsazure.mobileservices.table.query.QueryOperations.year;

public class QueryTests extends TestGroup {
    private static final String MOVIES_TABLE_NAME = "IntIdMovies";

    // //private static final String STRING_ID_MOVIES_TABLE_NAME = "Movies";

    @SuppressWarnings("unchecked")
    public QueryTests() {
        super("Query tests");

        // numeric functions
        this.addTest(createQueryTest("GreaterThan and LessThan - Movies from the 90s", field("Year").gt(1989).and().field("Year").lt(2000),
                new SimpleMovieFilter() {

                    @Override
                    protected boolean criteria(Movie movie) {
                        return movie.getYear() > 1989 && movie.getYear() < 2000;
                    }
                }));

        this.addTest(createQueryTest("GreaterEqual and LessEqual - Movies from the 90s", field("Year").ge(1990).and().field("Year").le(1999),
                new SimpleMovieFilter() {

                    @Override
                    protected boolean criteria(Movie movie) {
                        return movie.getYear() >= 1990 && movie.getYear() <= 1999;
                    }
                }));

        this.addTest(createQueryTest("Compound statement - OR of ANDs - Movies from the 30s and 50s",
                field("Year").ge(1930).and().field("Year").lt(1940).or(field("Year").ge(1950).and().field("Year").lt(1960)), new SimpleMovieFilter() {

                    @Override
                    protected boolean criteria(Movie movie) {
                        return (movie.getYear() >= 1930 && movie.getYear() < 1940) || (movie.getYear() >= 1950 && movie.getYear() < 1960);
                    }
                }));

        this.addTest(createQueryTest("Division, equal and different - Movies from the Year 2000 with rating other than R", query(field("Year").div(1000d))
                .eq(2).and().field("MpaaRating").ne("R"), new SimpleMovieFilter() {

            @Override
            protected boolean criteria(Movie movie) {
                return (movie.getYear() / 1000d == 2) && movie.getMPAARating() != "R";
            }
        }));

        this.addTest(createQueryTest("Addition, subtraction, relational, AND - Movies from the 1980s which last less than 2 hours",
                field("Year").sub(1900).ge(80).and().field("Year").add(10).lt(2000).and().field("Duration").lt(120), new SimpleMovieFilter() {

                    @Override
                    protected boolean criteria(Movie movie) {
                        return (movie.getYear() - 1900 >= 80) && (movie.getYear() + 10 < 2000) && (movie.getDuration() < 120);
                    }
                }));

        // string functions

        this.addTest(createQueryTest("StartsWith - Movies which starts with 'The'", startsWith("Title", "The"), new SimpleMovieFilter() {

            @Override
            protected boolean criteria(Movie movie) {
                return movie.getTitle().startsWith("The");
            }

            @Override
            public FilterResult<Movie> filter(List<? extends Movie> list) {
                return applyTopSkip(super.filter(list), 100, 0);
            }
        }, 100));

        this.addTest(createQueryTest("StartsWith, case insensitive - Movies which start with 'the'", startsWith(toLower(field("Title")), val("the")),
                new SimpleMovieFilter() {

                    @Override
                    protected boolean criteria(Movie movie) {
                        return movie.getTitle().toLowerCase(Locale.getDefault()).startsWith("the");
                    }

                    @Override
                    public FilterResult<Movie> filter(List<? extends Movie> list) {
                        return applyTopSkip(super.filter(list), 100, 0);
                    }
                }, 100));

        this.addTest(createQueryTest("EndsWith, case insensitive - Movies which end with 'r'", endsWith(toLower(field("Title")), val("r")),
                new SimpleMovieFilter() {

                    @Override
                    protected boolean criteria(Movie movie) {
                        return movie.getTitle().toLowerCase(Locale.getDefault()).endsWith("r");
                    }
                }));

        this.addTest(createQueryTest("Contains - Movies which contain the word 'one', case insensitive", subStringOf(val("ONE"), toUpper(field("Title"))),
                new SimpleMovieFilter() {

                    @Override
                    protected boolean criteria(Movie movie) {
                        return movie.getTitle().toUpperCase(Locale.getDefault()).contains("ONE");
                    }
                }));

        this.addTest(createQueryTest("String equals - Movies since 1980 with rating PG-13", field("Year").ge(1980).and().field("MpaaRating").eq("PG-13"),
                new SimpleMovieFilter() {

                    @Override
                    protected boolean criteria(Movie movie) {
                        return movie.getYear() >= 1980 && movie.getMPAARating() == "PG-13";
                    }

                    @Override
                    public FilterResult<Movie> filter(List<? extends Movie> list) {
                        return applyTopSkip(super.filter(list), 100, 0);
                    }
                }, 100));

        this.addTest(createQueryTest("String field, comparison to null - Movies since 1980 without a MPAA rating",
                field("Year").ge(1980).and().field("MpaaRating").eq((String) null), new SimpleMovieFilter() {

                    @Override
                    protected boolean criteria(Movie movie) {
                        return movie.getYear() >= 1980 && movie.getMPAARating() == null;
                    }

                    @Override
                    public FilterResult<Movie> filter(List<? extends Movie> list) {
                        return applyTopSkip(super.filter(list), 100, 0);
                    }
                }, 100));

        this.addTest(createQueryTest("String field, comparison (not equal) to null - Movies before 1970 with a MPAA rating", field("Year").lt(1970).and()
                .field("MpaaRating").ne((String) null), new SimpleMovieFilter() {

            @Override
            protected boolean criteria(Movie movie) {
                return movie.getYear() < 1970 && movie.getMPAARating() != null;
            }

            @Override
            public FilterResult<Movie> filter(List<? extends Movie> list) {
                return applyTopSkip(super.filter(list), 100, 0);
            }
        }, 100));

        // Numeric functions
        this.addTest(createQueryTest("Floor - Movies which last more than 3 hours", floor(field("Duration").div(60d)).ge(3), new SimpleMovieFilter() {

            @Override
            protected boolean criteria(Movie movie) {
                return Math.floor(movie.getDuration() / 60d) >= 3;
            }
        }));

        this.addTest(createQueryTest("Ceiling - Best picture winners which last at most 2 hours",
                field("BestPictureWinner").eq(true).and().ceiling(field("Duration").div(60d)).eq(2), new SimpleMovieFilter() {

                    @Override
                    protected boolean criteria(Movie movie) {
                        return movie.isBestPictureWinner() && Math.ceil(movie.getDuration() / 60d) == 2;
                    }
                }));

        this.addTest(createQueryTest("Round - Best picture winners which last more than 2.5 hours",
                field("BestPictureWinner").eq(true).and().round(field("Duration").div(60d)).gt(2), new SimpleMovieFilter() {

                    @Override
                    protected boolean criteria(Movie movie) {
                        return movie.isBestPictureWinner() && Math.round(movie.getDuration() / 60d) > 2;
                    }
                }));

        this.addTest(createQueryTest("Date: Greater than, less than - Movies with release date in the 70s",
                field("ReleaseDate").gt(Util.getUTCDate(1969, 12, 31)).and().field("ReleaseDate").lt(Util.getUTCDate(1971, 1, 1)), new SimpleMovieFilter() {

                    @Override
                    protected boolean criteria(Movie movie) {
                        return movie.getReleaseDate().compareTo(Util.getUTCDate(1969, 12, 31)) > 0
                                && movie.getReleaseDate().compareTo(Util.getUTCDate(1971, 1, 1)) < 0;
                    }
                }));

        this.addTest(createQueryTest("Date: Greater than, less than - Movies with release date in the 80s", field("ReleaseDate")
                .ge(Util.getUTCDate(1980, 1, 1)).and().field("ReleaseDate").le(Util.getUTCDate(1989, 1, 1, 23, 59, 59)), new SimpleMovieFilter() {

            @Override
            protected boolean criteria(Movie movie) {
                return movie.getReleaseDate().compareTo(Util.getUTCDate(1980, 1, 1)) >= 0
                        && movie.getReleaseDate().compareTo(Util.getUTCDate(1989, 1, 1, 23, 59, 59)) <= 0;
            }
        }));

        this.addTest(createQueryTest("Date: Date: Equal - Movies released on 1994-10-14 (Shawshank Redemption / Pulp Fiction)",
                field("ReleaseDate").eq(Util.getUTCDate(1994, 10, 14)), new SimpleMovieFilter() {

                    @Override
                    protected boolean criteria(Movie movie) {
                        return movie.getReleaseDate().compareTo(Util.getUTCDate(1994, 10, 14)) == 0;
                    }
                }));

        // Date functions
        this.addTest(createQueryTest("Date (month): Movies released in November", month("ReleaseDate").eq(11), new SimpleMovieFilter() {

            @Override
            protected boolean criteria(Movie movie) {
                return Util.getUTCCalendar(movie.getReleaseDate()).get(Calendar.MONTH) == Calendar.NOVEMBER;
            }
        }));

        this.addTest(createQueryTest("Date (day): Movies released in the first day of the month", day("ReleaseDate").eq(1), new SimpleMovieFilter() {

            @Override
            protected boolean criteria(Movie movie) {
                return Util.getUTCCalendar(movie.getReleaseDate()).get(Calendar.DAY_OF_MONTH) == 1;
            }
        }));

        this.addTest(createQueryTest("Date (Year): Movies whose Year is different than its release Year", year("ReleaseDate").ne().field("Year"),
                new SimpleMovieFilter() {

                    @Override
                    protected boolean criteria(Movie movie) {
                        return Util.getUTCCalendar(movie.getReleaseDate()).get(Calendar.YEAR) != movie.getYear();
                    }

                    @Override
                    public FilterResult<Movie> filter(List<? extends Movie> list) {
                        return applyTopSkip(super.filter(list), 100, 0);
                    }
                }, 100));

        // boolean fields
        this.addTest(createQueryTest("Bool: equal to true - Best picture winners before 1950",
                field("Year").lt(1950).and().field("BestPictureWinner").eq(true), new SimpleMovieFilter() {

                    @Override
                    protected boolean criteria(Movie movie) {
                        return movie.getYear() < 1950 && movie.isBestPictureWinner();
                    }
                }));

        this.addTest(createQueryTest("Bool: equal to false - Best picture winners after 2000",
                field("Year").ge(2000).and().not(field("BestPictureWinner").eq(false)), new SimpleMovieFilter() {

                    @Override
                    protected boolean criteria(Movie movie) {
                        return movie.getYear() >= 2000 && !(movie.isBestPictureWinner() == false);
                    }
                }));

        this.addTest(createQueryTest("Bool: not equal to false - Best picture winners after 2000",
                field("BestPictureWinner").ne(false).and().field("Year").ge(2000), new SimpleMovieFilter() {

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
            public FilterResult<Movie> filter(List<? extends Movie> list) {
                return applyTopSkip(super.filter(list), 500, 0);
            }
        }, 500));

        this.addTest(createQueryTest("Skip all using large skip - 500", null, new SimpleMovieFilter() {

            @Override
            protected boolean criteria(Movie movie) {
                return true;
            }

            @Override
            public FilterResult<Movie> filter(List<? extends Movie> list) {
                return applyTopSkip(super.filter(list), Integer.MAX_VALUE, 500);
            }

            @Override
            protected List<Movie> applyOrder(List<? extends Movie> movies) {
                Collections.sort(movies, new MovieComparator("getTitle"));
                return new ArrayList<Movie>(movies);
            }
        }, null, 500, createOrder(new Pair<String, QueryOrder>("Title", QueryOrder.Ascending)), null, false, null));

        this.addTest(createQueryTest("Get first ($top) - 10", null, new SimpleMovieFilter() {

            @Override
            protected boolean criteria(Movie movie) {
                return true;
            }

            @Override
            public FilterResult<Movie> filter(List<? extends Movie> list) {
                return applyTopSkip(super.filter(list), 10, 0);
            }
        }, 10));

        final int movieCountMinus10 = QueryTestData.getAllIntIdMovies().size() - 10;
        this.addTest(createQueryTest("Get last ($skip) - 10", null, new SimpleMovieFilter() {

            @Override
            protected boolean criteria(Movie movie) {
                return true;
            }

            @Override
            public FilterResult<Movie> filter(List<? extends Movie> list) {
                return applyTopSkip(super.filter(list), Integer.MAX_VALUE, movieCountMinus10);
            }

            @Override
            protected List<Movie> applyOrder(List<? extends Movie> movies) {
                Collections.sort(movies, new MovieComparator("getTitle"));
                return new ArrayList<Movie>(movies);
            }
        }, null, movieCountMinus10, createOrder(new Pair<String, QueryOrder>("Title", QueryOrder.Ascending)), null, false, null));

        this.addTest(createQueryTest("Skip, take, includeTotalCount - movies 21-30, ordered by Title", null, new SimpleMovieFilter() {

            @Override
            protected boolean criteria(Movie movie) {
                return true;
            }

            @Override
            public FilterResult<Movie> filter(List<? extends Movie> list) {
                return applyTopSkip(super.filter(list), 10, 20);
            }

            @Override
            protected List<Movie> applyOrder(List<? extends Movie> movies) {
                Collections.sort(movies, new MovieComparator("getTitle"));
                return new ArrayList<Movie>(movies);
            }

        }, 10, 20, createOrder(new Pair<String, QueryOrder>("Title", QueryOrder.Ascending)), null, false, null));

        this.addTest(createQueryTest("Skip, take, filter includeTotalCount - movies 11-20 which won a best picture award, ordered by Year",
                field("BestPictureWinner").eq(true), new SimpleMovieFilter() {

                    @Override
                    protected boolean criteria(Movie movie) {
                        return movie.isBestPictureWinner();
                    }

                    @Override
                    public FilterResult<Movie> filter(List<? extends Movie> list) {
                        return applyTopSkip(super.filter(list), 10, 10);
                    }

                    @Override
                    protected List<Movie> applyOrder(List<? extends Movie> movies) {
                        Collections.sort(movies, new MovieComparator(new Pair<String, QueryOrder>("getYear", QueryOrder.Descending)));
                        return new ArrayList<Movie>(movies);
                    }

                }, 10, 10, createOrder(new Pair<String, QueryOrder>("Year", QueryOrder.Descending)), null, true, null));

        this.addTest(createQueryTest("Select one field - Only Title of movies from 2008", field("Year").eq(2008), new SimpleMovieFilter() {

            @Override
            protected FilterResult<Movie> getElements(List<? extends Movie> list) {
                FilterResult<Movie> res = super.getElements(list);
                FilterResult<Movie> newRes = new FilterResult<Movie>();
                newRes.totalCount = res.totalCount;
                newRes.elements = new ArrayList<Movie>();

                for (Movie movie : res.elements) {
                    // only add Title field
                    Movie newMovie = new IntIdMovie();
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

        this.addTest(createQueryTest("Select multiple fields - List of movies from the 2000's", field("Year").ge(2000), new SimpleMovieFilter() {

                    @Override
                    protected FilterResult<Movie> getElements(List<? extends Movie> list) {
                        FilterResult<Movie> res = super.getElements(list);
                        FilterResult<Movie> newRes = new FilterResult<Movie>();
                        newRes.totalCount = res.totalCount;
                        newRes.elements = new ArrayList<Movie>();

                        for (Movie movie : res.elements) {
                            // only add Title, BestPictureWinner,
                            // ReleaseDate
                            // and Duration fields
                            IntIdMovie newMovie = new IntIdMovie();
                            newMovie.setTitle(movie.getTitle());
                            newMovie.setBestPictureWinner(movie.isBestPictureWinner());
                            newMovie.setReleaseDate(movie.getReleaseDate());
                            newMovie.setDuration(movie.getDuration());
                            newRes.elements.add(newMovie);
                        }

                        return newRes;
                    }

                    @Override
                    public FilterResult<Movie> filter(List<? extends Movie> list) {
                        return applyTopSkip(super.filter(list), 5, 0);
                    }

                    @Override
                    protected boolean criteria(Movie movie) {
                        return movie.getYear() >= 2000;
                    }

                    @Override
                    protected List<Movie> applyOrder(List<? extends Movie> movies) {
                        Collections.sort(movies, new MovieComparator(new Pair<String, QueryOrder>("getReleaseDate", QueryOrder.Descending),
                                new Pair<String, QueryOrder>("getTitle", QueryOrder.Ascending)));
                        return new ArrayList<Movie>(movies);
                    }

                }, 5, null,
                createOrder(new Pair<String, QueryOrder>("ReleaseDate", QueryOrder.Descending), new Pair<String, QueryOrder>("Title", QueryOrder.Ascending)),
                project("title", "bestPictureWinner", "duration", "releaseDate"), true, null));

        this.addTest(createQueryTest("(Neg) Very large top value", field("Year").gt(2000), null, 1001, null, null, null, false, MobileServiceException.class));

        this.addTest(createJsonQueryTest("Select multiple fields - Json - List of movies from the 2000's", field("Year").ge(2000), new SimpleMovieFilter() {

                    @Override
                    protected FilterResult<Movie> getElements(List<? extends Movie> list) {
                        FilterResult<Movie> res = super.getElements(list);
                        FilterResult<Movie> newRes = new FilterResult<Movie>();
                        newRes.totalCount = res.totalCount;
                        newRes.elements = new ArrayList<Movie>();

                        for (Movie movie : res.elements) {
                            // only add Title, BestPictureWinner,
                            // ReleaseDate
                            // and Duration fields
                            IntIdMovie newMovie = new IntIdMovie();
                            newMovie.setTitle(movie.getTitle());
                            newMovie.setBestPictureWinner(movie.isBestPictureWinner());
                            newMovie.setReleaseDate(movie.getReleaseDate());
                            newMovie.setDuration(movie.getDuration());
                            newRes.elements.add(newMovie);
                        }

                        return newRes;
                    }

                    @Override
                    public FilterResult<Movie> filter(List<? extends Movie> list) {
                        return applyTopSkip(super.filter(list), 5, 0);
                    }

                    @Override
                    protected boolean criteria(Movie movie) {
                        return movie.getYear() >= 2000;
                    }

                    @Override
                    protected List<Movie> applyOrder(List<? extends Movie> movies) {
                        Collections.sort(movies, new MovieComparator(new Pair<String, QueryOrder>("getReleaseDate", QueryOrder.Descending),
                                new Pair<String, QueryOrder>("getTitle", QueryOrder.Ascending)));
                        return new ArrayList<Movie>(movies);
                    }

                }, 5, null,
                createOrder(new Pair<String, QueryOrder>("ReleaseDate", QueryOrder.Descending), new Pair<String, QueryOrder>("Title", QueryOrder.Ascending)),
                project("title", "bestPictureWinner", "duration", "releaseDate"), true, null));

        // invalid lookup
        for (int i = -1; i <= 0; i++) {
            final int id = i;
            TestCase test = new TestCase() {

                @Override
                protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

                    final TestCase testCase = this;

                    log("lookup id " + id);

                    TestResult result = new TestResult();
                    result.setTestCase(testCase);

                    try {
                        client.getTable(MOVIES_TABLE_NAME).lookUp(id).get();
                        result.setStatus(TestStatus.Passed);

                    } catch (Exception exception) {
                        createResultFromException(result, exception);
                        result.setStatus(TestStatus.Failed);
                    } finally {
                        if (callback != null)
                            callback.onTestComplete(testCase, result);
                    }
                }
            };

            test.setExpectedExceptionClass(IllegalArgumentException.class);
            test.setName("(Neg) Invalid Lookup - ID: " + id);
            this.addTest(test);
        }

        // With Callback
        this.addTest(createQueryWithCallbackTest("With Callback - Addition, subtraction, relational, AND - Movies from the 1980s which last less than 2 hours",
                field("Year").sub(1900).ge(80).and().field("Year").add(10).lt(2000).and().field("Duration").lt(120), new SimpleMovieFilter() {

                    @Override
                    protected boolean criteria(Movie movie) {
                        return (movie.getYear() - 1900 >= 80) && (movie.getYear() + 10 < 2000) && (movie.getDuration() < 120);
                    }
                }));

        this.addTest(createQueryWithCallbackTest("With Callback - String field, comparison (not equal) to null - Movies before 1970 with a MPAA rating",
                field("Year").lt(1970).and().field("MpaaRating").ne((String) null), new SimpleMovieFilter() {

                    @Override
                    protected boolean criteria(Movie movie) {
                        return movie.getYear() < 1970 && movie.getMPAARating() != null;
                    }

                    @Override
                    public FilterResult<Movie> filter(List<? extends Movie> list) {
                        return applyTopSkip(super.filter(list), 100, 0);
                    }
                }, 100));

        this.addTest(createQueryWithCallbackTest("With Callback - Date: Date: Equal - Movies released on 1994-10-14 (Shawshank Redemption / Pulp Fiction)",
                field("ReleaseDate").eq(Util.getUTCDate(1994, 10, 14)), new SimpleMovieFilter() {

                    @Override
                    protected boolean criteria(Movie movie) {
                        return movie.getReleaseDate().compareTo(Util.getUTCDate(1994, 10, 14)) == 0;
                    }
                }));

        this.addTest(createQueryWithCallbackTest("With Callback - Date (Year): Movies whose Year is different than its release Year", year("ReleaseDate").ne()
                .field("Year"), new SimpleMovieFilter() {

            @Override
            protected boolean criteria(Movie movie) {
                return Util.getUTCCalendar(movie.getReleaseDate()).get(Calendar.YEAR) != movie.getYear();
            }

            @Override
            public FilterResult<Movie> filter(List<? extends Movie> list) {
                return applyTopSkip(super.filter(list), 100, 0);
            }
        }, 100));

        this.addTest(createQueryWithCallbackTest("With Callback - Bool: not equal to false - Best picture winners after 2000",
                field("BestPictureWinner").ne(false).and().field("Year").ge(2000), new SimpleMovieFilter() {

                    @Override
                    protected boolean criteria(Movie movie) {
                        return movie.isBestPictureWinner() != false && movie.getYear() >= 2000;
                    }
                }));

        this.addTest(createQueryWithCallbackTest("With Callback - Select multiple fields - List of movies from the 2000's", field("Year").ge(2000),
                new SimpleMovieFilter() {

                    @Override
                    protected FilterResult<Movie> getElements(List<? extends Movie> list) {
                        FilterResult<Movie> res = super.getElements(list);
                        FilterResult<Movie> newRes = new FilterResult<Movie>();
                        newRes.totalCount = res.totalCount;
                        newRes.elements = new ArrayList<Movie>();

                        for (Movie movie : res.elements) {
                            // only add Title, BestPictureWinner,
                            // ReleaseDate
                            // and Duration fields
                            Movie newMovie = new IntIdMovie();
                            newMovie.setTitle(movie.getTitle());
                            newMovie.setBestPictureWinner(movie.isBestPictureWinner());
                            newMovie.setReleaseDate(movie.getReleaseDate());
                            newMovie.setDuration(movie.getDuration());
                            newRes.elements.add(newMovie);
                        }

                        return newRes;
                    }

                    @Override
                    public FilterResult<Movie> filter(List<? extends Movie> list) {
                        return applyTopSkip(super.filter(list), 5, 0);
                    }

                    @Override
                    protected boolean criteria(Movie movie) {
                        return movie.getYear() >= 2000;
                    }

                    @Override
                    protected List<Movie> applyOrder(List<? extends Movie> movies) {
                        Collections.sort(movies, new MovieComparator(new Pair<String, QueryOrder>("getReleaseDate", QueryOrder.Descending),
                                new Pair<String, QueryOrder>("getTitle", QueryOrder.Ascending)));
                        return new ArrayList<Movie>(movies);
                    }

                }, 5, null,
                createOrder(new Pair<String, QueryOrder>("ReleaseDate", QueryOrder.Descending), new Pair<String, QueryOrder>("Title", QueryOrder.Ascending)),
                project("title", "bestPictureWinner", "duration", "releaseDate"), true, null));

        this.addTest(createJsonQueryWithCallbackTest("With Callback - Select multiple fields - Json - List of movies from the 2000's", field("Year").ge(2000),
                new SimpleMovieFilter() {

                    @Override
                    protected FilterResult<Movie> getElements(List<? extends Movie> list) {
                        FilterResult<Movie> res = super.getElements(list);
                        FilterResult<Movie> newRes = new FilterResult<Movie>();
                        newRes.totalCount = res.totalCount;
                        newRes.elements = new ArrayList<Movie>();

                        for (Movie movie : res.elements) {
                            // only add Title, BestPictureWinner,
                            // ReleaseDate
                            // and Duration fields
                            Movie newMovie = new IntIdMovie();
                            newMovie.setTitle(movie.getTitle());
                            newMovie.setBestPictureWinner(movie.isBestPictureWinner());
                            newMovie.setReleaseDate(movie.getReleaseDate());
                            newMovie.setDuration(movie.getDuration());
                            newRes.elements.add(newMovie);
                        }

                        return newRes;
                    }

                    @Override
                    public FilterResult<Movie> filter(List<? extends Movie> list) {
                        return applyTopSkip(super.filter(list), 5, 0);
                    }

                    @Override
                    protected boolean criteria(Movie movie) {
                        return movie.getYear() >= 2000;
                    }

                    @Override
                    protected List<Movie> applyOrder(List<? extends Movie> movies) {
                        Collections.sort(movies, new MovieComparator(new Pair<String, QueryOrder>("getReleaseDate", QueryOrder.Descending),
                                new Pair<String, QueryOrder>("getTitle", QueryOrder.Ascending)));
                        return new ArrayList<Movie>(movies);
                    }

                }, 5, null,
                createOrder(new Pair<String, QueryOrder>("ReleaseDate", QueryOrder.Descending), new Pair<String, QueryOrder>("Title", QueryOrder.Ascending)),
                project("title", "bestPictureWinner", "duration", "releaseDate"), true, null));
    }

    private static String[] project(String... fields) {
        return fields;
    }

    @SuppressWarnings("unchecked")
    private List<Pair<String, QueryOrder>> createOrder(Pair<String, QueryOrder>... entries) {
        List<Pair<String, QueryOrder>> ret = new ArrayList<Pair<String, QueryOrder>>();

        for (Pair<String, QueryOrder> Pair : entries) {
            ret.add(Pair);
        }

        return ret;
    }

    private TestCase createQueryTest(String name, final Query filter, final ListFilter<Movie> expectedResultFilter) {
        return createQueryTest(name, filter, expectedResultFilter, null, null, null, null, false, null);
    }

    private TestCase createQueryTest(String name, final Query filter, final ListFilter<Movie> expectedResultFilter, final int top) {
        return createQueryTest(name, filter, expectedResultFilter, top, null, null, null, false, null);
    }

    private TestCase createQueryTest(String name, final Query filter, final ListFilter<Movie> expectedResultFilter, final Integer top, final Integer skip,
                                     final List<Pair<String, QueryOrder>> orderBy, final String[] projection, final boolean includeInlineCount, final Class<?> expectedExceptionClass) {

        final TestCase test = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

                ExecutableQuery<IntIdMovie> query;

                if (filter != null) {
                    log("add filter");
                    query = client.getTable(MOVIES_TABLE_NAME, IntIdMovie.class).where(filter);
                } else {
                    query = client.getTable(MOVIES_TABLE_NAME, IntIdMovie.class).where();
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

                TestCase testCase = this;
                TestResult result = new TestResult();
                result.setStatus(TestStatus.Passed);
                result.setTestCase(testCase);

                try {
                    List<IntIdMovie> movies = query.execute().get();

                    FilterResult<Movie> expectedData = expectedResultFilter.filter(QueryTestData.getAllIntIdMovies());

                    log("verify result");

                    if (Util.compareLists(expectedData.elements, new ArrayList<Movie>(movies))) {
                    } else {
                        createResultFromException(result, new ExpectedValueException(Util.listToString(expectedData.elements), Util.listToString(movies)));
                    }
                } catch (Exception exception) {

                    createResultFromException(result, exception);
                } finally {
                    callback.onTestComplete(testCase, result);

                }
            }
        };

        test.setExpectedExceptionClass(expectedExceptionClass);
        test.setName(name);

        return test;
    }

    private TestCase createJsonQueryTest(String name, final Query filter, final ListFilter<Movie> expectedResultFilter, final Integer top, final Integer skip,
                                         final List<Pair<String, QueryOrder>> orderBy, final String[] projection, final boolean includeInlineCount, final Class<?> expectedExceptionClass) {

        final TestCase test = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

                ExecutableJsonQuery query;

                if (filter != null) {
                    log("add filter");
                    query = client.getTable(MOVIES_TABLE_NAME).where(filter);
                } else {
                    query = client.getTable(MOVIES_TABLE_NAME).where();
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

                TestCase testCase = this;
                TestResult result = new TestResult();
                result.setStatus(TestStatus.Passed);
                result.setTestCase(testCase);

                try {
                    JsonElement moviesJson = query.execute().get();
                    List<IntIdMovie> movies;

                    Gson gson = client.getGsonBuilder().create();

                    if (moviesJson.isJsonObject()) {
                        JsonElement elements = moviesJson.getAsJsonObject().get("results");

                        movies = JsonEntityParser.parseResults(elements, gson, IntIdMovie.class);
                    } else {
                        movies = JsonEntityParser.parseResults(moviesJson, gson, IntIdMovie.class);
                    }

                    FilterResult<Movie> expectedData = expectedResultFilter.filter(QueryTestData.getAllIntIdMovies());

                    log("verify result");

                    if (Util.compareLists(expectedData.elements, new ArrayList<Movie>(movies))) {
                    } else {
                        createResultFromException(result, new ExpectedValueException(Util.listToString(expectedData.elements), Util.listToString(movies)));
                    }

                } catch (Exception exception) {

                    createResultFromException(result, exception);
                } finally {
                    callback.onTestComplete(testCase, result);

                }
            }
        };

        test.setExpectedExceptionClass(expectedExceptionClass);
        test.setName(name);

        return test;
    }

    private TestCase createQueryWithCallbackTest(String name, final Query filter, final ListFilter<Movie> expectedResultFilter) {

        return createQueryTest(name, filter, expectedResultFilter, null, null, null, null, false, null);
    }

    private TestCase createQueryWithCallbackTest(String name, final Query filter, final ListFilter<Movie> expectedResultFilter, final int top) {

        return createQueryTest(name, filter, expectedResultFilter, top, null, null, null, false, null);
    }

    @SuppressWarnings("deprecation")
    private TestCase createQueryWithCallbackTest(String name, final Query filter, final ListFilter<Movie> expectedResultFilter, final Integer top,
                                                 final Integer skip, final List<Pair<String, QueryOrder>> orderBy, final String[] projection, final boolean includeInlineCount,
                                                 final Class<?> expectedExceptionClass) {

        final TestCase test = new TestCase() {
            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

                ExecutableQuery<IntIdMovie> query;

                if (filter != null) {
                    log("add filter");
                    query = client.getTable(MOVIES_TABLE_NAME, IntIdMovie.class).where(filter);
                } else {
                    query = client.getTable(MOVIES_TABLE_NAME, IntIdMovie.class).where();
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

                final TestResult result = new TestResult();
                result.setStatus(TestStatus.Passed);
                result.setTestCase(testCase);

                try {
                    query.execute(new TableQueryCallback<IntIdMovie>() {

                        @Override
                        public void onCompleted(List<IntIdMovie> movies, int count, Exception exception, ServiceFilterResponse response) {

                            if (exception == null) {
                                FilterResult<Movie> expectedData = expectedResultFilter.filter(QueryTestData.getAllIntIdMovies());

                                log("verify result");
                                if (Util.compareLists(expectedData.elements, new ArrayList<Movie>(movies))) {
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
                } catch (Exception exception) {
                    createResultFromException(result, exception);
                    callback.onTestComplete(testCase, result);
                }
            }
        };

        test.setExpectedExceptionClass(expectedExceptionClass);
        test.setName(name);

        return test;
    }

    @SuppressWarnings("deprecation")
    private TestCase createJsonQueryWithCallbackTest(String name, final Query filter, final ListFilter<Movie> expectedResultFilter, final Integer top,
                                                     final Integer skip, final List<Pair<String, QueryOrder>> orderBy, final String[] projection, final boolean includeInlineCount,
                                                     final Class<?> expectedExceptionClass) {

        final TestCase test = new TestCase() {
            @Override
            protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {

                ExecutableJsonQuery query;

                if (filter != null) {
                    log("add filter");
                    query = client.getTable(MOVIES_TABLE_NAME).where(filter);
                } else {
                    query = client.getTable(MOVIES_TABLE_NAME).where();
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

                final TestResult result = new TestResult();
                result.setStatus(TestStatus.Passed);
                result.setTestCase(testCase);

                try {
                    query.execute(new TableJsonQueryCallback() {

                        @Override
                        public void onCompleted(JsonElement moviesJson, Exception exception, ServiceFilterResponse response) {

                            if (exception == null) {
                                List<IntIdMovie> movies;

                                Gson gson = client.getGsonBuilder().create();

                                if (moviesJson.isJsonObject()) {
                                    JsonElement elements = moviesJson.getAsJsonObject().get("results");

                                    movies = JsonEntityParser.parseResults(elements, gson, IntIdMovie.class);
                                } else {
                                    movies = JsonEntityParser.parseResults(moviesJson, gson, IntIdMovie.class);
                                }

                                FilterResult<Movie> expectedData = expectedResultFilter.filter(QueryTestData.getAllIntIdMovies());

                                log("verify result");
                                if (Util.compareLists(expectedData.elements, new ArrayList<Movie>(movies))) {
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
                } catch (Exception exception) {
                    createResultFromException(result, exception);
                    callback.onTestComplete(testCase, result);
                }
            }
        };

        test.setExpectedExceptionClass(expectedExceptionClass);
        test.setName(name);

        return test;
    }
}