// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------


#import <XCTest/XCTest.h>
#import "MSCoreDataStore.h"
#import "MSCoreDataStore+TestHelper.h"
#import "MSJSONSerializer.h"
#import "TodoItem.h"

@interface MSCoreDataStoreTests : XCTestCase {
    BOOL done;
}
@property (nonatomic, strong) MSCoreDataStore *store;
@property (nonatomic, strong) NSManagedObjectContext *context;

@end

@implementation MSCoreDataStoreTests

- (void)setUp
{
    NSLog(@"%@ setUp", self.name);
    
    self.context = [MSCoreDataStore inMemoryManagedObjectContext];
    self.store = [[MSCoreDataStore alloc] initWithManagedObjectContext:self.context];
    XCTAssertNotNil(self.store, @"In memory store could not be created");
    
    done = NO;
}

-(void)tearDown
{
    self.store = nil;
    NSLog(@"%@ tearDown", self.name);

    [super tearDown];
}

-(void)testInit
{
    XCTAssertNotNil(self.store, @"store creation failed");
}

-(void)testUpsertSingleRecordAndReadSuccess
{
    NSError *error;
    NSArray *testArray = @[@{@"id":@"ABC", @"text": @"test1", @"__version":@"APPLE"}];
    
    [self.store upsertItems:testArray table:@"TodoItem" orError:&error];
    XCTAssertNil(error, @"upsert failed: %@", error.description);
    
    NSDictionary *item = [self.store readTable:@"TodoItem" withItemId:@"ABC" orError:&error];
    XCTAssertNil(error, @"readTable:withItemId: failed: %@", error.description);
    XCTAssertNotNil(item, @"item should not have been nil");
    XCTAssertTrue([item[@"id"] isEqualToString:@"ABC"], @"Incorrect item id");
    XCTAssertNotNil(item[MSSystemColumnVersion], @"__version was missing");
    XCTAssertNil(item[@"ms_version"], @"__version was missing");
    
    MSSyncTable *todoItem = [[MSSyncTable alloc] initWithName:@"TodoItem" client:nil];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoItem predicate:nil];
    
    MSSyncContextReadResult *result = [self.store readWithQuery:query orError:&error];
    XCTAssertNil(error, @"readWithQuery: failed: %@", error.description);
    XCTAssertEqual(result.items.count, 1);
    
    item = (result.items)[0];
    XCTAssertNotNil(item);
    XCTAssertEqualObjects(item[@"id"], @"ABC");
    XCTAssertNotNil(item[MSSystemColumnVersion]);
    XCTAssertNil(item[@"ms_version"]);
}

-(void)testUpsertMultipleRecordsAndReadSuccess
{
    NSError *error;
    NSArray *testArray = @[@{@"id":@"A", @"text": @"test1"},
                            @{@"id":@"B", @"text": @"test2"},
                            @{@"id":@"C", @"text": @"test3"}];
    
    [self.store upsertItems:testArray table:@"TodoItem" orError:&error];
    XCTAssertNil(error, @"upsert failed: %@", error.description);
    
    NSDictionary *item = [self.store readTable:@"TodoItem" withItemId:@"B" orError:&error];
    XCTAssertNil(error, @"readTable:withItemId: failed: %@", error.description);
    XCTAssertNotNil(item);
    XCTAssertEqualObjects(item[@"id"], @"B");
    XCTAssertEqualObjects(item[@"text"], @"test2");
    
    MSSyncTable *todoItem = [[MSSyncTable alloc] initWithName:@"TodoItem" client:nil];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoItem predicate:nil];
    
    MSSyncContextReadResult *result = [self.store readWithQuery:query orError:&error];
    XCTAssertNil(error, @"readWithQuery: failed: %@", error.description);
    XCTAssertEqual(result.items.count, 3);
}

-(void)testUpsertWithoutVersionAndReadSuccess
{
    NSError *error;
    NSArray *testArray = @[@{@"id":@"A", @"text": @"test1"}];
    
    [self.store upsertItems:testArray table:@"TodoNoVersion" orError:&error];
    XCTAssertNil(error, @"upsert failed: %@", error.description);
    
    NSDictionary *item = [self.store readTable:@"TodoNoVersion" withItemId:@"A" orError:&error];
    XCTAssertNil(error, @"readTable:withItemId: failed: %@", error.description);
    XCTAssertNotNil(item, @"item should not have been nil");
    XCTAssertTrue([item[@"id"] isEqualToString:@"A"], @"Incorrect item id");
    XCTAssertNil(item[MSSystemColumnVersion]);
    XCTAssertNil(item[@"ms_version"]);
    
    MSSyncTable *todoItem = [[MSSyncTable alloc] initWithName:@"TodoNoVersion" client:nil];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoItem predicate:nil];
    
    MSSyncContextReadResult *result = [self.store readWithQuery:query orError:&error];
    XCTAssertNil(error, @"readWithQuery: failed: %@", error.description);
    XCTAssertNotNil(result);
    XCTAssertEqual(result.items.count, 1);
    
    item = (result.items)[0];
    XCTAssertNotNil(item);
    XCTAssertEqualObjects(item[@"id"], @"A");
    XCTAssertNil(item[MSSystemColumnVersion]);
    XCTAssertNil(item[@"ms_version"]);
}

-(void)testUpsertSystemColumnsConvert_Success
{
    NSError *error;
    
    NSDate *now = [NSDate date];
    MSJSONSerializer *serializer = [MSJSONSerializer JSONSerializer];
    NSData *rawDate = [@"\"2014-05-27T20:37:33.055Z\"" dataUsingEncoding:NSUTF8StringEncoding];
    NSDate *testDate = [serializer itemFromData:rawDate withOriginalItem:nil ensureDictionary:NO orError:&error];
    
    NSDictionary *originalItem = @{
                               MSSystemColumnId:@"AmazingRecord1",
                               @"text": @"test1",
                               MSSystemColumnVersion: @"AAAAAAAAjlg=",
                               MSSystemColumnCreatedAt: testDate,
                               MSSystemColumnUpdatedAt: now,
                               @"__meaningOfLife": @42,
                               MSSystemColumnDeleted : @NO
                           };
    
    [self.store upsertItems:@[originalItem] table:@"ManySystemColumns" orError:&error];
    XCTAssertNil(error, @"upsert failed: %@", error.description);
    
    // Test read with id

    NSDictionary *item = [self.store readTable:@"ManySystemColumns" withItemId:@"AmazingRecord1" orError:&error];
    XCTAssertNil(error, @"readTable:withItemId: failed: %@", error.description);
    
    XCTAssertNotNil(item, @"item should not have been nil");
    XCTAssertTrue([item[MSSystemColumnId] isEqualToString:@"AmazingRecord1"], @"Incorrect item id");
    XCTAssertTrue([item[MSSystemColumnVersion] isEqualToString:originalItem[MSSystemColumnVersion]], @"Incorrect version");
    XCTAssertEqualObjects(item[MSSystemColumnUpdatedAt], originalItem[MSSystemColumnUpdatedAt], @"Incorrect updated at");
    XCTAssertEqualObjects(item[MSSystemColumnCreatedAt], originalItem[MSSystemColumnCreatedAt], @"Incorrect created at");
    XCTAssertEqualObjects(item[MSSystemColumnDeleted], originalItem[MSSystemColumnDeleted], @"Incorrect deleted");
    XCTAssertEqualObjects(item[@"__meaningOfLife"], originalItem[@"__meaningOfLife"], @"Incorrect meaning of life");
    
    NSSet *msKeys = [item keysOfEntriesPassingTest:^BOOL(id key, id obj, BOOL *stop) {
        *stop = [(NSString *)key hasPrefix:@"ms_"];
        return *stop;
    }];
    XCTAssertTrue(msKeys.count == 0, @"ms_ column keys were exposed");
    
    // Repeat for query
    
    MSSyncTable *manySystemColumns = [[MSSyncTable alloc] initWithName:@"ManySystemColumns" client:nil];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:manySystemColumns predicate:nil];
    
    MSSyncContextReadResult *result = [self.store readWithQuery:query orError:&error];
    XCTAssertNil(error, @"readWithQuery: failed: %@", error.description);
    XCTAssertNotNil(result);
    XCTAssertEqual(result.items.count, 1);
    
    XCTAssertNotNil(item, @"item should not have been nil");
    XCTAssertEqualObjects(item[MSSystemColumnId], @"AmazingRecord1");
    XCTAssertEqualObjects(item[MSSystemColumnVersion], originalItem[MSSystemColumnVersion]);
    XCTAssertEqualObjects(item[MSSystemColumnUpdatedAt], originalItem[MSSystemColumnUpdatedAt]);
    XCTAssertEqualObjects(item[MSSystemColumnCreatedAt], originalItem[MSSystemColumnCreatedAt]);
    XCTAssertEqualObjects(item[MSSystemColumnDeleted], originalItem[MSSystemColumnDeleted]);
    XCTAssertEqualObjects(item[@"__meaningOfLife"], originalItem[@"__meaningOfLife"]);
    
    msKeys = [item keysOfEntriesPassingTest:^BOOL(id key, id obj, BOOL *stop) {
        *stop = [(NSString *)key hasPrefix:@"ms_"];
        return *stop;
    }];
    XCTAssertTrue(msKeys.count == 0, @"ms_ column keys were exposed");
}

-(void)testUpsertNoTableError
{
    NSError *error;
    NSArray *testArray = @[@{@"id":@"A", @"text": @"test1"}];
    
    [self.store upsertItems:testArray table:@"NoSuchTable" orError:&error];

    XCTAssertNotNil(error, @"upsert failed: %@", error.description);
    XCTAssertEqual(error.code, MSSyncTableLocalStoreError);
}

-(void)testReadWithQuery
{
    NSError *error;
    
    [self populateTestData];
    
    MSSyncTable *todoItem = [[MSSyncTable alloc] initWithName:@"TodoItem" client:nil];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoItem predicate:nil];
    query.predicate = [NSPredicate predicateWithFormat:@"text == 'test3'"];
    
    MSSyncContextReadResult *result = [self.store readWithQuery:query orError:&error];
    XCTAssertNil(error, @"readWithQuery: failed: %@", error.description);
    XCTAssertNotNil(result);
    XCTAssertEqual(result.items.count, 1);
    XCTAssertEqual(result.items[0][@"id"], @"C");
}

-(void)testReadWithQuery_Take1_IncludeTotalCount
{
    NSError *error;
    
    [self populateTestData];
    
    MSSyncTable *todoItem = [[MSSyncTable alloc] initWithName:@"TodoItem" client:nil];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoItem predicate:nil];
    query.fetchLimit = 1;
    query.includeTotalCount = YES;
    
    MSSyncContextReadResult *result = [self.store readWithQuery:query orError:&error];
    XCTAssertNil(error, @"readWithQuery: failed: %@", error.description);
    XCTAssertNotNil(result);
    XCTAssertEqual(result.totalCount, 3);
    XCTAssertEqual(result.items.count, 1);
}

-(void)testReadWithQuery_SortAscending
{
    NSError *error;
    
    [self populateTestData];
    
    MSSyncTable *todoItem = [[MSSyncTable alloc] initWithName:@"TodoItem" client:nil];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoItem predicate:nil];
    [query orderByAscending:@"sort"];
    
    MSSyncContextReadResult *result = [self.store readWithQuery:query orError:&error];
    XCTAssertNil(error, @"readWithQuery: failed: %@", error.description);
    XCTAssertNotNil(result);
    XCTAssertEqual(result.items.count, 3);
    
    NSDictionary *item = (result.items)[0];
    XCTAssertTrue([item[@"id"] isEqualToString:@"C"], @"sort incorrect");
}

-(void)testReadWithQuery_SortDescending
{
    NSError *error;
    
    [self populateTestData];
    
    MSSyncTable *todoItem = [[MSSyncTable alloc] initWithName:@"TodoItem" client:nil];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoItem predicate:nil];
    [query orderByDescending:@"sort"];
    
    MSSyncContextReadResult *result = [self.store readWithQuery:query orError:&error];
    XCTAssertNil(error, @"readWithQuery: failed: %@", error.description);
    XCTAssertNotNil(result, @"result should not have been nil");
    XCTAssertEqual(result.items.count, 3);
    
    NSDictionary *item = result.items[0];
    XCTAssertEqualObjects(item[@"id"], @"B", @"Incorrect sort order");
}

-(void)testReadWithQuery_Select
{
    NSError *error;
    
    [self populateTestData];
    
    MSSyncTable *todoItem = [[MSSyncTable alloc] initWithName:@"TodoItem" client:nil];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoItem predicate:nil];
    query.selectFields = @[@"sort", @"text"];
    
    MSSyncContextReadResult *result = [self.store readWithQuery:query orError:&error];
    XCTAssertNil(error, @"readWithQuery: failed: %@", error.description);
    XCTAssertNotNil(result);
    XCTAssertEqual(result.items.count, 3);
    
    NSDictionary *item = (result.items)[0];
    XCTAssertNil(item[@"id"], @"Unexpected id: %@", item[@"id"]);
    XCTAssertNotNil(item[@"sort"]);
    XCTAssertNotNil(item[@"text"]);
    
    // NOTE: to not break oc, you get version regardless
    XCTAssertNotNil(item[@"__version"]);
}

-(void)testReadWithQuery_Select_SystemColumns
{
    NSError *error;
    
    NSArray *testData = @[
      @{ MSSystemColumnId:@"A", @"text": @"t1", MSSystemColumnVersion: @"AAAAAAAAjlg=", @"__meaningOfLife": @42},
      @{ MSSystemColumnId:@"B", @"text": @"t2", MSSystemColumnVersion: @"AAAAAAAAjlh=", @"__meaningOfLife": @43},
      @{ MSSystemColumnId:@"C", @"text": @"t3", MSSystemColumnVersion: @"AAAAAAAAjli=", @"__meaningOfLife": @44}
    ];
    
    [self.store upsertItems:testData table:@"ManySystemColumns" orError:&error];
    XCTAssertNil(error, @"Upsert failed: %@", error.description);
    
    // Now check selecting subset of columns
    MSSyncTable *todoItem = [[MSSyncTable alloc] initWithName:@"ManySystemColumns" client:nil];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoItem predicate:nil];
    query.selectFields = @[@"text", @"__version", @"__meaningOfLife"];
    
    MSSyncContextReadResult *result = [self.store readWithQuery:query orError:&error];
    XCTAssertNil(error, @"readWithQuery: failed: %@", error.description);
    XCTAssertNotNil(result, @"result should not have been nil");
    XCTAssertEqual(result.items.count, 3);
    
    NSDictionary *item = (result.items)[0];
    XCTAssertNotNil(item[@"text"]);
    XCTAssertNotNil(item[@"__meaningOfLife"]);
    XCTAssertNotNil(item[MSSystemColumnVersion]);
    XCTAssertEqual(item.count, 3, @"Select returned extra columns");
}

-(void)testReadWithQuery_NoTable_Error
{
    NSError *error;

    MSSyncTable *todoItem = [[MSSyncTable alloc] initWithName:@"NoSuchTable" client:nil];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoItem predicate:nil];
    
    MSSyncContextReadResult *result = [self.store readWithQuery:query orError:&error];

    XCTAssertNil(result);
    XCTAssertNotNil(error);
    XCTAssertEqual(error.code, MSSyncTableLocalStoreError);
}

-(void)testDeleteWithId_Success
{
    NSError *error;
    
    [self populateTestData];
    
    [self.store deleteItemsWithIds:@[@"B"] table:@"TodoItem" orError:&error];
    XCTAssertNil(error, @"deleteItemsWithIds: failed: %@", error.description);

    NSDictionary *item = [self.store readTable:@"TodoItem" withItemId:@"B" orError:&error];
    XCTAssertNil(error, @"readTable:withItemId: failed: %@", error.description);
    XCTAssertNil(item, @"item should have been nil");
}

-(void)testDeleteWithId_MultipleRecords_Success
{
    NSError *error;
    
    [self populateTestData];
    
    [self.store deleteItemsWithIds:@[@"A", @"C"] table:@"TodoItem" orError:&error];
    XCTAssertNil(error, @"deleteItemsWithIds: failed: %@", error.description);
    
    NSDictionary *item = [self.store readTable:@"TodoItem" withItemId:@"A" orError:&error];
    XCTAssertNil(error, @"readTable:withItemId: failed: %@", error.description);
    XCTAssertNil(item, @"item should have been nil");

    item = [self.store readTable:@"TodoItem" withItemId:@"B" orError:&error];
    XCTAssertNil(error, @"readTable:withItemId: failed: %@", error.description);
    XCTAssertNotNil(item, @"item should not have been nil");
}

-(void)testDeleteWithId_NoRecord_Success
{
    NSError *error;
    
    [self.store deleteItemsWithIds:@[@"B"] table:@"TodoNoVersion" orError:&error];
    XCTAssertNil(error, @"deleteItemsWithIds: failed: %@", error.description);
}

-(void)testDeleteWithIds_NoTable_Error
{
    NSError *error;
    
    [self.store deleteItemsWithIds:@[@"B"] table:@"NoSuchTable" orError:&error];
    
    XCTAssertNotNil(error, @"upsert failed: %@", error.description);
    XCTAssertEqual(error.code, MSSyncTableLocalStoreError);
}

- (void)testDeleteWithQuery_AllRecord_Success
{
    NSError *error;
    
    [self populateTestData];

    MSSyncTable *todoItem = [[MSSyncTable alloc] initWithName:@"TodoItem" client:nil];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoItem predicate:nil];
    
    [self.store deleteUsingQuery:query orError:&error];
    XCTAssertNil(error, @"deleteItemsWithIds: failed: %@", error.description);

    MSSyncContextReadResult *result = [self.store readWithQuery:query orError:&error];
    XCTAssertNil(error, @"readWithQuery: failed: %@", error.description);
    XCTAssertNotNil(result);
    XCTAssertEqual(result.items.count, 0);
}

- (void)testDeleteWithQuery_Predicate_Success
{
    NSError *error;
    
    [self populateTestData];
    
    MSSyncTable *todoItem = [[MSSyncTable alloc] initWithName:@"TodoItem" client:nil];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoItem predicate:nil];
    query.predicate = [NSPredicate predicateWithFormat:@"text == 'test3'"];
    
    [self.store deleteUsingQuery:query orError:&error];
    XCTAssertNil(error, @"deleteItemsWithIds: failed: %@", error.description);
    
    query.predicate = nil;
    MSSyncContextReadResult *result = [self.store readWithQuery:query orError:&error];
    XCTAssertNil(error, @"readWithQuery: failed: %@", error.description);
    XCTAssertNotNil(result, @"result should not have been nil");
    XCTAssertEqual(result.items.count, 2);
    XCTAssertFalse([result.items[0][@"id"] isEqualToString:@"C"], @"Record C should have been deleted");
    XCTAssertFalse([result.items[1][@"id"] isEqualToString:@"C"], @"Record C should have been deleted");
}

-(void)testDeleteWithQuery_NoTable_Error
{
    NSError *error;
    
    MSSyncTable *todoItem = [[MSSyncTable alloc] initWithName:@"NoSuchTable" client:nil];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoItem predicate:nil];
    query.predicate = [NSPredicate predicateWithFormat:@"text == 'test3'"];
    
    [self.store deleteUsingQuery:query orError:&error];

    XCTAssertNotNil(error, @"upsert failed: %@", error.description);
    XCTAssertEqual(error.code, MSSyncTableLocalStoreError);
}

-(void)testSystemProperties
{
    MSSystemProperties properties = [self.store systemPropertiesForTable:@"ManySystemColumns"];
    XCTAssertEqual(properties, MSSystemPropertyCreatedAt | MSSystemPropertyUpdatedAt | MSSystemPropertyVersion | MSSystemPropertyDeleted);

    properties = [self.store systemPropertiesForTable:@"TodoItem"];
    XCTAssertEqual(properties, MSSystemPropertyVersion);

    properties = [self.store systemPropertiesForTable:@"TodoItemNoVersion"];
    XCTAssertEqual(properties, MSSystemPropertyNone);
}

-(void)testObjectConversion
{
    [self populateTestData];
    
    NSFetchRequest *request = [NSFetchRequest fetchRequestWithEntityName:@"TodoItem"];
    request.predicate = [NSPredicate predicateWithFormat:@"id == %@", @"A"];
    NSArray *results = [self.context executeFetchRequest:request error:nil];
    
    TodoItem *toDoItemObject = results[0];
    // Confirm we are using an internal version column
    XCTAssertEqualObjects(toDoItemObject.ms_version, @"APPLE");
    
    NSDictionary *todoItemDictionary = [MSCoreDataStore tableItemFromManagedObject:toDoItemObject];

    XCTAssertNotNil(todoItemDictionary);
    XCTAssertEqual(todoItemDictionary.count, 4);
    XCTAssertEqualObjects(todoItemDictionary[MSSystemColumnId], @"A");
    // Confirm version was remapped
    XCTAssertNil(todoItemDictionary[@"ms_version"]);
    XCTAssertEqualObjects(todoItemDictionary[MSSystemColumnVersion], @"APPLE");
    XCTAssertEqualObjects(todoItemDictionary[@"text"], @"test1");
    
    XCTAssertEqualObjects(todoItemDictionary[@"sort"], @10);
}

- (void) populateTestData
{
    NSError *error;
    NSArray *testArray = @[@{@"id":@"A", @"text": @"test1", @"sort":@10, @"__version":@"APPLE"},
                          @{@"id":@"B", @"text": @"test2", @"sort":@15, @"__version":@"APPLE"},
                          @{@"id":@"C", @"text": @"test3", @"sort":@5, @"__version":@"APPLE"}];
    
    [self.store upsertItems:testArray table:@"TodoItem" orError:&error];
    XCTAssertNil(error, @"upsert failed: %@", error.description);
}

@end
