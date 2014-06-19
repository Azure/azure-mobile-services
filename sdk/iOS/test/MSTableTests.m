// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <SenTestingKit/SenTestingKit.h>
#import "MSTable.h"
#import "MSTestFilter.h"
#import "MSQuery.h"
#import "MSTable+MSTableTestUtilities.h"

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
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSTable *todoTable = [filteredClient tableWithName:@"NoSuchTable"];
    
    // Create the item
    id item = @{ @"id":@"120", @"name":@"test name" };
    
    // Test deletion of the item
    [todoTable delete:item completion:^(id itemId, NSError *error) {
        STAssertNil(itemId, @"item should have been nil.");
        STAssertEquals(error.code, [@MSErrorPreconditionFailed integerValue], @"Error should be precondition");
        NSDictionary* serverItem =[error.userInfo objectForKey:MSErrorServerItemKey];
        STAssertEqualObjects([serverItem objectForKey:@"id"], @120, @"id portion of ServerItem was not expected value");
        STAssertEqualObjects([serverItem objectForKey:@"name"], @"test name", @"name portion of ServerItem was not expected value");
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
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
        STAssertNil(itemId, @"item should have been nil.");
        STAssertEquals(error.code, [@MSErrorPreconditionFailed integerValue], @"Error should be precondition");
        NSDictionary* serverItem =[error.userInfo objectForKey:MSErrorServerItemKey];
        STAssertEquals(serverItem.count, (unsigned int) 0, @"empty JSON object error has no members in userInfo");
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
    
    // Delete the item
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
    
    // Delete the item
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
    
    // Delete the item
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
    
    // Delete the item
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
    
    // Delete the item
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
    
    // Delete the item
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

    // Delete the item
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
    
    // Delete the item
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
        
    // Delete the item
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
    
    // Delete the item
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
    
    // Delete the item
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
    
    // Read the item
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
    
    // Read the item
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

    // Read the item
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
    
    // Read the item
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
    
    // Read the item
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

# pragma mark System Property Tests

-(void) testInsertStringIdPropertiesNotRemovedFromRequest
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
        
        NSString *dataString = [NSString stringWithFormat:@"{\"id\":\"an id\",\"%@\":\"a value\",\"string\":\"What?\"}", property];
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
            STAssertTrue([bodyString rangeOfString:property].location != NSNotFound, @"The body was not serialized as expected.");
            STAssertEqualObjects(@"a value", [item objectForKey:property], @"Property %@ was removed", property);
            done = YES;
        }];
        STAssertTrue([self waitForTest:0.1], @"Test timed out.");
    }
}

-(void) testInsertNullIdSystemPropertiesNotRemovedFromRequest
{
    __block NSURLRequest *actualRequest = nil;
    NSArray *testSystemProperties = [MSTable testValidSystemProperties];

    for (NSString *property in testSystemProperties)
    {
        MSTestFilter *testFilter = [[MSTestFilter alloc] init];
        NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                       initWithURL:nil
                                       statusCode:200
                                       HTTPVersion:nil headerFields:nil];
        testFilter.responseToUse = response;
        NSString *dataString = [NSString stringWithFormat:@"{\"id\":\"an id\",\"%@\":\"a value\",\"string\":\"What?\"}", property];
        testFilter.dataToUse = [dataString dataUsingEncoding:NSUTF8StringEncoding];
        testFilter.ignoreNextFilter = YES;
        testFilter.onInspectRequest =  ^(NSURLRequest *request) {
            actualRequest = request;
            return request;
        };
        
        MSClient *filteredClient = [client clientWithFilter:testFilter];
        MSTable *todoTable = [filteredClient tableWithName:@"someTable"];
        
        NSDictionary *itemToInsert = @{@"id": [NSNull null], @"string": @"What?", property: @"a value"};
        [todoTable insert:itemToInsert completion:^(NSDictionary *item, NSError *error) {
            NSData *actualBody = actualRequest.HTTPBody;
            NSString *bodyString = [[NSString alloc] initWithData:actualBody
                                                         encoding:NSUTF8StringEncoding];
            STAssertTrue([bodyString rangeOfString:property].location != NSNotFound, @"The body was not serialized as expected.");
            STAssertEqualObjects(@"a value", [item objectForKey:property], @"system property %@ was removed", property);
            
            done = YES;
        }];
        STAssertTrue([self waitForTest:0.1], @"Test timed out.");
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
            STAssertTrue([bodyString rangeOfString:property].location != NSNotFound, @"Error: The body was not serialized as expected.");
            STAssertEqualObjects(@"a value", [item objectForKey:property], @"Error: Non system property %@ was removed", property);
            done = YES;
        }];
        STAssertTrue([self waitForTest:0.1], @"Test timed out.");
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
            STAssertEqualObjects(bodyString, @"{\"id\":\"an id\",\"string\":\"What?\"}",
                                 @"The body was not serialized as expected.");
            
            done = YES;
        }];
        STAssertTrue([self waitForTest:0.1], @"Test timed out.");
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
            STAssertTrue([bodyString rangeOfString:property].location != NSNotFound, @"The body was not serialized as expected.");
            STAssertEqualObjects(@"a value", [item objectForKey:property], @"Non system property %@ was removed", property);
            done = YES;
        }];
        STAssertTrue([self waitForTest:0.1], @"Test timed out.");
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
            STAssertTrue([bodyString rangeOfString:property].location != NSNotFound,
                         @"The body was not serialized as expected: %@", bodyString);
            STAssertEqualObjects(@"a value", [item objectForKey:property], @"Property %@ was removed", property);
            done = YES;
        }];
        STAssertTrue([self waitForTest:0.1], @"Test timed out.");
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
            STAssertTrue([self checkRequestURL:[actualRequest URL] SystemProperty:systemProperty], @"Error for property %d with url: %@", systemProperty, [[actualRequest URL] query]);
            done = YES;
        }];
        STAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        done = NO;
        [todoTable update:item completion:^(NSDictionary *item, NSError *error) {
            STAssertTrue([self checkRequestURL:[actualRequest URL] SystemProperty:systemProperty], @"Error with url: %@", [[actualRequest URL] query]);
            done = YES;
        }];
        STAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        done = NO;
        [todoTable readWithId:item completion:^(NSDictionary *item, NSError *error) {
            STAssertTrue([self checkRequestURL:[actualRequest URL] SystemProperty:systemProperty], @"Error with url: %@", [[actualRequest URL] query]);
            done = YES;
        }];
        STAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        done = NO;
        [todoTable delete:item completion:^(id itemId, NSError *error) {
            STAssertTrue([self checkRequestURL:[actualRequest URL] SystemProperty:systemProperty], @"Error with url: %@", [[actualRequest URL] query]);
            done = YES;
        }];
        STAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        // Integer Id
        done = NO;
        item = @{@"String": @"what?"};
        testFilter.dataToUse = [@"{\"id\":5,\"String\":\"Hey\"}" dataUsingEncoding:NSUTF8StringEncoding];
        [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
            STAssertTrue([self checkRequestURL:[actualRequest URL] SystemProperty:systemProperty], @"Error with url: %@", [[actualRequest URL] query]);
            done = YES;
        }];
        STAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        done = NO;
        item = @{@"id": @5, @"String": @"what?"};
        [todoTable update:item completion:^(NSDictionary *item, NSError *error) {
            STAssertTrue([self checkRequestURL:[actualRequest URL] SystemProperty:systemProperty], @"Error with url: %@", [[actualRequest URL] query]);
            done = YES;
        }];
        STAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        done = NO;
        [todoTable readWithId:item completion:^(NSDictionary *item, NSError *error) {
            STAssertTrue([self checkRequestURL:[actualRequest URL] SystemProperty:systemProperty], @"Error with url: %@", [[actualRequest URL] query]);
            done = YES;
        }];
        STAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        done = NO;
        [todoTable delete:item completion:^(id itemId, NSError *error) {
            STAssertTrue([self checkRequestURL:[actualRequest URL] SystemProperty:systemProperty], @"Error with url: %@", [[actualRequest URL] query]);
            done = YES;
        }];
        STAssertTrue([self waitForTest:0.1], @"Test timed out.");

        // Query
        
        done = NO;
        [todoTable readWithQueryString:@"$filter=id eq 5" completion:^(NSArray *items, NSInteger totalCount, NSError *error) {
            STAssertTrue([self checkRequestURL:[actualRequest URL] SystemProperty:systemProperty], @"Error with url: %@", [[actualRequest URL] query]);
            done = YES;
        }];
        STAssertTrue([self waitForTest:0.1], @"Test timed out.");

        done = NO;
        [todoTable readWithQueryString:@"$select=id,String" completion:^(NSArray *items, NSInteger totalCount, NSError *error) {
            STAssertTrue([self checkRequestURL:[actualRequest URL] SystemProperty:systemProperty], @"Error with url: %@", [[actualRequest URL] query]);
            done = YES;
        }];
        STAssertTrue([self waitForTest:0.1], @"Test timed out.");
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
            STAssertTrue([url rangeOfString:@"__systemProperties=__createdAt"].location != NSNotFound, @"Incorrect query: %@", url);
            done = YES;
        }];
        STAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        done = NO;
        [todoTable update:item parameters:@{ @"__systemProperties": @"createdAt"} completion:^(NSDictionary *item, NSError *error) {
            NSString *url = [[actualRequest URL] query];
            STAssertTrue([url rangeOfString:@"__systemProperties=createdAt"].location != NSNotFound, @"Incorrect query: %@", url);
            done = YES;
        }];
        STAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        done = NO;
        [todoTable readWithId:@"an id" parameters:@{ @"__systemProperties": @"CreatedAt"} completion:^(NSDictionary *item, NSError *error) {
            NSString *url = [[actualRequest URL] query];
            STAssertTrue([url rangeOfString:@"__systemProperties=CreatedAt"].location != NSNotFound, @"Incorrect query: %@", url);
            done = YES;
        }];
        STAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        done = NO;
        [todoTable delete:item parameters:@{ @"__systemProperties": @"unknown"} completion:^(id itemId, NSError *error) {
            NSString *url = [[actualRequest URL] query];
            STAssertTrue([url rangeOfString:@"__systemProperties=unknown"].location != NSNotFound, @"Incorrect query: %@", url);
            done = YES;
        }];
        STAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        // Integer Id
        done = NO;
        item = @{@"string":@"what?"};
        testFilter.dataToUse = [@"{\"id\":5,\"String\":\"Hey\"}" dataUsingEncoding:NSUTF8StringEncoding];
        [todoTable insert:item parameters:@{ @"__systemProperties": @"__createdAt"} completion:^(NSDictionary *item, NSError *error) {
            NSString *url = [[actualRequest URL] query];
            STAssertTrue([url rangeOfString:@"__systemProperties=__createdAt"].location != NSNotFound, @"Incorrect query: %@", url);
            done = YES;
        }];
        STAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        done = NO;
        item = @{@"id":@5, @"string":@"what?"};
        [todoTable update:item parameters:@{ @"__systemProperties": @"createdAt"} completion:^(NSDictionary *item, NSError *error) {
            NSString *url = [[actualRequest URL] query];
            STAssertTrue([url rangeOfString:@"__systemProperties=createdAt"].location != NSNotFound, @"Incorrect query: %@", url);
            done = YES;
        }];
        STAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        done = NO;
        [todoTable readWithId:@5 parameters:@{ @"__systemProperties": @"CreatedAt"} completion:^(NSDictionary *item, NSError *error) {
            NSString *url = [[actualRequest URL] query];
            STAssertTrue([url rangeOfString:@"__systemProperties=CreatedAt"].location != NSNotFound, @"Incorrect query: %@", url);
            done = YES;
        }];
        STAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        done = NO;
        [todoTable delete:item parameters:@{ @"__systemProperties": @"unknown"} completion:^(id itemId, NSError *error) {
            NSString *url = [[actualRequest URL] query];
            STAssertTrue([url rangeOfString:@"__systemProperties=unknown"].location != NSNotFound, @"Incorrect query: %@", url);
            done = YES;
        }];
        STAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        // Query
        
        done = NO;
        [todoTable readWithQueryString:@"$filter=id%20eq%205&__systemproperties=__createdAt" completion:^(NSArray *items, NSInteger totalCount, NSError *error) {
            NSString *url = [[actualRequest URL] query];
            STAssertEqualObjects(@"$filter=id%20eq%205&__systemproperties=__createdAt", url, @"Incorrect query: %@", url);
            done = YES;
        }];
        STAssertTrue([self waitForTest:0.1], @"Test timed out.");
        
        done = NO;
        [todoTable readWithQueryString:@"$select=id,String&__systemProperties=__CreatedAt" completion:^(NSArray *items, NSInteger totalCount, NSError *error) {
            NSString *url = [[actualRequest URL] query];
            STAssertEqualObjects(@"$select=id,String&__systemProperties=__CreatedAt", url, @"Incorrect query: %@", url);
            done = YES;
        }];
        STAssertTrue([self waitForTest:0.1], @"Test timed out.");
    }
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
