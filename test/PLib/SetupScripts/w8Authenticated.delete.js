function del(id, user, request) {
    var isAuth = false;
    if (request.parameters.userIsAuthenticated === 'true') {
        isAuth = true;
        if (user.level !== 'authenticated') {
            request.respond(statusCodes.INTERNAL_SERVER_ERROR, {
                error: 'user.level should be authenticated; user = ' + JSON.stringify(user)
            });
            return;
        }

        if (!user.userId) {
            request.respond(statusCodes.INTERNAL_SERVER_ERROR, {
                error: 'request is authenticated, but user.userId is not set; user = ' + JSON.stringify(user)
            });
            return;
        }
    }

    if (request.parameters.userIsAuthenticated && !isAuth) {
        if (user.level !== 'anonymous') {
            request.respond(statusCodes.INTERNAL_SERVER_ERROR, {
                error: 'user.level should be anonymous; user = ' + JSON.stringify(user)
            });
            return;
        }
    }

    tables.getTable('w8Authenticated').where({id: id}).read({
        success: function(results) {
            if (results.length) {
                var existingItem = results[0];
                if (existingItem.userId !== user.userId) {
                    request.respond(400, {error: 'Mismatching user id'});
                } else {
                    request.execute(); // it's ok, continue
                }
            } else {
                request.execute(); // default behavior, error due to 404
            }
        }
    });
}
