exports.post = function(request, response) {
    // Use "request.service" to access features of your mobile service, e.g.:
    //   var tables = request.service.tables;
    //   var push = request.service.push;

    response.send(statusCodes.OK, { message : 'Hello World!' });
};

exports.get = function(request, response) {
    //var nhPushEnabled= request.service.push.enableExternalPush;    
    var nhPushEnabled= true;
    var  features;
    if(nhPushEnabled)
    {
        features="nhPushEnabled";
    }
    response.send(statusCodes.OK, { message : features });
};