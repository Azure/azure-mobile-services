// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <SenTestingKit/SenTestingKit.h>
#import "MSSyncTable.h"
#import "MSTestFilter.h"
#import "MSQuery.h"
#import "MSTable+MSTableTestUtilities.h"
#import "MSOfflinePassthroughHelper.h"

@interface MSSyncTableTests : SenTestCase {
    MSClient *client;
    BOOL done;
    MSOfflinePassthroughHelper *offline;
}
@end

@implementation MSSyncTableTests

#pragma mark * Setup and TearDown

-(void) setUp
{
    NSLog(@"%@ setUp", self.name);
    
    client = [MSClient clientWithApplicationURLString:@"https://someUrl/"];
    offline = [MSOfflinePassthroughHelper new];
    
    // Enable offline mode
    client.syncContext = [[MSSyncContext alloc] initWithDelegate:offline dataSource:offline callback:nil];
    
    done = NO;
}

-(void) tearDown
{
    NSLog(@"%@ tearDown", self.name);
}

-(void) testInitWithNameAndClient
{
    MSSyncTable *table = [[MSSyncTable alloc] initWithName:@"SomeName" client:client];
    
    STAssertNotNil(table, @"table should not be nil.");
    
    STAssertNotNil(table.client, @"table.client should not be nil.");
    STAssertTrue([table.name isEqualToString:@"SomeName"],
                 @"table.name shouldbe 'SomeName'");
}

-(void) testInsertItemWithNoId
{
    MSSyncTable *todoTable = [client syncTableWithName:@"NoSuchTable"];
    
    // Create the item
    id item = @{ @"name":@"test name" };
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        STAssertNotNil([item objectForKey:@"id"], @"The item should have an id");
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInsertItemWithInvalidId
{
    // TODO: Fix error message when non string ids are used
    
    MSSyncTable *todoTable = [client syncTableWithName:@"NoSuchTable"];
    
    // Create the item
    id item = @{ @"id": @12345, @"name":@"test name" };
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        STAssertNotNil(error, @"error should have been set.");
        STAssertTrue(error.localizedDescription, @"The item provided must not have an id.");
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInsertItemWithInvalidItem
{
    MSSyncTable *todoTable = [client syncTableWithName:@"NoSuchTable"];
    
    // Create the item
    id item = @[ @"I_am_a", @"Array", @"I should be an object" ];
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        STAssertNotNil(error, @"error should have been set.");
        STAssertTrue(error.localizedDescription, @"The item provided was not valid.");
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

- (void) testInsertItemWithoutDatasource
{
    MSSyncTable *todoTable = [client syncTableWithName:@"NoSuchTable"];
    client.syncContext.dataSource = nil;
    
    // Create the item
    id item = @{ @"id": @"ok", @"data": @"lots of stuff here" };
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        STAssertNotNil(error, @"error should have been set.");
        //STAssertTrue(error.localizedDescription, @"The item provided was not valid.");
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInsertItemWithValidId
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    NSString* stringData = @"{\"id\": \"test1\", \"name\":\"test name\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    BOOL __block insertRanToServer = NO;
    
    testFilter.responseToUse = response;
    testFilter.dataToUse = data;
    testFilter.ignoreNextFilter = YES;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        insertRanToServer = YES;
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:@"NoSuchTable"];
    
    // Create the item
    NSDictionary *item = @{ @"id": @"test1", @"name":@"test name" };
    
    // Insert the item
    done = NO;
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        STAssertNil(error, @"error should have been nil.");
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    done = NO;
    [client.syncContext pushWithCompletion:^(NSError *error) {
        STAssertNil(error, @"error should have been nil.");
        STAssertTrue(insertRanToServer, @"the insert call didn't go to the server");
        done = YES;
    }];
    STAssertTrue([self waitForTest:2000.1], @"Test timed out.");
}

-(void) testInsertPushInsertPush
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    NSString* stringData = @"{\"id\": \"test1\", \"name\":\"test name\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    BOOL __block insertRanToServer = NO;
    
    testFilter.responseToUse = response;
    testFilter.dataToUse = data;
    testFilter.ignoreNextFilter = YES;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        insertRanToServer = YES;
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:@"NoSuchTable"];
    
    // Create the item
    NSDictionary *item = @{ @"id": @"test1", @"name":@"test name" };
    
    // Insert the item
    done = NO;
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        STAssertNil(error, @"error should have been nil.");
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    done = NO;
    [client.syncContext pushWithCompletion:^(NSError *error) {
        STAssertNil(error, @"error should have been nil.");
        STAssertTrue(insertRanToServer, @"the insert call didn't go to the server");
        done = YES;
    }];
    STAssertTrue([self waitForTest:1110.1], @"Test timed out.");

    // Create the item
    item = @{ @"id": @"test2", @"name":@"test name" };
    
    // Insert the item
    done = NO;
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        STAssertNil(error, @"error should have been nil.");
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    insertRanToServer = NO;    
    done = NO;
    [client.syncContext pushWithCompletion:^(NSError *error) {
        STAssertNil(error, @"error should have been nil.");
        STAssertTrue(insertRanToServer, @"the insert call didn't go to the server");
        done = YES;
    }];
    STAssertTrue([self waitForTest:2000.1], @"Test timed out.");

}

-(void) testInsertItemWithValidIdConflict
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:412
                                   HTTPVersion:nil headerFields:nil];
    NSString* stringData = @"{\"id\": \"test1\", \"name\":\"test name\", \"__version\":\"1\" }";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    BOOL __block insertRanToServer = NO;
    
    testFilter.responseToUse = response;
    testFilter.dataToUse = data;
    testFilter.ignoreNextFilter = YES;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        insertRanToServer = YES;
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:@"NoSuchTable"];
    
    // Create the item
    NSDictionary *item = @{ @"id": @"test1", @"name":@"test name" };
    
    // Insert the item
    done = NO;
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        STAssertNil(error, @"error should have been nil.");
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    done = NO;
    [client.syncContext pushWithCompletion:^(NSError *error) {
        // Verify the call went to the server
        STAssertTrue(insertRanToServer, @"the insert call didn't go to the server");

        // Verify we got the expected error results
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertEquals(error.code, [@MSPushCompleteWithErrors integerValue], @"Unexpected error code");
        NSArray *errors = [error.userInfo objectForKey:MSErrorPushResultKey];
        STAssertNotNil(errors, @"error should not have been nil.");
        
        // Verify we have a precondition failed error
        MSTableOperationError *errorInfo = [errors objectAtIndex:0];
        
        STAssertEquals(errorInfo.statusCode, [@412 integerValue], @"Unexpected status code");
        STAssertEquals(errorInfo.code, [@MSErrorPreconditionFailed integerValue], @"Unexpected status code");
        
        NSDictionary *actualItem = errorInfo.serverItem;
        STAssertNotNil(actualItem, @"Expected server version to be present");
        
        done = YES;
    }];
    STAssertTrue([self waitForTest:330.1], @"Test timed out.");
}

-(void) testInsertUpdateCollapseSuccess
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    NSString* stringData = @"{\"id\": \"test1\", \"name\":\"updated name\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    __block NSInteger callsToServer = 0;
    __block NSInteger postCalls = 0;
    
    testFilter.responseToUse = response;
    testFilter.dataToUse = data;
    testFilter.ignoreNextFilter = YES;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        callsToServer++;
        if ([request.HTTPMethod isEqualToString:@"POST"]) {
            postCalls++;
        }
        
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:@"NoSuchTable"];
    
    // Create the item
    NSDictionary *item = @{ @"id": @"test1", @"name": @"test name" };
    
    // Insert the item
    done = NO;
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        STAssertNil(error, @"error should have been nil.");
        done = YES;
    }];
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    // Update the item
    done = NO;
    item = @{ @"id": @"test1", @"name": @"updated name" };
    [todoTable update:item completion:^(NSError *error) {
        STAssertNil(error, @"error should have been nil.");
        done = YES;
    }];
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    // Push queue to server
    done = NO;
    [client.syncContext pushWithCompletion:^(NSError *error) {
        STAssertNil(error, @"error should have been nil.");
        STAssertTrue(callsToServer == 1, @"only one call to server should have been made");
        STAssertTrue(postCalls == 1, @"call should have been a POST");
        done = YES;
    }];
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInsertDeleteCollapseSuccess
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    NSString* stringData = @"{\"id\": \"test1\", \"name\":\"updated name\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    __block NSInteger callsToServer = 0;
    
    testFilter.responseToUse = response;
    testFilter.dataToUse = data;
    testFilter.ignoreNextFilter = YES;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        callsToServer++;
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:@"NoSuchTable"];
    
    // Create the item
    NSDictionary *item = @{ @"id": @"test1", @"name": @"test name" };
    
    // Insert the item
    done = NO;
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        STAssertNil(error, @"error should have been nil.");
        done = YES;
    }];
    STAssertTrue([self waitForTest:100000.1], @"Test timed out.");
    
    // Update the item
    done = NO;
    [todoTable delete:item completion:^(NSError *error) {
        STAssertNil(error, @"error should have been nil.");
        done = YES;
    }];
    STAssertTrue([self waitForTest:1000000.1], @"Test timed out.");
    
    // Push queue to server
    done = NO;
    [client.syncContext pushWithCompletion:^(NSError *error) {
        STAssertNil(error, @"error should have been nil.");
        STAssertTrue(callsToServer == 0, @"no calls to server should have been made");
        done = YES;
    }];
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInsertInsertCollapseThrows
{
    MSSyncTable *todoTable = [client syncTableWithName:@"NoSuchTable"];
    
    NSDictionary *item = @{ @"name": @"test" };
    [todoTable insert:item completion:^(NSDictionary *itemOne, NSError *error) {
       [todoTable insert:itemOne completion:^(NSDictionary *itemTwo, NSError *error) {
           STAssertNotNil(error, @"expected an error");
           STAssertTrue(error.code == -1150, @"unexpected error code");
           done = YES;
       }];
    }];
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testReadLocalStoreError
{
    offline.returnErrors = YES;
    
    MSSyncTable *todoTable = [client syncTableWithName:@"NoSuchTable"];
    
    [todoTable readWithId:@"10" completion:^(NSDictionary *item, NSError *error) {
        STAssertNotNil(error, @"Error should have been returned");
    }];
}

-(void) testPullSuccess
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    
    NSString* stringData = @"[{\"id\": \"one\", \"name\":\"first item\"},{\"id\": \"two\", \"name\":\"second item\"}]";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    testFilter.responseToUse = response;
    testFilter.dataToUse = data;
    testFilter.ignoreNextFilter = YES;
    offline.upsertCalls = 0;

    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:@"NoSuchTable"];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    [todoTable pullWithQuery:query completion:^(NSError *error) {
        STAssertNil(error, error.description);
        STAssertEquals(offline.upsertCalls, 2, @"Unexpected number of upsert calls");
        done = YES;        
    }];
    
    STAssertTrue([self waitForTest:30.0], @"Test timed out.");
}

-(void) testPurgeWithEmptyQueueSuccess
{
    MSSyncTable *todoTable = [client syncTableWithName:@"NoSuchTable"];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    [todoTable purgeWithQuery:query completion:^(NSError *error) {
        STAssertNil(error, error.description);
        done = YES;
    }];

    STAssertTrue([self waitForTest:30.0], @"Test timed out.");
}

-(void) testPurgeWithPushSuccess
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    
    NSString* stringData = @"{\"id\": \"test1\", \"name\":\"updated name\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    __block NSInteger callsToServer = 0;
    
    testFilter.responseToUse = response;
    testFilter.dataToUse = data;
    testFilter.ignoreNextFilter = YES;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        callsToServer++;
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:@"NoSuchTable"];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    [todoTable insert:@{ @"id":@"test1", @"name": @"test one"} completion:^(NSDictionary *item, NSError *error) {
        [todoTable purgeWithQuery:query completion:^(NSError *error) {
            STAssertNil(error, error.description);
            STAssertTrue(callsToServer == 1, @"expected push to have called serer");
            done = YES;
        }];
    }];
    
    STAssertTrue([self waitForTest:100.0], @"Test timed out.");
}

-(void) testPurgeNoPushOnDifferentTableSuccess
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    testFilter.ignoreNextFilter = YES;
    testFilter.errorToUse = [NSError errorWithDomain:MSErrorDomain code:-1 userInfo:nil];
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *testTable = [filteredClient syncTableWithName:@"Test"];

    MSSyncTable *todoTable = [filteredClient syncTableWithName:@"ToDo"];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    [testTable insert:@{ @"name": @"test one"} completion:^(NSDictionary *item, NSError *error) {
        [todoTable purgeWithQuery:query completion:^(NSError *error) {
            STAssertNil(error, error.description);
            done = YES;
        }];
    }];
    
    
    STAssertTrue([self waitForTest:30.0], @"Test timed out.");
}

-(void) testPurgeWithPushFailure
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:409
                                   HTTPVersion:nil headerFields:nil];
    
    NSString* stringData = @"{\"id\": \"test1\", \"name\":\"updated name\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    __block NSInteger callsToServer = 0;
    
    testFilter.responseToUse = response;
    testFilter.dataToUse = data;
    testFilter.ignoreNextFilter = YES;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        callsToServer++;
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:@"NoSuchTable"];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    [todoTable insert:@{ @"name": @"test one"} completion:^(NSDictionary *item, NSError *error) {
        [todoTable purgeWithQuery:query completion:^(NSError *error) {
            STAssertNotNil(error, error.description);
            STAssertTrue(callsToServer == 1, @"expected push to have called serer");
            done = YES;
        }];
    }];
    
    STAssertTrue([self waitForTest:1000.1], @"Test timed out.");
}

#pragma mark * Async Test Helper Method


-(BOOL) waitForTest:(NSTimeInterval)testDuration {
    
    NSDate *timeoutAt = [NSDate dateWithTimeIntervalSinceNow:testDuration];
    
    while (!done) {
        [[NSRunLoop currentRunLoop] runMode:NSRunLoopCommonModes
                                 beforeDate:timeoutAt];
        if([timeoutAt timeIntervalSinceNow] <= 0.0) {
            break;
        }
    };
    
    return done;
}

@end
