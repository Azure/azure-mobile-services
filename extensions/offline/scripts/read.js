function read(query, user, request) {

    var timestamp = request.parameters.timestamp;
    
    var requestHasTimestamp = false;
    if(timestamp !== undefined)
    {
        requestHasTimestamp = true;
    }
    
    request.execute({
        success: function (results)
        {
            var response = {};
            if(results.totalCount !== undefined)
            {            
                response.count = results.totalCount;
            }
            
            // get latest timestamp
            mssql.query("SELECT @@DBTS", {
                success: function(result)
                {          
                    response.timestamp = result[0].Column0.toString('hex');
                    
                    var deleted = [];
                    var nondeleted = [];
                    
                    results.map(function(r) {
                        r.timestamp = r.timestamp.toString('hex');
                        var isDeleted = r.isDeleted;
                        delete r.isDeleted;
                        if(!requestHasTimestamp)
                        {
                            if(!isDeleted)
                            {
                                nondeleted.push(r);                                
                            }
                        }
                        else if(r.timestamp > timestamp)
                        {
                            if(!isDeleted)
                            {
                                nondeleted.push(r);                                
                            }
                            else
                            {
                                deleted.push(r.guid);
                            }
                        }
                    });
            
                    response.results = nondeleted;
                    response.deleted = deleted;
                    request.respond(200,response);
                }
            });
        }
    });
}