function read(query, user, request) {
    request.execute({
        success: function(results) {
            results.forEach(function(item) {
                if (item.complexType1) {
                    item.complexType1 = JSON.parse(item.complexType1);
                }

                if (item.complexType2) {
                    item.complexType2 = JSON.parse(item.complexType2);
                }
            });

            request.respond();
        }
    });
}
