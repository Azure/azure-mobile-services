var listMembers = tables.getTable('listMembers');
var invites = tables.getTable('invites');
var devices = tables.getTable('devices');
var profiles = tables.getTable('profiles');

function insert(item, user, context) {
    if (item.fromUserId !== user.userId) {
        context.respond(400, 'You cannot pretend to be another user when you issue an invite');
        return;
    }
    if (item.toUserId === user.userId) {
        context.respond(400, 'You cannot invite yourself to lists');
        return;
    }

    // Check if the invitee is already a member of the list he is being invited to
    listMembers
        .where({ userId: item.toUserId, listId: item.listId })
        .read({ success: checkUserListMembership });

    function checkUserListMembership(results) {
        if (results.length > 0) {
            context.respond(400, 'The user is already a member of that list');
            return;
        }
        
        // The invitee is not a member of the list, but he may already have a
        // pending invite
        invites.where({ toUserId: item.toUserId, listId: item.listId })
            .read({ success: checkRedundantInvite });
    }

    function checkRedundantInvite(results) {
        if (results.length > 0) {
            context.respond(400, 'This user already has a pending invite to this list');
            return;
        }

        // The invitee is eligible to be invited to the list, but check if the user
        // sending the invite is a member of the list he is inviting someone to
       listMembers.where({ userId: user.userId, listId: item.listId })
            .read({ success: checkIsMemberOfList });
    }

    function checkIsMemberOfList(results) {
        if (results.length === 0) {
            context.respond(400, 'You have to be a member of a list to invite another user to that list.');
            return;
        }

        // Everything checks out, process the invite
        item.approved = false;
        context.execute({ 
            success: function(results) {
                context.respond();
                getProfile(results);
            }  
        });
    }
    
    function getProfile(results) {
        profiles.where({ userId : user.userId }).read({
            success: function(profileResults) {
                sendNotifications(results, profileResults[0]);
            }
        });
    }

    function sendNotifications(results, profile) {

        // Send push notifictions to all devices registered to 
        // the invitee
        devices.where({ userId: item.toUserId }).read({
            success: function (results) {
                results.forEach(function (device) {
                    push.wns.sendToastImageAndText01(device.channelUri, {
                        image1src: profile.imageUrl,
                        text1: 'You have been invited to list "' + item.listName +
                            '" by ' + item.fromUserName
                    }, {
                        succees: function(data) {
                            console.log(data);
                        },
                        error: function (err) {
                            // The notification address for this device has expired, so
                            // remove this device. This may happen routinely as part of
                            // how push notifications work.
                            if (err.statusCode === 403 || err.statusCode === 404) {
                                devices.del(device.id);
                            } else {
                                console.log("Problem sending push notification", err);
                            }
                        }
                    });
                }); 
             }
        });
    }
}

