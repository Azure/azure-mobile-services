exports.get = function(request, response) {
    response.send(statusCodes.OK, {
        runtime : { type: 'node.js', version: process.version },
        features: {
            intIdTables: true,
            stringIdTables: true,
            nhPushEnabled: !!(request.service.push.send),
            queryExpandSupport: false,
            usersEnabled: /\"users\"/i.test(process.env.MS_PreviewFeatures),
            liveSDKLogin: true,
            azureActiveDirectoryLogin: true,
            singleSignOnLogin: true
        }
    });
};
