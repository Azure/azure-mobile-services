function del(id, user, request) {
    var params = request.parameters;
    request.respond(200, { id: id, parameters: JSON.stringify(params) });
}
