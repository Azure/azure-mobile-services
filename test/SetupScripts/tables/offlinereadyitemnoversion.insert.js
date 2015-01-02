function insert(item, user, request) {

    if (item.offlinereadyitems === undefined ) {
        console.log('No offlinereadyitems property');
        request.execute();
    } else {
        console.log('offlinereadyitems property');
        var table = tables.current;
        populateTable(table, request, item.offlinereadyitems);
    }
}

function populateTable(table, request, items) {
    var index = 0;
    var insertNext = function() {
        if (index < items.length) {
            var toInsert = items[index];
            table.insert(toInsert, {
                success: function() {
                    index++;
                    if ((index % 20) === 0) {
                        console.log('Inserted %d items', index);
                    }
                    
                    insertNext();
                }
            });
        }
        else if (index == items.length){
            request.respond(201, { id: 1, status: 'Table is populated' });
        }
    };
    
    insertNext();
}