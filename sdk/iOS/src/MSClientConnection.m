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


#pragma mark * MSClientConnection Private Interface


@interface MSClientConnection () 

// Private properties
@property (nonatomic, strong)               NSURLConnection *urlConnection;
@property (nonatomic, strong)               NSData *data;
@property (nonatomic, strong)               NSHTTPURLResponse *response;
@property (nonatomic, strong, readonly)     MSSuccessBlock successBlock;
@property (nonatomic, strong, readonly)     MSErrorBlock errorBlock;

@end


#pragma mark * MSClientConnection Implementation


@implementation MSClientConnection

@synthesize client = client_;
@synthesize urlConnection = urlConnection_;
@synthesize successBlock = successBlock_;
@synthesize errorBlock = errorBlock_;
@synthesize data = data_;
@synthesize response = response_;


# pragma mark * Public Initializer Methods


-(id) initWithRequest:(NSURLRequest *)request
           withClient:(MSClient *)client
            onSuccess:(MSSuccessBlock)onSuccess
              onError:(MSErrorBlock)onError
{
    self = [super init];
    if (self) {
        client_ = client;
        successBlock_ = [onSuccess copy];
        errorBlock_ = [onError copy];
        urlConnection_ = [[NSURLConnection alloc]
                                initWithRequest:request
                                delegate:self];
    }
    
    return self;
}

-(void) dealloc
{
    // Cancel any outstanding NSUrlConnection
    [urlConnection_ cancel];
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
