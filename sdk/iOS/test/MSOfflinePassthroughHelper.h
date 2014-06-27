// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------


#import <Foundation/Foundation.h>
#import "MSSyncContext.h"
#import "MSCoreDataStore.h"

@interface MSOfflinePassthroughHelper : MSCoreDataStore <MSSyncContextDelegate>

@property (nonatomic) BOOL returnErrors;

@property (nonatomic) NSInteger upsertCalls;
@property (nonatomic) NSInteger upsertedItems;
@property (nonatomic) NSInteger deleteCalls;
@property (nonatomic) NSInteger deletedItems;

@property (nonatomic, strong) NSMutableDictionary *data;

-(void) resetCounts;
@end
