function read(query, user, request) {
    var params = request.parameters;
    if (query.id) {
        request.respond(200, { id: query.id, parameters: JSON.stringify(params) });
    } else {
        request.respond(200, [{ id: 1, parameters: JSON.stringify(params) }]);
    }
}
