// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <SenTestingKit/SenTestingKit.h>
#import "MSTable.h"
#import "MSURLBuilder.h"

@interface MSURLBuilderTests : SenTestCase

@end

@implementation MSURLBuilderTests


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
        
        NSString *expectedURL = [testCase objectAtIndex:0];
        NSString *appURL = [testCase objectAtIndex:1];
        NSString *tableName = [testCase objectAtIndex:2];
        
        MSClient *client = [MSClient clientWithApplicationURLString:appURL];
        MSTable *table = [client tableWithName:tableName];
        
        NSURL *url = [MSURLBuilder URLForTable:table
                                     parameters:nil
                                            orError:nil];
        
        STAssertNotNil(url, @"url should not be nil");
        STAssertTrue([[url absoluteString] isEqualToString:expectedURL],
                     @"the url was: %@", [url absoluteString]);
    }
}

-(void) testURLForTableWithParameters
{
    // For each test case: the first element is the expected  URL;
    // the second element is the app format URL; and third element is
    // the table name and the forth element is the parameters
    NSArray *testCases = @[
    
    // Vanilla test case
    @[ @"http://someApp.com/tables/someTable?x=5",
       @"http://someApp.com",
       @"someTable",
       @{@"x" : @5}],
    
    // Parameters that should be URL encoded
    @[ @"http://someApp.com/tables/someTable?x=%23%3F%26$'%20encode%20me%21",
       @"http://someApp.com/",
       @"someTable",
       @{@"x" : @"#?&$' encode me!"}],
    
    // App URL with query string
    @[ @"http://someApp.com/tables/someTable?x=y&x=5",
       @"http://someApp.com?x=y",
       @"someTable",
       @{@"x" : @5}],
    
    // Whitespace and path in App URL
    @[ @"http://someApp.com/some%20path/tables/some%20table?%26encode=5",
       @"http://someApp.com/some path",
       @"some table",
       @{@"&encode" : @5}]
    ];
    
    for (id testCase in testCases) {
        
        NSString *expectedURL = [testCase objectAtIndex:0];
        NSString *appURL = [testCase objectAtIndex:1];
        NSString *tableName = [testCase objectAtIndex:2];
        NSDictionary *parameters = [testCase objectAtIndex:3];
        
        MSClient *client = [MSClient clientWithApplicationURLString:appURL];
        MSTable *table = [client tableWithName:tableName];
        
        NSURL *url = [MSURLBuilder URLForTable:table
                                     parameters:parameters
                                            orError:nil];
        
        STAssertNotNil(url, @"url should not be nil");
        STAssertTrue([[url absoluteString] isEqualToString:expectedURL],
                     @"the url was: %@", [url absoluteString]);
    }
}

-(void) testURLForTableWithInvalidParameters
{
    NSError *error = nil;
    NSString *appURL = @"http://someApp.com/";
    NSString *tableName = @"someTable";
    NSDictionary *parameters = @{ @"$notAllowed" : @5 };
    
    MSClient *client = [MSClient clientWithApplicationURLString:appURL];
    MSTable *table = [client tableWithName:tableName];
    
    
    NSURL *url = [MSURLBuilder URLForTable:table
                                 parameters:parameters
                                        orError:&error];
    
    STAssertNil(url, @"url should be nil");
    STAssertNotNil(error, @"error should not have been nil.");
    STAssertTrue(error.domain == MSErrorDomain,
                 @"error domain should have been MSErrorDomain.");
    STAssertTrue(error.code == MSInvalidUserParameterWithRequest,
                 @"error code should have been MSInvalidUserParameterWithRequest.");
    
    NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
    STAssertTrue([description isEqualToString:@"'$notAllowed' is an invalid user-defined query string parameter. User-defined query string parameters must not begin with a '$'."],
                 @"description was: %@", description);
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
        MSTable *table = [client tableWithName:tableName];
        
        NSURL *url = [MSURLBuilder URLForTable:table
                                   itemIdString:itemId
                                     parameters:nil
                                            orError:nil];
        
        STAssertNotNil(url, @"url should not be nil");
        STAssertTrue([[url absoluteString] isEqualToString:expectedURL],
                     @"the url was: %@", [url absoluteString]);
    }
}

-(void) testURLForTableWithItemIdWithParameters
{
    // For each test case: the first element is the expected  URL;
    // the second element is the app format URL; the third element is
    // the table name; the forth element is the itemId and the fifth
    // element is the parameters
    NSArray *testCases = @[
    
    // Vanilla test case
    @[ @"http://someApp.com/tables/someTable/5?x=5",
       @"http://someApp.com",
       @"someTable",
       @"5",
       @{@"x" : @5}],
    
    // Parameter values that should be URL encoded
    @[ @"http://someApp.com/tables/someTable/100?x=y&x=%23%3F%26$'%20encode%20me%21",
       @"http://someApp.com?x=y",
       @"someTable",
       @"100",
       @{@"x" : @"#?&$' encode me!"}],
    
    // Parameter names that should be URL encoded
    @[ @"http://someApp.com/some%20path/tables/some%20table/an%20id?%26encode=5",
       @"http://someApp.com/some path",
       @"some table",
       @"an id",
       @{@"&encode" : @5}]
    ];
    
    for (id testCase in testCases) {
        
        NSString *expectedURL = [testCase objectAtIndex:0];
        NSString *appURL = [testCase objectAtIndex:1];
        NSString *tableName = [testCase objectAtIndex:2];
        NSString *itemId = [testCase objectAtIndex:3];
        NSDictionary *parameters = [testCase objectAtIndex:4];
        
        MSClient *client = [MSClient clientWithApplicationURLString:appURL];
        MSTable *table = [client tableWithName:tableName];
        
        NSURL *url = [MSURLBuilder URLForTable:table
                                   itemIdString:itemId
                                     parameters:parameters
                                            orError:nil];
        
        STAssertNotNil(url, @"url should not be nil");
        STAssertTrue([[url absoluteString] isEqualToString:expectedURL],
                     @"the url was: %@", [url absoluteString]);
    }
}

-(void) testURLForTableWithItemIdWithInvalidParameters
{
    NSError *error = nil;
    NSString *appURL = @"http://someApp.com/";
    NSString *itemId = @"someId";
    NSString *tableName = @"someTable";
    NSDictionary *parameters = @{ @"$" : @5 };
    
    MSClient *client = [MSClient clientWithApplicationURLString:appURL];
    MSTable *table = [client tableWithName:tableName];
    
    
    NSURL *url = [MSURLBuilder URLForTable:table
                               itemIdString:itemId 
                                 parameters:parameters
                                        orError:&error];
    
    STAssertNil(url, @"url should be nil");
    STAssertNotNil(error, @"error should not have been nil.");
    STAssertTrue(error.domain == MSErrorDomain,
                 @"error domain should have been MSErrorDomain.");
    STAssertTrue(error.code == MSInvalidUserParameterWithRequest,
                 @"error code should have been MSInvalidUserParameterWithRequest.");
    
    NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
    STAssertTrue([description isEqualToString:@"'$' is an invalid user-defined query string parameter. User-defined query string parameters must not begin with a '$'."],
                 @"description was: %@", description);
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
           @"$filter=(cost%20gt%2015)"],
        
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
        MSTable *table = [client tableWithName:tableName];
        
        NSURL *url = [MSURLBuilder URLForTable:table
                                          query:query];
        
        STAssertNotNil(url, @"url should not be nil");
        STAssertTrue([[url absoluteString] isEqualToString:expectedURL],
                     @"the url was: %@", [url absoluteString]);
    }
}

-(void) testURLForApi
{
    // For each test case: the first element is the expected  URL;
    // the second element is the app format URL; and third element is
    // the table name
    NSArray *testCases = @[
    
      // Vanilla test case
      @[ @"http://someApp.com/api/someApi",
         @"http://someApp.com",
         @"someApi"],
    
      // App URL with extra slashes
      @[ @"http://someApp.com/api/someApi/with/a/path",
         @"http://someApp.com/",
         @"someApi/with/a/path"],
    
      // App URL with query string
      @[ @"http://someApp.com/api/someApi/with/a/path/and?x=y&some=query&string=yeah!",
         @"http://someApp.com?x=y",
         @"someApi/with/a/path/and?some=query&string=yeah!"],
    
      // Whitespace and path in App URL
      @[ @"http://someApp.com/some%20path/api/some%20api",
         @"http://someApp.com/some path",
         @"some api"]
    ];
    
    for (id testCase in testCases) {
        
        NSString *expectedURL = [testCase objectAtIndex:0];
        NSString *appURL = [testCase objectAtIndex:1];
        NSString *APIName = [testCase objectAtIndex:2];
        
        MSClient *client = [MSClient clientWithApplicationURLString:appURL];
        
        NSURL *url = [MSURLBuilder URLForApi:client
                                     APIName:APIName
                                  parameters:nil
                                     orError:nil];
        
        STAssertNotNil(url, @"url should not be nil");
        STAssertTrue([[url absoluteString] isEqualToString:expectedURL],
                     @"the url was: %@", [url absoluteString]);
    }
}

-(void) testURLForApiWithParameters
{
    // For each test case: the first element is the expected  URL;
    // the second element is the app format URL; and third element is
    // the table name and the forth element is the parameters
    NSArray *testCases = @[
    
      // Vanilla test case
      @[ @"http://someApp.com/api/someApi?x=5",
         @"http://someApp.com",
         @"someApi",
         @{@"x" : @5}],
    
      // Parameters that should be URL encoded
      @[ @"http://someApp.com/api/some/api/with/a/path?x=%23%3F%26$'%20encode%20me%21",
         @"http://someApp.com/",
         @"some/api/with/a/path",
         @{@"x" : @"#?&$' encode me!"}],
    
      // App URL with query string
      @[ @"http://someApp.com/api/someApi/with/a/path/and?x=y&some=query&string=yeah!&x=5",
         @"http://someApp.com?x=y",
         @"someApi/with/a/path/and?some=query&string=yeah!",
         @{@"x" : @5}],
    
      // Whitespace and path in App URL
      @[ @"http://someApp.com/some%20path/api/some%20api?%26encode=5",
         @"http://someApp.com/some path",
         @"some api",
         @{@"&encode" : @5}]
    ];
    
    for (id testCase in testCases) {
        
        NSString *expectedURL = [testCase objectAtIndex:0];
        NSString *appURL = [testCase objectAtIndex:1];
        NSString *APIName = [testCase objectAtIndex:2];
        NSDictionary *parameters = [testCase objectAtIndex:3];
        
        MSClient *client = [MSClient clientWithApplicationURLString:appURL];
        
        NSURL *url = [MSURLBuilder URLForApi:client
                                     APIName:APIName
                                  parameters:parameters
                                     orError:nil];
        
        STAssertNotNil(url, @"url should not be nil");
        STAssertTrue([[url absoluteString] isEqualToString:expectedURL],
                     @"the url was: %@", [url absoluteString]);
    }
}


-(void) testQueryStringFromQueryEscapesPredicateConstants
{
    NSString *expectedQueryString = @"$filter=(title%20eq%20'%23%3F%26$%20encode%20me%21')&$inlinecount=none";
    NSString *appURL = @"http:\\someApp.com";
    NSString *tableName = @"someTable";
    NSString *predicateString = @"title == '#?&$ encode me!'";
    
    MSClient *client = [MSClient clientWithApplicationURLString:appURL];
    MSTable *table = [client tableWithName:tableName];
    
    NSPredicate *predicate = [NSPredicate predicateWithFormat:predicateString];
    
    MSQuery *query = [[MSQuery alloc] initWithTable:table predicate:predicate];
    
    NSString *queryString = [MSURLBuilder queryStringFromQuery:query
                                                            orError:nil];
    
    STAssertNotNil(queryString, @"queryString should not be nil");
    STAssertTrue([queryString isEqualToString:expectedQueryString],
                 @"the queryString was: %@", queryString);
    
}

-(void) testQueryStringFromQueryValidatesUserParameters
{
    NSError *error = nil;
    NSString *appURL = @"http:\\someApp.com";
    NSString *tableName = @"someTable";
    NSString *predicateString = @"title == '#?&$ encode me!'";
    
    MSClient *client = [MSClient clientWithApplicationURLString:appURL];
    MSTable *table = [client tableWithName:tableName];
    
    NSPredicate *predicate = [NSPredicate predicateWithFormat:predicateString];
    
    MSQuery *query = [[MSQuery alloc] initWithTable:table predicate:predicate];
    query.parameters = @{
        @"key1": @"someValue",
        @"$key2": @"14",
    };

    NSString *queryString = [MSURLBuilder queryStringFromQuery:query
                                                            orError:&error];
    
    STAssertNil(queryString, @"queryString should be nil");
    STAssertNotNil(error, @"error should not have been nil.");
    STAssertTrue(error.domain == MSErrorDomain,
                 @"error domain should have been MSErrorDomain.");
    STAssertTrue(error.code == MSInvalidUserParameterWithRequest,
                 @"error code should have been MSInvalidUserParameterWithRequest.");
    
    NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
    STAssertTrue([description isEqualToString:@"'$key2' is an invalid user-defined query string parameter. User-defined query string parameters must not begin with a '$'."],
                 @"description was: %@", description);}

@end
