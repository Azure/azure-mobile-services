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
    
    if (method === 'sendToastText01') {
        push.wns.sendToastText01(channelUri, payload, options);
    } else if (method === 'sendToastText02') {
        push.wns.sendToastText02(channelUri, payload, options);
    } else if (method === 'sendToastText03') {
        push.wns.sendToastText03(channelUri, payload, options);
    } else if (method === 'sendToastText04') {
        push.wns.sendToastText04(channelUri, payload, options);
    } else if (method === 'sendToastImageAndText01') {
        push.wns.sendToastImageAndText01(channelUri, payload, options);
    } else if (method === 'sendToastImageAndText02') {
        push.wns.sendToastImageAndText02(channelUri, payload, options);
    } else if (method === 'sendToastImageAndText03') {
        push.wns.sendToastImageAndText03(channelUri, payload, options);
    } else if (method === 'sendToastImageAndText04') {
        push.wns.sendToastImageAndText04(channelUri, payload, options);
    } else {
        request.respond(400, { error: 'method not supported: ' + method });
    }
}
