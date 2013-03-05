function update(item, user, request) {
    if (item.complexType) {
        item.complexType = JSON.stringify(item.complexType);
    }

    request.execute({
        success: function() {
            if (item.complexType) {
                item.complexType = JSON.parse(item.complexType);
            }

            request.respond();
        }
    });
}
