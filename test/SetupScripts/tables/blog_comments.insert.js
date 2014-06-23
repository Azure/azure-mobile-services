function insert(item, user, request) {
    if (!item.postid) {
        request.respond(400, { error: 'comments must have a postid' });
        return;
    }

    request.execute({
        success: function () {
            mssql.query("update [zumo_app].[blog_posts] set commentcount = (select count(*) from [zumo_app].[blog_posts] where id = ?) where id = ?", [item.postid, item.postid], {
                    success: function () {
                        request.respond();                        
                    }
                }
            );
        }
    });
}
