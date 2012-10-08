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

#import "MSClientConnection.h"
#import "MSUserAgentBuilder.h"
#import "MSFilter.h"


#pragma mark * HTTP Header String Constants


NSString *const xApplicationHeader = @"X-ZUMO-APPLICATION";
NSString *const contentTypeHeader = @"Content-Type";
NSString *const userAgentHeader = @"User-Agent";
NSString *const jsonContentType = @"application/json";



#pragma mark * MSConnectionDelegate Private Interface


// The |MSConnectionDelegate| is a private class that implements the
// |NSURLConnectionDataDelegate| and surfaces success and error blocks. It
// is used only by the |MSClientConnection|.
@interface MSConnectionDelegate : NSObject <NSURLConnectionDataDelegate>

@property (nonatomic, strong)               NSData *data;
@property (nonatomic, strong)               NSHTTPURLResponse *response;
@property (nonatomic, strong, readonly)     MSSuccessBlock successBlock;
@property (nonatomic, strong, readonly)     MSErrorBlock errorBlock;

-(id) initWithOnSuccess:(MSSuccessBlock)onSuccess
                onError:(MSErrorBlock)onError;
@end


#pragma mark * MSClientConnection Private Interface


@interface MSClientConnection ()

// Private properties
@property (nonatomic, copy, readonly)       MSSuccessBlock onSuccess;
@property (nonatomic, copy, readonly)       MSErrorBlock onError;

@end


#pragma mark * MSClientConnection Implementation


@implementation MSClientConnection

@synthesize client = client_;
@synthesize request = request_;
@synthesize onSuccess = onSuccess_;
@synthesize onError = onError_;


# pragma mark * Public Initializer Methods


-(id) initWithRequest:(NSURLRequest *)request
           withClient:(MSClient *)client
            onSuccess:(MSSuccessBlock)onSuccess
              onError:(MSErrorBlock)onError
{
    self = [super init];
    if (self) {
        client_ = client;
        request_ = [MSClientConnection configureHeadersOnRequest:request
                                                      withClient:client];
        onSuccess_ = onSuccess;
        onError_ = onError;
    }
    
    return self;
}


#pragma mark * Public Start Method


-(void) start
{
    [MSClientConnection invokeNextFilter:self.client.filters
                             withRequest:self.request
                              onResponse:self.onSuccess
                                 onError:self.onError];
}


# pragma mark * Private Static Methods


+(void) invokeNextFilter:(NSArray *)filters
             withRequest:(NSURLRequest *)request
               onResponse:(MSFilterResponseBlock)onResponse
                 onError:(MSErrorBlock)onError
{
    if (!filters || filters.count == 0) {
        
        // No filters to invoke so use |NSURLConnection | to actually
        // send the request.
        MSConnectionDelegate *delegate = [[MSConnectionDelegate alloc]
                                          initWithOnSuccess:onResponse
                                          onError:onError];
        [NSURLConnection connectionWithRequest:request delegate:delegate];
    }
    else {
        
        // Since we have at least one more filter, construct the nextBlock
        // for it and then invoke the filter
        id<MSFilter> nextFilter = [filters objectAtIndex:0];
        NSArray *nextFilters = [filters subarrayWithRange:
                                NSMakeRange(1, filters.count - 1)];
    
        MSFilterNextBlock onNext =
        [^(NSURLRequest *onNextRequest,
           MSFilterResponseBlock onNextResponse,
           MSErrorBlock onNextError)
        {
            [MSClientConnection invokeNextFilter:nextFilters
                                     withRequest:onNextRequest
                                      onResponse:onNextResponse
                                         onError:onNextError];
                                    
        } copy];
        
        [nextFilter handleRequest:request
                           onNext:onNext
                       onResponse:onResponse
                          onError:onError];
    }
}

+(NSURLRequest *) configureHeadersOnRequest:(NSURLRequest *)request
                                 withClient:(MSClient *)client
{
    NSMutableURLRequest *mutableRequest = [request mutableCopy];
    
    // TODO: Add the authentication header
    
    // Set the User Agent header
    NSString *userAgentValue = [MSUserAgentBuilder userAgent];
    [mutableRequest setValue:userAgentValue forHTTPHeaderField:userAgentHeader];
    
    // Set the special Application key header
    NSString *appKey = client.applicationKey;
    if (appKey != nil) {
        [mutableRequest setValue:appKey forHTTPHeaderField:xApplicationHeader];
    }

    // Set the content type header
    [mutableRequest setValue:jsonContentType forHTTPHeaderField:contentTypeHeader];
    
    return mutableRequest;
}


@end


#pragma mark * MSConnectionDelegate Private Implementation


@implementation MSConnectionDelegate

@synthesize successBlock = successBlock_;
@synthesize errorBlock = errorBlock_;
@synthesize data = data_;
@synthesize response = response_;


# pragma mark * Public Initializer Methods


-(id) initWithOnSuccess:(MSSuccessBlock)onSuccess
                onError:(MSErrorBlock)onError
{
    self = [super init];
    if (self) {
        successBlock_ = [onSuccess copy];
        errorBlock_ = [onError copy];
    }
    
    return self;
}


# pragma mark * NSURLConnectionDelegate Methods


-(void) connection:(NSURLConnection *)connection
  didFailWithError:(NSError *)error
{
    if (self.errorBlock) {
        self.errorBlock(error);
    }
}


# pragma mark * NSURLConnectionDataDelegate Methods


- (void)connection:(NSURLConnection *)connection
didReceiveResponse:(NSURLResponse *)response
{
    // We should only be making HTTP requests
    self.response = (NSHTTPURLResponse *)response;
}

- (void)connection:(NSURLConnection *)connection
    didReceiveData:(NSData *)data
{
    // If we haven't received any data before, just take this data instance
    if (!self.data) {
        self.data = data;
    }
    else {
        
        // Otherwise, append this data to what we have
        NSMutableData *newData = [NSMutableData dataWithData:self.data];
        [newData appendData:data];
        self.data = newData;
    }
}

- (NSCachedURLResponse *)connection:(NSURLConnection *)connection
                  willCacheResponse:(NSCachedURLResponse *)cachedResponse
{
    // We don't want to cache anything
    return nil;
}

- (void)connectionDidFinishLoading:(NSURLConnection *)connection
{
    if (self.successBlock) {
        self.successBlock(self.response, self.data);
    }
}

@end

