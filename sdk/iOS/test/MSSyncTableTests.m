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

static NSString *const TodoTableNoVersion = @"TodoNoVersion";
static NSString *const AllColumnTypesTable = @"ColumnTypes";

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
    MSSyncTable *todoTable = [client syncTableWithName:TodoTableNoVersion];
    
    // Create the item
    id item = @{ @"name":@"test name" };
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNotNil(item[@"id"], @"The item should have an id");
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInsertItemWithInvalidId
{
    // TODO: Fix error message when non string ids are used
    
    MSSyncTable *todoTable = [client syncTableWithName:TodoTableNoVersion];
    
    // Create the item
    id item = @{ @"id": @12345, @"name":@"test name" };
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNotNil(error, @"error should have been set.");
        XCTAssertTrue(error.localizedDescription, @"The item provided must not have an id.");
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInsertItemWithInvalidItem
{
    MSSyncTable *todoTable = [client syncTableWithName:TodoTableNoVersion];
    
    // Create the item
    id item = @[ @"I_am_a", @"Array", @"I should be an object" ];
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNotNil(error, @"error should have been set.");
        XCTAssertTrue(error.localizedDescription, @"The item provided was not valid.");
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

- (void) testInsertItemWithoutDatasource
{
    MSSyncTable *todoTable = [client syncTableWithName:TodoTableNoVersion];
    client.syncContext.dataSource = nil;
    
    // Create the item
    id item = @{ @"id": @"ok", @"data": @"lots of stuff here" };
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNotNil(error, @"error should have been set.");
        //STAssertTrue(error.localizedDescription, @"The item provided was not valid.");
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInsertItemWithValidId
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    NSString* stringData = @"{\"id\": \"test1\", \"text\":\"test name\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    BOOL __block insertRanToServer = NO;
    
    testFilter.responseToUse = response;
    testFilter.dataToUse = data;
    testFilter.ignoreNextFilter = YES;
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
    done = NO;
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    done = NO;
    [client.syncContext pushWithCompletion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue(insertRanToServer, @"the insert call didn't go to the server");
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:2000.1], @"Test timed out.");
}

-(void) testInsertWithAllColumnTypes {
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:200
                                                                 data:@"{\"id\": \"test1\", \"text\":\"test name\"}"];
    BOOL __block insertRanToServer = NO;
    
    testFilter.ignoreNextFilter = YES;
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
                            @"data": [NSDate dateWithTimeIntervalSinceNow:0],
                        };
    
    // Insert the item
    done = NO;
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    done = NO;
    [client.syncContext pushWithCompletion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue(insertRanToServer, @"the insert call didn't go to the server");
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:2000.1], @"Test timed out.");
}

-(void) testInsertWithBinaryFail {
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:500];
    
    BOOL __block insertRanToServer = NO;
    
    testFilter.ignoreNextFilter = YES;
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
    done = NO;
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    done = NO;
    [client.syncContext pushWithCompletion:^(NSError *error) {
        XCTAssertFalse(insertRanToServer);
        XCTAssertNotNil(error);
        XCTAssertEqual(error.code, MSPushCompleteWithErrors);
        
        NSArray *pushErrors = [error.userInfo objectForKey:MSErrorPushResultKey];
        XCTAssertNotNil(pushErrors);
        XCTAssertEqual(pushErrors.count, 1);
        
        MSTableOperationError *tableError = [pushErrors objectAtIndex:0];
        XCTAssertEqual(tableError.code, MSInvalidItemWithRequest);
        
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:2000.1], @"Test timed out.");
}

-(void) testInsertPushInsertPush
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    NSString* stringData = @"{\"id\": \"test1\", \"text\":\"test name\"}";
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
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    
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
    [client.syncContext pushWithCompletion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue(insertRanToServer, @"the insert call didn't go to the server");
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:1110.1], @"Test timed out.");

    // Create the item
    item = @{ @"id": @"test2", @"name":@"test name" };
    
    // Insert the item
    done = NO;
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    insertRanToServer = NO;    
    done = NO;
    [client.syncContext pushWithCompletion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue(insertRanToServer, @"the insert call didn't go to the server");
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:2000.1], @"Test timed out.");

}

-(void) testInsertItemWithValidIdConflict
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:412
                                   HTTPVersion:nil headerFields:nil];
    NSString* stringData = @"{\"id\": \"test1\", \"text\":\"test name\", \"__version\":\"1\" }";
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
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    
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
    [client.syncContext pushWithCompletion:^(NSError *error) {
        // Verify the call went to the server
        XCTAssertTrue(insertRanToServer, @"the insert call didn't go to the server");

        // Verify we got the expected error results
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertEqual(error.code, [@MSPushCompleteWithErrors integerValue], @"Unexpected error code");
        NSArray *errors = error.userInfo[MSErrorPushResultKey];
        XCTAssertNotNil(errors, @"error should not have been nil.");
        
        // Verify we have a precondition failed error
        MSTableOperationError *errorInfo = errors[0];
        
        XCTAssertEqual(errorInfo.statusCode, [@412 integerValue], @"Unexpected status code");
        XCTAssertEqual(errorInfo.code, [@MSErrorPreconditionFailed integerValue], @"Unexpected status code");
        
        NSDictionary *actualItem = errorInfo.serverItem;
        XCTAssertNotNil(actualItem, @"Expected server version to be present");
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:330.1], @"Test timed out.");
}

-(void) testInsertUpdateCollapseSuccess
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    NSString* stringData = @"{\"id\": \"test1\", \"text\":\"updated name\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    __block NSInteger callsToServer = 0;
    
    testFilter.responseToUse = response;
    testFilter.dataToUse = data;
    testFilter.ignoreNextFilter = YES;
    testFilter.onInspectRequest =  ^(NSURLRequest *request) {
        callsToServer++;
        XCTAssertEqualObjects(request.HTTPMethod, @"POST", @"Unexpected method: %@", request.HTTPMethod);
        NSString *bodyString = [[NSString alloc] initWithData:request.HTTPBody
                                                     encoding:NSUTF8StringEncoding];
        XCTAssertEqualObjects(bodyString, @"{\"id\":\"test1\",\"text\":\"updated name\"}");
        
        return request;
    };
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    
    // Create the item
    NSDictionary *item = @{ @"id": @"test1", @"text": @"test name" };
    
    // Insert the item
    done = NO;
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    // Update the item
    done = NO;
    item = @{ @"id": @"test1", @"text": @"updated name" };
    [todoTable update:item completion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    // Push queue to server
    done = NO;
    [client.syncContext pushWithCompletion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue(callsToServer == 1, @"only one call to server should have been made");
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInsertDeleteCollapseSuccess
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    NSString* stringData = @"{\"id\": \"test1\", \"text\":\"updated name\"}";
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
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    
    // Create the item
    NSDictionary *item = @{ @"id": @"test1", @"name": @"test name" };
    
    // Insert the item
    done = NO;
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    // Update the item
    done = NO;
    [todoTable delete:item completion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    // Push queue to server
    done = NO;
    [client.syncContext pushWithCompletion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue(callsToServer == 0, @"no calls to server should have been made");
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInsertInsertCollapseThrows
{
    MSSyncTable *todoTable = [client syncTableWithName:TodoTableNoVersion];
    
    NSDictionary *item = @{ @"name": @"test" };
    [todoTable insert:item completion:^(NSDictionary *itemOne, NSError *error) {
       [todoTable insert:itemOne completion:^(NSDictionary *itemTwo, NSError *error) {
           XCTAssertNotNil(error, @"expected an error");
           XCTAssertTrue(error.code == MSSyncTableInvalidAction, @"unexpected error code");
           done = YES;
       }];
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
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
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    done = NO;
    [client.syncContext pushWithCompletion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue(updateSentToServer, @"the update call didn't go to the server");
        done = YES;
    }];
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
    [client.syncContext pushWithCompletion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue(callsToServer == 1, @"expected only 1 call to the server");
        done = YES;
    }];
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
    [client.syncContext pushWithCompletion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue(callsToServer == 1, @"expected only 1 call to the server");
        done = YES;
    }];
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
    [client.syncContext pushWithCompletion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue(deleteSentToServer, @"the delete call didn't go to the server");
        done = YES;
    }];
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
    [client.syncContext pushWithCompletion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertTrue(deleteSentToServer, @"the delete call didn't go to the server");
        done = YES;
    }];
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
    
    [todoTable pullWithQuery:query queryId:nil completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        XCTAssertEqual(offline.upsertCalls, 1);
        XCTAssertEqual(offline.upsertedItems, 2);
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:30.0], @"Test timed out.");
    
    NSURLRequest *firstRequest = (NSURLRequest *)filter.actualRequests[0];
    NSURLRequest *secondRequest = (NSURLRequest *)filter.actualRequests[1];

    XCTAssertEqualObjects(firstRequest.URL.absoluteString, @"https://someUrl/tables/TodoItem?__includeDeleted=1&__systemProperties=__deleted,__version");
    XCTAssertEqualObjects(secondRequest.URL.absoluteString, @"https://someUrl/tables/TodoItem?$skip=2&__includeDeleted=1&__systemProperties=__deleted,__version");
}

-(void) testPullWithSystemPropeties
{
    NSString* stringData = @"[{\"id\": \"one\", \"text\":\"first item\"},{\"id\": \"two\", \"text\":\"second item\"}]";
    MSMultiRequestTestFilter *filter = [MSMultiRequestTestFilter testFilterWithStatusCodes:@[@200] data:@[stringData] appendEmptyRequest:YES];
    
    offline.upsertCalls = 0;
    
    MSClient *filteredClient = [client clientWithFilter:filter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    [todoTable pullWithQuery:query queryId:nil completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        XCTAssertEqual((int)offline.upsertCalls, 1, @"Unexpected number of upsert calls");
        XCTAssertEqual((int)offline.upsertedItems, 2, @"Unexpected number of upsert calls");
        done = YES;
    }];

    XCTAssertTrue([self waitForTest:30.0], @"Test timed out.");
    
    NSURLRequest *firstRequest = (NSURLRequest *)filter.actualRequests[0];
    NSURLRequest *secondRequest = (NSURLRequest *)filter.actualRequests[1];
    
    XCTAssertEqualObjects(firstRequest.URL.absoluteString, @"https://someUrl/tables/TodoNoVersion?__includeDeleted=1&__systemProperties=__deleted");
    XCTAssertEqualObjects(secondRequest.URL.absoluteString, @"https://someUrl/tables/TodoNoVersion?$skip=2&__includeDeleted=1&__systemProperties=__deleted");
}

-(void) testPullSuccessWithDeleted
{
    NSString* stringData = @"[{\"id\": \"one\", \"text\":\"first item\", \"__deleted\":false},{\"id\": \"two\", \"text\":\"second item\", \"__deleted\":true}, {\"id\": \"three\", \"text\":\"third item\", \"__deleted\":null}]";
    MSMultiRequestTestFilter *testFilter = [MSMultiRequestTestFilter testFilterWithStatusCodes:@[@200] data:@[stringData] appendEmptyRequest:@YES];
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    [todoTable pullWithQuery:query queryId:nil completion:^(NSError *error) {
        XCTAssertNil(error, @"Error occurred: %@", error.description);
        XCTAssertEqual((int)offline.upsertCalls, 1, @"Unexpected number of upsert calls");
        XCTAssertEqual((int)offline.upsertedItems, 2, @"Unexpected number of upsert calls");
        XCTAssertEqual((int)offline.deleteCalls, 1, @"Unexpected number of delete calls");
        XCTAssertEqual((int)offline.deletedItems, 1, @"Unexpected number of upsert calls");
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:30.0], @"Test timed out.");
    
    NSURLRequest *firstRequest = testFilter.actualRequests[0];
    NSURLRequest *secondRequest = testFilter.actualRequests[1];
    XCTAssertEqualObjects(firstRequest.URL.absoluteString, @"https://someUrl/tables/TodoNoVersion?__includeDeleted=1&__systemProperties=__deleted");
    XCTAssertEqualObjects(secondRequest.URL.absoluteString, @"https://someUrl/tables/TodoNoVersion?$skip=3&__includeDeleted=1&__systemProperties=__deleted");
}

-(void) testPullWithPushSuccess
{
    NSString *insertResponse = @"{\"id\": \"one\", \"text\":\"first item\"}";
    NSString *pullResponse = @"[{\"id\": \"one\", \"text\":\"first item\"},{\"id\": \"two\", \"text\":\"second item\"}]";
    MSMultiRequestTestFilter *testFilter = [MSMultiRequestTestFilter testFilterWithStatusCodes:@[@200,@200] data:@[insertResponse,pullResponse] appendEmptyRequest:@YES];

    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    [todoTable insert:@{ @"id":@"test1", @"name": @"test one"} completion:^(NSDictionary *item, NSError *error) {
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:60.0], @"Test timed out.");
    
    done = NO;
    
    [todoTable pullWithQuery:query queryId:nil completion:^(NSError *error) {
        XCTAssertNil(error, @"Unexpected error: %@", error.description);
        XCTAssertEqual(offline.upsertCalls, 4);
        XCTAssertEqual(offline.upsertedItems, 5);
        
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:300.0], @"Test timed out.");
    
    NSURLRequest *insertRequest = testFilter.actualRequests[0];
    NSURLRequest *firstPullRequest = testFilter.actualRequests[1];
    NSURLRequest *secondPullRequest = testFilter.actualRequests[2];
    XCTAssertEqualObjects(insertRequest.URL.absoluteString, @"https://someUrl/tables/TodoNoVersion?__systemProperties=__version");
    XCTAssertEqualObjects(firstPullRequest.URL.absoluteString, @"https://someUrl/tables/TodoNoVersion?__includeDeleted=1&__systemProperties=__deleted");
    XCTAssertEqualObjects(secondPullRequest.URL.absoluteString, @"https://someUrl/tables/TodoNoVersion?$skip=2&__includeDeleted=1&__systemProperties=__deleted");
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

    [todoTable pullWithQuery:query queryId:nil completion:^(NSError *error) {
        XCTAssertNil(error, @"Unexpected error: %@", error.description);
        XCTAssertNotNil(actualRequest);

        NSString *featuresHeader = [actualRequest.allHTTPHeaderFields valueForKey:MSFeaturesHeaderName];
        XCTAssertNotNil(featuresHeader, @"actualHeader should not have been nil.");
        NSString *expectedFeatures = @"TQ,OL";
        XCTAssertTrue([featuresHeader isEqualToString:expectedFeatures], @"Header value (%@) was not as expected (%@)", featuresHeader, expectedFeatures);

        done = YES;
    }];

    XCTAssertTrue([self waitForTest:30.0], @"Test timed out.");
}

-(void) testPullWithCustomParameters
{
    NSString* stringData = @"[{\"id\": \"one\", \"text\":\"first item\"},{\"id\": \"two\", \"text\":\"second item\"}]";
    MSMultiRequestTestFilter *testFilter = [MSMultiRequestTestFilter testFilterWithStatusCodes:@[@200] data:@[stringData] appendEmptyRequest:@YES];

    offline.upsertCalls = 0;
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    query.parameters = @{@"mykey": @"myvalue"};
    
    [todoTable pullWithQuery:query queryId:nil completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        XCTAssertEqual((int)offline.upsertCalls, 1, @"Unexpected number of upsert calls");
        XCTAssertEqual((int)offline.upsertedItems, 2, @"Unexpected number of upsert calls");
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:30.0], @"Test timed out.");
    
    NSURLRequest *firstRequest = testFilter.actualRequests[0];
    NSURLRequest *secondRequest = testFilter.actualRequests[1];
    XCTAssertEqualObjects(firstRequest.URL.absoluteString, @"https://someUrl/tables/TodoNoVersion?mykey=myvalue&__includeDeleted=1&__systemProperties=__deleted");
    XCTAssertEqualObjects(secondRequest.URL.absoluteString, @"https://someUrl/tables/TodoNoVersion?$skip=2&mykey=myvalue&__includeDeleted=1&__systemProperties=__deleted");
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
    query.parameters = @{@"__systemProperties": @"__createdAt,__somethingRandom"};
    
    [todoTable pullWithQuery:query queryId:nil completion:^(NSError *error) {
        XCTAssertNotNil(error);
        XCTAssertEqual(error.code, MSInvalidParameter);
        XCTAssertEqual((int)offline.upsertCalls, 0, @"Unexpected number of upsert calls");
        XCTAssertEqual((int)offline.upsertedItems, 0, @"Unexpected number of upsert calls");
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:30.0], @"Test timed out.");
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
    [client.syncContext pushWithCompletion:^(NSError *error) {
        XCTAssertNil(error, @"error should have been nil.");
        XCTAssertNotNil(actualRequest, @"actualRequest should not have been nil.");

        NSString *featuresHeader = [actualRequest.allHTTPHeaderFields valueForKey:MSFeaturesHeaderName];
        XCTAssertNotNil(featuresHeader, @"actualHeader should not have been nil.");
        NSString *expectedFeatures = @"OL";
        XCTAssertTrue([featuresHeader isEqualToString:expectedFeatures], @"Header value (%@) was not as expected (%@)", featuresHeader, expectedFeatures);

        done = YES;
    }];
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
    // verifies that we allow __includeDeleted=YES
    query.parameters = @{@"__includeDeleted":@YES};
    
    [todoTable pullWithQuery:query queryId:@"test_1" completion:^(NSError *error) {
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
    
    XCTAssertTrue([self waitForTest:1000.1], @"Test timed out.");
    done = NO;
    XCTAssertEqual(3, filter.actualRequests.count);

    NSURLRequest *firstRequest = (NSURLRequest *)filter.actualRequests[0];
    NSURLRequest *secondRequest = (NSURLRequest *)filter.actualRequests[1];
    NSURLRequest *thirdRequest = (NSURLRequest *)filter.actualRequests[2];
    
    XCTAssertEqualObjects(firstRequest.URL.absoluteString, @"https://someUrl/tables/TodoItem?$filter=(__updatedAt%20ge%20datetimeoffset'1970-01-01T00%3A00%3A00.000Z')&__includeDeleted=1&$orderby=__updatedAt%20asc&__systemProperties=__updatedAt,__deleted,__version");
    XCTAssertEqualObjects(secondRequest.URL.absoluteString, @"https://someUrl/tables/TodoItem?$filter=(__updatedAt%20ge%20datetimeoffset'1999-12-03T15%3A44%3A29.000Z')&__includeDeleted=1&$orderby=__updatedAt%20asc&__systemProperties=__updatedAt,__deleted,__version");
    XCTAssertEqualObjects(thirdRequest.URL.absoluteString, @"https://someUrl/tables/TodoItem?$filter=(__updatedAt%20ge%20datetimeoffset'1999-12-04T16%3A44%3A59.000Z')&__includeDeleted=1&$orderby=__updatedAt%20asc&__systemProperties=__updatedAt,__deleted,__version");
    
    // now try again and make sure we start with the same deltaToken
    [offline resetCounts];
    [todoTable pullWithQuery:query queryId:@"test_1" completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        XCTAssertEqual(offline.upsertCalls, 0);
        XCTAssertEqual(offline.upsertedItems, 0);
        // readWithQuery is only called once to get the initial deltaToken. otherwise, it's cached.
        XCTAssertEqual(offline.readWithQueryCalls, 2);
        XCTAssertEqual(offline.readWithQueryItems, 1);
        XCTAssertEqual(offline.readTableCalls, 0);
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:10000.1], @"Test timed out.");
    XCTAssertEqual(4, filter.actualRequests.count);
    
    NSURLRequest *fourthRequest = (NSURLRequest *)filter.actualRequests[3];
    
    XCTAssertEqualObjects(fourthRequest.URL.absoluteString, @"https://someUrl/tables/TodoItem?$filter=(__updatedAt%20ge%20datetimeoffset'1999-12-04T16%3A44%3A59.000Z')&__includeDeleted=1&$orderby=__updatedAt%20asc&__systemProperties=__updatedAt,__deleted,__version");
}

-(void) testIncrementalPullWithSkipFails
{
    MSMultiRequestTestFilter *filter = [MSMultiRequestTestFilter testFilterWithStatusCodes:@[] data:@[] appendEmptyRequest:YES];
    
    MSClient *filteredClient = [client clientWithFilter:filter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:@"TodoItem"];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    query.fetchOffset = 10;
    
    // without queryId, it should work
    [todoTable pullWithQuery:query queryId:nil completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        XCTAssertEqual(offline.upsertCalls, 0);
        XCTAssertEqual(offline.upsertedItems, 0);
        XCTAssertEqual(offline.readTableCalls, 0);
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    done = NO;
    XCTAssertEqual(1, filter.actualRequests.count);
    
    NSURLRequest *firstRequest = (NSURLRequest *)filter.actualRequests[0];
    
    XCTAssertEqualObjects(firstRequest.URL.absoluteString, @"https://someUrl/tables/TodoItem?$skip=10&__includeDeleted=1&__systemProperties=__deleted,__version");
    
    [offline resetCounts];
    // with queryId, this should produce an error
    [todoTable pullWithQuery:query queryId:@"test_1" completion:^(NSError *error) {
        XCTAssertNotNil(error);
        XCTAssertEqual(MSInvalidParameter, error.code);
        XCTAssertEqual(offline.upsertCalls, 0);
        XCTAssertEqual(offline.upsertedItems, 0);
        // readTable is only called once to get the initial deltaToken. otherwise, it's cached.
        XCTAssertEqual(offline.readTableCalls, 0);
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    XCTAssertEqual(1, filter.actualRequests.count);
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
    [todoTable pullWithQuery:query queryId:nil completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        XCTAssertEqual(offline.upsertCalls, 0);
        XCTAssertEqual(offline.upsertedItems, 0);
        XCTAssertEqual(offline.readTableCalls, 0);
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    done = NO;
    XCTAssertEqual(1, filter.actualRequests.count);
    
    NSURLRequest *firstRequest = (NSURLRequest *)filter.actualRequests[0];
    
    XCTAssertEqualObjects(firstRequest.URL.absoluteString, @"https://someUrl/tables/TodoItem?__includeDeleted=1&$orderby=id%20desc&__systemProperties=__deleted,__version");
    
    [offline resetCounts];
    // with queryId, this should produce an error
    [todoTable pullWithQuery:query queryId:@"test_1" completion:^(NSError *error) {
        XCTAssertNotNil(error);
        XCTAssertEqual(MSInvalidParameter, error.code);
        XCTAssertEqual(offline.upsertCalls, 0);
        XCTAssertEqual(offline.upsertedItems, 0);
        XCTAssertEqual(offline.readWithQueryCalls, 0);
        XCTAssertEqual(offline.readTableCalls, 0);
        done = YES;
    }];
    
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
    
    [todoTable pullWithQuery:query queryId:nil completion:completion];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    done = NO;
    
    query.parameters = @{@"__includeDeleted":@YES, @"__INCLUDEDELETED":@NO};
    [todoTable pullWithQuery:query queryId:nil completion:completion];
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    done = NO;
    
    query.parameters = @{@"__includeDeleted":@"NONSENSE"};
    [todoTable pullWithQuery:query queryId:nil completion:completion];
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
    // we'll include a top here to make sure it works
    query.fetchLimit = 50;
    
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

    [todoTable pullWithQuery:query queryId:@"test-1" completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        XCTAssertEqual(offline.upsertCalls, 6);
        XCTAssertEqual(offline.upsertedItems, 8);
        NSLog(@"done pull");
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:30.0], @"Test timed out.");
    
    // since we hijacked the first request, the 0th one is actually the second
    NSURLRequest *secondPullRequest = (NSURLRequest *)filter.actualRequests[0];
    NSURLRequest *thirdPullRequest = (NSURLRequest *)filter.actualRequests[1];
    
    XCTAssertEqualObjects(firstPullRequest.URL.absoluteString, @"https://someUrl/tables/TodoItem?$filter=(__updatedAt%20ge%20datetimeoffset'1970-01-01T00%3A00%3A00.000Z')&$orderby=__updatedAt%20asc&__includeDeleted=1&$top=50&__systemProperties=__updatedAt,__deleted,__version");
    XCTAssertEqualObjects(secondPullRequest.URL.absoluteString, @"https://someUrl/tables/TodoItem?$filter=(__updatedAt%20ge%20datetimeoffset'1999-12-03T15%3A44%3A29.000Z')&$orderby=__updatedAt%20asc&__includeDeleted=1&$top=50&__systemProperties=__updatedAt,__deleted,__version");
    XCTAssertEqualObjects(thirdPullRequest.URL.absoluteString, @"https://someUrl/tables/TodoItem?$filter=(__updatedAt%20ge%20datetimeoffset'1999-12-07T15%3A44%3A28.000Z')&$orderby=__updatedAt%20asc&__includeDeleted=1&$top=50&__systemProperties=__updatedAt,__deleted,__version");

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
    
    [todoTable pullWithQuery:query queryId:@"test-1" completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        XCTAssertEqual(offline.upsertCalls, 9);
        XCTAssertEqual(offline.upsertedItems, 10);
        XCTAssertEqual(offline.deleteCalls, 5);
        // the pending op gets deleted on push
        XCTAssertEqual(offline.deletedItems, 1);
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:30.0], @"Test timed out.");
    
    // since we hijacked the second request and the first request is an insert, the item at [1] is actually the second pull
    NSURLRequest *insertRequest = (NSURLRequest *)filter.actualRequests[0];
    NSURLRequest *secondPullRequest = (NSURLRequest *)filter.actualRequests[1];
    NSURLRequest *thirdPullRequest = (NSURLRequest *)filter.actualRequests[2];
    
    XCTAssertEqualObjects(insertRequest.URL.absoluteString, @"https://someUrl/tables/TodoItem?__systemProperties=__version");
    XCTAssertEqualObjects(firstPullRequest.URL.absoluteString, @"https://someUrl/tables/TodoItem?$filter=(__updatedAt%20ge%20datetimeoffset'1970-01-01T00%3A00%3A00.000Z')&__includeDeleted=1&$orderby=__updatedAt%20asc&__systemProperties=__updatedAt,__deleted,__version");    XCTAssertEqualObjects(secondPullRequest.URL.absoluteString, @"https://someUrl/tables/TodoItem?$filter=(__updatedAt%20ge%20datetimeoffset'1999-12-03T15%3A44%3A29.000Z')&__includeDeleted=1&$orderby=__updatedAt%20asc&__systemProperties=__updatedAt,__deleted,__version");
    XCTAssertEqualObjects(thirdPullRequest.URL.absoluteString, @"https://someUrl/tables/TodoItem?$filter=(__updatedAt%20ge%20datetimeoffset'1999-12-07T15%3A44%3A28.000Z')&__includeDeleted=1&$orderby=__updatedAt%20asc&__systemProperties=__updatedAt,__deleted,__version");
    
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
    
    [todoTable pullWithQuery:query queryId:@"test_1" completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        XCTAssertEqual(offline.upsertCalls, 7);
        XCTAssertEqual(offline.upsertedItems, 11);
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    
    NSURLRequest *firstRequest = (NSURLRequest *)filter.actualRequests[0];
    NSURLRequest *secondRequest = (NSURLRequest *)filter.actualRequests[1];
    NSURLRequest *thirdRequest = (NSURLRequest *)filter.actualRequests[2];
    NSURLRequest *fourthRequest = (NSURLRequest *)filter.actualRequests[3];
    NSURLRequest *fifthRequest = (NSURLRequest *)filter.actualRequests[4];
    NSURLRequest *sixthRequest = (NSURLRequest *)filter.actualRequests[5];
    
    XCTAssertEqual(6, filter.actualRequests.count);
    
    XCTAssertEqualObjects(firstRequest.URL.absoluteString, @"https://someUrl/tables/TodoItem?$filter=(__updatedAt%20ge%20datetimeoffset'1970-01-01T00%3A00%3A00.000Z')&__includeDeleted=1&$orderby=__updatedAt%20asc&__systemProperties=__updatedAt,__deleted,__version");
    XCTAssertEqualObjects(secondRequest.URL.absoluteString, @"https://someUrl/tables/TodoItem?$filter=(__updatedAt%20ge%20datetimeoffset'2000-01-01T00%3A00%3A00.000Z')&__includeDeleted=1&$orderby=__updatedAt%20asc&__systemProperties=__updatedAt,__deleted,__version");
    // TODO: why does the ordering of $orderby and __includeDeleted change here?
    XCTAssertEqualObjects(thirdRequest.URL.absoluteString, @"https://someUrl/tables/TodoItem?$skip=2&$filter=(__updatedAt%20ge%20datetimeoffset'2000-01-01T00%3A00%3A00.000Z')&$orderby=__updatedAt%20asc&__includeDeleted=1&__systemProperties=__updatedAt,__deleted,__version");
    XCTAssertEqualObjects(fourthRequest.URL.absoluteString, @"https://someUrl/tables/TodoItem?$skip=4&$filter=(__updatedAt%20ge%20datetimeoffset'2000-01-01T00%3A00%3A00.000Z')&$orderby=__updatedAt%20asc&__includeDeleted=1&__systemProperties=__updatedAt,__deleted,__version");
    XCTAssertEqualObjects(fifthRequest.URL.absoluteString, @"https://someUrl/tables/TodoItem?$filter=(__updatedAt%20ge%20datetimeoffset'2000-01-02T00%3A00%3A00.000Z')&__includeDeleted=1&$orderby=__updatedAt%20asc&__systemProperties=__updatedAt,__deleted,__version");
    XCTAssertEqualObjects(sixthRequest.URL.absoluteString, @"https://someUrl/tables/TodoItem?$skip=1&$filter=(__updatedAt%20ge%20datetimeoffset'2000-01-02T00%3A00%3A00.000Z')&$orderby=__updatedAt%20asc&__includeDeleted=1&__systemProperties=__updatedAt,__deleted,__version");
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
    
    [todoTable pullWithQuery:query queryId:@"test_1" completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        XCTAssertEqual(offline.upsertCalls, 3);
        XCTAssertEqual(offline.upsertedItems, 4);
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1], @"Test timed out.");
    XCTAssertEqual(3, filter.actualRequests.count);
    
    NSURLRequest *firstRequest = (NSURLRequest *)filter.actualRequests[0];
    NSURLRequest *secondRequest = (NSURLRequest *)filter.actualRequests[1];
    NSURLRequest *thirdRequest = (NSURLRequest *)filter.actualRequests[2];
    
    XCTAssertEqualObjects(firstRequest.URL.absoluteString, @"https://someUrl/tables/TodoItem?$filter=((text%20eq%20'MATCH')%20and%20(__updatedAt%20ge%20datetimeoffset'1970-01-01T00%3A00%3A00.000Z'))&__includeDeleted=1&$orderby=__updatedAt%20asc&__systemProperties=__updatedAt,__deleted,__version");
    XCTAssertEqualObjects(secondRequest.URL.absoluteString, @"https://someUrl/tables/TodoItem?$filter=((text%20eq%20'MATCH')%20and%20(__updatedAt%20ge%20datetimeoffset'2000-01-01T00%3A00%3A00.000Z'))&__includeDeleted=1&$orderby=__updatedAt%20asc&__systemProperties=__updatedAt,__deleted,__version");
    XCTAssertEqualObjects(thirdRequest.URL.absoluteString, @"https://someUrl/tables/TodoItem?$skip=1&$filter=((text%20eq%20'MATCH')%20and%20(__updatedAt%20ge%20datetimeoffset'2000-01-01T00%3A00%3A00.000Z'))&$orderby=__updatedAt%20asc&__includeDeleted=1&__systemProperties=__updatedAt,__deleted,__version");
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
    [todoTable pullWithQuery:query queryId:@"something" completion:^(NSError *error) {
        XCTAssertNotNil(error);
        XCTAssertFalse(alreadyCalled);
        alreadyCalled = YES;
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:0.1]);
    done = NO;
    XCTAssertFalse([self waitForTest:0.1]);
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
    
    [todoTable purgeWithQuery:query completion:^(NSError *error) {
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

    XCTAssertTrue([self waitForTest:30.0], @"Test timed out.");
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
        [todoTable purgeWithQuery:nil completion:^(NSError *error) {
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
    }];
    
    
    XCTAssertTrue([self waitForTest:30.0], @"Test timed out.");
}

-(void) testPurgeWithPendingOperationsFails
{
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:409];
    testFilter.errorToUse = [NSError errorWithDomain:MSErrorDomain code:-1 userInfo:nil];
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    [todoTable insert:@{ @"name": @"test one"} completion:^(NSDictionary *item, NSError *error) {
        [todoTable purgeWithQuery:query completion:^(NSError *error) {
            XCTAssertNotNil(error);
            XCTAssertEqual(error.code, MSPurgeAbortedPendingChanges);
            XCTAssertEqual(offline.deleteCalls, 0);
            XCTAssertEqual(client.syncContext.pendingOperationsCount, 1);
            
            done = YES;
        }];
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

#pragma mark * deltaToken Test Helper Method

-(void) initDeltaTokenWithContext:(MSSyncContext *)syncContext query:(MSQuery*)query queryId:(NSString *)queryId date:(NSDate *)date {
    NSDateFormatter *formatter = [MSNaiveISODateFormatter naiveISODateFormatter];
    NSString *key = [NSString stringWithFormat:@"deltaToken|%@|%@", query.table.name, queryId];
    NSDictionary *delta = @{@"id":key, @"value":[formatter stringFromDate:date]};
    
    [syncContext.dataSource upsertItems:@[delta] table:syncContext.dataSource.configTableName orError:nil];
}

@end
