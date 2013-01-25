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
        console.log('WNS push sent: ', pushResponse);
        request.respond(201, { id: 1, response: pushResponse });
    };
    
    var errorFunction = function(pushError) {
        console.log('Error in WNS push: ', pushError);
        request.respond(500, { error: pushError });
    }
    
    var options = {
        success: successFunction,
        error: errorFunction
    };
    
    var wnsFunction = push.wns[method];
    if (wnsFunction) {
        wnsFunction(channelUri, payload, options);
    } else {
        request.respond(400, { error: 'method not supported: ' + method });
    }
}