// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSCoreDataStore.h"

@interface MSCoreDataStore()
@property (nonatomic, weak) NSManagedObjectContext *context;
@end

@implementation MSCoreDataStore

-(id) initWithManagedObjectContext:(NSManagedObjectContext *)context
{
    self = [super init];
    if (self) {
        self.context = context;
    }
    return self;
}

- (NSString *) operationTableName {
    return @"MS_TableOperations";
}

- (NSString *) errorTableName {
    return @"MS_TableOperationErrors";
}

/// Helper function to get a specific record from a table
- (NSManagedObject *) getRecordForTable:(NSString *)table itemId:(NSString *)itemId orError:(NSError **)error
{
    NSFetchRequest *fr = [NSFetchRequest fetchRequestWithEntityName:table];
    fr.predicate = [NSPredicate predicateWithFormat:@"id == %@", itemId];
    
    NSArray *results = [self.context executeFetchRequest:fr error:error];
    if (error && *error) {
        return nil;
    }
    
    return [results firstObject];
}

/// Helper function to conver a managed object into a correctly formatted NSDictionary
- (NSDictionary *) itemFromManagedObject:(NSManagedObject *)object {
    NSArray *keys = [[[object entity] attributesByName] allKeys];
    
    NSMutableDictionary *item = [[object dictionaryWithValuesForKeys:keys] mutableCopy];
    
    // Move version to __version
    [item setValue:[item objectForKey:MSSystemColumnVersion] forKey:MSSystemColumnVersion];
    [item removeObjectForKey:@"MS_Version"];
    
    return [item copy];
}

#pragma mark - MSSyncContextDataSource

- (NSDictionary *)readTable:(NSString *)table withItemId:(NSString *)itemId orError:(NSError *__autoreleasing *)error
{
    NSManagedObject *item = [self getRecordForTable:table itemId:itemId orError:error];
    if (error && *error) {
        return nil;
    }
    
    return [self itemFromManagedObject:item];
}

- (MSSyncContextReadResult *)readWithQuery:(MSQuery *)query orError:(NSError *__autoreleasing *)error
{
    NSInteger totalCount = -1;
    
    NSFetchRequest *fr = [NSFetchRequest fetchRequestWithEntityName:query.syncTable.name];
    fr.predicate = query.predicate;
    fr.sortDescriptors = query.orderBy;
    
    // Only calculate total count if fetchLimit/Offset is set
    if (query.includeTotalCount && query.fetchLimit != -1 && query.fetchOffset != -1) {
        totalCount = [self.context countForFetchRequest:fr error:error];
        if (error && *error) {
            return nil;
        }
        
        // If they just want a count quit out
        if (query.fetchLimit == 0) {
            return [[MSSyncContextReadResult alloc] initWithCount:totalCount items:nil];
        }
    }
    
    if (query.fetchOffset != -1) {
        fr.fetchOffset = query.fetchOffset;
    }
    
    if (query.fetchLimit != -1) {
        fr.fetchLimit = query.fetchLimit;
    }
    
    NSArray *rawResult = [self.context executeFetchRequest:fr error:error];
    if (error && *error) {
        return nil;
    }
    
    NSMutableArray *items = [NSMutableArray new];
    for (NSManagedObject *object in rawResult) {
        [items addObject:[self itemFromManagedObject:object]];
    }
    
    return [[MSSyncContextReadResult alloc] initWithCount:totalCount items:items];
}

- (BOOL) upsertItem:(NSDictionary *)item table:(NSString *)table orError:(NSError *__autoreleasing *)error
{
    NSManagedObject *todoitem = [self getRecordForTable:table itemId:[item objectForKey:@"id"] orError:error];
    if (error && *error) {
        return NO;
    }
    
    if (todoitem == nil) {
        todoitem = [NSEntityDescription
                    insertNewObjectForEntityForName:table
                    inManagedObjectContext:self.context];
    }
    
    NSMutableDictionary *adjustedItem = [item mutableCopy];
    [adjustedItem setValue:[item objectForKey:MSSystemColumnVersion] forKey:@"MS_Version"];
    [adjustedItem removeObjectForKey:MSSystemColumnVersion];
    
    [todoitem setValuesForKeysWithDictionary:adjustedItem];
    
    if (![self.context save:error]) {
        return NO;
    }
    
    return YES;
}

- (BOOL) deleteItemWithId:(NSString *)item table:(NSString *)table orError:(NSError *__autoreleasing *)error
{
    NSFetchRequest *request = [[NSFetchRequest alloc] init];
    NSEntityDescription *entity = [NSEntityDescription entityForName:table
                                              inManagedObjectContext:self.context];
    [request setEntity:entity];
    
    NSPredicate *predicate = [NSPredicate predicateWithFormat:@"id == %@", item];
    [request setPredicate:predicate];
    
    NSArray *array = [self.context executeFetchRequest:request error:error];
    if (array == nil) {
        return YES;
    }
    
    [self.context deleteObject:[array firstObject]];
    
    if (![self.context save:error]) {
        return NO;
    }
    
    return YES;
}

- (BOOL) deleteUsingQuery:(MSQuery *)query orError:(NSError *__autoreleasing *)error
{
    NSFetchRequest *fr = [NSFetchRequest fetchRequestWithEntityName:query.syncTable.name];
    fr.predicate = query.predicate;
    fr.sortDescriptors = query.orderBy;
    
    if (query.fetchOffset != -1) {
        fr.fetchOffset = query.fetchOffset;
    }
    
    if (query.fetchLimit != -1) {
        fr.fetchLimit = query.fetchLimit;
    }
    
    fr.includesPropertyValues = NO;
    
    NSArray *array = [self.context executeFetchRequest:fr error:error];
    for (NSManagedObject *object in array) {
        [self.context deleteObject:object];
    }
    
    if (![self.context save:error]) {
        NSLog(@"Couldn't save: %@", [*error localizedDescription]);
        return NO;
    }
    
    return YES;
}

@end
