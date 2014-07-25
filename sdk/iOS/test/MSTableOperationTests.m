// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <SenTestingKit/SenTestingKit.h>
#import "MSTableOperation.h"
#import "MSTableOperationInternal.h"
@interface MSTableOperationTests : SenTestCase {
    BOOL done;
}
@end

@implementation MSTableOperationTests

- (void)setUp
{
    [super setUp];
    // Put setup code here. This method is called before the invocation of each test method in the class.
}

- (void)tearDown
{
    // Put teardown code here. This method is called after the invocation of each test method in the class.
    [super tearDown];
}

- (void)testOperationAbortSuccess
{
    NSOperation *baseOperation = [[NSOperation alloc] init];
    MSTableOperation *tableOperation = [[MSTableOperation alloc] initWithTable:@"testTable" type:MSTableOperationInsert itemId:@"ABC"];
    tableOperation.pushOperation = baseOperation;
    
    [tableOperation cancelPush];
    
    STAssertTrue(baseOperation.isCancelled, @"NSOperation was not cancelled");
}

- (void)testOperationSerializationSuccess
{
    MSTableOperation *originalTableOperation = [[MSTableOperation alloc] initWithTable:@"testTable" type:MSTableOperationInsert itemId:@"ABC"];
    originalTableOperation.operationId = 7;
    originalTableOperation.type = MSTableOperationInsert;
    originalTableOperation.item = @{ @"id" : @1, @"column1": @NO, @"column2": @"yes" };
    
    NSDictionary *info = [originalTableOperation serialize];
    MSTableOperation *tableOperation = [[MSTableOperation alloc] initWithItem:info];
    
    STAssertEquals((int)tableOperation.operationId, 7, @"Incorrect id");
    STAssertEquals(tableOperation.tableName, @"testTable", @"Incorrect table name");
    STAssertEquals(tableOperation.itemId, @"ABC", @"Incorrect table name");
    STAssertEquals(tableOperation.type, MSTableOperationInsert, @"incorrect type");
    STAssertNil(tableOperation.item, @"Did not expect an item");
}

- (void)testDeleteOperationSerialization_KeepsItem_Success
{
    MSTableOperation *originalTableOperation = [[MSTableOperation alloc] initWithTable:@"testTable" type:MSTableOperationInsert itemId:@"ABC"];
    originalTableOperation.operationId = 7;
    originalTableOperation.type = MSTableOperationDelete;
    originalTableOperation.item = @{ @"id" : @1, @"column1": @YES, @"column2": @"Hello" };
    
    NSDictionary *info = [originalTableOperation serialize];
    MSTableOperation *tableOperation = [[MSTableOperation alloc] initWithItem:info];
    
    STAssertEquals((int)tableOperation.operationId, 7, @"Incorrect id");
    STAssertEquals(tableOperation.tableName, @"testTable", @"Incorrect table name");
    STAssertEquals(tableOperation.itemId, @"ABC", @"Incorrect table name");
    STAssertEquals(tableOperation.type, MSTableOperationDelete, @"incorrect type");
    STAssertNotNil(tableOperation.item, @"Expected an item");
    STAssertEqualObjects(tableOperation.item[@"id"], @1, nil);
    STAssertEqualObjects(tableOperation.item[@"column1"], @YES, nil);
    STAssertEqualObjects(tableOperation.item[@"column2"], @"Hello", nil);
}

@end
