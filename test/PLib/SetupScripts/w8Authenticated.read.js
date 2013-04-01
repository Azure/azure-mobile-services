function read(query, user, request) {
    query.where({userId: user.userId});
    request.execute({
        success: function(results) {
            var identities = JSON.stringify(user.getIdentities());
            results.forEach(function(item) {
                item.Identities = identities;
            });
            request.respond();
        }
    });
}
