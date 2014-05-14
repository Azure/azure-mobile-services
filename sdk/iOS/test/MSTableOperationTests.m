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
    MSTableOperation *tableOperation = [[MSTableOperation alloc] initWithTable:@"testTable" type:MSTableOperationInsert itemId:@"ABC"];
    tableOperation.operationId = 7;
    tableOperation.
    
    
    STAssertEquals(tableOperation.operationId, 7, @"Incorrect id");
    STAssertEquals(tableOperation.tableName, @"testTable", @"Incorrect table name");
    STAssertEquals(tableOperation.itemId, @"ABC", @"Incorrect table name");
    STAssertEquals(tableOperation.type, MSTableOperationInsert, @"incorrect type");
    
    tableOperation.type = MSTableOperationInsert;
    
    NSDictionary *info = [tableOperation serialize];
    
    STAssertEquals(info, serializedOp, @"not equal");
}
@end
