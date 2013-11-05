// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <SenTestingKit/SenTestingKit.h>
#import "MSTable.h"
#import "MSTestFilter.h"
#import "MSQuery.h"


@interface MSTableTests : SenTestCase {
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
    
    STAssertNotNil(table, @"table should not be nil.");
    
    STAssertNotNil(table.client, @"table.client should not be nil.");
    STAssertTrue([table.name isEqualToString:@"SomeName"],
                 @"table.name shouldbe 'SomeName'");
}

-(void) testInitWithNameAndClientAllowsNil
{
    MSTable *table = [[MSTable alloc] initWithName:nil client:nil];
    
    STAssertNotNil(table, @"table should not be nil.");
    
    STAssertNil(table.client, @"table.client should be nil.");
    STAssertNil(table.name, @"table.name should be nil.");
}


#pragma mark * Insert Method Tests

// See the WindowsAzureMobileServicesFunctionalTests.m tests for additional
// insert tests that require a working Windows Azure Mobile Service.

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
        STAssertNotNil(item, @"item should not have  been nil.");
        STAssertNil(error, @"error should have been nil.");
        STAssertTrue([[item valueForKey:@"name"] isEqualToString:@"test name"],
                     @"item should have been inserted.");
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInsertItemWithNilItem
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Insert the item
    [todoTable insert:nil completion:^(NSDictionary *item, NSError *error) {
    
        STAssertNil(item, @"item should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSExpectedItemWithRequest,
                     @"error code should have been MSExpectedItemWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"No item was provided."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInsertItemWithInvalidItem
{
    MSTable *todoTable = [client tableWithName:@"NoSuchTable"];
    
    // Create the item
    id item = [[NSDate alloc] initWithTimeIntervalSinceReferenceDate:0.0];
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
    
        STAssertNil(item, @"item should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSInvalidItemWithRequest,
                     @"error code should have been MSInvalidItemWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided was not valid."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
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
        STAssertNotNil(item, @"item should not have  been nil.");
        STAssertNil(error, @"error should have been nil.");
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
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
        STAssertNotNil(item, @"item should not have  been nil.");
        STAssertNil(error, @"error should have been nil.");
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
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
        STAssertNotNil(item, @"item should not have  been nil.");
        STAssertNil(error, @"error should have been nil.");
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
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
        STAssertNotNil(item, @"item should not have  been nil.");
        STAssertNil(error, @"error should have been nil.");
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
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
        
        STAssertNotNil(contentType, @"Content-Type should not have been nil.");
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}


#pragma mark * Update Method Tests


// See the WindowsAzureMobileServicesFunctionalTests.m tests for additional
// update tests that require a working Windows Azure Mobile Service.

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
        STAssertNotNil(item, @"item should not have  been nil.");
        STAssertNil(error, @"error should have been nil.");
        STAssertTrue([[item valueForKey:@"name"] isEqualToString:@"test name updated"],
                       @"item should have been updated.");
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
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
        STAssertNotNil(item, @"item should not have  been nil.");
        STAssertNil(error, @"error should have been nil.");
        STAssertTrue([[item valueForKey:@"name"] isEqualToString:@"test name updated"],
                     @"item should have been updated.");
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testUpdateItemWithNilItem
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Update the item
    [todoTable update:nil completion:^(NSDictionary *item, NSError *error) {
    
        STAssertNil(item, @"item should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSExpectedItemWithRequest,
                     @"error code should have been MSExpectedItemWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"No item was provided."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testUpdateItemWithInvalidItem
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    id item = [[NSDate alloc] initWithTimeIntervalSinceReferenceDate:0.0];
    
    // Update the item
    [todoTable update:item completion:^(NSDictionary *item, NSError *error) {
        
        STAssertNil(item, @"item should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSInvalidItemWithRequest,
                     @"error code should have been MSInvalidItemWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided was not valid."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testUpdateItemWithNoItemId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write unit tests!", @"complete": @(NO) };
    
    // Update the item
    [todoTable update:item completion:^(NSDictionary *item, NSError *error) {
  
        STAssertNil(item, @"item should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSMissingItemIdWithRequest,
                     @"error code should have been MSMissingItemIdWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided did not have an id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testUpdateItemWithEmptyStringItemId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write unit tests!", @"id":@"" };
    
    // Update the item
    [todoTable update:item completion:^(NSDictionary *item, NSError *error) {
    
        STAssertNil(item, @"item should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testUpdateItemWithwhiteSpaceItemId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write unit tests!", @"id":@"  " };
    
    // Update the item
    [todoTable update:item completion:^(NSDictionary *item, NSError *error) {
        
        STAssertNil(item, @"item should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testUpdateItemWithItemIdZero
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write unit tests!", @"id":@0 };
    
    // Update the item
    [todoTable update:item completion:^(NSDictionary *item, NSError *error) {
        
        STAssertNil(item, @"item should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

#pragma mark * Delete Method Tests


// See the WindowsAzureMobileServicesFunctionalTests.m tests for additional
// delete tests that require a working Windows Azure Mobile Service.


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
        STAssertNotNil(itemId, @"item should not have  been nil.");
        STAssertNil(error, @"error should have been nil.");
        STAssertTrue([itemId isEqualToNumber:@120],
                     @"item should have been inserted.");
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
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
        STAssertNotNil(itemId, @"item should not have  been nil.");
        STAssertNil(error, @"error should have been nil.");
        STAssertTrue([itemId isEqualToString:@"120"],
                     @"item should have been inserted.");
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithNilItem
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Update the item
    [todoTable delete:nil completion:^(id itemId, NSError *error) {
  
        STAssertNil(itemId, @"itemId should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSExpectedItemWithRequest,
                     @"error code should have been MSExpectedItemWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"No item was provided."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithInvalidItem
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    id item = [[NSDate alloc] initWithTimeIntervalSinceReferenceDate:0.0];
    
    // Update the item
    [todoTable delete:item completion:^(id itemId, NSError *error) {
        
        STAssertNil(itemId, @"itemId should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSInvalidItemWithRequest,
                     @"error code should have been MSInvalidItemWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided was not valid."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithNoItemId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write unit tests!", @"complete": @(NO) };
    
    // Update the item
    [todoTable delete:item completion:^(id itemId, NSError *error) {
    
        STAssertNil(itemId, @"itemId should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSMissingItemIdWithRequest,
                     @"error code should have been MSMissingItemIdWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided did not have an id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithInvalidItemId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write unit tests!", @"id":@0 };
    
    // Update the item
    [todoTable delete:item completion:^(id itemId, NSError *error) {
        
        STAssertNil(itemId, @"itemId should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithEmptyStringId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write unit tests!", @"id":@"" };
    
    // Update the item
    [todoTable delete:item completion:^(id itemId, NSError *error) {
        
        STAssertNil(itemId, @"itemId should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithWhiteSpaceId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write unit tests!", @"id":@"  " };
    
    // Update the item
    [todoTable delete:item completion:^(id itemId, NSError *error) {
        
        STAssertNil(itemId, @"itemId should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithItemIdZero
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write unit tests!", @"id":@0 };
    
    // Update the item
    [todoTable delete:item completion:^(id itemId, NSError *error) {
        
        STAssertNil(itemId, @"itemId should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
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
        STAssertNotNil(itemId, @"item should not have  been nil.");
        STAssertNil(error, @"error should have been nil.");
        STAssertTrue([itemId isEqualToNumber:@120],
                     @"item should have been inserted.");
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
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
        STAssertNotNil(itemId, @"item should not have  been nil.");
        STAssertNil(error, @"error should have been nil.");
        STAssertTrue([itemId isEqualToString:@"120"],
                     @"item should have been inserted.");
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithIdWithNoItemId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];

    // Update the item
    [todoTable deleteWithId:nil completion:^(id itemId, NSError *error) {
    
        STAssertNil(itemId, @"itemId should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSExpectedItemIdWithRequest,
                     @"error code should have been MSExpectedItemIdWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item id was not provided."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithIdWithInvalidItemId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    id itemId = [[NSDate alloc] initWithTimeIntervalSince1970:0.0];
    
    // Update the item
    [todoTable deleteWithId:itemId completion:^(id itemId, NSError *error) {
        
        STAssertNil(itemId, @"itemId should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithIdWithIdZero
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
        
    // Update the item
    [todoTable deleteWithId:@0 completion:^(id itemId, NSError *error) {
        
        STAssertNil(itemId, @"itemId should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithIdWithEmptyStringId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Update the item
    [todoTable deleteWithId:@"" completion:^(id itemId, NSError *error) {
        
        STAssertNil(itemId, @"itemId should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithIdWithWhiteSpaceId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Update the item
    [todoTable deleteWithId:@" " completion:^(id itemId, NSError *error) {
        
        STAssertNil(itemId, @"itemId should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
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
  
        STAssertNil(contentType, @"Content-Type should not have been set.");
    
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}


#pragma mark * ReadWithId Method Tests


// See the WindowsAzureMobileServicesFunctionalTests.m tests for additional
// readWithId tests that require a working Windows Azure Mobile Service.

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
        STAssertNotNil(item, @"item should not have  been nil.");
        STAssertNil(error, @"error should have been nil.");
        STAssertTrue([[item valueForKey:@"id"] isEqualToNumber:@120],
                     @"item should have been read.");
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
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
        STAssertNotNil(item, @"item should not have  been nil.");
        STAssertNil(error, @"error should have been nil.");
        STAssertTrue([[item valueForKey:@"id"] isEqualToString:@"120"],
                     @"item should have been read.");
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testReadItemWithIdWithNoItemId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Update the item
    [todoTable readWithId:nil completion:^(NSDictionary *item, NSError *error) {
    
        STAssertNil(item, @"item should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSExpectedItemIdWithRequest,
                     @"error code should have been MSExpectedItemIdWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item id was not provided."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testReadItemWithIdWithInvalidItemId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    id itemId = [[NSDate alloc] initWithTimeIntervalSince1970:0.0];
    
    // Update the item
    [todoTable readWithId:itemId completion:^(NSDictionary *item, NSError *error) {
     
        STAssertNil(item, @"item should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testReadItemWithIdWithIdZero
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];

    // Update the item
    [todoTable readWithId:@0 completion:^(NSDictionary *item, NSError *error) {
        
        STAssertNil(item, @"item should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testReadItemWithEmptyStringId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Update the item
    [todoTable readWithId:@"" completion:^(NSDictionary *item, NSError *error) {
        
        STAssertNil(item, @"item should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testReadItemWithWhiteSpaceId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Update the item
    [todoTable readWithId:@"  " completion:^(NSDictionary *item, NSError *error) {
        
        STAssertNil(item, @"item should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSInvalidItemIdWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
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
        
        STAssertNil(contentType, @"Content-Type should not have been set.");
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}


#pragma mark * Query Method Tests


-(void) testQueryReturnsNonNil
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    MSQuery *query = [todoTable query];
    
    STAssertNotNil(query, @"query should not have been nil.");    
}

-(void) testQueryWithPredicateReturnsNonNil
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    MSQuery *query = [todoTable queryWithPredicate:nil];
    
    STAssertNotNil(query, @"query should not have been nil.");
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
    [query readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
        
        STAssertNil(contentType, @"Content-Type should not have been set.");
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
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
