// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <SenTestingKit/SenTestingKit.h>
#import "WindowsAzureMobileServices.h"
#import "MSTestFilter.h"
#import "MSTable+MSTableTestUtilities.h"

@interface MSTableFuncTests : SenTestCase
@property (nonatomic) BOOL testsEnabled;
@property (nonatomic) BOOL done;
@property (nonatomic, strong) MSTable *table;
@end

@implementation MSTableFuncTests

- (void)setUp
{
    [super setUp];
    
    NSLog(@"%@ setUp", self.name);
    
    self.testsEnabled = YES;
    STAssertTrue(self.testsEnabled, @"The functional tests are currently disabled.");
    
    // These functional tests requires a working Windows Mobile Azure Service
    // with a table named "todoItem". Simply enter the application URL and
    // application key for the Windows Mobile Azure Service below and set the
    // 'testsEnabled' BOOL above to YES.
    MSClient *client = [MSClient
                        clientWithApplicationURLString:@"<Windows Azure Mobile Service App URL>"
                        applicationKey:@"<Application Key>"];
    
    self.done = NO;
    self.table = [client tableWithName:@"stringId_objC_test_table"];
    
    STAssertNotNil(self.table, @"Could not create test table.");
    
    // Clean up table, all tests start at empty table
    [self cleanUpData];
}

- (void)tearDown
{
    // Put teardown code here; it will be run once, after the last test case.
    [self cleanUpData];
    self.table = nil;
    
    [super tearDown];
}

- (void) cleanUpData
{
    // Clean up table, all tests start at empty table
    self.done = NO;
    [self.table deleteAllItemsWithCompletion:^(NSError *error) {
        STAssertNil(error, error.localizedDescription);
        self.done = YES;
    }];
    [self waitForTest:90.0];
}

- (void) populateData
{
    self.done = NO;
    
    NSArray *validIds = [MSTable testValidStringIds];
    NSMutableArray *items = [NSMutableArray array];
    for(NSString *validId in validIds)
    {
        [items addObject:@{@"id": validId, @"string": @"Hey" }];
    }
    
    [self.table insertItems:items completion:^(NSError *error) {
        STAssertNil(error, @"Couldn't insert all records for test");
        self.done = YES;
    }];
    [self waitForTest:90.0];
}


- (void)testTableOperationsWithValidStringIdAgainstStringIdTable
{
    NSArray *testIds = [MSTable testValidStringIds];
    
    // Verify functional queries with valid ids
    for (NSString *testId in testIds) {
        self.done = NO;
        [self.table insert:@{@"id": testId, @"string": @"Hey"} completion:^(NSDictionary *item, NSError *error) {
            STAssertNil(error, @"Insert failed for id %@ with error: %@", testId, error.localizedDescription);
            STAssertEqualObjects(testId, [item objectForKey:@"id"], @"Id %@ was not found", testId);
            STAssertEqualObjects(@"Hey", [item objectForKey:@"string"], @"String value was not retrieved correctly");
            self.done = YES;
        }];
        [self waitForTest:30.0];
        
        self.done = NO;
        [self.table readWithId:testId completion:^(NSDictionary *item, NSError *error) {
            STAssertNil(error, @"ReadWithId failed for id %@ with error: %@", testId, error.localizedDescription);
            STAssertEqualObjects(testId, [item objectForKey:@"id"], @"Id %@ was not found", testId);
            STAssertEqualObjects(@"Hey", [item objectForKey:@"string"], @"String value was not retrieved correctly");
            self.done = YES;
        }];
        [self waitForTest:15.0];
        
        NSPredicate *predicate = [NSPredicate predicateWithFormat:@"id == %@", testId];
        self.done = NO;
        [self.table readWithPredicate:predicate completion:^(NSArray *items, NSInteger totalCount, NSError *error) {
            STAssertNil(error, @"Failure for id %@: %@", testId, error.localizedDescription);
            NSDictionary *item = [items objectAtIndex:0];
            STAssertEqualObjects(testId, [item objectForKey:@"id"], @"Id %@ was not found", testId);
            STAssertEqualObjects(@"Hey", [item objectForKey:@"string"], @"String value was not retrieved correctly");
            self.done = YES;
        }];
        [self waitForTest:15.0];
         
        // projection, querystring
        MSQuery *query = [[MSQuery alloc] initWithTable:self.table];
        query.selectFields = @[@"id", @"string"];
        self.done = NO;
        [query readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
            STAssertNil(error, @"Failure for id %@: %@", testId, error.localizedDescription);
            STAssertTrue(items.count == 1, @"Incorrect number of items found");
            NSDictionary *item = [items objectAtIndex:0];
            STAssertEqualObjects(testId, [item objectForKey:@"id"], @"Invalid id found");
            STAssertEqualObjects(@"Hey", [item objectForKey:@"string"], @"Invalid string value found");
            self.done = YES;
        }];
        [self waitForTest:15.0];
        
        // update
        self.done = NO;
        [self.table update:@{@"id":testId, @"string":@"What?"} completion:^(NSDictionary *item, NSError *error) {
            STAssertNil(error, @"Failure for id %@: %@", testId, error.localizedDescription);
            STAssertEqualObjects(testId, [item objectForKey:@"id"], @"Invalid id found");
            STAssertEqualObjects(@"What?", [item objectForKey:@"string"], @"Invalid string value found");
            self.done = YES;
        }];
        [self waitForTest:15.0];
         
        // delete
        self.done = NO;
        [self.table deleteWithId:testId completion:^(id itemId, NSError *error) {
            STAssertNil(error, @"Failure for id %@: %@", testId, error.localizedDescription);
            STAssertEqualObjects(testId, itemId, @"Deleted wrong item; %@ and not %@", itemId, testId);
            self.done = YES;
        }];
        [self waitForTest:15.0];
                             
        // Insert again, would fail if id in use
        self.done = NO;
        [self.table insert:@{@"id": testId, @"string": @"Hey"} completion:^(NSDictionary *item, NSError *error) {
            STAssertNil(error, @"Insert failed for id %@ with error: %@", testId, error.localizedDescription);
            self.done = YES;
        }];
        [self waitForTest:15.0];
         
        // delete with item
        self.done = NO;
        [self.table delete:@{@"id":testId} completion:^(id itemId, NSError *error) {
            STAssertNil(error, @"Failure for id %@: %@", testId, error.localizedDescription);
            STAssertEqualObjects(testId, itemId, @"Deleted wrong item; %@ and not %@", itemId, testId);
            self.done = YES;
        }];
        [self waitForTest:15.0];

        // verify no item
        self.done = NO;
        [self.table readWithId:testId completion:^(NSDictionary *item, NSError *error) {
            STAssertNil(item, @"Item should have been deleted");
            NSString *expectedErrorMessage = [NSString stringWithFormat:@"Error: An item with id '%@' does not exist.", testId];
            STAssertEquals(MSErrorMessageErrorCode, error.code, @"Unexpected error code: %d: %@", error.code, error.localizedDescription);
            STAssertEqualObjects(expectedErrorMessage, error.localizedDescription, @"Wrong error: %@", error.localizedDescription);
            self.done = YES;
        }];
        [self waitForTest:15.0];
    };
}

-(void) testOrderingReadWithValidStringIdAgainstStringIdTable
{	
    NSArray *testIdData = @[ @"a", @"b", @"C", @"_A", @"_B", @"_C", @"1", @"2", @"3" ];
    NSArray *orderedTestIdData = @[ @"_A", @"_B", @"_C", @"1", @"2", @"3", @"a", @"b", @"C" ];
    
    // Populate table with data
    NSMutableArray *originalItems = [NSMutableArray array];
    for (NSString *testId in testIdData)
    {
        [originalItems addObject:@{@"id" : testId, @"string": @"Hey" }];
    }
    self.done = NO;
    [self.table insertItems:originalItems completion:^(NSError *error) {
        STAssertNil(error, @"Test failed with error: %@", error.localizedDescription);
        self.done = YES;
    }];
    [self waitForTest:90.0];
    
    // Verify order by ascending works
    self.done = NO;
    MSQuery *query = [[MSQuery alloc] initWithTable:self.table];
    [query orderByAscending:@"id"];
    [query readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
        STAssertNil(error, @"Test failed with error: %@", error.localizedDescription);
        NSDictionary *item;
        NSUInteger index = 0;
        for(NSString *sortedId in orderedTestIdData) {
            item = [items objectAtIndex:index++];
            STAssertEqualObjects(sortedId, [item objectForKey:@"id"], @"incorrect id order");
        }
        self.done = YES;
    }];
    [self waitForTest:90.0];

    // Verify order by descending works
    self.done = NO;
    query = [[MSQuery alloc] initWithTable:self.table];
    [query orderByDescending:@"id"];
    [query readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
        STAssertNil(error, @"Test failed with error: %@", error.localizedDescription);
        NSDictionary *item;
        NSUInteger index = orderedTestIdData.count;
        for(NSString *sortedId in orderedTestIdData) {
            item = [items objectAtIndex:--index];
            STAssertEqualObjects(sortedId, [item objectForKey:@"id"], @"incorrect id order");
        }
        self.done = YES;
    }];
    [self waitForTest:90.0];
}

-(void) testFilterReadWithEmptyAndInvalidStringIdsAgainstStringIdTable
{
    NSArray *testIds = [MSTable testInvalidStringIds];
    testIds = [testIds arrayByAddingObjectsFromArray:[MSTable testEmptyStringIdsIncludingNull:YES]];
    
    [self populateData];
    for (NSString *testId in testIds) {
        self.done = NO;
        NSPredicate *predicate = [NSPredicate predicateWithFormat:@"id == %@", testId];
        [self.table readWithPredicate:predicate completion:^(NSArray *items, NSInteger totalCount, NSError *error) {
            STAssertNil(error, error.localizedDescription);
            STAssertTrue(items.count == 0, @"Received %d results when none expected", items.count);
            self.done = YES;
        }];
        STAssertTrue([self waitForTest:20.0], @"Test timed out.");
    }
}

-(void) testReadIdWithInvalidIdsAgainstStringIdTable
{
    NSArray *testIds = [MSTable testInvalidStringIds];
    for(NSString *testId in testIds)
    {
        self.done = NO;
        [self.table readWithId:testId completion:^(NSDictionary *item, NSError *error) {
            STAssertNil(item, @"Unexpected item found for id %@", testId);
            STAssertEquals(MSInvalidItemIdWithRequest, error.code, @"Unexpected error code: %d", error.code);
            STAssertEqualObjects(@"The item provided did not have a valid id.", error.localizedDescription, @"Incorrect error message: %@", error.localizedDescription);
            self.done = YES;
        }];
        STAssertTrue([self waitForTest:20.0], @"Test timed out.");
    }
}

-(void) testInsertWithEmptyStringIdAgainstStringIdTable
{
    NSUInteger count = 0;
    NSArray *testIds = [MSTable testEmptyStringIdsIncludingNull:YES];
    NSString *countAsString;
    for (NSString *testId in testIds)
    {
        countAsString = [NSString stringWithFormat:@"%d", count++];
        NSDictionary *item = @{ @"id": testId, @"string": countAsString };
        
        self.done = NO;
        [self.table insert:item completion:^(NSDictionary *item, NSError *error) {
            STAssertNil(error, error.localizedDescription);
            STAssertNotNil([item objectForKey:@"id"], @"Inserted item did not have an id");
            STAssertEqualObjects(countAsString, [item objectForKey:@"string"], @"Item's value was incorrect");
            self.done = YES;
        }];
        STAssertTrue([self waitForTest:20.0], @"Test timed out.");
    }
}

-(void) testInsertWithExistingItemAgainstStringIdTable
{
    [self populateData];
    
    NSArray *testIds = [MSTable testValidStringIds];
    for(NSString *testId in testIds)
    {
        self.done = NO;
        [self.table insert:@{@"id": testId, @"string": @"No we're talking!"} completion:^(NSDictionary *item, NSError *error) {
            STAssertNotNil(error, @"An error should have occurred");
            STAssertEquals(MSErrorMessageErrorCode, error.code, @"Unexpected error code:  %d", error.code);
            STAssertEqualObjects(@"Error: Could not insert the item because an item with that id already exists.", error.localizedDescription, @"Wrong error: %@", error.localizedDescription);
            self.done = YES;
        }];
        STAssertTrue([self waitForTest:90.0], @"Test timed out.");
    }
}

- (void) testUpdateWithNosuchItemAgainstStringIdTable
{
    NSArray *testIds = [MSTable testValidStringIds];
    for(NSString *testId in testIds)
    {
        self.done = NO;
        [self.table update:@{@"id":testId, @"string": @"Alright!"} completion:^(NSDictionary *item, NSError *error) {
            STAssertNotNil(error, @"An error should have occurred");
            STAssertEquals(MSErrorMessageErrorCode, error.code, @"Unexpected error code:  %d", error.code);
            NSString *expectedErrorMessage = [NSString stringWithFormat:@"Error: An item with id '%@' does not exist.", testId];
            STAssertEqualObjects(expectedErrorMessage, error.localizedDescription, @"Wrong error: %@", error.localizedDescription);
            self.done = YES;
        }];
        STAssertTrue([self waitForTest:90.0], @"Test timed out.");
    }
}

-(void) testDeleteWithNosuchItemAgainstStringIdTable
{
    NSArray *testIds = [MSTable testValidStringIds];
    for (NSString *testId in testIds)
    {
        self.done = NO;
        [self.table delete:@{@"id":testId} completion:^(id itemId, NSError *error) {
            STAssertNotNil(error, @"An error should have occurred");
            self.done = YES;
        }];
        STAssertTrue([self waitForTest:90.0], @"Test timed out.");
    }
}

-(void) testDeleteWithIdWithNosuchItemAgainstStringIdTable
{
    NSArray *testIds = [MSTable testValidStringIds];
    for (NSString *testId in testIds)
    {
        self.done = NO;
        [self.table deleteWithId:testId completion:^(id itemId, NSError *error) {
            STAssertNotNil(error, @"An error should have occurred");
            self.done = YES;
        }];
        STAssertTrue([self waitForTest:90.0], @"Test timed out.");
    }
}

# pragma mark Test Utilities

-(BOOL) waitForTest:(NSTimeInterval)testDuration
{
    NSDate *timeoutAt = [NSDate dateWithTimeIntervalSinceNow:testDuration];
    while (!self.done) {
        [[NSRunLoop currentRunLoop] runMode:NSDefaultRunLoopMode
                                 beforeDate:timeoutAt];
        if([timeoutAt timeIntervalSinceNow] <= 0.0) {
            break;
        }
    };
    return self.done;
}

@end
