// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSOperationQueue.h"

#import "MSTable.h"
#import "MSTableOperationInternal.h"
#import "MSQuery.h"

@interface MSOperationQueue()
@property (nonatomic, weak) id<MSSyncContextDataSource> dataSource;
@property (nonatomic, weak) MSClient *client;
@property (atomic, strong) NSMutableDictionary *locks;
@end

@implementation MSOperationQueue
@synthesize dataSource = dataSource_;
@synthesize client = client_;
@synthesize locks = locks_;

- (id) initWithClient:(MSClient *)client dataSource:(id<MSSyncContextDataSource>)dataSource
{
    self = [super init];
    if (self) {
        dataSource_ = dataSource;
        client_ = client;
        locks_ = [NSMutableDictionary new];
    }
    return self;
}

/// Return the total count of operations that exist in the queue
-(NSUInteger) count
{
    MSSyncTable *table = [[MSSyncTable alloc] initWithName:self.dataSource.operationTableName client:self.client];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:table];
    
    // We want the total count from the DB, but no records
    query.includeTotalCount = YES;
    query.fetchLimit = 0;

    NSError *error;
    MSSyncContextReadResult *result = [self.dataSource readWithQuery:query orError:&error];

    // Return -1 if count fails
    if (error) {
        return -1;
    }
    
    return result.totalCount;
}

/// Load up all operations for a given table and optionally a specific item on that table
/// and return the list to the caller
-(NSArray *) getOperationsForTable:(NSString *)table item:(NSString *)item
{
    NSError *error;
    MSSyncTable *syncTable = [[MSSyncTable alloc] initWithName:self.dataSource.operationTableName client:self.client];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:syncTable];
    
    if (item) {
        query.predicate = [NSPredicate predicateWithFormat:@"(table == %@) AND (itemId == %@)", table, item];
    } else {
        query.predicate = [NSPredicate predicateWithFormat:@"(table == %@)", table];
    }

    MSSyncContextReadResult *result = [self.dataSource readWithQuery:query orError:&error];
    if (error) {
        return nil;
    }
    
    NSMutableArray *operations = [NSMutableArray new];
    for (NSDictionary *item in result.items) {
        MSTableOperation *op = [[MSTableOperation alloc] initWithItem:item];
        op.inProgress = [self isLocked:op];
        [operations addObject:op];
    }
    return [operations copy];
}

-(void) addOperation:(MSTableOperation *)operation orError:(NSError **)error
{
    [self.dataSource upsertItem:[operation serialize]
                          table:self.dataSource.operationTableName
                        orError:error];
}

-(void) removeOperation:(MSTableOperation *)operation orError:(NSError **)error
{
    [self.dataSource deleteItemWithId:[NSNumber numberWithInteger:operation.operationId]
                                table:[self.dataSource operationTableName]
                              orError:error];
    
    // Make sure to clean up any lock if one existed
    [self unlockOperation:operation];
}

- (id) peek
{
    return [self getOperationAfter:-1];
}

- (id) getOperationAfter:(NSInteger)operationId
{
    MSSyncTable *table = [[MSSyncTable alloc] initWithName:self.dataSource.operationTableName client:self.client];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:table];
    
    // We want the highest id from the DB, but no records
    query.fetchLimit = 1;
    [query orderByAscending:@"id"];
    if (operationId >= 0) {
        query.predicate = [NSPredicate predicateWithFormat:@"id > %d", operationId];
    }
    
    MSSyncContextReadResult *result = [self.dataSource readWithQuery:query orError:nil];
    if (result.items && result.items.count > 0) {
        NSDictionary *item = [result.items objectAtIndex:0];
        if (item) {
            MSTableOperation *op = [[MSTableOperation alloc] initWithItem:item];
            op.inProgress = [self isLocked:op];
            return op;
        }
    }
    
    return nil;
}

/// Return the highest id currently in the table + 1. This API should not be called in parallel with
/// an insert function.
-(NSInteger) getNextOperationId
{
    MSSyncTable *table = [[MSSyncTable alloc] initWithName:self.dataSource.operationTableName client:self.client];
    MSQuery *query = [[MSQuery alloc] initWithSyncTable:table];
    
    // We want the highest id from the DB, but no records
    query.fetchLimit = 1;
    [query orderByDescending:@"id"];
    
    NSError *error;
    MSSyncContextReadResult *result = [self.dataSource readWithQuery:query orError:&error];
    
    // Return -1 if count fails
    if (error) {
        return -1;
    }
    
    if (result.items && result.items.count > 0) {
        NSDictionary *item = [result.items objectAtIndex:0];
        return [[item objectForKey:@"id"] integerValue] + 1;
    } else {
        return 1;
    }
}

- (BOOL) lockOperation:(MSTableOperation *)operation
{
    NSNumber *key = [NSNumber numberWithInteger:operation.operationId];
    BOOL locked = [[self.locks objectForKey:key] boolValue];
    if (locked) {
        return NO;
    } else {
        [self.locks setObject:[NSNumber numberWithBool:YES] forKey:key];
        return YES;
    }
}

- (BOOL) unlockOperation:(MSTableOperation *)operation
{
    NSNumber *key = [NSNumber numberWithInteger:operation.operationId];
    [self.locks removeObjectForKey:key];
    return YES;
}

- (BOOL) isLocked:(MSTableOperation *)operation
{
    NSNumber *key = [NSNumber numberWithInteger:operation.operationId];
    return [[self.locks objectForKey:key] boolValue];
}

@end
