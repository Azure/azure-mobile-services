function update(item, user, request) {
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

    // Since this is a public update table, we want to prevent malicious
    // users from filling up our database, so we'll throttle the requests
    // for small payloads only
    if (JSON.stringify(item).length > 50) {
        request.respond(statusCodes.BAD_REQUEST, {error: 'Payload too large'});
    } else {
        request.execute();
    }
}
