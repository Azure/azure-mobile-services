//
//  MSOfflinePassthroughHelper.m
//  WindowsAzureMobileServices
//
//  Created by Phillip Van Nortwick on 2/2/14.
//  Copyright (c) 2014 Windows Azure. All rights reserved.
//

#import "MSOfflinePassthroughHelper.h"
#import "MSQuery.h"

@implementation MSOfflinePassthroughHelper
@synthesize returnErrors = returnErrors_;
@synthesize upsertCalls = upsertCalls_;
@synthesize data = data_;
- (NSMutableDictionary *)data {
    if (data_ == nil) {
        data_ = [NSMutableDictionary new];
    }
    
    return data_;
}

- (NSString *)errorTableName {
    return @"zumo_errors";
}

- (NSString *)operationTableName {
    return @"zumo_operations";
}

- (MSSyncContextReadResult *) readWithQuery:(MSQuery *)query orError:(NSError **)error
{
    NSMutableDictionary *tableData = [self.data objectForKey:query.syncTable.name];
    
    if (tableData == nil) {
        return [[MSSyncContextReadResult alloc] initWithCount:0 items:nil];
    }
    
    NSArray *allTableRows = [tableData allValues];
    NSArray *filteredResult;
    if (query.predicate) {
        filteredResult = [allTableRows filteredArrayUsingPredicate:query.predicate];
    } else {
        filteredResult = allTableRows;
    }
    
    return [[MSSyncContextReadResult alloc] initWithCount:filteredResult.count items:filteredResult];
}

- (NSDictionary *) readTable:(NSString *)table withItemId:(NSString *)itemId orError:(NSError **)error
{
    if (self.returnErrors) {
        if (error) {
            *error = [NSError errorWithDomain:@"TestCode" code:101 userInfo:nil];
        }
        return nil;
    }
    
    NSMutableDictionary *tableData = [self.data objectForKey:table];
    if (tableData == nil) {
        return nil;
    }
    
    return [tableData objectForKey:itemId];
}

- (BOOL) upsertItems:(NSArray *)items table:(NSString *)table orError:(NSError **)error
{
    NSMutableDictionary *tableData = [self.data objectForKey:table];
    if (tableData == nil) {
        tableData = [NSMutableDictionary new];
        [self.data setObject:tableData forKey:table];
    }
    
    for (NSDictionary *item in items) {
        [tableData setObject:item forKey:[item objectForKey:@"id"]];
        self.upsertCalls++;
    }
    
    return YES;
}

- (BOOL) deleteItemsWithIds:(NSArray *)items table:(NSString *)table orError:(NSError **)error
{
    NSMutableDictionary *tableData = [self.data objectForKey:table];
    [tableData removeObjectsForKeys:items];
    
    return YES;
}

- (BOOL) deleteUsingQuery:(MSQuery *)query orError:(NSError **)error
{
    [self.data removeObjectForKey:query.syncTable.name];
     
    return YES;
}

@end
