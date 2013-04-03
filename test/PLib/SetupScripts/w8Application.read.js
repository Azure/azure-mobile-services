function read(query, user, request) {
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

    request.execute();
}
