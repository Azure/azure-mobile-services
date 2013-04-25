function update(item, user, request) {

    // item must have a guid
    if(item.guid === undefined)
    {
        request.respond(statusCodes.BAD_REQUEST,
        "item must have a guid");
        return;
    }
    //existing items must have a timestamp
    if(item.timestamp === undefined)
    {
        request.respond(statusCodes.BAD_REQUEST,
        "update operation must have timestamp");
        return;
    }
    //item cannot set isDeleted
    if(item.isDeleted !== undefined)
    {
        request.respond(statusCodes.BAD_REQUEST,
        "item cannot set isDeleted, isDeleted is a reserved column name");
        return;
    }
    
    var tableName = "todoitem";
    
    var sql = "SELECT * FROM " + tableName + " WHERE guid = ?";
    
    //updating happens here
    var processResult = function(result)
    {
        item = result;
        // you can't update a timestamp column
        delete item.timestamp;
        
        request.execute({
            success: function()
            {
                var response = {};
                
                // put in right group
                if(item.isDeleted)
                {       
                    response.results = [ ];     
                    response.deleted = [ item ];
                }
                else
                {
                    response.results = [ item ];
                    response.deleted = [ ];
                }
                
                //we dont want to send deletion information
                delete item.isDeleted;
                
                mssql.query("SELECT timestamp FROM " + tableName + " WHERE id = ?", [item.id], {
                    success: function(ts)
                    {
                        item.timestamp = ts[0].timestamp.toString('hex');
                        request.respond(statusCodes.OK, response);
                    }
                });
            }
        });        
    }
    
    mssql.query(sql, [item.guid], {
        success: function(results)
        {  
            if(results.length > 1)
            {
                request.respond(statusCodes.INTERNAL_SERVER_ERROR, 
                "multiple items were returned by the database for guid: " + item.guid);
            }
            if(results.length < 1)
            {
                request.respond(statusCodes.NOT_FOUND, 
                "an item with guid: " + item.guid + " was not found");
            }
            
            var result = results[0]
            //make hex string of timestamp
            result.timestamp = result.timestamp.toString('hex');
            
            if(result.timestamp == item.timestamp)
            { 
                processResult(item);
            }
            else if(result.timestamp > item.timestamp)
            {
                resolveConflict(result, item, processResult);
            }
            // client item is not known by the server
            else
            {
                request.respond(statusCodes.BAD_REQUEST);
            }  
        }
    });    

}

//handle conflicts
function resolveConflict(currentItem, newItem, resolvedCallback)
{
    if(currentItem.isDeleted)
    {
        resolvedCallback(currentItem);
    }
    //for now last write wins in other cases
    resolvedCallback(newItem);
}