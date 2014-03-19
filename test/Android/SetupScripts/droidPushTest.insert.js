function insert(item, user, request) {
    var method = item.method;
    if (!method) {
        request.respond(400, { error: 'request must have a \'method\' member' });
        return;
    }
    
    var registrationId = item.registrationId;
    var tag = item.tag;
    
    var usingNH = item.usingNH;
    if (!usingNH) {
        if (!registrationId) {
            request.respond(400, { error: 'request must have a \'registrationId\' member' });
            return;
        }
    }
    
    var payload = item.payload;
    if (!payload) {
        request.respond(400, { error: 'request must have a \'payload\' member' });
        return;
    }
    
    var successFunction = function(pushResponse) {
        console.log('GCM push sent: ', pushResponse);
        request.respond(201, { id: 1, response: pushResponse });
    };
    
    var errorFunction = function(pushError) {
        console.log('Error in GCM push: ', pushError);
        request.respond(500, { error: pushError });
    };
    
    var options = {
        success: successFunction,
        error: errorFunction
    };
    
    if (usingNH) {
        if (item.templatePush) {
            push.send(tag, item.templateNotification, options);
        } else {
            push.gcm.send(tag, payload, options);
        }
    } else {
        var gcmFunction = push.gcm[method];
        if (gcmFunction) {
            gcmFunction(registrationId, payload, options);
        } else {
            request.respond(400, { error: 'method not supported: ' + method });
        }
    }
}