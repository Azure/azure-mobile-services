function update(item, user, request) {
    var params = request.parameters;
    request.respond(200, { id: item.id, parameters: JSON.stringify(params) });
}
