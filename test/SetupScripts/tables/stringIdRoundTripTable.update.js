function update(item, user, request) {
    if (item.complex) {
        item.complex = JSON.stringify(item.complex);
    }

    request.execute({
        success: function() {
            if (item.complex) {
                item.complex = JSON.parse(item.complex);
            }

            request.respond();
        }
    });
}
