// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <XCTest/XCTest.h>
#import "MSTable.h"
#import "MSTestFilter.h"
#import "MSQuery.h"
#import "MSTable+MSTableTestUtilities.h"
#import "MSSDKFeatures.h"

@interface MSTableTests : XCTestCase {
    MSClient *client;
    BOOL done;
}

@end


@implementation MSTableTests


#pragma mark * Setup and TearDown


-(void) setUp
{
    NSLog(@"%@ setUp", self.name);
    
    client = [MSClient clientWithApplicationURLString:@"https://someUrl/"];
    
    done = NO;
}

-(void) tearDown
{
    NSLog(@"%@ tearDown", self.name);
}


#pragma mark * Init Method Tests


-(void) testInitWithNameAndClient
{
    MSTable *table = [[MSTable alloc] initWithName:@"SomeName" client:client];
    
    XCTAssertNotNil(table, @"table should not be nil.");
    
    XCTAssertNotNil(table.client, @"table.client should not be nil.");
    XCTAssertTrue([table.name isEqualToString:@"SomeName"],
                 @"table.name shouldbe 'SomeName'");
}

-(void) testInitWithNameAndClientAllowsNil
{
    MSTable *table = [[MSTable alloc] initWithName:nil client:nil];
    
    XCTAssertNotNil(table, @"table should not be nil.");
    
    XCTAssertNil(table.client, @"table.client should be nil.");
    XCTAssertNil(table.name, @"table.name should be nil.");
}


#pragma mark * Insert Method Tests


// See the WindowsAzureMobileServicesFunctionalTests.m tests for additional
// insert tests that require a working Microsoft Azure Mobile Service.

-(void) testInsertItem
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    NSString* stringData = @"{\"id\": 120, \"name\":\"test name\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    testFilter.responseToUse = response;
    testFilter.dataToUse = data;
    testFilter.ignoreNextFilter = YES;
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"NoSuchTable"];
    
    // Create the item
    id item = @{ @"name":@"test name" };
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNotNil(item, @"item should not have  been nil.");
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue([[item valueForKey:@"name"] isEqualToString:@"test name"],
                     @"item should have been inserted.");
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInsertItemWithNilItem
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Insert the item
    [todoTable insert:nil completion:^(NSDictionary *item, NSError *error) {
    
        XCTAssertNil(item, @"item should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSExpectedItemWithRequest,
                     @"error code should have been MSExpectedItemWithRequest.");
        
        NSString *description = error.localizedDescription;
        XCTAssertTrue([description isEqualToString:@"No item was provided."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInsertItemWithInvalidItem
{
    MSTable *todoTable = [client tableWithName:@"NoSuchTable"];
    
    // Create the item
    id item = [[NSDate alloc] initWithTimeIntervalSinceReferenceDate:0.0];
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
    
        XCTAssertNil(item, @"item should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSInvalidItemWithRequest,
                     @"error code should have been MSInvalidItemWithRequest.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"The item provided was not valid."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInsertItemWithIdZero
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    NSString* stringData = @"{\"id\": 120, \"name\":\"test name\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    testFilter.responseToUse = response;
    testFilter.dataToUse = data;
    testFilter.ignoreNextFilter = YES;
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"NoSuchTable"];
    
    // Create the item
    id item = @{ @"id":@0, @"name":@"test name" };
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNotNil(item, @"item should not have  been nil.");
        XCTAssertNil(error, @"error should have been nil.");
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInsertItemWithStringId
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    NSString* stringData = @"{\"id\": \"120\", \"name\":\"test name\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    testFilter.responseToUse = response;
    testFilter.dataToUse = data;
    testFilter.ignoreNextFilter = YES;
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"NoSuchTable"];
    
    // Create the item
    id item = @{ @"id":@"120", @"name":@"test name" };
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNotNil(item, @"item should not have  been nil.");
        XCTAssertNil(error, @"error should have been nil.");
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInsertItemWithNullId
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    NSString* stringData = @"{\"id\": \"120\", \"name\":\"test name\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    testFilter.responseToUse = response;
    testFilter.dataToUse = data;
    testFilter.ignoreNextFilter = YES;
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"NoSuchTable"];
    
    // Create the item
    id item = @{ @"id":[NSNull null], @"name":@"test name" };
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNotNil(item, @"item should not have  been nil.");
        XCTAssertNil(error, @"error should have been nil.");
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInsertItemWithEmptyStringId
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    NSString* stringData = @"{\"id\": \"120\", \"name\":\"test name\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    testFilter.responseToUse = response;
    testFilter.dataToUse = data;
    testFilter.ignoreNextFilter = YES;
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"NoSuchTable"];
    
    // Create the item
    id item = @{ @"id":@"", @"name":@"test name" };
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNotNil(item, @"item should not have  been nil.");
        XCTAssertNil(error, @"error should have been nil.");
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInsertHasContentType
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    __block NSString *contentType = nil;
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:400
                                   HTTPVersion:nil headerFields:nil];
    testFilter.responseToUse = response;
    testFilter.ignoreNextFilter = YES;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        contentType = [request valueForHTTPHeaderField:@"Content-Type"];
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"todoItem"];
    
    // Create the item
    id item = @{ @"id":@0, @"name":@"test name" };
    
    // insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        
        XCTAssertNotNil(contentType, @"Content-Type should not have been nil.");
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInsertStripsSystemProperties
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    MSInspectRequestBlock inspectBlock = ^NSURLRequest *(NSURLRequest *request) {
         testFilter.responseToUse = [[NSHTTPURLResponse alloc]
                                    initWithURL:request.URL
                                    statusCode:200
                                    HTTPVersion:nil headerFields:nil];
        
        return request;
    };
    
    NSString* stringData = @"{\"id\": \"A\", \"name\":\"test name\", \"__version\":\"ABC\", \"__createdAt\":\"12-01-01\",\"__unknown\":123}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    testFilter.dataToUse = data;
    testFilter.ignoreNextFilter = YES;
    testFilter.onInspectRequest =  [inspectBlock copy];
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"NoSuchTable"];
    
    // Create the item
    id item = @{ @"name":@"test name" };
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(error);
        XCTAssertNotNil(item);

        XCTAssertNil(item[MSSystemColumnVersion]);
        XCTAssertNil(item[MSSystemColumnCreatedAt]);
        XCTAssertNil(item[@"__unknown"]);

        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");    
    done = NO;
    
    // Allow some through table enum
    todoTable.systemProperties = MSSystemPropertyCreatedAt | MSSystemPropertyVersion;
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(error);
        XCTAssertNotNil(item);
        
        XCTAssertNotNil(item[MSSystemColumnVersion]);
        XCTAssertNotNil(item[MSSystemColumnCreatedAt]);
        XCTAssertNil(item[@"__unknown"]);
        
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    done = NO;
    
    // Allow all now
    todoTable.systemProperties = MSSystemPropertyAll;
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(error);
        XCTAssertNotNil(item);
        
        XCTAssertNotNil(item[MSSystemColumnVersion]);
        XCTAssertNotNil(item[MSSystemColumnCreatedAt]);
        XCTAssertNotNil(item[@"__unknown"]);
        
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    done = NO;
    
    // Now using the querystring instead
    todoTable.systemProperties = MSSystemPropertyNone;
    [todoTable insert:item
           parameters:@{@"__systemProperties":MSSystemColumnCreatedAt}
           completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(error);
        XCTAssertNotNil(item);
        
        XCTAssertNil(item[MSSystemColumnVersion]);
        XCTAssertNotNil(item[MSSystemColumnCreatedAt]);
        XCTAssertNil(item[@"__unknown"]);
        
        done = YES;
    }];
    
    // And check int Ids keep them all still
    stringData = @"{\"id\": 123, \"name\":\"test name\", \"__version\":\"ABC\", \"__createdAt\":\"12-01-01\",\"__unknown\":123}";
    testFilter.dataToUse = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(error);
        XCTAssertNotNil(item);

        XCTAssertNotNil(item[MSSystemColumnVersion]);
        XCTAssertNotNil(item[MSSystemColumnCreatedAt]);
        XCTAssertNotNil(item[@"__unknown"]);

        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}


#pragma mark * Update Method Tests


// See the WindowsAzureMobileServicesFunctionalTests.m tests for additional
// update tests that require a working Microsoft Azure Mobile Service.

-(void) testUpdateItemWithIntId
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    NSString* stringData = @"{\"id\":120, \"name\":\"test name updated\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    testFilter.responseToUse = response;
    testFilter.dataToUse = data;
    testFilter.ignoreNextFilter = YES;
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"NoSuchTable"];
    
    // Create the item
    id item = @{ @"id":@120, @"name":@"test name" };
    
    // Insert the item
    [todoTable update:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNotNil(item, @"item should not have  been nil.");
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue([[item valueForKey:@"name"] isEqualToString:@"test name updated"],
                       @"item should have been updated.");
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testUpdateItemWithStringId
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    NSString* stringData = @"{\"id\":\"120\", \"name\":\"test name updated\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    testFilter.responseToUse = response;
    testFilter.dataToUse = data;
    testFilter.ignoreNextFilter = YES;
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"NoSuchTable"];
    
    // Create the item
    id item = @{ @"id":@"120", @"name":@"test name" };
    
    // Insert the item
    [todoTable update:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNotNil(item, @"item should not have  been nil.");
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue([[item valueForKey:@"name"] isEqualToString:@"test name updated"],
                     @"item should have been updated.");
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testUpdateItemWithNilItem
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Update the item
    [todoTable update:nil completion:^(NSDictionary *item, NSError *error) {
    
        XCTAssertNil(item, @"item should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSExpectedItemWithRequest,
                     @"error code should have been MSExpectedItemWithRequest.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"No item was provided."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testUpdateItemWithInvalidItem
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    id item = [[NSDate alloc] initWithTimeIntervalSinceReferenceDate:0.0];
    
    // Update the item
    [todoTable update:item completion:^(NSDictionary *item, NSError *error) {
        
        XCTAssertNil(item, @"item should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSInvalidItemWithRequest,
                     @"error code should have been MSInvalidItemWithRequest.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"The item provided was not valid."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testUpdateItemWithNoItemId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write unit tests!", @"complete": @(NO) };
    
    // Update the item
    [todoTable update:item completion:^(NSDictionary *item, NSError *error) {
  
        XCTAssertNil(item, @"item should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSMissingItemIdWithRequest,
                     @"error code should have been MSMissingItemIdWithRequest.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"The item provided did not have an id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testUpdateItemWithEmptyStringItemId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write unit tests!", @"id":@"" };
    
    // Update the item
    [todoTable update:item completion:^(NSDictionary *item, NSError *error) {
    
        XCTAssertNil(item, @"item should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testUpdateItemWithwhiteSpaceItemId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write unit tests!", @"id":@"  " };
    
    // Update the item
    [todoTable update:item completion:^(NSDictionary *item, NSError *error) {
        
        XCTAssertNil(item, @"item should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testUpdateItemWithItemIdZero
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write unit tests!", @"id":@0 };
    
    // Update the item
    [todoTable update:item completion:^(NSDictionary *item, NSError *error) {
        
        XCTAssertNil(item, @"item should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testUpdateStripsSystemProperties
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    MSInspectRequestBlock inspectBlock = ^NSURLRequest *(NSURLRequest *request) {
        testFilter.responseToUse = [[NSHTTPURLResponse alloc]
                                    initWithURL:request.URL
                                    statusCode:200
                                    HTTPVersion:nil headerFields:@{@"Etag":@"\"AAAAAAAALNU=\""}];
        
        return request;
    };
    
    NSString* stringData = @"{\"id\": \"A\", \"name\":\"test name\", \"__version\":\"ABC\", \"__createdAt\":\"12-01-01\",\"__unknown\":123}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    //testFilter.responseToUse = response;
    testFilter.dataToUse = data;
    testFilter.ignoreNextFilter = YES;
    testFilter.onInspectRequest =  [inspectBlock copy];
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"NoSuchTable"];
    
    // Create the item
    id item = @{ @"id": @"A", @"name":@"test name" };
    
    // Insert the item
    [todoTable update:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(error);
        XCTAssertNotNil(item);
        
        XCTAssertNil(item[MSSystemColumnVersion]);
        XCTAssertNil(item[MSSystemColumnCreatedAt]);
        XCTAssertNil(item[@"__unknown"]);
        
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    done = NO;
    
    // Allow some through table enum
    todoTable.systemProperties = MSSystemPropertyCreatedAt | MSSystemPropertyVersion;
    [todoTable update:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(error);
        XCTAssertNotNil(item);
        
        XCTAssertNotNil(item[MSSystemColumnVersion]);
        XCTAssertNotNil(item[MSSystemColumnCreatedAt]);
        XCTAssertNil(item[@"__unknown"]);
        
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    done = NO;
    
    // Allow all now
    todoTable.systemProperties = MSSystemPropertyAll;
    [todoTable update:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(error);
        XCTAssertNotNil(item);
        
        XCTAssertNotNil(item[MSSystemColumnVersion]);
        XCTAssertNotNil(item[MSSystemColumnCreatedAt]);
        XCTAssertNotNil(item[@"__unknown"]);
        
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    done = NO;
    
    // Now using the querystring instead
    todoTable.systemProperties = MSSystemPropertyNone;
    [todoTable update:item
           parameters:@{@"__systemProperties":MSSystemColumnCreatedAt}
           completion:^(NSDictionary *item, NSError *error) {
               XCTAssertNil(error);
               XCTAssertNotNil(item);
               
               XCTAssertNil(item[MSSystemColumnVersion]);
               XCTAssertNotNil(item[MSSystemColumnCreatedAt]);
               XCTAssertNil(item[@"__unknown"]);
               
               done = YES;
           }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    done = NO;
    
    // And check int Ids keep them all still
    stringData = @"{\"id\": 123, \"name\":\"test name\", \"__version\":\"ABC\", \"__createdAt\":\"12-01-01\",\"__unknown\":123}";
    item = @{ @"id": @123, @"name":@"test name" };
    testFilter.dataToUse = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    [todoTable update:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(error);
        XCTAssertNotNil(item);
        
        XCTAssertNotNil(item[MSSystemColumnVersion]);
        XCTAssertNotNil(item[MSSystemColumnCreatedAt]);
        XCTAssertNotNil(item[@"__unknown"]);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}


#pragma mark * Delete Method Tests


// See the WindowsAzureMobileServicesFunctionalTests.m tests for additional
// delete tests that require a working Microsoft Azure Mobile Service.


-(void) testDeleteItemWithIntId
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    testFilter.responseToUse = response;
    testFilter.ignoreNextFilter = YES;
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"NoSuchTable"];
    
    // Create the item
    id item = @{ @"id":@120, @"name":@"test name" };
    
    // Insert the item
    [todoTable delete:item completion:^(id itemId, NSError *error) {
        XCTAssertNotNil(itemId, @"item should not have  been nil.");
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue([itemId isEqualToNumber:@120],
                     @"item should have been inserted.");
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithStringId
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    testFilter.responseToUse = response;
    testFilter.ignoreNextFilter = YES;
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"NoSuchTable"];
    
    // Create the item
    id item = @{ @"id":@"120", @"name":@"test name" };
    
    // Insert the item
    [todoTable delete:item completion:^(id itemId, NSError *error) {
        XCTAssertNotNil(itemId, @"item should not have  been nil.");
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue([itemId isEqualToString:@"120"],
                     @"item should have been inserted.");
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithStringIdConflict
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    NSString* stringData = @"{\"id\": 120, \"name\":\"test name\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:412
                                   HTTPVersion:nil headerFields:nil];
    testFilter.responseToUse = response;
    testFilter.ignoreNextFilter = YES;
    testFilter.dataToUse = data;
    
    testFilter.onInspectRequest = ^(NSURLRequest *request) {
        NSString *ifMatchHeader = request.allHTTPHeaderFields[@"If-Match"];
        XCTAssertEqualObjects(ifMatchHeader, @"\"123\"", @"Unexpected header");
        return request;
    };
    
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"NoSuchTable"];
    
    // Create the item
    id item = @{ @"id":@"120", MSSystemColumnVersion:@"123", @"name":@"test name" };
    
    // Test deletion of the item
    [todoTable delete:item completion:^(id itemId, NSError *error) {
        XCTAssertNil(itemId, @"item should have been nil.");
        XCTAssertEqual(error.code, [@MSErrorPreconditionFailed integerValue], @"Error should be precondition");
        NSDictionary* serverItem =(error.userInfo)[MSErrorServerItemKey];
        XCTAssertEqualObjects(serverItem[@"id"], @120, @"id portion of ServerItem was not expected value");
        XCTAssertEqualObjects(serverItem[@"name"], @"test name", @"name portion of ServerItem was not expected value");
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithStringIdConflictWithEmptyJsonError
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    NSString* stringData = @"{}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:412
                                   HTTPVersion:nil headerFields:nil];
    testFilter.responseToUse = response;
    testFilter.ignoreNextFilter = YES;
    testFilter.dataToUse = data;
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"NoSuchTable"];
    
    // Create the item
    id item = @{ @"id":@"120", @"name":@"test name" };
    
    // Test deletion of the item
    [todoTable delete:item completion:^(id itemId, NSError *error) {
        XCTAssertNil(itemId, @"item should have been nil.");
        XCTAssertEqual(error.code, [@MSErrorPreconditionFailed integerValue], @"Error should be precondition");
        NSDictionary* serverItem =(error.userInfo)[MSErrorServerItemKey];
        XCTAssertTrue(serverItem.count == 0, @"empty JSON object error has no members in userInfo");
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithNilItem
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Update the item
    [todoTable delete:nil completion:^(id itemId, NSError *error) {
  
        XCTAssertNil(itemId, @"itemId should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSExpectedItemWithRequest,
                     @"error code should have been MSExpectedItemWithRequest.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"No item was provided."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithInvalidItem
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    id item = [[NSDate alloc] initWithTimeIntervalSinceReferenceDate:0.0];
    
    // Delete the item
    [todoTable delete:item completion:^(id itemId, NSError *error) {
        
        XCTAssertNil(itemId, @"itemId should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSInvalidItemWithRequest,
                     @"error code should have been MSInvalidItemWithRequest.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"The item provided was not valid."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithNoItemId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write unit tests!", @"complete": @(NO) };
    
    // Delete the item
    [todoTable delete:item completion:^(id itemId, NSError *error) {
    
        XCTAssertNil(itemId, @"itemId should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSMissingItemIdWithRequest,
                     @"error code should have been MSMissingItemIdWithRequest.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"The item provided did not have an id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithInvalidItemId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write unit tests!", @"id":@0 };
    
    // Delete the item
    [todoTable delete:item completion:^(id itemId, NSError *error) {
        
        XCTAssertNil(itemId, @"itemId should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithEmptyStringId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write unit tests!", @"id":@"" };
    
    // Delete the item
    [todoTable delete:item completion:^(id itemId, NSError *error) {
        
        XCTAssertNil(itemId, @"itemId should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithWhiteSpaceId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write unit tests!", @"id":@"  " };
    
    // Delete the item
    [todoTable delete:item completion:^(id itemId, NSError *error) {
        
        XCTAssertNil(itemId, @"itemId should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithItemIdZero
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write unit tests!", @"id":@0 };
    
    // Delete the item
    [todoTable delete:item completion:^(id itemId, NSError *error) {
        
        XCTAssertNil(itemId, @"itemId should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
}

-(void) testDeleteItemWithIdwithIntId
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    testFilter.responseToUse = response;
    testFilter.ignoreNextFilter = YES;
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"NoSuchTable"];

    // Insert the item
    [todoTable deleteWithId:@120 completion:^(id itemId, NSError *error) {
        XCTAssertNotNil(itemId, @"item should not have  been nil.");
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue([itemId isEqualToNumber:@120],
                     @"item should have been inserted.");
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithIdwithStringId
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    testFilter.responseToUse = response;
    testFilter.ignoreNextFilter = YES;
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"NoSuchTable"];
    
    // Insert the item
    [todoTable deleteWithId:@"120" completion:^(id itemId, NSError *error) {
        XCTAssertNotNil(itemId, @"item should not have  been nil.");
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue([itemId isEqualToString:@"120"],
                     @"item should have been inserted.");
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithIdWithNoItemId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];

    // Delete the item
    [todoTable deleteWithId:nil completion:^(id itemId, NSError *error) {
    
        XCTAssertNil(itemId, @"itemId should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSExpectedItemIdWithRequest,
                     @"error code should have been MSExpectedItemIdWithRequest.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"The item id was not provided."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithIdWithInvalidItemId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    id itemId = [[NSDate alloc] initWithTimeIntervalSince1970:0.0];
    
    // Delete the item
    [todoTable deleteWithId:itemId completion:^(id itemId, NSError *error) {
        
        XCTAssertNil(itemId, @"itemId should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithIdWithIdZero
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
        
    // Delete the item
    [todoTable deleteWithId:@0 completion:^(id itemId, NSError *error) {
        
        XCTAssertNil(itemId, @"itemId should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithIdWithEmptyStringId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Delete the item
    [todoTable deleteWithId:@"" completion:^(id itemId, NSError *error) {
        
        XCTAssertNil(itemId, @"itemId should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithIdWithWhiteSpaceId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Delete the item
    [todoTable deleteWithId:@" " completion:^(id itemId, NSError *error) {
        
        XCTAssertNil(itemId, @"itemId should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteDoesNotHaveContentType
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    __block NSString *contentType = nil;
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:400
                                   HTTPVersion:nil headerFields:nil];
    testFilter.responseToUse = response;
    testFilter.ignoreNextFilter = YES;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        contentType = [request valueForHTTPHeaderField:@"Content-Type"];
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"todoItem"];
    
    // delete the item
    [todoTable deleteWithId:@5 completion:^(id itemId, NSError *error) {
  
        XCTAssertNil(contentType, @"Content-Type should not have been set.");
    
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}


#pragma mark * UndeleteItem Method Tests


-(void) testUnDeleteItemWithStringId
{
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:200 data:@"{\"id\":\"an id\",\"String\":\"Hey\"}"];

    __block NSURLRequest *actualRequest;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        actualRequest = request;
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"NoSuchTable"];
    
    // Create the item
    id item = @{ @"id":@"ID-ABC", @"name":@"test name" };
    
    // Insert the item
    [todoTable undelete:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertEqualObjects(actualRequest.HTTPMethod, @"POST", @"Expected undelete to send a POST, not %@", actualRequest.HTTPMethod);
        XCTAssertEqualObjects(actualRequest.URL.absoluteString, @"https://someUrl/tables/NoSuchTable/ID-ABC", @"Unexpected URL");
        
        XCTAssertNil(error, @"error should have been nil.");
        
        XCTAssertNotNil(item, @"item should not have  been nil.");
        XCTAssertEqualObjects(item[@"id"], @"an id", @"item id should have come from server.");
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testUnDeleteItemWithParametersWithStringId
{
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:200 data:@"{\"id\":\"an id\",\"String\":\"Hey\", \"__version\":\"def\"}"];
    
    __block NSURLRequest *actualRequest;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        actualRequest = request;
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"NoSuchTable"];
    
    // Create the item
    id item = @{ @"id":@"ID-ABC", @"name":@"test name", MSSystemColumnVersion: @"abc" };
    
    // Insert the item
    [todoTable undelete:item parameters:@{@"extra-extra": @"read-all-about-it"} completion:^(NSDictionary *item, NSError *error) {
        XCTAssertEqualObjects(actualRequest.HTTPMethod, @"POST", @"Expected undelete to send a POST, not %@", actualRequest.HTTPMethod);
        XCTAssertEqualObjects(actualRequest.URL.absoluteString, @"https://someUrl/tables/NoSuchTable/ID-ABC?extra-extra=read-all-about-it", @"Unexpected URL");
        XCTAssertEqualObjects(actualRequest.allHTTPHeaderFields[@"If-Match"], @"\"abc\"", @"Missing if-match header");
                                          
        XCTAssertNil(error, @"error should have been nil.");
        
        XCTAssertNotNil(item, @"item should not have  been nil.");
        XCTAssertEqualObjects(item[@"id"], @"an id", @"item id should have come from server.");
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}


-(void) testUnDeleteItemWithNoItemId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write unit tests!", @"complete": @(NO) };
    
    // Update the item
    [todoTable undelete:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(item, @"item should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertEqualObjects(error.domain, MSErrorDomain);
        XCTAssertEqual(error.code, MSMissingItemIdWithRequest);
        NSString *description = error.userInfo[NSLocalizedDescriptionKey];
        XCTAssertEqualObjects(description, @"The item provided did not have an id.");
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}


#pragma mark * ReadWithId Method Tests


// See the WindowsAzureMobileServicesFunctionalTests.m tests for additional
// readWithId tests that require a working Microsoft Azure Mobile Service.

-(void) testReadItemWithIntId
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    NSString* stringData = @"{\"id\": 120, \"name\":\"test name\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    testFilter.responseToUse = response;
    testFilter.dataToUse = data;
    testFilter.ignoreNextFilter = YES;
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"NoSuchTable"];

    // Insert the item
    [todoTable readWithId:@120 completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNotNil(item, @"item should not have  been nil.");
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue([[item valueForKey:@"id"] isEqualToNumber:@120],
                     @"item should have been read.");
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testReadItemWithStringId
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    NSString* stringData = @"{\"id\": \"120\", \"name\":\"test name\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    testFilter.responseToUse = response;
    testFilter.dataToUse = data;
    testFilter.ignoreNextFilter = YES;
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"NoSuchTable"];
    
    // Insert the item
    [todoTable readWithId:@"120" completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNotNil(item, @"item should not have  been nil.");
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue([[item valueForKey:@"id"] isEqualToString:@"120"],
                     @"item should have been read.");
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testReadItemWithIdWithNoItemId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Read the item
    [todoTable readWithId:nil completion:^(NSDictionary *item, NSError *error) {
    
        XCTAssertNil(item, @"item should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSExpectedItemIdWithRequest,
                     @"error code should have been MSExpectedItemIdWithRequest.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"The item id was not provided."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testReadItemWithIdWithInvalidItemId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    id itemId = [[NSDate alloc] initWithTimeIntervalSince1970:0.0];
    
    // Read the item
    [todoTable readWithId:itemId completion:^(NSDictionary *item, NSError *error) {
     
        XCTAssertNil(item, @"item should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testReadItemWithIdWithIdZero
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];

    // Read the item
    [todoTable readWithId:@0 completion:^(NSDictionary *item, NSError *error) {
        
        XCTAssertNil(item, @"item should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testReadItemWithEmptyStringId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Read the item
    [todoTable readWithId:@"" completion:^(NSDictionary *item, NSError *error) {
        
        XCTAssertNil(item, @"item should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testReadItemWithWhiteSpaceId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Read the item
    [todoTable readWithId:@"  " completion:^(NSDictionary *item, NSError *error) {
        
        XCTAssertNil(item, @"item should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testReadItemWithIdDoesNotHaveContentType
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    __block NSString *contentType = nil;
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:400
                                   HTTPVersion:nil headerFields:nil];
    testFilter.responseToUse = response;
    testFilter.ignoreNextFilter = YES;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        contentType = [request valueForHTTPHeaderField:@"Content-Type"];
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"todoItem"];
    
    // read with id
    [todoTable readWithId:@5 completion:^(NSDictionary *item, NSError *error){
        
        XCTAssertNil(contentType, @"Content-Type should not have been set.");
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}


#pragma mark * Query Method Tests


-(void) testQueryReturnsNonNil
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    MSQuery *query = [todoTable query];
    
    XCTAssertNotNil(query, @"query should not have been nil.");    
}

-(void) testQueryWithPredicateReturnsNonNil
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    MSQuery *query = [todoTable queryWithPredicate:nil];
    
    XCTAssertNotNil(query, @"query should not have been nil.");
}

-(void) testQueryDoesNotHaveContentType
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    __block NSString *contentType = nil;
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:400
                                   HTTPVersion:nil headerFields:nil];
    testFilter.responseToUse = response;
    testFilter.ignoreNextFilter = YES;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        contentType = [request valueForHTTPHeaderField:@"Content-Type"];
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"todoItem"];
    NSPredicate *predicate = [NSPredicate predicateWithFormat:@"TRUEPREDICATE"];
    MSQuery *query = [todoTable queryWithPredicate:predicate];
    
    // query
    [query readWithCompletion:^(MSQueryResult *result, NSError *error) {
        
        XCTAssertNil(contentType, @"Content-Type should not have been set.");
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}


#pragma mark System Property Tests


-(void) testInsertStringIdPropertiesNotRemovedFromRequest
{
    __block NSURLRequest *actualRequest = nil;
    NSArray *testProperties = [MSTable testNonSystemProperties];
    testProperties = [testProperties arrayByAddingObjectsFromArray:[MSTable testValidSystemProperties]];
    
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    MSInspectRequestBlock inspectBlock = ^NSURLRequest *(NSURLRequest *request) {
        actualRequest = request;
        
        testFilter.responseToUse = [[NSHTTPURLResponse alloc]
                                    initWithURL:request.URL
                                    statusCode:200
                                    HTTPVersion:nil headerFields:nil];
        
        return request;
    };
    
    for (NSString *property in testProperties)
    {
        NSString *dataString = [NSString stringWithFormat:@"{\"id\":\"an id\",\"%@\":\"a value\",\"string\":\"What?\"}", property];
        testFilter.dataToUse = [dataString dataUsingEncoding:NSUTF8StringEncoding];
        testFilter.ignoreNextFilter = YES;
        testFilter.onInspectRequest =  [inspectBlock copy];
        
        MSClient *filteredClient = [client clientWithFilter:testFilter];
        MSTable *todoTable = [filteredClient tableWithName:@"someTable"];
        todoTable.systemProperties = MSSystemPropertyAll;
        
        NSDictionary *itemToInsert = @{@"id": @"an id", @"string": @"What?", property: @"a value"};
        [todoTable insert:itemToInsert completion:^(NSDictionary *item, NSError *error) {
            NSData *actualBody = actualRequest.HTTPBody;
            NSString *bodyString = [[NSString alloc] initWithData:actualBody
                                                         encoding:NSUTF8StringEncoding];
            XCTAssertTrue([bodyString rangeOfString:property].location != NSNotFound, @"The body was not serialized as expected.");
            XCTAssertEqualObjects(@"a value", item[property], @"Property %@ was removed", property);
            done = YES;
        }];
        XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    }
}

-(void) testInsertNullIdSystemPropertiesNotRemovedFromRequest
{
    __block NSURLRequest *actualRequest = nil;
    NSArray *testSystemProperties = [MSTable testValidSystemProperties];
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    testFilter.ignoreNextFilter = YES;

    MSInspectRequestBlock inspectBlock = ^NSURLRequest *(NSURLRequest *request) {
        actualRequest = request;
        testFilter.responseToUse = [[NSHTTPURLResponse alloc]
                                    initWithURL:request.URL
                                    statusCode:200
                                    HTTPVersion:nil headerFields:nil];
        
        return request;
    };
    
    for (NSString *property in testSystemProperties)
    {
        NSString *dataString = [NSString stringWithFormat:@"{\"id\":\"an id\",\"%@\":\"a value\",\"string\":\"What?\"}", property];
        testFilter.dataToUse = [dataString dataUsingEncoding:NSUTF8StringEncoding];
        testFilter.onInspectRequest = [inspectBlock copy];
        
        MSClient *filteredClient = [client clientWithFilter:testFilter];
        MSTable *todoTable = [filteredClient tableWithName:@"someTable"];
        todoTable.systemProperties = MSSystemPropertyAll;
        
        NSDictionary *itemToInsert = @{@"id": [NSNull null], @"string": @"What?", property: @"a value"};
        [todoTable insert:itemToInsert completion:^(NSDictionary *item, NSError *error) {
            NSData *actualBody = actualRequest.HTTPBody;
            NSString *bodyString = [[NSString alloc] initWithData:actualBody
                                                         encoding:NSUTF8StringEncoding];
            XCTAssertTrue([bodyString rangeOfString:property].location != NSNotFound, @"The body was not serialized as expected.");
            XCTAssertEqualObjects(@"a value", item[property], @"system property %@ was removed", property);
            
            done = YES;
        }];
        XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    }
}

-(void) testInsertNullIdNonSystemPropertiesNotRemovedFromRequest
{
    __block NSURLRequest *actualRequest = nil;
    NSArray *testProperties = [MSTable testNonSystemProperties];
    
    for (NSString *property in testProperties)
    {
        MSTestFilter *testFilter = [[MSTestFilter alloc] init];
        NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                       initWithURL:nil
                                       statusCode:200
                                       HTTPVersion:nil headerFields:nil];
        testFilter.responseToUse = response;
        NSString *dataString = [NSString stringWithFormat:@"{\"id\":\"an id\",\"%@\":\"a value\",\"string\":\"Hey?\"}", property];
        testFilter.dataToUse = [dataString dataUsingEncoding:NSUTF8StringEncoding];
        testFilter.ignoreNextFilter = YES;
        testFilter.onInspectRequest =  ^(NSURLRequest *request) {
            actualRequest = request;
            return request;
        };
        
        MSClient *filteredClient = [client clientWithFilter:testFilter];
        MSTable *todoTable = [filteredClient tableWithName:@"someTable"];
        
        NSDictionary *itemToInsert = @{@"id": [NSNull null], @"string": @"what?", property: @"a value"};
        [todoTable insert:itemToInsert completion:^(NSDictionary *item, NSError *error) {
            NSData *actualBody = actualRequest.HTTPBody;
            NSString *bodyString = [[NSString alloc] initWithData:actualBody
                                                         encoding:NSUTF8StringEncoding];
            XCTAssertTrue([bodyString rangeOfString:property].location != NSNotFound, @"Error: The body was not serialized as expected.");
            XCTAssertEqualObjects(@"a value", item[property], @"Error: Non system property %@ was removed", property);
            done = YES;
        }];
        XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    }
}

-(void) testUpdateAsyncStringIdSystemPropertiesRemovedFromRequest
{
    __block NSURLRequest *actualRequest = nil;
    NSArray *testProperties = [MSTable testValidSystemProperties];
    
    for (NSString *property in testProperties)
    {
        MSTestFilter *testFilter = [[MSTestFilter alloc] init];
        NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                       initWithURL:nil
                                       statusCode:200
                                       HTTPVersion:nil headerFields:nil];
        testFilter.responseToUse = response;
        testFilter.dataToUse = [@"{\"id\":\"an id\",\"String\":\"Hey\"}" dataUsingEncoding:NSUTF8StringEncoding];
        testFilter.ignoreNextFilter = YES;
        testFilter.onInspectRequest =  ^(NSURLRequest *request) {
            actualRequest = request;
            return request;
        };
        
        MSClient *filteredClient = [client clientWithFilter:testFilter];
        MSTable *todoTable = [filteredClient tableWithName:@"someTable"];
        
        NSDictionary *itemToInsert = @{@"id": @"an id", @"string": @"What?", property: @"a value"};
        [todoTable update:itemToInsert completion:^(NSDictionary *item, NSError *error) {
            NSData *actualBody = actualRequest.HTTPBody;
            NSString *bodyString = [[NSString alloc] initWithData:actualBody
                                                         encoding:NSUTF8StringEncoding];
            XCTAssertEqualObjects(bodyString, @"{\"id\":\"an id\",\"string\":\"What?\"}",
                                 @"The body was not serialized as expected.");
            
            done = YES;
        }];
        XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    }
}

-(void) testUpdateStringIdNonSystemPropertiesNotRemovedFromRequest
{
    __block NSURLRequest *actualRequest = nil;
    NSArray *testProperties = [MSTable testNonSystemProperties];
    
    for (NSString *property in testProperties)
    {
        MSTestFilter *testFilter = [[MSTestFilter alloc] init];
        NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                       initWithURL:nil
                                       statusCode:200
                                       HTTPVersion:nil headerFields:nil];
        testFilter.responseToUse = response;
        
        NSString *dataString = [NSString stringWithFormat:@"{\"id\":\"an id\",\"%@\":\"a value\",\"string\":\"Hey\"}", property];
        testFilter.dataToUse = [dataString dataUsingEncoding:NSUTF8StringEncoding];
        testFilter.ignoreNextFilter = YES;
        testFilter.onInspectRequest =  ^(NSURLRequest *request) {
            actualRequest = request;
            return request;
        };
        
        MSClient *filteredClient = [client clientWithFilter:testFilter];
        MSTable *todoTable = [filteredClient tableWithName:@"someTable"];
        
        NSDictionary *itemToInsert = @{@"id": @"an id", @"string": @"What?", property: @"a value"};
        [todoTable insert:itemToInsert completion:^(NSDictionary *item, NSError *error) {
            NSData *actualBody = actualRequest.HTTPBody;
            NSString *bodyString = [[NSString alloc] initWithData:actualBody
                                                         encoding:NSUTF8StringEncoding];
            XCTAssertTrue([bodyString rangeOfString:property].location != NSNotFound, @"The body was not serialized as expected.");
            XCTAssertEqualObjects(@"a value", item[property], @"Non system property %@ was removed", property);
            done = YES;
        }];
        XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    }
}

-(void) testUpdateIntegerIdNoPropertiesRemovedFromRequest
{
    __block NSURLRequest *actualRequest = nil;
    NSArray *testProperties = [MSTable testNonSystemProperties];
    testProperties = [testProperties arrayByAddingObjectsFromArray:[MSTable testValidSystemProperties]];
    
    for (NSString *property in testProperties)
    {
        MSTestFilter *testFilter = [[MSTestFilter alloc] init];
        NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                       initWithURL:nil
                                       statusCode:200
                                       HTTPVersion:nil headerFields:nil];
        testFilter.responseToUse = response;
        
        NSString *dataString = [NSString stringWithFormat:@"{\"id\":5,\"%@\":\"a value\",\"string\":\"Hey\"}", property];
        testFilter.dataToUse = [dataString dataUsingEncoding:NSUTF8StringEncoding];
        testFilter.ignoreNextFilter = YES;
        testFilter.onInspectRequest =  ^(NSURLRequest *request) {
            actualRequest = request;
            return request;
        };
        
        MSClient *filteredClient = [client clientWithFilter:testFilter];
        MSTable *todoTable = [filteredClient tableWithName:@"someTable"];
        
        NSDictionary *itemToUpdate = @{@"id": @5, @"string": @"What?", property: @"a value"};
        [todoTable update:itemToUpdate completion:^(NSDictionary *item, NSError *error) {
            NSData *actualBody = actualRequest.HTTPBody;
            NSString *bodyString = [[NSString alloc] initWithData:actualBody
                                                         encoding:NSUTF8StringEncoding];
            XCTAssertTrue([bodyString rangeOfString:property].location != NSNotFound,
                         @"The body was not serialized as expected: %@", bodyString);
            XCTAssertEqualObjects(@"a value", item[property], @"Property %@ was removed", property);
            done = YES;
        }];
        XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    }
}

- (BOOL) checkRequestURL:(NSURL *)url SystemProperty:(MSSystemProperties) property
{
    NSString *query = [url query];
    
    BOOL result = YES;
    if (property == MSSystemPropertyNone)
    {
        return query == nil || [query rangeOfString:@"__systemProperties"].location == NSNotFound;
    } else if (property == MSSystemPropertyAll) {
        return [query rangeOfString:@"__systemProperties=%2A"].location != NSNotFound;
    }
    
    // Check individual combinations
    if(query == nil || [query rangeOfString:@"__systemProperties"].location == NSNotFound) {
        return NO;
    }
    
    if (property & MSSystemPropertyCreatedAt)
    {
        result = result && [query rangeOfString:@"__createdAt"].location != NSNotFound;
    }
    
    if (property & MSSystemPropertyUpdatedAt)
    {
        result = result &&  [query rangeOfString:@"__updatedAt"].location != NSNotFound;
    }
    
    if (property & MSSystemPropertyVersion)
    {
        result = result &&  [query rangeOfString:@"__version"].location != NSNotFound;
    }
    
    return result;
}

- (void) testReadWithQueryString_ReturnsLinkHeader_IfPresent
{
    [self verifyLinkHeaderOnRead:@"https://contoso.com; rel=next" expectedLink:@"https://contoso.com"];
    [self verifyLinkHeaderOnRead:@"http://contoso.com; rel=next" expectedLink:@"http://contoso.com"];
}

- (void) testReadWithQueryString_ReturnsNil_IfNotPresent
{
    [self verifyLinkHeaderOnRead:@"" expectedLink:nil];
}

- (void) testReadWithQueryString_ReturnsNil_IfWrongFormat
{
    [self verifyLinkHeaderOnRead:@"http://contoso.com" expectedLink:nil];
}

- (void) testReadWithQueryString_ReturnsNil_IfRelIsNotNext
{
    [self verifyLinkHeaderOnRead:@"http://contoso.com; rel=prev" expectedLink:nil];
}

- (void) verifyLinkHeaderOnRead: (NSString *) actualLink  expectedLink: (NSString *) expectedLink {
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil
                                   headerFields:@{@"Link": actualLink}];
    testFilter.responseToUse = response;
    testFilter.ignoreNextFilter = YES;
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"someTable"];
    testFilter.dataToUse = [@"[]" dataUsingEncoding:NSUTF8StringEncoding];
    [todoTable readWithQueryString:@"$filter=1 eq 2" completion:^(MSQueryResult *result, NSError *error) {
        XCTAssertNil(error);
        if (expectedLink == nil) {
            XCTAssertNil(result.nextLink);
        }
        else {
            XCTAssertEqualObjects(result.nextLink, expectedLink);
        }
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testTableOperationSystemPropertiesQueryStringIsCorrect
{
    __block NSURLRequest *actualRequest = nil;
    NSArray *testSystemProperties = [MSTable testSystemProperties];
    
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    testFilter.responseToUse = response;
    testFilter.ignoreNextFilter = YES;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        actualRequest = request;
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"someTable"];
    
    for (NSNumber *systemPropertyAsNumber in testSystemProperties)
    {
        NSUInteger systemProperty = [systemPropertyAsNumber unsignedIntegerValue];
        todoTable.systemProperties = systemProperty;

        // String Id (Insert, Update, ReadWithId, Delete)

        done = NO;
        NSDictionary *item = @{@"id":@"an id",@"String":@"what?"};
        testFilter.dataToUse = [@"{\"id\":\"an id\",\"String\":\"Hey\"}" dataUsingEncoding:NSUTF8StringEncoding];
        [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
            XCTAssertTrue([self checkRequestURL:[actualRequest URL] SystemProperty:systemProperty], @"Error for property %lu with url: %@", (unsigned long)systemProperty, [[actualRequest URL] query]);
            done = YES;
        }];
        XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        done = NO;
        [todoTable update:item completion:^(NSDictionary *item, NSError *error) {
            XCTAssertTrue([self checkRequestURL:[actualRequest URL] SystemProperty:systemProperty], @"Error with url: %@", [[actualRequest URL] query]);
            done = YES;
        }];
        XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        done = NO;
        [todoTable readWithId:item completion:^(NSDictionary *item, NSError *error) {
            XCTAssertTrue([self checkRequestURL:[actualRequest URL] SystemProperty:systemProperty], @"Error with url: %@", [[actualRequest URL] query]);
            done = YES;
        }];
        XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        done = NO;
        [todoTable delete:item completion:^(id itemId, NSError *error) {
            XCTAssertTrue([self checkRequestURL:[actualRequest URL] SystemProperty:systemProperty], @"Error with url: %@", [[actualRequest URL] query]);
            done = YES;
        }];
        XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        // Integer Id
        done = NO;
        item = @{@"String": @"what?"};
        testFilter.dataToUse = [@"{\"id\":5,\"String\":\"Hey\"}" dataUsingEncoding:NSUTF8StringEncoding];
        [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
            XCTAssertTrue([self checkRequestURL:[actualRequest URL] SystemProperty:systemProperty], @"Error with url: %@", [[actualRequest URL] query]);
            done = YES;
        }];
        XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        done = NO;
        item = @{@"id": @5, @"String": @"what?"};
        [todoTable update:item completion:^(NSDictionary *item, NSError *error) {
            XCTAssertTrue([self checkRequestURL:[actualRequest URL] SystemProperty:systemProperty], @"Error with url: %@", [[actualRequest URL] query]);
            done = YES;
        }];
        XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        done = NO;
        [todoTable readWithId:item completion:^(NSDictionary *item, NSError *error) {
            XCTAssertTrue([self checkRequestURL:[actualRequest URL] SystemProperty:systemProperty], @"Error with url: %@", [[actualRequest URL] query]);
            done = YES;
        }];
        XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        done = NO;
        [todoTable delete:item completion:^(id itemId, NSError *error) {
            XCTAssertTrue([self checkRequestURL:[actualRequest URL] SystemProperty:systemProperty], @"Error with url: %@", [[actualRequest URL] query]);
            done = YES;
        }];
        XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");

        // Query
        
        done = NO;
        [todoTable readWithQueryString:@"$filter=id eq 5" completion:^(MSQueryResult *result, NSError *error) {
            XCTAssertTrue([self checkRequestURL:[actualRequest URL] SystemProperty:systemProperty], @"Error with url: %@", [[actualRequest URL] query]);
            done = YES;
        }];
        XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");

        done = NO;
        [todoTable readWithQueryString:@"$select=id,String" completion:^(MSQueryResult *result, NSError *error) {
            XCTAssertTrue([self checkRequestURL:[actualRequest URL] SystemProperty:systemProperty], @"Error with url: %@", [[actualRequest URL] query]);
            done = YES;
        }];
        XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    }
}

-(void) testTableOperationUserParameterWithSystemPropertyQueryStringIsCorrect
{
    __block NSURLRequest *actualRequest = nil;
    NSArray *testSystemProperties = [MSTable testSystemProperties];
    
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    testFilter.responseToUse = response;
    testFilter.ignoreNextFilter = YES;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        actualRequest = request;
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"someTable"];
    
    for (NSNumber *systemProperty in testSystemProperties)
    {
        todoTable.systemProperties = [systemProperty integerValue];
        
        done = NO;
        NSDictionary *item = @{@"id":@"an id", @"string":@"what?"};
        
        testFilter.dataToUse = [@"{\"id\":\"an id\",\"String\":\"Hey\"}" dataUsingEncoding:NSUTF8StringEncoding];
        [todoTable insert:item parameters:@{ @"__systemProperties": @"__createdAt"} completion:^(NSDictionary *item, NSError *error) {
            NSString *url = [[actualRequest URL] query];
            XCTAssertTrue([url rangeOfString:@"__systemProperties=__createdAt"].location != NSNotFound, @"Incorrect query: %@", url);
            done = YES;
        }];
        XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        done = NO;
        [todoTable update:item parameters:@{ @"__systemProperties": @"createdAt"} completion:^(NSDictionary *item, NSError *error) {
            NSString *url = [[actualRequest URL] query];
            XCTAssertTrue([url rangeOfString:@"__systemProperties=createdAt"].location != NSNotFound, @"Incorrect query: %@", url);
            done = YES;
        }];
        XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        done = NO;
        [todoTable readWithId:@"an id" parameters:@{ @"__systemProperties": @"CreatedAt"} completion:^(NSDictionary *item, NSError *error) {
            NSString *url = [[actualRequest URL] query];
            XCTAssertTrue([url rangeOfString:@"__systemProperties=CreatedAt"].location != NSNotFound, @"Incorrect query: %@", url);
            done = YES;
        }];
        XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        done = NO;
        [todoTable delete:item parameters:@{ @"__systemProperties": @"unknown"} completion:^(id itemId, NSError *error) {
            NSString *url = [[actualRequest URL] query];
            XCTAssertTrue([url rangeOfString:@"__systemProperties=unknown"].location != NSNotFound, @"Incorrect query: %@", url);
            done = YES;
        }];
        XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        // Integer Id
        done = NO;
        item = @{@"string":@"what?"};
        testFilter.dataToUse = [@"{\"id\":5,\"String\":\"Hey\"}" dataUsingEncoding:NSUTF8StringEncoding];
        [todoTable insert:item parameters:@{ @"__systemProperties": @"__createdAt"} completion:^(NSDictionary *item, NSError *error) {
            NSString *url = [[actualRequest URL] query];
            XCTAssertTrue([url rangeOfString:@"__systemProperties=__createdAt"].location != NSNotFound, @"Incorrect query: %@", url);
            done = YES;
        }];
        XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        done = NO;
        item = @{@"id":@5, @"string":@"what?"};
        [todoTable update:item parameters:@{ @"__systemProperties": @"createdAt"} completion:^(NSDictionary *item, NSError *error) {
            NSString *url = [[actualRequest URL] query];
            XCTAssertTrue([url rangeOfString:@"__systemProperties=createdAt"].location != NSNotFound, @"Incorrect query: %@", url);
            done = YES;
        }];
        XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        done = NO;
        [todoTable readWithId:@5 parameters:@{ @"__systemProperties": @"CreatedAt"} completion:^(NSDictionary *item, NSError *error) {
            NSString *url = [[actualRequest URL] query];
            XCTAssertTrue([url rangeOfString:@"__systemProperties=CreatedAt"].location != NSNotFound, @"Incorrect query: %@", url);
            done = YES;
        }];
        XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        done = NO;
        [todoTable delete:item parameters:@{ @"__systemProperties": @"unknown"} completion:^(id itemId, NSError *error) {
            NSString *url = [[actualRequest URL] query];
            XCTAssertTrue([url rangeOfString:@"__systemProperties=unknown"].location != NSNotFound, @"Incorrect query: %@", url);
            done = YES;
        }];
        XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        // Query
        
        done = NO;
        [todoTable readWithQueryString:@"$filter=id%20eq%205&__systemproperties=__createdAt" completion:^(MSQueryResult *result, NSError *error) {
            NSString *url = [[actualRequest URL] query];
            XCTAssertEqualObjects(@"$filter=id%20eq%205&__systemproperties=__createdAt", url, @"Incorrect query: %@", url);
            done = YES;
        }];
        XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        done = NO;
        [todoTable readWithQueryString:@"$select=id,String&__systemProperties=__CreatedAt" completion:^(MSQueryResult *result, NSError *error) {
            NSString *url = [[actualRequest URL] query];
            XCTAssertEqualObjects(@"$select=id,String&__systemProperties=__CreatedAt", url, @"Incorrect query: %@", url);
            done = YES;
        }];
        XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    }
}


#pragma mark * Telemetry Features Header Tests

-(void) testQueryAddsProperFeaturesHeader {
    __block NSURLRequest *actualRequest = nil;
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:200 data:@"[]"];
    testFilter.onInspectRequest = ^(NSURLRequest *request) {
        actualRequest = request;
        return request;
    };

    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"NoSuchTable"];

    // Read with raw query
    [todoTable readWithQueryString:@"$filter=a eq 1" completion:^(MSQueryResult *result, NSError *error) {
        XCTAssertNotNil(actualRequest);
        XCTAssertNil(error);

        NSString *featuresHeader = [actualRequest.allHTTPHeaderFields valueForKey:MSFeaturesHeaderName];
        XCTAssertNotNil(featuresHeader);
        XCTAssertTrue([featuresHeader isEqualToString:MSFeatureCodeTableReadRaw], @"Header value (%@) was not as expected (%@)", featuresHeader, MSFeatureCodeTableReadRaw);

        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    actualRequest = nil;
    done = NO;

    // Read with predicate
    [todoTable readWithPredicate:[NSPredicate predicateWithFormat:@"a = 1"] completion:^(MSQueryResult *result, NSError *error) {
        XCTAssertNotNil(actualRequest, @"actualRequest should not have been nil.");
        XCTAssertNil(error, @"error should have been nil.");

        NSString *featuresHeader = [actualRequest.allHTTPHeaderFields valueForKey:MSFeaturesHeaderName];
        XCTAssertNotNil(featuresHeader, @"actualHeader should not have been nil.");
        XCTAssertTrue([featuresHeader isEqualToString:MSFeatureCodeTableReadQuery], @"Header value (%@) was not as expected (%@)", featuresHeader, MSFeatureCodeTableReadQuery);

        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    actualRequest = nil;
    done = NO;

    // Read with query
    MSQuery *query = [todoTable query];
    query.fetchLimit = 10;
    query.fetchOffset = 10;
    [query readWithCompletion:^(MSQueryResult *result, NSError *error) {
        XCTAssertNotNil(actualRequest, @"actualRequest should not have been nil.");
        XCTAssertNil(error, @"error should have been nil.");

        NSString *featuresHeader = [actualRequest.allHTTPHeaderFields valueForKey:MSFeaturesHeaderName];
        XCTAssertNotNil(featuresHeader, @"actualHeader should not have been nil.");
        XCTAssertTrue([featuresHeader isEqualToString:MSFeatureCodeTableReadQuery], @"Header value (%@) was not as expected (%@)", featuresHeader, MSFeatureCodeTableReadQuery);

        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInsertUpdateDeleteAddsProperFeaturesHeader {
    __block NSURLRequest *actualRequest = nil;
    NSString* response = @"{\"id\": \"A\", \"name\":\"test name\", \"__version\":\"ABC\"}";
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:200 data:response];
    testFilter.onInspectRequest = ^(NSURLRequest *request) {
        actualRequest = request;
        return request;
    };

    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"NoSuchTable"];
    todoTable.systemProperties = MSSystemPropertyVersion;

    // Create the item
    id item = @{ @"id":@"the-id", @"name":@"test name" };

    // Insert without parameters does not have features header
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNotNil(actualRequest, @"actualRequest should not have been nil.");
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertNil([actualRequest.allHTTPHeaderFields valueForKey:MSFeaturesHeaderName], @"Unexpected features header");

        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    actualRequest = nil;
    done = NO;

    // Insert with parameters has appropriate features header
    [todoTable insert:item parameters:@{@"a":@"b"} completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNotNil(actualRequest, @"actualRequest should not have been nil.");
        XCTAssertNil(error, @"error should have been nil.");

        NSString *featuresHeader = [actualRequest.allHTTPHeaderFields valueForKey:MSFeaturesHeaderName];
        XCTAssertNotNil(featuresHeader, @"actualHeader should not have been nil.");
        XCTAssertTrue([featuresHeader isEqualToString:MSFeatureCodeQueryParameters], @"Header value (%@) was not as expected (%@)", featuresHeader, MSFeatureCodeQueryParameters);

        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    actualRequest = nil;
    done = NO;

    // Update with no __version or parameters has no features header
    [todoTable update:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNotNil(actualRequest, @"actualRequest should not have been nil.");
        XCTAssertNil(error, @"error should have been nil.");

        XCTAssertNil([actualRequest.allHTTPHeaderFields valueForKey:MSFeaturesHeaderName], @"Unexpected features header");

        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    actualRequest = nil;
    done = NO;

    // Update with __version has OC features header
    NSDictionary *itemWithVersion = @{@"id":@"the-id",@"name":@"value",@"__version":@"abc"};
    [todoTable update:itemWithVersion completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNotNil(actualRequest, @"actualRequest should not have been nil.");
        XCTAssertNil(error, @"error should have been nil.");

        NSString *featuresHeader = [actualRequest.allHTTPHeaderFields valueForKey:MSFeaturesHeaderName];
        XCTAssertNotNil(featuresHeader, @"actualHeader should not have been nil.");
        XCTAssertTrue([featuresHeader isEqualToString:MSFeatureCodeOpportunisticConcurrency], @"Header value (%@) was not as expected (%@)", featuresHeader, MSFeatureCodeOpportunisticConcurrency);

        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    actualRequest = nil;
    done = NO;

    // Update with parameters has appropriate features header
    [todoTable update:itemWithVersion parameters:@{@"a":@"b"} completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNotNil(actualRequest, @"actualRequest should not have been nil.");
        XCTAssertNil(error, @"error should have been nil.");

        NSString *expectedHeader = [MSSDKFeatures httpHeaderForFeatures:MSFeatureOpportunisticConcurrency | MSFeatureQueryParameters];
        NSString *featuresHeader = [actualRequest.allHTTPHeaderFields valueForKey:MSFeaturesHeaderName];
        XCTAssertNotNil(featuresHeader, @"actualHeader should not have been nil.");
        XCTAssertTrue([featuresHeader isEqualToString:expectedHeader], @"Header value (%@) was not as expected (%@)", featuresHeader, expectedHeader);

        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    actualRequest = nil;
    done = NO;

    // Delete with no __version or parameters has no features header
    [todoTable delete:item completion:^(id itemId, NSError *error) {
        XCTAssertNotNil(actualRequest, @"actualRequest should not have been nil.");
        XCTAssertNil(error, @"error should have been nil.");

        XCTAssertNil([actualRequest.allHTTPHeaderFields valueForKey:MSFeaturesHeaderName], @"Unexpected features header");

        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    actualRequest = nil;
    done = NO;

    // Delete with __version has OC features header
    [todoTable delete:itemWithVersion completion:^(id itemId, NSError *error) {
        XCTAssertNotNil(actualRequest, @"actualRequest should not have been nil.");
        XCTAssertNil(error, @"error should have been nil.");

        NSString *featuresHeader = [actualRequest.allHTTPHeaderFields valueForKey:MSFeaturesHeaderName];
        XCTAssertNotNil(featuresHeader, @"actualHeader should not have been nil.");
        XCTAssertTrue([featuresHeader isEqualToString:MSFeatureCodeOpportunisticConcurrency], @"Header value (%@) was not as expected (%@)", featuresHeader, MSFeatureCodeOpportunisticConcurrency);

        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    actualRequest = nil;
    done = NO;

    // Delete with parameters has appropriate features header
    [todoTable delete:itemWithVersion parameters:@{@"a":@"b"} completion:^(id itemId, NSError *error) {
        XCTAssertNotNil(actualRequest, @"actualRequest should not have been nil.");
        XCTAssertNil(error, @"error should have been nil.");

        NSString *expectedHeader = [MSSDKFeatures httpHeaderForFeatures:MSFeatureOpportunisticConcurrency | MSFeatureQueryParameters];
        NSString *featuresHeader = [actualRequest.allHTTPHeaderFields valueForKey:MSFeaturesHeaderName];
        XCTAssertNotNil(featuresHeader, @"actualHeader should not have been nil.");
        XCTAssertTrue([featuresHeader isEqualToString:expectedHeader], @"Header value (%@) was not as expected (%@)", featuresHeader, expectedHeader);

        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    actualRequest = nil;
    done = NO;

    // Delete with id with no parameters has no features header
    [todoTable deleteWithId:@"the-id" completion:^(id itemId, NSError *error) {
        XCTAssertNotNil(actualRequest, @"actualRequest should not have been nil.");
        XCTAssertNil(error, @"error should have been nil.");

        XCTAssertNil([actualRequest.allHTTPHeaderFields valueForKey:MSFeaturesHeaderName], @"Unexpected features header");

        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    actualRequest = nil;
    done = NO;

    // Delete with parameters has appropriate features header
    [todoTable deleteWithId:@"the-id" parameters:@{@"a":@"b"} completion:^(id itemId, NSError *error) {
        XCTAssertNotNil(actualRequest, @"actualRequest should not have been nil.");
        XCTAssertNil(error, @"error should have been nil.");

        NSString *featuresHeader = [actualRequest.allHTTPHeaderFields valueForKey:MSFeaturesHeaderName];
        XCTAssertNotNil(featuresHeader, @"actualHeader should not have been nil.");
        XCTAssertTrue([featuresHeader isEqualToString:MSFeatureCodeQueryParameters], @"Header value (%@) was not as expected (%@)", featuresHeader, MSFeatureCodeQueryParameters);

        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}


#pragma mark * Async Test Helper Method


-(BOOL) waitForTest:(NSTimeInterval)testDuration {
 
    NSDate *timeoutAt = [NSDate dateWithTimeIntervalSinceNow:testDuration];
 
    while (!done) {
        [[NSRunLoop currentRunLoop] runMode:NSDefaultRunLoopMode
                                 beforeDate:timeoutAt];
        if([timeoutAt timeIntervalSinceNow] <= 0.0) {
            break;
        }
    };
 
    return done;
}

@end
