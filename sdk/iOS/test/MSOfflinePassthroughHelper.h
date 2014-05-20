// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------


#import <Foundation/Foundation.h>
#import "MSSyncContext.h"

@interface MSOfflinePassthroughHelper : NSObject <MSSyncContextDataSource, MSSyncContextDelegate>

@property (nonatomic) BOOL returnErrors;
@property (nonatomic) NSInteger upsertCalls;
@property (nonatomic, strong) NSMutableDictionary *data;
@end
