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

@protocol MSSyncContextDelegate <NSObject>

@optional
/// Called once for each entry on the queue, allowing for any adjustments to the item to the server, or custom handling
/// of the server's response (such as conflict handling). Errors returned from this function will be collected and sent
/// as a group to the [pushResult: onComplete:] function.
-(void) tableOperation:(MSTableOperation *)operation onComplete:(MSSyncItemBlock)completion;

/// Called when all operations that occurred due to a pushWithCompletion: call have completed. If not provided, any
/// errors will be passed along to the pushWithCompletion: call.
-(void) syncContext:(MSSyncContext *)context onPushCompleteWithError:(NSError *)error completion:(MSSyncPushCompletionBlock)completion;

@end


@protocol MSSyncContextDataSource <NSObject>

- (NSString *) operationTableName;

- (NSString *) errorTableName;

// Returns a dictionary containing the items and totalCount
- (MSSyncContextReadResult *) readWithQuery:(MSQuery *)query orError:(NSError **)error;

/// Should retrieve a single item from the local store or nil if item with the given ID does not exist.
-(NSDictionary *) readTable:(NSString *)table withItemId:(NSString *)itemId orError:(NSError **)error;

/// Should insert/update the given item in the local store
-(BOOL) upsertItem:(NSDictionary *)item table:(NSString *)table orError:(NSError **)error;

/// Should remove the provided item from the local store
-(BOOL) deleteItemWithId:(id)item table:(NSString *)table orError:(NSError **)error;

/// Should remove all entries from the specific
-(BOOL) deleteUsingQuery:(MSQuery *)query orError:(NSError **)error;

@end


/// The *MSSyncContext* object controls how offline operations using the *MSSyncTable* object are processed,
/// stored in local data storage, and sent to the mobile service.
@interface MSSyncContext : NSObject

/// Returns the number of outbound operations on the queue
@property (nonatomic, readonly) NSUInteger pendingOperationsCount;

/// Executes all current pending operations on the queue
- (void) pushWithCompletion:(MSSyncBlock)completion;

@property (nonatomic, strong) id<MSSyncContextDelegate> delegate;
@property (nonatomic, strong) id<MSSyncContextDataSource> dataSource;

- (id) initWithDelegate:(id<MSSyncContextDelegate>)delegate dataSource:(id<MSSyncContextDataSource>) dataSource callback:(NSOperationQueue *)callbackQueue;

@end
