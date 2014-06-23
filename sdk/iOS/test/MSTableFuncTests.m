// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <SenTestingKit/SenTestingKit.h>
#import "WindowsAzureMobileServices.h"
#import "MSTestFilter.h"
#import "MSTable+MSTableTestUtilities.h"
#import "MSJSONSerializer.h"

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
                        clientWithApplicationURLString:@"<Microsoft Azure Mobile Service App URL>"
                        applicationKey:@"<Application Key>"];
    
    self.table = [client tableWithName:@"stringId_objC_test_table"];
    
    STAssertNotNil(self.table, @"Could not create test table.");
    
    // Clean up table, all tests start at empty table
    [self cleanUpData];
    
    self.done = NO;
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
            STAssertEquals([@MSErrorMessageErrorCode integerValue], error.code, @"Unexpected error code: %d: %@", error.code, error.localizedDescription);
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
        [originalItems addObject:@{@"id":testId, @"string": @"Hey" }];
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
            STAssertEquals([@MSInvalidItemIdWithRequest integerValue], error.code, @"Unexpected error code: %d", error.code);
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
        countAsString = [NSString stringWithFormat:@"%lu", (unsigned long)count++];
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
            STAssertEquals([@MSErrorMessageErrorCode integerValue], error.code, @"Unexpected error code:  %d", error.code);
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
            STAssertEquals([@MSErrorMessageErrorCode integerValue], error.code, @"Unexpected error code:  %d", error.code);
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
            STAssertEquals([@MSErrorMessageErrorCode integerValue], error.code, @"Unexpected error code: %d", error.code);
            NSString *expectedErrorMessage = [NSString stringWithFormat:@"Error: An item with id '%@' does not exist.", testId];
            STAssertEqualObjects(expectedErrorMessage, error.localizedDescription, @"Wrong error: %@", error.localizedDescription);
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
            STAssertEquals([@MSErrorMessageErrorCode integerValue], error.code, @"Unexpected error code: %d", error.code);
            NSString *expectedErrorMessage = [NSString stringWithFormat:@"Error: An item with id '%@' does not exist.", testId];
            STAssertEqualObjects(expectedErrorMessage, error.localizedDescription, @"Wrong error: %@", error.localizedDescription);
            self.done = YES;
        }];
        STAssertTrue([self waitForTest:15.0], @"Test timed out.");
    }
}

# pragma mark System Properties

-(void) testAsyncTableOperationsWithAllSystemProperties
{
    NSString *myid = @"an id";
    
    NSDictionary *item = @{ @"id": myid, @"String": @"a value" };
    self.table.systemProperties = MSSystemPropertyAll;
    self.done = NO;
    [self.table insert:item completion:^(NSDictionary *item, NSError *error) {
        STAssertNotNil([item objectForKey:MSSystemColumnCreatedAt], @"Missing property");
        STAssertNotNil([item objectForKey:MSSystemColumnUpdatedAt], @"Missing property");
        STAssertNotNil([item objectForKey:MSSystemColumnVersion], @"Missing property");
        self.done = YES;
    }];
    [self waitForTest:30.0];
    
    self.done = NO;
    __block id savedItem;
    [self.table readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
        STAssertTrue(items.count == 1, @"Incorrect count: %d", items.count);
        NSDictionary *item = [items objectAtIndex:0];
        STAssertNotNil([item objectForKey:MSSystemColumnCreatedAt], @"Missing property");
        STAssertNotNil([item objectForKey:MSSystemColumnUpdatedAt], @"Missing property");
        STAssertNotNil([item objectForKey:MSSystemColumnVersion], @"Missing property");
        savedItem = item;
        self.done = YES;
    }];
    [self waitForTest:30.0];

    self.done = NO;
    NSPredicate *predicate = [NSPredicate predicateWithFormat:@"__version == %@", [savedItem objectForKey:MSSystemColumnVersion]];
    [self.table readWithPredicate:predicate completion:^(NSArray *items, NSInteger totalCount, NSError *error) {
        STAssertTrue(items.count == 1, @"Incorrect count: %d for %@", items.count, [savedItem objectForKey:MSSystemColumnVersion]);
        NSDictionary *item = [items objectAtIndex:0];
        STAssertEqualObjects([item objectForKey:MSSystemColumnCreatedAt],[savedItem objectForKey:MSSystemColumnCreatedAt], @"Incorrect property");
        STAssertEqualObjects([item objectForKey:MSSystemColumnUpdatedAt],[savedItem objectForKey:MSSystemColumnUpdatedAt], @"Incorrect property");
        STAssertEqualObjects([item objectForKey:MSSystemColumnVersion],[savedItem objectForKey:MSSystemColumnVersion], @"Incorrect property");
        self.done = YES;
    }];
    [self waitForTest:30.0];
    
    // Filter against createdAt
    self.done = NO;
    predicate = [NSPredicate predicateWithFormat:@"__createdAt == %@", [savedItem objectForKey:MSSystemColumnCreatedAt]];
    [self.table readWithPredicate:predicate completion:^(NSArray *items, NSInteger totalCount, NSError *error) {
        STAssertTrue(items.count == 1, @"Incorrect count: %d for %@", items.count, [savedItem objectForKey:MSSystemColumnCreatedAt]);
        NSDictionary *item = [items objectAtIndex:0];
        STAssertEqualObjects([item objectForKey:MSSystemColumnCreatedAt],[savedItem objectForKey:MSSystemColumnCreatedAt], @"Incorrect property");
        STAssertEqualObjects([item objectForKey:MSSystemColumnUpdatedAt],[savedItem objectForKey:MSSystemColumnUpdatedAt], @"Incorrect property");
        STAssertEqualObjects([item objectForKey:MSSystemColumnVersion],[savedItem objectForKey:MSSystemColumnVersion], @"Incorrect property");
        self.done = YES;
    }];
    [self waitForTest:30.0];

    // Filter against updatedAt
    self.done = NO;
    predicate = [NSPredicate predicateWithFormat:@"__updatedAt == %@", [savedItem objectForKey:MSSystemColumnUpdatedAt]];
    [self.table readWithPredicate:predicate completion:^(NSArray *items, NSInteger totalCount, NSError *error) {
        STAssertTrue(items.count == 1, @"Incorrect count: %d for %@", items.count, [savedItem objectForKey:MSSystemColumnUpdatedAt]);
        NSDictionary *item = [items objectAtIndex:0];
        STAssertEqualObjects([item objectForKey:MSSystemColumnCreatedAt],[savedItem objectForKey:MSSystemColumnCreatedAt], @"Incorrect property");
        STAssertEqualObjects([item objectForKey:MSSystemColumnUpdatedAt],[savedItem objectForKey:MSSystemColumnUpdatedAt], @"Incorrect property");
        STAssertEqualObjects([item objectForKey:MSSystemColumnVersion],[savedItem objectForKey:MSSystemColumnVersion], @"Incorrect property");
        self.done = YES;
    }];
    [self waitForTest:30.0];
    
    // Lookup
    self.done = NO;
    [self.table readWithId:[savedItem objectForKey:@"id"] completion:^(NSDictionary *item, NSError *error) {
        STAssertEqualObjects([item objectForKey:@"id"],[savedItem objectForKey:@"id"], @"Incorrect property");
        STAssertEqualObjects([item objectForKey:MSSystemColumnCreatedAt],[savedItem objectForKey:MSSystemColumnCreatedAt], @"Incorrect property");
        STAssertEqualObjects([item objectForKey:MSSystemColumnUpdatedAt],[savedItem objectForKey:MSSystemColumnUpdatedAt], @"Incorrect property");
        STAssertEqualObjects([item objectForKey:MSSystemColumnVersion],[savedItem objectForKey:MSSystemColumnVersion], @"Incorrect property");
        self.done = YES;
    }];
    [self waitForTest:30.0];

    self.done = NO;
    [savedItem setObject:@"Hello!" forKey:@"string"];
    [self.table update:savedItem completion:^(NSDictionary *item, NSError *error) {
        STAssertEqualObjects([item objectForKey:@"id"],[savedItem objectForKey:@"id"], @"Incorrect property");
        STAssertEqualObjects([item objectForKey:MSSystemColumnCreatedAt],[savedItem objectForKey:MSSystemColumnCreatedAt], @"Incorrect property");
        NSDate *originalDate = [savedItem objectForKey:MSSystemColumnUpdatedAt];
        NSDate *updatedDate = [item objectForKey:MSSystemColumnUpdatedAt];
        STAssertTrue([originalDate compare:updatedDate] == NSOrderedAscending, @"Updated incorrect");
        STAssertFalse([[item objectForKey:MSSystemColumnVersion] isEqualToString:[savedItem objectForKey:MSSystemColumnVersion]], @"Version not updated");
        savedItem = item;
        self.done = YES;
    }];
    [self waitForTest:30.0];
    
    self.done = NO;
    [self.table readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
        NSDictionary *item = [items objectAtIndex:0];
        STAssertEqualObjects([item objectForKey:@"id"],[savedItem objectForKey:@"id"], @"Incorrect property");
        STAssertEqualObjects([item objectForKey:MSSystemColumnCreatedAt],[savedItem objectForKey:MSSystemColumnCreatedAt], @"Incorrect property");
        STAssertEqualObjects([item objectForKey:MSSystemColumnUpdatedAt],[savedItem objectForKey:MSSystemColumnUpdatedAt], @"Incorrect property");
        STAssertEqualObjects([item objectForKey:MSSystemColumnVersion],[savedItem objectForKey:MSSystemColumnVersion], @"Incorrect property");
        self.done = YES;
    }];
    [self waitForTest:30.0];
}

-(void) testAsyncTableOperationsWithSystemPropertiesSetExplicitly
{
    NSDictionary *item = @{ @"String": @"a value" };
    
    self.table.systemProperties = MSSystemPropertyVersion | MSSystemPropertyCreatedAt | MSSystemPropertyUpdatedAt;
    [self.table insert:item completion:^(NSDictionary *item, NSError *error) {
        STAssertNotNil([item objectForKey:MSSystemColumnCreatedAt], @"Missing property");
        STAssertNotNil([item objectForKey:MSSystemColumnUpdatedAt], @"Missing property");
        STAssertNotNil([item objectForKey:MSSystemColumnVersion], @"Missing property");
        self.done = YES;
    }];
    [self waitForTest:30.0];
    
    // Explicit System Properties insert
    self.done = NO;
    self.table.systemProperties = MSSystemPropertyVersion | MSSystemPropertyCreatedAt;
    [self.table insert:item completion:^(NSDictionary *item, NSError *error) {
        STAssertNotNil([item objectForKey:MSSystemColumnCreatedAt], @"Missing property");
        STAssertNil([item objectForKey:MSSystemColumnUpdatedAt], @"Shouldn't have had updated");
        STAssertNotNil([item objectForKey:MSSystemColumnVersion], @"Missing property");
        self.done = YES;
    }];
    [self waitForTest:30.0];

    self.done = NO;
    __block NSString *savedItemId;
    self.table.systemProperties = MSSystemPropertyUpdatedAt | MSSystemPropertyCreatedAt;
    [self.table readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
        NSDictionary *item = [items objectAtIndex:0];
        STAssertNotNil([item objectForKey:MSSystemColumnCreatedAt], @"Missing property");
        STAssertNotNil([item objectForKey:MSSystemColumnUpdatedAt], @"Missing property");
        STAssertNil([item objectForKey:MSSystemColumnVersion], @"Has extra property");
        savedItemId = [item objectForKey:@"id"];
        self.done = YES;
    }];
    [self waitForTest:30.0];

    self.done = NO;
    self.table.systemProperties = MSSystemPropertyUpdatedAt;
    [self.table readWithId:savedItemId completion:^(NSDictionary *item, NSError *error) {
        STAssertNil([item objectForKey:MSSystemColumnCreatedAt], @"Has extra property");
        STAssertNotNil([item objectForKey:MSSystemColumnUpdatedAt], @"Missing property");
        STAssertNil([item objectForKey:MSSystemColumnVersion], @"Has extra property");
        self.done = YES;
    }];
    [self waitForTest:30.0];
}

-(void) testAsyncTableOperationsWithAllSystemPropertiesUsingCustomSystemParameters
{
    NSCharacterSet *equals = [NSCharacterSet characterSetWithCharactersInString:@"="];
    for (NSString *systemProperties in [MSTable testValidSystemPropertyQueryStrings])
    {
        NSArray *systemPropertiesKeyValue = [systemProperties componentsSeparatedByCharactersInSet:equals];
        NSDictionary *userParams = @{[systemPropertiesKeyValue objectAtIndex:0]: [systemPropertiesKeyValue objectAtIndex:1]};
        __block id savedItem;
        
        BOOL shouldHaveCreatedAt = [systemProperties rangeOfString:@"created" options:NSCaseInsensitiveSearch].location != NSNotFound;
        BOOL shouldHaveUpdatedAt = [systemProperties rangeOfString:@"updated" options:NSCaseInsensitiveSearch].location != NSNotFound;
        BOOL shouldHaveVersion = [systemProperties rangeOfString:@"version" options:NSCaseInsensitiveSearch].location != NSNotFound;
        if([systemProperties rangeOfString:@"*" options:NSCaseInsensitiveSearch].location != NSNotFound) {
            shouldHaveCreatedAt = shouldHaveUpdatedAt = shouldHaveVersion = YES;
        }
        
        NSString *myId = @"an id";
        NSDictionary *item = @{ @"id": myId, @"String": @"a value" };
        
        self.done = NO;
        [self.table insert:item parameters:userParams completion:^(NSDictionary *item, NSError *error) {
            STAssertEquals(shouldHaveCreatedAt, (BOOL)([item objectForKey:MSSystemColumnCreatedAt] != nil), @"Property invalid: %@", systemProperties);
            STAssertEquals(shouldHaveUpdatedAt, (BOOL)([item objectForKey:MSSystemColumnUpdatedAt] != nil), @"Property invalid: %@", systemProperties);
            STAssertEquals(shouldHaveVersion, (BOOL)([item objectForKey:MSSystemColumnVersion] != nil), @"Property invalid: %@", systemProperties);
            savedItem = item;
            self.done = YES;
        }];
        [self waitForTest:30.0];
        
        self.done = NO;
        MSQuery *query = self.table.query;
        query.parameters = userParams;
        [query readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
            STAssertTrue(items.count == 1, @"Should have had results");
            NSDictionary *item = [items objectAtIndex:0];
            STAssertEquals(shouldHaveCreatedAt, (BOOL)([item objectForKey:MSSystemColumnCreatedAt] != nil), @"Property invalid: %@", systemProperties);
            STAssertEquals(shouldHaveUpdatedAt, (BOOL)([item objectForKey:MSSystemColumnUpdatedAt] != nil), @"Property invalid: %@", systemProperties);
            STAssertEquals(shouldHaveVersion, (BOOL)([item objectForKey:MSSystemColumnVersion] != nil), @"Property invalid: %@", systemProperties);
            self.done = YES;
        }];
        [self waitForTest:30.0];
        
        self.done = NO;
        query.predicate = [NSPredicate predicateWithFormat:@"__version == %@", [savedItem objectForKey:MSSystemColumnVersion]];
        [query readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
            if (shouldHaveVersion) {
                STAssertTrue(items.count == 1, @"Should have had results");
                NSDictionary *item = [items objectAtIndex:0];
                STAssertEquals(shouldHaveCreatedAt, (BOOL)([item objectForKey:MSSystemColumnCreatedAt] != nil), @"Property invalid: %@", systemProperties);
                STAssertEquals(shouldHaveUpdatedAt, (BOOL)([item objectForKey:MSSystemColumnUpdatedAt] != nil), @"Property invalid: %@", systemProperties);
                STAssertEquals(shouldHaveVersion, (BOOL)([item objectForKey:MSSystemColumnVersion] != nil), @"Property invalid: %@", systemProperties);
            } else {
                STAssertTrue(items.count == 0, @"Shouldn't have had results");
            }
            self.done = YES;
        }];
        [self waitForTest:30.0];
        
        self.done = NO;
        query.predicate = [NSPredicate predicateWithFormat:@"__createdAt == %@", [savedItem objectForKey:MSSystemColumnCreatedAt]];
        [query readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
            if (shouldHaveCreatedAt) {
                STAssertTrue(items.count == 1, @"Should have had results");
                NSDictionary *item = [items objectAtIndex:0];
                STAssertEquals(shouldHaveCreatedAt, (BOOL)([item objectForKey:MSSystemColumnCreatedAt] != nil), @"Property invalid: %@", systemProperties);
                STAssertEquals(shouldHaveUpdatedAt, (BOOL)([item objectForKey:MSSystemColumnUpdatedAt] != nil), @"Property invalid: %@", systemProperties);
                STAssertEquals(shouldHaveVersion, (BOOL)([item objectForKey:MSSystemColumnVersion] != nil), @"Property invalid: %@", systemProperties);
            } else {
                STAssertTrue(items.count == 0, @"Shouldn't have had results");
            }
            self.done = YES;
        }];
        [self waitForTest:30.0];
        
        self.done = NO;
        query.predicate = [NSPredicate predicateWithFormat:@"__updatedAt == %@", [savedItem objectForKey:MSSystemColumnUpdatedAt]];
        [query readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
            if (shouldHaveUpdatedAt) {
                STAssertTrue(items.count == 1, @"Should have had results");
                NSDictionary *item = [items objectAtIndex:0];
                STAssertEquals(shouldHaveCreatedAt, (BOOL)([item objectForKey:MSSystemColumnCreatedAt] != nil), @"Property invalid: %@", systemProperties);
                STAssertEquals(shouldHaveUpdatedAt, (BOOL)([item objectForKey:MSSystemColumnUpdatedAt] != nil), @"Property invalid: %@", systemProperties);
                STAssertEquals(shouldHaveVersion, (BOOL)([item objectForKey:MSSystemColumnVersion] != nil), @"Property invalid: %@", systemProperties);
            } else {
                STAssertTrue(items.count == 0, @"Shouldn't have had results");
            }
            self.done = YES;
        }];
        [self waitForTest:30.0];
        
        self.done = NO;
        [self.table readWithId:myId parameters:userParams completion:^(NSDictionary *item, NSError *error) {
            STAssertEquals(shouldHaveCreatedAt, (BOOL)([item objectForKey:MSSystemColumnCreatedAt] != nil), @"Property invalid: %@", systemProperties);
            STAssertEquals(shouldHaveUpdatedAt, (BOOL)([item objectForKey:MSSystemColumnUpdatedAt] != nil), @"Property invalid: %@", systemProperties);
            STAssertEquals(shouldHaveVersion, (BOOL)([item objectForKey:MSSystemColumnVersion] != nil), @"Property invalid: %@", systemProperties);
            self.done = YES;
        }];
        [self waitForTest:30.0];
        
        self.done = NO;
        [savedItem setValue:@"Hello!" forKey:@"String"];
        [self.table update:savedItem parameters:userParams completion:^(NSDictionary *item, NSError *error) {
            STAssertEquals(shouldHaveCreatedAt, (BOOL)([item objectForKey:MSSystemColumnCreatedAt] != nil), @"Property invalid: %@", systemProperties);
            STAssertEquals(shouldHaveUpdatedAt, (BOOL)([item objectForKey:MSSystemColumnUpdatedAt] != nil), @"Property invalid: %@", systemProperties);
            STAssertEquals(shouldHaveVersion, (BOOL)([item objectForKey:MSSystemColumnVersion] != nil), @"Property invalid: %@", systemProperties);
            if (shouldHaveVersion) {
                STAssertFalse([[savedItem objectForKey:MSSystemColumnVersion] isEqualToString:[item objectForKey:MSSystemColumnVersion]], @"Invalid version");
            }
            self.done = YES;
        }];
        [self waitForTest:30.0];
        
        self.done = NO;
        [self.table delete:item completion:^(id itemId, NSError *error) {
            self.done = YES;
        }];
        [self waitForTest:30.0];
    }
}

-(void) testAsyncTableOperationsWithInvalidSystemPropertiesQuerystring
{
    NSDictionary *item = @{@"id":@"an id", @"String":@"a value"};
    
    __block NSDictionary *savedItem;
    [self.table insert:item completion:^(NSDictionary *item, NSError *error) {
        savedItem = item;
        self.done = YES;
    }];
    [self waitForTest:30.0];

    NSCharacterSet *equals = [NSCharacterSet characterSetWithCharactersInString:@"="];
    for (NSString *systemProperties in [MSTable testInvalidSystemPropertyQueryStrings])
    {
        NSArray *systemPropertiesKeyValue = [systemProperties componentsSeparatedByCharactersInSet:equals];
        NSDictionary *userParams = @{[systemPropertiesKeyValue objectAtIndex:0]: [systemPropertiesKeyValue objectAtIndex:1]};
        
        self.done = NO;
        [self.table insert:item parameters:userParams completion:^(NSDictionary *item, NSError *error) {
            STAssertNotNil(error, @"An error should have occurred");
            STAssertEquals(error.code, [@MSErrorMessageErrorCode integerValue], @"Unexpected error %d", error.code);
            STAssertTrue([error.localizedDescription rangeOfString:@"is not a supported system property."].location != NSNotFound, @"Unexpected message %@", error.localizedDescription);
            self.done = YES;
        }];
        [self waitForTest:30.0];
        
        // Read
        self.done = NO;
        MSQuery *query = [[MSQuery alloc] initWithTable:self.table];
        query.parameters = userParams;
        [query readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
            STAssertNotNil(error, @"An error should have occurred");
            STAssertEquals(error.code, [@MSErrorMessageErrorCode integerValue], @"Unexpected error %d", error.code);
            STAssertTrue([error.localizedDescription rangeOfString:@"is not a supported system property."].location != NSNotFound, @"Unexpected message %@", error.localizedDescription);
            self.done = YES;
            }];
        [self waitForTest:30.0];
        
        self.done = NO;
        NSPredicate *predicate = [NSPredicate predicateWithFormat:@"__version == %@", [savedItem objectForKey:MSSystemColumnVersion]];
        query.predicate = predicate;
        [query readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
            STAssertNotNil(error, @"An error should have occurred");
            STAssertEquals(error.code, [@MSErrorMessageErrorCode integerValue], @"Unexpected error %d", error.code);
            STAssertTrue([error.localizedDescription rangeOfString:@"is not a supported system property."].location != NSNotFound, @"Unexpected message %@", error.localizedDescription);
            self.done = YES;
        }];
        [self waitForTest:30.0];
        
        self.done = NO;
        [self.table readWithId:@"an id" parameters:userParams completion:^(NSDictionary *item, NSError *error) {
            STAssertNotNil(error, @"An error should have occurred");
            STAssertEquals(error.code, [@MSErrorMessageErrorCode integerValue], @"Unexpected error %d", error.code);
            STAssertTrue([error.localizedDescription rangeOfString:@"is not a supported system property."].location != NSNotFound, @"Unexpected message %@", error.localizedDescription);
            self.done = YES;
        }];
        [self waitForTest:30.0];
        
        self.done = NO;
        [savedItem setValue:@"Hello!" forKey:@"String"];
        [self.table update:savedItem parameters:userParams completion:^(NSDictionary *item, NSError *error) {
            STAssertNotNil(error, @"An error should have occurred");
            STAssertEquals(error.code, [@MSErrorMessageErrorCode integerValue], @"Unexpected error %d", error.code);
            STAssertTrue([error.localizedDescription rangeOfString:@"is not a supported system property."].location != NSNotFound, @"Unexpected message %@", error.localizedDescription);
            self.done = YES;
        }];
        [self waitForTest:30.0];
    }
}

-(void) testAsyncTableOperationsWithInvalidSystemParameterQueryString
{
    NSDictionary *item = @{@"id":@"an id", @"String":@"a value"};

    __block NSDictionary *savedItem;
    [self.table insert:item completion:^(NSDictionary *item, NSError *error) {
        savedItem = item;
        self.done = YES;
    }];
    [self waitForTest:30];
    
    NSCharacterSet *equals = [NSCharacterSet characterSetWithCharactersInString:@"="];
    NSArray *systemPropertiesKeyValue = [[MSTable testInvalidSystemParameterQueryString] componentsSeparatedByCharactersInSet:equals];
    NSDictionary *userParams = @{[systemPropertiesKeyValue objectAtIndex:0]: [systemPropertiesKeyValue objectAtIndex:1]};
    
    self.done = NO;
    [self.table insert:item parameters:userParams completion:^(NSDictionary *item, NSError *error) {
        STAssertNotNil(error, @"An error should have occurred");
        STAssertEquals([@MSErrorMessageErrorCode integerValue], error.code, @"Unexpected error code: %d: %@", error.code, error.localizedDescription);
        STAssertTrue([error.localizedDescription rangeOfString:@"Custom query parameter names must start with a letter."].location != NSNotFound, @"Incorrect error: %@", error.localizedDescription);
        self.done = YES;
    }];
    [self waitForTest:30.0];
    
    // Read
    self.done = NO;
    MSQuery *query = [[MSQuery alloc] initWithTable:self.table];
    query.parameters = userParams;
    [query readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
        STAssertNotNil(error, @"An error should have occurred");
        STAssertEquals([@MSErrorMessageErrorCode integerValue], error.code, @"Unexpected error code: %d: %@", error.code, error.localizedDescription);
        STAssertTrue([error.localizedDescription rangeOfString:@"Custom query parameter names must start with a letter."].location != NSNotFound, @"Incorrect error: %@", error.localizedDescription);
        self.done = YES;
    }];
    
    // Filter
    self.done = NO;
    query.predicate = [NSPredicate predicateWithFormat:@"__version == %@", [savedItem objectForKey:MSSystemColumnVersion]];
    [query readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
        STAssertNotNil(error, @"An error should have occurred");
        STAssertEquals([@MSErrorMessageErrorCode integerValue], error.code, @"Unexpected error code: %d: %@", error.code, error.localizedDescription);
        STAssertTrue([error.localizedDescription rangeOfString:@"Custom query parameter names must start with a letter."].location != NSNotFound, @"Incorrect error: %@", error.localizedDescription);
        self.done = YES;
    }];
    
    self.done = NO;
    [self.table readWithId:@"an id" parameters:userParams completion:^(NSDictionary *item, NSError *error) {
        STAssertNotNil(error, @"An error should have occurred");
        STAssertEquals([@MSErrorMessageErrorCode integerValue], error.code, @"Unexpected error code: %d: %@", error.code, error.localizedDescription);
        STAssertTrue([error.localizedDescription rangeOfString:@"Custom query parameter names must start with a letter."].location != NSNotFound, @"Incorrect error: %@", error.localizedDescription);
        self.done = YES;
    }];
    [self waitForTest:30.0];
    
    self.done = NO;
    [savedItem setValue:@"Hello!" forKey:@"String"];
    [self.table update:savedItem parameters:userParams completion:^(NSDictionary *item, NSError *error) {
        STAssertNotNil(error, @"An error should have occurred");
        STAssertEquals([@MSErrorMessageErrorCode integerValue], error.code, @"Unexpected error code: %d: %@", error.code, error.localizedDescription);
        STAssertTrue([error.localizedDescription rangeOfString:@"Custom query parameter names must start with a letter."].location != NSNotFound, @"Incorrect error: %@", error.localizedDescription);
        self.done = YES;
    }];
    [self waitForTest:30.0];
}

-(void) testAsyncFilterSelectOrderingOperationsNotImpactedBySystemProperties
{
    self.table.systemProperties = MSSystemPropertyAll;
    
    __block NSMutableArray *savedItems = [NSMutableArray array];
    for(NSUInteger i = 1; i < 6; i++)
    {
        NSDictionary *item = @{@"id": [NSString stringWithFormat:@"%lu", (unsigned long)i], @"String": @"a value"};
        self.done = NO;
        [self.table insert:item completion:^(NSDictionary *item, NSError *error) {
            [savedItems addObject:item];
            self.done = YES;
        }];
        [self waitForTest:30.0];
    }
    
    //testSystemProperties
    for (NSNumber *systemProperties in [MSTable testSystemProperties])
    {
        self.table.systemProperties = [systemProperties unsignedIntegerValue];
        MSQuery *query = self.table.query;
        [query orderByAscending:MSSystemColumnCreatedAt];
        self.done = NO;
        [query readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
            for (NSUInteger i = 0; i < items.count - 1; i++) {
                NSInteger idOne = [[[items objectAtIndex:i] objectForKey:@"id"] integerValue];
                NSInteger idTwo = [[[items objectAtIndex:i+1] objectForKey:@"id"] integerValue];
                STAssertTrue(idOne < idTwo, @"Incorrect order");
            }
            self.done = YES;
        }];
        [self waitForTest:30.0];
        
        self.done = NO;
        query = self.table.query;
        [query orderByAscending:MSSystemColumnUpdatedAt];
        [query readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
            for (NSUInteger i = 0; i < items.count - 1; i++) {
                NSInteger idOne = [[[items objectAtIndex:i] objectForKey:@"id"] integerValue];
                NSInteger idTwo = [[[items objectAtIndex:i+1] objectForKey:@"id"] integerValue];
                STAssertTrue(idOne < idTwo, @"Incorrect order");
            }
            self.done = YES;
        }];
        [self waitForTest:30.0];

        self.done = NO;
        query = self.table.query;
        [query orderByAscending:MSSystemColumnVersion];
        [query readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
            for (NSUInteger i = 0; i < items.count - 1; i++) {
                NSInteger idOne = [[[items objectAtIndex:i] objectForKey:@"id"] integerValue];
                NSInteger idTwo = [[[items objectAtIndex:i+1] objectForKey:@"id"] integerValue];
                STAssertTrue(idOne < idTwo, @"Incorrect order");
            }
            self.done = YES;
        }];
        [self waitForTest:30.0];
        
        // Filtering
        self.done = NO;
        query = [self.table queryWithPredicate:[NSPredicate predicateWithFormat:@"__createdAt >= %@", [[savedItems objectAtIndex:3] objectForKey:MSSystemColumnCreatedAt]]];
        [query readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
            STAssertTrue(items.count == 2, @"Incorrect results");
            self.done = YES;
        }];
        [self waitForTest:30.0];

        self.done = NO;
        query = [self.table queryWithPredicate:[NSPredicate predicateWithFormat:@"__updatedAt >= %@", [[savedItems objectAtIndex:3] objectForKey:MSSystemColumnUpdatedAt]]];
        [query readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
            STAssertTrue(items.count == 2, @"Incorrect results");
            self.done = YES;
        }];
        [self waitForTest:30.0];
        
        self.done = NO;
        query = [self.table queryWithPredicate:[NSPredicate predicateWithFormat:@"__version == %@", [[savedItems objectAtIndex:3] objectForKey:MSSystemColumnVersion]]];
        [query readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
            STAssertTrue(items.count == 1, @"Incorrect results");
            self.done = YES;
        }];
        [self waitForTest:30.0];
        
        self.done = NO;
        query = self.table.query;
        query.selectFields = @[@"id", MSSystemColumnCreatedAt];
        [query readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
            for (NSDictionary *item in items) {
                STAssertNotNil([item objectForKey:MSSystemColumnCreatedAt], @"Missing createdAt");
            }
            self.done = YES;
        }];
        [self waitForTest:30.0];
        
        self.done = NO;
        query = self.table.query;
        query.selectFields = @[@"id", MSSystemColumnUpdatedAt];
        [query readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
            for (NSDictionary *item in items) {
                STAssertNotNil([item objectForKey:MSSystemColumnUpdatedAt], @"Missing updatedAt");
            }
            self.done = YES;
        }];
        [self waitForTest:30.0];
        
        self.done = NO;
        query = self.table.query;
        query.selectFields = @[@"id", MSSystemColumnVersion];
        [query readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
            for (NSDictionary *item in items) {
                STAssertNotNil([item objectForKey:MSSystemColumnVersion], @"Missing version");
            }
            self.done = YES;
        }];
        [self waitForTest:30.0];
    }
}

-(void) testUpdateAsyncWithWithMergeConflict
{
    NSDictionary *item = @{ @"id": @"an id", @"String": @"a value" };
    self.table.systemProperties = MSSystemPropertyAll;
    __block NSDictionary *savedItem;
    [self.table insert:item completion:^(NSDictionary *item, NSError *error) {
        savedItem = item;
        self.done = YES;
    }];
    [self waitForTest:30.0];

    self.done = NO;
    __block NSDictionary *savedItem2;
    [savedItem setValue:@"Hello!" forKey:@"String"];
    [self.table update:savedItem completion:^(NSDictionary *item, NSError *error) {
        STAssertNil(error, @"An error occcurred");
        STAssertFalse([[item objectForKey:@"__verison"] isEqualToString:[savedItem objectForKey:MSSystemColumnVersion]], @"Version should have changed");
        savedItem2 = item;
        self.done = YES;
    }];
    [self waitForTest:30.0];

    self.done = NO;
    [savedItem setValue:@"But Wait!" forKey:@"String"];
    [self.table update:savedItem completion:^(NSDictionary *item, NSError *error) {
        STAssertNotNil(error, @"An error should have occcurred");
        STAssertEquals([@MSErrorPreconditionFailed integerValue], error.code, @"Should have had precondition failed error");
        
        //NSDictionary *itemResponse = error.localizedDescription;
        NSHTTPURLResponse *response = [error.userInfo objectForKey:MSErrorResponseKey];
        STAssertNotNil(response, @"response should have been available");
        STAssertEquals([@412 integerValue], response.statusCode, @"response should have been pre condition failed");
        
        NSDictionary *actualItem = [error.userInfo objectForKey:MSErrorServerItemKey];
        STAssertEqualObjects([actualItem objectForKey:MSSystemColumnVersion], [savedItem2 objectForKey:MSSystemColumnVersion], @"Unexpected version");
        STAssertEqualObjects([actualItem objectForKey:@"string"], @"Hello!", @"Unexpected value");
        
        self.done = YES;
    }];
    [self waitForTest:30.0];
    
    self.done = NO;
    [savedItem2 setValue:@"Hello Again!" forKey:@"String"];
    [self.table update:savedItem2 completion:^(NSDictionary *item, NSError *error) {
        STAssertNil(error, @"An error occcurred");
        STAssertFalse([[item objectForKey:@"__verison"] isEqualToString:[savedItem2 objectForKey:MSSystemColumnVersion]], @"Version should have changed");
        self.done = YES;
    }];
    [self waitForTest:30.0];
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
