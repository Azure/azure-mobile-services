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
@property (nonatomic) NSInteger readTableCalls;
@property (nonatomic) NSInteger readTableItems;
@property (nonatomic) NSInteger readWithQueryCalls;
@property (nonatomic) NSInteger readWithQueryItems;

@property (nonatomic, copy) void (^operationCompletedHandler)(NSDictionary *item, NSError *error);

@property (nonatomic) BOOL errorOnReadWithQueryOrError;
@property (nonatomic) BOOL errorOnReadTableWithItemIdOrError;

@property (nonatomic, strong) NSMutableDictionary *data;

-(void) resetCounts;
@end
