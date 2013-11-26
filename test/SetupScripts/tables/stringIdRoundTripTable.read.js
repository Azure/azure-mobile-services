function read(query, user, request) {
    request.execute({
        success: function(results) {
            results.forEach(function(item) {
                if (item.complex) {
                    item.complex = JSON.parse(item.complex);
                }
            });

            request.respond();
        }
    });
}
