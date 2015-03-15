//
//  MSManagedObjectObserver.m
//  WindowsAzureMobileServices
//
//  Created by Phillip Van Nortwick on 2/5/15.
//  Copyright (c) 2015 Windows Azure. All rights reserved.
//

#import "MSManagedObjectObserver.h"
#import "MSCoreDataStore.h"
#import "MSClient.h"
#import <CoreData/CoreData.h>

@interface MSManagedObjectObserver()
@property (nonatomic, strong) MSClient *client;
@property (nonatomic, weak) NSManagedObjectContext *context;
@end

@implementation MSManagedObjectObserver

- (instancetype) initWithClient:(MSClient *)client
{
    self = [super init];
    if (self) {
		// Copy so we can change the handling of how the sync table operations are handled
        _client = [client copy];
		
		if ([_client.syncContext.dataSource isKindOfClass:[MSCoreDataStore class]])
		{
			MSCoreDataStore *dataStore = _client.syncContext.dataSource;
			_context = dataStore.context;
			
			[[NSNotificationCenter defaultCenter] addObserver:self
													 selector:@selector(handleDidSaveNotification:)
														 name:NSManagedObjectContextDidSaveNotification
													   object:_context];
		}
		
		// Modify the handling of sync table operations for this instance of the data source
		_client.syncContext.dataSource.handlesSyncTableOperations = FALSE;
    }
    return self;
}

- (void)dealloc
{
	if (self.context != nil)
	{
		[[NSNotificationCenter defaultCenter] removeObserver:self
														name:NSManagedObjectContextDidSaveNotification
													  object:self.context];
	}
}

/// Checks the entity has the ms_version attribute to signal it should
/// be synchronised to the mobile service
- (BOOL)entityHasRemoteStoreAttributes:(NSManagedObject *)managedObject
{
	return (managedObject.entity.attributesByName[StoreVersion] != nil);
}

- (void)handleDidSaveNotification:(NSNotification *)notification
{
	NSSet *insertedObjects = notification.userInfo[NSInsertedObjectsKey];
	for (NSManagedObject *insertedObject in insertedObjects)
	{
		if (![self entityHasRemoteStoreAttributes:insertedObject]) {
			// Only apply table operations on entities we expect to be managed by the mobile service
			continue;
		}
		
		NSString *tableName = insertedObject.entity.name;
		NSDictionary *tableItem = [MSCoreDataStore tableItemFromManagedObject:insertedObject];
		
		MSSyncTable *syncTable = [self.client syncTableWithName:tableName];
		[syncTable insert:tableItem completion:^(NSDictionary *item, NSError *error) {
			if (self.observerActionCompleted != nil)
			{
				self.observerActionCompleted(MSTableOperationInsert, tableItem, error);
			}
		}];
	}
	
	NSSet *updatedObjects = notification.userInfo[NSUpdatedObjectsKey];
	for (NSManagedObject *updatedObject in updatedObjects)
	{
		if (![self entityHasRemoteStoreAttributes:updatedObject]) {
			// Only apply table operations on entities we expect to be managed by the mobile service
			continue;
		}
		
		NSString *tableName = updatedObject.entity.name;
		NSDictionary *tableItem = [MSCoreDataStore tableItemFromManagedObject:updatedObject];
		
		MSSyncTable *syncTable = [self.client syncTableWithName:tableName];
		[syncTable update:tableItem completion:^(NSError *error) {
			if (self.observerActionCompleted != nil)
			{
				self.observerActionCompleted(MSTableOperationUpdate, tableItem, error);
			}
		}];
	}
	
	NSSet *deletedObjects = notification.userInfo[NSDeletedObjectsKey];
	for (NSManagedObject *deletedObject in deletedObjects)
	{
		if (![self entityHasRemoteStoreAttributes:deletedObject]) {
			// Only apply table operations on entities we expect to be managed by the mobile service
			continue;
		}
		
		NSString *tableName = deletedObject.entity.name;
		NSDictionary *tableItem = [MSCoreDataStore tableItemFromManagedObject:deletedObject];
		
		MSSyncTable *syncTable = [self.client syncTableWithName:tableName];
		[syncTable delete:tableItem completion:^(NSError *error) {
			if (self.observerActionCompleted != nil)
			{
				self.observerActionCompleted(MSTableOperationDelete, tableItem, error);
			}
		}];
	}
}

@end
