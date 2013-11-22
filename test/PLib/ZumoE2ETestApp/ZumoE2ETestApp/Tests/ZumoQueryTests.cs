// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using ZumoE2ETestApp.Framework;
using ZumoE2ETestApp.Tests.Types;
using ZumoE2ETestApp.UIElements;

namespace ZumoE2ETestApp.Tests
{
    internal static class ZumoQueryTests
    {
        internal static ZumoTestGroup CreateTests()
        {
            ZumoTestGroup result = new ZumoTestGroup("Query tests");
            result.AddTest(CreatePopulateTableTest());
            result.AddTest(CreatePopulateStringIdTableTest());

            // Numeric fields
            result.AddTest(CreateQueryTestIntId("GreaterThan and LessThan - Movies from the 90s", m => m.Year > 1989 && m.Year < 2000));
            result.AddTest(CreateQueryTestIntId("GreaterEqual and LessEqual - Movies from the 90s", m => m.Year >= 1990 && m.Year <= 1999));
            result.AddTest(CreateQueryTestIntId("Compound statement - OR of ANDs - Movies from the 30s and 50s",
                m => ((m.Year >= 1930) && (m.Year < 1940)) || ((m.Year >= 1950) && (m.Year < 1960))));
            result.AddTest(CreateQueryTestIntId("Division, equal and different - Movies from the year 2000 with rating other than R",
                m => ((m.Year / 1000.0) == 2) && (m.MPAARating != "R")));
            result.AddTest(CreateQueryTestIntId("Addition, subtraction, relational, AND - Movies from the 1980s which last less than 2 hours",
                m => ((m.Year - 1900) >= 80) && (m.Year + 10 < 2000) && (m.Duration < 120)));

            result.AddTest(CreateQueryTestStringId("GreaterThan and LessThan - Movies from the 90s", m => m.Year > 1989 && m.Year < 2000));
            result.AddTest(CreateQueryTestStringId("GreaterEqual and LessEqual - Movies from the 90s", m => m.Year >= 1990 && m.Year <= 1999));
            result.AddTest(CreateQueryTestStringId("Compound statement - OR of ANDs - Movies from the 30s and 50s",
                m => ((m.Year >= 1930) && (m.Year < 1940)) || ((m.Year >= 1950) && (m.Year < 1960))));
            result.AddTest(CreateQueryTestStringId("Division, equal and different - Movies from the year 2000 with rating other than R",
                m => ((m.Year / 1000.0) == 2) && (m.MPAARating != "R")));
            result.AddTest(CreateQueryTestStringId("Addition, subtraction, relational, AND - Movies from the 1980s which last less than 2 hours",
                m => ((m.Year - 1900) >= 80) && (m.Year + 10 < 2000) && (m.Duration < 120)));

            // String functions
            result.AddTest(CreateQueryTestIntId("String: StartsWith - Movies which starts with 'The'",
                m => m.Title.StartsWith("The"), 100));
            result.AddTest(CreateQueryTestIntId("String: StartsWith, case insensitive - Movies which start with 'the'",
                m => m.Title.ToLower().StartsWith("the"), 100));
            result.AddTest(CreateQueryTestIntId("String: EndsWith, case insensitive - Movies which end with 'r'",
                m => m.Title.ToLower().EndsWith("r")));
            result.AddTest(CreateQueryTestIntId("String: Contains - Movies which contain the word 'one', case insensitive",
                m => m.Title.ToUpper().Contains("ONE")));
            result.AddTest(CreateQueryTestIntId("String: Length - Movies with small names",
                m => m.Title.Length < 10, 200));
            result.AddTest(CreateQueryTestIntId("String: Substring (1 parameter), length - Movies which end with 'r'",
                m => m.Title.Substring(m.Title.Length - 1) == "r"));
            result.AddTest(CreateQueryTestIntId("String: Substring (2 parameters), length - Movies which end with 'r'",
                m => m.Title.Substring(m.Title.Length - 1, 1) == "r"));
            result.AddTest(CreateQueryTestIntId("String: Replace - Movies ending with either 'Part 2' or 'Part II'",
                m => m.Title.Replace("II", "2").EndsWith("Part 2")));
            result.AddTest(CreateQueryTestIntId("String: Concat - Movies rated 'PG' or 'PG-13' from the 2000s",
                m => m.Year >= 2000 && string.Concat(m.MPAARating, "-13").StartsWith("PG-13")));

            result.AddTest(CreateQueryTestStringId("String: StartsWith - Movies which starts with 'The'",
                m => m.Title.StartsWith("The"), 100));
            result.AddTest(CreateQueryTestStringId("String: StartsWith, case insensitive - Movies which start with 'the'",
                m => m.Title.ToLower().StartsWith("the"), 100));
            result.AddTest(CreateQueryTestStringId("String: EndsWith, case insensitive - Movies which end with 'r'",
                m => m.Title.ToLower().EndsWith("r")));
            result.AddTest(CreateQueryTestStringId("String: Contains - Movies which contain the word 'one', case insensitive",
                m => m.Title.ToUpper().Contains("ONE")));
            result.AddTest(CreateQueryTestStringId("String: Length - Movies with small names",
                m => m.Title.Length < 10, 200));
            result.AddTest(CreateQueryTestStringId("String: Substring (1 parameter), length - Movies which end with 'r'",
                m => m.Title.Substring(m.Title.Length - 1) == "r"));
            result.AddTest(CreateQueryTestStringId("String: Substring (2 parameters), length - Movies which end with 'r'",
                m => m.Title.Substring(m.Title.Length - 1, 1) == "r"));
            result.AddTest(CreateQueryTestStringId("String: Replace - Movies ending with either 'Part 2' or 'Part II'",
                m => m.Title.Replace("II", "2").EndsWith("Part 2")));
            result.AddTest(CreateQueryTestStringId("String: Concat - Movies rated 'PG' or 'PG-13' from the 2000s",
                m => m.Year >= 2000 && string.Concat(m.MPAARating, "-13").StartsWith("PG-13")));

            // String fields
            result.AddTest(CreateQueryTestIntId("String equals - Movies since 1980 with rating PG-13",
                m => m.Year >= 1980 && m.MPAARating == "PG-13", 100));
            result.AddTest(CreateQueryTestIntId("String field, comparison to null - Movies since 1980 without a MPAA rating",
                m => m.Year >= 1980 && m.MPAARating == null));
            result.AddTest(CreateQueryTestIntId("String field, comparison (not equal) to null - Movies before 1970 with a MPAA rating",
                m => m.Year < 1970 && m.MPAARating != null));

            result.AddTest(CreateQueryTestStringId("String equals - Movies since 1980 with rating PG-13",
                m => m.Year >= 1980 && m.MPAARating == "PG-13", 100));
            result.AddTest(CreateQueryTestStringId("String field, comparison to null - Movies since 1980 without a MPAA rating",
                m => m.Year >= 1980 && m.MPAARating == null));
            result.AddTest(CreateQueryTestStringId("String field, comparison (not equal) to null - Movies before 1970 with a MPAA rating",
                m => m.Year < 1970 && m.MPAARating != null));

            // Numeric functions
            result.AddTest(CreateQueryTestIntId("Floor - Movies which last more than 3 hours",
                m => Math.Floor(m.Duration / 60.0) >= 3));
            result.AddTest(CreateQueryTestIntId("Ceiling - Best picture winners which last at most 2 hours",
                m => m.BestPictureWinner == true && Math.Ceiling(m.Duration / 60.0) == 2));
            result.AddTest(CreateQueryTestIntId("Round - Best picture winners which last more than 2.5 hours",
                m => m.BestPictureWinner == true && Math.Round(m.Duration / 60.0) > 2));

            result.AddTest(CreateQueryTestStringId("Floor - Movies which last more than 3 hours",
                m => Math.Floor(m.Duration / 60.0) >= 3));
            result.AddTest(CreateQueryTestStringId("Ceiling - Best picture winners which last at most 2 hours",
                m => m.BestPictureWinner == true && Math.Ceiling(m.Duration / 60.0) == 2));
            result.AddTest(CreateQueryTestStringId("Round - Best picture winners which last more than 2.5 hours",
                m => m.BestPictureWinner == true && Math.Round(m.Duration / 60.0) > 2));

            // Date fields
            result.AddTest(CreateQueryTestIntId("Date: Greater than, less than - Movies with release date in the 70s",
                m => m.ReleaseDate > new DateTime(1969, 12, 31, 0, 0, 0, DateTimeKind.Utc) &&
                    m.ReleaseDate < new DateTime(1971, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
            result.AddTest(CreateQueryTestIntId("Date: Greater than, less than - Movies with release date in the 80s",
                m => m.ReleaseDate >= new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc) &&
                    m.ReleaseDate < new DateTime(1989, 12, 31, 23, 59, 59, DateTimeKind.Utc)));
            result.AddTest(CreateQueryTestIntId("Date: Equal - Movies released on 1994-10-14 (Shawshank Redemption / Pulp Fiction)",
                m => m.ReleaseDate == new DateTime(1994, 10, 14, 0, 0, 0, DateTimeKind.Utc)));

            result.AddTest(CreateQueryTestStringId("Date: Greater than, less than - Movies with release date in the 70s",
                m => m.ReleaseDate > new DateTime(1969, 12, 31, 0, 0, 0, DateTimeKind.Utc) &&
                    m.ReleaseDate < new DateTime(1971, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
            result.AddTest(CreateQueryTestStringId("Date: Greater than, less than - Movies with release date in the 80s",
                m => m.ReleaseDate >= new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc) &&
                    m.ReleaseDate < new DateTime(1989, 12, 31, 23, 59, 59, DateTimeKind.Utc)));
            result.AddTest(CreateQueryTestStringId("Date: Equal - Movies released on 1994-10-14 (Shawshank Redemption / Pulp Fiction)",
                m => m.ReleaseDate == new DateTime(1994, 10, 14, 0, 0, 0, DateTimeKind.Utc)));

            // Date functions
            result.AddTest(CreateQueryTestIntId("Date (month): Movies released in November",
                m => m.ReleaseDate.Month == 11));
            result.AddTest(CreateQueryTestIntId("Date (day): Movies released in the first day of the month",
                m => m.ReleaseDate.Day == 1));
            result.AddTest(CreateQueryTestIntId("Date (year): Movies whose year is different than its release year",
                m => m.ReleaseDate.Year != m.Year, 100));

            result.AddTest(CreateQueryTestStringId("Date (month): Movies released in November",
                m => m.ReleaseDate.Month == 11));
            result.AddTest(CreateQueryTestStringId("Date (day): Movies released in the first day of the month",
                m => m.ReleaseDate.Day == 1));
            result.AddTest(CreateQueryTestStringId("Date (year): Movies whose year is different than its release year",
                m => m.ReleaseDate.Year != m.Year, 100));

            // Bool fields
            result.AddTest(CreateQueryTestIntId("Bool: equal to true - Best picture winners before 1950",
                m => m.Year < 1950 && m.BestPictureWinner == true));
            result.AddTest(CreateQueryTestIntId("Bool: equal to false - Best picture winners after 2000",
                m => m.Year >= 2000 && !(m.BestPictureWinner == false)));
            result.AddTest(CreateQueryTestIntId("Bool: not equal to false - Best picture winners after 2000",
                m => m.BestPictureWinner != false && m.Year >= 2000));

            result.AddTest(CreateQueryTestStringId("Bool: equal to true - Best picture winners before 1950",
                m => m.Year < 1950 && m.BestPictureWinner == true));
            result.AddTest(CreateQueryTestStringId("Bool: equal to false - Best picture winners after 2000",
                m => m.Year >= 2000 && !(m.BestPictureWinner == false)));
            result.AddTest(CreateQueryTestStringId("Bool: not equal to false - Best picture winners after 2000",
                m => m.BestPictureWinner != false && m.Year >= 2000));

            // Top and skip
            result.AddTest(CreateQueryTestIntId("Get all using large $top - 500", null, 500));
            result.AddTest(CreateQueryTestIntId("Skip all using large skip - 500", null, null, 500));
            result.AddTest(CreateQueryTestIntId("Get first ($top) - 10", null, 10));
            result.AddTest(CreateQueryTestIntId("Get last ($skip) - 10", null, null, ZumoQueryTestData.AllMovies.Length - 10));
            result.AddTest(CreateQueryTestIntId("Skip, take, includeTotalCount - movies 11-20, ordered by title",
                null, 10, 10, new[] { new OrderByClause("Title", true) }, null, true));
            result.AddTest(CreateQueryTestIntId("Skip, take, filter includeTotalCount - movies 11-20 which won a best picture award, ordered by year",
                m => m.BestPictureWinner == true, 10, 10, new[] { new OrderByClause("Year", false) }, null, true));

            result.AddTest(CreateQueryTestStringId("Get all using large $top - 500", null, 500));
            result.AddTest(CreateQueryTestStringId("Skip all using large skip - 500", null, null, 500));
            result.AddTest(CreateQueryTestStringId("Get first ($top) - 10", null, 10));
            result.AddTest(CreateQueryTestStringId("Get last ($skip) - 10", null, null, ZumoQueryTestData.AllMovies.Length - 10));
            result.AddTest(CreateQueryTestStringId("Skip, take, includeTotalCount - movies 11-20, ordered by title",
                null, 10, 10, new[] { new OrderByClause("Title", true) }, null, true));
            result.AddTest(CreateQueryTestStringId("Skip, take, filter includeTotalCount - movies 11-20 which won a best picture award, ordered by year",
                m => m.BestPictureWinner == true, 10, 10, new[] { new OrderByClause("Year", false) }, null, true));

            // Order by
            result.AddTest(CreateQueryTestIntId("Order by date and string - 50 movies, ordered by release date, then title",
                null, 50, null, new[] { new OrderByClause("ReleaseDate", false), new OrderByClause("Title", true) }));
            result.AddTest(CreateQueryTestIntId("Order by number - 30 shortest movies since 1970",
                m => m.Year >= 1970, 30, null, new[] { new OrderByClause("Duration", true), new OrderByClause("Title", true) }, null, true));

            result.AddTest(CreateQueryTestStringId("Order by date and string - 50 movies, ordered by release date, then title",
                null, 50, null, new[] { new OrderByClause("ReleaseDate", false), new OrderByClause("Title", true) }));
            result.AddTest(CreateQueryTestStringId("Order by number - 30 shortest movies since 1970",
                m => m.Year >= 1970, 30, null, new[] { new OrderByClause("Duration", true), new OrderByClause("Title", true) }, null, true));

            // Select
            result.AddTest(CreateQueryTestIntId("Select one field - Only title of movies from 2008",
                m => m.Year == 2008, null, null, null, m => m.Title));
            result.AddTest(CreateQueryTestIntId("Select multiple fields - Nicely formatted list of movies from the 2000's",
                m => m.Year >= 2000, 200, null, new[] { new OrderByClause("ReleaseDate", false), new OrderByClause("Title", true) },
                m => string.Format("{0} {1} - {2} minutes", m.Title.PadRight(30), m.BestPictureWinner ? "(best picture)" : "", m.Duration)));

            result.AddTest(CreateQueryTestStringId("Select one field - Only title of movies from 2008",
                m => m.Year == 2008, null, null, null, m => m.Title));
            result.AddTest(CreateQueryTestStringId("Select multiple fields - Nicely formatted list of movies from the 2000's",
                m => m.Year >= 2000, 200, null, new[] { new OrderByClause("ReleaseDate", false), new OrderByClause("Title", true) },
                m => string.Format("{0} {1} - {2} minutes", m.Title.PadRight(30), m.BestPictureWinner ? "(best picture)" : "", m.Duration)));

            // Tests passing the OData query directly to the Read operation
            result.AddTest(CreateQueryTestIntId("Passing OData query directly - movies from the 80's, ordered by Title, items 3, 4 and 5",
                whereClause: m => m.Year >= 1980 && m.Year <= 1989,
                top: 3, skip: 2,
                orderBy: new OrderByClause[] { new OrderByClause("Title", true) },
                odataQueryExpression: "$filter=((Year ge 1980) and (Year le 1989))&$top=3&$skip=2&$orderby=Title asc"));

            result.AddTest(CreateQueryTestStringId("Passing OData query directly - movies from the 80's, ordered by Title, items 3, 4 and 5",
                whereClause: m => m.Year >= 1980 && m.Year <= 1989,
                top: 3, skip: 2,
                orderBy: new OrderByClause[] { new OrderByClause("Title", true) },
                odataQueryExpression: "$filter=((Year ge 1980) and (Year le 1989))&$top=3&$skip=2&$orderby=Title asc"));

            // Negative tests
            result.AddTest(CreateQueryTest<Movie, MobileServiceInvalidOperationException>("[Int id] (Neg) Very large top value", m => m.Year > 2000, 1001));
            result.AddTest(CreateQueryTest<StringIdMovie, MobileServiceInvalidOperationException>("[String id] (Neg) Very large top value", m => m.Year > 2000, 1001));
            result.AddTest(CreateQueryTest<Movie, NotSupportedException>("[Int id] (Neg) Unsupported predicate: unsupported arithmetic",
                m => Math.Sqrt(m.Year) > 43));
            result.AddTest(CreateQueryTest<StringIdMovie, NotSupportedException>("[String id] (Neg) Unsupported predicate: unsupported arithmetic",
                m => Math.Sqrt(m.Year) > 43));

            // Invalid lookup
            for (int i = -1; i <= 0; i++)
            {
                int id = i;
                result.AddTest(new ZumoTest("(Neg) Invalid id for lookup: " + i, async delegate(ZumoTest test)
                {
                    var table = ZumoTestGlobals.Instance.Client.GetTable<Movie>();
                    try
                    {
                        var item = await table.LookupAsync(id);
                        test.AddLog("Error, LookupAsync for id = {0} should have failed, but succeeded: {1}", id, item);
                        return false;
                    }
                    catch (InvalidOperationException ex)
                    {
                        test.AddLog("Caught expected exception - {0}: {1}", ex.GetType().FullName, ex.Message);
                        return true;
                    }
                }));
            }

#if !WINDOWS_PHONE
            var statusTest = ZumoTestCommon.CreateTestWithSingleAlert("The next test will show a dialog with certain movies. Please validate that movie titles and release years are shown correctly in the list.");
            statusTest.CanRunUnattended = false;
            result.AddTest(statusTest);
            result.AddTest(new ZumoTest("ToCollection - displaying movies on a ListBox", async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var table = client.GetTable<Movie>();
                var query = from m in table
                            where m.Year > 1980
                            orderby m.ReleaseDate descending
                            select new
                            {
                                Date = m.ReleaseDate.ToUniversalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                                Title = m.Title
                            };
                var newPage = new MoviesDisplayControl();
                var collection = await query.ToCollectionAsync();
                newPage.SetMoviesSource(collection);
                if (ZumoTestGlobals.ShowAlerts)
                {
                    await newPage.Display();
                }
                return true;
            })
            {
                CanRunUnattended = false
            });
            var validationTest = ZumoTestCommon.CreateYesNoTest("Were the movies displayed correctly?", true);
            validationTest.CanRunUnattended = false;
            result.AddTest(validationTest);
#endif
            return result;
        }

        internal static ZumoTest CreatePopulateTableTest()
        {
            return new ZumoTest("Populate movies table, if necessary", new TestExecution(async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var table = client.GetTable<AllMovies>();
                AllMovies allMovies = new AllMovies
                {
                    Movies = ZumoQueryTestData.AllMovies
                };
                await table.InsertAsync(allMovies);
                test.AddLog("Result of populating table: {0}", allMovies.Status);
                return true;
            }));
        }

        internal static ZumoTest CreatePopulateStringIdTableTest()
        {
            return new ZumoTest("Populate [string id] movies table, if necessary", new TestExecution(async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var table = client.GetTable<AllStringIdMovies>();
                AllStringIdMovies allMovies = new AllStringIdMovies
                {
                    Movies = new StringIdMovie[ZumoQueryTestData.AllMovies.Length]
                };
                for (int i = 0; i < allMovies.Movies.Length; i++)
                {
                    allMovies.Movies[i] = new StringIdMovie(string.Format("Movie {0:000}", i), ZumoQueryTestData.AllMovies[i]);
                }

                await table.InsertAsync(allMovies);
                test.AddLog("Result of populating table: {0}", allMovies.Status);
                return true;
            }));
        }

        class OrderByClause
        {
            public OrderByClause(string fieldName, bool isAscending)
            {
                this.FieldName = fieldName;
                this.IsAscending = isAscending;
            }

            public bool IsAscending { get; private set; }
            public string FieldName { get; private set; }
        }

        private static ZumoTest CreateQueryTestIntId(
            string name, Expression<Func<Movie, bool>> whereClause,
            int? top = null, int? skip = null, OrderByClause[] orderBy = null,
            Expression<Func<Movie, string>> selectExpression = null, bool? includeTotalCount = null,
            string odataQueryExpression = null)
        {
            return CreateQueryTest<Movie, ExceptionTypeWhichWillNeverBeThrown>(
                "[Int id] " + name, whereClause, top, skip, orderBy, selectExpression, includeTotalCount, odataQueryExpression, false);
        }

        private static ZumoTest CreateQueryTestStringId(
            string name, Expression<Func<StringIdMovie, bool>> whereClause,
            int? top = null, int? skip = null, OrderByClause[] orderBy = null,
            Expression<Func<StringIdMovie, string>> selectExpression = null, bool? includeTotalCount = null,
            string odataQueryExpression = null)
        {
            return CreateQueryTest<StringIdMovie, ExceptionTypeWhichWillNeverBeThrown>(
                "[String id] " + name, whereClause, top, skip, orderBy, selectExpression, includeTotalCount, odataQueryExpression, true);
        }

        private static ZumoTest CreateQueryTest<MovieType>(
            string name, Expression<Func<MovieType, bool>> whereClause,
            int? top = null, int? skip = null, OrderByClause[] orderBy = null,
            Expression<Func<MovieType, string>> selectExpression = null, bool? includeTotalCount = null,
            string odataQueryExpression = null, bool useStringIdTable = false) where MovieType : class, IMovie
        {
            return CreateQueryTest<MovieType, ExceptionTypeWhichWillNeverBeThrown>(name, whereClause, top, skip, orderBy, selectExpression, includeTotalCount, odataQueryExpression);
        }

        private static ZumoTest CreateQueryTest<MovieType, TExpectedException>(
            string name, Expression<Func<MovieType, bool>> whereClause,
            int? top = null, int? skip = null, OrderByClause[] orderBy = null,
            Expression<Func<MovieType, string>> selectExpression = null, bool? includeTotalCount = null,
            string odataExpression = null, bool useStringIdTable = false) where MovieType : class, IMovie
            where TExpectedException : Exception
        {
            return new ZumoTest(name, async delegate(ZumoTest test)
            {
                try
                {
                    var table = ZumoTestGlobals.Instance.Client.GetTable<MovieType>();
                    IEnumerable<MovieType> readMovies = null;
                    IEnumerable<string> readProjectedMovies = null;

                    if (odataExpression == null)
                    {
                        IMobileServiceTableQuery<MovieType> query = null;
                        IMobileServiceTableQuery<string> selectedQuery = null;

                        if (whereClause != null)
                        {
                            query = table.Where(whereClause);
                        }

                        if (orderBy != null)
                        {
                            if (query == null)
                            {
                                query = table.Where(m => m.Duration == m.Duration);
                            }

                            query = ApplyOrdering(query, orderBy);
                        }

                        if (top.HasValue)
                        {
                            query = query == null ? table.Take(top.Value) : query.Take(top.Value);
                        }

                        if (skip.HasValue)
                        {
                            query = query == null ? table.Skip(skip.Value) : query.Skip(skip.Value);
                        }

                        if (selectExpression != null)
                        {
                            selectedQuery = query == null ? table.Select(selectExpression) : query.Select(selectExpression);
                        }

                        if (includeTotalCount.HasValue)
                        {
                            query = query.IncludeTotalCount();
                        }

                        if (selectedQuery == null)
                        {
                            // Both ways of querying should be equivalent, so using both with equal probability here.
                            var tickCount = Environment.TickCount;
                            if ((tickCount % 2) == 0)
                            {
                                test.AddLog("Querying using MobileServiceTableQuery<T>.ToEnumerableAsync");
                                readMovies = await query.ToEnumerableAsync();
                            }
                            else
                            {
                                test.AddLog("Querying using IMobileServiceTable<T>.ReadAsync(MobileServiceTableQuery<U>)");
                                readMovies = await table.ReadAsync(query);
                            }
                        }
                        else
                        {
                            readProjectedMovies = await selectedQuery.ToEnumerableAsync();
                        }
                    }
                    else
                    {
                        test.AddLog("Using the OData query directly");
                        JToken result = await table.ReadAsync(odataExpression);
                        readMovies = result.ToObject<IEnumerable<MovieType>>();
                    }

                    long actualTotalCount = -1;
                    ITotalCountProvider totalCountProvider = (readMovies as ITotalCountProvider) ?? (readProjectedMovies as ITotalCountProvider);
                    if (totalCountProvider != null)
                    {
                        actualTotalCount = totalCountProvider.TotalCount;
                    }

                    IEnumerable<MovieType> expectedData;
                    if (useStringIdTable)
                    {
                        var movies = ZumoQueryTestData.AllStringIdMovies();
                        expectedData = new MovieType[movies.Length];
                        for (var i = 0; i < movies.Length; i++)
                        {
                            ((MovieType[])expectedData)[i] = (MovieType)(IMovie)movies[i];
                        }
                    }
                    else
                    {
                        expectedData = ZumoQueryTestData.AllMovies.Select(s => (MovieType)(IMovie)s);
                    }

                    if (whereClause != null)
                    {
                        expectedData = expectedData.Where(whereClause.Compile());
                    }

                    long expectedTotalCount = -1;
                    if (includeTotalCount.HasValue && includeTotalCount.Value)
                    {
                        expectedTotalCount = expectedData.Count();
                    }

                    if (orderBy != null)
                    {
                        expectedData = ApplyOrdering(expectedData, orderBy);
                    }

                    if (skip.HasValue)
                    {
                        expectedData = expectedData.Skip(skip.Value);
                    }

                    if (top.HasValue)
                    {
                        expectedData = expectedData.Take(top.Value);
                    }


                    if (includeTotalCount.HasValue)
                    {
                        if (expectedTotalCount != actualTotalCount)
                        {
                            test.AddLog("Total count was requested, but the returned value is incorrect: expected={0}, actual={1}", expectedTotalCount, actualTotalCount);
                            return false;
                        }
                    }

                    List<string> errors = new List<string>();
                    bool expectedDataIsSameAsReadData;

                    if (selectExpression != null)
                    {
                        string[] expectedProjectedData = expectedData.Select(selectExpression.Compile()).ToArray();
                        expectedDataIsSameAsReadData = Util.CompareArrays(expectedProjectedData, readProjectedMovies.ToArray(), errors);
                    }
                    else
                    {
                        expectedDataIsSameAsReadData = Util.CompareArrays(expectedData.ToArray(), readMovies.ToArray(), errors);
                    }

                    if (!expectedDataIsSameAsReadData)
                    {
                        foreach (var error in errors)
                        {
                            test.AddLog(error);
                        }

                        test.AddLog("Expected data is different");
                        return false;
                    }
                    else
                    {
                        if (typeof(TExpectedException) == typeof(ExceptionTypeWhichWillNeverBeThrown))
                        {
                            return true;
                        }
                        else
                        {
                            test.AddLog("Error, test should have failed with {0}, but succeeded.", typeof(TExpectedException).FullName);
                            return false;
                        }
                    }
                }
                catch (TExpectedException ex)
                {
                    test.AddLog("Caught expected exception - {0}: {1}", ex.GetType().FullName, ex.Message);
                    return true;
                }
            });
        }

        private static IMobileServiceTableQuery<MovieType> ApplyOrdering<MovieType>(IMobileServiceTableQuery<MovieType> query, OrderByClause[] orderBy)
            where MovieType : class, IMovie
        {
            if (orderBy.Length == 1)
            {
                if (orderBy[0].IsAscending && orderBy[0].FieldName == "Title")
                {
                    return query.OrderBy(m => m.Title);
                }
                else if (!orderBy[0].IsAscending && orderBy[0].FieldName == "Year")
                {
                    return query.OrderByDescending(m => m.Year);
                }
            }
            else if (orderBy.Length == 2)
            {
                if (orderBy[1].FieldName == "Title" && orderBy[1].IsAscending)
                {
                    if (orderBy[0].FieldName == "Duration" && orderBy[0].IsAscending)
                    {
                        return query.OrderBy(m => m.Duration).ThenBy(m => m.Title);
                    }
                    else if (orderBy[0].FieldName == "ReleaseDate" && !orderBy[0].IsAscending)
                    {
                        return query.OrderByDescending(m => m.ReleaseDate).ThenBy(m => m.Title);
                    }
                }
            }

            throw new NotImplementedException(string.Format("Ordering by [{0}] not implemented yet",
                string.Join(", ", orderBy.Select(c => string.Format("{0} {1}", c.FieldName, c.IsAscending ? "asc" : "desc")))));
        }

        private static IEnumerable<MovieType> ApplyOrdering<MovieType>(IEnumerable<MovieType> data, OrderByClause[] orderBy)
            where MovieType : class, IMovie
        {
            if (orderBy.Length == 1)
            {
                if (orderBy[0].IsAscending && orderBy[0].FieldName == "Title")
                {
                    return data.OrderBy(m => m.Title);
                }
                else if (!orderBy[0].IsAscending && orderBy[0].FieldName == "Year")
                {
                    return data.OrderByDescending(m => m.Year);
                }
            }
            else if (orderBy.Length == 2)
            {
                if (orderBy[1].FieldName == "Title" && orderBy[1].IsAscending)
                {
                    if (orderBy[0].FieldName == "Duration" && orderBy[0].IsAscending)
                    {
                        return data.OrderBy(m => m.Duration).ThenBy(m => m.Title);
                    }
                    else if (orderBy[0].FieldName == "ReleaseDate" && !orderBy[0].IsAscending)
                    {
                        return data.OrderByDescending(m => m.ReleaseDate).ThenBy(m => m.Title);
                    }
                }
            }

            throw new NotImplementedException(string.Format("Ordering by [{0}] not implemented yet",
                string.Join(", ", orderBy.Select(c => string.Format("{0} {1}", c.FieldName, c.IsAscending ? "asc" : "desc")))));
        }
    }
}
