// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSSyncContext.h"
#import "MSSyncContextInternal.h"
#import "MSClientInternal.h"
#import "MSTable.h"
#import "MSTableOperationInternal.h"
#import "MSTableOperationError.m"
#import "MSQuery.h"
#import "MSBookmarkOperation.h"
#import "MSQueuePushOperation.h"

@implementation MSSyncContext {
    dispatch_queue_t writeOperationQueue;
    dispatch_queue_t callbackQueue;
}

static NSOperationQueue *pushQueue_;

@synthesize delegate = delegate_;
@synthesize dataSource = dataSource_;
@synthesize operationQueue = operationQueue_;
@synthesize errors = errors_;

- (id) init
{
    return [self initWithDelegate:nil andDataSource:nil];
}

- (id) initWithDelegate:(id<MSSyncContextDelegate>) delegate andDataSource:(id<MSSyncContextDataSource>) dataSource
{
    self = [super init];
    if (self)
    {
        writeOperationQueue = dispatch_queue_create("WriteOperationQueue", DISPATCH_QUEUE_CONCURRENT);
        callbackQueue = dispatch_queue_create("SyncTableCallbacks", DISPATCH_QUEUE_CONCURRENT);
        
        pushQueue_ = [NSOperationQueue new];
        [pushQueue_ setMaxConcurrentOperationCount:1];
        
        dataSource_ = dataSource;
        delegate_ = delegate;
        operationQueue_ = [MSOperationQueue new];
        errors_ = [NSMutableArray new];
    }
    return self;
}

/// Load previously stored operations from local store
-(void) loadOperations
{
    dispatch_async(writeOperationQueue, ^{
        MSSyncTable *table = [[MSSyncTable alloc] initWithName:[self.dataSource operationTableName] client:self.client];
        MSQuery *query = [[MSQuery alloc] initWithSyncTable:table];
        
        [self.dataSource readWithQuery:query completion:^(NSArray *items, NSInteger totalCount, NSError *error) {
            for (NSDictionary *item in items) {
                [self.operationQueue addOperation:[[MSTableOperation alloc] initWithItem:item]];
            }
        }];
    });
}

/// Return the number of pending operations (including in progress)
-(NSUInteger) pendingOperationsCount
{
    return [self.operationQueue count];
}

/// Begin sending pending operations to the remote tables. Abort the push attempt whenever any single operation
/// recieves an error due to network or authorization. Otherwise operations will all run and all errors returned
/// to the caller
-(void) pushWithCompletion:(MSSyncBlock)completion
{
    // TODO: Allow users to cancel
    
    dispatch_async(writeOperationQueue, ^{
        MSBookmarkOperation *pushOperation = [MSBookmarkOperation new];
        [self.operationQueue addOperation:pushOperation];
        
        MSQueuePushOperation *push = [[MSQueuePushOperation alloc] initWithPushOperation:pushOperation
                                                                             syncContext:self
                                                                           dispatchQueue:writeOperationQueue
                                                                              completion:completion];
        
        [pushQueue_ addOperation:push];
    });
}


#pragma mark private interface implementation


/// Given an item and an action to perform (insert, update, delete) determines how that should be represented when sent to the
/// server based on pending operations and how the local store should therefore be updated.
- (void) syncTable:(NSString *)table item:(NSDictionary *)item action:(MSTableOperationTypes)action completion:(MSSyncItemBlock)completion
{
    NSError *error;
    NSMutableDictionary *itemToSave = [item mutableCopy];
    NSString *itemId;
    
    // Validate our input and state
    if (!self.dataSource) {
        error = [self errorWithDescription:@"Missing required datasource for MSSyncContext" andErrorCode:MSSyncContextInvalid];
    }
    else {
        // All sync table operations require a valid string Id
        itemId = [self.client.serializer stringIdFromItem:item orError:&error];
        if (error) {
            if (error.code == MSMissingItemIdWithRequest && action == MSTableOperationInsert) {
                itemId = [MSJSONSerializer generateGUID];
                [itemToSave setValue:itemId forKey:@"id"];
                error = nil;
            }
        }
    }
    
    if (error) {
        if (completion) {
            completion(nil, error);
        }
        return;
    }
    
    // Remove system properties but keep __version
    NSString *version = [itemToSave objectForKey:MSSystemColumnVersion];
    [self.client.serializer removeSystemProperties:itemToSave];
    if (version != nil) {
        [itemToSave setValue:version forKey:MSSystemColumnVersion];
    }

    // Add the operation to the queue
    dispatch_async(writeOperationQueue, ^{
        NSError *error;
        MSCondenseAction condenseAction = MSCondenseAddNew;
        
        // Check if this table-item pair already has a pending operation and if so, how the new action
        // should be combined with the previous one
        NSArray *pendingActions = [self.operationQueue getOperationsForTable:table item:itemId];
        MSTableOperation *operation = [pendingActions lastObject];
        if (operation) {
            condenseAction = [MSTableOperation condenseAction:action withExistingOperation:operation];
            if (condenseAction == MSCondenseNotSupported) {
                error = [self errorWithDescription:@"The requested operation is not allowed due to an already pending operation"
                                      andErrorCode:MSSyncTableInvalidAction];
            }
        }
        
        if (condenseAction == MSCondenseAddNew) {
            operation = [MSTableOperation pushOperationForTable:table type:action item:itemId];
        }

        // Update local store and then the operation queue
        if (error == nil) {
            switch (action) {
                case MSTableOperationInsert:
                    [self.dataSource upsertItem:itemToSave table:table orError:&error];
                    break;
                
                case MSTableOperationUpdate:
                    [self.dataSource upsertItem:itemToSave table:table orError:&error];
                    break;
                    
                case MSTableOperationDelete:
                    // TODO: Save a copy of the original item
                    [self.dataSource deleteItemWithId:itemId table:table orError:&error];
                    break;
                
                default:
                    error = [self errorWithDescription:@"Unknown table action" andErrorCode:MSSyncTableInvalidAction];
                    break;
            }
        }
        
        if (error) {
            if (completion) {
                dispatch_async(callbackQueue, ^{
                    completion(nil, error);
                });
            }
            return;
        }

        // Update the operation queue now
        if (condenseAction == MSCondenseAddNew) {
            [self.operationQueue addOperation:operation];
            [self.dataSource upsertItem:[operation serialize]
                                  table:[self.dataSource operationTableName]
                                orError:&error];
            
        }
        else if (condenseAction == MSCondenseToDelete) {
            operation.type = MSTableOperationDelete;
            [self.dataSource upsertItem:[operation serialize]
                                  table:[self.dataSource operationTableName]
                                orError:&error];
            
        } else if (condenseAction != MSCondenseKeep) {
            [self removeOperation:operation];
        }
        
        if (completion) {
            dispatch_async(callbackQueue, ^{
                completion([itemToSave copy], error);
            });
        }
    });
}

/// Simple passthrough to the local storage data source to retrive a list of items
-(void)readWithQuery:(MSQuery *)query completion:(MSSyncReadBlock)completion {
    [self.dataSource readWithQuery:query completion:completion];
}

/// Simple passthrough to the local storage data source to retrive a single item using its Id
- (NSDictionary *) syncTable:(NSString *)table readWithId:(NSString *)itemId orError:(NSError **)error;
{
    return [self.dataSource readTable:table withItemId:itemId orError:error];
}

/// Assumes running with access to the operation queue
- (NSError *) removeOperation:(MSTableOperation *)operation
{
    NSError *error;
    
    [self.operationQueue removeOperation:operation];
    [self.dataSource deleteItemWithId:operation.guid
                                table:[self.dataSource operationTableName]
                              orError:&error];
    
    return error;
}


#pragma mark Local Storage Logic


/// Verify our input is valid and try to pull our data down from the server
- (void) pullWithQuery:(MSQuery *)query completion:(MSSyncBlock)completion;
{
    // We want to throw on unsupported fields so we can change this decision later
    NSError *error;
    if (query.selectFields) {
        error = [self errorWithDescription:@"Use of selectFields in not supported in pullWithQuery:" andErrorCode:MSInvalidParameter];
    }
    else if (query.includeTotalCount) {
        error = [self errorWithDescription:@"Use of includeTotalCount is not supported in pullWithQuery:" andErrorCode:MSInvalidParameter];
    }
    else if (query.table) {
        // We allow users to pull with a regular table, if it is setup properly
        if (query.table.systemProperties != MSSystemPropertyVersion) {
            error = [self errorWithDescription:@"Only the version system property is supported in pullWithQuery:" andErrorCode:MSInvalidParameter];
        }
    } else if (query.syncTable) {
        // Otherwise we convert the sync table to a normal table
        query.table = [[MSTable alloc] initWithName:query.syncTable.name client:query.syncTable.client];
        query.table.systemProperties = MSSystemPropertyVersion;
        query.syncTable = nil;
    } else {
        // MSQuery itself should disallow this, but for safety verify we have a table object
        error = [self errorWithDescription:@"Missing required syncTable object in query" andErrorCode:MSInvalidParameter];
    }
    
    // Begin the actual pull request
    [self pullWithQueryInternal:query completion:completion];
}

/// Basic pull logic is:
///  Check if our table has pending operations, if so, push
///    If push fails, return error, else repeat while we have pending operations
///  Read from server and get the new results
///  If our table became dirty while we read from the server, start over
///  Else save our data into the local store
- (void)pullWithQueryInternal:(MSQuery *)query completion:(MSSyncBlock)completion
{
    dispatch_async(writeOperationQueue, ^{
        // Before we can pull from the remote, we need to make sure out table doesn't having pending operations
        NSArray *tableOps = [self.operationQueue getOperationsForTable:query.syncTable.name item:nil];
        if (tableOps.count > 0) {
            [self pushWithCompletion:^(NSError *error) {
                // For now we just abort the pull if the push failed to complete successfully
                // Long term we can be smarter and check if our table succeeded
                if (error) {
                    if (completion) {
                        completion(error);
                    }
                }
                else {
                    // Check again if we have new pending ops while we synced, and repeat as needed
                    [self pullWithQueryInternal:query completion:completion];
                }
            }];
            return;
        }
        
        // Read from server
        [query readWithCompletion:^(NSArray *serverItems, NSInteger totalCount, NSError *error) {
            if (error) {
                if (completion) {
                    completion(error);
                    return;
                }
            }
            
            // Update our local store (we need to block inbound operations while we do this)
            dispatch_sync(writeOperationQueue, ^{
                // Check if have any pending ops on this table
                NSArray *pendingOps = [self.operationQueue getOperationsForTable:query.table.name item:nil];
                if (pendingOps.count > 0) {
                    // This is not the best choice here, but for simplicity in the initial cut we just start over
                    // if someone else has made our table dirty
                    [self pullWithQueryInternal:query completion:completion];
                    return;
                }
                
                // upsert each item into table that isn't pending to go to server
                NSError *localDataSourceError;
                for (NSDictionary *serverItem in serverItems) {
                    [self.dataSource upsertItem:serverItem table:query.table.name orError:&localDataSourceError];
                    if (error) {
                        // Not ideal, but any store error aborts the pull in progress (we will want to review this
                        // when we add a smarter sync policy)
                        break;
                    }
                }
                
                if (completion) {
                    dispatch_async(callbackQueue, ^{
                        completion(localDataSourceError);
                    });
                }
            });
        }];
    });
}

/// In order to purge data from the local store, purge first checks if there are any pending operations for
/// the specific table on the query. If there are, a push is done to the server. Once the push completes, if no
/// errors (and no new table operations) data is removed from the local store.
- (void) purgeWithQuery:(MSQuery *)query completion:(MSSyncBlock)completion
{
    dispatch_async(writeOperationQueue, ^{
        // Check if our table is dirty, if so, we need to force a push, otherwise we can proceed
        NSArray *tableOps = [self.operationQueue getOperationsForTable:query.syncTable.name item:nil];
        if (tableOps.count > 0) {
            [self pushWithCompletion:^(NSError *error) {
                // For now all push errors abort the purge, long term we could try again
                // if our table operations were successful
                if (error) {
                    completion(error);
                    return;
                }
                
                // try a purge again (more inbound operations could have occurred) so we must start over
                [self purgeWithQuery:query completion:completion];
            }];
        }
        else {
            // We can safely delete all items on this table (no pending operations)
            NSError *error;
            [self.dataSource deleteUsingQuery:query orError:&error];
            
            if (completion) {
                dispatch_async(callbackQueue, ^{
                    completion(error);
                });
            }
        }
    });
}


# pragma mark * NSError helpers


-(NSError *) errorWithDescription:(NSString *)description
                     andErrorCode:(NSInteger)errorCode
{
    NSDictionary *userInfo = @{ NSLocalizedDescriptionKey :description };
    
    return [NSError errorWithDomain:MSErrorDomain
                               code:errorCode
                           userInfo:userInfo];
}

@end
