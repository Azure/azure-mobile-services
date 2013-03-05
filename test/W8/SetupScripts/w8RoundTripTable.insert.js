function insert(item, user, request) {
    if (item.complexType1) {
        item.complexType1 = JSON.stringify(item.complexType1);
    }

    if (item.complexType2) {
        item.complexType2 = JSON.stringify(item.complexType2);
    }

    request.execute({
        success: function() {
            if (item.complexType1) {
                item.complexType1 = JSON.parse(item.complexType1);
            }

            if (item.complexType2) {
                item.complexType2 = JSON.parse(item.complexType2);
            }
            
            request.respond();
        }
    });
}
