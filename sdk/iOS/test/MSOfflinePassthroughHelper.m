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

-(void) readWithQuery:(MSQuery *)query completion:(MSSyncReadBlock)completion
{
    NSMutableDictionary *tableData = [self.data objectForKey:query.syncTable.name];
    if (tableData == nil) {
        completion(nil, 0, nil);
    }
    
    NSArray *items = [tableData allValues];
    completion(items, -1, nil);
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

- (BOOL) upsertItem:(NSDictionary *)item table:(NSString *)table orError:(NSError **)error
{
    NSMutableDictionary *tableData = [self.data objectForKey:table];
    if (tableData == nil) {
        tableData = [NSMutableDictionary new];
        [self.data setObject:tableData forKey:table];
    }
    
    [tableData setObject:item forKey:[item objectForKey:@"id"]];
    self.upsertCalls++;
    
    return YES;
}

- (BOOL) deleteItemWithId:(NSString *)item table:(NSString *)table orError:(NSError **)error
{
    NSMutableDictionary *tableData = [self.data objectForKey:table];
    if (tableData) {
        [tableData removeObjectForKey:item];
    }
    return YES;
}

- (BOOL) deleteUsingQuery:(MSQuery *)query orError:(NSError **)error
{
    [self.data removeObjectForKey:query.syncTable.name];
     
    return YES;
}

@end
