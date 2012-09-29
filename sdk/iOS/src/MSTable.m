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

#import "MSTable.h"
#import "MSQuery.h"
#import "MSJSONSerializer.h"
#import "MSTableRequest.h"
#import "MSTableConnection.h"


#pragma mark * MSTable Private Interface


@interface MSTable ()

// Private properties
@property (nonatomic, strong, readonly)         id<MSSerializer> serializer;

@end


#pragma mark * MSTable Implementation


@implementation MSTable

@synthesize client = client_;
@synthesize name = name_;
@synthesize serializer = serializer_;


#pragma mark * Public Initializer Methods


-(id) initWithName:(NSString *)tableName andClient:(MSClient *)client;
{    
    self = [super init];
    if (self)
    {
        client_ = client;
        name_ = tableName;
    }
    return self;
}


#pragma mark * Public Insert, Update, Delete Methods


-(void) insert:(NSDictionary *)item
            onSuccess:(MSItemSuccessBlock)onSuccess
            onError:(MSErrorBlock)onError
{
    // Create the request
    MSTableItemRequest *request = [MSTableRequest
                                   requestToInsertItem:item
                                   withTable:self
                                   withSerializer:self.serializer
                                   onError:onError];
    // Send the request
    if (request) {
        [MSTableConnection connectionWithItemRequest:request
                                           onSuccess:onSuccess
                                             onError:onError];
    }
}

-(void) update:(NSDictionary *)item
            onSuccess:(MSItemSuccessBlock)onSuccess
            onError:(MSErrorBlock)onError
{
    // Create the request
    MSTableItemRequest *request = [MSTableRequest
                                   requestToUpdateItem:item
                                   withTable:self
                                   withSerializer:self.serializer
                                   onError:onError];
    // Send the request
    if (request) {
        [MSTableConnection connectionWithItemRequest:request
                                           onSuccess:onSuccess
                                             onError:onError];
    }
}

-(void) delete:(NSDictionary *)item
            onSuccess:(MSDeleteSuccessBlock)onSuccess
            onError:(MSErrorBlock)onError
{
    // Create the request
    MSTableDeleteRequest *request = [MSTableRequest
                                     requestToDeleteItem:item
                                     withTable:self
                                     withSerializer:self.serializer
                                     onError:onError];
    // Send the request
    if (request) {
        [MSTableConnection connectionWithDeleteRequest:request
                                             onSuccess:onSuccess
                                               onError:onError];
    }
}

-(void) deleteWithId:(NSNumber *)itemId
            onSuccess:(MSDeleteSuccessBlock)onSuccess
            onError:(MSErrorBlock)onError
{
    // Create the request
    MSTableDeleteRequest *request = [MSTableRequest
                                     requestToDeleteItemWithId:itemId
                                     withTable:self
                                     withSerializer:self.serializer
                                     onError:onError];
    // Send the request
    if (request) {
        [MSTableConnection connectionWithDeleteRequest:request
                                             onSuccess:onSuccess
                                               onError:onError];
    }
}


#pragma mark * Public Read Methods


-(void) readWithId:(NSNumber *)itemId
            onSuccess:(MSItemSuccessBlock)onSuccess
            onError:(MSErrorBlock)onError
{
    // Create the request
    MSTableItemRequest *request = [MSTableRequest
                                   requestToReadWithId:itemId
                                   withTable:self
                                   withSerializer:self.serializer
                                   onError:onError];
    // Send the request
    if (request) {
        [MSTableConnection connectionWithItemRequest:request
                                           onSuccess:onSuccess
                                             onError:onError];
    }
}

-(void) readWithQueryString:(NSString *)queryString
            onSuccess:(MSReadQuerySuccessBlock)onSuccess
            onError:(MSErrorBlock)onError
{
    // Create the request
    MSTableReadQueryRequest *request = [MSTableRequest
                                        requestToReadItemsWithQuery:queryString
                                        withTable:self
                                        withSerializer:self.serializer
                                        onError:onError];
    // Send the request
    if (request) {
        [MSTableConnection connectionWithReadRequest:request
                                           onSuccess:onSuccess
                                             onError:onError];
    }
}

-(void) readOnSuccess:(MSReadQuerySuccessBlock)onSuccess
              onError:(MSErrorBlock)onError;
{
    // Read without a query string
    [self readWithQueryString:nil
                   onSuccess:onSuccess
                     onError:onError];
    
}

-(void) readWhere:(NSPredicate *) predicate
            onSuccess:(MSReadQuerySuccessBlock)onSuccess
            onError:(MSErrorBlock)onError
{
    // Create the query from the predicate
    MSQuery *query = [self queryWhere:predicate];
    
    // Call read on the query
    [query readOnSuccess:onSuccess onError:onError];
}


#pragma mark * Public Query Methods


-(MSQuery *) query
{
    return [[MSQuery alloc] initWithTable:self];
}

-(MSQuery *) queryWhere:(NSPredicate *)predicate
{
    return [[MSQuery alloc] initWithTable:self withPredicate:predicate];
}


#pragma mark * Private Methods


-(id<MSSerializer>) serializer
{
    // Just use a hard coded reference to MSJSONSerializer
    return [MSJSONSerializer JSONSerializer];
}

@end
