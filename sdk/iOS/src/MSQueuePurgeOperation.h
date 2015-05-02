// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "BlockDefinitions.h"
@class MSQuery;
@class MSSyncContext;

@interface MSQueuePurgeOperation : NSOperation {
    BOOL executing_;
    BOOL finished_;
}

- (id) initPurgeWithSyncContext:(MSSyncContext *)syncContext
                          query:(MSQuery *)query
                          force:(BOOL)force
                  dispatchQueue:(dispatch_queue_t)dispatchQueue
                  callbackQueue:(NSOperationQueue *)callbackQueue
                     completion:(MSSyncBlock)completion;

@end
