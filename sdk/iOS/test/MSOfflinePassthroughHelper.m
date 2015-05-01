// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSOfflinePassthroughHelper.h"
#import "MSQuery.h"
#import "MSSyncContext.h"
#import "MSCoreDataStore.h"

@implementation MSOfflinePassthroughHelper

@synthesize data = data_;
- (NSMutableDictionary *)data {
    if (data_ == nil) {
        data_ = [NSMutableDictionary new];
    }
    
    return data_;
}

- (BOOL) upsertItems:(NSArray *)items table:(NSString *)table orError:(NSError **)error
{
    self.upsertCalls++;
    
    if (self.errorOnUpsertItemsForOperations && table == self.operationTableName) {
        *error = [NSError errorWithDomain:@"TestError" code:1 userInfo:nil];
        return NO;
    }
    
    if ([super upsertItems:items table:table orError:error]) {
        self.upsertedItems += items.count;
        return YES;
    }
    return NO;
}

- (BOOL) deleteUsingQuery:(MSQuery *)query orError:(NSError *__autoreleasing *)error
{
    self.deleteCalls++;
    
    MSSyncContextReadResult *preDeleteResult = [self readWithQuery:query orError:error];
    if (![super deleteUsingQuery:query orError:error]) {
        return NO;
    }
    
    MSSyncContextReadResult *postDeleteResult = [self readWithQuery:query orError:error];
    self.deletedItems += preDeleteResult.items.count - postDeleteResult.items.count;
    
    return YES;
}

- (BOOL) deleteItemsWithIds:(NSArray *)items table:(NSString *)table orError:(NSError **)error
{
    self.deleteCalls++;

    if ([super deleteItemsWithIds:items table:table orError:error]) {
        self.deletedItems += items.count;
        return YES;
    }
    
    return NO;
}

- (MSSyncContextReadResult *) readWithQuery:(MSQuery *)query orError:(NSError **)error
{
    self.readWithQueryCalls++;
    
    MSSyncContextReadResult *results = [super readWithQuery:query orError:error];
    if (self.errorOnReadWithQueryOrError) {
        *error = [NSError errorWithDomain:@"TestError" code:1 userInfo:nil];
        return nil;
    }
    
    self.readWithQueryItems += results.items.count;
    return results;
}


-(NSDictionary *) readTable:(NSString *)table withItemId:(NSString *)itemId orError:(NSError **)error
{
    self.readTableCalls++;
    
    NSDictionary *results = [super readTable:table withItemId:itemId orError:error];
    
    if (self.errorOnReadTableWithItemIdOrError) {
        *error = [NSError errorWithDomain:@"TestError" code:1 userInfo:nil];
        return nil;
    }
    
    self.readTableItems += results.count;
    return results;
}

-(void) resetCounts
{
    self.upsertedItems = 0;
    self.upsertCalls = 0;
    self.deleteCalls = 0;
    self.deletedItems = 0;
    self.readTableCalls = 0;
    self.readTableItems = 0;
    self.readWithQueryCalls = 0;
    self.readWithQueryItems = 0;
}

@end
