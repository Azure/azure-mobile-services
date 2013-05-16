function read(query, user, request) {
    request.execute({
        success: function(results) {
            var identities = user.getIdentities();
            results.forEach(function(item) {
                item.Identities = identities;
            });
            request.respond();
        }
    });
}
