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
#import "MSQuery.h"
#import "MSTable.h"
#import "MSClient.h"


@interface MSQueryTests : SenTestCase {
    MSClient *client;
    MSTable *table;
    MSQuery *query;
}

@end

@implementation MSQueryTests


#pragma mark * Setup and TearDown


- (void) setUp
{
    NSLog(@"%@ setUp", self.name);
    
    client = [MSClient clientWithApplicationURLString:@"http://someAppUrl"];
    STAssertNotNil(client, @"Could not create test client.");
    
    table = [[MSTable alloc] initWithName:@"someTable" client:client];
    STAssertNotNil(table, @"Could not create test table.");
}

- (void) tearDown
{
    NSLog(@"%@ tearDown", self.name);
}


#pragma mark * Init Method Tests


-(void)testMSQueryInitWithNilPredicateIsAllowed
{
    NSLog(@"%@ start", self.name);
    
    query = [[MSQuery alloc] initWithTable:table predicate:nil];
    STAssertTrue([query.description isEqualToString:@"$inlinecount=none"],
                 @"OData query string was: %@",
                 query.description);
    
    NSLog(@"%@ end", self.name);
}

-(void)testMSQueryInitWithSimplePredicate
{    
    NSPredicate *predicate = [NSPredicate predicateWithFormat:@"name == 'bob'"];
    
    query = [[MSQuery alloc] initWithTable:table predicate:predicate];
    STAssertTrue([query.description
                  isEqualToString:@"$filter=(name eq 'bob')&$inlinecount=none"],
                 @"OData query string was: %@",
                 query.description);
}


#pragma mark * Property Tests


-(void)testMSQueryTableProperty
{
    query = [[MSQuery alloc] initWithTable:table predicate:nil];
    STAssertEqualObjects(query.table,
                         table,
                         @"'table' property didn't return the table from init.");
}

-(void)testMSQueryFetchLimitPropertyCanBeSet
{
    query = [[MSQuery alloc] initWithTable:table predicate:nil];
    query.fetchLimit = 22;
    STAssertTrue([query.description isEqualToString:
                  @"$top=22&$inlinecount=none"],
                  @"OData query string was: %@",
                  query.description);
}

-(void)testMSQueryFetchOffsetPropertyCanBeSet
{
    query = [[MSQuery alloc] initWithTable:table predicate:nil];
    query.fetchOffset = 542;
    STAssertTrue([query.description isEqualToString:@"$inlinecount=none&$skip=542"],
                 @"OData query string was: %@",
                 query.description);
}

-(void)testMSQueryIncludeTotalCountPropertySetToTrue
{
    query = [[MSQuery alloc] initWithTable:table predicate:nil];
    query.includeTotalCount = YES;
    STAssertTrue([query.description isEqualToString:@"$inlinecount=allpages"],
                 @"Query string was: %@",
                 query.description);
}

-(void)testMSQueryIncludeTotalCountPropertySetToFalse
{
    query = [[MSQuery alloc] initWithTable:table predicate:nil];
    query.includeTotalCount = NO;
    STAssertTrue([query.description isEqualToString:@"$inlinecount=none"],
                 @"Query string was: %@",
                 query.description);
}

-(void)testMSQueryParametersPropertyCanBeSet
{    
    query = [[MSQuery alloc] initWithTable:table predicate:nil];
    query.parameters = @{
        @"key1": @"someValue",
        @"$top": @"14",
    };
    
    STAssertTrue([query.description isEqualToString:
                 @"$top=14&key1=someValue&$inlinecount=none"],
                 @"Query string was: %@",
                 query.description);
}

-(void)testMSQuerySelectFieldsPropertyCanBeSet
{
    query = [[MSQuery alloc] initWithTable:table predicate:nil];
    query.selectFields = @[ @"address", @"birthdate" ];
    STAssertTrue([query.description isEqualToString:
                 @"$select=address,birthdate&$inlinecount=none"],
                 @"Query string was: %@",
                 query.description);
}


#pragma mark * OrderBy Methods


-(void)testMSQueryOrderByCapturesMethodCallOrder
{
    query = [[MSQuery alloc] initWithTable:table predicate:nil];
    [query orderByAscending:@"name"];
    [query orderByDescending:@"zipcode"];
    [query orderByAscending:@"birthdate"];
    STAssertTrue([query.description isEqualToString:
                @"$orderby=name asc,zipcode desc,birthdate asc&$inlinecount=none"],
                 @"Query string was: %@",
                 query.description);
}

@end
