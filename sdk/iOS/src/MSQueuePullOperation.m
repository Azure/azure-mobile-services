// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSQueuePullOperation.h"
#import "MSTableOperationError.h"
#import "MSSyncContextInternal.h"
#import "MSClientInternal.h"
#import "MSTableOperationInternal.h"
#import "MSQuery.h"
#import "MSSyncContext.h"
#import "MSSyncContextInternal.h"
#import "MSClientInternal.h"
#import "MSQueryInternal.h"
#import "MSNaiveISODateFormatter.h"
#import "MSDateOffset.h"
#import "MSTableConfigValue.h"

@interface MSQueuePullOperation()

@property (nonatomic, strong)   NSError *error;
@property (nonatomic, weak)     dispatch_queue_t dispatchQueue;
@property (nonatomic, weak)     NSOperationQueue *callbackQueue;
@property (nonatomic, weak)     MSSyncContext *syncContext;
@property (nonatomic, copy)     MSSyncBlock completion;
@property (nonatomic, strong)   MSQuery* query;
@property (nonatomic, strong)   NSString *queryId;
@property (nonatomic)           NSInteger recordsProcessed;
@property (nonatomic, strong)   NSDate *maxDate;
@property (nonatomic, strong)   NSDate *deltaToken;
@property (nonatomic, strong)   NSPredicate *originalPredicate;
@property (nonatomic, strong)   MSTableConfigValue *deltaTokenEntity;

@end

@implementation MSQueuePullOperation

- (id) initWithSyncContext:(MSSyncContext *)syncContext
                     query:(MSQuery *)query
                   queryId:(NSString *)queryId
             dispatchQueue:(dispatch_queue_t)dispatchQueue
             callbackQueue:(NSOperationQueue *)callbackQueue
                completion:(MSSyncBlock)completion
{
    self = [super init];
    if (self) {
        _syncContext = syncContext;
        _query = query;
        _queryId = queryId;
        _dispatchQueue = dispatchQueue;
        _callbackQueue = callbackQueue;
        _completion = [completion copy];
        _recordsProcessed = 0;
        _maxDate = [NSDate dateWithTimeIntervalSince1970:0.0];
        _deltaToken = nil;
        _originalPredicate = self.query.predicate;
    }
    return self;
}

- (void) completeOperation {
    [self willChangeValueForKey:@"isFinished"];
    [self willChangeValueForKey:@"isExecuting"];
    
    executing_ = NO;
    finished_ = YES;
    
    [self didChangeValueForKey:@"isExecuting"];
    [self didChangeValueForKey:@"isFinished"];
}

// Check if the operation was cancelled and if so, begin winding down
-(BOOL) checkIsCanceled
{
    if (self.isCancelled) {
        self.error = [self errorWithDescription:@"Pull cancelled" code:MSPullAbortedUnknown];
        [self completeOperation];
    }
    
    return self.isCancelled;
}

-(void) start
{
    if (finished_) {
        return;
    }
    else if (self.isCancelled) {
        [self completeOperation];
        return;
    }
    
    [self willChangeValueForKey:@"isExecuting"];
    executing_ = YES;
    [self didChangeValueForKey:@"isExecuting"];
    
    if (self.queryId) {
        __block NSError *localDataSourceError;
        dispatch_sync(self.dispatchQueue, ^{
            [self updateQueryFromDeltaTokenOrError:&localDataSourceError];
        });
        
        if([self callCompletionIfError:localDataSourceError])
        {
            return;
        }
    }
    
    [self processPullOperation];
}

/// For a given pending table operation, create the request to send it to the remote table
- (void) processPullOperation
{
    if ([self checkIsCanceled]) {
        return;
    }
    
    // Read from server
    [self.query readInternalWithFeatures:MSFeatureOffline completion:^(MSQueryResult *result, NSError *error) {
        if ([self checkIsCanceled]) {
            return;
        }
        // If error, or no results we can stop processing
        if (error || result.items.count == 0) {
            if (self.completion) {
                [self.callbackQueue addOperationWithBlock:^{
                    self.completion(error);
                }];
            }
            [self completeOperation];
            return;
        }
        
        // Update our local store (we need to block inbound operations while we do this)
        dispatch_async(self.dispatchQueue, ^{
            if ([self checkIsCanceled]) {
                return;
            }
            
            NSError *localDataSourceError;
            
            // Check if have any pending ops on this table
            NSArray *pendingOps = [self.syncContext.operationQueue getOperationsForTable:self.query.table.name item:nil];
            
            NSMutableArray *itemsToUpsert = [NSMutableArray array];
            NSMutableArray *itemIdsToDelete = [NSMutableArray array];
            
            [result.items enumerateObjectsUsingBlock:^(id obj, NSUInteger idx, BOOL *stop) {
                if (self.queryId) {
                    self.maxDate = [self.maxDate laterDate:(NSDate *)obj[MSSystemColumnUpdatedAt]];
                }
                BOOL isDeleted = NO;
                NSObject *isDeletedObj = obj[MSSystemColumnDeleted];
                if (isDeletedObj && isDeletedObj != [NSNull null]) {
                    isDeleted = ((NSNumber *)isDeletedObj).boolValue;
                }
                
                NSPredicate *predicate = [NSPredicate predicateWithFormat:@"%K == %@", @"itemId", obj[MSSystemColumnId]];
                NSArray *matchingRecords = [pendingOps filteredArrayUsingPredicate:predicate];
                
                // we want to ignore items that have been touched since the Pull was started
                if (matchingRecords.count == 0) {
                    if (isDeleted) {
                        [itemIdsToDelete addObject:obj[MSSystemColumnId]];
                    }
                    else {
                        [itemsToUpsert addObject:obj];
                    }
                }
            }];
            
            [self.syncContext.dataSource deleteItemsWithIds:itemIdsToDelete table:self.query.table.name orError:&localDataSourceError];
            if ([self callCompletionIfError:localDataSourceError]) {
                return;
            }
            
            // upsert each item into table that isn't pending to go to the server
            [self.syncContext.dataSource upsertItems:itemsToUpsert table:self.query.table.name orError:&localDataSourceError];
            if ([self callCompletionIfError:localDataSourceError]) {
                return;
            }
            
            self.recordsProcessed += result.items.count;
            
            if (self.queryId) {
                if (!self.deltaToken || [self.deltaToken compare:self.maxDate] == NSOrderedAscending) {
                    // if we have no deltaToken or the maxDate has increased, store it, and requery
                    [self upsertDeltaTokenOrError:&localDataSourceError];
                    if([self callCompletionIfError:localDataSourceError]) {
                        return;
                    }
                    
                    self.recordsProcessed = 0;
                    
                    [self updateQueryFromDeltaTokenOrError:&localDataSourceError];
                    if ([self callCompletionIfError:localDataSourceError]) {
                        return;
                    }
                }
                else {
                    self.query.fetchOffset = self.recordsProcessed;
                }
            }
            else {
                self.query.fetchOffset = self.recordsProcessed;
            }
            
            // try to Pull again with the updated offset or query
            [self processPullOperation];
        });
    }];
}

-(void) upsertDeltaTokenOrError:(NSError **)error
{
    NSDateFormatter *formatter = [MSNaiveISODateFormatter naiveISODateFormatter];
    self.deltaTokenEntity.value = [formatter stringFromDate:self.maxDate];
    [self.syncContext.dataSource upsertItems:@[self.deltaTokenEntity.serialize] table:self.syncContext.dataSource.configTableName orError:error];
    if (error && *error) {
        return;
    }
    self.deltaToken = self.maxDate;
}

/// Updates self.query.predicate with the date stored in self.deltaToken. The deltaToken is loaded from
/// the syncContext's dataSource, if required. This method must be called on self.dispatchQueue.
-(void) updateQueryFromDeltaTokenOrError:(NSError **)error
{
    // only load from local database if nil; we update it when writing
    if (!self.deltaToken) {
        NSDateFormatter *formatter = [MSNaiveISODateFormatter naiveISODateFormatter];
        MSSyncTable *configTable = [[MSSyncTable alloc] initWithName:self.syncContext.dataSource.configTableName client:self.syncContext.client];
        NSPredicate *predicate = [NSPredicate predicateWithFormat:@"table == %@ && key == %@ && keyType == %ld", self.query.table.name, self.queryId, MSConfigKeyDeltaToken];
        MSQuery *query = [[MSQuery alloc] initWithSyncTable:configTable predicate:predicate];
        NSArray *results = [self.syncContext.dataSource readWithQuery:query orError:error].items;
        if (error && *error) {
            return;
        }
        
        NSDictionary *deltaTokenDict = results.count > 0 ? results[0] : nil;
        
        if (deltaTokenDict) {
            self.deltaTokenEntity = [[MSTableConfigValue alloc] initWithSerializedItem:deltaTokenDict];
            self.deltaToken = [formatter dateFromString:self.deltaTokenEntity.value];
        }
        else {
            self.deltaTokenEntity = [MSTableConfigValue new];
            self.deltaTokenEntity.table = self.query.table.name;
            self.deltaTokenEntity.keyType = MSConfigKeyDeltaToken;
            self.deltaTokenEntity.key = self.queryId;
            // we set the value right before we upsert it
            self.deltaToken = [NSDate dateWithTimeIntervalSince1970:0.0];
        }
    }
    
    self.query.fetchOffset = -1;
    
    if (self.deltaToken) {
        MSDateOffset *offset = [[MSDateOffset alloc]initWithDate:self.deltaToken];
        NSPredicate *updatedAt = [NSPredicate predicateWithFormat:@"%K >= %@", MSSystemColumnUpdatedAt, offset];
        if (self.originalPredicate) {
            self.query.predicate = [NSCompoundPredicate andPredicateWithSubpredicates:@[self.originalPredicate, updatedAt]];
        }
        else {
            self.query.predicate = updatedAt;
        }
    }
}

-(BOOL) callCompletionIfError:(NSError *)error
{
    BOOL isError = NO;
    if (error) {
        isError = YES;
        if (self.completion) {
            [self.callbackQueue addOperationWithBlock:^{
                self.completion(error);
            }];
        }
        [self completeOperation];
    }
    return isError;
}

- (NSError *) errorWithDescription:(NSString *)description code:(NSInteger)code
{
    NSDictionary *userInfo = @{ NSLocalizedDescriptionKey: description };
    return [NSError errorWithDomain:MSErrorDomain code:code userInfo:userInfo];
}

- (BOOL) isConcurrent {
    return YES;
}

- (BOOL) isExecuting {
    return executing_;
}

- (BOOL) isFinished {
    return finished_;
}

@end
