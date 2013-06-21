// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSTableConnection.h"
#import "MSSerializer.h"


#pragma mark * MSTableConnection Implementation


@implementation MSTableConnection

@synthesize table = table_;


#pragma mark * Public Static Constructors


+(MSTableConnection *) connectionWithItemRequest:(MSTableItemRequest *)request
                                      completion:(MSItemBlock)completion
{
    // We'll use the conection in the response block below but won't set
    // it until the init at the end, so we need to use __block
    __block MSTableConnection *connection = nil;
    
    // Create an HTTP response block that will invoke the MSItemBlock
    MSResponseBlock responseCompletion = nil;
    
    if (completion) {
    
        responseCompletion = 
        ^(NSHTTPURLResponse *response, NSData *data, NSError *error)
        {
            id item = nil;
            
            if (!error) {
                
                [connection isSuccessfulResponse:response
                                        data:data
                                         orError:&error];
                if (!error)
                {
                    item = [connection itemFromData:data
                                           response:response
                                   ensureDictionary:YES
                                            orError:&error];
                }
            }
            
            [connection addRequestAndResponse:response toError:&error];
            completion(item, error);
            connection = nil;
        };
    }
    
    // Now create the connection with the MSResponseBlock
    connection = [[MSTableConnection alloc] initWithTableRequest:request
                                                      completion:responseCompletion];
    return connection;
}

+(MSTableConnection *) connectionWithDeleteRequest:(MSTableDeleteRequest *)request
                                        completion:(MSDeleteBlock)completion
{
    // We'll use the conection in the response block below but won't set
    // it until the init at the end, so we need to use __block
    __block MSTableConnection *connection = nil;
    
    // Create an HTTP response block that will invoke the MSDeleteBlock
    MSResponseBlock responseCompletion = nil;
    
    if (completion) {
    
        responseCompletion =
        ^(NSHTTPURLResponse *response, NSData *data, NSError *error)
        {
            
            if (!error) {
                [connection isSuccessfulResponse:response
                                        data:data
                                         orError:&error];
            }
            
            if (error) {
                [connection addRequestAndResponse:response toError:&error];
                completion(nil, error);
            }
            else {
                completion(request.itemId, nil);
            }
            connection = nil;
        };
    }
    
    // Now create the connection with the MSResponseBlock
    connection = [[MSTableConnection alloc] initWithTableRequest:request
                                                      completion:responseCompletion];
    return connection;

}
                                      
+(MSTableConnection *) connectionWithReadRequest:(MSTableReadQueryRequest *)request
                                      completion:(MSReadQueryBlock)completion
{
    // We'll use the conection in the response block below but won't set
    // it until the init at the end, so we need to use __block
    __block MSTableConnection *connection = nil;
    
    // Create an HTTP response block that will invoke the MSReadQueryBlock
    MSResponseBlock responseCompletion = nil;
    
    if (completion) {
    
        responseCompletion =
        ^(NSHTTPURLResponse *response, NSData *data, NSError *error)
        {
            NSArray *items = nil;
            NSInteger totalCount = -1;
            
            if (!error) {
                
                [connection isSuccessfulResponse:response
                                        data:data
                                         orError:&error];
                if (!error) {
                    totalCount = [connection items:&items
                                          fromData:data
                                      withResponse:response
                                           orError:&error];
                }
            }
            
            [connection addRequestAndResponse:response toError:&error];
            completion(items, totalCount, error);
            connection = nil;
        };
    }
    
    // Now create the connection with the MSSuccessBlock
    connection = [[MSTableConnection alloc] initWithTableRequest:request
                                                      completion:responseCompletion];
    return connection;
}


# pragma mark * Private Init Methods


-(id) initWithTableRequest:(MSTableRequest *)request
                 completion:(MSResponseBlock)completion
{
    self = [super initWithRequest:request
                       client:request.table.client
                        completion:completion];
    
    if (self) {
        table_ = request.table;
    }
    
    return self;
}


# pragma mark * Private Methods


-(NSInteger) items:(NSArray **)items
                fromData:(NSData *)data
                withResponse:(NSHTTPURLResponse *)response
                orError:(NSError **)error
{
    // Try to deserialize the data
    NSInteger totalCount = [self.client.serializer totalCountAndItems:items
                                                             fromData:data
                                                              orError:error];
    
    // If there was an error, add the request and response
    if (error && *error) {
        [self addRequestAndResponse:response toError:error];
    }
    
    return totalCount;
}


@end
