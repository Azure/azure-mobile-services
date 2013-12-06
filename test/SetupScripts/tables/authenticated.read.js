function read(query, user, request) {    
    query.where({userId: user.userId});
    request.execute({
        success: addIdentityToResults.bind(null, request, user)
    });
}

function addIdentityToResults(request, user, results) {
    user.getIdentities({
        success: function(identities) {
            identities = JSON.stringify(identities);
            results.forEach(function(item) {
                item.Identities = identities;
                item.UsersEnabled = isUsersEnabled();
            });
            request.respond();
        }
   });    
}

function isUsersEnabled() {
    var enabled = /\"users\"/i.test(process.env.MS_PreviewFeatures);
    return enabled;
}