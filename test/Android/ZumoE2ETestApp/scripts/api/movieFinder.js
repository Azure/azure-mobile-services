    exports.register = function(app) {
    app.get('/title/:title', getByTitle);
    app.get('/date/:year/:month/:day', getByDate);
    app.post('/moviesOnSameYear', fetchMoviesSameYear);
    app.post('/moviesWithSameDuration', fetchMoviesSameDuration);
}

function getByTitle(req, res) { getMovie(req, res, 'title', req.params.title); }
function getByDate(req, res) {
    var year = parseInt(req.params.year, 10);
    var month = parseInt(req.params.month, 10);
    var day = parseInt(req.params.day, 10);
    getMovie(req, res, 'releaseDate', new Date(Date.UTC(year, month - 1, day)));
}
function replyWithError(response) {
    return function(err) {
        console.log('Error: ', err);
        response.send(500, { error: err });
    }
}
function getMovie(req, res, field, value) {
    var table = req.service.tables.getTable('droidMovies');
    console.log('table: ', table);
    var filter = {};
    filter[field] = value;
    console.log('Field: ', field, ', value: ', value);
    table.where(filter).read({
        success: function(results) {
            res.send(200, { movies: results });
        }, error: replyWithError(res)
    });
}
function fetchMoviesSameYear(req, res) {
	console.log(req.body);
    console.log(req.query);
    var movie = req.body;
    var table = req.service.tables.getTable('droidMovies');
    var orderBy = req.query.orderBy || 'title';
    table.where({ year: movie.year }).orderBy(orderBy).read({
        success: function(results) {
            res.send(200, { movies: results });
        }, error: replyWithError(res)
    });
}
function fetchMoviesSameDuration(req, res) {
	console.log(req.body);
    console.log(req.query);
    var movie = req.body;
    var table = req.service.tables.getTable('droidMovies');
    var orderBy = req.query.orderBy || 'title';
    table.where({ duration: movie.duration }).orderBy(orderBy).read({
        success: function(results) {
            console.log(results);
            res.send(200, { movies: results });
        }, error: replyWithError(res)
    });
}
