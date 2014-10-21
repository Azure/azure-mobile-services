// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSTableRequest.h"
#import "MSURLBuilder.h"


#pragma mark * HTTP Method String Constants


static NSString *const httpGet = @"GET";
static NSString *const httpPatch = @"PATCH";
static NSString *const httpPost = @"POST";
static NSString *const httpDelete = @"DELETE";


#pragma mark * MSTableRequest Private Interface


@interface MSTableRequest ()

// Public readonly and private readwrite properties 
@property (nonatomic, readwrite)             MSTableRequestType requestType;

// Private initalizer method
-(id) initWithURL:(NSURL *)url
        withTable:(MSTable *)table;

@end


#pragma mark * MSTableItemRequest Private Interface


@interface MSTableItemRequest ()

// Public readonly and private readwrite properties
@property (nonatomic, strong, readwrite)     id item;
@property (nonatomic, strong, readwrite)     id itemId;

@end


#pragma mark * MSTableDeleteRequest Private Interface


@interface MSTableDeleteRequest ()

// Public readonly and private readwrite properties
@property (nonatomic, strong, readwrite)     id item;
@property (nonatomic, strong, readwrite)     id itemId;

@end


#pragma mark * MSTableReadQueryRequest Private Interface


@interface MSTableReadQueryRequest ()

// Public readonly and private readwrite properties
@property (nonatomic, copy, readwrite)       NSString *queryString;

@end


#pragma mark * MSTableRequest Implementation


@implementation MSTableRequest

@synthesize requestType = requestType_;
@synthesize table = table_;


#pragma mark * Private Initializer Method


-(id) initWithURL:(NSURL *)url
            withTable:(MSTable *)table
{
    self = [super initWithURL:url];
    if (self) {
        table_ = table;
    }
    
    return self;
}


#pragma mark * Public Static Constructors


+(MSTableItemRequest *) requestToInsertItem:(id)item
                                      table:(MSTable *)table
                                 parameters:(NSDictionary *)parameters
                                 completion:(MSItemBlock)completion
{
    MSTableItemRequest *request = nil;
    NSError *error = nil;

    // Create the URL
    NSURL *url = [MSURLBuilder URLForTable:table
                                 parameters:parameters
                                    orError:&error];
    if (!error) {
        // Create the request
        request = [[MSTableItemRequest alloc] initWithURL:url
                                                withTable:table];
        
        // Create the body or capture the error from serialization
        NSData *data = [table.client.serializer dataFromItem:item
                                                   idAllowed:NO
                                            ensureDictionary:YES
                                      removeSystemProperties:NO
                                                     orError:&error];
        if (!error) {
            // Set the body
            request.HTTPBody = data;
            
            // Set the additionl properties
            request.requestType = MSTableInsertRequestType;
            request.item = item;
            
            // Set the method and headers
            request.HTTPMethod = httpPost;
        }
    }
    
    // If there was an error, call the completion and make sure
    // to return nil for the request
    if (error) {
        request = nil;
        if (completion) {
            completion(nil, error);
        }
    }
    
    return request;
}

+(MSTableItemRequest *) requestToUpdateItem:(id)item
                                      table:(MSTable *)table
                                 parameters:(NSDictionary *)parameters
                                 completion:(MSItemBlock)completion

{
    MSTableItemRequest *request = nil;
    NSError *error = nil;
    id<MSSerializer> serializer = table.client.serializer;
    
    id itemId = [serializer itemIdFromItem:item orError:&error];
    if (!error) {
        // Ensure we can get a string from the item Id
        NSString *idString = [serializer stringFromItemId:itemId
                                                  orError:&error];
        
        if (!error) {        

            // Create the URL
            NSURL *url = [MSURLBuilder URLForTable:table
                                       itemIdString:idString
                                         parameters:parameters
                                                orError:&error];
            if (!error) {
                // Create the request
                request = [[MSTableItemRequest alloc] initWithURL:url
                                                        withTable:table];
                request.itemId = itemId;
            
                NSString *version = [self getVersionFromItem:item itemId:itemId];
                
                // Create the body or capture the error from serialization
                NSData *data = [serializer dataFromItem:item
                                              idAllowed:YES
                                       ensureDictionary:YES
                                 removeSystemProperties:YES
                                                orError:&error];
                if (!error) {
                    // Set the body
                    request.HTTPBody = data;
                    
                    // Set the properties
                    request.requestType = MSTableUpdateRequestType;
                    request.item = item;
                    
                    // Set the method and headers
                    request.HTTPMethod = httpPatch;
                    
                    // Version becomes an etag if passed
                    if(version) {
                        [self setVersion:version request:request];
                    }
                }
            }
        }
    }
    
    // If there was an error, call the completion and make sure
    // to return nil for the request
    if (error) {
        request = nil;
        if (completion) {
            completion(nil, error);
        }
    }
    
    return request;
}

+(MSTableDeleteRequest *) requestToDeleteItem:(id)item
                                    table:(MSTable *)table
                               parameters:(NSDictionary *)parameters
                               completion:(MSDeleteBlock)completion
{
    MSTableDeleteRequest *request = nil;
    NSError *error = nil;
    
    // Ensure we can get the item Id
    id itemId = [table.client.serializer itemIdFromItem:item orError:&error];
    if (!error) {
        NSString *version = [self getVersionFromItem:item itemId:itemId];
        
        // Get the request from the other constructor
        request = [MSTableRequest requestToDeleteItemWithId:itemId
                                                      table:table
                                                 parameters:parameters
                                                 completion:completion];
        
        // Set the additional properties
        request.item = item;
        
        // Version becomes an etag if passed
        if(version) {
            [self setVersion:version request:request];
        }
    }
    
    // If there was an error, call the completion and make sure
    // to return nil for the request
    if (error) {
        request = nil;
        if (completion) {
            completion(nil, error);
        }
    }
    
    return request;
}

+(MSTableDeleteRequest *) requestToDeleteItemWithId:(id)itemId
                                              table:(MSTable *)table
                                         parameters:(NSDictionary *)parameters
                                         completion:(MSDeleteBlock)completion
{
    MSTableDeleteRequest *request = nil;
    NSError *error = nil;
    
    // Ensure we can get the id as a string
    NSString *idString = [table.client.serializer stringFromItemId:itemId
                                                           orError:&error];
    if (!error) {
    
        // Create the URL
        NSURL *url = [MSURLBuilder URLForTable:table
                                   itemIdString:idString
                                     parameters:parameters
                                            orError:&error];
        if (!error) {
            
            // Create the request
            request = [[MSTableDeleteRequest alloc] initWithURL:url
                                                      withTable:table];
            
            // Set the additional properties
            request.requestType = MSTableDeleteRequestType;
            request.itemId = itemId;
            
            // Set the method and headers
            request.HTTPMethod = httpDelete;
        }
    }
    
    // If there was an error, call the completion and make sure
    // to return nil for the request
    if (error) {
        request = nil;
        if (completion) {
            completion(nil, error);
        }
    }
    
    return request;
}

+(MSTableItemRequest *) requestToReadWithId:(id)itemId
                                      table:(MSTable *)table
                                 parameters:(NSDictionary *)parameters
                                 completion:(MSItemBlock)completion
{
    MSTableItemRequest *request = nil;
    NSError *error = nil;
    
    // Ensure we can get the id as a string
    NSString *idString = [table.client.serializer stringFromItemId:itemId
                                                           orError:&error];
    if (!error) {

        // Create the URL
        NSURL *url =  [MSURLBuilder URLForTable:table
                                    itemIdString:idString
                                      parameters:parameters
                                             orError:&error];
        if (!error) {
            
            // Create the request
            request = [[MSTableItemRequest alloc] initWithURL:url
                                                    withTable:table];
            
            // Set the additional properties
            request.requestType = MSTableReadRequestType;
            request.itemId = itemId;
            
            // Set the method and headers
            request.HTTPMethod = httpGet;
        }
    }
    
    // If there was an error, call the completion and make sure
    // to return nil for the request
    if (error) {
        request = nil;
        if (completion) {
            completion(nil, error);
        }
    }
    
    return request;
}

+(MSTableReadQueryRequest *) requestToReadItemsWithQuery:(NSString *)queryString
                                                   table:(MSTable *)table
                                              completion:(MSReadQueryBlock)completion
{
    MSTableReadQueryRequest *request = nil;
    
    // Create the URL
    NSURL *url = [MSURLBuilder URLForTable:table query:queryString];
    
    // Create the request
    request = [[MSTableReadQueryRequest alloc] initWithURL:url
                                                 withTable:table];
    
    // Set the additional properties
    request.requestType = MSTableReadQueryRequestType;
    request.queryString = queryString;
    
    // Set the method and headers
    request.HTTPMethod = httpGet;
    
    return request;
}

#pragma mark * Private Static Constructors

+ (NSString *)getVersionFromItem:(id)item itemId:(id)itemId
{
    // If string id, cache the version field as we strip it out during serialization
    NSString *version= nil;
    if([itemId isKindOfClass:[NSString class]]) {
        version = [item objectForKey:MSSystemColumnVersion];
    }
    return version;
}

+ (void)setVersion:(NSString *)version request:(MSTableRequest *)request
{
    version = [NSString stringWithFormat:@"\"%@\"", [version stringByReplacingOccurrencesOfString:@"\"" withString:@"\\\""]];
    [request addValue:version forHTTPHeaderField:@"If-Match"];
}

@end


#pragma mark * MSTableItemRequest Implementation


@implementation MSTableItemRequest

@synthesize itemId = itemId_;
@synthesize item = item_;

@end


#pragma mark * MSTableDeleteRequest Implementation


@implementation MSTableDeleteRequest

@synthesize itemId = itemId_;
@synthesize item = item_;

@end


#pragma mark * MSTableReadQueryRequest Implementation


@implementation MSTableReadQueryRequest

@synthesize queryString = queryString_;

@end
