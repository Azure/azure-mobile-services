function insert(item, user, request) {
    var method = item.method;
    if (!method) {
        request.respond(400, { error: 'request must have a \'method\' member' });
        return;
    }
    
    if (method === 'send') {
        var token = item.token;
        var payload = item.payload;
        var delay = item.delay || 0;
        if (!payload || !token) {
            request.respond(400, { error: 'request must have a \'payload\' and a \'token\' members for sending push notifications.' });
        } else {
            var sendPush = function() {
                push.apns.send(token, payload, {
                    error: function(err) {
                        console.warn('Error sending push notification: ', err);
                    }
                });
            }
            if (delay) {
                setTimeout(sendPush, delay);
            } else {
                sendPush();
            }
            
            request.respond(201, { id: 1, status: 'Push sent' });
        }
    } else if (method === 'getFeedback') {
        push.apns.getFeedback({
            success: function(results) {
                request.respond(201, { id: 1, devices: results });
            }, error: function(err) {
                request.respond(500, { error: err });
            }
        });
    } else {
        request.respond(400, { error: 'valid values for the \'method\' parameter are \'send\' and \'getFeedback\'.' });
    }
}
