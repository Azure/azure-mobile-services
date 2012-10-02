// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#import "MSTableRequest.h"
#import "MSTableURLBuilder.h"


#pragma mark * HTTP Method and Header String Constants


NSString *const httpGet = @"GET";
NSString *const httpPatch = @"PATCH";
NSString *const httpPost = @"POST";
NSString *const httpDelete = @"DELETE";
NSString *const xApplicationHeader = @"X-ZUMO-APPLICATION";
NSString *const contentTypeHeader = @"Content-Type";
NSString *const jsonContentType = @"application/json";


#pragma mark * MSTableRequest Private Interface


@interface MSTableRequest ()

// Public readonly and private readwrite properties 
@property (nonatomic, readwrite)             MSTableRequestType requestType;

// Private initalizer method
-(id) initWithURL:(NSURL *)url
            withTable:(MSTable *)table
            withSerializer:(id<MSSerializer>)serializer;

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
@synthesize serializer = serializer_;


#pragma mark * Private Initializer Method

-(id) initWithURL:(NSURL *)url
            withTable:(MSTable *)table
            withSerializer:(id<MSSerializer>)serializer
{
    self = [super initWithURL:url];
    if (self) {
        table_ = table;
        serializer_ = serializer;
    }
    
    return self;
}

#pragma mark * Public Static Constructors


+(MSTableItemRequest *) requestToInsertItem:(id)item
                                  withTable:(MSTable *)table
                             withSerializer:(id<MSSerializer>)serializer
                                    onError:(MSErrorBlock)onError;
{
    MSTableItemRequest *request = nil;
    
    // Create the URL
    NSURL *url = [MSTableURLBuilder URLForTable:table];
    
    // Create the request
    request = [[MSTableItemRequest alloc] initWithURL:url
                                            withTable:table
                                       withSerializer:serializer];
    
    // Create the body or capture the error from serialization
    NSError *error = nil;
    NSData *data = [serializer dataFromItem:item orError:&error];
    if (!data) {
        request = nil;
        if (onError) {
            onError(error);
        }
    }
    else {
        // Set the body
        request.HTTPBody = data;
        
        // Set the additionl properties
        request.requestType = MSTableInsertRequestType;
        request.item = item;
        
        // Set the method and headers
        request.HTTPMethod = httpPost;
        [request configureHeaders];
    }
    
    return request;
}

+(MSTableItemRequest *) requestToUpdateItem:(id)item
                                  withTable:(MSTable *)table
                             withSerializer:(id<MSSerializer>)serializer
                                    onError:(MSErrorBlock)onError;

{
    MSTableItemRequest *request = nil;
    
    // Ensure we can get an itemId
    NSNumber *itemId = [MSTableRequest idFromItem:item
                                   withSerializer:serializer
                                          onError:onError];
    if (itemId) {

        // Ensure we can get a string from the item Id
        NSString *idString = [MSTableRequest stringFromItemId:itemId
                                               withSerializer:serializer
                                                      onError:onError];
        if (idString) {
            
            // Create the URL
            NSURL *url = [MSTableURLBuilder URLForTable:table
                                       withItemIdString:idString];
        
            // Create the request
            request = [[MSTableItemRequest alloc] initWithURL:url
                                                    withTable:table
                                               withSerializer:serializer];
        
            // Create the body or capture the error from serialization
            NSError *error = nil;
            NSData *data = [serializer dataFromItem:item orError:&error];
            if (!data) {
                request = nil;
                if (onError) {
                    onError(error);
                }
            }
            else {
                // Set the body
                request.HTTPBody = data;
                
                // Set the properties
                request.requestType = MSTableUpdateRequestType;
                request.item = item;

                
                // Set the method and headers
                request.HTTPMethod = httpPatch;
                [request configureHeaders];
            }
        }
    }
    
    return request;
}

+(MSTableDeleteRequest *) requestToDeleteItem:(id)item
                                    withTable:(MSTable *)table
                               withSerializer:(id<MSSerializer>)serializer
                                      onError:(MSErrorBlock)onError;
{
    MSTableDeleteRequest *request = nil;
    
    // Ensure we can get the item Id
    NSNumber *itemId = [MSTableRequest idFromItem:item
                                   withSerializer:serializer
                                          onError:onError];
    if (itemId) {
        
        // Get the request from the other constructor
        request = [MSTableRequest requestToDeleteItemWithId:itemId
                                                  withTable:table
                                             withSerializer:serializer
                                                    onError:onError];
        
        // Set the additional properties
        request.item = item;
    }

    return request;
}

+(MSTableDeleteRequest *) requestToDeleteItemWithId:(id)itemId
                                    withTable:(MSTable *)table
                               withSerializer:(id<MSSerializer>)serializer
                                      onError:(MSErrorBlock)onError;
{
    MSTableDeleteRequest *request = nil;
    
    // Ensure we can get the id as a string
    NSString *idString = [MSTableRequest stringFromItemId:itemId
                                           withSerializer:serializer
                                                  onError:onError];
    if (idString) {
    
        // Create the URL
        NSURL *url = [MSTableURLBuilder URLForTable:table
                                   withItemIdString:idString];
        
        // Create the request
        request = [[MSTableDeleteRequest alloc] initWithURL:url
                                                  withTable:table
                                             withSerializer:serializer];
        
        // Set the additional properties
        request.requestType = MSTableDeleteRequestType;
        request.itemId = itemId;
        
        // Set the method and headers
        request.HTTPMethod = httpDelete;
        [request configureHeaders];
    }
    
    return request;
}

+(MSTableItemRequest *) requestToReadWithId:(id)itemId
                                  withTable:(MSTable *)table
                             withSerializer:(id<MSSerializer>)serializer
                                    onError:(MSErrorBlock)onError;
{
    MSTableItemRequest *request = nil;
    
    // Ensure we can get the id as a string
    NSString *idString = [MSTableRequest stringFromItemId:itemId
                                          withSerializer:serializer
                                                 onError:onError];
    if (idString) {
        // Create the URL
        NSURL *url =  [MSTableURLBuilder URLForTable:table
                                    withItemIdString:idString];
        
        // Create the request
        request = [[MSTableItemRequest alloc] initWithURL:url
                                                withTable:table
                                           withSerializer:serializer];
        
        // Set the additional properties
        request.requestType = MSTableReadRequestType;
        request.itemId = itemId;
        
        // Set the method and headers
        request.HTTPMethod = httpGet;
        [request configureHeaders];
    }
    
    return request;
}

+(MSTableReadQueryRequest *) requestToReadItemsWithQuery:(NSString *)queryString
                                      withTable:(MSTable *)table
                                 withSerializer:(id<MSSerializer>)serializer
                                        onError:(MSErrorBlock)onError;
{
    MSTableReadQueryRequest *request = nil;
    
    // Create the URL
    NSURL *url = [MSTableURLBuilder URLForTable:table withQuery:queryString];
    
    // Create the request
    request = [[MSTableReadQueryRequest alloc] initWithURL:url
                                                withTable:table
                                            withSerializer:serializer];
    
    // Set the additional properties
    request.requestType = MSTableReadQueryRequestType;
    request.queryString = queryString;
    
    // Set the method and headers
    request.HTTPMethod = httpGet;
    [request configureHeaders];
    
    return request;
}


#pragma mark * Private Methods


-(void) configureHeaders
{
    // TODO: Add the authentication header
    
    NSString *appKey = self.table.client.applicationKey;
    if (appKey != nil) {
        [self setValue:appKey forHTTPHeaderField:xApplicationHeader];
    }
    
    [self setValue:jsonContentType forHTTPHeaderField:contentTypeHeader];
}


+(NSError *) errorWithRequest:(MSTableRequest *)request
                 addedToError:(NSError *)error
{
    // Get a copy of the user info and add the request to it
    NSMutableDictionary *userInfo = (error.userInfo == nil) ?
        [NSMutableDictionary dictionary] :
        [error.userInfo mutableCopy];
    
    [userInfo setValue:request forKey:MSErrorRequestKey];
    
    // Return a new NSError 
    return [NSError errorWithDomain:error.domain
                               code:error.code
                           userInfo:userInfo];
}

+(NSString *) stringFromItemId:(NSNumber *)itemId
                withSerializer:serializer
                       onError:(MSErrorBlock)onError
{
    NSError *error = nil;
    
    NSString *idString = [serializer stringFromItemId:itemId orError:&error];
    if (!idString) {
        if (onError) {
            onError(error);
        }
    }
    
    return idString;
}

+(NSNumber *) idFromItem:(NSDictionary *)item
                withSerializer:serializer
                onError:(MSErrorBlock)onError
{
    NSError *error = nil;
    
    NSNumber *itemId = [serializer itemIdFromItem:item orError:&error];
    if (!itemId) {
        if (onError) {
            onError(error);
        }
    }
    
    return itemId;
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
