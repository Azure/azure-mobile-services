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
        applicationKey_ = key;
    }
    return self;
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

@end
