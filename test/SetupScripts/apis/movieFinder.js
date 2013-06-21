exports.register = function(app) {
    app.get('/title/:title', getByTitle);
    app.get('/date/:year/:month/:day', getByDate);
    app.post('moviesOnSameYear', fetchMoviesSameYear);
    app.post('moviesWithSameDuration', fetchMoviesSameDuration);
}

function getByTitle(req, res) { getMovie(req, res, 'Title', req.params.title); }
function getByDate(req, res) {
    var year = parseInt(req.params.year, 10);
    var month = parseInt(req.params.month, 10);
    var day = parseInt(req.params.day, 10);
    getMovie(req, res, 'ReleaseDate', new Date(Date.UTC(year, month - 1, day)));
}
function replyWithError(response) {
    return function(err) {
        console.log('Error: ', err);
        response.send(500, { error: err });
    }
}
function getMovie(req, res, field, value) {
    var table = req.service.tables.getTable('w8Movies');
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
    var movie = req.body;
    var table = req.service.tables.getTable('w8Movies');
    var orderBy = req.query.orderBy || 'Title';
    table.where({ Year: movie.Year }).orderBy(orderBy).read({
        success: function(results) {
            res.send(200, { movies: results });
        }, error: replyWithError(res)
    });
}
function fetchMoviesSameDuration(req, res) {
    var movie = req.body;
    var table = req.service.tables.getTable('w8Movies');
    var orderBy = req.query.orderBy || 'Title';
    table.where({ Duration: movie.Duration }).orderBy(orderBy).read({
        success: function(results) {
            res.send(200, { movies: results });
        }, error: replyWithError(res)
    });
}
