// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------


#import <SenTestingKit/SenTestingKit.h>
#import "MSCoreDataStore.h"
#import "MSCoreDataStore+TestHelper.h"
#import "MSJSONSerializer.h"

@interface MSCoreDataStoreTests : SenTestCase {
    BOOL done;
}
@property (nonatomic, strong) MSCoreDataStore *store;
@end

@implementation MSCoreDataStoreTests

- (void)setUp
{
    NSLog(@"%@ setUp", self.name);
    
    self.store = [[MSCoreDataStore alloc] initWithManagedObjectContext:[MSCoreDataStore inMemoryManagedObjectContext]];
    STAssertNotNil(self.store, @"In memory store could not be created");
    
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
    STAssertNotNil(self.store, @"store creation failed");
}

-(void)testUpsertSingleRecordAndReadSuccess
{
    NSError *error;
    NSArray *testArray = [NSArray arrayWithObject:@{@"id":@"ABC", @"text": @"test1", @"__version":@"APPLE"}];
    
    [self.store upsertItems:testArray table:@"TodoItem" orError:&error];
    STAssertNil(error, @"upsert failed: %@", error.description);
    
    NSDictionary *item = [self.store readTable:@"TodoItem" withItemId:@"ABC" orError:&error];
    STAssertNil(error, @"readTable:withItemId: failed: %@", error.description);
    STAssertNotNil(item, @"item should not have been nil");
    STAssertTrue([[item objectForKey:@"id"] isEqualToString:@"ABC"], @"Incorrect item id");
    STAssertNotNil([item objectForKey:MSSystemColumnVersion], @"__version was missing");
    STAssertNil([item objectForKey:@"ms_version"], @"__version was missing");
    
    MSSyncTable *todoItem = [[MSSyncTable alloc] initWithName:@"TodoItem" client:nil];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoItem predicate:nil];
    
    MSSyncContextReadResult *result = [self.store readWithQuery:query orError:&error];
    STAssertNil(error, @"readWithQuery: failed: %@", error.description);
    STAssertNotNil(result, @"item should not have been nil");
    STAssertTrue(result.items.count == 1, @"Unexpected result count %d", result.items.count);
        
    item = [result.items objectAtIndex:0];
    STAssertNotNil(item, @"item should not have been nil");
    STAssertTrue([[item objectForKey:@"id"] isEqualToString:@"ABC"], @"Incorrect item id");
    STAssertNotNil([item objectForKey:MSSystemColumnVersion], @"__version was missing");
    STAssertNil([item objectForKey:@"ms_version"], @"__version was missing");
}

-(void)testUpsertMultipleRecordsAndReadSuccess
{
    NSError *error;
    NSArray *testArray = [NSArray arrayWithObjects:
                            @{@"id":@"A", @"text": @"test1"},
                            @{@"id":@"B", @"text": @"test2"},
                            @{@"id":@"C", @"text": @"test3"},
                            nil];
    
    [self.store upsertItems:testArray table:@"TodoItem" orError:&error];
    STAssertNil(error, @"upsert failed: %@", error.description);
    
    NSDictionary *item = [self.store readTable:@"TodoItem" withItemId:@"B" orError:&error];
    STAssertNil(error, @"readTable:withItemId: failed: %@", error.description);
    STAssertNotNil(item, @"item should not have been nil");
    STAssertTrue([[item objectForKey:@"id"] isEqualToString:@"B"], @"Incorrect item id");
    STAssertTrue([[item objectForKey:@"text"] isEqualToString:@"test2"], @"Incorrect item id");
    
    MSSyncTable *todoItem = [[MSSyncTable alloc] initWithName:@"TodoItem" client:nil];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoItem predicate:nil];
    
    MSSyncContextReadResult *result = [self.store readWithQuery:query orError:&error];
    STAssertNil(error, @"readWithQuery: failed: %@", error.description);
    STAssertNotNil(result, @"item should not have been nil");
    STAssertTrue(result.items.count == 3, @"Unexpected result count %d", result.items.count);
}

-(void)testUpsertWithoutVersionAndReadSuccess
{
    NSError *error;
    NSArray *testArray = [NSArray arrayWithObject:@{@"id":@"A", @"text": @"test1"}];
    
    [self.store upsertItems:testArray table:@"TodoNoVersion" orError:&error];
    STAssertNil(error, @"upsert failed: %@", error.description);
    
    NSDictionary *item = [self.store readTable:@"TodoNoVersion" withItemId:@"A" orError:&error];
    STAssertNil(error, @"readTable:withItemId: failed: %@", error.description);
    STAssertNotNil(item, @"item should not have been nil");
    STAssertTrue([[item objectForKey:@"id"] isEqualToString:@"A"], @"Incorrect item id");
    STAssertNil([item objectForKey:MSSystemColumnVersion], @"__version was missing");
    STAssertNil([item objectForKey:@"ms_version"], @"__version was missing");
    
    MSSyncTable *todoItem = [[MSSyncTable alloc] initWithName:@"TodoNoVersion" client:nil];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoItem predicate:nil];
    
    MSSyncContextReadResult *result = [self.store readWithQuery:query orError:&error];
    STAssertNil(error, @"readWithQuery: failed: %@", error.description);
    STAssertNotNil(result, @"result should not have been nil");
    STAssertTrue(result.items.count == 1, @"Unexpected result count %d", result.items.count);
    
    item = [result.items objectAtIndex:0];
    STAssertNotNil(item, @"item should not have been nil");
    STAssertTrue([[item objectForKey:@"id"] isEqualToString:@"A"], @"Incorrect item id");
    STAssertNil([item objectForKey:MSSystemColumnVersion], @"__version was missing");
    STAssertNil([item objectForKey:@"ms_version"], @"__version was missing");
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
    STAssertNil(error, @"upsert failed: %@", error.description);
    
    // Test read with id

    NSDictionary *item = [self.store readTable:@"ManySystemColumns" withItemId:@"AmazingRecord1" orError:&error];
    STAssertNil(error, @"readTable:withItemId: failed: %@", error.description);
    
    STAssertNotNil(item, @"item should not have been nil");
    STAssertTrue([item[MSSystemColumnId] isEqualToString:@"AmazingRecord1"], @"Incorrect item id");
    STAssertTrue([item[MSSystemColumnVersion] isEqualToString:originalItem[MSSystemColumnVersion]], @"Incorrect version");
    STAssertEqualObjects(item[MSSystemColumnUpdatedAt], originalItem[MSSystemColumnUpdatedAt], @"Incorrect updated at");
    STAssertEqualObjects(item[MSSystemColumnCreatedAt], originalItem[MSSystemColumnCreatedAt], @"Incorrect created at");
    STAssertEqualObjects(item[MSSystemColumnDeleted], originalItem[MSSystemColumnDeleted], @"Incorrect deleted");
    STAssertEqualObjects(item[@"__meaningOfLife"], originalItem[@"__meaningOfLife"], @"Incorrect meaning of life");
    
    NSSet *msKeys = [item keysOfEntriesPassingTest:^BOOL(id key, id obj, BOOL *stop) {
        *stop = [(NSString *)key hasPrefix:@"ms_"];
        return *stop;
    }];
    STAssertTrue(msKeys.count == 0, @"ms_ column keys were exposed");
    
    // Repeat for query
    
    MSSyncTable *manySystemColumns = [[MSSyncTable alloc] initWithName:@"ManySystemColumns" client:nil];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:manySystemColumns predicate:nil];
    
    MSSyncContextReadResult *result = [self.store readWithQuery:query orError:&error];
    STAssertNil(error, @"readWithQuery: failed: %@", error.description);
    STAssertNotNil(result, @"result should not have been nil");
    STAssertTrue(result.items.count == 1, @"Unexpected result count %d", result.items.count);
    
    STAssertNotNil(item, @"item should not have been nil");
    STAssertTrue([item[MSSystemColumnId] isEqualToString:@"AmazingRecord1"], @"Incorrect item id");
    STAssertTrue([item[MSSystemColumnVersion] isEqualToString:originalItem[MSSystemColumnVersion]], @"Incorrect version");
    STAssertEqualObjects(item[MSSystemColumnUpdatedAt], originalItem[MSSystemColumnUpdatedAt], @"Incorrect updated at");
    STAssertEqualObjects(item[MSSystemColumnCreatedAt], originalItem[MSSystemColumnCreatedAt], @"Incorrect created at");
    STAssertEqualObjects(item[MSSystemColumnDeleted], originalItem[MSSystemColumnDeleted], @"Incorrect deleted");
    STAssertEqualObjects(item[@"__meaningOfLife"], originalItem[@"__meaningOfLife"], @"Incorrect meaning of life");
    
    msKeys = [item keysOfEntriesPassingTest:^BOOL(id key, id obj, BOOL *stop) {
        *stop = [(NSString *)key hasPrefix:@"ms_"];
        return *stop;
    }];
    STAssertTrue(msKeys.count == 0, @"ms_ column keys were exposed");
}

-(void)testUpsertNoTableError
{
    NSError *error;
    NSArray *testArray = [NSArray arrayWithObject:@{@"id":@"A", @"text": @"test1"}];
    
    [self.store upsertItems:testArray table:@"NoSuchTable" orError:&error];

    STAssertNotNil(error, @"upsert failed: %@", error.description);
    STAssertEquals(error.code, MSSyncTableLocalStoreError, @"Unexpected code");
}

-(void)testReadWithQuery
{
    NSError *error;
    
    [self populateTestData];
    
    MSSyncTable *todoItem = [[MSSyncTable alloc] initWithName:@"TodoItem" client:nil];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoItem predicate:nil];
    query.predicate = [NSPredicate predicateWithFormat:@"text == 'test3'"];
    
    MSSyncContextReadResult *result = [self.store readWithQuery:query orError:&error];
    STAssertNil(error, @"readWithQuery: failed: %@", error.description);
    STAssertNotNil(result, @"result should not have been nil");
    STAssertTrue(result.items.count == 1, @"Unexpected result count %d", result.items.count);
    STAssertTrue([[[result.items objectAtIndex:0] objectForKey:@"id"] isEqualToString:@"C"], @"Record C should have been found");
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
    STAssertNil(error, @"readWithQuery: failed: %@", error.description);
    STAssertNotNil(result, @"result should not have been nil");
    STAssertTrue(result.totalCount == 3, @"Unexpected total count %d", result.items.count);
    STAssertTrue(result.items.count == 1, @"Unexpected result count %d", result.items.count);
}

-(void)testReadWithQuery_SortAscending
{
    NSError *error;
    
    [self populateTestData];
    
    MSSyncTable *todoItem = [[MSSyncTable alloc] initWithName:@"TodoItem" client:nil];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoItem predicate:nil];
    [query orderByAscending:@"sort"];
    
    MSSyncContextReadResult *result = [self.store readWithQuery:query orError:&error];
    STAssertNil(error, @"readWithQuery: failed: %@", error.description);
    STAssertNotNil(result, @"result should not have been nil");
    STAssertTrue(result.items.count == 3, @"Unexpected result count %d", result.items.count);
    
    NSDictionary *item = [result.items objectAtIndex:0];
    STAssertTrue([[item objectForKey:@"id"] isEqualToString:@"C"], @"sort incorrect");
}

-(void)testReadWithQuery_SortDescending
{
    NSError *error;
    
    [self populateTestData];
    
    MSSyncTable *todoItem = [[MSSyncTable alloc] initWithName:@"TodoItem" client:nil];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoItem predicate:nil];
    [query orderByDescending:@"sort"];
    
    MSSyncContextReadResult *result = [self.store readWithQuery:query orError:&error];
    STAssertNil(error, @"readWithQuery: failed: %@", error.description);
    STAssertNotNil(result, @"result should not have been nil");
    STAssertTrue(result.items.count == 3, @"Unexpected result count %d", result.items.count);
    
    NSDictionary *item = [result.items objectAtIndex:0];
    STAssertTrue([[item objectForKey:@"id"] isEqualToString:@"B"], @"sort incorrect");
}

-(void)testReadWithQuery_Select
{
    NSError *error;
    
    [self populateTestData];
    
    MSSyncTable *todoItem = [[MSSyncTable alloc] initWithName:@"TodoItem" client:nil];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoItem predicate:nil];
    query.selectFields = [NSArray arrayWithObjects:@"sort", @"text", nil];
    
    MSSyncContextReadResult *result = [self.store readWithQuery:query orError:&error];
    STAssertNil(error, @"readWithQuery: failed: %@", error.description);
    STAssertNotNil(result, @"result should not have been nil");
    STAssertTrue(result.items.count == 3, @"Unexpected result count %d", result.items.count);
    
    NSDictionary *item = [result.items objectAtIndex:0];
    STAssertNil([item objectForKey:@"id"], @"id should have been nil");
    STAssertNotNil([item objectForKey:@"sort"], @"sort should not have been nil");
    STAssertNotNil([item objectForKey:@"text"], @"text should not have been nil");
    
    // NOTE: to not break oc, you get version regardless
    STAssertNotNil([item objectForKey:@"__version"], @"version should have been nil");
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
    STAssertNil(error, @"Upsert failed: %@", error.description);
    
    // Now check selecting subset of columns
    MSSyncTable *todoItem = [[MSSyncTable alloc] initWithName:@"ManySystemColumns" client:nil];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoItem predicate:nil];
    query.selectFields = [NSArray arrayWithObjects:@"text", @"__version", @"__meaningOfLife", nil];
    
    MSSyncContextReadResult *result = [self.store readWithQuery:query orError:&error];
    STAssertNil(error, @"readWithQuery: failed: %@", error.description);
    STAssertNotNil(result, @"result should not have been nil");
    STAssertTrue(result.items.count == 3, @"Unexpected result count %d", result.items.count);
    
    NSDictionary *item = [result.items objectAtIndex:0];
    STAssertNotNil([item objectForKey:@"text"], @"Expected text");
    STAssertNotNil([item objectForKey:@"__meaningOfLife"], @"Expected __meaningOfLine");
    STAssertNotNil([item objectForKey:MSSystemColumnVersion], @"Expected __version");
    STAssertEquals(item.count, 3U, @"Select returned extra columns");
}

-(void)testReadWithQuery_NoTable_Error
{
    NSError *error;

    MSSyncTable *todoItem = [[MSSyncTable alloc] initWithName:@"NoSuchTable" client:nil];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoItem predicate:nil];
    
    MSSyncContextReadResult *result = [self.store readWithQuery:query orError:&error];

    STAssertNil(result, @"Result should have been nil");
    STAssertNotNil(error, @"upsert failed: %@", error.description);
    STAssertEquals(error.code, MSSyncTableLocalStoreError, @"Unexpected code");
}

-(void)testDeleteWithId_Success
{
    NSError *error;
    
    [self populateTestData];
    
    [self.store deleteItemsWithIds:[NSArray arrayWithObject:@"B"] table:@"TodoItem" orError:&error];
    STAssertNil(error, @"deleteItemsWithIds: failed: %@", error.description);

    NSDictionary *item = [self.store readTable:@"TodoItem" withItemId:@"B" orError:&error];
    STAssertNil(error, @"readTable:withItemId: failed: %@", error.description);
    STAssertNil(item, @"item should have been nil");
}

-(void)testDeleteWithId_MultipleRecords_Success
{
    NSError *error;
    
    [self populateTestData];
    
    [self.store deleteItemsWithIds:[NSArray arrayWithObjects:@"A", @"C", nil] table:@"TodoItem" orError:&error];
    STAssertNil(error, @"deleteItemsWithIds: failed: %@", error.description);
    
    NSDictionary *item = [self.store readTable:@"TodoItem" withItemId:@"A" orError:&error];
    STAssertNil(error, @"readTable:withItemId: failed: %@", error.description);
    STAssertNil(item, @"item should have been nil");

    item = [self.store readTable:@"TodoItem" withItemId:@"B" orError:&error];
    STAssertNil(error, @"readTable:withItemId: failed: %@", error.description);
    STAssertNotNil(item, @"item should not have been nil");
}

-(void)testDeleteWithId_NoRecord_Success
{
    NSError *error;
    
    [self.store deleteItemsWithIds:[NSArray arrayWithObject:@"B"] table:@"TodoNoVersion" orError:&error];
    STAssertNil(error, @"deleteItemsWithIds: failed: %@", error.description);
}

-(void)testDeleteWithIds_NoTable_Error
{
    NSError *error;
    
    [self.store deleteItemsWithIds:[NSArray arrayWithObject:@"B"] table:@"NoSuchTable" orError:&error];
    
    STAssertNotNil(error, @"upsert failed: %@", error.description);
    STAssertEquals(error.code, MSSyncTableLocalStoreError, @"Unexpected code");
}

- (void)testDeleteWithQuery_AllRecord_Success
{
    NSError *error;
    
    [self populateTestData];

    MSSyncTable *todoItem = [[MSSyncTable alloc] initWithName:@"TodoItem" client:nil];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoItem predicate:nil];
    
    [self.store deleteUsingQuery:query orError:&error];
    STAssertNil(error, @"deleteItemsWithIds: failed: %@", error.description);

    MSSyncContextReadResult *result = [self.store readWithQuery:query orError:&error];
    STAssertNil(error, @"readWithQuery: failed: %@", error.description);
    STAssertNotNil(result, @"result should not have been nil");
    STAssertTrue(result.items.count == 0, @"Unexpected result count %d", result.items.count);
}

- (void)testDeleteWithQuery_Predicate_Success
{
    NSError *error;
    
    [self populateTestData];
    
    MSSyncTable *todoItem = [[MSSyncTable alloc] initWithName:@"TodoItem" client:nil];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoItem predicate:nil];
    query.predicate = [NSPredicate predicateWithFormat:@"text == 'test3'"];
    
    [self.store deleteUsingQuery:query orError:&error];
    STAssertNil(error, @"deleteItemsWithIds: failed: %@", error.description);
    
    query.predicate = nil;
    MSSyncContextReadResult *result = [self.store readWithQuery:query orError:&error];
    STAssertNil(error, @"readWithQuery: failed: %@", error.description);
    STAssertNotNil(result, @"result should not have been nil");
    STAssertTrue(result.items.count == 2, @"Unexpected result count %d", result.items.count);
    STAssertFalse([[[result.items objectAtIndex:0] objectForKey:@"id"] isEqualToString:@"C"], @"Record C should have been deleted");
    STAssertFalse([[[result.items objectAtIndex:1] objectForKey:@"id"] isEqualToString:@"C"], @"Record C should have been deleted");
}

-(void)testDeleteWithQuery_NoTable_Error
{
    NSError *error;
    
    MSSyncTable *todoItem = [[MSSyncTable alloc] initWithName:@"NoSuchTable" client:nil];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:todoItem predicate:nil];
    query.predicate = [NSPredicate predicateWithFormat:@"text == 'test3'"];
    
    [self.store deleteUsingQuery:query orError:&error];

    STAssertNotNil(error, @"upsert failed: %@", error.description);
    STAssertEquals(error.code, MSSyncTableLocalStoreError, @"Unexpected code");
}

- (void) populateTestData
{
    NSError *error;
    NSArray *testArray = [NSArray arrayWithObjects:
                          @{@"id":@"A", @"text": @"test1", @"sort":@10, @"__version":@"APPLE"},
                          @{@"id":@"B", @"text": @"test2", @"sort":@15, @"__version":@"APPLE"},
                          @{@"id":@"C", @"text": @"test3", @"sort":@5, @"__version":@"APPLE"},
                          nil];
    
    [self.store upsertItems:testArray table:@"TodoItem" orError:&error];
    STAssertNil(error, @"upsert failed: %@", error.description);
}

@end
