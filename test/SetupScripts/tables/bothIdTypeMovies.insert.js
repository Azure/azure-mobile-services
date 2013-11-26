function insert(item, user, request) {
    item.id = 1;
    var table = tables.current;
    table.take(1).read({
        success: function(items) {
            if (items.length > 0) {
                // table already populated
                request.respond(201, {id: 1, status: 'Already populated'});
            } else {
                // Need to populate the table
                populateTable(table, request, item.movies);
            }
        }
    });
}

function populateTable(table, request, films) {
    var index = 0;
    films.forEach(changeReleaseDate);
    var insertNext = function() {
        if (index >= films.length) {
            request.respond(201, {id : 1, status : 'Table populated successfully'});
        } else {
            var toInsert = films[index];
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
    };
    
    insertNext();
}

function changeReleaseDate(obj) {
    var releaseDate = obj.ReleaseDate;
    if (typeof releaseDate === 'string') {
        releaseDate = new Date(releaseDate);
        obj.ReleaseDate = releaseDate;
    }
}
