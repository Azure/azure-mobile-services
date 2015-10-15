// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import <CoreData/CoreData.h>
#import "MSSyncContext.h"

extern NSString *const StoreVersion;

/// The MSCoreDataStore class is for use when using the offline capabilities
/// of mobile services. This class is a local store which manages records and sync
/// logic using CoreData.
/// This class assumes the provided managed object context has the following tables:
/// MS_TableOperations:      Columns: id (Integer 64), itemId (string), table (string), properties (binary data)
/// MS_TableOperationErrors: Columns: id (string), properties (binary data)
/// and all tables contain a ms_version column
@interface MSCoreDataStore : NSObject <MSSyncContextDataSource>

/// The NSManagedObjectContext that is associated with this data store
@property (readonly, nonatomic, strong) NSManagedObjectContext *context;

/// Disables the store from recieving information about the items passed into all sync table
/// calls (insert, delete, update). If set, the application is responsible for already having
/// saved the item in the persisten store. This flag is intended to be used when application
/// code is working directly with NSManagedObjects.
@property (nonatomic) BOOL handlesSyncTableOperations;

#pragma  mark * Public Static Constructor Methods

/// @name Initializing the MSClient Object
/// @{

/// Creates a CoreDataStore with the given managed object context.
-(id) initWithManagedObjectContext:(NSManagedObjectContext *)context;

/// @}

/// Disables the store from recieving information about the items passed into all sync table
/// calls (insert, delete, update). If set, the application is responsible for already having
/// saved the item in the persisten store. This flag is intended to be used when application
/// code is working directly with NSManagedObjects.
@property (nonatomic) BOOL handlesSyncTableOperations;

#pragma mark * Helper functions

/// @{name Working with the table APIs

/// Converts a managed object from the core data layer back into a dictionary with the
/// properties expected when using a MSTable or MSSyncTable
+(NSDictionary *) tableItemFromManagedObject:(NSManagedObject *)object;

/// @}

/// @{name Working with multiple ManagedObjectContext

/// Function to subscribe a given context to all MOC saves. These will be used to generate sync table
/// calls.

/// @}

@end
