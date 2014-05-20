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

-(NSString *) operationTableName {
    return @"MS_TableOperations";
}

-(NSString *) errorTableName {
    return @"MS_TableOperationErrors";
}

/// Helper function to get a specific record from a table
-(id) getRecordForTable:(NSString *)table itemId:(NSString *)itemId asDictionary:(BOOL)asDictionary orError:(NSError **)error
{
    NSFetchRequest *fr = [NSFetchRequest fetchRequestWithEntityName:table];
    fr.predicate = [NSPredicate predicateWithFormat:@"id == %@", itemId];
    
    if (asDictionary) {
        fr.resultType = NSDictionaryResultType;
    }
    
    NSArray *results = [self.context executeFetchRequest:fr error:error];
    if (error && *error) {
        return nil;
    }
    
    NSDictionary *item = [results firstObject];
    if (asDictionary) {
        // Otherwise this dictionary can't be worked with as user's expect
        if (item) {
            return [NSDictionary dictionaryWithDictionary:item];
        } else {
            return nil;
        }
    }
    
    return [results firstObject];
}

/// Helper function to convert a managed object into a correctly formatted NSDictionary
+(NSDictionary *) adjustSystemPropertiesOnItem:(NSDictionary *)item {
    NSMutableDictionary *exposedItem = [item mutableCopy];
    
    [exposedItem setValue:[exposedItem objectForKey:@"ms_version"] forKey:MSSystemColumnVersion];
    [exposedItem removeObjectForKey:@"ms_version"];
    
    return [exposedItem copy];
}

#pragma mark - MSSyncContextDataSource

-(NSDictionary *)readTable:(NSString *)table withItemId:(NSString *)itemId orError:(NSError *__autoreleasing *)error
{
    __block NSDictionary *item;
    [self.context performBlockAndWait:^{
        item = [self getRecordForTable:table itemId:itemId asDictionary:YES orError:error];
        
        if (error && *error) {
            item = nil;
        }
    }];

    return [MSCoreDataStore adjustSystemPropertiesOnItem:item];
}

-(MSSyncContextReadResult *)readWithQuery:(MSQuery *)query orError:(NSError *__autoreleasing *)error
{
    __block NSInteger totalCount = -1;
    __block NSArray *results;
    [self.context performBlockAndWait:^{
        NSEntityDescription *entity = [NSEntityDescription entityForName:query.syncTable.name inManagedObjectContext:self.context];
        
        NSFetchRequest *fr = [NSFetchRequest fetchRequestWithEntityName:query.syncTable.name];
        fr.predicate = query.predicate;
        fr.sortDescriptors = query.orderBy;
        fr.resultType = NSDictionaryResultType;

        // Only calculate total count if fetchLimit/Offset is set
        if (query.includeTotalCount && (query.fetchLimit != -1 || query.fetchOffset != -1)) {
            totalCount = [self.context countForFetchRequest:fr error:error];
            if (error && *error) {
                return;
            }
            
            // If they just want a count quit out
            if (query.fetchLimit == 0) {
                return;
            }
        }
        
        if (query.fetchOffset != -1) {
            fr.fetchOffset = query.fetchOffset;
        }
        
        if (query.fetchLimit != -1) {
            fr.fetchLimit = query.fetchLimit;
        }
        
        // Convert version back to __version from MS_Version
        NSAttributeDescription *versionProperty;
        NSMutableArray *properties;
        
        for (NSAttributeDescription *desc in entity.properties) {
            if ([desc.name isEqualToString:@"ms_version"]) {
                versionProperty = desc;
                break;
            }
        }

        // If the table has __version, add a conversion step
        if (versionProperty) {
            if (query.selectFields) {
                properties = [query.selectFields mutableCopy];
            } else {
                properties = [[entity properties] mutableCopy];
                [properties removeObject:versionProperty];
            }
            
            NSExpression *actualVersionColumn = [NSExpression expressionForKeyPath:@"ms_version"];
            NSExpressionDescription *systemVersionColumn = [[NSExpressionDescription alloc] init];
            [systemVersionColumn setName:@"__version"];
            [systemVersionColumn setExpression:actualVersionColumn];
            [systemVersionColumn setExpressionResultType:NSStringAttributeType];
            
            [properties addObject:systemVersionColumn];
        }
        
        fr.propertiesToFetch = properties;
        
        NSArray *rawResult = [self.context executeFetchRequest:fr error:error];
        if (error && *error) {
            return;
        }
        
        // Convert NSKeyedDictionary to regular dictionary objects since for now this keyed dictionaries don't
        // seem to convert to mutable dictionaries as a user may expect
        NSMutableArray *finalResult = [[NSMutableArray alloc] initWithCapacity:rawResult.count];
        [rawResult enumerateObjectsUsingBlock:^(id obj, NSUInteger idx, BOOL *stop) {
            [finalResult addObject:[NSDictionary dictionaryWithDictionary:obj]];
        }];
        
        // If there was no fetch/skip (totalCount will still be -1) and total count requested, use the results array for the count
        if (query.includeTotalCount && totalCount == -1) {
            totalCount = results.count;
        }
        
        results = [finalResult copy];
    }];
    
    if (error && *error) {
        return nil;
    } else {
        return [[MSSyncContextReadResult alloc] initWithCount:totalCount items:results];
    }
}

-(BOOL) upsertItems:(NSArray *)items table:(NSString *)table orError:(NSError *__autoreleasing *)error
{
    __block BOOL success;
    [self.context performBlockAndWait:^{
        for (NSDictionary *item in items) {
            NSManagedObject *managedItem = [self getRecordForTable:table itemId:[item objectForKey:@"id"] asDictionary:NO orError:error];
            if (error && *error) {
                success = NO;
                
                // Reset since we may have made changes earlier
                [self.context reset];
                
                return;
            }
            
            if (managedItem == nil) {
                managedItem = [NSEntityDescription insertNewObjectForEntityForName:table
                                                            inManagedObjectContext:self.context];
            }
            
            id version = [item objectForKey:MSSystemColumnVersion];
            if (version) {
                NSMutableDictionary *adjustedItem = [item mutableCopy];
                [adjustedItem setValue:[item objectForKey:MSSystemColumnVersion] forKey:@"ms_version"];
                [adjustedItem removeObjectForKey:MSSystemColumnVersion];
                
                [managedItem setValuesForKeysWithDictionary:adjustedItem];
            } else {
                [managedItem setValuesForKeysWithDictionary:item];
            }
        }
        
        success = [self.context save:error];
        if (!success) {
            [self.context reset];
        }
    }];
    
    return success;
}

-(BOOL) deleteItemsWithIds:(NSArray *)items table:(NSString *)table orError:(NSError **)error
{
    __block BOOL success;
    [self.context performBlockAndWait:^{
        for (NSString *itemId in items) {
            NSManagedObject *foundItem = [self getRecordForTable:table itemId:itemId asDictionary:NO orError:error];
            if (error && *error) {
                success = NO;
                return;
            }
            
            if (foundItem) {
                [self.context deleteObject:foundItem];
            }
        }
        
        success = [self.context save:error];
        if (!success) {
            [self.context reset];
        }
    }];
    
    return success;
}

-(BOOL) deleteUsingQuery:(MSQuery *)query orError:(NSError *__autoreleasing *)error
{
    __block BOOL success;
    [self.context performBlockAndWait:^{
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
        
        success = [self.context save:error];
        if (!success) {
            [self.context reset];
        }
    }];
    
    return success;
}

@end
