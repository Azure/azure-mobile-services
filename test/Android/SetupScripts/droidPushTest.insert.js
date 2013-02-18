function insert(item, user, request) {
    var method = item.method;
    if (!method) {
        request.respond(400, { error: 'request must have a \'method\' member' });
        return;
    }
    
    var registrationId = item.registrationId;
    if (!registrationId) {
        request.respond(400, { error: 'request must have a \'registrationId\' member' });
        return;
    }
    
    var payload = item.payload;
    if (!payload) {
        request.respond(400, { error: 'request must have a \'payload\' member' });
        return;
    }

    if (!push.gcm[method]) {
        request.respond(400, { error: 'Invalid method \'' + method + '\' - unsupported' });
        return;
    }
    
    console.log('item: ', item);
    push.gcm[method](registrationId, payload, function(error, result) {
        if (error) {
            console.log('Error sending GCM push: ', error);
            request.respond(500, { error: error });            
        } else {
            console.log('GCM push sent successfully: ', result);
            request.respond(200, { result: result });
        }
    });

}

