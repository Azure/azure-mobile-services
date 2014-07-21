// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSOfflinePassthroughHelper.h"
#import "MSQuery.h"

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
    
    if ([super upsertItems:items table:table orError:error]) {
        self.upsertedItems += items.count;
        return YES;
    }
    return NO;
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

-(void) resetCounts
{
    self.upsertedItems = 0;
    self.upsertCalls = 0;
    self.deleteCalls = 0;
    self.deletedItems = 0;
}

@end
