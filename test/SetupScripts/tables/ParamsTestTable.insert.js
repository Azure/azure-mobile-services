function insert(item, user, request) {
    var params = request.parameters;
    request.respond(201, { id: 1, parameters: JSON.stringify(params) });
}
