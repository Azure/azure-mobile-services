var items = tables.getTable('items');
var listMembers = tables.getTable('listMembers');

function del(id, user, context) {

    // Find the item being deleted
    items.where({ id: id }).read({
        success: function (results) {
            if (results.length > 0) {
                var item = results[0];

                // Check if the user doing the deletion is a member
                // of the list that the item belongs to
                listMembers
                    .where({ userId: user.userId, listId: item.listId })
                    .read({
                        success: function (results) {
                            if (results.length > 0) {
                                // The user is a member, do the deletion
                                context.execute();
                            } else {
                                context.respond(400,
                                    'You cannot delete an item if you are not a member of the list containing that item.');
                            }
                        }
                    });
            } else {
                // They are trying to delete a non-existant item
                context.respond(404);
            }
        }
    });
}