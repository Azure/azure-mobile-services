function update(item, user, request) {
    // Since this is a public update table, we want to prevent malicious
    // users from filling up our database, so we'll throttle the requests
    // for small payloads only
    if (JSON.stringify(item).length > 50) {
        request.respond(statusCodes.BAD_REQUEST, {error: 'Payload too large'});
    } else {
        request.execute();
    }
}
