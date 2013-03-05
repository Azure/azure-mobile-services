function read(query, user, request) {
    request.execute({
        success: function(results) {
            results.forEach(function(item) {
                if (item.complexType) {
                    item.complexType = JSON.parse(item.complexType);
                }
            });

            request.respond();
        }
    });
}
