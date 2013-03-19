function insert(item, user, request) {
    var method = item.method;
    if (!method) {
        request.respond(400, { error: 'request must have a \'method\' member' });
        return;
    }
    
    var channelUri = item.channelUri;
    if (!channelUri) {
        request.respond(400, { error: 'request must have a \'channelUri\' member' });
        return;
    }

    var payload = item.payload;
    if (!payload) {
        request.respond(400, { error: 'request must have a \'payload\' member' });
        return;
    }

    var successFunction = function(pushResponse) {
        console.log('MPNS push sent: ', pushResponse);
        request.respond(201, { id: 1, response: pushResponse });
    };
    
    var errorFunction = function(pushError) {
        console.log('Error in MPNS push: ', pushError);
        request.respond(500, { error: pushError });
    }
    
    var options = {
        success: successFunction,
        error: errorFunction
    };
    
    var mpnsFunction = push.mpns[method];
    if (mpnsFunction) {
        mpnsFunction(channelUri, payload, options);
    } else {
        request.respond(400, { error: 'method not supported: ' + method });
    }
}