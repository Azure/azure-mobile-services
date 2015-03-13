//
//  MSManagedObjectObserverTests.m
//  WindowsAzureMobileServices
//
//  Created by Damien Pontifex on 13/03/2015.
//  Copyright (c) 2015 Windows Azure. All rights reserved.
//

#import <XCTest/XCTest.h>
#import "MSCoreDataStore.h"
#import "MSCoreDataStore+TestHelper.h"
#import "MSJSONSerializer.h"
#import "TodoItem.h"
#import "MSManagedObjectObserver.h"
#import "MSTableOperation.h"

@interface MSManagedObjectObserverTests : XCTestCase
@property (nonatomic, strong) MSCoreDataStore *store;
@property (nonatomic, strong) NSManagedObjectContext *context;
@property (nonatomic, strong) MSManagedObjectObserver *observer;

@property (nonatomic, strong) TodoItem *item;
@end

@implementation MSManagedObjectObserverTests

- (void)setUp
{
    [super setUp];
	
	self.context = [MSCoreDataStore inMemoryManagedObjectContext];
	self.store = [[MSCoreDataStore alloc] initWithManagedObjectContext:self.context];
	
	MSClient *client = [[MSClient alloc] initWithApplicationURL:nil applicationKey:nil];
	client.syncContext = [[MSSyncContext alloc] initWithDelegate:nil dataSource:self.store callback:nil];
	self.observer = [[MSManagedObjectObserver alloc] initWithClient:client];
	
	
	self.item = [NSEntityDescription insertNewObjectForEntityForName:@"TodoItem" inManagedObjectContext:self.context];
	
	self.item.text = @"Test item";
	self.item.id = @"ABC";
	
	NSError *err;
	[self.context save:&err];
}

- (void)tearDown
{
	self.store = nil;
	
    [super tearDown];
}

- (void)testObservingInsertOperation
{
	NSError *err;
	
	/// The save notification and subsequent calls to create table operations is ansynchronous so we have to wait a bit for this process to occur
	/// TODO: Improve how this is handled. Maybe a callback block for each operation action for
	sleep(1);
	
	NSFetchRequest *tableOperationsRequest = [NSFetchRequest fetchRequestWithEntityName:@"MS_TableOperations"];
	NSArray *tableOperations = [self.context executeFetchRequest:tableOperationsRequest error:&err];
	
	XCTAssertEqual(tableOperations.count, 1, @"Should have one insert operation after the save of TodoItem %@", self.item);
	
	NSManagedObject *tableOperation = tableOperations.firstObject;
	NSString *operationTable = [tableOperation valueForKey:@"table"];
	XCTAssertEqualObjects(operationTable, self.item.entity.name, @"The operation should be associated for the %@ table", self.item.entity.name);
	
	NSString *operationItemId = [tableOperation valueForKey:@"itemId"];
	XCTAssertEqualObjects(operationItemId, self.item.id, @"The operation should be associated for the inserted item with id %@", self.item.id);
	
	NSDictionary *properties = [[MSJSONSerializer JSONSerializer] itemFromData:[tableOperation valueForKey:@"properties"] withOriginalItem:nil ensureDictionary:YES orError:&err];
	
	XCTAssertEqual([properties[@"type"] integerValue], MSTableOperationInsert, @"Associated operation should be an insert with newly created object");
}

- (void)testObservingUpdateOperation
{
	NSError *err;
	
	/// The save notification and subsequent calls to create table operations is ansynchronous so we have to wait a bit for this process to occur
	/// TODO: Improve how this is handled. Maybe a callback block for each operation action for
	sleep(1);
	
	NSFetchRequest *tableOperationsRequest = [NSFetchRequest fetchRequestWithEntityName:@"MS_TableOperations"];
	NSArray *tableOperations = [self.context executeFetchRequest:tableOperationsRequest error:&err];
	
	for (NSManagedObject *managedObject in tableOperations)
	{
		// Clean out the pending insert operation
		[self.context deleteObject:managedObject];
	}
	[self.context save:&err];
	
	self.item.text = @"Test item updated";
	[self.context save:&err];
	
	sleep(1);
	
	tableOperations = [self.context executeFetchRequest:tableOperationsRequest error:&err];
	
	XCTAssertEqual(tableOperations.count, 1, @"Should have one insert operation after the save of TodoItem %@", self.item);
	
	NSManagedObject *tableOperation = tableOperations.firstObject;
	NSString *operationTable = [tableOperation valueForKey:@"table"];
	XCTAssertEqualObjects(operationTable, self.item.entity.name, @"The operation should be associated for the %@ table", self.item.entity.name);
	
	NSString *operationItemId = [tableOperation valueForKey:@"itemId"];
	XCTAssertEqualObjects(operationItemId, self.item.id, @"The operation should be associated for the inserted item with id %@", self.item.id);
	
	NSDictionary *properties = [[MSJSONSerializer JSONSerializer] itemFromData:[tableOperation valueForKey:@"properties"] withOriginalItem:nil ensureDictionary:YES orError:&err];
	
	XCTAssertEqual([properties[@"type"] integerValue], MSTableOperationUpdate, @"Associated operation should be an insert with newly created object");
}

- (void)testObservingDeleteOperation
{
	NSError *err;
	
	/// The save notification and subsequent calls to create table operations is ansynchronous so we have to wait a bit for this process to occur
	/// TODO: Improve how this is handled. Maybe a callback block for each operation action for
	sleep(1);
	
	NSFetchRequest *tableOperationsRequest = [NSFetchRequest fetchRequestWithEntityName:@"MS_TableOperations"];
	NSArray *tableOperations = [self.context executeFetchRequest:tableOperationsRequest error:&err];
	
	for (NSManagedObject *managedObject in tableOperations)
	{
		// Clean out the pending insert operation
		[self.context deleteObject:managedObject];
	}
	[self.context save:&err];
	
	NSString *originalItemId = self.item.id;
	
	[self.context deleteObject:self.item];
	[self.context save:&err];
	
	sleep(1);
	
	tableOperations = [self.context executeFetchRequest:tableOperationsRequest error:&err];
	
	XCTAssertEqual(tableOperations.count, 1, @"Should have one insert operation after the save of TodoItem %@", self.item);
	
	NSManagedObject *tableOperation = tableOperations.firstObject;
	NSString *operationTable = [tableOperation valueForKey:@"table"];
	XCTAssertEqualObjects(operationTable, self.item.entity.name, @"The operation should be associated for the %@ table", self.item.entity.name);
	
	NSString *operationItemId = [tableOperation valueForKey:@"itemId"];
	XCTAssertEqualObjects(operationItemId, originalItemId, @"The operation should be associated for the inserted item with id %@", originalItemId);
	
	NSDictionary *properties = [[MSJSONSerializer JSONSerializer] itemFromData:[tableOperation valueForKey:@"properties"] withOriginalItem:nil ensureDictionary:YES orError:&err];
	
	XCTAssertEqual([properties[@"type"] integerValue], MSTableOperationDelete, @"Associated operation should be an insert with newly created object");
}

@end
