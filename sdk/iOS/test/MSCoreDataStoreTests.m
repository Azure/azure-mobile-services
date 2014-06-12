// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------


#import <SenTestingKit/SenTestingKit.h>
#import "MSCoreDataStore.h"

@interface MSCoreDataStoreTests : SenTestCase {
    BOOL done;
}
@property (nonatomic, strong) MSCoreDataStore *store;
@end

@implementation MSCoreDataStoreTests

-(NSManagedObjectContext *)inMemoryManagedObjectContext
{
    NSBundle *bundle = [NSBundle bundleForClass:[self class]];
    NSURL *url = [bundle URLForResource:@"CoreDataTestModel" withExtension:@"momd"];
    NSManagedObjectModel *model = [[NSManagedObjectModel alloc] initWithContentsOfURL:url];
    STAssertNotNil(model, @"NSManagedObjectModel creation failed");
    
    NSPersistentStoreCoordinator *coordinator = [[NSPersistentStoreCoordinator alloc] initWithManagedObjectModel:model];
    STAssertNotNil(coordinator, @"NSPersistentStoreCoordinator creation failed");
    
    NSPersistentStore *store = [coordinator addPersistentStoreWithType:NSInMemoryStoreType configuration:nil URL:nil options:nil error:0];
    STAssertNotNil(store, @"NSPersistentStore creation failed");
    
    NSManagedObjectContext *context = [[NSManagedObjectContext alloc] initWithConcurrencyType:NSPrivateQueueConcurrencyType];
    context.persistentStoreCoordinator = coordinator;
    
    return context;
}

- (void)setUp
{
    NSLog(@"%@ setUp", self.name);
    
    self.store = [[MSCoreDataStore alloc] initWithManagedObjectContext:[self inMemoryManagedObjectContext]];
    
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

- (void)testReadWithQuery
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
    
    // note: to not break oc, you get version always
    STAssertNotNil([item objectForKey:@"__version"], @"version should have been nil");
}

-(void)testDeleteWithIdSuccess
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
