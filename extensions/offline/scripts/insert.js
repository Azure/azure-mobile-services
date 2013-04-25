function insert(item, user, request) {

    // item must have a guid
    if(!item.guid)
    {
        request.respond(statusCodes.BAD_REQUEST,
        "item must have a guid");
        return;
    }
    //new items cannot have a timestamp
    if(item.timestamp)
    {
        request.respond(statusCodes.BAD_REQUEST,
        "insert operation cannot have timestamp");
        return;
    }
    //item cannot set isDeleted
    if(item.isDeleted)
    {
        request.respond(statusCodes.BAD_REQUEST,
        "item cannot set isDeleted, isDeleted is a reserved column name");
        return;
    }
    
    var tableName = "todoitem";
    
    // an inserted item cannot be deleted
    item.isDeleted = false;
    
    request.execute({
        success: function ()
        {
            var result = item;
            var response = {};
            
            // copy totalcount over
            if(result.totalCount !== undefined)
            {            
                response.count = result.totalCount;
            }            
            
            //we dont want to send deletion information
            delete result.isDeleted;
            response.results = [ result ];
            response.deleted = [];
            
            mssql.query("SELECT timestamp FROM " + tableName + " WHERE id = ?", [item.id], {
                success: function(ts)
                {
                    result.timestamp = ts[0].timestamp.toString('hex');
                    request.respond(statusCodes.OK, response);
                }
            });
        }
    });

}