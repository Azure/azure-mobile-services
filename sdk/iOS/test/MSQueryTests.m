// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

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
                  isEqualToString:@"$filter=(name%20eq%20'bob')&$inlinecount=none"],
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

-(void)testMSQueryFetchLimitPropertyBoundsCase
{
    query = [[MSQuery alloc] initWithTable:table predicate:nil];
    query.fetchLimit = 0;
    query.includeTotalCount = YES;
    STAssertTrue([query.description isEqualToString:
                  @"$top=0&$inlinecount=allpages"],
                 @"OData query string was: %@",
                 query.description);
}

-(void)testMSQueryFetchOffsetPropertyCanBeSet
{
    query = [[MSQuery alloc] initWithTable:table predicate:nil];
    query.fetchOffset = 542;
    
    NSArray *result = [[query.description componentsSeparatedByString:@"&"]
                        sortedArrayUsingSelector:@selector(localizedCaseInsensitiveCompare:)];
    NSArray *expected = @[@"$inlinecount=none", @"$skip=542"];
    
    STAssertTrue([result isEqualToArray:expected],
                 @"OData query string was: %@",
                 query.description);
}

-(void)testMSQueryFetchOffsetPropertyBoundsCase
{
    query = [[MSQuery alloc] initWithTable:table predicate:nil];
    query.fetchOffset = 0;
    NSArray *result = [[query.description componentsSeparatedByString:@"&"]
                       sortedArrayUsingSelector:@selector(localizedCaseInsensitiveCompare:)];
    NSArray *expected = @[@"$inlinecount=none", @"$skip=0"];
    
    STAssertTrue([result isEqualToArray:expected],
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
        @"key2": @"14",
    };

    NSArray *result = [[query.description componentsSeparatedByString:@"&"]
                       sortedArrayUsingSelector:@selector(localizedCaseInsensitiveCompare:)];
    NSArray *expected = @[@"$inlinecount=none", @"key1=someValue", @"key2=14"];
    
    STAssertTrue([result isEqualToArray:expected],
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
    
    NSArray *result = [[query.description componentsSeparatedByString:@"&"]
                       sortedArrayUsingSelector:@selector(localizedCaseInsensitiveCompare:)];
    NSArray *expected = @[@"$inlinecount=none", @"$orderby=name%20asc,zipcode%20desc,birthdate%20asc"];

    STAssertTrue([result isEqualToArray:expected],
                 @"Query string was: %@",
                 query.description);
}

@end
