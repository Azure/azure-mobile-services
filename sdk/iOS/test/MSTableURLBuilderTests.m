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
#import "MSTable.h"
#import "MSTableURLBuilder.h"

@interface MSTableURLBuilderTests : SenTestCase

@end

@implementation MSTableURLBuilderTests


#pragma mark * Setup and TearDown


-(void) setUp
{
    NSLog(@"%@ setUp", self.name);
}

-(void) tearDown
{
    NSLog(@"%@ tearDown", self.name);
}


#pragma mark * URL Building Method Tests

-(void) testURLForTable
{
    // For each test case: the first element is the expected  URL;
    // the second element is the app format URL; and third element is
    // the table name
    NSArray *testCases = @[
    
        // Vanilla test case
        @[ @"http://someApp.com/tables/someTable",
           @"http://someApp.com",
           @"someTable"],
    
        // App URL with extra slashes
        @[ @"http://someApp.com/tables/someTable",
           @"http://someApp.com/",
           @"someTable"],
    
        // App URL with query string
        @[ @"http://someApp.com/tables/someTable?x=y",
           @"http://someApp.com?x=y",
           @"someTable"],
    
        // Whitespace and path in App URL
        @[ @"http://someApp.com/some%20path/tables/some%20table",
           @"http://someApp.com/some path",
           @"some table"]
    ];
    
    for (id testCase in testCases) {
        
        NSString *appURL = [testCase objectAtIndex:1];
        NSString *tableName = [testCase objectAtIndex:2];
        NSString *expectedURL = [testCase objectAtIndex:0];
        
        MSClient *client = [MSClient clientWithApplicationURLString:appURL];
        MSTable *table = [client getTable:tableName];
        
        NSURL *url = [MSTableURLBuilder URLForTable:table];
        
        STAssertNotNil(url, @"url sould not be nil");
        STAssertTrue([[url absoluteString] isEqualToString:expectedURL],
                     @"the url was: %@", [url absoluteString]);
    }
}

-(void) testURLForTableWithItemId
{
    // For each test case: the first element is the expected  URL;
    // the second element is the app format URL; the third element is
    // the table name; and the forth element is the itemId
    NSArray *testCases = @[
    
        // Vanilla test case
        @[ @"http://someApp.com/tables/someTable/5",
           @"http://someApp.com",
           @"someTable",
           @"5"],
    
        // App URL with query string
        @[ @"http://someApp.com/tables/someTable/100?x=y",
           @"http://someApp.com?x=y",
           @"someTable",
           @"100"],
    
        // Whitespace and path in App URL and non integer id
        @[ @"http://someApp.com/some%20path/tables/some%20table/an%20id",
           @"http://someApp.com/some path",
           @"some table",
           @"an id"]
    ];
    
    for (id testCase in testCases) {
        
        NSString *appURL = [testCase objectAtIndex:1];
        NSString *tableName = [testCase objectAtIndex:2];
        NSString *itemId = [testCase objectAtIndex:3];
        NSString *expectedURL = [testCase objectAtIndex:0];
        
        MSClient *client = [MSClient clientWithApplicationURLString:appURL];
        MSTable *table = [client getTable:tableName];
        
        NSURL *url = [MSTableURLBuilder URLForTable:table withItemIdString:itemId];
        
        STAssertNotNil(url, @"url sould not be nil");
        STAssertTrue([[url absoluteString] isEqualToString:expectedURL],
                     @"the url was: %@", [url absoluteString]);
    }
}

-(void) testURLForTableWithQuery
{
    // For each test case: the first element is the expected  URL;
    // the second element is the app format URL; the third element is
    // the table name; and the forth element is the itemId
    NSArray *testCases = @[
    
        // Vanilla test case
        @[ @"http://someApp.com/tables/someTable?$top=5",
           @"http://someApp.com",
           @"someTable",
           @"$top=5"],
        
        // App URL with query string
        @[ @"http://someApp.com/tables/someTable?x=y&$filter=(cost%20gt%2015)",
           @"http://someApp.com?x=y",
           @"someTable",
           @"$filter=(cost gt 15)"],
        
        // Whitespace and path in App URL
        @[ @"http://someApp.com/some%20path/tables/some%20table?a=b",
           @"http://someApp.com/some path",
           @"some table",
           @"a=b"]
    ];
    
    for (id testCase in testCases) {
        
        NSString *appURL = [testCase objectAtIndex:1];
        NSString *tableName = [testCase objectAtIndex:2];
        NSString *query = [testCase objectAtIndex:3];
        NSString *expectedURL = [testCase objectAtIndex:0];
        
        MSClient *client = [MSClient clientWithApplicationURLString:appURL];
        MSTable *table = [client getTable:tableName];
        
        NSURL *url = [MSTableURLBuilder URLForTable:table withQuery:query];
        
        STAssertNotNil(url, @"url sould not be nil");
        STAssertTrue([[url absoluteString] isEqualToString:expectedURL],
                     @"the url was: %@", [url absoluteString]);
    }
}


@end
