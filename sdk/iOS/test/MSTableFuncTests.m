// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <XCTest/XCTest.h>
#import "WindowsAzureMobileServices.h"
#import "MSTestFilter.h"
#import "MSTable+MSTableTestUtilities.h"
#import "MSJSONSerializer.h"
#import "MSQueryResult.h"

@interface MSTableFuncTests : XCTestCase
@property (nonatomic) BOOL done;
@property (nonatomic, strong) MSTable *table;
@end

@implementation MSTableFuncTests

-(void) setUp
{
    [super setUp];
    self.continueAfterFailure = NO;
    
    NSLog(@"%@ setUp", self.name);
    
    // These functional tests requires a working Windows Mobile Azure Service
    // with a table named "todoItem". Enter the application URL and application
    // key for the Windows Mobile Azure Service in the settings.plist file.
    
    NSString *path = [[NSBundle mainBundle] pathForResource:@"settings" ofType:@"plist"];
    NSDictionary *settings = [[NSDictionary alloc] initWithContentsOfFile:path];
    
    MSClient *client = [MSClient clientWithApplicationURLString:[settings objectForKey:@"TestAppUrl"]];
    
    XCTAssertTrue([client.applicationURL.description hasPrefix:@"https://"], @"The functional tests are currently disabled.");
    self.continueAfterFailure = YES;
    
    self.table = [client tableWithName:@"stringId_objC_test_table"];
    XCTAssertNotNil(self.table, @"Could not create test table.");
    
    // Clean up table, all tests start at empty table
    [self cleanUpData];
    
    self.done = NO;
}

- (void)tearDown
{
    self.continueAfterFailure = YES;
    
    // Put teardown code here; it will be run once, after the last test case.
    self.table = nil;
    
    [super tearDown];
}

- (void) cleanUpData
{
    // Clean up table, all tests start at empty table
    self.done = NO;
    [self.table deleteAllItemsWithCompletion:^(NSError *error) {
        XCTAssertNil(error, @"Error %@", error.localizedDescription);
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
        XCTAssertNil(error, @"Couldn't insert all records for test");
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
            XCTAssertNil(error, @"Insert failed for id %@ with error: %@", testId, error.localizedDescription);
            XCTAssertEqualObjects(testId, item[@"id"], @"Id %@ was not found", testId);
            XCTAssertEqualObjects(@"Hey", item[@"string"], @"String value was not retrieved correctly");
            self.done = YES;
        }];
        [self waitForTest:30.0];
        
        self.done = NO;
        [self.table readWithId:testId completion:^(NSDictionary *item, NSError *error) {
            XCTAssertNil(error, @"ReadWithId failed for id %@ with error: %@", testId, error.localizedDescription);
            XCTAssertEqualObjects(testId, item[@"id"], @"Id %@ was not found", testId);
            XCTAssertEqualObjects(@"Hey", item[@"string"], @"String value was not retrieved correctly");
            self.done = YES;
        }];
        [self waitForTest:15.0];
        
        NSPredicate *predicate = [NSPredicate predicateWithFormat:@"id == %@", testId];
        self.done = NO;
        [self.table readWithPredicate:predicate completion:^(MSQueryResult *result, NSError *error) {
            XCTAssertNil(error, @"Failure for id %@: %@", testId, error.localizedDescription);
            NSDictionary *item = result.items[0];
            XCTAssertEqualObjects(testId, item[@"id"], @"Id %@ was not found", testId);
            XCTAssertEqualObjects(@"Hey", item[@"string"], @"String value was not retrieved correctly");
            self.done = YES;
        }];
        [self waitForTest:15.0];
         
        // projection, querystring
        MSQuery *query = [[MSQuery alloc] initWithTable:self.table];
        query.selectFields = @[@"id", @"string"];
        self.done = NO;
        [query readWithCompletion:^(MSQueryResult *result, NSError *error) {
            XCTAssertNil(error, @"Failure for id %@: %@", testId, error.localizedDescription);
            XCTAssertTrue(result.items.count == 1, @"Incorrect number of items found");
            NSDictionary *item = result.items[0];
            XCTAssertEqualObjects(testId, item[@"id"], @"Invalid id found");
            XCTAssertEqualObjects(@"Hey", item[@"string"], @"Invalid string value found");
            self.done = YES;
        }];
        [self waitForTest:15.0];
        
        // update
        self.done = NO;
        [self.table update:@{@"id":testId, @"string":@"What?"} completion:^(NSDictionary *item, NSError *error) {
            XCTAssertNil(error, @"Failure for id %@: %@", testId, error.localizedDescription);
            XCTAssertEqualObjects(testId, item[@"id"], @"Invalid id found");
            XCTAssertEqualObjects(@"What?", item[@"string"], @"Invalid string value found");
            self.done = YES;
        }];
        [self waitForTest:15.0];
         
        // delete
        self.done = NO;
        [self.table deleteWithId:testId completion:^(id itemId, NSError *error) {
            XCTAssertNil(error, @"Failure for id %@: %@", testId, error.localizedDescription);
            XCTAssertEqualObjects(testId, itemId, @"Deleted wrong item; %@ and not %@", itemId, testId);
            self.done = YES;
        }];
        [self waitForTest:15.0];
                             
        // Insert again, would fail if id in use
        self.done = NO;
        [self.table insert:@{@"id": testId, @"string": @"Hey"} completion:^(NSDictionary *item, NSError *error) {
            XCTAssertNil(error, @"Insert failed for id %@ with error: %@", testId, error.localizedDescription);
            self.done = YES;
        }];
        [self waitForTest:15.0];
         
        // delete with item
        self.done = NO;
        [self.table delete:@{@"id":testId} completion:^(id itemId, NSError *error) {
            XCTAssertNil(error, @"Failure for id %@: %@", testId, error.localizedDescription);
            XCTAssertEqualObjects(testId, itemId, @"Deleted wrong item; %@ and not %@", itemId, testId);
            self.done = YES;
        }];
        [self waitForTest:15.0];

        // verify no item
        self.done = NO;
        [self.table readWithId:testId completion:^(NSDictionary *item, NSError *error) {
            XCTAssertNil(item, @"Item should have been deleted");
            NSString *expectedErrorMessage = [NSString stringWithFormat:@"Error: An item with id '%@' does not exist.", testId];
            XCTAssertEqual([@MSErrorMessageErrorCode integerValue], error.code, @"Unexpected error code: %ld: %@", (long)error.code, error.localizedDescription);
            XCTAssertEqualObjects(expectedErrorMessage, error.localizedDescription, @"Wrong error: %@", error.localizedDescription);
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
        [originalItems addObject:@{@"id":testId, @"string": @"Hey" }];
    }
    self.done = NO;
    [self.table insertItems:originalItems completion:^(NSError *error) {
        XCTAssertNil(error, @"Test failed with error: %@", error.localizedDescription);
        self.done = YES;
    }];
    [self waitForTest:90.0];
    
    // Verify order by ascending works
    self.done = NO;
    MSQuery *query = [[MSQuery alloc] initWithTable:self.table];
    [query orderByAscending:@"id"];
    [query readWithCompletion:^(MSQueryResult *result, NSError *error) {
        XCTAssertNil(error, @"Test failed with error: %@", error.localizedDescription);
        NSDictionary *item;
        NSUInteger index = 0;
        for(NSString *sortedId in orderedTestIdData) {
            item = result.items[index++];
            XCTAssertEqualObjects(sortedId, item[@"id"], @"incorrect id order");
        }
        self.done = YES;
    }];
    [self waitForTest:90.0];

    // Verify order by descending works
    self.done = NO;
    query = [[MSQuery alloc] initWithTable:self.table];
    [query orderByDescending:@"id"];
    [query readWithCompletion:^(MSQueryResult *result, NSError *error) {
        XCTAssertNil(error, @"Test failed with error: %@", error.localizedDescription);
        NSDictionary *item;
        NSUInteger index = orderedTestIdData.count;
        for(NSString *sortedId in orderedTestIdData) {
            item = result.items[--index];
            XCTAssertEqualObjects(sortedId, item[@"id"], @"incorrect id order");
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
        [self.table readWithPredicate:predicate completion:^(MSQueryResult *result, NSError *error) {
            XCTAssertNil(error, @"Error: %@", error.localizedDescription);
            XCTAssertTrue(result.items.count == 0, @"Received %lu results when none expected", (unsigned long)result.items.count);
            self.done = YES;
        }];
        XCTAssertTrue([self waitForTest:20.0], @"Test timed out.");
    }
}

-(void) testReadIdWithInvalidIdsAgainstStringIdTable
{
    NSArray *testIds = [MSTable testInvalidStringIds];
    for(NSString *testId in testIds)
    {
        self.done = NO;
        [self.table readWithId:testId completion:^(NSDictionary *item, NSError *error) {
            XCTAssertNil(item, @"Unexpected item found for id %@", testId);
            XCTAssertEqual([@MSInvalidItemIdWithRequest integerValue], error.code, @"Unexpected error code: %ld", (long)error.code);
            XCTAssertEqualObjects(@"The item provided did not have a valid id.", error.localizedDescription, @"Incorrect error message: %@", error.localizedDescription);
            self.done = YES;
        }];
        XCTAssertTrue([self waitForTest:20.0], @"Test timed out.");
    }
}

-(void) testInsertWithEmptyStringIdAgainstStringIdTable
{
    NSUInteger count = 0;
    NSArray *testIds = [MSTable testEmptyStringIdsIncludingNull:YES];
    NSString *countAsString;
    for (NSString *testId in testIds)
    {
        countAsString = [NSString stringWithFormat:@"%lu", (unsigned long)count++];
        NSDictionary *item = @{ @"id": testId, @"string": countAsString };
        
        self.done = NO;
        [self.table insert:item completion:^(NSDictionary *item, NSError *error) {
            XCTAssertNil(error, @"Error: %@", error.localizedDescription);
            XCTAssertNotNil(item[@"id"], @"Inserted item did not have an id");
            XCTAssertEqualObjects(countAsString, item[@"string"], @"Item's value was incorrect");
            self.done = YES;
        }];
        XCTAssertTrue([self waitForTest:20.0], @"Test timed out.");
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
            XCTAssertNotNil(error, @"An error should have occurred");
            XCTAssertEqual([@MSErrorMessageErrorCode integerValue], error.code, @"Unexpected error code:  %ld", (long)error.code);
            XCTAssertEqualObjects(@"Error: Could not insert the item because an item with that id already exists.", error.localizedDescription, @"Wrong error: %@", error.localizedDescription);
            self.done = YES;
        }];
        XCTAssertTrue([self waitForTest:90.0], @"Test timed out.");
    }
}

- (void) testUpdateWithNosuchItemAgainstStringIdTable
{
    NSArray *testIds = [MSTable testValidStringIds];
    for(NSString *testId in testIds)
    {
        self.done = NO;
        [self.table update:@{@"id":testId, @"string": @"Alright!"} completion:^(NSDictionary *item, NSError *error) {
            XCTAssertNotNil(error, @"An error should have occurred");
            XCTAssertEqual([@MSErrorMessageErrorCode integerValue], error.code, @"Unexpected error code:  %ld", (long)error.code);
            NSString *expectedErrorMessage = [NSString stringWithFormat:@"Error: An item with id '%@' does not exist.", testId];
            XCTAssertEqualObjects(expectedErrorMessage, error.localizedDescription, @"Wrong error: %@", error.localizedDescription);
            self.done = YES;
        }];
        XCTAssertTrue([self waitForTest:90.0], @"Test timed out.");
    }
}

-(void) testDeleteWithNosuchItemAgainstStringIdTable
{
    NSArray *testIds = [MSTable testValidStringIds];
    for (NSString *testId in testIds)
    {
        self.done = NO;
        [self.table delete:@{@"id":testId} completion:^(id itemId, NSError *error) {
            XCTAssertNotNil(error, @"An error should have occurred");
            XCTAssertEqual([@MSErrorMessageErrorCode integerValue], error.code, @"Unexpected error code: %ld", (long)error.code);
            NSString *expectedErrorMessage = [NSString stringWithFormat:@"Error: An item with id '%@' does not exist.", testId];
            XCTAssertEqualObjects(expectedErrorMessage, error.localizedDescription, @"Wrong error: %@", error.localizedDescription);
            self.done = YES;
        }];
        XCTAssertTrue([self waitForTest:90.0], @"Test timed out.");
    }
}

-(void) testDeleteWithIdWithNosuchItemAgainstStringIdTable
{
    NSArray *testIds = [MSTable testValidStringIds];
    for (NSString *testId in testIds)
    {
        self.done = NO;
        [self.table deleteWithId:testId completion:^(id itemId, NSError *error) {
            XCTAssertNotNil(error, @"An error should have occurred");	
            XCTAssertEqual([@MSErrorMessageErrorCode integerValue], error.code, @"Unexpected error code: %ld", (long)error.code);
            NSString *expectedErrorMessage = [NSString stringWithFormat:@"Error: An item with id '%@' does not exist.", testId];
            XCTAssertEqualObjects(expectedErrorMessage, error.localizedDescription, @"Wrong error: %@", error.localizedDescription);
            self.done = YES;
        }];
        XCTAssertTrue([self waitForTest:15.0], @"Test timed out.");
    }
}


#pragma mark System Properties


-(void) testAsyncTableOperationsWithAllSystemProperties
{
    NSString *myid = @"an id";
    
    NSDictionary *item = @{ @"id": myid, @"string": @"a value" };
    self.done = NO;
    [self.table insert:item completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNotNil(item[MSSystemColumnCreatedAt], @"Missing property");
        XCTAssertNotNil(item[MSSystemColumnUpdatedAt], @"Missing property");
        XCTAssertNotNil(item[MSSystemColumnVersion], @"Missing property");
        self.done = YES;
    }];
    [self waitForTest:30.0];
    
    self.done = NO;
    __block id savedItem;
    [self.table readWithCompletion:^(MSQueryResult *result, NSError *error) {
        XCTAssertTrue(result.items.count == 1, @"Incorrect count: %lu", (unsigned long)result.items.count);
        NSDictionary *item = result.items[0];
        XCTAssertNotNil(item[MSSystemColumnCreatedAt], @"Missing property");
        XCTAssertNotNil(item[MSSystemColumnUpdatedAt], @"Missing property");
        XCTAssertNotNil(item[MSSystemColumnVersion], @"Missing property");
        savedItem = item;
        self.done = YES;
    }];
    [self waitForTest:30.0];

    self.done = NO;
    NSPredicate *predicate = [NSPredicate predicateWithFormat:@"__version == %@", savedItem[MSSystemColumnVersion]];
    [self.table readWithPredicate:predicate completion:^(MSQueryResult *result, NSError *error) {
        XCTAssertTrue(result.items.count == 1, @"Incorrect count: %lu for %@", (unsigned long)result.items.count, savedItem[MSSystemColumnVersion]);
        NSDictionary *item = result.items[0];
        XCTAssertEqualObjects(item[MSSystemColumnCreatedAt], savedItem[MSSystemColumnCreatedAt], @"Incorrect property");
        XCTAssertEqualObjects(item[MSSystemColumnUpdatedAt], savedItem[MSSystemColumnUpdatedAt], @"Incorrect property");
        XCTAssertEqualObjects(item[MSSystemColumnVersion], savedItem[MSSystemColumnVersion], @"Incorrect property");
        self.done = YES;
    }];
    [self waitForTest:30.0];
    
    // Filter against createdAt
    self.done = NO;
    predicate = [NSPredicate predicateWithFormat:@"__createdAt == %@", savedItem[MSSystemColumnCreatedAt]];
    [self.table readWithPredicate:predicate completion:^(MSQueryResult *result, NSError *error) {
        XCTAssertTrue(result.items.count == 1, @"Incorrect count: %lu for %@", (unsigned long)result.items.count, savedItem[MSSystemColumnCreatedAt]);
        NSDictionary *item = result.items[0];
        XCTAssertEqualObjects(item[MSSystemColumnCreatedAt], savedItem[MSSystemColumnCreatedAt], @"Incorrect property");
        XCTAssertEqualObjects(item[MSSystemColumnUpdatedAt], savedItem[MSSystemColumnUpdatedAt], @"Incorrect property");
        XCTAssertEqualObjects(item[MSSystemColumnVersion], savedItem[MSSystemColumnVersion], @"Incorrect property");
        self.done = YES;
    }];
    [self waitForTest:30.0];

    // Filter against updatedAt
    self.done = NO;
    predicate = [NSPredicate predicateWithFormat:@"__updatedAt == %@", savedItem[MSSystemColumnUpdatedAt]];
    [self.table readWithPredicate:predicate completion:^(MSQueryResult *result, NSError *error) {
        XCTAssertTrue(result.items.count == 1, @"Incorrect count: %lu for %@", (unsigned long)result.items.count, savedItem[MSSystemColumnUpdatedAt]);
        NSDictionary *item = result.items[0];
        XCTAssertEqualObjects(item[MSSystemColumnCreatedAt], savedItem[MSSystemColumnCreatedAt], @"Incorrect property");
        XCTAssertEqualObjects(item[MSSystemColumnUpdatedAt], savedItem[MSSystemColumnUpdatedAt], @"Incorrect property");
        XCTAssertEqualObjects(item[MSSystemColumnVersion], savedItem[MSSystemColumnVersion], @"Incorrect property");
        self.done = YES;
    }];
    [self waitForTest:30.0];
    
    // Lookup
    self.done = NO;
    [self.table readWithId:savedItem[@"id"] completion:^(NSDictionary *item, NSError *error) {
        XCTAssertEqualObjects(item[@"id"],savedItem[@"id"], @"Incorrect property");
        XCTAssertEqualObjects(item[MSSystemColumnCreatedAt],savedItem[MSSystemColumnCreatedAt], @"Incorrect property");
        XCTAssertEqualObjects(item[MSSystemColumnUpdatedAt], savedItem[MSSystemColumnUpdatedAt], @"Incorrect property");
        XCTAssertEqualObjects(item[MSSystemColumnVersion],savedItem[MSSystemColumnVersion], @"Incorrect property");
        self.done = YES;
    }];
    [self waitForTest:30.0];

    self.done = NO;
    savedItem[@"string"] = @"Hello!";
    [self.table update:savedItem completion:^(NSDictionary *item, NSError *error) {
        XCTAssertEqualObjects(item[@"id"],savedItem[@"id"], @"Incorrect property");
        XCTAssertEqualObjects(item[MSSystemColumnCreatedAt],savedItem[MSSystemColumnCreatedAt], @"Incorrect property");
        NSDate *originalDate = savedItem[MSSystemColumnUpdatedAt];
        NSDate *updatedDate = item[MSSystemColumnUpdatedAt];
        XCTAssertTrue([originalDate compare:updatedDate] == NSOrderedAscending, @"Updated incorrect");
        XCTAssertFalse([item[MSSystemColumnVersion] isEqualToString:savedItem[MSSystemColumnVersion]], @"Version not updated");
        savedItem = item;
        self.done = YES;
    }];
    [self waitForTest:30.0];
    
    self.done = NO;
    [self.table readWithCompletion:^(MSQueryResult *result, NSError *error) {
        NSDictionary *item = result.items[0];
        XCTAssertEqualObjects(item[@"id"],savedItem[@"id"], @"Incorrect property");
        XCTAssertEqualObjects(item[MSSystemColumnCreatedAt],savedItem[MSSystemColumnCreatedAt], @"Incorrect property");
        XCTAssertEqualObjects(item[MSSystemColumnUpdatedAt], savedItem[MSSystemColumnUpdatedAt], @"Incorrect property");
        XCTAssertEqualObjects(item[MSSystemColumnVersion],savedItem[MSSystemColumnVersion], @"Incorrect property");
        self.done = YES;
    }];
    [self waitForTest:30.0];
}

-(void) testAsyncFilterSelectOrderingOperationsNotImpactedBySystemProperties
{
    __block NSMutableArray *savedItems = [NSMutableArray array];
    for(NSUInteger i = 1; i < 6; i++)
    {
        NSDictionary *item = @{@"id": [NSString stringWithFormat:@"%lu", (unsigned long)i], @"string": @"a value"};
        self.done = NO;
        [self.table insert:item completion:^(NSDictionary *item, NSError *error) {
            if (error) {
                XCTAssertNotNil(error);
            } else {
                [savedItems addObject:item];
            }
            
            self.done = YES;
        }];
        [self waitForTest:30.0];
    }
    
    MSQuery *query = self.table.query;
    [query orderByAscending:MSSystemColumnCreatedAt];
    self.done = NO;
    [query readWithCompletion:^(MSQueryResult *result, NSError *error) {
        for (NSUInteger i = 0; i < result.items.count - 1; i++) {
            NSInteger idOne = [result.items[i][@"id"] integerValue];
            NSInteger idTwo = [result.items[i+1][@"id"] integerValue];
            XCTAssertTrue(idOne < idTwo, @"Incorrect order");
        }
        self.done = YES;
    }];
    [self waitForTest:30.0];
    
    self.done = NO;
    query = self.table.query;
    [query orderByAscending:MSSystemColumnUpdatedAt];
    [query readWithCompletion:^(MSQueryResult *result, NSError *error) {
        for (NSUInteger i = 0; i < result.items.count - 1; i++) {
            NSInteger idOne = [result.items[i][@"id"] integerValue];
            NSInteger idTwo = [result.items[i+1][@"id"] integerValue];
            XCTAssertTrue(idOne < idTwo, @"Incorrect order");
        }
        self.done = YES;
    }];
    [self waitForTest:30.0];

    self.done = NO;
    query = self.table.query;
    [query orderByAscending:MSSystemColumnVersion];
    [query readWithCompletion:^(MSQueryResult *result, NSError *error) {
        for (NSUInteger i = 0; i < result.items.count - 1; i++) {
            NSInteger idOne = [result.items[i][@"id"] integerValue];
            NSInteger idTwo = [result.items[i+1][@"id"] integerValue];
            XCTAssertTrue(idOne < idTwo, @"Incorrect order");
        }
        self.done = YES;
    }];
    [self waitForTest:30.0];
    
    // Filtering
    self.done = NO;
    query = [self.table queryWithPredicate:[NSPredicate predicateWithFormat:@"__createdAt >= %@", savedItems[3][MSSystemColumnCreatedAt]]];
    [query readWithCompletion:^(MSQueryResult *result, NSError *error) {
        XCTAssertTrue(result.items.count == 2, @"Incorrect results");
        self.done = YES;
    }];
    [self waitForTest:30.0];

    self.done = NO;
    query = [self.table queryWithPredicate:[NSPredicate predicateWithFormat:@"__updatedAt >= %@", savedItems[3][MSSystemColumnUpdatedAt]]];
    [query readWithCompletion:^(MSQueryResult *result, NSError *error) {
        XCTAssertTrue(result.items.count == 2, @"Incorrect results");
        self.done = YES;
    }];
    [self waitForTest:30.0];
    
    self.done = NO;
    query = [self.table queryWithPredicate:[NSPredicate predicateWithFormat:@"__version == %@", savedItems[3][MSSystemColumnVersion]]];
    [query readWithCompletion:^(MSQueryResult *result, NSError *error) {
        XCTAssertTrue(result.items.count == 1, @"Incorrect results");
        self.done = YES;
    }];
    [self waitForTest:30.0];
    
    self.done = NO;
    query = self.table.query;
    query.selectFields = @[@"id", MSSystemColumnCreatedAt];
    [query readWithCompletion:^(MSQueryResult *result, NSError *error) {
        for (NSDictionary *item in result.items) {
            XCTAssertNotNil(item[MSSystemColumnCreatedAt], @"Missing createdAt");
        }
        self.done = YES;
    }];
    [self waitForTest:30.0];
    
    self.done = NO;
    query = self.table.query;
    query.selectFields = @[@"id", MSSystemColumnUpdatedAt];
    [query readWithCompletion:^(MSQueryResult *result, NSError *error) {
        for (NSDictionary *item in result.items) {
            XCTAssertNotNil(item[MSSystemColumnUpdatedAt], @"Missing updatedAt");
        }
        self.done = YES;
    }];
    [self waitForTest:30.0];
    
    self.done = NO;
    query = self.table.query;
    query.selectFields = @[@"id", MSSystemColumnVersion];
    [query readWithCompletion:^(MSQueryResult *result, NSError *error) {
        for (NSDictionary *item in result.items) {
            XCTAssertNotNil(item[MSSystemColumnVersion], @"Missing version");
        }
        self.done = YES;
    }];
    [self waitForTest:30.0];
}

-(void) testUpdateAsyncWithWithMergeConflict
{
    NSDictionary *item = @{ @"id": @"an id", @"string": @"a value" };
    __block NSDictionary *savedItem;
    [self.table insert:item completion:^(NSDictionary *item, NSError *error) {
        savedItem = item;
        self.done = YES;
    }];
    [self waitForTest:30.0];

    self.done = NO;
    __block NSDictionary *savedItem2;
    [savedItem setValue:@"Hello!" forKey:@"string"];
    [self.table update:savedItem completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(error, @"An error occcurred");
        XCTAssertFalse([item[@"__verison"] isEqualToString:savedItem[MSSystemColumnVersion]], @"Version should have changed");
        savedItem2 = item;
        self.done = YES;
    }];
    [self waitForTest:30.0];

    self.done = NO;
    [savedItem setValue:@"But Wait!" forKey:@"string"];
    [self.table update:savedItem completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNotNil(error, @"An error should have occcurred");
        XCTAssertEqual([@MSErrorPreconditionFailed integerValue], error.code, @"Should have had precondition failed error");
        
        NSHTTPURLResponse *response = (error.userInfo)[MSErrorResponseKey];
        XCTAssertNotNil(response, @"response should have been available");
        XCTAssertEqual([@412 integerValue], response.statusCode, @"response should have been pre condition failed");
        
        NSDictionary *actualItem = (error.userInfo)[MSErrorServerItemKey];
        XCTAssertEqualObjects(actualItem[MSSystemColumnVersion], savedItem2[MSSystemColumnVersion], @"Unexpected version");
        XCTAssertEqualObjects(actualItem[@"string"], @"Hello!", @"Unexpected value");
        
        self.done = YES;
    }];
    [self waitForTest:30.0];
    
    self.done = NO;
    [savedItem2 setValue:@"Hello Again!" forKey:@"string"];
    [self.table update:savedItem2 completion:^(NSDictionary *item, NSError *error) {
        XCTAssertNil(error, @"An error occcurred");
        XCTAssertFalse([item[@"__verison"] isEqualToString:savedItem2[MSSystemColumnVersion]], @"Version should have changed");
        self.done = YES;
    }];
    [self waitForTest:30.0];
}


#pragma mark Test Utilities


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
