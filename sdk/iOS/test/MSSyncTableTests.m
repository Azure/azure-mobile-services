// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <XCTest/XCTest.h>
#import "MSSyncTable.h"
#import "MSTestFilter.h"
#import "MSQuery.h"
#import "MSTable+MSTableTestUtilities.h"
#import "MSOfflinePassthroughHelper.h"
#import "MSCoreDataStore+TestHelper.h"
#import "MSSDKFeatures.h"

static NSString *const TodoTableNoVersion = @"TodoNoVersion";

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
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    
    NSString* stringData = @"[{\"id\": \"one\", \"text\":\"first item\"},{\"id\": \"two\", \"text\":\"second item\"}]";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    testFilter.responseToUse = response;
    testFilter.dataToUse = data;
    testFilter.ignoreNextFilter = YES;
    offline.upsertCalls = 0;

    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:@"TodoItem"];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    [todoTable pullWithQuery:query completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        XCTAssertEqual(offline.upsertCalls, 1);
        XCTAssertEqual(offline.upsertedItems, 2);
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:30.0], @"Test timed out.");
}

-(void) testPullWithSystemPropeties
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    
    NSString* stringData = @"[{\"id\": \"one\", \"text\":\"first item\"},{\"id\": \"two\", \"text\":\"second item\"}]";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    testFilter.responseToUse = response;
    testFilter.dataToUse = data;
    testFilter.ignoreNextFilter = YES;
    offline.upsertCalls = 0;
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    [todoTable pullWithQuery:query completion:^(NSError *error) {
        XCTAssertNil(error, @"Error found: %@", error.description);
        XCTAssertEqual((int)offline.upsertCalls, 1, @"Unexpected number of upsert calls");
        XCTAssertEqual((int)offline.upsertedItems, 2, @"Unexpected number of upsert calls");
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:30.0], @"Test timed out.");
}

-(void) testPullSuccessWithDeleted
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    
    NSString* stringData = @"[{\"id\": \"one\", \"text\":\"first item\", \"__deleted\":false},{\"id\": \"two\", \"text\":\"second item\", \"__deleted\":true}, {\"id\": \"three\", \"text\":\"third item\", \"__deleted\":null}]";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    testFilter.responseToUse = response;
    testFilter.dataToUse = data;
    testFilter.ignoreNextFilter = YES;
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    [todoTable pullWithQuery:query completion:^(NSError *error) {
        XCTAssertNil(error, @"Error occurred: %@", error.description);
        XCTAssertEqual((int)offline.upsertCalls, 1, @"Unexpected number of upsert calls");
        XCTAssertEqual((int)offline.upsertedItems, 2, @"Unexpected number of upsert calls");
        XCTAssertEqual((int)offline.deleteCalls, 1, @"Unexpected number of delete calls");
        XCTAssertEqual((int)offline.deletedItems, 1, @"Unexpected number of upsert calls");
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:30.0], @"Test timed out.");
}

-(void) testPullWithPushSuccess
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];

    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    
    __block NSInteger nextResponse = 0;
    NSString *insertResponse = @"{\"id\": \"one\", \"text\":\"first item\"}";
    NSString *pushResponse = @"[{\"id\": \"one\", \"text\":\"first item\"},{\"id\": \"two\", \"text\":\"second item\"}]";
    
    __block NSArray *responses = @[[insertResponse dataUsingEncoding:NSUTF8StringEncoding],
                                  [pushResponse dataUsingEncoding:NSUTF8StringEncoding]];
    
    testFilter.onInspectResponseData = ^(NSURLRequest *request, NSData *data) {
        return responses[nextResponse++];
    };
    
    testFilter.responseToUse = response;
    testFilter.ignoreNextFilter = YES;
    testFilter.dataToUse = nil;
    
    MSClient *filteredClient = [client clientWithFilter:testFilter];
    MSSyncTable *todoTable = [filteredClient syncTableWithName:TodoTableNoVersion];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoTable];
    
    [todoTable insert:@{ @"id":@"test1", @"name": @"test one"} completion:^(NSDictionary *item, NSError *error) {
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:60.0], @"Test timed out.");
    
    done = NO;
    
    [todoTable pullWithQuery:query completion:^(NSError *error) {
        XCTAssertNil(error, @"Unexpected error: %@", error.description);
        XCTAssertEqual(offline.upsertCalls, 4);
        XCTAssertEqual(offline.upsertedItems, 5);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:300.0], @"Test timed out.");
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

    [todoTable pullWithQuery:query completion:^(NSError *error) {
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
    XCTAssertTrue([self waitForTest:10.1], @"Test timed out.");
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
    
    XCTAssertTrue([self waitForTest:30.1], @"Test timed out.");
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
