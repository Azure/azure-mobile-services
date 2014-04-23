// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSQueuePushOperation.h"
#import "MSTableOperationError.h"
#import "MSSyncContextInternal.h"
#import "MSClientInternal.h"
#import "MSTableOperationInternal.h"
#import "MSQuery.h"

@interface MSQueuePushOperation()

@property (nonatomic, strong) NSError *error;
@property (nonatomic, copy) NSString *guid;
@property (nonatomic, weak) dispatch_queue_t dispatchQueue;
@property (nonatomic, weak) MSSyncContext *syncContext;
@property (nonatomic, copy) MSSyncBlock completion;

@end

@implementation MSQueuePushOperation

static NSOperationQueue * delegateQueue_;

@synthesize error = error_;
@synthesize guid = guid_;
@synthesize dispatchQueue = dispatchQueue_;
@synthesize syncContext = syncContext_;
@synthesize completion = completion_;

- (id) initWithPushOperation:(MSBookmarkOperation *)pushOperation
                 syncContext:(MSSyncContext *)syncContext
               dispatchQueue:(dispatch_queue_t)dispatchQueue
                  completion:(MSSyncBlock)completion
{
    self = [super init];
    if (self) {
        guid_ = pushOperation.guid;
        syncContext_ = syncContext;
        dispatchQueue_ = dispatchQueue;
        completion_ = [completion copy];
        delegateQueue_ = [NSOperationQueue new];
    }
    return self;
}

-(void) endExecution
{
    [self willChangeValueForKey:@"isExecuting"];
    [self willChangeValueForKey:@"isFinished"];
    executing_ = NO;
    finished_  = YES;
    [self didChangeValueForKey:@"isFinished"];
    [self didChangeValueForKey:@"isExecuting"];
}

// Check if the operation was cancelled and if so, begin winding down
-(BOOL) checkIsCanceled
{
    if (self.isCancelled) {
        self.error = [self errorWithDescription:@"Push cancelled" code:MSPushAbortedUnknown internalError:nil];
        [self pushComplete];
    }
    
    return self.isCancelled;
}

-(void) start
{
    if (finished_) {
        return;
    }
    else if (self.isCancelled) {
        [self pushComplete];
        return;
    }
    
    [self willChangeValueForKey:@"isExecuting"];
    executing_ = YES;
    [self didChangeValueForKey:@"isExecuting"];
    
    // For now, we process one operation at a time
    [self processQueueEntry];
}

/// Pick an operation up out of the queue until we run out of opertions or find push
/// For each operation, attempt to send to the server and record the results until we
/// recieve a fatal error or we finish all pending operations
- (void) processQueueEntry
{
    dispatch_async(self.dispatchQueue, ^{
        id nextOperation = [self.syncContext.operationQueue peek];
        if ([nextOperation isKindOfClass:[MSTableOperation class]]) {
            [self processTableOperation:nextOperation];
            return;
        }
        
        BOOL complete = false;
        // Remove ourselves if present
        if(nextOperation) {
            // Aborted pushes may have left their bookmark in the queue so throw it away
            if ([nextOperation isKindOfClass:[MSBookmarkOperation class]]) {
                MSBookmarkOperation *operation = (MSBookmarkOperation *)nextOperation;
                complete = [self.guid isEqualToString:operation.guid];
            }
            [self.syncContext.operationQueue removeOperation:nextOperation];
        } else {
            complete = true;
        }
        
        // Stop if we found our bookmark or we hit the end of the queue
        if (complete) {
            [self pushComplete];
        } else {
            [self processQueueEntry];
        }
        return;
    });
}

/// For a given pending table operation, create the request to send it to the remote table
- (void) processTableOperation:(MSTableOperation *)operation
{
    operation.inProgress = YES;
    
    NSError *error;
    
    // Read the item from local store
    if (operation.type != MSTableOperationDelete) {
        operation.item = [self.syncContext.dataSource readTable:operation.tableName withItemId:operation.itemId orError:&error];
    }
    
    if (error) {
        NSString *errorMessage = [NSString stringWithFormat:@"Unable to read item '%@' from table '%@'", operation.itemId, operation.tableName];
        self.error = [self errorWithDescription:errorMessage code:MSPushAbortedDataSource internalError:error];
        [self pushComplete];
        return;
    }
    
    if ([self checkIsCanceled]) {
        return;
    }
    
    // Inserts need system properties removed
    if (operation.type == MSTableOperationInsert && operation.item) {
        NSMutableDictionary *item = [operation.item mutableCopy];
        [self.syncContext.client.serializer removeSystemProperties:item];
        operation.item = [item copy];
    }

    // Block to process the results of the table request and populate the appropriate data store
    id postTableOperation = ^(NSDictionary *item, NSError *error) {
        [self finishTableOperation:operation item:item error:error];
    };
    
    // Begin sending the table operation to the remote table
    operation.client = self.syncContext.client;
    id<MSSyncContextDelegate> syncDelegate = self.syncContext.delegate;
    
    // Let go of the operation queue
    dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^{
        if (syncDelegate && [syncDelegate respondsToSelector:@selector(tableOperation:onComplete:)]) {
            [syncDelegate tableOperation:operation onComplete:postTableOperation];
        } else {
            [operation executeWithCompletion:postTableOperation];
        }
    });
}

/// Process the returned item/error from a call to a remote table. Update the local store state or cancel
/// the remaining operations as necessary
- (void) finishTableOperation:(MSTableOperation *)operation item:(NSDictionary *)item error:(NSError *)error
{
    // Check if we were cancelled while we awaited our results
    if ([self checkIsCanceled]) {
        return;
    }

    // Remove the operation
    dispatch_async(self.dispatchQueue, ^{
        if (error) {
            // TODO: Condense any table-item logic since new actions can come in while this one is outgoing
            // Fix insert-update (keep insert), insert-delete (remove both), update-delete (keep delete)
            NSHTTPURLResponse *response = [error.userInfo objectForKey:MSErrorResponseKey];
            if (response && response.statusCode == 401) {
                self.error = [self errorWithDescription:@"Push aborted due to authentication error"
                                                   code:MSPushAbortedAuthentication
                                          internalError:error];
            }
            else if(error.domain == NSURLErrorDomain) {
                self.error = [self errorWithDescription:@"Push aborted due to network error"
                                                   code:MSPushAbortedUnknown
                                          internalError:error];
            }
            else {
                MSTableOperationError *tableError = [[MSTableOperationError alloc] initWithOperation:operation
                                                                                                item:operation.item
                                                                                               error:error];
                
                NSError *storeError;
                [self.syncContext.dataSource upsertItem:[tableError serialize]
                                                  table:[self.syncContext.dataSource errorTableName]
                                                orError:&storeError];
                if (storeError) {
                    self.error = [self errorWithDescription:@"Push aborted due to failure to save operation errors to store"
                                                       code:MSPushAbortedDataSource
                                              internalError:storeError];
                }
            }
        }
        else if (operation.type != MSTableOperationDelete && item != nil) {
            // The operation excuted successfully, so save the item (if we have one)
            NSError *storeError;
            if ([self.syncContext.operationQueue getOperationsForTable:operation.tableName item:operation.itemId].count <= 1) {
                [self.syncContext.dataSource upsertItem:item table:operation.tableName orError:&storeError];
                
                if (storeError) {
                    NSString *errorMessage = [NSString stringWithFormat:@"Unable to upsert item '%@' into table '%@'",
                                              operation.itemId, operation.tableName];
                    
                    self.error = [self errorWithDescription:errorMessage
                                                       code:MSPushAbortedDataSource
                                              internalError:storeError];
                }
            }
        }

        // Remove our operation
        NSError *storeError = [self.syncContext removeOperation:operation];
        
        if (self.error) {
            [self pushComplete];
            return;
        }
        else if (storeError) {
            self.error = [self errorWithDescription:@"error removing successful operation from queue"
                                               code:MSSyncTableInternalError
                                      internalError:error];
            [self pushComplete];
            return;
        }
        
        [self processQueueEntry];
    });
}

/// When all operations are complete (with errors) or any one operation encountered a fatal error
/// this can be called to begin finalizing a push operation
-(void) pushComplete
{
    MSSyncTable *table = [[MSSyncTable alloc] initWithName:[self.syncContext.dataSource errorTableName]
                                                    client:self.syncContext.client];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:table];
    
    [self.syncContext.dataSource readWithQuery:query completion:^(NSArray *items, NSInteger totalCount, NSError *error) {
        // remove all the errors now
        [self.syncContext.dataSource deleteUsingQuery:query orError:nil];
        
        // Create the containing error as needed
        if (items && items.count > 0) {
            NSMutableArray *tableErrors = [NSMutableArray new];
            for (NSDictionary *item in items) {
                [tableErrors addObject:[[MSTableOperationError alloc] initWithSerializedItem:item]];
            }
            
            if (self.error == nil) {
                self.error = [self errorWithDescription:@"Not all operations completed successfully"
                                                   code:MSPushCompleteWithErrors
                                             pushErrors:tableErrors
                                          internalError:nil];
            } else {
                self.error = [self errorWithDescription:[self.error localizedDescription]
                                                   code:self.error.code
                                             pushErrors:tableErrors
                                          internalError:[self.error.userInfo objectForKey:NSUnderlyingErrorKey]];
            }
        }
        
        // Send the final table operation results to the delegate
        id<MSSyncContextDelegate> syncDelegate = self.syncContext.delegate;
        dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^{
            if (syncDelegate && [syncDelegate respondsToSelector:@selector(pushCompleteWithError:completion:)]) {
                [syncDelegate pushCompleteWithError:self.error completion:^{
                    [self processErrors];
                }];
            } else {
                [self processErrors];
            }
        });
    }];
}

/// Analyze the final errors from all the operations, updating the state as appropriate
- (void) processErrors
{
    if (self.error) {
        // Update core error to reflect any changes in push errors
        NSMutableDictionary *userInfo = [self.error.userInfo mutableCopy];
        
        NSArray *pushErrors = [userInfo objectForKey:MSErrorPushResultKey];
        NSMutableArray *remainingErrors = [[NSMutableArray alloc] init];
        for (MSTableOperationError *error in pushErrors) {
            // Remove any operations the delegate handled
            if (!error.handled) {
                [remainingErrors addObject:error];
            }
        }
        
        // Ajdust the error
        if (self.error.code == MSPushCompleteWithErrors && remainingErrors.count == 0) {
            self.error = nil;
        } else {
            self.error = [self errorWithDescription:[self.error localizedDescription]
                                               code:self.error.code
                                         pushErrors:remainingErrors
                                      internalError:[self.error.userInfo objectForKey:NSUnderlyingErrorKey]];
        }
    }
    
    if (self.completion) {
        self.completion(self.error);
    }
    
    [self endExecution];
}

/// Builds a NSError containing the errors related to a push operation
- (NSError *) errorWithDescription:(NSString *)description code:(NSInteger)code internalError:(NSError *)error
{
    NSMutableDictionary *userInfo = [@{ NSLocalizedDescriptionKey: description } mutableCopy];
    
    if (error) {
        [userInfo setObject:error forKey:NSUnderlyingErrorKey];
    }
    
    return [NSError errorWithDomain:MSErrorDomain code:code userInfo:userInfo];
}

/// Builds a NSError containing the errors related to a push operation
-(NSError *) errorWithDescription:(NSString *)description code:(NSInteger)code pushErrors:(NSArray *)pushErrors internalError:(NSError *)error
{
    NSMutableDictionary *userInfo = [@{ NSLocalizedDescriptionKey: description } mutableCopy];
    
    if (error) {
        [userInfo setObject:error forKey:NSUnderlyingErrorKey];
    }
    
    if (pushErrors && pushErrors.count > 0) {
        [userInfo setObject:pushErrors forKey:MSErrorPushResultKey];
    }
    
    return [NSError errorWithDomain:MSErrorDomain code:code userInfo:userInfo];
}

@end
