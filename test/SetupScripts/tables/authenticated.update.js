function update(item, user, request) {
    tables.current.where({id: item.id}).read({
        success: function(results) {
            if (results.length) {
                var existingItem = results[0];
                if (existingItem.userId !== user.userId) {
                    request.respond(400, {error: 'Mismatching user id'});
                } else {
                    request.execute(); // it's ok, continue
                }
            } else {
                request.execute(); // default behavior, error due to 404
            }
        }
    });
}