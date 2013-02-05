/// <reference path="../testFramework.js" />

function defineQueryTestsNamespace() {
    var tests = [];
    var i;
    var tableName = 'w8jsMovies';

    tests.push(new zumo.Test('Populate table, if necessary', function (test, done) {
        test.addLog('Populating the table');
        var item = {
            movies: zumo.tests.getQueryTestData()
        };
        var client = zumo.getClient();
        var table = client.getTable(tableName);
        table.insert(item).done(function (readItem) {
            var status = readItem ? readItem.status : (item.status || 'no status');
            test.addLog('status: ' + JSON.stringify(status));
            done(true);
        }, function (err) {
            test.addLog('Error populating the table: ' + JSON.stringify(err));
            done(false);
        });
    }));

    tests.push(createQueryTest('GreaterThan and LessThen - Movies from the 90s', function (table) {
        return table.where(function () { return this.Year > 1989 && this.Year < 2000; });
    }, function (item) { return item.Year > 1989 && item.Year < 2000; }));
    tests.push(createQueryTest('GreaterEqual and LessEqual - Movies from the 90s', function (table) {
        return table.where(function () { return this.Year >= 1990 && this.Year <= 1999; });
    }, function (item) { return item.Year > 1989 && item.Year < 2000; }));
    tests.push(createQueryTest('Compound statement - OR of ANDs - Movies from the 30s and 50s', function (table) {
        return table.where(function () { return (this.Year >= 1930 && this.Year < 1940) || (this.Year >= 1950 && this.Year < 1960); });
    }, function (item) { return (item.Year >= 1930 && item.Year < 1940) || (item.Year >= 1950 && item.Year < 1960); }));
    tests.push(createQueryTest('Division, equal and different - Movies from the year 2000 with rating other than R', function (table) {
        return table.where(function () { return ((this.Year / 1000.0) == 2) && this.MPAARating != 'R'; });
    }, function (item) { return ((item.Year / 1000.0) == 2) && item.MPAARating != 'R'; }));
    tests.push(createQueryTest('Addition, subtraction, relational, AND - Movies from the 1980s which last less than 2 hours', function (table) {
        return table.where(function () { return ((this.Year - 1900) >= 80) && (this.Year + 10 < 2000) && (this.Duration < 120); });
    }, function (item) { return ((item.Year - 1900) >= 80) && (item.Year + 10 < 2000) && (item.Duration < 120); }));

    tests.push(createQueryTest('Query via template object - Movies from the year 2000 with rating R', function (table) {
        return table.where({Year: 2000, MPAARating: 'R'});
    }, function (item) { return item.Year == 2000 && item.MPAARating == 'R'; }));

    // String functions
    tests.push(createQueryTest('String.indexOf - Movies which start with \'The\'',
        function (table) { return table.where(function () { return this.Title.indexOf('The') === 0; }); },
        function (item) { return item.Title.indexOf('The') === 0; }, { top: 100 }));
    tests.push(createQueryTest('String.toUpperCase - Finding \'THE GODFATHER\'',
        function (table) { return table.where(function () { return this.Title.toUpperCase() === 'THE GODFATHER'; }); },
        function (item) { return item.Title.toUpperCase() === 'THE GODFATHER'; }));
    tests.push(createQueryTest('String.toLowerCase - Finding \'pulp fiction\'',
        function (table) { return table.where(function () { return this.Title.toLowerCase() === 'pulp fiction'; }); },
        function (item) { return item.Title.toLowerCase() === 'pulp fiction'; }));
    tests.push(createQueryTest('String.indexOf, String.toLowerCase - Movie which start with \'the\'',
        function (table) { return table.where(function () { return this.Title.toLowerCase().indexOf('the') === 0; }); },
        function (item) { return item.Title.toLowerCase().indexOf('the') === 0; }, { top: 100 }));
    tests.push(createQueryTest('String.indexOf, String.toUpperCase - Movie which start with \'THE\'',
        function (table) { return table.where(function () { return this.Title.toUpperCase().indexOf('THE') === 0; }); },
        function (item) { return item.Title.toUpperCase().indexOf('THE') === 0; }, { top: 100 }));
    for (i = 0; i < 2; i++) {
        var characters = (i + 1) * 6;
        tests.push(createQueryTest('String.length - Movie with small names (< ' + characters + ' characters)',
            function (table) { return table.where(function (numCharacters) { return this.Title.length <= numCharacters; }, characters); },
            function (item) { return item.Title.length <= characters; }, { top: 100 }));
    }
    tests.push(createQueryTest('String.indexOf (non-ASCII) - movies containing the \'é\' character',
        function (table) { return table.where(function () { return this.Title.indexOf('é') >= 0; }); },
        function (item) { return item.Title.indexOf('é') >= 0; }));
    tests.push(createQueryTest('String.replace, trim - movies starting with \'godfather\', after removing starting \'the\'',
        function (table) { return table.where(function () { return this.Title.replace('The', '').trim().indexOf('Godfather') === 0; }); },
        function (item) { return item.Title.replace('The', '').trim().indexOf('Godfather') === 0; }));
    tests.push(createQueryTest('String.substring, length - movies which end with \'r\'',
        function (table) { return table.where(function () { return this.Title.substring(this.Title.length - 1, 1) === 'r'; }); },
        function (item) { return /r$/.test(item.Title); }));
    tests.push(createQueryTest('String.substring (2 parameters) - movies with \'father\' starting at position 7',
        function (table) { return table.where(function () { return this.Title.substring(7, 13) === 'father'; }); },
        function (item) { var result = item.Title.substring(7, 13) === 'father'; return result; }, { debug: true }));
    tests.push(createQueryTest('String.substr - movies with \'father\' starting at position 7',
        function (table) { return table.where(function () { return this.Title.substr(7, 6) === 'father'; }); },
        function (item) { var result = item.Title.substr(7, 6) === 'father'; return result; }, { debug: true }));

    // String fields
    tests.push(createQueryTest('Equals - movies since 1980 with rating PG-13',
        function (table) { return table.where(function () { return this.MPAARating === 'PG-13' && this.Year >= 1980; }); },
        function (item) { return item.MPAARating == 'PG-13' && item.Year >= 1980; }, { top: 100 }));
    tests.push(createQueryTest('Comparison to null - Movies since 1980 without a MPAA rating',
        function (table) { return table.where(function () { return this.MPAARating == null && this.Year >= 1980; }); },
        function (item) { return item.MPAARating == null && item.Year >= 1980; }));
    tests.push(createQueryTest('Comparison to null (not null) - Movies before 1970 with a MPAA rating',
        function (table) { return table.where(function () { return this.MPAARating != null && this.Year < 1970; }); },
        function (item) { return item.MPAARating != null && item.Year < 1970; }));

    // Numeric functions
    tests.push(createQueryTest('Math.floor - Movies which last more than 3 hours',
        function (table) { return table.where(function () { return Math.floor(this.Duration / 60.0) >= 3; }); },
        function (item) { return Math.floor(item.Duration / 60.0) >= 3; }));
    tests.push(createQueryTest('Math.ceil - Movies which last at most 2 hours',
        function (table) { return table.where(function () { return Math.ceil(this.Duration / 60.0) == 2; }); },
        function (item) { return Math.ceil(item.Duration / 60.0) == 2; }, { top: 200 }));
    tests.push(createQueryTest('Math.round - Movies which last between 2.5 (inclusive) and 3.5 (exclusive) hours',
        function (table) { return table.where(function () { return Math.round(this.Duration / 60.0) == 3; }); },
        function (item) { return Math.round(item.Duration / 60.0) == 3; }));

    // Date fields
    var dateBefore1970 = new Date(Date.UTC(1969, 12 - 1, 31, 23, 59, 59));
    var date1980 = new Date(Date.UTC(1980, 1 - 1, 1));
    var dateBefore1990 = new Date(Date.UTC(1989, 12 - 1, 31, 23, 59, 59));
    var dateOfPulpFiction = new Date(Date.UTC(1994, 10 - 1, 14));
    tests.push(createQueryTest('Date: Greater than, less than - Movies with release date in the 70s',
        function (table) { return table.where(function (d1, d2) { return this.ReleaseDate > d1 && this.ReleaseDate < d2; }, dateBefore1970, date1980); },
        function (item) { return item.ReleaseDate > dateBefore1970 && item.ReleaseDate < date1980; }));
    tests.push(createQueryTest('Date: Greater or equal, less or equal - Movies with release date in the 80s',
        function (table) { return table.where(function (d1, d2) { return this.ReleaseDate >= d1 && this.ReleaseDate <= d2; }, date1980, dateBefore1990); },
        function (item) { return item.ReleaseDate >= date1980 && item.ReleaseDate <= dateBefore1990; }));
    tests.push(createQueryTest('Date: Equals - Movies released on 1994-10-14 (Shawshank Redemption / Pulp Fiction)',
        function (table) { return table.where({ ReleaseDate: dateOfPulpFiction }); },
        function (item) { return item.ReleaseDate.getTime() === dateOfPulpFiction.getTime(); }));

    // Date functions
    tests.push(createQueryTest('Date (month): Movies released in November',
        function (table) { return table.where(function () { return this.ReleaseDate.getMonth() == (11 - 1); }); },
        function (item) { return item.ReleaseDate.getMonth() == (11 - 1); }));
    tests.push(createQueryTest('Date (day): Movies released in the first day of the month',
        function (table) { return table.where(function () { return this.ReleaseDate.getDate() == 1; }); },
        function (item) { return item.ReleaseDate.getDate() == 1; }));
    tests.push(createQueryTest('Date (year): Movies whose year is different than its release year',
        function (table) { return table.where(function () { return this.ReleaseDate.getFullYear() !== this.Year; }); },
        function (item) { return item.ReleaseDate.getFullYear() !== item.Year; }, { debug: true, top: 100 }));

    // Boolean fields
    tests.push(createQueryTest('Boolean: equal to true - Best picture winners before 1950',
        function (table) { return table.where(function () { return this.BestPictureWinner === true && this.Year < 1950; }); },
        function (item) { return item.BestPictureWinner === true && item.Year < 1950; }, { debug: true }));
    tests.push(createQueryTest('Boolean: not and equal to false - Best picture winners since 2000',
        function (table) { return table.where(function () { return !(this.BestPictureWinner === false) && this.Year >= 2000; }); },
        function (item) { return !(item.BestPictureWinner === false) && item.Year >= 2000; }));
    tests.push(createQueryTest('Boolean: not equal to false - Best picture winners since 2000',
        function (table) { return table.where(function () { return this.BestPictureWinner !== false && this.Year >= 2000; }); },
        function (item) { return item.BestPictureWinner !== false && item.Year >= 2000; }));

    // In operator
    tests.push(createQueryTest('In operator: Movies since 2000 with MPAA ratings of \'G\', \'PG\' or \' PG-13\'',
        function (table) { return table.where(function (ratings) { return this.Year >= 2000 && this.MPAARating in ratings; }, ['G', 'PG', 'PG-13']); },
        function (item) { return item.Year >= 2000 && ['G', 'PG', 'PG-13'].indexOf(item.MPAARating) >= 0; }));

    // Top and skip
    tests.push(createQueryTest('Get all using large $top: take(500)', null, null, { top: 500 }));
    tests.push(createQueryTest('Skip all using large skip: skip(500)', null, null, { skip: 500 }));
    tests.push(createQueryTest('Skip, take and includeTotalCount: movies 11-20, ordered by title', null, null,
        { top: 10, skip: 10, includeTotalCount: true, sortFields: [new OrderByClause('Title', true)] }));
    tests.push(createQueryTest('Skip, take and includeTotalCount with filter: movies 11-20, which won the best picture, ordered by release date',
        function (table) { return table.where({ BestPictureWinner: true }); },
        function (item) { return item.BestPictureWinner; },
        { top: 10, skip: 10, includeTotalCount: true, sortFields: [new OrderByClause('ReleaseDate', false)] }));

    // Orderby
    tests.push(createQueryTest('Order by date and string - 50 movies, ordered by release date, then title', null, null,
        { top: 50, sortFields: [new OrderByClause('ReleaseDate', false), new OrderByClause('Title', true)] }));
    tests.push(createQueryTest('Order by number - 30 shortest movies since 1970', 
        function (table) { return table.where(function () { return this.Year >= 1970; }); },
        function (item) { return item.Year >= 1970; },
        { top: 30, sortFields: [new OrderByClause('Duration', true), new OrderByClause('Title', true)] }));

    // Select
    tests.push(createQueryTest('Select single field - Title of movies since 2000',
        function (table) { return table.where(function () { return this.Year >= 2000; }); },
        function (item) { return item.Year >= 2000; },
        { top: 200, selectFields: ['Title'] }));
    tests.push(createQueryTest('Select multiple fields - Title, BestPictureWinner, Duration, order by release date, of movies from the 1990s',
        function (table) { return table.where(function () { return this.Year >= 1990 && this.Year < 2000; }); },
        function (item) { return item.Year >= 1990 && item.Year < 2000; },
        {
            top: 200,
            selectFields: ['Title', 'BestPictureWinner', 'Duration'],
            sortFields: [new OrderByClause('ReleaseDate', true), new OrderByClause('Title', true)]
        }));

    // Negative tests
    tests.push(createQueryTest('(Neg) Unsupported predicate: lastIndexOf',
        function (table) { return table.where(function () { return this.Title.lastIndexOf('r') >= 0; }); },
        function (item) { item.lastIndexOf('r') == item.length - 1 }, { isNegativeClientValidation: true }));
    tests.push(createQueryTest('(Neg) Unsupported predicate: regular expression',
        function (table) { return table.where(function () { return /Godfather/.test(this.Title); }); },
        function (item) { return /Godfather/.test(item.Title); }, { isNegativeClientValidation: true }));
    tests.push(createQueryTest('(Neg) Unsupported predicate: function with multiple statements',
        function (table) { return table.where(function () { var year = 2000; return this.Year > year; }); },
        function (item) { var year = 2000; return item.Title > year; }, { isNegativeClientValidation: true }));
    var someVariable = 2000;
    tests.push(createQueryTest('(Neg) Unsupported predicate: using variables from closure',
        function (table) { return table.where(function () { return this.Year > someVariable; }); },
        function (item) { return item.Title > someVariable; }, { isNegativeClientValidation: true }));
    tests.push(createQueryTest('(Neg) Very large \'top\'', null, null, { top: 1001, isNegativeServerValidation: true }));
    
    for (i = -1; i <= 0; i++) {
        var id = i;
        tests.push(new zumo.Test('(Neg) invalid offset: ' + id, function (test, done) {
            var client = zumo.getClient();
            var table = client.getTable(tableName);
            table.lookup(id).done(function (item) {
                test.addLog('Error, lookup for id = ' + id + ' succeeded, it should have failed. Item = ' + JSON.stringify(item));
                done(false);
            }, function (err) {
                test.addLog('Got expected error: ' + JSON.stringify(err));
                done(true);
            });
        }));
    }

    function OrderByClause(fieldName, isAscending) {
        this.fieldName = fieldName;
        this.isAscending = isAscending;
    }

    OrderByClause.prototype.getSortFunction = function(a, b) {
        var field1 = a[this.fieldName];
        var field2 = b[this.fieldName];
        var result = 0;
        if (typeof field1 === 'string') {
            if (field1 < field2) {
                result = -1;
            } else if (field1 > field2) {
                result = 1;
            }
        } else if (typeof field1 === 'number') {
            result = field1 - field2;
        } else if (typeof field1 === 'object' && Object.prototype.toString.call(field1) == '[object Date]') {
            result = field1 - field2;
        } else {
            result = 0; // unsupported
        }

        if (!this.isAscending) {
            result = -result;
        }

        return result;
    }

    // options structure:
    // {
    //   top (number): number of elements to 'take'
    //   skip (number): number of elements to 'skip'
    //   includeTotalCount (boolean): whether to request a total could (wrapped response)
    //   sortFields (array of OrderByClause): fields to sort the data by
    //   selectFields (array of string): fields to retrieve
    //   debug (boolean): whether to log more information than normal
    //   isNegativeClientValidation (boolean): if an exception should be thrown before the request is sent the server
    //   isNegativeServerValidation (boolean): if the error callback should be returned by the server
    // }
    function createQueryTest(testName, getQueryFunction, filter, options) {
        return new zumo.Test(testName, function (test, done) {
            var i;
            var filteredMovies;
            var query;
            var exception = null;
            options = options || {};
            var top = options.top;
            var skip = options.skip;
            var sortFields = options.sortFields;
            var includeTotalCount = options.includeTotalCount;
            var selectFields = options.selectFields;
            var debug = options.debug;
            var isNegativeClientValidation = options.isNegativeClientValidation;
            var isNegativeServerValidation = options.isNegativeServerValidation;

            try {
                var client = zumo.getClient();
                var allMovies = zumo.tests.getQueryTestData();
                var table = client.getTable(tableName);

                if (getQueryFunction) {
                    query = getQueryFunction(table);
                } else {
                    query = table;
                }

                if (filter) {
                    filteredMovies = allMovies.filter(filter);
                } else {
                    filteredMovies = allMovies;
                }

                var expectedTotalCount = filteredMovies.length;

                if (includeTotalCount) {
                    query = query.includeTotalCount();
                }

                if (sortFields && sortFields.length) {
                    for (i = 0; i < sortFields.length; i++) {
                        if (sortFields[i].isAscending) {
                            query = query.orderBy(sortFields[i].fieldName);
                        } else {
                            query = query.orderByDescending(sortFields[i].fieldName);
                        }
                    }

                    var compareFunction = function (a, b) {
                        var i;
                        for (i = 0; i < sortFields.length; i++) {
                            var temp = sortFields[i].getSortFunction(a, b);
                            if (temp !== 0) {
                                return temp;
                            }
                        }

                        return 0;
                    }
                    filteredMovies = filteredMovies.sort(compareFunction);
                }

                if (skip) {
                    query = query.skip(skip);
                    filteredMovies = filteredMovies.slice(skip, 1000);
                }

                if (top) {
                    query = query.take(top);
                    filteredMovies = filteredMovies.slice(0, top);
                }

                if (selectFields && selectFields.length) {
                    query.select(selectFields.join(','));
                    filteredMovies = filteredMovies.map(function (obj) {
                        var result = {};
                        var key;
                        for (key in obj) {
                            if (obj.hasOwnProperty(key)) {
                                if (selectFields.indexOf(key) >= 0) {
                                    result[key] = obj[key];
                                }
                            }
                        }
                        return result;
                    });
                }
            } catch (ex) {
                exception = ex;
            }

            if (exception) {
                test.addLog('Exception: ' + JSON.stringify(exception));
                if (isNegativeClientValidation) {
                    test.addLog('Exception is expected.');
                    done(true);
                } else {
                    done(false);
                }
                return;
            } else {
                if (isNegativeClientValidation) {
                    test.addLog('Error, expected an exception before sending the \'read\' call, but none was thrown.');
                    done(false);
                    return;
                }
            }

            query.read().done(function (results) {
                test.addLog('Received results from the server');
                if (isNegativeServerValidation) {
                    test.addLog('Error, expected an error response from the server, but received results: ' + JSON.stringify(results));
                    done(false);
                    return;
                }
                if (debug) {
                    test.addLog('Results: ' + JSON.stringify(results));
                }
                var actualTotalCount;
                var actualResults;
                if (includeTotalCount) {
                    actualTotalCount = results.totalCount;
                    actualResults = results;
                    if (actualResults === undefined || actualTotalCount === undefined) {
                        test.addLog('Error, query with \'includeTotalCount()\' should return an object, it did not');
                        test.addLog('Result: ' + JSON.stringify(results));
                        done(false);
                        return;
                    }

                    if (expectedTotalCount !== actualTotalCount) {
                        test.addLog('Error in \'totalCount\'. Expected: ' + expectedTotalCount + ', actual: ' + actualTotalCount);
                        done(false);
                        return;
                    }
                } else {
                    actualTotalCount = -1;
                    actualResults = results;
                }

                var errors = [];
                if (!zumo.util.compare(filteredMovies, actualResults, errors)) {
                    errors.forEach(function (error) {
                        test.addLog(error);
                    });
                    test.addLog('Retrieved data is not the expected');
                    if (debug) {
                        test.addLog('Expected: ' + JSON.stringify(filteredMovies));
                        test.addLog('Actual: ' + JSON.stringify(actualResults));
                    }
                    done(false);
                } else {
                    done(true);
                }
            }, function (err) {
                test.addLog('Read called the error callback: ' + JSON.stringify(err));
                zumo.util.traceResponse(test, err.request);
                if (isNegativeServerValidation) {
                    test.addLog('Error was expected');
                    done(true);
                } else {
                    done(false);
                }
            });
        });
    }

    return {
        name: 'Query',
        tests: tests
    };
}

zumo.tests.query = defineQueryTestsNamespace();
