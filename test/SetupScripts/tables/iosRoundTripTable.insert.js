function insert(item, user, request) {
    var products = item.products;
    if (!products) {
        // Normal case, no scripts needed
        request.execute();
    } else {
        delete item.products;
        var productToString = function(prod) {
            var result = '{';
            var first = true;
            for (var key in prod) {
                if (prod.hasOwnProperty(key)) {
                    if (first) {
                        first = false;
                    } else {
                        result = result + ', ';
                    }
                    result = result + prod[key];
                }
            }

            result = result + '}';
            return result;
        }

        var strProducts = '[' + products.map(productToString).join(',') + ']';
        item.products = strProducts;

        request.execute({
            success: function() {
                item.products = products;
                var totalPrice = products.reduce(function(total, obj) {
                    return obj.price + total;
                }, 0);
                item.totalPrice = totalPrice;
                request.respond();
            }
        });
    }
}
