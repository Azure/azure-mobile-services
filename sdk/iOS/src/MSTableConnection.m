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

#import "MSTableConnection.h"
#import "MSSerializer.h"


#pragma mark * MSTableConnection Private Interface


@interface MSTableConnection ()

// Private properties
@property (nonatomic, strong, readwrite)        MSErrorBlock errorBlock;
@property (nonatomic, strong, readwrite)        id<MSSerializer> serializer;

@end


#pragma mark * MSTableConnection Implementation


@implementation MSTableConnection

@synthesize table = table_;
@synthesize errorBlock = errorBlock_;
@synthesize serializer = serializer_;


#pragma mark * Public Static Constructors


+(MSTableConnection *) connectionWithItemRequest:(MSTableItemRequest *)request
                                       onSuccess:(MSItemSuccessBlock)onSuccess
                                         onError:(MSErrorBlock)onError
{
    // We'll use the conection in the response block below but won't set
    // it until the init at the end, so we need to use __block
    __block MSTableConnection *connection = nil;
    
    // Create an HTTP response success block that will invoke the
    // MSItemSuccessBlock
    MSSuccessBlock responseSuccess =
    
        ^(NSHTTPURLResponse *response, NSData *data)
        {
            // |isSuccessfulResponse:withData| will call the onError block
            // if there is an error.
            if ([connection isSuccessfulResponse:response withData:data]) {
                
                // |itemFromData:withResponse:| will call the onError block
                // if there is an error.
                id item = [connection itemFromData:data withResponse:response];
                
                if (item && onSuccess) {
                    onSuccess(item);
                }
            }
                
        };
    
    // Now create the connection with the MSSuccessBlock
    connection = [[MSTableConnection alloc] initWithTableRequest:request
                                                       onSuccess:responseSuccess
                                                         onError:onError];
    return connection;
}

+(MSTableConnection *) connectionWithDeleteRequest:(MSTableDeleteRequest *)request
                                         onSuccess:(MSDeleteSuccessBlock)onSuccess
                                           onError:(MSErrorBlock)onError
{
    // We'll use the conection in the response block below but won't set
    // it until the init at the end, so we need to use __block
    __block MSTableConnection *connection = nil;
    
    // Create an HTTP response success block that will invoke the
    // MSDeleteSuccessBlock
    MSSuccessBlock responseSuccess =
    
        ^(NSHTTPURLResponse *response, NSData *data)
        {
            // |isSuccessfulResponse:withData| will call the onError block
            // if there is an error.
            if ([connection isSuccessfulResponse:response withData:data]) {
                
                if (onSuccess) {
                    onSuccess(request.itemId );
                }
            }
            
        };
    
    // Now create the connection with the MSSuccessBlock
    connection = [[MSTableConnection alloc] initWithTableRequest:request
                                                       onSuccess:responseSuccess
                                                         onError:onError];
    return connection;

}
                                      
+(MSTableConnection *) connectionWithReadRequest:(MSTableReadQueryRequest *)request
                                       onSuccess:(MSReadQuerySuccessBlock)onSuccess
                                         onError:(MSErrorBlock)onError
{
    // We'll use the conection in the response block below but won't set
    // it until the init at the end, so we need to use __block
    __block MSTableConnection *connection = nil;
    
    // Create an HTTP response success block that will invoke the
    // MSReadQuerySuccessBlock
    MSSuccessBlock responseSuccess =
    
        ^(NSHTTPURLResponse *response, NSData *data)
        {
            // |isSuccessfulResponse:withData| will call the onError block
            // if there is an error.
            if ([connection isSuccessfulResponse:response withData:data]) {
                
                // |items:fromData:withResponse:| will call the onError block
                // if there is an error.
                NSArray *items;
                NSInteger totalCount = [connection items:&items
                                                fromData:data
                                            withResponse:response];
                if (items && onSuccess) {
                    onSuccess(items, totalCount);
                }
            }
            
        };
    
    // Now create the connection with the MSSuccessBlock
    connection = [[MSTableConnection alloc] initWithTableRequest:request
                                                       onSuccess:responseSuccess
                                                         onError:onError];
    return connection;
}


# pragma mark * Private Init Methods


-(id) initWithTableRequest:(MSTableRequest *)request
                 onSuccess:(MSSuccessBlock)onSuccess
                   onError:(MSErrorBlock)onError
{
    self = [super initWithRequest:request
                       withClient:request.table.client
                        onSuccess:onSuccess
                          onError:onError];
    
    if (self) {
        table_ = request.table;
        serializer_ = request.serializer;
        errorBlock_ = onError;
    }
    
    return self;
}


# pragma mark * Private Methods


-(BOOL) isSuccessfulResponse:(NSHTTPURLResponse *)response
                    withData:(NSData *)data
{
    // Success is determined just by the HTTP status code
    BOOL isSuccessful = response.statusCode < 400;
    
    if (!isSuccessful && self.errorBlock) {
        
        NSError *error = nil;
        
        // Try to read the error message from the response body
        if (data) {
            error =[self.serializer errorFromData:data];
        }
        
        // Otherwise use a generic error message
        if (!error) {
            NSString *description =
                NSLocalizedString(@"HTTPResponseErrorStatusSansMessage", nil);
            
            // Include the request and response in the userInfo
            NSDictionary *userInfo = @{
                NSLocalizedDescriptionKey : description,
                MSErrorRequestKey : self.request,
                MSErrorResponseKey : response
            };
            
            error = [NSError errorWithDomain:MSErrorDomain
                                        code:MSErrorNoMessageErrorCode
                                    userInfo:userInfo];
        }
        
        self.errorBlock(error);
        
    }

    return isSuccessful;
}

-(id) itemFromData:(NSData *)data withResponse:(NSHTTPURLResponse *)response
{
    // Try to deserialize the data
    NSError *error = nil;
    id item = [self.serializer itemFromData:data
                           withOriginalItem:nil
                                    orError:&error];
    
    // If there was an error, invoke the errorBlock
    [self callErrorBlockIfError:error withResponse:response];
    
    return item;
}

-(NSInteger) items:(NSArray **)items
                fromData:(NSData *)data
                withResponse:(NSHTTPURLResponse *)response
{
    // Try to deserialize the data
    NSError *error = nil;
    NSInteger totalCount = [self.serializer totalCountAndItems:items
                                                      fromData:data
                                                       orError:&error];
    
    // If there was an error, invoke the errorBlock
    [self callErrorBlockIfError:error withResponse:response];
    
    return totalCount;
}

-(void) callErrorBlockIfError:(NSError *)error
                 withResponse:(NSHTTPURLResponse *)response
{
    if (error && self.errorBlock) {
        
        // Create a new error with request and the response in the userInfo...
        NSMutableDictionary *userInfo = [error.userInfo mutableCopy];
        [userInfo setObject:self.request forKey:MSErrorRequestKey];
        [userInfo setObject:response forKey:MSErrorResponseKey];
        
        NSError *newError = [NSError errorWithDomain:error.domain
                                                code:error.code
                                            userInfo:userInfo];
        
        // and then call the error block
        self.errorBlock(newError);
    }
}

@end
