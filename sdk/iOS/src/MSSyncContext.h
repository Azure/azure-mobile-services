// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSTableOperation.h"
#import "MSSyncContextReadResult.h"

@class MSQuery;
@class MSSyncContext;

/// Callback for updates and deletes. If there was an error, the *error* will be non-nil.
typedef void (^MSSyncBlock)(NSError *error);

/// Callback for inserts. If there was an error, the *error* will be non-nil.
typedef void (^MSSyncItemBlock)(NSDictionary *item, NSError *error);

/// Callback for push operations
typedef void (^MSSyncPushCompletionBlock)(void);

/// The MSSyncContextDelegate allows for customizing the handling of errors, conflicts, and other
/// conditions that may occur when syncing data between the device and the mobile service.
@protocol MSSyncContextDelegate <NSObject>

/// @name Handling Conflicts and Errors
/// @{

@optional
/// Called once for each entry on the queue, allowing for any adjustments to the item before it is sent to the server,
/// or custom handling of the server's response (such as conflict handling). Errors returned from this function will
/// be collected and sent as a group to the [syncContext: onPushCompleteWithError: completion:] function.
-(void) tableOperation:(MSTableOperation *)operation onComplete:(MSSyncItemBlock)completion;

/// Called when all operations that were triggered due to a [pushWithCompletion:] call have completed. If not provided, any
/// errors will be passed along to the [pushWithCompletion:] call. If provided, errors can be handled and additional changes
/// may be made to the local or remote database.
-(void) syncContext:(MSSyncContext *)context onPushCompleteWithError:(NSError *)error completion:(MSSyncPushCompletionBlock)completion;

/// @}

@end

/// The MSSyncContextDataSource controls how data is stored and retrieved on the device. Errors returned from here will abort
/// any given sync operation and will be surfaced to the mobile service through push or the delegate.
@protocol MSSyncContextDataSource <NSObject>

/// @name Controlling Where Data is Stored
/// @{

/// Provides the name of the table to track all table operation meta data
- (NSString *) operationTableName;

/// Provides the name of the table to track all table operation errors until they have been resolved
- (NSString *) errorTableName;

/// Provides the name of the table to track configuration data
- (NSString *) configTableName;

/// @}

/// @name Fetching and Retrieving Data
/// @{

/// Returns a dictionary containing the items and totalCount
- (MSSyncContextReadResult *) readWithQuery:(MSQuery *)query orError:(NSError **)error;

/// Should retrieve a single item from the local store or nil if item with the given ID does not exist.
-(NSDictionary *) readTable:(NSString *)table withItemId:(NSString *)itemId orError:(NSError **)error;

/// Should insert/update the given item in the local store as appropriate
-(BOOL) upsertItems:(NSArray *)item table:(NSString *)table orError:(NSError **)error;

/// Should remove the provided item from the local store
-(BOOL) deleteItemsWithIds:(NSArray *)items table:(NSString *)table orError:(NSError **)error;

/// Should remove all entries from the specified table in the local store
-(BOOL) deleteUsingQuery:(MSQuery *)query orError:(NSError **)error;

/// @}

/// @name Controlling system properties in local tables
/// @{

@optional

/// Returns the MSSystemProperties that should be stored locally (example: __createdAt, __updatedAt)
/// If not implemented, the default of __version will be asked for from the server
-(NSUInteger) systemPropertiesForTable:(NSString *)table;

/// @}

@end

/// The *MSSyncContext* object controls how offline operations using the *MSSyncTable* object are processed,
/// stored in local data storage, and sent to the mobile service.
@interface MSSyncContext : NSObject

/// @name Initializing the MSSyncContext Object
/// @{

- (id) initWithDelegate:(id<MSSyncContextDelegate>)delegate dataSource:(id<MSSyncContextDataSource>) dataSource callback:(NSOperationQueue *)callbackQueue;

/// @}

/// @name Syncing and Storing Data
/// @{

/// Returns the number of pending outbound operations on the queue
@property (nonatomic, readonly) NSUInteger pendingOperationsCount;

/// Executes all current pending operations on the queue
- (void) pushWithCompletion:(MSSyncBlock)completion;

/// Specifies the delegate that will be used in the resolution of syncing issues
@property (nonatomic, strong) id<MSSyncContextDelegate> delegate;

/// Specifies the dataSource that owns the local data and store of operations
@property (nonatomic, strong) id<MSSyncContextDataSource> dataSource;

/// @}

@end
