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

#import <SenTestingKit/SenTestingKit.h>
#import "MSClient.h"

@interface MSClientTests : SenTestCase

@end


@implementation MSClientTests


#pragma mark * Setup and TearDown


-(void) setUp {
    
    NSLog(@"%@ setUp", self.name);
}

-(void) tearDown {
    
    NSLog(@"%@ tearDown", self.name);
}


#pragma mark * Static Constructor Method Tests


-(void) testStaticConstructor1ReturnsClient
{
    MSClient *client =
    [MSClient clientWithApplicationURLString:@"http://someURL.com"];
    
    STAssertNotNil(client, @"client should not be nil.");
    
    STAssertNotNil(client.applicationURL, @"client.applicationURL should not be nil.");
    NSString *urlString = [client.applicationURL absoluteString];
    STAssertTrue([urlString isEqualToString:@"http://someURL.com"],
                 @"The client should be using the url it was created with.");
    
    STAssertNil(client.applicationKey, @"client.applicationKey should be nil.");
}

-(void) testStaticConstructor2ReturnsClient
{
    MSClient *client =
    [MSClient clientWithApplicationURLString:@"http://someURL.com"
                          withApplicationKey:@"here is some key"];
    
    STAssertNotNil(client, @"client should not be nil.");
    
    STAssertNotNil(client.applicationURL, @"client.applicationURL should not be nil.");
    NSString *urlString = [client.applicationURL absoluteString];
    STAssertTrue([urlString isEqualToString:@"http://someURL.com"],
                 @"The client should be using the url it was created with.");
    
    STAssertNotNil(client.applicationKey, @"client.applicationKey should not be nil.");
    STAssertTrue([client.applicationKey isEqualToString:@"here is some key"],
                 @"The client should be using the url it was created with.");
}

-(void) testStaticConstructor3ReturnsClient
{
    NSURL *appURL = [NSURL URLWithString:@"http://someURL.com"];
    
    MSClient *client =
    [MSClient clientWithApplicationURL:appURL];
    
    STAssertNotNil(client, @"client should not be nil.");
    
    STAssertNotNil(client.applicationURL, @"client.applicationURL should not be nil.");
    NSString *urlString = [client.applicationURL absoluteString];
    STAssertTrue([urlString isEqualToString:@"http://someURL.com"],
                 @"The client should be using the url it was created with.");
    
    STAssertNil(client.applicationKey, @"client.applicationKey should be nil.");
}

-(void) testStaticConstructor4ReturnsClient
{
    NSURL *appURL = [NSURL URLWithString:@"http://someURL.com"];
    
    MSClient *client =
    [MSClient clientWithApplicationURL:appURL
                    withApplicationKey:@"here is some key"];
    
    STAssertNotNil(client, @"client should not be nil.");
    
    STAssertNotNil(client.applicationURL, @"client.applicationURL should not be nil.");
    NSString *urlString = [client.applicationURL absoluteString];
    STAssertTrue([urlString isEqualToString:@"http://someURL.com"],
                 @"The client should be using the url it was created with.");
    
    STAssertNotNil(client.applicationKey, @"client.applicationKey should not be nil.");
    STAssertTrue([client.applicationKey isEqualToString:@"here is some key"],
                 @"The client should be using the url it was created with.");
}

-(void) testStaticConstructorPercentEncodesURL
{
    MSClient *client =
        [MSClient clientWithApplicationURLString:@"http://yeah! .com"];
    
    NSString *urlString = [client.applicationURL absoluteString];
    STAssertTrue([urlString isEqualToString:@"http://yeah!%20.com"],
                 @"The client should have encoded and normalized the url: %@", urlString);
}


#pragma mark * Init Method Tests


-(void) testInitWithApplicationURL
{
    NSURL *appURL = [NSURL URLWithString:@"http://someURL.com"];
    
    MSClient *client = [[MSClient alloc] initWithApplicationURL:appURL];
    
    STAssertNotNil(client, @"client should not be nil.");
    
    STAssertNotNil(client.applicationURL, @"client.applicationURL should not be nil.");
    NSString *urlString = [client.applicationURL absoluteString];
    STAssertTrue([urlString isEqualToString:@"http://someURL.com"],
                 @"The client should be using the url it was created with.");
    
    STAssertNil(client.applicationKey, @"client.applicationKey should be nil.");
}

-(void) testInitWithApplicationURLAllowsNilURL
{    
    MSClient *client = [[MSClient alloc] initWithApplicationURL:nil];
    
    STAssertNotNil(client, @"client should not be nil.");
    
    STAssertNil(client.applicationURL, @"client.applicationURL should be nil.");
    
    STAssertNil(client.applicationKey, @"client.applicationKey should be nil.");
}


-(void) testInitWithApplicationURLAndApplicationKey
{
    NSURL *appURL = [NSURL URLWithString:@"http://someURL.com"];
    
    MSClient *client =
    [[MSClient alloc] initWithApplicationURL:appURL
                    withApplicationKey:@"here is some key"];
    
    STAssertNotNil(client, @"client should not be nil.");
    
    STAssertNotNil(client.applicationURL, @"client.applicationURL should not be nil.");
    NSString *urlString = [client.applicationURL absoluteString];
    STAssertTrue([urlString isEqualToString:@"http://someURL.com"],
                 @"The client should be using the url it was created with.");
    
    STAssertNotNil(client.applicationKey, @"client.applicationKey should not be nil.");
    STAssertTrue([client.applicationKey isEqualToString:@"here is some key"],
                 @"The client should be using the url it was created with.");
}


#pragma mark * GetTable Method Tests

-(void) testGetTableReturnsTable
{
    MSClient *client =
    [MSClient clientWithApplicationURLString:@"http://someURL.com"];

    MSTable *table = [client getTable:@"Some Table Name"];
    
    STAssertNotNil(table, @"table should not be nil.");
}

-(void) testGetTableAllowsNilTableName
{
    MSClient *client =
    [MSClient clientWithApplicationURLString:@"http://someURL.com"];
    
    MSTable *table = [client getTable:nil];
    
    STAssertNotNil(table, @"table should not be nil.");
}

@end
