function read(query, user, request) {
    console.log('blog comments read script called');

    // Only return comments for blog posts that have not been deleted.
    var table = tables.getTable("blog_posts");
    table.where({ showComments: true }).select(function () { return this.id; }).read({
        success: function (results) {
            query.where(function (posts) { return this.postid in posts; }, results);
            request.execute();
        },
        error: function (err) {
            console.error(err);
            request.respond();
        }
    });
}
