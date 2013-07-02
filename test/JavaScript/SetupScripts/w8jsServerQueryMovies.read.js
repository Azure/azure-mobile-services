function read(query, user, request) {
    var table = tables.getTable('w8jsMovies');
    var testName = request.parameters.testName;
    if (testName === 'getWhereClauseCreator') {
        var code = getWhereClauseCreator.toString();
        request.respond(200, [{ code: code }]);
        return;
    }
    try {
        var options = JSON.parse(request.parameters.options || '{}');
        var whereCreator = getWhereClauseCreator(testName);
        
        if (whereCreator) {
            var serverQuery = whereCreator(table);
            if (options.sortFields && options.sortFields.length) {
                options.sortFields.forEach(function(sortClause) {
                    if (sortClause.isAscending) {
                        serverQuery = serverQuery.orderBy(sortClause.fieldName);
                    } else {
                        serverQuery = serverQuery.orderByDescending(sortClause.fieldName);
                    }
                });
            }
            if (options.top) {
                serverQuery = serverQuery.take(options.top);
            }
            if (options.skip) {
                serverQuery = serverQuery.skip(options.skip);
            }
            if (options.selectFields && options.selectFields.length) {
                serverQuery = serverQuery.select(options.selectFields.join(','));
            }
            serverQuery.read({
                success: function(results) {
                    request.respond(200, results);
                }, error: function(err){
                    request.respond(500, { error: err });
                }
            });
        } else {
            request.respond(400, { error: 'Invalid test name: ' + testName });
        }
    } catch (ex) {
        console.log('Error: ', ex);
        request.respond(500, { error: ex });
    }
}

function getWhereClauseCreator(testName) {
    switch (testName) {
        case 'GreaterThan and LessThen - Movies from the 90s':
            return function (table) { return table.where(function () { return this.Year > 1989 && this.Year < 2000; }); };
        case 'GreaterEqual and LessEqual - Movies from the 90s':
            return function (table) { return table.where(function () { return this.Year >= 1990 && this.Year <= 1999; }); };
        case 'Compound statement - OR of ANDs - Movies from the 30s and 50s':
            return function (table) { return table.where(function () { return (this.Year >= 1930 && this.Year < 1940) || (this.Year >= 1950 && this.Year < 1960); }); };
        case 'Division, equal and different - Movies from the year 2000 with rating other than R':
            return function (table) { return table.where(function () { return ((this.Year / 1000.0) == 2) && this.MPAARating != 'R'; }); };
        case 'Addition, subtraction, relational, AND - Movies from the 1980s which last less than 2 hours':
            return function (table) { return table.where(function () { return ((this.Year - 1900) >= 80) && (this.Year + 10 < 2000) && (this.Duration < 120); }); };
        case 'Query via template object - Movies from the year 2000 with rating R':
            return function (table) { return table.where({ Year: 2000, MPAARating: 'R' }); };
        case 'String.indexOf - Movies which start with "The"':
            return function (table) { return table.where(function () { return this.Title.indexOf('The') === 0; }); };
        case 'String.toUpperCase - Finding "THE GODFATHER"':
            return function (table) { return table.where(function () { return this.Title.toUpperCase() === 'THE GODFATHER'; }); };
        case 'String.toLowerCase - Finding "pulp fiction"':
            return function (table) { return table.where(function () { return this.Title.toLowerCase() === 'pulp fiction'; }); };
        case 'String.indexOf, String.toLowerCase - Movie which start with "the"':
            return function (table) { return table.where(function () { return this.Title.toLowerCase().indexOf('the') === 0; }); };
        case 'String.indexOf, String.toUpperCase - Movie which start with "THE"':
            return function (table) { return table.where(function () { return this.Title.toUpperCase().indexOf('THE') === 0; }); };
        case 'String.indexOf (non-ASCII) - movies containing the "é" character':
            return function (table) { return table.where(function () { return this.Title.indexOf('é') >= 0; }); };
        case 'String.substring, length - movies which end with "r"':
            return function (table) { return table.where(function () { return this.Title.substring(this.Title.length - 1, 1) === 'r'; }); };
        case 'String.substring (2 parameters) - movies with "father" starting at position 7':
            return function (table) { return table.where(function () { return this.Title.substring(7, 13) === 'father'; }); };
        case 'String.substr - movies with "father" starting at position 7':
            return function (table) { return table.where(function () { return this.Title.substr(7, 6) === 'father'; }); };
        case 'Equals - movies since 1980 with rating PG-13':
            return function (table) { return table.where(function () { return this.MPAARating === 'PG-13' && this.Year >= 1980; }); };
        case 'Comparison to null - Movies since 1980 without a MPAA rating':
            return function (table) { return table.where(function () { return this.MPAARating === null && this.Year >= 1980; }); };
        case 'Comparison to null (not null) - Movies before 1970 with a MPAA rating':
            return function (table) { return table.where(function () { return this.MPAARating !== null && this.Year < 1970; }); };
        case 'Math.floor - Movies which last more than 3 hours':
            return function (table) { return table.where(function () { return Math.floor(this.Duration / 60.0) >= 3; }); };
        case 'Math.ceil - Movies which last at most 2 hours':
            return function (table) { return table.where(function () { return Math.ceil(this.Duration / 60.0) == 2; }); };
        case 'Math.round - Movies which last between 2.5 (inclusive) and 3.5 (exclusive) hours':
            return function (table) { return table.where(function () { return Math.round(this.Duration / 60.0) == 3; }); };
        case 'Date: Equals - Movies released on 1994-10-14 (Shawshank Redemption / Pulp Fiction)':
            return function (table) { return table.where({ ReleaseDate: new Date(Date.UTC(1994, 10 - 1, 14)) }); };
        case 'Date (month): Movies released in November':
            return function (table) { return table.where(function () { return this.ReleaseDate.getUTCMonth() == (11 - 1); }); };
        case 'Date (day): Movies released in the first day of the month':
            return function (table) { return table.where(function () { return this.ReleaseDate.getUTCDate() == 1; }); };
        case 'Date (year): Movies whose year is different than its release year':
            return function (table) { return table.where(function () { return this.ReleaseDate.getUTCFullYear() !== this.Year; }); };
        case 'Boolean: equal to true - Best picture winners before 1950':
            return function (table) { return table.where(function () { return this.BestPictureWinner === true && this.Year < 1950; }); };
        case 'Boolean: not and equal to false - Best picture winners since 2000':
            return function (table) { return table.where(function () { return !(this.BestPictureWinner === false) && this.Year >= 2000; }); };
        case 'Boolean: not equal to false - Best picture winners since 2000':
            return function (table) { return table.where(function () { return this.BestPictureWinner !== false && this.Year >= 2000; }); };
        case 'In operator: Movies since 2000 with MPAA ratings of "G", "PG" or " PG-13"':
            return function (table) { return table.where(function (ratings) { return this.Year >= 2000 && this.MPAARating in ratings; }, ['G', 'PG', 'PG-13']); };
        case 'Skip, take and includeTotalCount with filter: movies 11-20, which won the best picture, ordered by release date':
            return function (table) { return table.where({ BestPictureWinner: true }); };
        case 'Order by number - 30 shortest movies since 1970':
            return function (table) { return table.where(function () { return this.Year >= 1970; }); };
        case 'Select single field - Title of movies since 2000':
            return function (table) { return table.where(function () { return this.Year >= 2000; }); };
        case 'Select multiple fields - Title, BestPictureWinner, Duration, order by release date, of movies from the 1990s':
            return function (table) { return table.where(function () { return this.Year >= 1990 && this.Year < 2000; }); };
    }
    return null;
}
