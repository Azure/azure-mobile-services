function update(item, user, request) {
    if (item.complex) {
        item.complex = JSON.stringify(item.complex);
    }

    request.execute({
        success: function() {
            if (item.complex) {
                item.complex = JSON.parse(item.complex);
            }

            request.respond();
        }, conflict: function(serverItem) {
            var policy = request.parameters.conflictPolicy || "";
            policy = policy.toLowerCase();
            if (policy === 'client' || policy === 'clientwins') {
                // item.__version has been updated with server version
                request.execute();
            } else if (policy === 'server' || policy === 'serverwins') {
                request.respond(statusCodes.OK, serverItem);
            } else {
                if (serverItem.complex) {
                    serverItem.complex = JSON.parse(serverItem.complex);
                }
                
                request.respond(statusCodes.PRECONDITION_FAILED, serverItem);
            }
        }
    });
}
