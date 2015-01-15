// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import <WindowsAzureMobileServices/WindowsAzureMobileServices.h>
#import <CoreData/CoreData.h>

/// The MSCoreDataStore class is for use when using the offline capabilities
/// of mobile services. This class is a local store which manages records and sync
/// logic using CoreData.
/// This class assumes the provided managed object context has the following tables:
/// MS_TableOperations:      Columns: id (Integer 64), itemId (string), table (string), properties (binary data)
/// MS_TableOperationErrors: Columns: id (string), properties (binary data)
/// and all tables contain a ms_version column
@interface MSCoreDataStore : NSObject <MSSyncContextDataSource>

#pragma  mark * Public Static Constructor Methods

/// @name Initializing the MSClient Object
/// @{

/// Creates a CoreDataStore with the given managed object context.
-(id) initWithManagedObjectContext:(NSManagedObjectContext *)context;

/// @}

#pragma mark * Helper functions

/// @{name Working with the table APIs

/// Converts a managed object from the core data layer back into a dictionary with the
/// properties expected when using a MSTable or MSSyncTable
+(NSDictionary *) tableItemFromManagedObject:(NSManagedObject *)object;

/// @}

@end
