// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <XCTest/XCTest.h>
#import "MSSyncTable.h"
#import "MSTestFilter.h"
#import "MSMultiRequestTestFilter.h"
#import "MSQuery.h"
#import "MSTable+MSTableTestUtilities.h"
#import "MSOfflinePassthroughHelper.h"
#import "MSCoreDataStore+TestHelper.h"
#import "MSSDKFeatures.h"
#import "MSTestWaiter.h"
#import "MSNaiveISODateFormatter.h"
#import "MSTableOperation.h"
#import "MSTableOperationInternal.h"
#import "MSTableOperationError.h"
#import "MSSyncContextInternal.h"
#import "MSTableConfigValue.h"

static NSString *const TodoTableNoVersion = @"TodoNoVersion";
static NSString *const AllColumnTypesTable = @"ColumnTypes";
static NSString *const SyncContextQueueName = @"Sync Context: Operation Callbacks";

@interface MSSyncTableTests : XCTestCase {
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
    offline = [[MSOfflinePassthroughHelper alloc] initWithManagedObjectContext:[MSCoreDataStore inMemoryManagedObjectContext]];
    
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
    
    XCTAssertNotNil(table, @"table should not be nil.");
    
    XCTAssertNotNil(table.client, @"table.client should not be nil.");
    XCTAssertTrue([table.name isEqualToString:@"SomeName"],
                  @"table.name shouldbe 'SomeName'");
}


#pragma mark Insert Tests


-(void) testInsertItemWithNoId
{
    XCTestExpectation *expectation = [self expectationWithDescription:self.name];
    
    MSSyncTable *todoTable = [client syncTableWithName:TodoTableNoVersion];
    
    // Create the item
    id item = @{ @"text":@"test name" };
    __block NSString *itemId;
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNotNil(item[@"id"], @"The item should have an id");
        XCTAssertEqualObjects([NSOperationQueue currentQueue].name, SyncContextQueueName);
        itemId = item[@"id"];
        
        [expectation fulfill];
    }];
    
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
    
    // Verify the item was in local store
    NSError *error = nil;
    NSDictionary *newItem = [offline readTable:TodoTableNoVersion withItemId:itemId orError:&error];
    XCTAssertNil(error);
    XCTAssertNotNil(newItem);
    XCTAssertEqualObjects(newItem[@"text"], item[@"text"]);
}

-(void) testInsertItemWithInvalidId
{
    XCTestExpectation *expectation = [self expectationWithDescription:self.name];
    
    MSSyncTable __block *todoTable = nil;
    todoTable = [client syncTableWithName:TodoTableNoVersion];
    
    // Create the item
    id item = @{ @"id": @12345, @"text":@"test name" };
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNotNil(error, @"error should have been set.");
        XCTAssertEqualObjects(error.localizedDescription, @"The item provided did not have a valid id.");
        XCTAssertEqualObjects([NSOperationQueue currentQueue].name, SyncContextQueueName);
        [expectation fulfill];
    }];
    
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
}

-(void) testInsertItemWithInvalidItem
{
    XCTestExpectation *expectation = [self expectationWithDescription:self.name];
    MSSyncTable *todoTable = [client syncTableWithName:TodoTableNoVersion];
    
    // Create the item
    id item = @[ @"I_am_a", @"Array", @"I should be an object" ];
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNotNil(error, @"error should have been set.");
        XCTAssertEqualObjects(error.localizedDescription, @"The item provided was not valid.");
        XCTAssertEqualObjects([NSOperationQueue currentQueue].name, SyncContextQueueName);
        [expectation fulfill];
    }];
    
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
}

- (void) testInsertItemWithoutDatasource
{
    XCTestExpectation *expectation = [self expectationWithDescription:self.name];
    
    MSSyncTable *todoTable = [client syncTableWithName:TodoTableNoVersion];
    client.syncContext.dataSource = nil;
    
    // Create the item
    id item = @{ @"id": @"ok", @"data": @"lots of stuff here" };
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNotNil(error, @"error should have been set.");
        XCTAssertEqualObjects(error.localizedDescription, @"Missing required datasource for MSSyncContext");
        XCTAssertEqualObjects([NSOperationQueue currentQueue].name, SyncContextQueueName);
        [expectation fulfill];
    }];
    
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
}

-(void) testInsertIgnoresUnknownColumns
{
    XCTestExpectation *expectation = [self expectationWithDescription:self.name];
    
    MSSyncTable *todoTable = [client syncTableWithName:TodoTableNoVersion];
    
    // Create the item with unknown columns
    id item = @{ @"fake_column":@"test name", @"anotherone": @3, @"text": @"hello" };
    
    // Insert the item and verify no error occurs
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(error);
        
        [expectation fulfill];
    }];
    
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
}

-(void) testInsertItemWithValidId
{
    XCTestExpectation *expectation = [self expectationWithDescription:self.name];
    
    NSString* stringData = @"{\"id\": \"test1\", \"text\":\"test name\"}";
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:200 data:stringData];
    
    BOOL __block insertRanToServer = NO;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        XCTAssertEqualObjects(request.HTTPMethod, @"POST", @"Incorrect operation (%@) sent to server", request.HTTPMethod);
        insertRanToServer = YES;
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    
    // Create the item
    NSDictionary *item = @{ @"id": @"test1", @"name":@"test name" };
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        [expectation fulfill];
    }];
    
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
    
    // Now try sending the pending operation to the server
    expectation = [self expectationWithDescription:@"Push for Valid Item"];
    
    NSOperation *push = [client.syncContext pushWithCompletion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue(insertRanToServer, @"the insert call didn't go to the server");
        XCTAssertEqualObjects([NSOperationQueue currentQueue].name, SyncContextQueueName);
        [expectation fulfill];
    }];
	
	XCTAssertNotNil(push);
    
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
}

-(void) testInsertWithAllColumnTypes {
    XCTestExpectation *expectation = [self expectationWithDescription:self.name];
    
    NSString *testData = @"{\"id\": \"test1\", \"text\":\"test name\"}";
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:200
                                                                 data:testData];
    
    BOOL __block insertRanToServer = NO;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        XCTAssertEqualObjects(request.HTTPMethod, @"POST", @"Incorrect operation (%@) sent to server", request.HTTPMethod);
        insertRanToServer = YES;
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:AllColumnTypesTable];
    
    // Create the item
    NSDictionary *item = @{ @"id": @"simpleId",
                            @"string":@"string value",
                            @"int16": [NSNumber numberWithInt:16],
                            @"int32": [NSNumber numberWithInt:32],
                            @"int64": [NSNumber numberWithInt:64],
                            @"float": [NSNumber numberWithFloat:3.14],
                            @"decimal": [NSDecimalNumber decimalNumberWithMantissa:6 exponent:2 isNegative:NO],
                            @"double": [NSNumber numberWithDouble:12.12],
                            @"date": [NSDate dateWithTimeIntervalSinceNow:0],
                            };
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        [expectation fulfill];
    }];
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
    
    // Verify the item in local store is correct
    NSError *error = nil;
    NSDictionary *newItem = [offline readTable:AllColumnTypesTable withItemId:@"simpleId" orError:&error];
    XCTAssertNil(error);
    XCTAssertNotNil(newItem);
    XCTAssertEqualObjects(newItem[@"int16"], item[@"int16"]);
    XCTAssertEqualObjects(newItem[@"int32"], item[@"int32"]);
    XCTAssertEqualObjects(newItem[@"int64"], item[@"int64"]);
    XCTAssertEqualObjects(newItem[@"float"], item[@"float"]);
    XCTAssertEqualObjects(newItem[@"decimal"], item[@"decimal"]);
    XCTAssertEqualObjects(newItem[@"double"], item[@"double"]);
    XCTAssertEqualObjects(newItem[@"date"], item[@"date"]);
    
    // Now push this item to the server
    expectation = [self expectationWithDescription:@"Push with many column types"];
    
    NSOperation *push = [client.syncContext pushWithCompletion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue(insertRanToServer, @"the insert call didn't go to the server");
        XCTAssertEqualObjects([NSOperationQueue currentQueue].name, SyncContextQueueName);
        [expectation fulfill];
    }];
	XCTAssertNotNil(push);
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
}

-(void) testInsertWithBinaryFail {
    XCTestExpectation *expectation = [self expectationWithDescription:self.name];
    
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:500];
    
    BOOL __block insertRanToServer = NO;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        insertRanToServer = YES;
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:AllColumnTypesTable];
    
    // Create the item
    NSDictionary *item = @{ @"id": @"simpleId",
                            @"binary": [@"my test data" dataUsingEncoding:NSUTF8StringEncoding]
                            };
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        [expectation fulfill];
    }];
    
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
    
    // Now verify that the item is invalid for the server to handle
    expectation = [self expectationWithDescription:@"Push with Binary data in it"];
    NSOperation *push = [client.syncContext pushWithCompletion:^(NSError *error) {
        XCTAssertFalse(insertRanToServer);
        XCTAssertNotNil(error);
        XCTAssertEqual(error.code, MSPushCompleteWithErrors);
        
        NSArray *pushErrors = [error.userInfo objectForKey:MSErrorPushResultKey];
        XCTAssertNotNil(pushErrors);
        XCTAssertEqual(pushErrors.count, 1);
        
        MSTableOperationError *tableError = [pushErrors objectAtIndex:0];
        XCTAssertEqual(tableError.code, MSInvalidItemWithRequest);

        XCTAssertEqualObjects([NSOperationQueue currentQueue].name, SyncContextQueueName);
        [expectation fulfill];
        
    }];
	
	XCTAssertNotNil(push);
	
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
}

-(void) testInsertPushInsertPush
{
    NSString* stringData = @"{\"id\": \"test1\", \"text\":\"test name\"}";
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:200 data:stringData];
                                
    NSInteger __block serverCalls = 0;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        serverCalls++;
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    
    // Create the first item to insert
    XCTestExpectation *expectation = [self expectationWithDescription:self.name];
    NSDictionary *item = @{ @"id": @"test1", @"name":@"test name" };
    
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        [expectation fulfill];
    }];
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
    
    // Now push the first item to the server
    expectation = [self expectationWithDescription:@"Pushing First Insert"];
		NSOperation *push = [client.syncContext pushWithCompletion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue(serverCalls == 1, @"the insert call didn't go to the server");
        
        [expectation fulfill];
    }];
	XCTAssertNotNil(push);
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
    
    // Create the a new item and insert it
    expectation = [self expectationWithDescription:@"Second Insert"];
    item = @{ @"id": @"test2", @"name":@"test name" };
    
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        [expectation fulfill];
    }];
    [self waitForExpectationsWithTimeout:1.0 handler:nil];

    // Finally, push the second item to server
    expectation = [self expectationWithDescription:@"Pushing Second Insert"];
    push = [client.syncContext pushWithCompletion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue(serverCalls == 2, @"the insert call didn't go to the server");
        XCTAssertEqualObjects([NSOperationQueue currentQueue].name, SyncContextQueueName);
        
        
        [expectation fulfill];
    }];
	XCTAssertNotNil(push);
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
}

-(void) testInsertItemWithValidIdConflict
{
    NSString* stringData = @"{\"id\": \"test1\", \"text\":\"servers name\", \"__version\":\"1\" }";
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:412 data:stringData];
    
    NSInteger __block serverCalls = 0;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        serverCalls++;
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    
    // Create the item
    XCTestExpectation *expectation = [self expectationWithDescription:self.name];
    NSDictionary *item = @{ @"id": @"test1", @"text":@"test name" };
    
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        [expectation fulfill];
    }];
    XCTAssertEqual(serverCalls, 0);
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
    
    // Now try to push and trigger a conflict response
    expectation = [self expectationWithDescription:@"Push with server conflict"];
    NSOperation *push = [client.syncContext pushWithCompletion:^(NSError *error) {
        // Verify the call went to the server
        XCTAssertEqual(serverCalls, 1, @"the insert call didn't go to the server");
        
        // Verify we got the expected error results
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertEqual(error.code, [@MSPushCompleteWithErrors integerValue], @"Unexpected error code");
        
        NSArray *errors = error.userInfo[MSErrorPushResultKey];
        XCTAssertNotNil(errors, @"error should not have been nil.");
        
        // Verify we have a precondition failed error
        MSTableOperationError *errorInfo = errors[0];
        
        XCTAssertEqual(errorInfo.statusCode, [@412 integerValue], @"Unexpected status code");
        XCTAssertEqual(errorInfo.code, [@MSErrorPreconditionFailed integerValue], @"Unexpected error code");
        
        NSDictionary *actualItem = errorInfo.serverItem;
        XCTAssertNotNil(actualItem, @"Expected server version to be present");
        XCTAssertEqualObjects(actualItem[@"text"], @"servers name");

        NSDictionary *sentItem = errorInfo.item;
        XCTAssertNotNil(sentItem, @"Expected local version to be present");
        XCTAssertEqualObjects(sentItem[@"text"], @"test name");
        
        XCTAssertEqualObjects([NSOperationQueue currentQueue].name, SyncContextQueueName);        
        
        [expectation fulfill];
    }];
	XCTAssertNotNil(push);
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
}

-(void) testInsertUpdateCollapseSuccess
{
    NSString* stringData = @"{\"id\": \"test1\", \"text\":\"updated name\"}";
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:200 data:stringData];
    
    __block NSInteger callsToServer = 0;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        callsToServer++;
        
        XCTAssertEqualObjects(request.HTTPMethod, @"POST", @"Unexpected method: %@", request.HTTPMethod);
        
        // Verify the item == the final (post update) value and not the initial insert's value)
        NSString *bodyString = [[NSString alloc] initWithData:request.HTTPBody
                                                     encoding:NSUTF8StringEncoding];
        XCTAssertEqualObjects(bodyString, @"{\"id\":\"test1\",\"text\":\"updated name\"}");
        
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    
    // Create & insert the item
    XCTestExpectation *expectation = [self expectationWithDescription:self.name];
    NSDictionary *item = @{ @"id": @"test1", @"text": @"test name" };
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        [expectation fulfill];
    }];
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
    
    // Update the item
    expectation = [self expectationWithDescription:self.name];
    item = @{ @"id": @"test1", @"text": @"updated name" };
    [todoTable update:item completion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        [expectation fulfill];
    }];
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
    
    // Push queue to server
    expectation = [self expectationWithDescription:self.name];
    NSOperation *push = [client.syncContext pushWithCompletion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue(callsToServer == 1, @"only one call to server should have been made");
        XCTAssertEqualObjects([NSOperationQueue currentQueue].name, SyncContextQueueName);
        [expectation fulfill];
        
    }];
	XCTAssertNotNil(push);
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
}

-(void) testInsertDeleteCollapseSuccess
{
    NSString *stringData = @"{\"id\": \"test1\", \"text\":\"updated name\"}";
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:200 data:stringData];
        
    __block NSInteger callsToServer = 0;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        callsToServer++;
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    
    // Create the item
    NSDictionary *item = @{ @"id": @"test1", @"name": @"test name" };
    XCTestExpectation *expectation = [self expectationWithDescription:@"Inserting an item"];
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        [expectation fulfill];
    }];
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
    
    // Now delete the item
    expectation = [self expectationWithDescription:@"Deleting the pending item"];
    [todoTable delete:item completion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        [expectation fulfill];
    }];
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
    
    // Push to server (no calls expected)
    expectation = [self expectationWithDescription:@"Pushing (expecting no items)"];
    NSOperation *push = [client.syncContext pushWithCompletion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue(callsToServer == 0, @"no calls to server should have been made");
        XCTAssertEqualObjects([NSOperationQueue currentQueue].name, SyncContextQueueName);
        [expectation fulfill];
        
    }];
	XCTAssertNotNil(push);
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
}

-(void) testInsertInsertCollapseThrows
{
    XCTestExpectation *expectation = [self expectationWithDescription:self.name];
    MSSyncTable *todoTable = [client syncTableWithName:TodoTableNoVersion];
    
    NSDictionary *item = @{ @"name": @"test" };
    [todoTable insert:item completion:^(NSDictionary *itemOne, NSError *error) {
        [todoTable insert:itemOne completion:^(NSDictionary *itemTwo, NSError *error) {
            XCTAssertNotNil(error, @"expected an error");
            XCTAssertTrue(error.code == MSSyncTableInvalidAction, @"unexpected error code");
            XCTAssertEqualObjects([NSOperationQueue currentQueue].name, SyncContextQueueName);
            
            [expectation fulfill];
        }];
    }];
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
}

-(void) testInsertThenInsertSameItem
{
    XCTestExpectation *expectation = [self expectationWithDescription:self.name];
    
    NSString *insertResponse = @"{\"id\":\"one\", \"text\":\"first item\"}";
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:200 data:insertResponse];
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    
    NSDictionary *item = @{ @"id": @"one", @"text": @"first item" };
    [todoTable insert:item completion:^(NSDictionary *i, NSError *error) {
        XCTAssertNil(error);
        
        // push it to clear out pending operations
        [todoTable.client.syncContext pushWithCompletion:^(NSError *error) {
            [expectation fulfill];
        }];
    }];
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
    
    expectation = [self expectationWithDescription:@"Second Insert"];
    [todoTable insert:item completion:^(NSDictionary *i, NSError *error) {
        XCTAssertNotNil(error);
        XCTAssertEqual(error.code, MSSyncTableInvalidAction);
        XCTAssertEqualObjects([NSOperationQueue currentQueue].name, SyncContextQueueName);
        
        
        [expectation fulfill];
    }];
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
}

-(void) testInsertWithReadError
{
    NSString *insertResponse = @"{\"id\":\"one\", \"text\":\"first item\"}";
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:200 data:insertResponse];
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    
    offline.errorOnReadTableWithItemIdOrError = YES;
    NSDictionary *item = @{ @"id": @"one", @"text": @"first item" };

    // insert without any items; should give an error
    XCTestExpectation *expectation = [self expectationWithDescription:@"InsertError: Item not in table"];
    
    [todoTable insert:item completion:^(NSDictionary *i, NSError *error) {
        XCTAssertNotNil(error);
        XCTAssertEqual(error.code, 1);
        XCTAssertEqual(offline.upsertCalls, 0);
        XCTAssertEqual(offline.readTableCalls, 1);
        XCTAssertEqualObjects([NSOperationQueue currentQueue].name, SyncContextQueueName);
        
        [expectation fulfill];
    }];
    
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
    
    offline.errorOnReadTableWithItemIdOrError = NO;
    [offline resetCounts];
    expectation = [self expectationWithDescription:@"InsertError: Setup Expectation"];
    
    // now insert so we end up with an item in the local store
    [todoTable insert:item completion:^(NSDictionary *i, NSError *error) {
        XCTAssertNil(error);
        XCTAssertEqual(offline.upsertCalls, 2); // one for the item, one for the operation
        XCTAssertEqual(offline.readTableCalls, 1);
        
        // push it to clear out pending operations
        [todoTable.client.syncContext pushWithCompletion:^(NSError *error) {
            [expectation fulfill];
        }];
    }];
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
    
    offline.errorOnReadTableWithItemIdOrError = YES;
    [offline resetCounts];
    expectation = [self expectationWithDescription:@"InsertError: Item already in table"];

    // now this should error as well with our read error.
    [todoTable insert:item completion:^(NSDictionary *i, NSError *error) {
        XCTAssertNotNil(error);
        XCTAssertEqual(error.code, 1);
        XCTAssertEqual(offline.upsertCalls, 0);
        XCTAssertEqual(offline.readTableCalls, 1);
        XCTAssertEqualObjects([NSOperationQueue currentQueue].name, SyncContextQueueName);
        
        
        [expectation fulfill];
    }];
    
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
}

-(void) testInsertWithOperationError {
    XCTestExpectation *expectation = [self expectationWithDescription:self.name];
    MSSyncTable *todoTable = [client syncTableWithName:TodoTableNoVersion];
    
    // Insert an item
    offline.errorOnUpsertItemsForOperations = YES;
    [todoTable insert:@{ @"name":@"test name" } completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNotNil(error);
        XCTAssertNil(item);
        XCTAssertEqualObjects([NSOperationQueue currentQueue].name, SyncContextQueueName);
        [expectation fulfill];
    }];
    
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
}


#pragma mark Update Tests


-(void) testUpdate_Push_Success
{
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:200
                                                                 data:@"{\"id\": \"test1\", \"text\":\"test name\"}"];
    
    BOOL __block updateSentToServer = NO;
    
    testFilter.ignoreNextFilter = YES;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        XCTAssertEqualObjects(request.HTTPMethod, @"PATCH", @"Incorrect operation (%@) sent to server", request.HTTPMethod);
        updateSentToServer = YES;
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    
    // Create the item
    NSDictionary *item = @{ @"id": @"test1", @"name":@"test name" };
    
    // Insert the item
    done = NO;
    [todoTable update:item completion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertEqualObjects([NSOperationQueue currentQueue].name, SyncContextQueueName);
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    done = NO;
    NSOperation *push = [client.syncContext pushWithCompletion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue(updateSentToServer, @"the update call didn't go to the server");
        XCTAssertEqualObjects([NSOperationQueue currentQueue].name, SyncContextQueueName);
        done = YES;
    }];
	XCTAssertNotNil(push);
    XCTAssertTrue([self waitForTest:2000.1], @"Test timed out.");
}

-(void) testUpdateInsert_Collapse_Throws
{
    MSSyncTable *todoTable = [client syncTableWithName:TodoTableNoVersion];
    
    NSDictionary *item = @{ @"id": @"itemA", @"text": @"throw an error" };
    
    [todoTable update:item completion:^(NSError *error) {
        [todoTable insert:item completion:^(NSDictionary *itemTwo, NSError *error) {
            XCTAssertNotNil(error);
            XCTAssertEqual(error.code, MSSyncTableInvalidAction);
            done = YES;
        }];
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testUpdateUpdate_Collapse_Success
{
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:200
                                                                 data:@"{\"id\": \"test1\", \"text\":\"test name\"}"];
    
    NSInteger __block callsToServer = 0;
    
    testFilter.ignoreNextFilter = YES;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        XCTAssertEqualObjects(request.HTTPMethod, @"PATCH", @"Incorrect operation (%@) sent to server", request.HTTPMethod);
        NSString *bodyString = [[NSString alloc] initWithData:request.HTTPBody
                                                     encoding:NSUTF8StringEncoding];
        XCTAssertEqualObjects(bodyString, @"{\"id\":\"test1\",\"text\":\"updated name\"}", @"Unexpected item: %@", bodyString);
        callsToServer++;
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    
    // Create the item
    NSDictionary *item = @{ @"id": @"test1", @"text":@"test name" };
    
    done = NO;
    [todoTable update:item completion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    done = NO;
    item = @{ @"id": @"test1", @"text":@"updated name" };
    [todoTable update:item completion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    done = NO;
    NSOperation *push = [client.syncContext pushWithCompletion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue(callsToServer == 1, @"expected only 1 call to the server");
        done = YES;
    }];
	XCTAssertNotNil(push);
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testUpdateDelete_CollapseToDelete_Success
{
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:204];
    NSInteger __block callsToServer = 0;
    
    testFilter.ignoreNextFilter = YES;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        XCTAssertEqualObjects(request.HTTPMethod, @"DELETE", @"Incorrect operation (%@) sent to server", request.HTTPMethod);
        XCTAssertEqualObjects(request.URL.absoluteString, @"https://someUrl/tables/TodoNoVersion/test1?__systemProperties=__version");
        callsToServer++;
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    
    NSDictionary *item = @{ @"id": @"test1", @"text":@"test name" };
    
    done = NO;
    [todoTable update:item completion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    done = NO;
    [todoTable delete:item completion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    done = NO;
    NSOperation *push = [client.syncContext pushWithCompletion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue(callsToServer == 1, @"expected only 1 call to the server");
        done = YES;
    }];
	XCTAssertNotNil(push);
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}


#pragma mark Delete Tests


-(void) testDeleteNoVersion_Push_Success
{
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:200
                                                                 data:@"{\"id\": \"test1\", \"text\":\"test name\"}"];
    
    BOOL __block deleteSentToServer = NO;
    
    testFilter.ignoreNextFilter = YES;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        XCTAssertEqualObjects(request.HTTPMethod, @"DELETE", @"Incorrect operation (%@) sent to server", request.HTTPMethod);
        XCTAssertNil(request.allHTTPHeaderFields[@"If-Match"], @"If-Match header should have been nil");
        deleteSentToServer = YES;
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    
    // Create the item
    NSDictionary *item = @{ @"id": @"test1", @"name":@"test name" };
    
    // Insert the item
    done = NO;
    [todoTable delete:item completion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    done = NO;
    NSOperation *push = [client.syncContext pushWithCompletion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue(deleteSentToServer, @"the delete call didn't go to the server");
        done = YES;
    }];
	XCTAssertNotNil(push);
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteWithVersion_Push_ItemSentWithVersion_Success
{
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:200
                                                                 data:@"{\"id\": \"test1\", \"text\":\"test name\"}"];
    
    BOOL __block deleteSentToServer = NO;
    
    testFilter.ignoreNextFilter = YES;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        NSString *ifMatchHeader = request.allHTTPHeaderFields[@"If-Match"];
        XCTAssertTrue([ifMatchHeader isEqualToString:@"\"123\""], @"Unexpected header: %@", ifMatchHeader);
        deleteSentToServer = YES;
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    
    // Create the item
    NSDictionary *item = @{ @"id": @"test1", MSSystemColumnVersion: @"123", @"name":@"test name" };
    
    // Insert the item
    done = NO;
    [todoTable delete:item completion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:1000.1], @"Test timed out.");
    
    done = NO;
    NSOperation *push = [client.syncContext pushWithCompletion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue(deleteSentToServer, @"the delete call didn't go to the server");
        done = YES;
    }];
	XCTAssertNotNil(push);
    XCTAssertTrue([self waitForTest:2000.1], @"Test timed out.");
}

-(void) testDeleteInsert_Collapse_Throws
{
    MSSyncTable *todoTable = [client syncTableWithName:TodoTableNoVersion];
    
    NSDictionary *item = @{ @"id": @"itemA", @"text": @"throw an error" };
    
    [todoTable delete:item completion:^(NSError *error) {
        [todoTable insert:item completion:^(NSDictionary *itemTwo, NSError *error) {
            XCTAssertNotNil(error);
            XCTAssertEqual(error.code, MSSyncTableInvalidAction);
            done = YES;
        }];
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteUpdate_Collapse_Throws
{
    MSSyncTable *todoTable = [client syncTableWithName:TodoTableNoVersion];
    
    NSDictionary *item = @{ @"id": @"itemA", @"text": @"throw an error" };
    
    [todoTable delete:item completion:^(NSError *error) {
        [todoTable update:item completion:^(NSError *error) {
            XCTAssertNotNil(error);
            XCTAssertEqual(error.code, MSSyncTableInvalidAction);
            done = YES;
        }];
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteDelete_Collapse_Throws
{
    MSSyncTable *todoTable = [client syncTableWithName:TodoTableNoVersion];
    
    NSDictionary *item = @{ @"id": @"itemA", @"text": @"throw an error" };
    
    [todoTable delete:item completion:^(NSError *error) {
        [todoTable delete:item completion:^(NSError *error) {
            XCTAssertNotNil(error);
            XCTAssertEqual(error.code, MSSyncTableInvalidAction);
            done = YES;
        }];
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}


#pragma mark Read Tests


-(void) testReadWithIdNoItemSuccess
{
    offline.returnErrors = YES;
    
    MSSyncTable *todoTable = [client syncTableWithName:TodoTableNoVersion];
    
    [todoTable readWithId:@"10" completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(item, @"No item should have been found");
        XCTAssertNil(error, @"No error should have been returned");
    }];
}


#pragma mark Pull Tests


-(void) testPullSuccess
{
    NSString* stringData = @"[{\"id\": \"one\", \"text\":\"first item\", \"__updatedAt\":\"1999-12-03T15:44:29.0Z\"},{\"id\": \"two\", \"text\":\"second item\", \"__updatedAt\":\"1999-12-03T15:44:29.0Z\"}]";
    MSMultiRequestTestFilter *filter = [MSMultiRequestTestFilter testFilterWithStatusCodes:@[@200] data:@[stringData] appendEmptyRequest:YES];
    
    MSClient *filteredClient = [client clientWithFilter:filter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:@"TodoItem"];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    NSOperation *pull = [todoTable pullWithQuery:query queryId:nil completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        XCTAssertEqual(offline.upsertCalls, 1);
        XCTAssertEqual(offline.upsertedItems, 2);
        done = YES;
    }];
	
	XCTAssertNotNil(pull);
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    XCTAssertEqual(2, filter.actualRequests.count);
    
    NSURLRequest *firstRequest = (NSURLRequest *)filter.actualRequests[0];
    NSURLRequest *secondRequest = (NSURLRequest *)filter.actualRequests[1];
    
    NSArray *expectedFirstResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted%2C__version", @"$top=50", @"$skip=0"];
    XCTAssertTrue([self checkURL:firstRequest.URL withPath:@"/tables/TodoItem" andQuery:expectedFirstResult],
                  @"Invalid URL: %@", firstRequest.URL.absoluteString);

    NSArray *expectedSecondResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted%2C__version", @"$top=50", @"$skip=2"];
    XCTAssertTrue([self checkURL:secondRequest.URL withPath:@"/tables/TodoItem" andQuery:expectedSecondResult],
                  @"Invalid URL: %@", secondRequest.URL.absoluteString);
}

-(void) testPullWithoutVersion
{
    NSString* stringData = @"[{\"id\": \"one\", \"text\":\"first item\"},{\"id\": \"two\", \"text\":\"second item\"}]";
    MSMultiRequestTestFilter *filter = [MSMultiRequestTestFilter testFilterWithStatusCodes:@[@200] data:@[stringData] appendEmptyRequest:YES];
    
    offline.upsertCalls = 0;
    
    MSClient *filteredClient = [client clientWithFilter:filter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    NSOperation *pull = [todoTable pullWithQuery:query queryId:nil completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        XCTAssertEqual((int)offline.upsertCalls, 1, @"Unexpected number of upsert calls");
        XCTAssertEqual((int)offline.upsertedItems, 2, @"Unexpected number of upsert calls");
        done = YES;
    }];
	
	XCTAssertNotNil(pull);
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    XCTAssertEqual(2, filter.actualRequests.count);
    
    NSURLRequest *firstRequest = (NSURLRequest *)filter.actualRequests[0];
    NSURLRequest *secondRequest = (NSURLRequest *)filter.actualRequests[1];
    
    NSArray *expectedFirstResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=50", @"$skip=0"];
    XCTAssertTrue([self checkURL:firstRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedFirstResult],
                  @"Invalid URL: %@", firstRequest.URL.absoluteString);
    
    NSArray *expectedSecondResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=50", @"$skip=2"];
    XCTAssertTrue([self checkURL:secondRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedSecondResult],
                  @"Invalid URL: %@", secondRequest.URL.absoluteString);
}

-(void) testPullSuccessWithDeleted
{
    NSString* stringData = @"[{\"id\": \"one\", \"text\":\"first item\", \"__deleted\":false},{\"id\": \"two\", \"text\":\"second item\", \"__deleted\":true}, {\"id\": \"three\", \"text\":\"third item\", \"__deleted\":null}]";
    MSMultiRequestTestFilter *testFilter = [MSMultiRequestTestFilter testFilterWithStatusCodes:@[@200] data:@[stringData] appendEmptyRequest:YES];
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    NSOperation *pull = [todoTable pullWithQuery:query queryId:nil completion:^(NSError *error) {
        XCTAssertNil(error, @"Error occurred: %@", error.description);
        XCTAssertEqual((int)offline.upsertCalls, 1, @"Unexpected number of upsert calls");
        XCTAssertEqual((int)offline.upsertedItems, 2, @"Unexpected number of upsert calls");
        XCTAssertEqual((int)offline.deleteCalls, 1, @"Unexpected number of delete calls");
        XCTAssertEqual((int)offline.deletedItems, 1, @"Unexpected number of upsert calls");
        done = YES;
    }];
	
	XCTAssertNotNil(pull);
	
    XCTAssertTrue([self waitForTest:30.0], @"Test timed out.");
    
    NSURLRequest *firstRequest = testFilter.actualRequests[0];
    NSURLRequest *secondRequest = testFilter.actualRequests[1];
    
    NSArray *expectedFirstResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=50", @"$skip=0"];
    XCTAssertTrue([self checkURL:firstRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedFirstResult],
                  @"Invalid URL: %@", firstRequest.URL.absoluteString);
    NSArray *expectedSecondResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=50", @"$skip=3"];
    XCTAssertTrue([self checkURL:secondRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedSecondResult],
                  @"Invalid URL: %@", secondRequest.URL.absoluteString);
}

-(void) testPullWithPushSuccess
{
    NSString *insertResponse = @"{\"id\": \"one\", \"text\":\"first item\"}";
    NSString *pullResponse = @"[{\"id\": \"one\", \"text\":\"first item\"},{\"id\": \"two\", \"text\":\"second item\"}]";
    MSMultiRequestTestFilter *testFilter = [MSMultiRequestTestFilter testFilterWithStatusCodes:@[@200,@200] data:@[insertResponse,pullResponse] appendEmptyRequest:YES];
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    [todoTable insert:@{ @"id":@"test1", @"name": @"test one"} completion:^(NSDictionary *item, NSError *error) {
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:60.0], @"Test timed out.");
    
    done = NO;
    
    NSOperation *pull = [todoTable pullWithQuery:query queryId:nil completion:^(NSError *error) {
        XCTAssertNil(error, @"Unexpected error: %@", error.description);
        XCTAssertEqual(offline.upsertCalls, 4);
        XCTAssertEqual(offline.upsertedItems, 5);
        
        done = YES;
    }];
	
	XCTAssertNotNil(pull);
	
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    NSURLRequest *insertRequest = testFilter.actualRequests[0];
    NSURLRequest *firstPullRequest = testFilter.actualRequests[1];
    NSURLRequest *secondPullRequest = testFilter.actualRequests[2];
    XCTAssertEqualObjects(insertRequest.URL.absoluteString, @"https://someUrl/tables/TodoNoVersion?__systemProperties=__version");
    
    NSArray *expectedFirstPullResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=50", @"$skip=0"];
    XCTAssertTrue([self checkURL:firstPullRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedFirstPullResult],
                  @"Invalid URL: %@", firstPullRequest.URL.absoluteString);
    
    NSArray *expectedSecondPullResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=50", @"$skip=2"];
    XCTAssertTrue([self checkURL:secondPullRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedSecondPullResult],
                  @"Invalid URL: %@", secondPullRequest.URL.absoluteString);
}

-(void) testPullAddsProperFeaturesHeader
{
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:200 data:@"[]"];
    __block NSURLRequest *actualRequest = nil;
    
    testFilter.onInspectRequest = ^(NSURLRequest *request) {
        actualRequest = request;
        return request;
    };
    offline.upsertCalls = 0;
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:@"TodoItem"];
    MSQuery *query = [todoTable query];
    
    NSOperation *pull = [todoTable pullWithQuery:query queryId:nil completion:^(NSError *error) {
        XCTAssertNil(error, @"Unexpected error: %@", error.description);
        XCTAssertNotNil(actualRequest);
        
        NSString *featuresHeader = [actualRequest.allHTTPHeaderFields valueForKey:MSFeaturesHeaderName];
        XCTAssertNotNil(featuresHeader, @"actualHeader should not have been nil.");
        NSString *expectedFeatures = @"TQ,OL";
        XCTAssertTrue([featuresHeader isEqualToString:expectedFeatures], @"Header value (%@) was not as expected (%@)", featuresHeader, expectedFeatures);
        
        done = YES;
    }];
	
	XCTAssertNotNil(pull);
    
    XCTAssertTrue([self waitForTest:30.0], @"Test timed out.");
}

-(void) testIncrementalPullAddsProperFeaturesHeader
{
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:200 data:@"[]"];
    __block NSURLRequest *actualRequest = nil;
    
    testFilter.onInspectRequest = ^(NSURLRequest *request) {
        actualRequest = request;
        return request;
    };
    offline.upsertCalls = 0;
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:@"TodoItem"];
    MSQuery *query = [todoTable query];
    
    NSOperation *pull = [todoTable pullWithQuery:query queryId:@"something" completion:^(NSError *error) {
        XCTAssertNil(error, @"Unexpected error: %@", error.description);
        XCTAssertNotNil(actualRequest);
        
        NSString *featuresHeader = [actualRequest.allHTTPHeaderFields valueForKey:MSFeaturesHeaderName];
        XCTAssertNotNil(featuresHeader, @"actualHeader should not have been nil.");
        NSString *expectedFeatures = @"TQ,OL,IP";
        XCTAssertTrue([featuresHeader isEqualToString:expectedFeatures], @"Header value (%@) was not as expected (%@)", featuresHeader, expectedFeatures);
        
        done = YES;
    }];
	
	XCTAssertNotNil(pull);
    
    XCTAssertTrue([self waitForTest:30.0], @"Test timed out.");
}

-(void) testPullWithCustomParameters
{
    NSString* stringData = @"[{\"id\": \"one\", \"text\":\"first item\"},{\"id\": \"two\", \"text\":\"second item\"}]";
    MSMultiRequestTestFilter *testFilter = [MSMultiRequestTestFilter testFilterWithStatusCodes:@[@200] data:@[stringData] appendEmptyRequest:YES];
    
    offline.upsertCalls = 0;
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    query.parameters = @{@"mykey": @"myvalue"};
    
    NSOperation *pull = [todoTable pullWithQuery:query queryId:nil completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        XCTAssertEqual((int)offline.upsertCalls, 1, @"Unexpected number of upsert calls");
        XCTAssertEqual((int)offline.upsertedItems, 2, @"Unexpected number of upsert calls");
        done = YES;
    }];
	
	XCTAssertNotNil(pull);
    
    XCTAssertTrue([self waitForTest:30.0], @"Test timed out.");
    
    NSURLRequest *firstRequest = testFilter.actualRequests[0];
    NSURLRequest *secondRequest = testFilter.actualRequests[1];
    
    NSArray *expectedFirstResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=50", @"mykey=myvalue", @"$skip=0"];
    XCTAssertTrue([self checkURL:firstRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedFirstResult],
                  @"Invalid URL: %@", firstRequest.URL.absoluteString);
    
    NSArray *expectedSecondResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=50", @"$skip=2", @"mykey=myvalue"];
    XCTAssertTrue([self checkURL:secondRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedSecondResult],
                  @"Invalid URL: %@", secondRequest.URL.absoluteString);
}

-(void) testPullWithSystemPropertiesFails
{
    NSString* stringData = @"[{\"id\": \"one\", \"text\":\"first item\"},{\"id\": \"two\", \"text\":\"second item\"}]";
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:200 data:stringData];
    
    __block NSURLRequest *actualRequest = nil;
    testFilter.onInspectRequest = ^(NSURLRequest *request) {
        actualRequest = request;
        return request;
    };
    
    offline.upsertCalls = 0;
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    query.parameters = @{@"__systemProperties": @"__createdAt%2C__somethingRandom"};
    
    NSOperation *pull = [todoTable pullWithQuery:query queryId:nil completion:^(NSError *error) {
        XCTAssertNotNil(error);
        XCTAssertEqual(error.code, MSInvalidParameter);
        XCTAssertEqual((int)offline.upsertCalls, 0, @"Unexpected number of upsert calls");
        XCTAssertEqual((int)offline.upsertedItems, 0, @"Unexpected number of upsert calls");
        done = YES;
    }];
	
	XCTAssertNil(pull);
    
    XCTAssertTrue([self waitForTest:30.0], @"Test timed out.");
}

-(void) testPullWithFetchLimit
{
    // Pull should make requests with $top=50 until it has pulled all the data up to fetchLimit.
    
    // the ids don't really matter for this test
    NSString* fiftyItems = [MSMultiRequestTestFilter testDataWithItemCount:50 startId:0];
    
    MSMultiRequestTestFilter *testFilter = [MSMultiRequestTestFilter testFilterWithStatusCodes:@[@200, @200, @200] data:@[fiftyItems, fiftyItems, fiftyItems] appendEmptyRequest:NO];
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    query.fetchLimit = 150;
	
    NSOperation *pull = [todoTable pullWithQuery:query queryId:nil completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        done = YES;
    }];
	
	XCTAssertNotNil(pull);
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    XCTAssertEqual(3, testFilter.actualRequests.count);
    
    NSURLRequest *firstRequest = testFilter.actualRequests[0];
    NSURLRequest *secondRequest = testFilter.actualRequests[1];
    NSURLRequest *thirdRequest = testFilter.actualRequests[2];
    
    NSArray *expectedFirstResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=50", @"$skip=0"];
    XCTAssertTrue([self checkURL:firstRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedFirstResult],
                  @"Invalid URL: %@", firstRequest.URL.absoluteString);

    NSArray *expectedSecondResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=50", @"$skip=50"];
    XCTAssertTrue([self checkURL:secondRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedSecondResult],
                  @"Invalid URL: %@", secondRequest.URL.absoluteString);
    
    NSArray *expectedThirdResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=50", @"$skip=100"];
    XCTAssertTrue([self checkURL:thirdRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedThirdResult],
                  @"Invalid URL: %@", thirdRequest.URL.absoluteString);
}

-(void) testPullWithFetchLimitLargerThanItems
{
    // Pull should make requests with $top=50 until it finds an empty result.
    
    // the ids don't really matter for this test
    NSString* fiftyItems = [MSMultiRequestTestFilter testDataWithItemCount:50 startId:0];
    NSString* twentyFiveItems = [MSMultiRequestTestFilter testDataWithItemCount:25 startId:0];
    
    MSMultiRequestTestFilter *testFilter = [MSMultiRequestTestFilter testFilterWithStatusCodes:@[@200, @200, @200] data:@[fiftyItems, fiftyItems, twentyFiveItems] appendEmptyRequest:YES];
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    query.fetchLimit = 1000;
    
    NSOperation *pull = [todoTable pullWithQuery:query queryId:nil completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        done = YES;
    }];
	
	XCTAssertNotNil(pull);
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    XCTAssertEqual(4, testFilter.actualRequests.count);
    
    NSURLRequest *firstRequest = testFilter.actualRequests[0];
    NSURLRequest *secondRequest = testFilter.actualRequests[1];
    NSURLRequest *thirdRequest = testFilter.actualRequests[2];
    NSURLRequest *fourthRequest = testFilter.actualRequests[3];
    
    NSArray *expectedFirstResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=50", @"$skip=0"];
    XCTAssertTrue([self checkURL:firstRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedFirstResult],
                  @"Invalid URL: %@", firstRequest.URL.absoluteString);
    
    NSArray *expectedSecondResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=50", @"$skip=50"];
    XCTAssertTrue([self checkURL:secondRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedSecondResult],
                  @"Invalid URL: %@", secondRequest.URL.absoluteString);
    
    NSArray *expectedThirdResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=50", @"$skip=100"];
    XCTAssertTrue([self checkURL:thirdRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedThirdResult],
                  @"Invalid URL: %@", thirdRequest.URL.absoluteString);
    
    NSArray *expectedFourthResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=50", @"$skip=125"];
    XCTAssertTrue([self checkURL:fourthRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedFourthResult],
                  @"Invalid URL: %@", fourthRequest.URL.absoluteString);
}

-(void) testPullWithFetchLimitSmallerThanDefault
{
    // Pull should make one requests with $top=25.
    
    // the ids don't really matter for this test
    NSString* fiftyItems = [MSMultiRequestTestFilter testDataWithItemCount:50 startId:0];
    NSString* twentyFiveItems = [MSMultiRequestTestFilter testDataWithItemCount:25 startId:0];
    
    MSMultiRequestTestFilter *testFilter = [MSMultiRequestTestFilter testFilterWithStatusCodes:@[@200, @200, @200] data:@[fiftyItems, fiftyItems, twentyFiveItems] appendEmptyRequest:YES];
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    // we'd expect 4 calls ($top=25)
    query.fetchLimit = 25;
    
    NSOperation *pull = [todoTable pullWithQuery:query queryId:nil completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        done = YES;
    }];
	
	XCTAssertNotNil(pull);
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    XCTAssertEqual(1, testFilter.actualRequests.count);
    
    NSURLRequest *firstRequest = testFilter.actualRequests[0];
    
    NSArray *expectedFirstResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=25", @"$skip=0"];
    XCTAssertTrue([self checkURL:firstRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedFirstResult],
                  @"Invalid URL: %@", firstRequest.URL.absoluteString);
}

-(void) testPullWithFetchLimitWhenServerHasLowerLimit
{
    // Pull should make requests with $top=50 but skip in increments of 25 since that's what the server has returned to us.
    
    // the ids don't really matter for this test
    NSString* twentyFiveItems = [MSMultiRequestTestFilter testDataWithItemCount:25 startId:0];
    NSString* tenItems = [MSMultiRequestTestFilter testDataWithItemCount:10 startId:0]; // end on a non-divisible-by-25 number
    
    MSMultiRequestTestFilter *testFilter = [MSMultiRequestTestFilter testFilterWithStatusCodes:@[@200, @200, @200] data:@[twentyFiveItems, twentyFiveItems, tenItems] appendEmptyRequest:YES];
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    // we'd expect 4 calls ($top=50, 50, 50, 50) ($skip=0, 25, 50, 60)
    query.fetchLimit = 100;
    
    NSOperation *pull = [todoTable pullWithQuery:query queryId:nil completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        done = YES;
    }];
	
	XCTAssertNotNil(pull);
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    XCTAssertEqual(4, testFilter.actualRequests.count);
    
    NSURLRequest *firstRequest = testFilter.actualRequests[0];
    NSURLRequest *secondRequest = testFilter.actualRequests[1];
    NSURLRequest *thirdRequest = testFilter.actualRequests[2];
    NSURLRequest *fourthRequest = testFilter.actualRequests[3];
    
    NSArray *expectedFirstResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=50", @"$skip=0"];
    XCTAssertTrue([self checkURL:firstRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedFirstResult],
                  @"Invalid URL: %@", firstRequest.URL.absoluteString);
    
    NSArray *expectedSecondResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=50", @"$skip=25"];
    XCTAssertTrue([self checkURL:secondRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedSecondResult],
                  @"Invalid URL: %@", secondRequest.URL.absoluteString);
    
    NSArray *expectedThirdResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=50", @"$skip=50"];
    XCTAssertTrue([self checkURL:thirdRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedThirdResult],
                  @"Invalid URL: %@", thirdRequest.URL.absoluteString);
    
    NSArray *expectedFourthResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=40", @"$skip=60"];
    XCTAssertTrue([self checkURL:fourthRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedFourthResult],
                  @"Invalid URL: %@", fourthRequest.URL.absoluteString);
}

-(void) testPullWithFetchLimitNotMultipleOfFifty
{
    // Pull should make requests with $top=50 but skip in increments of 25 since that's what the server has returned to us.
    
    // the ids don't really matter for this test
    NSString* fiftyItems = [MSMultiRequestTestFilter testDataWithItemCount:50 startId:0];
    NSString* fortyEightItems = [MSMultiRequestTestFilter testDataWithItemCount:48 startId:0];
    
    MSMultiRequestTestFilter *testFilter = [MSMultiRequestTestFilter testFilterWithStatusCodes:@[@200, @200, @200] data:@[fiftyItems, fiftyItems, fortyEightItems] appendEmptyRequest:YES];
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    // we'd expect 4 calls ($top=50, 50, 50, 50) ($skip=0, 25, 50, 60)
    query.fetchLimit = 148;
    
    NSOperation *pull = [todoTable pullWithQuery:query queryId:nil completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        done = YES;
    }];
	
	XCTAssertNotNil(pull);
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    XCTAssertEqual(3, testFilter.actualRequests.count);
    
    NSURLRequest *firstRequest = testFilter.actualRequests[0];
    NSURLRequest *secondRequest = testFilter.actualRequests[1];
    NSURLRequest *thirdRequest = testFilter.actualRequests[2];
    
    NSArray *expectedFirstResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=50", @"$skip=0"];
    XCTAssertTrue([self checkURL:firstRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedFirstResult],
                  @"Invalid URL: %@", firstRequest.URL.absoluteString);
    
    NSArray *expectedSecondResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=50", @"$skip=50"];
    XCTAssertTrue([self checkURL:secondRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedSecondResult],
                  @"Invalid URL: %@", secondRequest.URL.absoluteString);
    
    NSArray *expectedThirdResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=48", @"$skip=100"];
    XCTAssertTrue([self checkURL:thirdRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedThirdResult],
                  @"Invalid URL: %@", thirdRequest.URL.absoluteString);
}

-(void) testPullWithFetchOffset
{
    // Pull should start with a $skip and the skip should be incremented by the number of records pulled
    // we'll simulate having 100 records and skipping the first 12 with no fetchLimit
    
    // the ids don't really matter for this test
    NSString* fiftyItems = [MSMultiRequestTestFilter testDataWithItemCount:50 startId:0];
    NSString* thirtyEightItems = [MSMultiRequestTestFilter testDataWithItemCount:38 startId:0];
    
    MSMultiRequestTestFilter *testFilter = [MSMultiRequestTestFilter testFilterWithStatusCodes:@[@200, @200] data:@[fiftyItems, thirtyEightItems] appendEmptyRequest:YES];
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    // we'd expect 3 calls ($top=50, 50, 50) ($skip=12, 62, 100)
    query.fetchOffset = 12;
    
    NSOperation *pull = [todoTable pullWithQuery:query queryId:nil completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        done = YES;
    }];
	
	XCTAssertNotNil(pull);
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    XCTAssertEqual(3, testFilter.actualRequests.count);
    
    NSURLRequest *firstRequest = testFilter.actualRequests[0];
    NSURLRequest *secondRequest = testFilter.actualRequests[1];
    NSURLRequest *thirdRequest = testFilter.actualRequests[2];
    
    NSArray *expectedFirstResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=50", @"$skip=12"];
    XCTAssertTrue([self checkURL:firstRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedFirstResult],
                  @"Invalid URL: %@", firstRequest.URL.absoluteString);
    
    NSArray *expectedSecondResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=50", @"$skip=62"];
    XCTAssertTrue([self checkURL:secondRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedSecondResult],
                  @"Invalid URL: %@", secondRequest.URL.absoluteString);
    
    NSArray *expectedThirdResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=50", @"$skip=100"];
    XCTAssertTrue([self checkURL:thirdRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedThirdResult],
                  @"Invalid URL: %@", thirdRequest.URL.absoluteString);
}

-(void) testPullWithFetchOffsetAndFetchLimit
{
    // Pull should start with a $skip and the skip should be incremented by the number of records pulled
    // we'll simulate having 100 records and skipping the first 12 with no fetchLimit
    
    // the ids don't really matter for this test
    NSString* fiftyItems = [MSMultiRequestTestFilter testDataWithItemCount:50 startId:0];
    NSString* tenItems = [MSMultiRequestTestFilter testDataWithItemCount:10 startId:0];
    
    MSMultiRequestTestFilter *testFilter = [MSMultiRequestTestFilter testFilterWithStatusCodes:@[@200, @200, @200] data:@[fiftyItems, fiftyItems, tenItems] appendEmptyRequest:NO];
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    // we'd expect 3 calls ($top=50, 50, 50) ($skip=12, 62, 100)
    query.fetchOffset = 12;
    query.fetchLimit = 110;
    
    NSOperation *pull = [todoTable pullWithQuery:query queryId:nil completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        done = YES;
    }];
	
	XCTAssertNotNil(pull);
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    XCTAssertEqual(3, testFilter.actualRequests.count);
    
    NSURLRequest *firstRequest = testFilter.actualRequests[0];
    NSURLRequest *secondRequest = testFilter.actualRequests[1];
    NSURLRequest *thirdRequest = testFilter.actualRequests[2];

    NSArray *expectedFirstResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=50", @"$skip=12"];
    XCTAssertTrue([self checkURL:firstRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedFirstResult],
                  @"Invalid URL: %@", firstRequest.URL.absoluteString);
    
    NSArray *expectedSecondResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=50", @"$skip=62"];
    XCTAssertTrue([self checkURL:secondRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedSecondResult],
                  @"Invalid URL: %@", secondRequest.URL.absoluteString);
    
    NSArray *expectedThirdResult = @[@"__includeDeleted=true", @"__systemProperties=__deleted", @"$top=10", @"$skip=112"];
    XCTAssertTrue([self checkURL:thirdRequest.URL withPath:@"/tables/TodoNoVersion" andQuery:expectedThirdResult],
                  @"Invalid URL: %@", thirdRequest.URL.absoluteString);
}

-(void) testPushAddsProperFeaturesHeader
{
    NSString* stringData = @"{\"id\": \"test1\", \"text\":\"test name\"}";
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:200 data:stringData];
    __block NSURLRequest *actualRequest = nil;
    
    testFilter.onInspectRequest = ^(NSURLRequest *request) {
        actualRequest = request;
        return request;
    };
	
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:@"TodoNoVersion"];
    
    // Create the item
    NSDictionary *item = @{ @"id": @"test1", @"name":@"test name" };
    
    // Insert the item
    done = NO;
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    done = NO;
    actualRequest = nil;
    NSOperation *push = [client.syncContext pushWithCompletion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertNotNil(actualRequest, @"actualRequest should not have been nil.");
        
        NSString *featuresHeader = [actualRequest.allHTTPHeaderFields valueForKey:MSFeaturesHeaderName];
        XCTAssertNotNil(featuresHeader, @"actualHeader should not have been nil.");
        NSString *expectedFeatures = @"OL";
        XCTAssertTrue([featuresHeader isEqualToString:expectedFeatures], @"Header value (%@) was not as expected (%@)", featuresHeader, expectedFeatures);
        
        done = YES;
    }];
	XCTAssertNotNil(push);
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testIncrementalPullSuccess
{
    NSString* stringData1 = @"[{\"id\": \"one\", \"text\":\"first item\",\"__version\":\"AAAAAAAAHzg=\",\"__deleted\":false,\"__updatedAt\":\"1999-12-03T15:44:29.0Z\"},{\"id\": \"two\", \"text\":\"second item\", \"__updatedAt\":\"1999-12-03T15:44:29.0Z\"}]";
    NSString* stringData2 = @"[{\"id\": \"three\", \"text\":\"first item\", \"__updatedAt\":\"1999-12-04T16:44:29.0Z\"},{\"id\": \"four\", \"text\":\"second item\", \"__updatedAt\":\"1999-12-04T16:44:59.0Z\"}]";
    MSMultiRequestTestFilter *filter = [MSMultiRequestTestFilter testFilterWithStatusCodes:@[@200,@200,@200] data:@[stringData1,stringData2,@"[]"] appendEmptyRequest:YES];
    
    MSClient *filteredClient = [client clientWithFilter:filter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:@"TodoItem"];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    // TODO: look at if we want to reenable this
    // Verifies that we allow __includeDeleted=YES
    // query.parameters = @{@"__includeDeleted":@YES};
    
    NSOperation *pull = [todoTable pullWithQuery:query queryId:@"test_1" completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        XCTAssertEqual(offline.upsertCalls, 4);
        XCTAssertEqual(offline.upsertedItems, 6);
        // readWithQuery is only called once to get the initial deltaToken. otherwise, it's cached.
        // the other six calls are for each of the upserts (they all return nothing).
        // TODO -- see if i can do this without creating a new MSSyncTable internally.
        XCTAssertEqual(offline.readWithQueryCalls, 6);
        XCTAssertEqual(offline.readWithQueryItems, 0);
        XCTAssertEqual(offline.readTableCalls, 0);
        done = YES;
    }];
	
	XCTAssertNotNil(pull);
    
    XCTAssertTrue([self waitForTest:1000.1], @"Test timed out.");
    done = NO;
    XCTAssertEqual(3, filter.actualRequests.count);
    
    NSURLRequest *firstRequest = (NSURLRequest *)filter.actualRequests[0];
    NSURLRequest *secondRequest = (NSURLRequest *)filter.actualRequests[1];
    NSURLRequest *thirdRequest = (NSURLRequest *)filter.actualRequests[2];

    NSArray *expectedfirstRequestQuery = @[@"__includeDeleted=true",
                                           @"__systemProperties=__updatedAt%2C__deleted%2C__version",
                                           @"$filter=(__updatedAt%20ge%20datetimeoffset'1970-01-01T00%3A00%3A00.000Z')",
                                           @"$orderby=__updatedAt%20asc",
                                           @"$skip=0",
                                           @"$top=50"];
    XCTAssertTrue([self checkURL:firstRequest.URL
                        withPath:@"/tables/TodoItem"
                        andQuery:expectedfirstRequestQuery],
                  @"Invalue URL: %@", firstRequest.URL.absoluteString);
    
    NSArray *expectedSecondRequestQuery = @[@"__includeDeleted=true",
                                            @"__systemProperties=__updatedAt%2C__deleted%2C__version",
                                            @"$filter=(__updatedAt%20ge%20datetimeoffset'1999-12-03T15%3A44%3A29.000Z')",
                                            @"$orderby=__updatedAt%20asc",
                                            @"$skip=0",
                                            @"$top=50"];
    XCTAssertTrue([self checkURL:secondRequest.URL
                        withPath:@"/tables/TodoItem"
                        andQuery:expectedSecondRequestQuery],
                  @"Invalue URL: %@", secondRequest.URL.absoluteString);

    
    NSArray *expectedThirdRequestQuery = @[@"__includeDeleted=true",
                                           @"__systemProperties=__updatedAt%2C__deleted%2C__version",
                                           @"$filter=(__updatedAt%20ge%20datetimeoffset'1999-12-04T16%3A44%3A59.000Z')",
                                           @"$orderby=__updatedAt%20asc",
                                           @"$skip=0",
                                           @"$top=50"];
    XCTAssertTrue([self checkURL:thirdRequest.URL
                        withPath:@"/tables/TodoItem"
                        andQuery:expectedThirdRequestQuery],
                  @"Invalue URL: %@", thirdRequest.URL.absoluteString);
    
    
    // now try again and make sure we start with the same deltaToken
    [offline resetCounts];
    pull = [todoTable pullWithQuery:query queryId:@"test_1" completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        XCTAssertEqual(offline.upsertCalls, 0);
        XCTAssertEqual(offline.upsertedItems, 0);
        // readWithQuery is only called once to get the initial deltaToken. otherwise, it's cached.
        XCTAssertEqual(offline.readWithQueryCalls, 2);
        XCTAssertEqual(offline.readWithQueryItems, 1);
        XCTAssertEqual(offline.readTableCalls, 0);
        done = YES;
    }];
	
	XCTAssertNotNil(pull);
    
    XCTAssertTrue([self waitForTest:10000.1], @"Test timed out.");
    XCTAssertEqual(4, filter.actualRequests.count);
    
    NSURLRequest *fourthRequest = (NSURLRequest *)filter.actualRequests[3];

    NSArray *expectedFourthRequestQuery = @[@"__includeDeleted=true",
                                            @"__systemProperties=__updatedAt%2C__deleted%2C__version",
                                            @"$filter=(__updatedAt%20ge%20datetimeoffset'1999-12-04T16%3A44%3A59.000Z')",
                                            @"$orderby=__updatedAt%20asc",
                                            @"$skip=0",
                                            @"$top=50"];
    XCTAssertTrue([self checkURL:fourthRequest.URL
                        withPath:@"/tables/TodoItem"
                        andQuery:expectedFourthRequestQuery],
                  @"Invalue URL: %@", fourthRequest.URL.absoluteString);
}

-(void) testIncrementalPullWithOrderByFails
{
    MSMultiRequestTestFilter *filter = [MSMultiRequestTestFilter testFilterWithStatusCodes:@[] data:@[] appendEmptyRequest:YES];
    
    MSClient *filteredClient = [client clientWithFilter:filter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:@"TodoItem"];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    NSSortDescriptor *orderByText = [NSSortDescriptor sortDescriptorWithKey:MSSystemColumnId ascending:NO];
    query.orderBy = @[orderByText];
    
    // without queryId, it should work
    NSOperation *pull = [todoTable pullWithQuery:query queryId:nil completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        XCTAssertEqual(offline.upsertCalls, 0);
        XCTAssertEqual(offline.upsertedItems, 0);
        XCTAssertEqual(offline.readTableCalls, 0);
        done = YES;
    }];
	
	XCTAssertNotNil(pull);
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    done = NO;
    XCTAssertEqual(1, filter.actualRequests.count);
    
    NSURLRequest *firstRequest = (NSURLRequest *)filter.actualRequests[0];
    
    NSArray *expectedFirstRequestQuery = @[@"__includeDeleted=true",
                                           @"__systemProperties=__deleted%2C__version",
                                           @"$orderby=id%20desc",
                                           @"$skip=0",
                                           @"$top=50"];
    XCTAssertTrue([self checkURL:firstRequest.URL
                        withPath:@"/tables/TodoItem"
                        andQuery:expectedFirstRequestQuery],
                  @"Invalue URL: %@", firstRequest.URL.absoluteString);
    
    [offline resetCounts];
    // with queryId, this should produce an error
    pull = [todoTable pullWithQuery:query queryId:@"test_1" completion:^(NSError *error) {
        XCTAssertNotNil(error);
        XCTAssertEqual(MSInvalidParameter, error.code);
        XCTAssertEqual(offline.upsertCalls, 0);
        XCTAssertEqual(offline.upsertedItems, 0);
        XCTAssertEqual(offline.readWithQueryCalls, 0);
        XCTAssertEqual(offline.readTableCalls, 0);
        done = YES;
    }];
	
	XCTAssertNil(pull);
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    XCTAssertEqual(1, filter.actualRequests.count);
}

-(void) testPullWithIncludeDeletedReturnsErrorIfNotTrue
{
    MSSyncTable *todoTable = [client syncTableWithName:@"TodoItem"];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    query.parameters = @{@"__includeDeleted":@NO};
    
    void (^completion)(NSError*) = ^(NSError *error)
    {
        XCTAssertNotNil(error);
        XCTAssertEqual(error.code, MSInvalidParameter);
        XCTAssertEqual(offline.upsertCalls, 0, @"Unexpected number of upsert calls");
        XCTAssertEqual(offline.upsertedItems, 0, @"Unexpected number of upsert calls");
        done = YES;
    };
    
    NSOperation *pull = [todoTable pullWithQuery:query queryId:nil completion:completion];
	XCTAssertNil(pull);
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    done = NO;
    
    query.parameters = @{@"__includeDeleted":@YES, @"__INCLUDEDELETED":@NO};
    pull = [todoTable pullWithQuery:query queryId:nil completion:completion];
	XCTAssertNil(pull);
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    done = NO;
    
    query.parameters = @{@"__includeDeleted":@"NONSENSE"};
    pull = [todoTable pullWithQuery:query queryId:nil completion:completion];
	XCTAssertNil(pull);
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testIncrementalPullWithInsert
{
    // insert in the middle of a Pull should ignore the Insert and continue
    NSString *firstPullData = @"[{\"id\": \"one\", \"text\":\"first item\", \"__updatedAt\":\"1999-12-03T15:44:29.0Z\"},{\"id\": \"two\", \"text\":\"second item\", \"__updatedAt\":\"1999-12-03T15:44:28.0Z\"}]";
    NSString *secondPullData = @"[{\"id\": \"three\", \"text\":\"third item\", \"__updatedAt\":\"1999-12-03T15:44:29.0Z\"},{\"id\": \"four\", \"text\":\"fourth item\", \"__updatedAt\":\"1999-12-07T15:44:28.0Z\"}]";
    MSMultiRequestTestFilter *filter = [MSMultiRequestTestFilter testFilterWithStatusCodes:@[@200,@200] data:@[firstPullData,secondPullData] appendEmptyRequest:YES];
    
    MSClient *filteredClient = [client clientWithFilter:filter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:@"TodoItem"];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    // hijack the first filter request to insert
    __block NSURLRequest *firstPullRequest = nil;
    ((MSTestFilter *)filter.testFilters[0]).onInspectRequest = ^(NSURLRequest *request) {
        NSDictionary *item = @{ @"id": @"five", @"name":@"fifth item" };
        MSTestWaiter *insertWaiter = [MSTestWaiter new];
        
        [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
            XCTAssertNil(error, @"error should have been nil.");
            insertWaiter.done = YES;
        }];
        
        XCTAssertTrue([insertWaiter waitForTest:0.1], @"Test timed out.");
        
        firstPullRequest = request;
        return request;
    };
    
    NSOperation *pull = [todoTable pullWithQuery:query queryId:@"test-1" completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        XCTAssertEqual(offline.upsertCalls, 6);
        XCTAssertEqual(offline.upsertedItems, 8);
        NSLog(@"done pull");
        done = YES;
    }];
	
	XCTAssertNotNil(pull);
    
    XCTAssertTrue([self waitForTest:30.0], @"Test timed out.");
    
    // since we hijacked the first request, the 0th one is actually the second
    NSURLRequest *secondPullRequest = (NSURLRequest *)filter.actualRequests[0];
    NSURLRequest *thirdPullRequest = (NSURLRequest *)filter.actualRequests[1];
    
    NSArray *expectedFirstPullRequestQuery = @[@"__includeDeleted=true",
                                               @"__systemProperties=__updatedAt%2C__deleted%2C__version",
                                               @"$filter=(__updatedAt%20ge%20datetimeoffset'1970-01-01T00%3A00%3A00.000Z')",
                                               @"$orderby=__updatedAt%20asc",
                                               @"$top=50",
                                               @"$skip=0"];
    XCTAssertTrue([self checkURL:firstPullRequest.URL
                        withPath:@"/tables/TodoItem"
                        andQuery:expectedFirstPullRequestQuery],
                  @"Invalue URL: %@", firstPullRequest.URL.absoluteString);

    NSArray *expectedSecondPullRequestQuery = @[@"__includeDeleted=true",
                                                @"__systemProperties=__updatedAt%2C__deleted%2C__version",
                                                @"$filter=(__updatedAt%20ge%20datetimeoffset'1999-12-03T15%3A44%3A29.000Z')",
                                                @"$orderby=__updatedAt%20asc",
                                                @"$top=50",
                                                @"$skip=0"];
    XCTAssertTrue([self checkURL:secondPullRequest.URL
                        withPath:@"/tables/TodoItem"
                        andQuery:expectedSecondPullRequestQuery],
                  @"Invalue URL: %@", secondPullRequest.URL.absoluteString);
    

    NSArray *expectedThirdPullRequestQuery = @[@"__includeDeleted=true",
                                               @"__systemProperties=__updatedAt%2C__deleted%2C__version",
                                               @"$filter=(__updatedAt%20ge%20datetimeoffset'1999-12-07T15%3A44%3A28.000Z')",
                                               @"$orderby=__updatedAt%20asc",
                                               @"$top=50",
                                               @"$skip=0"];
    XCTAssertTrue([self checkURL:thirdPullRequest.URL
                        withPath:@"/tables/TodoItem"
                        andQuery:expectedThirdPullRequestQuery],
                  @"Invalue URL: %@", thirdPullRequest.URL.absoluteString);
    
    XCTAssertEqual(client.syncContext.pendingOperationsCount, 1);
}

-(void) testIncrementalPullWithUpdateSkipsUpdatedItem
{
    NSString *insertResponse = @"{\"id\": \"one\", \"text\":\"first item\", \"__updatedAt\":\"1999-12-03T15:44:29.0Z\"}";
    NSString *firstPullData = @"[{\"id\": \"one\", \"text\":\"first item\", \"__updatedAt\":\"1999-12-03T15:44:29.0Z\"},{\"id\": \"two\", \"text\":\"second item\", \"__updatedAt\":\"1999-12-03T15:44:28.0Z\"}]";
    NSString *secondPullData = @"[{\"id\": \"three\", \"text\":\"third item\", \"__updatedAt\":\"1999-12-03T15:44:29.0Z\"},{\"id\": \"four\", \"text\":\"fourth item\", \"__updatedAt\":\"1999-12-07T15:44:28.0Z\"}]";
    MSMultiRequestTestFilter *filter = [MSMultiRequestTestFilter testFilterWithStatusCodes:@[@201,@200,@200] data:@[insertResponse,firstPullData,secondPullData] appendEmptyRequest:YES];
    
    MSClient *filteredClient = [client clientWithFilter:filter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:@"TodoItem"];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    // insert an item that we'll update later
    NSDictionary *itemToInsert = @{ @"id": @"one", @"name":@"first item" };
    [todoTable insert:itemToInsert completion:^(NSDictionary *item, NSError *error) {
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    done = NO;
    
    // hijack the second filter request (the first pull) to update an item
    __block NSURLRequest *firstPullRequest = nil;
    ((MSTestFilter *)filter.testFilters[1]).onInspectRequest = ^(NSURLRequest *request) {
        NSDictionary *item = @{ @"id": @"one", @"name":@"UPDATED first item" };
        MSTestWaiter *updateWaiter = [MSTestWaiter new];
        
        [todoTable update:item completion:^(NSError *error) {
            XCTAssertNil(error, @"error should have been nil.");
            updateWaiter.done = YES;
        }];
        
        XCTAssertTrue([updateWaiter waitForTest:0.1], @"Test timed out.");
        
        firstPullRequest = request;
        return request;
    };
    
    NSOperation *pull = [todoTable pullWithQuery:query queryId:@"test-1" completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        XCTAssertEqual(offline.upsertCalls, 9);
        XCTAssertEqual(offline.upsertedItems, 10);
        XCTAssertEqual(offline.deleteCalls, 5);
        // the pending op gets deleted on push
        XCTAssertEqual(offline.deletedItems, 1);
        done = YES;
    }];
	
	XCTAssertNotNil(pull);
    
    XCTAssertTrue([self waitForTest:30.0], @"Test timed out.");
    
    // since we hijacked the second request and the first request is an insert, the item at [1] is actually the second pull
    NSURLRequest *insertRequest = (NSURLRequest *)filter.actualRequests[0];
    NSURLRequest *secondPullRequest = (NSURLRequest *)filter.actualRequests[1];
    NSURLRequest *thirdPullRequest = (NSURLRequest *)filter.actualRequests[2];
    
    XCTAssertEqualObjects(insertRequest.URL.absoluteString, @"https://someUrl/tables/TodoItem?__systemProperties=__version");
    NSArray *expectedfirstPullRequest = @[@"__includeDeleted=true",
                                          @"__systemProperties=__updatedAt%2C__deleted%2C__version",
                                          @"$filter=(__updatedAt%20ge%20datetimeoffset'1970-01-01T00%3A00%3A00.000Z')",
                                          @"$orderby=__updatedAt%20asc",
                                          @"$top=50",
                                          @"$skip=0"];

    XCTAssertTrue([self checkURL:firstPullRequest.URL withPath:@"/tables/TodoItem" andQuery:expectedfirstPullRequest],
                  @"Invalid URL: %@", firstPullRequest.URL.absoluteString);

    
    NSArray *expectedSecondPullRequest = @[@"__includeDeleted=true",
                                           @"__systemProperties=__updatedAt%2C__deleted%2C__version",
                                           @"$filter=(__updatedAt%20ge%20datetimeoffset'1999-12-03T15%3A44%3A29.000Z')",
                                           @"$orderby=__updatedAt%20asc",
                                           @"$top=50",
                                           @"$skip=0"];
    XCTAssertTrue([self checkURL:secondPullRequest.URL withPath:@"/tables/TodoItem" andQuery:expectedSecondPullRequest],
                  @"Invalid URL: %@", secondPullRequest.URL.absoluteString);

    NSArray *expectedThirdPullRequest = @[@"__includeDeleted=true",
                                          @"__systemProperties=__updatedAt%2C__deleted%2C__version",
                                          @"$filter=(__updatedAt%20ge%20datetimeoffset'1999-12-07T15%3A44%3A28.000Z')",
                                          @"$orderby=__updatedAt%20asc",
                                          @"$top=50",
                                          @"$skip=0"];
    XCTAssertTrue([self checkURL:thirdPullRequest.URL withPath:@"/tables/TodoItem" andQuery:expectedThirdPullRequest],
                  @"Invalid URL: %@", thirdPullRequest.URL.absoluteString);
    
    
    XCTAssertEqual(client.syncContext.pendingOperationsCount, 1);
}

-(void) testIncrementalPullRevertsToSkipAndBack
{
    // the first response has different _updatedAts so the deltaToken is adjusted with no skip
    // the second response has the same _updatedAts so it should fall back to skip
    // the third response also has the same __updatedAts so it should use skip
    // the fourth response has different __updatedAts so it should move back to use deltaToken with no skip
    // the fifth response has used >= __updateAt so it returns the seventh item alone, which we've already seen
    NSString* stringData1 = @"[{\"id\":\"one\",\"text\":\"first item\",\"__version\":\"1\",\"__deleted\":false,\"__updatedAt\":\"1999-12-03T15:44:29.0Z\"},{\"id\":\"two\",\"__version\":\"1\",\"__deleted\":false,\"text\":\"second item\", \"__updatedAt\":\"2000-01-01T00:00:00.0Z\"}]";
    NSString* stringData2 = @"[{\"id\": \"two\",\"text\":\"second item\",\"__version\":\"1\",\"__deleted\":false,\"__updatedAt\":\"2000-01-01T00:00:00.0Z\"},{\"id\": \"three\",\"text\":\"third item\",\"__version\":\"1\",\"__deleted\":false,\"__updatedAt\":\"2000-01-01T00:00:00.0Z\"}]";
    NSString* stringData3 = @"[{\"id\": \"four\",\"text\":\"fourth item\",\"__version\":\"1\",\"__deleted\":false,\"__updatedAt\":\"2000-01-01T00:00:00.0Z\"},{\"id\": \"five\",\"text\":\"fifth item\",\"__version\":\"1\",\"__deleted\":false,\"__updatedAt\":\"2000-01-01T00:00:00.0Z\"}]";
    NSString* stringData4 = @"[{\"id\": \"six\",\"text\":\"sixth item\",\"__version\":\"1\",\"__deleted\":false,\"__updatedAt\":\"2000-01-01T00:00:00.0Z\"},{\"id\": \"seven\",\"text\":\"seventh item\",\"__version\":\"1\",\"__deleted\":false,\"__updatedAt\":\"2000-01-02T00:00:00.0Z\"}]";
    NSString* stringData5 = @"[{\"id\": \"seven\",\"text\":\"seventh item\",\"__version\":\"1\",\"__deleted\":false,\"__updatedAt\":\"2000-01-02T00:00:00.0Z\"}]";
    MSMultiRequestTestFilter *filter = [MSMultiRequestTestFilter testFilterWithStatusCodes:@[@200,@200,@200,@200,@200] data:@[stringData1,stringData2,stringData3,stringData4,stringData5] appendEmptyRequest:YES];
    
    MSClient *filteredClient = [client clientWithFilter:filter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:@"TodoItem"];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    NSOperation *pull = [todoTable pullWithQuery:query queryId:@"test_1" completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        XCTAssertEqual(offline.upsertCalls, 7);
        XCTAssertEqual(offline.upsertedItems, 11);
        done = YES;
    }];
	
	XCTAssertNotNil(pull);
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    NSURLRequest *firstRequest = (NSURLRequest *)filter.actualRequests[0];
    NSURLRequest *secondRequest = (NSURLRequest *)filter.actualRequests[1];
    NSURLRequest *thirdRequest = (NSURLRequest *)filter.actualRequests[2];
    NSURLRequest *fourthRequest = (NSURLRequest *)filter.actualRequests[3];
    NSURLRequest *fifthRequest = (NSURLRequest *)filter.actualRequests[4];
    NSURLRequest *sixthRequest = (NSURLRequest *)filter.actualRequests[5];
    
    XCTAssertEqual(6, filter.actualRequests.count);

    NSArray *expectedFirstRequest = @[@"__includeDeleted=true",
                                      @"__systemProperties=__updatedAt%2C__deleted%2C__version",
                                      @"$filter=(__updatedAt%20ge%20datetimeoffset'1970-01-01T00%3A00%3A00.000Z')",
                                      @"$orderby=__updatedAt%20asc",
                                      @"$top=50",
                                      @"$skip=0"];
    XCTAssertTrue([self checkURL:firstRequest.URL withPath:@"/tables/TodoItem" andQuery:expectedFirstRequest],
                  @"Invalid URL: %@", firstRequest.URL.absoluteString);
    
    NSArray *expectedSecondRequest = @[@"__includeDeleted=true",
                                       @"__systemProperties=__updatedAt%2C__deleted%2C__version",
                                       @"$filter=(__updatedAt%20ge%20datetimeoffset'2000-01-01T00%3A00%3A00.000Z')",
                                       @"$orderby=__updatedAt%20asc",
                                       @"$top=50",
                                       @"$skip=0"];
    XCTAssertTrue([self checkURL:secondRequest.URL withPath:@"/tables/TodoItem" andQuery:expectedSecondRequest],
                  @"Invalid URL: %@", secondRequest.URL.absoluteString);
    
    // TODO: why does the ordering of $orderby and __includeDeleted change here?
    NSArray *expectedThirdRequest = @[@"__includeDeleted=true",
                                      @"__systemProperties=__updatedAt%2C__deleted%2C__version",
                                      @"$filter=(__updatedAt%20ge%20datetimeoffset'2000-01-01T00%3A00%3A00.000Z')",
                                      @"$orderby=__updatedAt%20asc",
                                      @"$skip=2",
                                      @"$top=50"];
    XCTAssertTrue([self checkURL:thirdRequest.URL withPath:@"/tables/TodoItem" andQuery:expectedThirdRequest],
                  @"Invalid URL: %@", thirdRequest.URL.absoluteString);

    // TODO: why does the ordering of $orderby and __includeDeleted change here?
    NSArray *expectedFourthRequest = @[@"__includeDeleted=true",
                                       @"__systemProperties=__updatedAt%2C__deleted%2C__version",
                                       @"$filter=(__updatedAt%20ge%20datetimeoffset'2000-01-01T00%3A00%3A00.000Z')",
                                       @"$orderby=__updatedAt%20asc",
                                       @"$skip=4",
                                       @"$top=50"];
    XCTAssertTrue([self checkURL:fourthRequest.URL withPath:@"/tables/TodoItem" andQuery:expectedFourthRequest],
                  @"Invalid URL: %@", fourthRequest.URL.absoluteString);
    
    NSArray *expectedFifthRequest = @[@"__includeDeleted=true",
                                      @"__systemProperties=__updatedAt%2C__deleted%2C__version",
                                      @"$filter=(__updatedAt%20ge%20datetimeoffset'2000-01-02T00%3A00%3A00.000Z')",
                                      @"$orderby=__updatedAt%20asc",
                                      @"$top=50",
                                      @"$skip=0"];
    XCTAssertTrue([self checkURL:fifthRequest.URL withPath:@"/tables/TodoItem" andQuery:expectedFifthRequest],
                  @"Invalid URL: %@", fifthRequest.URL.absoluteString);

    NSArray *expectedSixthRequest = @[@"__includeDeleted=true",
                                      @"__systemProperties=__updatedAt%2C__deleted%2C__version",
                                      @"$filter=(__updatedAt%20ge%20datetimeoffset'2000-01-02T00%3A00%3A00.000Z')",
                                      @"$orderby=__updatedAt%20asc",
                                      @"$skip=1",
                                      @"$top=50"];
    
    XCTAssertTrue([self checkURL:sixthRequest.URL withPath:@"/tables/TodoItem" andQuery:expectedSixthRequest],
                  @"Invalid URL: %@", sixthRequest.URL.absoluteString);
}

-(void) testIncrementalPullAppendsFilter
{
    NSString* stringData1 = @"[{\"id\":\"one\",\"text\":\"MATCH\",\"__version\":\"1\",\"__deleted\":false,\"__updatedAt\":\"1999-12-03T15:44:29.0Z\"},{\"id\":\"two\",\"__version\":\"1\",\"__deleted\":false,\"text\":\"MATCH\", \"__updatedAt\":\"2000-01-01T00:00:00.0Z\"}]";
    NSString* stringData2 = @"[{\"id\":\"two\",\"__version\":\"1\",\"__deleted\":false,\"text\":\"MATCH\", \"__updatedAt\":\"2000-01-01T00:00:00.0Z\"}]";
    MSMultiRequestTestFilter *filter = [MSMultiRequestTestFilter testFilterWithStatusCodes:@[@200,@200] data:@[stringData1,stringData2] appendEmptyRequest:YES];
    
    MSClient *filteredClient = [client clientWithFilter:filter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:@"TodoItem"];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    query.predicate = [NSPredicate predicateWithFormat:@"text == 'MATCH'"];
    
    NSOperation *pull = [todoTable pullWithQuery:query queryId:@"test_1" completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        XCTAssertEqual(offline.upsertCalls, 3);
        XCTAssertEqual(offline.upsertedItems, 4);
        done = YES;
    }];
	
	XCTAssertNotNil(pull);
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    XCTAssertEqual(3, filter.actualRequests.count);
    
    NSURLRequest *firstRequest = (NSURLRequest *)filter.actualRequests[0];
    NSURLRequest *secondRequest = (NSURLRequest *)filter.actualRequests[1];
    NSURLRequest *thirdRequest = (NSURLRequest *)filter.actualRequests[2];
    
    NSArray *expectedFirstRequest = @[@"__includeDeleted=true",
                                      @"__systemProperties=__updatedAt%2C__deleted%2C__version",
                                      @"$filter=((text%20eq%20'MATCH')%20and%20(__updatedAt%20ge%20datetimeoffset'1970-01-01T00%3A00%3A00.000Z'))",
                                      @"$orderby=__updatedAt%20asc",
                                      @"$top=50",
                                      @"$skip=0"];
    XCTAssertTrue([self checkURL:firstRequest.URL withPath:@"/tables/TodoItem" andQuery:expectedFirstRequest],
                  @"Invalid URL: %@", firstRequest.URL.absoluteString);

    NSArray *expectedSecondRequest = @[@"__includeDeleted=true",
                                       @"__systemProperties=__updatedAt%2C__deleted%2C__version",
                                       @"$filter=((text%20eq%20'MATCH')%20and%20(__updatedAt%20ge%20datetimeoffset'2000-01-01T00%3A00%3A00.000Z'))",
                                       @"$orderby=__updatedAt%20asc",
                                       @"$top=50",
                                       @"$skip=0"];
    XCTAssertTrue([self checkURL:secondRequest.URL withPath:@"/tables/TodoItem" andQuery:expectedSecondRequest],
                  @"Invalid URL: %@", secondRequest.URL.absoluteString);
    

    NSArray *expectedThirdRequest = @[@"__includeDeleted=true",
                                      @"__systemProperties=__updatedAt%2C__deleted%2C__version",
                                      @"$filter=((text%20eq%20'MATCH')%20and%20(__updatedAt%20ge%20datetimeoffset'2000-01-01T00%3A00%3A00.000Z'))",
                                      @"$orderby=__updatedAt%20asc",
                                      @"$skip=1",
                                      @"$top=50"];
    XCTAssertTrue([self checkURL:thirdRequest.URL withPath:@"/tables/TodoItem" andQuery:expectedThirdRequest],
                  @"Invalid URL: %@", thirdRequest.URL.absoluteString);

}

-(void) testDeltaTokenFailureAbortsPullOperation
{
    // passing a response, but this should never get called.
    MSMultiRequestTestFilter *filter = [MSMultiRequestTestFilter testFilterWithStatusCodes:@[] data:@[] appendEmptyRequest:YES];
    
    MSClient *testClient = [client clientWithFilter:filter];
    MSSyncTable *todoTable = [testClient syncTableWithName:@"DoesNotMatter"];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    offline.errorOnReadWithQueryOrError = true;
    
    // before the bug was fixed, an error would call the completion but continue to call the pull.
    // this fails with the bug but passes now that it's been fixed.
    __block BOOL alreadyCalled = NO;
    NSOperation *pull = [todoTable pullWithQuery:query queryId:@"something" completion:^(NSError *error) {
        XCTAssertNotNil(error);
        XCTAssertFalse(alreadyCalled);
        alreadyCalled = YES;
        done = YES;
    }];
	
	XCTAssertNotNil(pull);
    
    XCTAssertTrue([self waitForTest:0.1]);
    done = NO;
    XCTAssertFalse([self waitForTest:0.1]);
}

-(void) testIncrementalPullWithFetchLimitOrFetchOffsetErrors
{
    MSSyncTable *todoTable = [client syncTableWithName:@"TodoItem"];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    void (^completion)(NSError*) = ^(NSError *error)
    {
        XCTAssertNotNil(error);
        XCTAssertEqual(error.code, MSInvalidParameter);
        XCTAssertEqual(offline.upsertCalls, 0, @"Unexpected number of upsert calls");
        XCTAssertEqual(offline.upsertedItems, 0, @"Unexpected number of upsert calls");
        done = YES;
    };
    
    // verify error if fetchOffset is set
    query.fetchOffset = 10;
    NSOperation *pull = [todoTable pullWithQuery:query queryId:@"todo" completion:completion];
	XCTAssertNil(pull);
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    done = NO;
    
    // verify error if both fetchLimit and fetchOffset are set
    query.fetchLimit = 10;
    pull = [todoTable pullWithQuery:query queryId:@"todo" completion:completion];
	XCTAssertNil(pull);
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    done = NO;
    
    // now reset fetchOffset back to -1 and verify error if only fetchLimit is set
    query.fetchOffset = -1;
    pull = [todoTable pullWithQuery:query queryId:@"todo" completion:completion];
	XCTAssertNil(pull);
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}


#pragma mark Purge Tests


-(void) testPurgeWithEmptyQueueSuccess
{
    MSSyncTable *todoTable = [client syncTableWithName:TodoTableNoVersion];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    query.predicate = [NSPredicate predicateWithFormat:@"text == 'DELETE'"];
    
    NSError *storageError;
    [offline upsertItems:@[@{ @"id": @"A", @"text":@"DELETE"}, @{@"id":@"B",@"text":@"KEEP"},@{ @"id":@"C", @"text":@"DELETE"}]
                   table:TodoTableNoVersion
                 orError:&storageError];
    XCTAssertNil(storageError);
    
    NSOperation *purge = [todoTable purgeWithQuery:query completion:^(NSError *error) {
        XCTAssertNil(error, @"Unexpected error: %@", error.description);
        
        XCTAssertEqual(offline.deleteCalls, 1);
        XCTAssertEqual(offline.deletedItems, 2);
        
        NSError *storageError;
        MSSyncContextReadResult *result = [offline readWithQuery:query orError:&storageError];
        XCTAssertNil(storageError);
        XCTAssertTrue(result.items.count == 0, @"Items should have been deleted");
        
        NSDictionary *item = [offline readTable:TodoTableNoVersion withItemId:@"B" orError:&storageError];
        XCTAssertNil(storageError);
        XCTAssertNotNil(item, @"Item B should not have been deleted");
        
        done = YES;
    }];
	
	XCTAssertNotNil(purge);
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testPurgePendingOpsOnDifferentTableSuccess
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    testFilter.ignoreNextFilter = YES;
    testFilter.errorToUse = [NSError errorWithDomain:MSErrorDomain code:-1 userInfo:nil];
    
    NSError *storageError;
    [offline upsertItems:@[@{ @"id": @"A"}]
                   table:TodoTableNoVersion
                 orError:&storageError];
    XCTAssertNil(storageError);
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *testTable = [filteredClient syncTableWithName:@"TodoItem"];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    
    [testTable insert:@{ @"name": @"test one"} completion:^(NSDictionary *item, NSError *error) {
        NSOperation *purge = [todoTable purgeWithQuery:nil completion:^(NSError *error) {
            XCTAssertNil(error, @"Unexpected error: %@", error.description);
            
            XCTAssertEqual(offline.deleteCalls, 1);
            XCTAssertEqual(offline.deletedItems, 1);
            
            // Verify item is missing as well
            NSError *storageError;
            NSDictionary *item = [offline readTable:TodoTableNoVersion withItemId:@"A" orError:&storageError];
            XCTAssertNil(storageError);
            XCTAssertNil(item, @"Unexpected item found: %@", item.description);
            
            done = YES;
        }];
		XCTAssertNotNil(purge);
    }];
    
    
    XCTAssertTrue([self waitForTest:30.0], @"Test timed out.");
}

-(void) testForcePurgeWithLockedOperation
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    testFilter.ignoreNextFilter = YES;
    testFilter.errorToUse = [NSError errorWithDomain:MSErrorDomain code:-1 userInfo:nil];
    
    __block NSError *storageError;
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    [todoTable insert:@{ @"id": @"B"} completion:^(NSDictionary *item, NSError *error) {
        MSSyncContextReadResult *result = [offline readWithQuery:query orError:&storageError];
        XCTAssertNil(storageError);
        XCTAssertEqual(result.items.count, 1);
        done = YES;
    }];
    
    // at this point we have 1 deltaToken, 2 todo items, 1 operation, and 1 operation error
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    done = NO;
   
    // get the operation and lock it
    NSArray *operations = [filteredClient.syncContext.operationQueue getOperationsForTable:todoTable.name item:nil];
    [filteredClient.syncContext.operationQueue lockOperation:operations[0]];
    
    [offline resetCounts];
    [todoTable forcePurgeWithCompletion:^(NSError *error) {
        XCTAssertNotNil(error);
        XCTAssertEqual(error.code, MSPurgeAbortedPendingChanges);
        XCTAssertEqual(offline.deleteCalls, 0);
        XCTAssertEqual(offline.deletedItems, 0);
        XCTAssertEqual(filteredClient.syncContext.pendingOperationsCount, 1);
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}
-(void) testForcePurgeSuccess
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    testFilter.ignoreNextFilter = YES;
    testFilter.errorToUse = [NSError errorWithDomain:MSErrorDomain code:-1 userInfo:nil];
    
    __block NSError *storageError;
    MSTableConfigValue *deltaToken = [MSTableConfigValue new];
    deltaToken.table = TodoTableNoVersion;
    deltaToken.key = @"test_id";
    deltaToken.keyType = MSConfigKeyDeltaToken;
    deltaToken.value = @"SOME RANDOM VALUE";
    NSString *tokenId = deltaToken.id;
    XCTAssertNil(storageError);
    __block int operationId = 1;
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    [offline upsertItems:@[deltaToken.serialize] table:offline.configTableName orError:&storageError];
    [offline upsertItems:@[@{ @"id": @"A"}] table:TodoTableNoVersion orError:&storageError];
    [todoTable insert:@{ @"id": @"B"} completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(storageError);
        [todoTable insert:@{ @"id": @"C"} completion:^(NSDictionary *item, NSError *error) {
            MSSyncContextReadResult *result = [offline readWithQuery:query orError:&storageError];
            XCTAssertNil(storageError);
            XCTAssertEqual(result.items.count, 3);
            [offline upsertItems:@[@{ @"id": @"A", @"operationId":[NSNumber numberWithInt:operationId++]}] table:offline.errorTableName orError:&storageError];
            done = YES;
        }];
    }];
    
    // at this point we have 1 deltaToken, 3 todo items, 2 operations, and 1 operation error
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    done = NO;
    [offline resetCounts];
    [todoTable forcePurgeWithCompletion:^(NSError *error) {
        NSError *storageError;
        XCTAssertNil(error, @"Unexpected error: %@", error.description);
        
        XCTAssertEqual(offline.deleteCalls, 6);
        XCTAssertEqual(offline.deletedItems, 7);
        XCTAssertEqual(filteredClient.syncContext.pendingOperationsCount, 0);
        
        // Verify item is missing as well
        MSSyncContextReadResult *emptyResult = [offline readWithQuery:query orError:&storageError];
        XCTAssertNil(storageError);
        XCTAssertEqual(emptyResult.items.count, 0);
        
        NSDictionary *token = [offline readTable:offline.configTableName withItemId:tokenId orError:&storageError];
        XCTAssertNil(token);
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testPurgeWithPendingOperationsFails
{
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:409];
    testFilter.errorToUse = [NSError errorWithDomain:MSErrorDomain code:-1 userInfo:nil];
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    [todoTable insert:@{ @"name": @"test one"} completion:^(NSDictionary *item, NSError *error) {
        NSOperation *purge = [todoTable purgeWithQuery:query completion:^(NSError *error) {
            XCTAssertNotNil(error);
            XCTAssertEqual(error.code, MSPurgeAbortedPendingChanges);
            XCTAssertEqual(offline.deleteCalls, 0);
            XCTAssertEqual(client.syncContext.pendingOperationsCount, 1);
            
            done = YES;
        }];
		XCTAssertNotNil(purge);
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void)testOperationErrorRelationship
{
    NSString *opTableName = @"MS_TableOperations";
    NSString *opErrorTableName = @"MS_TableOperationErrors";
    NSString *todoTableName = @"TodoItem";
    NSError *error = [NSError errorWithDomain:@"TestDomain" code:123 userInfo:nil];
    
    MSSyncTable *opTable = [[MSSyncTable alloc] initWithName:opTableName client:client];
    MSQuery *opQuery = [[MSQuery alloc] initWithSyncTable:opTable];
    MSSyncTable *opErrorTable = [[MSSyncTable alloc] initWithName:opErrorTableName client:client];
    MSQuery *opErrorQuery = [[MSQuery alloc] initWithSyncTable:opErrorTable];
    MSSyncTable *todoTable = [[MSSyncTable alloc] initWithName:todoTableName client:client];
    MSQuery *todoQuery = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    MSTableOperation *op1 = [[MSTableOperation alloc] initWithTable:todoTableName type:MSTableOperationInsert itemId:@"one"];
    op1.operationId = 1;
    MSTableOperationError *opError1 = [[MSTableOperationError alloc] initWithOperation:op1 item:op1.item error:error];
    MSTableOperation *op2 = [[MSTableOperation alloc] initWithTable:todoTableName type:MSTableOperationInsert itemId:@"two"];
    op2.operationId = 2;
    MSTableOperationError *opError2 = [[MSTableOperationError alloc] initWithOperation:op2 item:op2.item error:error];
    MSTableOperationError *opError3 = [[MSTableOperationError alloc] initWithOperation:op1 item:op1.item error:error];
    NSDictionary *item1 = @{@"id":@"one"};
    NSDictionary *item2 = @{@"id":@"two"};
    
    NSError *localError;
    [client.syncContext.dataSource upsertItems:@[item1, item2] table:todoTableName orError:&localError];
    XCTAssertNil(localError, @"Error from upserting Todo item");
    [client.syncContext.dataSource upsertItems:@[op1.serialize, op2.serialize] table:opTableName orError:&localError];
    XCTAssertNil(localError, @"Error from upserting ops");
    [client.syncContext.dataSource upsertItems:@[opError1.serialize, opError2.serialize, opError3.serialize] table:opErrorTableName orError:&localError];
    XCTAssertNil(localError, @"Error from upserting opErrors");
    
    // make sure everything is there
    NSArray *todoItems = [client.syncContext.dataSource readWithQuery:todoQuery orError:&localError].items;
    XCTAssertNil(localError);
    NSArray *opItems = [client.syncContext.dataSource readWithQuery:opQuery orError:&error].items;
    XCTAssertNil(localError);
    NSArray *opErrorItems = [client.syncContext.dataSource readWithQuery:opErrorQuery orError:&error].items;
    XCTAssertNil(localError, @"Error from ");
    
    XCTAssertEqual(todoItems.count, 2);
    XCTAssertEqual(opItems.count, 2);
    XCTAssertEqual(opErrorItems.count, 3);
    
    [client.syncContext cancelOperation:op1 discardItemWithCompletion:^(NSError *error) {
        XCTAssertNil(error);
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    todoItems = [client.syncContext.dataSource readWithQuery:todoQuery orError:&localError].items;
    XCTAssertNil(localError);
    opItems = [client.syncContext.dataSource readWithQuery:opQuery orError:&error].items;
    XCTAssertNil(localError);
    opErrorItems = [client.syncContext.dataSource readWithQuery:opErrorQuery orError:&error].items;
    XCTAssertNil(localError, @"Error from ");
    
    XCTAssertEqual(todoItems.count, 1);
    XCTAssertEqualObjects([todoItems[0] valueForKey:@"id"], @"two");
    XCTAssertEqual(opItems.count, 1);
    XCTAssertEqualObjects([opItems[0] valueForKey:@"id"], @2);
    XCTAssertEqual(opErrorItems.count, 1);
    XCTAssertEqualObjects([opErrorItems[0] valueForKey:@"operationId"], @2);
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

-(BOOL) checkURL:(NSURL *)url withPath:(NSString *)path andQuery:(NSArray *)query
{
    if (![url.path isEqualToString:path]) {
        return NO;
    }
    
    NSArray *actualQuery = [[url.query componentsSeparatedByString:@"&"]
                                  sortedArrayUsingSelector:@selector(localizedCaseInsensitiveCompare:)];
    
    NSArray *sortedQuery = [query sortedArrayUsingSelector:@selector(localizedCaseInsensitiveCompare:)];
    
    return [actualQuery isEqualToArray:sortedQuery];
}

#pragma mark * deltaToken Test Helper Method

-(void) initDeltaTokenWithContext:(MSSyncContext *)syncContext query:(MSQuery*)query queryId:(NSString *)queryId date:(NSDate *)date {
    NSDateFormatter *formatter = [MSNaiveISODateFormatter naiveISODateFormatter];
    NSString *key = [NSString stringWithFormat:@"deltaToken|%@|%@", query.table.name, queryId];
    NSDictionary *delta = @{@"id":key, @"value":[formatter stringFromDate:date]};
    
    [syncContext.dataSource upsertItems:@[delta] table:syncContext.dataSource.configTableName orError:nil];
}

@end
