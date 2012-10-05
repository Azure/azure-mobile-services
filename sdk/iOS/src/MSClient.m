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

#import "MSClient.h"
#import "MSTable.h"
#import "MSUser.h"


#pragma mark * MSClient Private Interface


@interface MSClient ()

// Public readonly, private readwrite properties
@property (nonatomic, strong, readwrite)         NSArray *filters;

@end


#pragma mark * MSClient Implementation


@implementation MSClient

@synthesize applicationURL = applicationURL_;
@synthesize applicationKey = applicationKey_;
@synthesize currentUser = currentUser_;


#pragma mark * Public Static Constructor Methods


+(MSClient *) clientWithApplicationURLString:(NSString *)urlString
{
    return [MSClient clientWithApplicationURLString:urlString
                                 withApplicationKey:nil];
}

+(MSClient *) clientWithApplicationURLString:(NSString *)urlString
                           withApplicationKey:(NSString *)key
{
    // NSURL will be nil for non-percent escaped url strings so we have to
    // percent escape here
    NSString  *urlStringEncoded =
    [urlString stringByAddingPercentEscapesUsingEncoding:NSUTF8StringEncoding];
    
    NSURL *url = [NSURL URLWithString:urlStringEncoded];
    return [MSClient clientWithApplicationURL:url withApplicationKey:key];
}

+(MSClient *) clientWithApplicationURL:(NSURL *)url
{
    return [MSClient clientWithApplicationURL:url withApplicationKey:nil];
}

+(MSClient *) clientWithApplicationURL:(NSURL *)url
                    withApplicationKey:(NSString *)key
{
    return [[MSClient alloc] initWithApplicationURL:url withApplicationKey:key];    
}


#pragma mark * Public Initializer Methods


-(id) initWithApplicationURL:(NSURL *)url
{
    return [self initWithApplicationURL:url withApplicationKey:nil];
}

-(id) initWithApplicationURL:(NSURL *)url withApplicationKey:(NSString *)key
{
    self = [super init];
    if(self)
    {
        applicationURL_ = url;
        applicationKey_ = [key copy];
    }
    return self;
}


#pragma mark * Public Filter Methods


-(MSClient *) clientwithFilter:(id<MSFilter>)filter
{
    // Create a deep copy of the client (except for the filters)
    MSClient *newClient = [self copy];
    
    // Either copy or create a new filters array
    NSMutableArray *filters = [self.filters mutableCopy];
    if (!filters) {
        filters = [NSMutableArray arrayWithCapacity:1];
    }
    
    // Add the filter to the filters array
    [filters addObject:filter];
    
    // Set the new filters on the copied client
    newClient.filters = filters;
    
    return newClient;
}


#pragma mark * Public Authentication Methods


-(void) loginWithProvider:(NSString *)provider
         onSuccess:(MSClientLoginSuccessBlock)onSuccess
           onError:(MSErrorBlock)onError
{
    // TODO: Implement
    NSAssert(FALSE, @"Not yet implemented.");
}

-(void) loginWithProvider:(NSString *)provider
                withToken:(NSString *)token
         onSuccess:(MSClientLoginSuccessBlock)onSuccess
           onError:(MSErrorBlock)onError
{
    // TODO: Implement
    NSAssert(FALSE, @"Not yet implemented.");
}

-(void) logout
{
    // TODO: Implement
    NSAssert(FALSE, @"Not yet implemented.");
}


#pragma mark * Public Table Constructor Methods


-(MSTable *) getTable:(NSString *)tableName
{
    return [[MSTable alloc] initWithName:tableName andClient:self];
}


#pragma mark * NSCopying Methods


-(id) copyWithZone:(NSZone *)zone
{
    MSClient *client = [[MSClient allocWithZone:zone]
                            initWithApplicationURL:self.applicationURL
                                withApplicationKey:self.applicationKey];
                                                                            
    client.currentUser = [self.currentUser copyWithZone:zone];
    client.filters = [self.filters copyWithZone:zone];

    return client;
}

@end
