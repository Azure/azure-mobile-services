// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <UIKit/UIKit.h>
#import <XCTest/XCTest.h>

#import "MSClient.h"
#import "MSCoreDataStore.h"
#import "MSCoreDataStore+TestHelper.h"
#import "MSJSONSerializer.h"
#import "MSOfflinePassthroughHelper.h"
#import "MSSyncContext.h"
#import "MSTableOperationInternal.h"
#import "MSTableOperationError.h"
#import "TodoItem.h"

@interface MSTableOperationErrorTests : XCTestCase {
    MSClient *client;
    BOOL done;
    MSOfflinePassthroughHelper *offline;
    NSManagedObjectContext *context;
}
@end


#pragma mark * Setup and TearDown

@implementation MSTableOperationErrorTests

-(void) setUp
{
    NSLog(@"%@ setUp", self.name);
    
    client = [MSClient clientWithApplicationURLString:@"https://someUrl/"];
    context = [MSCoreDataStore inMemoryManagedObjectContext];
    offline = [[MSOfflinePassthroughHelper alloc] initWithManagedObjectContext:context];
    
    // Enable offline mode
    client.syncContext = [[MSSyncContext alloc] initWithDelegate:offline dataSource:offline callback:nil];
    
    done = NO;
}

-(void) tearDown
{
    NSLog(@"%@ tearDown", self.name);
}

-(void) testBasicInit {
    MSTableOperation *tableOp = [[MSTableOperation alloc] initWithTable:@"TodoItem" type:MSTableOperationInsert itemId:@"ABC"];
    NSDictionary *item = @{@"id": @"ABC", @"text": @"item one", @"complete": @NO};
    
    NSError *error = [NSError errorWithDomain:MSErrorDomain
                                         code:MSErrorPreconditionFailed
                                     userInfo:@{NSLocalizedDescriptionKey: @"Insert error..."}];
    
    MSTableOperationError *opError = [[MSTableOperationError alloc] initWithOperation:tableOp
                                                                                 item:item
                                                                              context:client.syncContext
                                                                                error:error];
    XCTAssertNotNil(opError);
    
    XCTAssertEqualObjects(opError.itemId, @"ABC");
    XCTAssertEqualObjects(opError.table, @"TodoItem");
    XCTAssertEqual(opError.code, MSErrorPreconditionFailed);
    XCTAssertEqualObjects(opError.description, @"Insert error...");
}

-(void) testSerializedInit {
    MSJSONSerializer *serializer = [MSJSONSerializer new];
    NSDictionary *details = @{
        @"id": @"1-2-3",
        @"code": @MSErrorPreconditionFailed,
        @"description": @"Insert error...",
        @"table": @"TodoItem",
        @"operation": [NSNumber numberWithInteger:MSTableOperationInsert],
        @"itemId": @"ABC",
        @"item": @{ @"id":@"ABC", @"text":@"one", @"complete":@NO },
        @"serverItem": @{ @"id":@"ABC", @"text":@"two", @"complete":@YES },
        @"statusCode": @312
    };
    NSData *data = [serializer dataFromItem:details idAllowed:YES ensureDictionary:NO removeSystemProperties:NO orError:nil];
    
    NSDictionary *serializedError = @{
        @"id": @"1-2-3",
        @"properties": data
    };
    
    MSTableOperationError *opError = [[MSTableOperationError alloc] initWithSerializedItem:serializedError
                                                                                   context:client.syncContext];
    
    XCTAssertNotNil(opError);
    XCTAssertEqualObjects(opError.itemId, @"ABC");
    XCTAssertEqualObjects(opError.table, @"TodoItem");
    XCTAssertEqual(opError.code, MSErrorPreconditionFailed);
    XCTAssertEqualObjects(opError.description, @"Insert error...");
}

-(void) testCancelAndDiscard {
    NSDictionary *item = @{@"id": @"ABC", @"text": @"initial value" };
    MSTableOperation *tableOp = [self createPendingOperationForItem:item];
    MSTableOperationError *opError = [[MSTableOperationError alloc] initWithOperation:tableOp
                                                                                 item:item
                                                                              context:client.syncContext
                                                                                error:nil];
    // Cancel our operation now
    XCTestExpectation *cancelExpectation = [self expectationWithDescription:@"CancelAndDiscard"];
    [opError cancelOperationAndDiscardItemWithCompletion:^(NSError *error) {
        XCTAssertNil(error);
        
        [cancelExpectation fulfill];
    }];
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
    
    // Verify item is no longer in the local store layer
    NSError *error;
    NSFetchRequest *fetchRequest = [NSFetchRequest fetchRequestWithEntityName:@"TodoItem"];
    NSArray *allTodos = [context executeFetchRequest:fetchRequest error:&error];
    XCTAssertNil(error);
    XCTAssertEqual(allTodos.count, 0);
    
    // Verify operation is not in the local store layer
    fetchRequest = [NSFetchRequest fetchRequestWithEntityName:offline.operationTableName];
    NSArray *allOps = [context executeFetchRequest:fetchRequest error:&error];
    XCTAssertNil(error);
    XCTAssertEqual(allOps.count, 0);
}

-(void) testCancelAndUpdate {
    NSDictionary *item = @{@"id": @"ABC", @"text": @"initial value" };
    MSTableOperation *tableOp = [self createPendingOperationForItem:item];
    MSTableOperationError *opError = [[MSTableOperationError alloc] initWithOperation:tableOp
                                                                                 item:item
                                                                              context:client.syncContext
                                                                                error:nil];
    
    // Cancel our pending operation and update the stored value
    XCTestExpectation *expectation = [self expectationWithDescription:@"CancelAndUpdateOperation"];
    NSDictionary *newItem = @{ @"id": @"ABC", @"text": @"value two" };
    [opError cancelOperationAndUpdateItem:newItem completion:^(NSError *error) {
        XCTAssertNil(error);
        [expectation fulfill];
    }];
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
    
    // Verify item is updated in the local store layer
    NSError *error;
    NSFetchRequest *fetchRequest = [NSFetchRequest fetchRequestWithEntityName:@"TodoItem"];
    NSArray *allTodos = [context executeFetchRequest:fetchRequest error:&error];
    XCTAssertNil(error);
    
    XCTAssertEqual(allTodos.count, 1);
    TodoItem *updatedItem = allTodos[0];
    XCTAssertEqualObjects(updatedItem.text, @"value two");
    
    // Verify operation is not in the local store layer
    fetchRequest = [NSFetchRequest fetchRequestWithEntityName:offline.operationTableName];
    NSArray *allOps = [context executeFetchRequest:fetchRequest error:&error];
    XCTAssertNil(error);
    XCTAssertEqual(allOps.count, 0);
    
}

-(void) testCancelAndUpdate_NoItem {
    NSDictionary *item = @{@"id": @"ABC", @"text": @"initial value" };
    MSTableOperation *tableOp = [self createPendingOperationForItem:item];
    MSTableOperationError *opError = [[MSTableOperationError alloc] initWithOperation:tableOp
                                                                                 item:item
                                                                              context:client.syncContext
                                                                                error:nil];
    
    // Cancel our pending operation and update the stored value
    XCTestExpectation *expectation = [self expectationWithDescription:@"CancelAndUpdateOperation"];
    [opError cancelOperationAndUpdateItem:nil completion:^(NSError *error) {
        XCTAssertNotNil(error);
        [expectation fulfill];
    }];
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
    
    // Verify operation is still in the local store layer
    NSError *error = nil;
    NSFetchRequest *fetchRequest = [NSFetchRequest fetchRequestWithEntityName:offline.operationTableName];
    NSArray *allOps = [context executeFetchRequest:fetchRequest error:&error];
    XCTAssertNil(error);
    XCTAssertEqual(allOps.count, 1);
    
}

- (MSTableOperation *) createPendingOperationForItem:(NSDictionary *)item
{
    MSSyncTable *table = [client syncTableWithName:@"TodoItem"];
    
    XCTestExpectation *expectation = [self expectationWithDescription:@"UpsertRecord"];
    [table update:item completion:^(NSError *error) {
        XCTAssertNil(error);
        [expectation fulfill];
    }];
    [self waitForExpectationsWithTimeout:1.0 handler:nil];

    // To be safe, verify operation is in the local store layer
    NSError *error;
    NSFetchRequest *fetchRequest = [NSFetchRequest fetchRequestWithEntityName:offline.operationTableName];
    NSArray *allOps = [context executeFetchRequest:fetchRequest error:&error];
    XCTAssertNil(error);
    XCTAssertEqual(allOps.count, 1);
    
    // Create a op that matches what we just did & fake an error
    MSTableOperation *tableOp = [[MSTableOperation alloc] initWithTable:@"TodoItem" type:MSTableOperationInsert itemId:@"ABC"];
    tableOp.operationId = [[allOps[0] valueForKey:@"id"] integerValue];

    return tableOp;
}

@end
